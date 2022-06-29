using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class ViewParams
    {
        public float rot;
        public float posX;
        public float posY;
        public float scaleX;
        public float scaleY;
        public float opacity;
        public int width;
        public int height;

        public float drawPosX;
        public float drawPosY;
        public float drawRot;
        public float drawScaleX;
        public float drawScaleY;
        public float drawOpacity;
        public float drawWidth;
        public float drawHeight;

        public Vector2 Pos
        {
            get { return new Vector2(this.posX, this.posY); }
            set
            {
                this.posX = value.X;
                this.posY = value.Y;
            }
        }
        public Vector2 DrawPos { get { return new Vector2(this.drawPosX, this.drawPosY); } }

        public ViewParams()
        { this.ResetValues(); }

        public void ResetValues()
        {
            // input params
            this.posX = 0f;
            this.posY = 0f;
            this.rot = 0f;
            this.scaleX = 1f;
            this.scaleY = 1f;
            this.opacity = 1f; // rarely used
            this.width = SonOfRobinGame.graphics.PreferredBackBufferWidth; // needed to rotate scene around its center and for centering
            this.height = SonOfRobinGame.graphics.PreferredBackBufferHeight; // needed to rotate scene around its center and for centering

            this.CopyBaseToDrawParams();
        }

        public void CopyBaseToDrawParams()
        {
            // output params (for drawing)

            this.drawPosX = this.posX;
            this.drawPosY = this.posY;
            this.drawRot = this.rot;
            this.drawScaleX = this.scaleX;
            this.drawScaleY = this.scaleY;
            this.drawOpacity = this.opacity;
            this.drawWidth = this.width;
            this.drawHeight = this.height;
        }

        public void CenterView(bool horizontally = true, bool vertically = true)
        {
            if (horizontally) this.posX = (int)(((SonOfRobinGame.VirtualWidth * this.scaleX) - this.width) / 2);
            if (vertically) this.posY = (int)(((SonOfRobinGame.VirtualHeight * this.scaleY) - this.height) / 2);
        }

        public void PutViewAtTheBottom()
        {
            this.posY = (int)((SonOfRobinGame.VirtualHeight * this.scaleY) - this.height);
        }

        public void PutViewAtTheRight()
        {
            this.posX = (int)((SonOfRobinGame.VirtualWidth * this.scaleX) - this.width);
        }

    }
}
