using FontStashSharp;
using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class AnimEditor : Scene
    {
        private readonly SpriteFontBase font;

        private AnimPkg currentAnimPkg;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 2);
            this.currentAnimPkg = new AnimPkg(pkgName: AnimData.PkgName.FoxWhite, colRect: new Rectangle(x: 0, y: 0, width: 80, height: 80));

            AnimFrameNew[] frameArray = new AnimFrameNew[] { new AnimFrameNew(atlasName: "characters/fox", layer: 1) };

            Anim anim = new(animPkg: this.currentAnimPkg, size: 1, frameArray: frameArray);
            this.currentAnimPkg.AddAnim(anim);
        }

        public override void Update()
        {
            // TODO add input (changing packages, animations and setting frame offset)
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Anim anim = this.currentAnimPkg.GetAnim(size: 1, name: "default");

            AnimFrameNew animFrame = anim.frameArray[0];
            animFrame.DrawWithRotation(position: new Vector2(10, 10), color: Color.White, rotation: 0f, opacity: 1f); // TODO find out why it doesn't draw anything

            font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: "anim editor test",
                position: new Vector2(10, 10),
                color: Color.White,
                scale: Vector2.One,
                effect: FontSystemEffect.Stroked,
                effectAmount: 2);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}