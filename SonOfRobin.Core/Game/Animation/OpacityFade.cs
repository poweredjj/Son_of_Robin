﻿namespace SonOfRobin
{
    public class OpacityFade
    {
        public enum Mode : byte
        {
            Normal,
            CameraTargetObstruct,
            CameraTargetObstructRevert,
        }

        public const int defaultDuration = 30;

        private readonly Sprite sprite;
        private readonly int duration;
        private readonly float destOpacity;
        private readonly float fadePerFrame;
        private int currentFrame;
        public readonly Mode mode;
        private readonly bool destroyPiece;

        public OpacityFade(Sprite sprite, float destOpacity, int duration = -1, bool destroyPiece = false, Mode mode = Mode.Normal)
        {
            this.sprite = sprite;
            this.duration = duration;
            if (this.duration == -1) this.duration = defaultDuration;
            this.destOpacity = destOpacity;
            this.currentFrame = 0;
            this.fadePerFrame = (destOpacity - this.sprite.opacity) / (float)this.duration;
            this.mode = mode;
            this.destroyPiece = destroyPiece;

            this.sprite.opacityFade = this;
        }

        public void Process()
        {
            this.currentFrame++;

            if (this.mode == Mode.CameraTargetObstruct)
            {
                if (!this.sprite.ObstructsCameraTarget)
                {
                    new OpacityFade(sprite: this.sprite, duration: this.duration, destOpacity: 1f, mode: Mode.CameraTargetObstructRevert);
                    return;
                }

                if (this.currentFrame <= this.duration) this.sprite.opacity += this.fadePerFrame;
                else this.sprite.opacity = this.destOpacity;
            }
            else
            {
                this.sprite.opacity += this.fadePerFrame;

                if (this.currentFrame > this.duration)
                {
                    this.Finish();
                }
            }
        }

        public void Finish()
        {
            this.sprite.opacity = this.destOpacity;
            this.sprite.opacityFade = null;
            if (this.destroyPiece)
            {
                if (this.sprite.lightEngine != null) this.sprite.lightEngine.Deactivate(); // to avoid flashing on the last frame
                this.sprite.boardPiece.Destroy();
            }
        }
    }
}