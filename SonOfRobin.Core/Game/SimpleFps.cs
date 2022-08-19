using Microsoft.Xna.Framework;
using System;

namespace SonOfRobin
{
    public class SimpleFps
    {
        private double frames = 0;
        private double updates = 0;
        private double elapsed = 0;
        private double last = 0;
        private double now = 0;
        public double msgFrequency = 1.0;
        public string Message { get { return $"FPS: {this.FPS} updates: {this.Updates} frames: {this.Frames}"; } }
        public double FPS { get; private set; }
        public double Updates { get; private set; }
        public double Frames { get; private set; }

        /// <summary>
        /// The msgFrequency here is the reporting time to update the message.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            this.now = gameTime.TotalGameTime.TotalSeconds;
            this.elapsed = now - last;

            if (this.elapsed > msgFrequency)
            {
                this.FPS = Math.Round(frames / elapsed);
                this.Updates = this.updates;
                this.Frames = this.frames;

                this.elapsed = 0;
                this.frames = 0;
                this.updates = 0;
                this.last = now;
            }

            this.updates++;
        }

        public void UpdateFpsCounter()
        {
            this.frames++;
        }

    }
}