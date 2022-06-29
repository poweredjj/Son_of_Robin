using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace SonOfRobin
{
    public class Camera
    {
        private readonly World world;
        private TrackingMode trackingMode;
        private Sprite trackedSprite;
        private Vector2 trackedPos;
        private Vector2 currentPos;
        private float targetZoom;
        public float currentZoom;
        private bool currentFluidMotion;
        public bool fluidMotionDisabled;
        private static readonly int movementSlowdown = 20;
        private int zoomSlowdown = 20;

        public Rectangle viewRect;
        public Vector2 viewPos;

        public int ScreenWidth
        { get { return Convert.ToInt32(SonOfRobinGame.VirtualWidth * this.world.viewParams.ScaleX); } }

        public int ScreenHeight
        { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * this.world.viewParams.ScaleY); } }

        public bool TrackedSpriteExists
        {
            get
            {
                if (this.trackingMode != TrackingMode.Sprite || this.trackedSprite == null) return false;
                return this.trackedSprite.boardPiece.exists;
            }
        }
        private enum TrackingMode
        {
            Undefined,
            Sprite,
            Position,
        }


        public Camera(World world)
        {
            this.world = world;
            this.currentPos = new Vector2(0, 0);

            this.viewRect = new Rectangle(0, 0, 0, 0);
            this.currentFluidMotion = false;
            this.fluidMotionDisabled = false;
            this.trackingMode = TrackingMode.Undefined;
            this.targetZoom = 1f;
            this.currentZoom = 1f;
        }
        public void Update()
        {
            // Update should not be called more than once per frame - this causes jerky motion.
            // Also, it should not be calculated on Draw(), because new viewPos cannot be used before the next spriteBatch.Begin (position and zoom will not match in the same frame).

            if (!Scene.processingUpdate) return;

            float xMin; float xMax; float yMin; float yMax;
            Vector2 currentTargetPos = this.GetTargetCoords();
            Vector2 viewCenter = new Vector2(0, 0); // to be updated below

            if (this.currentFluidMotion)
            {
                this.currentZoom += (this.targetZoom - this.currentZoom) / this.zoomSlowdown;
                if (this.currentZoom == this.targetZoom) this.zoomSlowdown = movementSlowdown; // resetting to default zoom speed, after reaching target value

                viewCenter.X = this.currentPos.X + ((currentTargetPos.X - this.currentPos.X) / movementSlowdown);
                viewCenter.Y = this.currentPos.Y + ((currentTargetPos.Y - this.currentPos.Y) / movementSlowdown);
            }
            else
            {
                this.currentZoom = this.targetZoom;
                viewCenter.X = currentTargetPos.X;
                viewCenter.Y = currentTargetPos.Y;
                this.currentFluidMotion = true; // only one frame should be displayed with instant scrolling...
            }

            if (this.fluidMotionDisabled) this.currentFluidMotion = false; // ...unless fluid motion is disabled

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

            this.currentPos = viewCenter;

            this.viewRect.X = (int)xMin;
            this.viewRect.Y = (int)yMin;
            this.viewRect.Width = (int)(xMax - xMin);
            this.viewRect.Height = (int)(yMax - yMin);

            this.viewPos = new Vector2(-xMin, -yMin);
        }

        public void TrackPiece(BoardPiece trackedPiece, bool fluidMotion = true)
        {
            this.trackingMode = TrackingMode.Sprite;
            this.trackedSprite = trackedPiece.sprite;
            this.currentFluidMotion = fluidMotion;
        }

        public void TrackCoords(Vector2 position, bool fluidMotion = true)
        {
            this.trackingMode = TrackingMode.Position;
            this.trackedSprite = null;
            this.trackedPos = new Vector2(position.X, position.Y);
            this.currentFluidMotion = fluidMotion;
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
            { return new Vector2(this.world.random.Next(0, this.world.width), this.world.random.Next(0, this.world.height)); }

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
                        if (this.trackedSprite.boardPiece.exists)
                        { return this.trackedSprite.position; }
                        else
                        {
                            this.TrackCoords(this.currentPos);
                            return this.trackedPos;
                        }
                    }

                case TrackingMode.Position:
                    { return this.trackedPos; }

                default:
                    { throw new DivideByZeroException($"Unsupported tracking mode - {this.trackingMode}."); }
            }
        }

        public void TrackLiveAnimal()
        {
            if (this.TrackedSpriteExists) return;

            var allSprites = this.world.grid.GetAllSprites(Cell.Group.ColAll);
            var animals = allSprites.Where(sprite => sprite.boardPiece.GetType() == typeof(Animal) && sprite.boardPiece.alive).ToList();
            var index = SonOfRobinGame.random.Next(0, animals.Count);
            this.TrackPiece(animals[index].boardPiece);
        }

    }
}
