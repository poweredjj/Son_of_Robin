using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace SonOfRobin
{
    public class AnimEditor : Scene
    {
        private readonly SpriteFontBase font;

        private AnimPkg animPkg;
        private AnimFrameNew animFrame;
        private Vector2 pos;

        private Rectangle gfxRect;
        private Rectangle colRect;

        private bool showColRect;
        private bool showGfxRect;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(150, 150);
            this.showColRect = true;
            this.showGfxRect = true;

            this.animPkg = new AnimPkg(pkgName: AnimData.PkgName.FoxWhite, colWidth: 80, colHeight: 40);

            // AnimFrameNew[] frameArray = [new(atlasName: "characters/fox", layer: 1)];
            AnimFrameNew[] frameArray = [new(atlasName: "_processed_plant_poison", layer: 1)];

            Anim anim = new(animPkg: this.animPkg, size: 1, frameArray: frameArray);
            this.animPkg.AddAnim(anim);

            this.animFrame = anim.frameArray[0];

            this.UpdateRects();
        }

        public override void Update()
        {
            Vector2 movePos = Vector2.Zero;

            if (Keyboard.IsPressed(Keys.Left)) movePos.X--;
            if (Keyboard.IsPressed(Keys.Right)) movePos.X++;
            if (Keyboard.IsPressed(Keys.Up)) movePos.Y--;
            if (Keyboard.IsPressed(Keys.Down)) movePos.Y++;
            if (Keyboard.HasBeenPressed(Keys.Z)) this.showColRect = !this.showColRect;
            if (Keyboard.HasBeenPressed(Keys.X)) this.showGfxRect = !this.showGfxRect;

            this.pos += movePos;

            this.UpdateRects();

            // TODO add input (changing packages, animations and setting frame offset)
        }

        private void UpdateRects()
        {
            // TODO move to Sprite class

            this.colRect = new(
                x: (int)(this.pos.X + this.animPkg.colRect.X),
                y: (int)(this.pos.Y + this.animPkg.colRect.Y),
                width: this.animPkg.colRect.Width,
                height: this.animPkg.colRect.Height);

            this.gfxRect = new(
                x: (int)(this.pos.X + this.animFrame.GfxOffset.X),
                y: (int)(this.pos.Y + this.animFrame.GfxOffset.Y),
                width: this.animFrame.CropRect.Width,
                height: this.animFrame.CropRect.Height);
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            this.animFrame.DrawWithRotation(position: this.pos, color: Color.White, rotation: 0.0f, opacity: 0.5f);
            this.animFrame.Draw(destRect: this.gfxRect, color: Color.White, opacity: 0.5f);

            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.gfxRect, color: Color.White * 0.35f);
            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: $"pos {(int)this.pos.X},{(int)this.pos.Y} colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}",
                position: new Vector2(4, 4),
                color: Color.White,
                scale: Vector2.One,
                effect: FontSystemEffect.Stroked,
                effectAmount: 1);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}