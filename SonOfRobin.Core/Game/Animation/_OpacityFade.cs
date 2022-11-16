using System;

namespace SonOfRobin
{
    [Serializable]
    public class OpacityFade
    {
        public enum Mode { Normal, CameraTargetObstruct, CameraTargetObstructRevert }

        public static readonly int defaultDuration = 30;

        [NonSerialized]
        public Sprite sprite; // must be updated manually after deserializing

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
        }

        public void Process()
        {
            this.currentFrame++;

            if (this.mode == Mode.CameraTargetObstruct)
            {
                if (!this.sprite.ObstructsCameraTarget)
                {
                    this.sprite.opacityFade = new OpacityFade(sprite: this.sprite, duration: this.duration, destOpacity: 1f, mode: Mode.CameraTargetObstructRevert);
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
                    this.sprite.opacity = this.destOpacity;
                    this.sprite.opacityFade = null;
                    if (this.destroyPiece) this.sprite.boardPiece.Destroy();
                }
            }
        }

    }
}
