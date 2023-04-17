using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class AiData
    // contains ai working data
    {
        private readonly BoardPiece boardPiece;
        private static Vector2 nullPosition = new Vector2(-1, -1);
        public bool targetPosIsSet { get; private set; }
        private Vector2 targetPos;
        public bool dontStop;
        public int timeLeft;

        // this struct is created without constructor - Reset() should be invoked after creation

        public AiData(BoardPiece boardPiece)
        {
            this.boardPiece = boardPiece;
            this.Reset();
        }

        public void Reset()
        {
            if (this.boardPiece.visualAid != null) new OpacityFade(sprite: this.boardPiece.visualAid.sprite, destOpacity: 0f, destroyPiece: true);
            this.boardPiece.visualAid = null;

            this.targetPos = nullPosition;
            this.targetPosIsSet = false;
            this.dontStop = false;
            this.timeLeft = 0;
        }

        public Vector2 TargetPos
        {
            get { return targetPos; }
            set
            {
                this.targetPos = value;
                this.targetPosIsSet = true;
            }
        }
    }
}