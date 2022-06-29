using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public double msgFrequency = 1.0f;
        public string msg = "";

        /// <summary>
        /// The msgFrequency here is the reporting time to update the message.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            now = gameTime.TotalGameTime.TotalSeconds;
            elapsed = (double)(now - last);
            if (elapsed > msgFrequency)
            {
                msg = $"FPS: {Math.Round(frames / elapsed)} updates: {updates} frames: {frames}";
                elapsed = 0;
                frames = 0;
                updates = 0;
                last = now;
            }
            updates++;
        }

        public void DrawFps(SpriteBatch spriteBatch, SpriteFont font, Vector2 fpsDisplayPosition, Color fpsTextColor)
        {
            int[] offsets = { -2, -1, 1, 2 };
            foreach (int x in offsets)
            {
                foreach (int y in offsets)
                { spriteBatch.DrawString(font, msg, fpsDisplayPosition + new Vector2(x, y), Color.Black); }
            }

            spriteBatch.DrawString(font, msg, fpsDisplayPosition, Color.White);
            frames++;
        }

        public void UpdateFpsCounter()
        {
            frames++;
        }



    }
}