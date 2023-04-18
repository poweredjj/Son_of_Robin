using Microsoft.Xna.Framework;
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
                if (this.lastCheckedFrame != camera.world.CurrentUpdate)
                {
                    camera.world.Grid.GetSpritesInCameraViewAndPutIntoList(camera: camera, groupName: groupName, spriteListToFill: this.spriteList, compareWithCameraRect: compareWithCameraRect);
                    this.lastCheckedFrame = camera.world.CurrentUpdate;
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
        private readonly bool keepInWorldBounds;
        private readonly bool useFluidMotionForMove;
        private readonly bool useFluidMotionForZoom;
        private readonly bool useWorldScale;
        private int lastUpdateFrame;
        private TrackingMode trackingMode;
        private Sprite trackedSprite;
        private bool trackedSpriteReached;
        private Vector2 trackedPos;

        public Vector2 TrackedPos
        { get { return this.trackingMode == TrackingMode.Position ? this.trackedPos : this.trackedSprite.position; } }

        public Vector2 CurrentPos { get; private set; }
        public float TargetZoom { get; private set; }
        public float CurrentZoom { get; private set; }

        public void SetViewParams(Scene scene)
        {
            scene.viewParams.Width = this.world.width;
            scene.viewParams.Height = this.world.height;
            scene.viewParams.PosX = this.viewPos.X;
            scene.viewParams.PosY = this.viewPos.Y;

            float worldScale = this.useWorldScale ? Preferences.WorldScale : 1f / Preferences.GlobalScale;
            float scale = 1f / (worldScale * this.CurrentZoom);
            scene.viewParams.ScaleX = scale;
            scene.viewParams.ScaleY = scale;
        }

        private bool disableFluidMotionMoveForOneFrame;
        private const int movementSlowdown = 20;
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
        {
            get
            {
                float worldScale = this.useWorldScale ? Preferences.WorldScale : 1f;
                return (int)(SonOfRobinGame.VirtualWidth / this.CurrentZoom / worldScale);
            }
        }

        public int ScreenHeight
        {
            get
            {
                float worldScale = this.useWorldScale ? Preferences.WorldScale : 1f;
                return (int)(SonOfRobinGame.VirtualHeight / this.CurrentZoom / worldScale);
            }
        }

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

                return trackedPiece.GetType() == typeof(Player) && this.trackedSpriteReached;
            }
        }

        private enum TrackingMode
        {
            Undefined,
            Sprite,
            Position,
        }

        public Camera(World world, bool useFluidMotionForMove, bool useFluidMotionForZoom, bool useWorldScale, bool keepInWorldBounds = true)
        {
            this.world = world;
            this.keepInWorldBounds = keepInWorldBounds;
            this.useFluidMotionForMove = useFluidMotionForMove;
            this.useFluidMotionForZoom = useFluidMotionForZoom;
            this.useWorldScale = useWorldScale;
            this.CurrentPos = new Vector2(0, 0);

            this.viewRect = new Rectangle(0, 0, 0, 0);
            this.disableFluidMotionMoveForOneFrame = false;
            this.trackingMode = TrackingMode.Undefined;
            this.TargetZoom = 1f;
            this.CurrentZoom = 1f;
            this.trackedSprite = null;
            this.trackedSpriteReached = false;
            this.trackedPos = Vector2.One;
            this.lastUpdateFrame = 0;
        }

        public void Update(Vector2 cameraCorrection)
        {
            if (Scene.ProcessingMode == Scene.ProcessingModes.Draw || this.lastUpdateFrame == SonOfRobinGame.CurrentUpdate) return;

            if (this.useFluidMotionForZoom)
            {
                this.CurrentZoom += (this.TargetZoom - this.CurrentZoom) / this.zoomSlowdown;
                if (this.CurrentZoom == this.TargetZoom) this.zoomSlowdown = movementSlowdown; // resetting to default zoom speed, after reaching target value
            }
            else
            {
                this.CurrentZoom = this.TargetZoom;
                this.zoomSlowdown = movementSlowdown;
            }

            Vector2 currentTargetPos = this.GetTargetCoords();
            Vector2 viewCenter = new Vector2(0, 0); // to be updated below

            if (this.useFluidMotionForMove && !this.disableFluidMotionMoveForOneFrame)
            {
                float cameraDistX = Math.Abs(this.viewRect.Center.X - currentTargetPos.X);
                float cameraDistY = Math.Abs(this.viewRect.Center.Y - currentTargetPos.Y);

                float movementSlowdownFactorX = (float)Helpers.ConvertRange(oldMin: 0, oldMax: this.viewRect.Width / 1.7, newMin: 0, newMax: 20, oldVal: cameraDistX, clampToEdges: true);
                float movementSlowdownFactorY = (float)Helpers.ConvertRange(oldMin: 0, oldMax: this.viewRect.Height / 1.7, newMin: 0, newMax: 20, oldVal: cameraDistY, clampToEdges: true);

                movementSlowdownFactorX = Math.Max(movementSlowdown - movementSlowdownFactorX, 4);
                movementSlowdownFactorY = Math.Max(movementSlowdown - movementSlowdownFactorY, 4);

                // MessageLog.AddMessage(msgType: MsgType.User, message: $"cameraDist {(int)cameraDistX} {(int)cameraDistY} factor {(int)movementSlowdownFactorX} {(int)movementSlowdownFactorY}"); // for testing

                viewCenter.X = this.CurrentPos.X + ((currentTargetPos.X - this.CurrentPos.X) / movementSlowdownFactorX);
                viewCenter.Y = this.CurrentPos.Y + ((currentTargetPos.Y - this.CurrentPos.Y) / movementSlowdownFactorY);
            }
            else
            {
                viewCenter.X = currentTargetPos.X;
                viewCenter.Y = currentTargetPos.Y;
                this.trackedSpriteReached = true;
                this.disableFluidMotionMoveForOneFrame = false;
            }

            viewCenter += cameraCorrection;

            float screenWidth = this.ScreenWidth;
            float screenHeight = this.ScreenHeight;

            float widthDivider = this.world.demoMode ? 5 : 2; // to put the camera center on the left side of the screen during demo (there is menu on the right)

            float xMin = viewCenter.X - (screenWidth / widthDivider);
            float yMin = viewCenter.Y - (screenHeight / 2f);

            if (this.keepInWorldBounds)
            {
                xMin = Math.Min(Math.Max(xMin, 0), (float)this.world.width - screenWidth - 1f);
                yMin = Math.Min(Math.Max(yMin, 0), (float)this.world.height - screenHeight - 1f);
            }

            float xMax = xMin + screenWidth;
            float yMax = yMin + screenHeight;

            if (this.keepInWorldBounds)
            {
                xMax = Math.Min(xMax, this.world.width);
                yMax = Math.Min(yMax, this.world.height);
            }

            this.CurrentPos = viewCenter;

            this.viewRect.X = (int)xMin;
            this.viewRect.Y = (int)yMin;
            this.viewRect.Width = (int)Math.Floor(xMax - xMin);
            this.viewRect.Height = (int)Math.Floor(yMax - yMin);

            this.viewPos = new Vector2(-xMin, -yMin);

            if (!this.trackedSpriteReached && Vector2.Distance(this.CurrentPos, currentTargetPos) < 30) this.trackedSpriteReached = true;
            this.lastUpdateFrame = SonOfRobinGame.CurrentUpdate;
        }

        public void TrackPiece(BoardPiece trackedPiece, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Sprite;
            this.trackedSprite = trackedPiece.sprite;
            this.trackedSpriteReached = false;
            this.disableFluidMotionMoveForOneFrame = moveInstantly;
        }

        public void TrackCoords(Vector2 position, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Position;
            this.trackedSprite = null;
            this.trackedSpriteReached = false;
            this.trackedPos = new Vector2(position.X, position.Y);
            this.disableFluidMotionMoveForOneFrame = moveInstantly;
        }

        public void ResetZoom(bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.SetZoom(zoom: 1f, setInstantly: setInstantly, zoomSpeedMultiplier: zoomSpeedMultiplier);
        }

        public void SetZoom(float zoom, bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.zoomSlowdown = (int)(movementSlowdown / zoomSpeedMultiplier);
            this.TargetZoom = zoom;
            if (setInstantly) this.CurrentZoom = zoom;
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

        public void TrackLiveAnimal(bool fluidMotion)
        {
            if (this.TrackedSpriteExists) return;

            var allSprites = this.world.Grid.GetSpritesFromAllCells(Cell.Group.ColMovement);
            var animals = allSprites.Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive);
            if (animals.Count() == 0) return;
            var index = BoardPiece.Random.Next(0, animals.Count());
            this.TrackPiece(trackedPiece: animals.ElementAt(index).boardPiece, moveInstantly: !fluidMotion);
        }

        public List<Sprite> GetVisibleSprites(Cell.Group groupName, bool compareWithCameraRect = false)
        {
            return BufferedSpriteSearch.SearchSprites(camera: this, groupName: groupName, compareWithCameraRect: compareWithCameraRect);
        }
    }
}