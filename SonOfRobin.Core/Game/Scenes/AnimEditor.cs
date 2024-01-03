using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class AnimEditor : Scene
    {
        private readonly SpriteFontBase font;

        private AnimPkg currentAnimPkg;
        private Vector2 pos;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 2);
            this.currentAnimPkg = new AnimPkg(pkgName: AnimData.PkgName.FoxWhite, colWidth: 80, colHeight: 40);
            this.pos = new Vector2(150, 150);

            AnimFrameNew[] frameArray = [new(atlasName: "characters/fox", layer: 1)];

            Anim anim = new(animPkg: this.currentAnimPkg, size: 1, frameArray: frameArray);
            this.currentAnimPkg.AddAnim(anim);
        }

        public override void Update()
        {
            Vector2 movePos = Vector2.Zero;

            if (Keyboard.IsPressed(Keys.Left)) movePos.X--;
            if (Keyboard.IsPressed(Keys.Right)) movePos.X++;
            if (Keyboard.IsPressed(Keys.Up)) movePos.Y--;
            if (Keyboard.IsPressed(Keys.Down)) movePos.Y++;

            this.pos += movePos * 0.85f;

            // TODO add input (changing packages, animations and setting frame offset)
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Anim anim = this.currentAnimPkg.GetAnim(size: 1, name: "default");

            AnimFrameNew animFrame = anim.frameArray[0];
            animFrame.DrawWithRotation(position: this.pos, color: Color.White, rotation: 0f, opacity: 1f);

            Rectangle colRect = this.currentAnimPkg.colRect;
            colRect.X += (int)this.pos.X;
            colRect.Y += (int)this.pos.Y;

            // SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.GfxRect, this.GfxRect, Color.White * 0.35f);

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(colRect.X, colRect.Y, colRect.Width, colRect.Height), SonOfRobinGame.WhiteRectangle.Bounds, Color.Red * 0.55f);

            //SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.position.X, (int)this.position.Y, 1, 1), color: Color.Blue, thickness: 2f);
            // SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle((int)this.position.X, (int)this.position.Y, 1, 1), Color.White);

            font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: $"pos {(int)this.pos.X},{(int)this.pos.Y}",
                position: new Vector2(8, 8),
                color: Color.White,
                scale: Vector2.One,
                effect: FontSystemEffect.Stroked,
                effectAmount: 2);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}