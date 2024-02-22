using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Camera
    {
        private readonly World world;
        private readonly bool keepInWorldBounds;
        private readonly bool useFluidMotionForMove;
        private readonly bool useFluidMotionForZoom;
        private readonly bool useWorldScale;
        private int lastUpdateFrame;
        private TrackingMode trackingMode;
        private Sprite trackedSprite;
        private Vector2 trackedSpritePrevPos;
        private Vector2 aheadCorrection;
        private int spriteTrackedSince;
        private bool trackedSpriteReached;
        private Vector2 trackedPos;
        private Tweener tweener;
        public Vector2 shakeVal;

        public Vector2 TrackedPos
        { get { return this.trackingMode == TrackingMode.Position ? this.trackedPos : this.trackedSprite.position; } }

        public int CurrentSpriteTrackingDuration
        { get { return this.trackedSprite == null ? 0 : this.world.CurrentUpdate - this.spriteTrackedSince; } }

        public Vector2 CurrentPos { get; private set; }
        public float TargetZoom { get; private set; }
        public float CurrentZoom { get; private set; }

        public Camera(World world, bool useFluidMotionForMove, bool useFluidMotionForZoom, bool useWorldScale, bool keepInWorldBounds = true)
        {
            this.ResetMovementSpeed();
            this.ResetZoom();

            this.world = world;
            this.tweener = new Tweener();
            this.shakeVal = Vector2.Zero;
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
            this.spriteTrackedSince = 0;
            this.trackedSprite = null;
            this.trackedSpritePrevPos = Vector2.Zero;
            this.aheadCorrection = Vector2.Zero;
            this.trackedSpriteReached = false;
            this.trackedPos = Vector2.One;
            this.lastUpdateFrame = 0;
        }

        public void SetViewParams(Scene scene)
        {
            scene.viewParams.Width = this.world.ActiveLevel.width;
            scene.viewParams.Height = this.world.ActiveLevel.height;
            scene.viewParams.PosX = this.viewPos.X;
            scene.viewParams.PosY = this.viewPos.Y;

            float worldScale = this.useWorldScale ? Preferences.worldScale : 1f;
            float scale = 1f / (worldScale * this.CurrentZoom);
            scene.viewParams.ScaleX = scale;
            scene.viewParams.ScaleY = scale;

            if (Preferences.debugShowOutsideCamera)
            {
                scene.viewParams.ScaleX *= 2;
                scene.viewParams.ScaleY *= 2;
                scene.viewParams.PosX += this.viewRect.Width / 2;
                scene.viewParams.PosY += this.viewRect.Height / 2;
            }
        }

        private const int defaultMovementSlowdown = 20;
        private const int defaultZoomSlowdown = 20;

        private bool disableFluidMotionMoveForOneFrame;
        private int movementSlowdown;
        private int zoomSlowdown;

        public Rectangle viewRect;
        private Vector2 viewPos;

        public Rectangle ExtendedViewRect
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
                float worldScale = this.useWorldScale ? Preferences.worldScale : 1f;
                return (int)(SonOfRobinGame.ScreenWidth / this.CurrentZoom / worldScale);
            }
        }

        public int ScreenHeight
        {
            get
            {
                float worldScale = this.useWorldScale ? Preferences.worldScale : 1f;
                return (int)(SonOfRobinGame.ScreenHeight / this.CurrentZoom / worldScale);
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

        private enum TrackingMode : byte
        {
            Undefined = 0,
            Sprite = 1,
            Position = 2,
        }

        public void Update(Vector2 cameraCorrection, bool calculateAheadCorrection)
        {
            if (Scene.ProcessingMode == Scene.ProcessingModes.Draw || this.lastUpdateFrame == SonOfRobinGame.CurrentUpdate) return;

            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            if (this.useFluidMotionForZoom)
            {
                this.CurrentZoom += (this.TargetZoom - this.CurrentZoom) / this.zoomSlowdown;
                if (Math.Abs(this.TargetZoom - this.CurrentZoom) < 0.00005f) this.CurrentZoom = this.TargetZoom;
                if (this.CurrentZoom == this.TargetZoom) this.zoomSlowdown = movementSlowdown; // resetting to default zoom speed, after reaching target value
            }
            else
            {
                this.CurrentZoom = this.TargetZoom;
                this.zoomSlowdown = movementSlowdown;
            }

            float screenWidth = this.ScreenWidth;
            float screenHeight = this.ScreenHeight;

            Vector2 currentTargetPos = this.GetTargetCoords();

            if (this.world.demoMode && Scene.GetTopSceneOfType(typeof(StartLogo)) == null) // when there is menu on the right
            {
                currentTargetPos.X += screenWidth * 0.33f; // to put the camera center on the left side
                calculateAheadCorrection = false; // to avoid losing sight of target
            }

            if (calculateAheadCorrection && Preferences.smartCamera &&
                this.trackingMode == TrackingMode.Sprite &&
                cameraCorrection == Vector2.Zero &&
                this.trackedSpriteReached &&
                this.trackedSprite.boardPiece.GetType() == typeof(Player))
            {
                bool moveBackToPlayer;
                Vector2 aheadCorrectionTarget;

                if (this.world.Player.pointWalkTarget != Vector2.Zero)
                {
                    moveBackToPlayer = this.world.Player.PointWalkTargetReached;

                    Vector2 pointWalkTargetCorrected = this.world.Player.pointWalkTarget;

                    if (!moveBackToPlayer)
                    {
                        Vector2 maxCorrectionSize = new Vector2(this.viewRect.Width * (Preferences.enableTouchJoysticks ? 0.25f : 0.35f), this.viewRect.Height * 0.35f);

                        Vector2 maxCorrectionLeftTop = this.world.Player.sprite.position - maxCorrectionSize;
                        Vector2 maxCorrectionRightBottom = this.world.Player.sprite.position + maxCorrectionSize;

                        pointWalkTargetCorrected = Vector2.Clamp(value1: pointWalkTargetCorrected, min: maxCorrectionLeftTop, max: maxCorrectionRightBottom);
                    }

                    aheadCorrectionTarget = pointWalkTargetCorrected - currentTargetPos;
                }
                else
                {
                    Vector2 spriteMovement = currentTargetPos - this.trackedSpritePrevPos;
                    Vector2 normalizedMovement = spriteMovement == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(spriteMovement); // Vector2.Zero cannot be normalized correctly
                    aheadCorrectionTarget = new Vector2(this.viewRect.Width * (Preferences.enableTouchJoysticks ? 0.25f : 0.35f), this.viewRect.Height * 0.35f) * normalizedMovement;
                    moveBackToPlayer = normalizedMovement == Vector2.Zero;
                }

                float movementSlowdownForCorrection = moveBackToPlayer ? 110 : 70;
                if (!this.viewRect.Contains(this.world.Player.sprite.position)) movementSlowdownForCorrection = 1; // catching up to player, like during zoom in

                this.aheadCorrection.X += (aheadCorrectionTarget.X - this.aheadCorrection.X) / movementSlowdownForCorrection;
                this.aheadCorrection.Y += (aheadCorrectionTarget.Y - this.aheadCorrection.Y) / movementSlowdownForCorrection;

                if (Math.Abs(aheadCorrectionTarget.X - this.aheadCorrection.X) < 0.5f) this.aheadCorrection = new Vector2(aheadCorrectionTarget.X, this.aheadCorrection.Y);
                if (Math.Abs(aheadCorrectionTarget.Y - this.aheadCorrection.Y) < 0.5f) this.aheadCorrection = new Vector2(this.aheadCorrection.X, aheadCorrectionTarget.Y);

                currentTargetPos += this.aheadCorrection;
            }

            if (cameraCorrection != Vector2.Zero) this.aheadCorrection = Vector2.Zero;

            Vector2 viewCenter = new Vector2(0, 0); // to be updated below

            if (this.useFluidMotionForMove && !this.disableFluidMotionMoveForOneFrame)
            {
                if (this.shakeVal == Vector2.Zero)
                {
                    float cameraDistX = Math.Abs(this.viewRect.Center.X - currentTargetPos.X);
                    float cameraDistY = Math.Abs(this.viewRect.Center.Y - currentTargetPos.Y);

                    float movementSlowdownFactorX = (float)Helpers.ConvertRange(oldMin: 0, oldMax: this.viewRect.Width / 1.7, newMin: 0, newMax: defaultMovementSlowdown, oldVal: cameraDistX, clampToEdges: true);
                    float movementSlowdownFactorY = (float)Helpers.ConvertRange(oldMin: 0, oldMax: this.viewRect.Height / 1.7, newMin: 0, newMax: defaultMovementSlowdown, oldVal: cameraDistY, clampToEdges: true);

                    movementSlowdownFactorX = Math.Max(movementSlowdown - movementSlowdownFactorX, 4);
                    movementSlowdownFactorY = Math.Max(movementSlowdown - movementSlowdownFactorY, 4);

                    // SonOfRobinGame.messageLog.AddMessage(text: $"cameraDist {(int)cameraDistX} {(int)cameraDistY} factor {(int)movementSlowdownFactorX} {(int)movementSlowdownFactorY}"); // for testing

                    viewCenter.X = this.CurrentPos.X + ((currentTargetPos.X - this.CurrentPos.X) / movementSlowdownFactorX);
                    viewCenter.Y = this.CurrentPos.Y + ((currentTargetPos.Y - this.CurrentPos.Y) / movementSlowdownFactorY);
                }
                else
                {
                    // needed to smoothen the motion, when shakeVal is present
                    viewCenter.X = this.CurrentPos.X + ((currentTargetPos.X - this.CurrentPos.X) / 2);
                    viewCenter.Y = this.CurrentPos.Y + ((currentTargetPos.Y - this.CurrentPos.Y) / 2);
                }
            }
            else
            {
                viewCenter.X = currentTargetPos.X;
                viewCenter.Y = currentTargetPos.Y;
                this.trackedSpriteReached = true;
                this.disableFluidMotionMoveForOneFrame = false;
            }

            viewCenter += cameraCorrection + this.shakeVal;

            float xMin = viewCenter.X - (screenWidth / 2f);
            float yMin = viewCenter.Y - (screenHeight / 2f);

            if (this.keepInWorldBounds)
            {
                xMin = Math.Min(Math.Max(xMin, 0), (float)this.world.ActiveLevel.width - screenWidth - 1f); // do not use Math.Clamp, it might throw ArgumentException
                yMin = Math.Min(Math.Max(yMin, 0), (float)this.world.ActiveLevel.height - screenHeight - 1f); // do not use Math.Clamp, it might throw ArgumentException
            }

            float xMax = xMin + screenWidth;
            float yMax = yMin + screenHeight;

            if (this.keepInWorldBounds)
            {
                xMax = Math.Min(xMax, this.world.ActiveLevel.width);
                yMax = Math.Min(yMax, this.world.ActiveLevel.height);
            }

            this.CurrentPos = viewCenter;

            if (Math.Abs(currentTargetPos.X - this.CurrentPos.X) < 0.5f) this.CurrentPos = new Vector2(currentTargetPos.X, this.CurrentPos.Y);
            if (Math.Abs(currentTargetPos.Y - this.CurrentPos.Y) < 0.5f) this.CurrentPos = new Vector2(this.CurrentPos.X, currentTargetPos.Y);

            this.viewRect.X = (int)xMin;
            this.viewRect.Y = (int)yMin;
            this.viewRect.Width = (int)Math.Floor(xMax - xMin);
            this.viewRect.Height = (int)Math.Floor(yMax - yMin);

            this.viewPos = new Vector2(-xMin, -yMin);

            if (!this.trackedSpriteReached && Vector2.Distance(this.CurrentPos, currentTargetPos) < 30) this.trackedSpriteReached = true;
            if (this.trackingMode == TrackingMode.Sprite) this.trackedSpritePrevPos = this.trackedSprite.position;
            this.lastUpdateFrame = SonOfRobinGame.CurrentUpdate;
        }

        public void AddRandomShake()
        {
            Vector2 movement = new Vector2(this.world.random.Next(-20, 20), this.world.random.Next(-20, 20));
            float durationSecs = (float)(this.world.random.NextSingle() * 0.09f) + 0.12f;

            this.AddShake(movement: movement, durationSecs: durationSecs);
        }

        public void AddShake(Vector2 movement, float durationSecs)
        {
            Tween tweenShake = this.tweener.FindTween(target: this, memberName: "shakeVal");
            if (tweenShake != null && tweenShake.IsAlive) return;

            this.tweener.TweenTo(target: this, expression: camera => camera.shakeVal, toValue: movement * 1.5f, duration: durationSecs * 0.7f, delay: 0)
            .AutoReverse()
            .Easing(EasingFunctions.BackInOut);
        }

        public void TrackPiece(BoardPiece trackedPiece, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Sprite;
            this.trackedSprite = trackedPiece.sprite;
            this.trackedSpritePrevPos = trackedPiece.sprite.position;
            this.aheadCorrection = Vector2.Zero;
            this.trackedSpriteReached = false;
            this.spriteTrackedSince = this.world.CurrentUpdate;
            this.disableFluidMotionMoveForOneFrame = moveInstantly;
        }

        public void TrackCoords(Vector2 position, bool moveInstantly = false)
        {
            this.trackingMode = TrackingMode.Position;
            this.trackedSprite = null;
            this.trackedSpriteReached = false;
            this.trackedSpritePrevPos = Vector2.Zero;
            this.aheadCorrection = Vector2.Zero;
            this.spriteTrackedSince = 0;
            this.trackedPos = new Vector2(position.X, position.Y);
            this.disableFluidMotionMoveForOneFrame = moveInstantly;
        }

        public void ResetZoom(bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.SetZoom(zoom: 1f, setInstantly: setInstantly, zoomSpeedMultiplier: zoomSpeedMultiplier);
        }

        public void SetZoom(float zoom, bool setInstantly = false, float zoomSpeedMultiplier = 1f)
        {
            this.zoomSlowdown = (int)(defaultZoomSlowdown / zoomSpeedMultiplier);
            this.TargetZoom = zoom;
            if (setInstantly) this.CurrentZoom = zoom;
        }

        public void ResetMovementSpeed()
        {
            this.SetMovementSpeed(1f);
        }

        public void SetMovementSpeed(float speedMultiplier)
        {
            this.movementSlowdown = (int)(defaultMovementSlowdown / speedMultiplier);
        }

        public Vector2 GetTargetCoords()
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

        public void TrackDemoModeTarget(bool firstRun)
        {
            if (this.TrackedSpriteExists && this.CurrentSpriteTrackingDuration < 60 * 30) return;

            if (firstRun)
            {
                this.SetZoom(zoom: 1.25f, setInstantly: firstRun);
                this.SetMovementSpeed(0.25f);
            }

            Rectangle searchRect = firstRun ? new Rectangle(x: 0, y: 0, width: this.world.ActiveLevel.width, height: this.world.ActiveLevel.height) : this.viewRect;

            for (int searchNo = 0; searchNo < (firstRun ? 1 : 4); searchNo++)
            {
                var spritesForRect = this.world.Grid.GetSpritesForRect(groupName: Cell.Group.Visible, rectangle: searchRect, addPadding: false);

                var namesToSearch = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox };

                var cameraTargets = spritesForRect.Where(sprite => sprite.boardPiece.alive && namesToSearch.Contains(sprite.boardPiece.name)).ToList();
                if (cameraTargets.Count == 0)
                {
                    searchRect.Inflate(searchRect.Width / 2, searchRect.Height / 2);
                    continue;
                }
                else
                {
                    var index = this.world.random.Next(cameraTargets.Count);
                    this.TrackPiece(trackedPiece: cameraTargets[index].boardPiece, moveInstantly: firstRun);

                    return;
                }
            }
        }
    }
}