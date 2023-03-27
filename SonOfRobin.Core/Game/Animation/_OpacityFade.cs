using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class OpacityFade
    {
        public enum Mode
        { Normal, CameraTargetObstruct, CameraTargetObstructRevert }

        public const int defaultDuration = 30;

        public readonly Sprite sprite;
        private readonly int duration;
        private readonly float destOpacity;
        private readonly float fadePerFrame;
        private int currentFrame;
        public readonly Mode mode;
        private readonly bool destroyPiece;

        public OpacityFade(Sprite sprite, float destOpacity, int duration = -1, bool destroyPiece = false, Mode mode = Mode.Normal)
        {
            // normal use

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

        public OpacityFade(Sprite sprite, int duration, float destOpacity, float fadePerFrame, int currentFrame, Mode mode, bool destroyPiece)
        {
            // deserialization

            this.sprite = sprite;
            this.duration = duration;
            this.destOpacity = destOpacity;
            this.currentFrame = currentFrame;
            this.fadePerFrame = fadePerFrame;
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
                    this.sprite.opacity = this.destOpacity;
                    this.sprite.opacityFade = null;
                    if (this.destroyPiece) this.sprite.boardPiece.Destroy();
                }
            }
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> opacityData = new Dictionary<string, object>
            {
                { "duration", this.duration },
                { "destOpacity", this.destOpacity },
                { "fadePerFrame", this.fadePerFrame },
                { "currentFrame", this.currentFrame },
                { "mode", this.mode },
                { "destroyPiece", this.destroyPiece },
            };

            return opacityData;
        }

        public static OpacityFade Deserialize(Object fadeData, Sprite sprite)
        {
            if (fadeData == null) return null;

            var fadeDict = (Dictionary<string, Object>)fadeData;

            int duration = (int)fadeDict["duration"];
            float destOpacity = (float)fadeDict["destOpacity"];
            float fadePerFrame = (float)fadeDict["fadePerFrame"];
            int currentFrame = (int)fadeDict["currentFrame"];
            Mode mode = (Mode)fadeDict["mode"];
            bool destroyPiece = (bool)fadeDict["destroyPiece"];

            return new OpacityFade(sprite: sprite, duration: duration, destroyPiece: destroyPiece, mode: mode, currentFrame: currentFrame, fadePerFrame: fadePerFrame, destOpacity: destOpacity);
        }
    }
}