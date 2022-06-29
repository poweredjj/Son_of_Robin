using System;

namespace SonOfRobin
{
    [Serializable]
    public class OpacityFade
    {
        public static readonly int defaultDuration = 20;

        [NonSerialized]
        public Sprite sprite; // must be updated manually after deserializing

        private readonly int duration;
        private readonly float destOpacity;
        private readonly float fadePerFrame;
        private int currentFrame;
        private readonly bool playerObstructMode;

        public OpacityFade(Sprite sprite, float destOpacity, int duration = -1, bool playerObstructMode = false)
        {
            this.sprite = sprite;
            this.duration = duration;
            if (this.duration == -1) this.duration = defaultDuration;
            this.destOpacity = destOpacity;
            this.currentFrame = 0;
            this.fadePerFrame = (destOpacity - this.sprite.opacity) / (float)this.duration;
            this.playerObstructMode = playerObstructMode;
        }

        public void Process()
        {
            this.currentFrame++;

            switch (this.playerObstructMode)
            {
                case true:

                    if (!this.sprite.ObstructsPlayer)
                    {
                        this.sprite.opacityFade = new OpacityFade(sprite: this.sprite, duration: this.duration, destOpacity: 1f);
                        return;
                    }

                    if (this.currentFrame <= this.duration) { this.sprite.opacity += this.fadePerFrame; }
                    else { this.sprite.opacity = this.destOpacity; }

                    return;

                case false:
                    this.sprite.opacity += this.fadePerFrame;
                    if (this.currentFrame > this.duration) this.sprite.opacity = this.destOpacity;
                    if (this.sprite.opacity == this.destOpacity) this.sprite.opacityFade = null;

                    return;
            }

        }

    }
}
