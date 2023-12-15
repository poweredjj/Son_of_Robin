using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class ViewParams
    {
        private float rot;

        public float Rot
        { get { return rot; } set { rot = value; drawRot = value; } }

        private float posX;

        public float PosX
        { get { return posX; } set { posX = value; drawPosX = value; } }

        private float posY;

        public float PosY
        { get { return posY; } set { posY = value; drawPosY = value; } }

        private float scaleX;

        public float ScaleX
        { get { return scaleX; } set { scaleX = value; drawScaleX = value; } }

        private float scaleY;

        public float ScaleY
        { get { return scaleY; } set { scaleY = value; drawScaleY = value; } }

        private float opacity;

        public float Opacity
        { get { return opacity; } set { opacity = value; drawOpacity = value; } }

        private int width;

        public int Width
        { get { return width; } set { width = value; drawWidth = value; } }

        private int height;

        public int Height
        { get { return height; } set { height = value; drawHeight = value; } }

        public float drawRot;
        public float drawPosX;
        public float drawPosY;
        public float drawScaleX;
        public float drawScaleY;
        public float drawOpacity;
        public int drawWidth;
        public int drawHeight;

        public int CenterPosX
        { get { return (int)(((SonOfRobinGame.ScreenWidth * this.scaleX) - this.width) / 2); } }

        public int CenterPosY
        { get { return (int)(((SonOfRobinGame.ScreenHeight * this.scaleY) - this.height) / 2); } }

        public Vector2 Pos
        {
            get { return new Vector2(this.PosX, this.PosY); }
            set
            {
                this.PosX = value.X;
                this.PosY = value.Y;
            }
        }

        public Vector2 DrawPos
        { get { return new Vector2(this.drawPosX, this.drawPosY); } }

        public bool IsInTheLeftHalf
        {
            get
            {
                float sceneCenter = this.PosX + (this.width / 2f);
                float screenCenter = SonOfRobinGame.ScreenWidth * this.scaleX / 2f;
                return sceneCenter < screenCenter;
            }
        }

        public bool IsInTheTopHalf
        {
            get
            {
                float sceneCenter = this.PosY + (this.height / 2f);
                float screenCenter = SonOfRobinGame.ScreenHeight * this.scaleY / 2f;
                return sceneCenter < screenCenter;
            }
        }

        public ViewParams()
        {
            this.ResetValues();
        }

        public void ResetValues()
        {
            // input params

            this.PosX = 0f;
            this.PosY = 0f;
            this.Rot = 0f;
            this.ScaleX = 1f;
            this.ScaleY = 1f;
            this.Opacity = 1f; // has to be manually used during Draw()
            this.Width = SonOfRobinGame.ScreenWidth; // needed to rotate scene around its center and for centering
            this.Height = SonOfRobinGame.ScreenHeight; // needed to rotate scene around its center and for centering

            this.CopyBaseToDrawParams();
        }

        public void CopyBaseToDrawParams()
        {
            // output params (for drawing)

            this.drawPosX = this.PosX;
            this.drawPosY = this.PosY;
            this.drawRot = this.Rot;
            this.drawScaleX = this.ScaleX;
            this.drawScaleY = this.ScaleY;
            this.drawOpacity = this.Opacity;
            this.drawWidth = this.Width;
            this.drawHeight = this.Height;
        }

        public void CenterView(bool horizontally = true, bool vertically = true)
        {
            if (horizontally) this.PosX = this.CenterPosX;
            if (vertically) this.PosY = this.CenterPosY;
        }

        public void PutViewAtTheLeft(int margin = 0)
        {
            this.PosX = margin;
        }

        public void PutViewAtTheRight(int margin = 0)
        {
            this.PosX = (int)((SonOfRobinGame.ScreenWidth * this.scaleX) - this.width) - margin;
        }

        public void PutViewAtTheTop(int margin = 0)
        {
            this.PosY = margin;
        }

        public void PutViewAtTheBottom(int margin = 0)
        {
            this.PosY = (int)((SonOfRobinGame.ScreenHeight * this.scaleY) - this.height) - margin;
        }
    }
}