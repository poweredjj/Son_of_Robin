using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Camera
    {
        public struct BufferedSpriteSearch
        {
            // Prevents from repeating the same search in one frame.
            // Also, prevents from creating multiple List<Sprite> every frame.

            private static Dictionary<string, BufferedSpriteSearch> searchByID = new Dictionary<string, BufferedSpriteSearch>();

            private readonly string id;
            private readonly bool compareWithCameraRect;
            private readonly List<Sprite> spriteList;
            private readonly Cell.Group groupName;
            private int lastCheckedFrame;

            private BufferedSpriteSearch(string id, Cell.Group groupName, bool compareWithCameraRect)
            {
                this.id = id;
                this.groupName = groupName;
                this.compareWithCameraRect = compareWithCameraRect;
                this.spriteList = new List<Sprite>();
                this.lastCheckedFrame = -1;

                searchByID[this.id] = this;
            }

            private List<Sprite> GetSprites(Camera camera)
            {
                if (this.lastCheckedFrame != camera.world.currentUpdate)
                {
                    camera.world.grid.GetSpritesInCameraViewAndPutIntoList(camera: camera, groupName: groupName, spriteListToFill: this.spriteList, compareWithCameraRect: compareWithCameraRect);
                    this.lastCheckedFrame = camera.world.currentUpdate;
                }
                // else MessageLog.AddMessage(msgType: MsgType.User, message: $"{camera.world.currentUpdate} reusing sprite search {groupName} - {compareWithCameraRect}");

                return this.spriteList;
            }

            public static List<Sprite> SearchSprites(Camera camera, Cell.Group groupName, bool compareWithCameraRect)
            {
                string id = $"{groupName}-{compareWithCameraRect}";
                if (!searchByID.ContainsKey(id)) searchByID[id] = new BufferedSpriteSearch(id: id, groupName: groupName, compareWithCameraRect: compareWithCameraRect);

                BufferedSpriteSearch currentSearch = searchByID[id];

                return currentSearch.GetSprites(camera);
            }
        }

        private readonly World world;
        private TrackingMode trackingMode;
        private Sprite trackedSprite;
        private bool trackedSpriteReached;
        private Vector2 trackedPos;
        public Vector2 TrackedPos
        { get { return this.trackingMode == TrackingMode.Position ? this.trackedPos : this.trackedSprite.position; } }
        public Vector2 CurrentPos { get; private set; }
        private float targetZoom;
        public float currentZoom;
        private readonly bool useFluidMotion;
        private bool disableFluidMotionForOneFrame;
        private static readonly int movementSlowdown = 20;
        private int zoomSlowdown = 20;

        public Rectangle viewRect;
        public Vector2 viewPos;

        public Rectangle ExtendedViewRect // to be used for whole screen effects, that does not show borders during transformations (position, scale)
        {
            get
            {
                int screenExtension = 200;

                return new Rectangle(
                    x: this.viewRect.X - (screenExtension / 2),
                    y: this.viewRect.Y - (screenExtension / 2),
                    width: this.viewRect.Width + screenExtension,
                    height: this.viewRect.Height + screenExtension);
            }
        }

        public int ScreenWidth
        { get { return (int)(SonOfRobinGame.VirtualWidth * this.world.viewParams.ScaleX); } }

        public int ScreenHeight
        { get { return (int)(SonOfRobinGame.VirtualHeight * this.world.viewParams.ScaleY); } }

        public bool TrackedSpriteExists
        {
            get
            {
                if (this.trackingMode != TrackingMode.Sprite || this.trackedSprite == null) return false;
                return this.trackedSprite.boardPiece.exists;
            }
        }
        public BoardPiece TrackedPiece
        {
            get
            {
                if (trackedSprite == null) return null;
                return this.trackedSprite.boardPiece;
            }
        }
        public bool IsTrackingPlayer
        {
            get
            {
                BoardPiece trackedPiece = this.TrackedPiece;
                if (trackedPiece == null) return false;

                return trackedPiece.name == PieceTemplate.Name.Player && this.trackedSpriteReached;
            }
        }
        private enum TrackingMode
        {
            Undefined,
            Sprite,
            Position,
        }

        public Camera(World world, bool useFluidMotion)
        {
            this.world = world;
            this.useFluidMotion = useFluidMotion;
            this.CurrentPos = new Vector2(0, 0);

            this.viewRect = new Rectangle(0, 0, 0, 0);
            this.disableFluidMotionForOneFrame = false;
            this.trackingMode = TrackingMode.Undefined;
            this.targetZoom = 1f;
            this.currentZoom = 1f;
            this.trackedSprite = null;
            this.trackedSpriteReached = false;
            this.trackedPos = Vector2.One;
        }
        public void Update()
        {
            // Update should not be called more than once per frame - this causes jerky motion.
            // Also, it should not be calculated on Draw(), because new viewPos cannot be used before the next spriteBatch.Begin() (position and zoom will not match in the same frame).

            if (!Scene.processingUpdate) return;

            float xMin, xMax, yMin, yMax;
            Vector2 currentTargetPos = this.GetTargetCoords();
            Vector2 viewCenter = new Vector2(0, 0); // to be updated below

            if (this.useFluidMotion && !this.disableFluidMotionForOneFrame)
            {
                this.currentZoom += (this.targetZoom - this.currentZoom) / this.zoomSlowdown;
                if (this.currentZoom == this.targetZoom) this.zoomSlowdown = movementSlowdown; // resetting to default zoom speed, after reaching target value

                viewCenter.X = this.CurrentPos.X + ((currentTargetPos.X - this.CurrentPos.X) / movementSlowdown);
                viewCenter.Y = this.CurrentPos.Y + ((currentTargetPos.Y - this.CurrentPos.Y) / movementSlowdown);
            }
            else
            {
                this.currentZoom = this.targetZoom;
                viewCenter.X = currentTargetPos.X;
                viewCenter.Y = currentTargetPos.Y;
                this.trackedSpriteReached = true;
                this.disableFluidMotionForOneFrame = false;
            }

            viewCenter += this.world.analogCameraCorrection;

            float screenWidth = this.ScreenWidth;
            float screenHeight = this.ScreenHeight;

            float widthDivider = this.world.demoMode ? 5 : 2; // to put the camera center on the left side of the screen during demo (there is menu on the right)

            xMin = Math.Min(Math.Max(viewCenter.X - (screenWidth / widthDivider), 0), (float)this.world.width - screenWidth - 1f);
            yMin = Math.Min(Math.Max(viewCenter.Y - (screenHeight / 2f), 0), (float)this.world.height - screenHeight - 1f);

            xMin = Math.Max(xMin, 0);
            yMin = Math.Max(yMin, 0);
            xMax = Math.Min(xMin + screenWidth, this.world.width);
            yMax = Math.Min(yMin + screenHeight, this.world.height);

            this.CurrentPos = viewCenter;

            this.viewRect.X = (int)xMin;
            this.viewRect.Y = (int)yMin;
            this.viewRect.Width = (int)Math.Floor(xMax - xMin);
            this.viewRect.Height = (int)Math.Floor(yMax - yMin);

            this.viewPos = new Vector2(-xMin, -yMin);

            SoundEffect.DistanceScale = this.viewRect.Width * 0.065f;

            if (!this.trackedSpriteReached && Vector2.Distance(this.CurrentPos, currentTargetPos) < 30) this.trackedSpriteReached = true;
        }

        public void TrackPiece(BoardPiece trackedPiece, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Sprite;
            this.trackedSprite = trackedPiece.sprite;
            this.trackedSpriteReached = false;
            this.disableFluidMotionForOneFrame = moveInstantly;
        }

        public void TrackCoords(Vector2 position, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Position;
            this.trackedSprite = null;
            this.trackedSpriteReached = false;
            this.trackedPos = new Vector2(position.X, position.Y);
            this.disableFluidMotionForOneFrame = moveInstantly;
        }
        public void ResetZoom(bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.SetZoom(zoom: 1f, setInstantly: setInstantly, zoomSpeedMultiplier: zoomSpeedMultiplier);
        }

        public void SetZoom(float zoom, bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.zoomSlowdown = (int)(movementSlowdown / zoomSpeedMultiplier);
            this.targetZoom = zoom;
            if (setInstantly) this.currentZoom = zoom;
        }

        public Vector2 GetRandomPositionOutsideCameraView()
        {
            Vector2 position = new Vector2(0, 0);

            // in case of map being smaller than the screen
            if (viewRect.Width >= this.world.width && viewRect.Height >= this.world.height)
            {
                return new Vector2(this.world.random.Next(0, this.world.width), this.world.random.Next(0, this.world.height));
            }

            while (true)
            {
                position.X = this.world.random.Next(0, this.world.width);
                position.Y = this.world.random.Next(0, this.world.height);

                if (!viewRect.Contains(position)) break;
            }

            return position;
        }

        private Vector2 GetTargetCoords()
        {
            switch (this.trackingMode)
            {
                case TrackingMode.Sprite:
                    {
                        if (this.trackedSprite.boardPiece.exists) return this.trackedSprite.position;
                        else
                        {
                            this.TrackCoords(this.CurrentPos);
                            return this.trackedPos;
                        }
                    }

                case TrackingMode.Position:
                    { return this.trackedPos; }

                default:
                    { throw new ArgumentException($"Unsupported tracking mode - {this.trackingMode}."); }
            }
        }

        public void TrackLiveAnimal(bool fluidMotion = true)
        {
            if (this.TrackedSpriteExists) return;

            var allSprites = this.world.grid.GetAllSprites(Cell.Group.ColMovement);
            var animals = allSprites.Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();
            if (animals.Count == 0) return;
            var index = BoardPiece.Random.Next(0, animals.Count);
            this.TrackPiece(trackedPiece: animals[index].boardPiece, moveInstantly: true);
        }

        public List<Sprite> GetVisibleSprites(Cell.Group groupName, bool compareWithCameraRect = false)
        {
            return BufferedSpriteSearch.SearchSprites(camera: this, groupName: groupName, compareWithCameraRect: compareWithCameraRect);
        }

    }
}
