using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AnimEditor : Scene
    {
        private readonly SpriteFontBase font;

        private readonly AnimPkg[] animPkgArray;
        private int currentAnimPkgIndex;

        private AnimPkg currentAnimPkg;
        private Anim currentAnim;
        private AnimFrameNew currentAnimFrame;
        private Vector2 pos;
        private float rot;

        private int currentFrameIndex;
        private int currentFrameTimeLeft;

        private Rectangle gfxRect;
        private Rectangle colRect;

        private bool showColRect;
        private bool showGfxRect;
        private bool showEffect;

        private readonly Effect effect;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(130, 130);
            this.rot = 0f;
            this.showColRect = true;
            this.showGfxRect = true;
            this.showEffect = true;

            var animPkgList = new List<AnimPkg> { };

            animPkgList.Add(new(pkgName: AnimData.PkgName.FoxWhite, colWidth: 20, colHeight: 20));
            animPkgList.LastOrDefault().AddAnim(
                new(animPkg: this.currentAnimPkg, size: 1,
                frameArray:
                [
                    new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: false, scale: 4f),
                    new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: true, mirrorY: false, scale: 4f),
                    new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: true, scale: 4f),
                    new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 3, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: true, mirrorY: true, scale: 4f),
                ]
                ));

            animPkgList.Add(new(pkgName: AnimData.PkgName.PlantPoison, colWidth: 16, colHeight: 15));
            animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
                [new AnimFrameNew(atlasName: "_processed_plant_poison", layer: 1, scale: 2.5f, cropRect: new Rectangle(0,0,32,33))]));

            animPkgList.Add(new(pkgName: AnimData.PkgName.Flame, colWidth: 8, colHeight: 4));
            animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
                [
                    new AnimFrameNew(atlasName: "_processed_flame_small_1", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 40, 46)),
                    new AnimFrameNew(atlasName: "_processed_flame_small_2", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 41, 43)),
                    new AnimFrameNew(atlasName: "_processed_flame_small_3", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 45, 44)),
                ]));

            this.animPkgArray = animPkgList.ToArray();
            this.currentAnimPkgIndex = 0;

            this.AssignCurrentAnim();
            this.UpdateRects();

            this.effect = SonOfRobinGame.ContentMgr.Load<Effect>("effects/Border");
        }

        private void AssignCurrentAnim()
        {
            this.currentAnimPkg = this.animPkgArray[this.currentAnimPkgIndex];
            this.currentAnim = this.currentAnimPkg.GetAnim(size: 1, name: "default");
            this.RewindAnim();
            this.currentAnimFrame = this.currentAnim.frameArray[0];
        }

        public override void Update()
        {
            if (Keyboard.IsPressed(Keys.Left)) this.pos.X--;
            if (Keyboard.IsPressed(Keys.Right)) this.pos.X++;
            if (Keyboard.IsPressed(Keys.Up)) this.pos.Y--;
            if (Keyboard.IsPressed(Keys.Down)) this.pos.Y++;

            if (Keyboard.HasBeenPressed(Keys.Z)) this.showColRect = !this.showColRect;
            if (Keyboard.HasBeenPressed(Keys.X)) this.showGfxRect = !this.showGfxRect;
            if (Keyboard.HasBeenPressed(Keys.C)) this.showEffect = !this.showEffect;

            float rotVal = 0.03f;
            if (Keyboard.IsPressed(Keys.A)) this.rot -= rotVal;
            if (Keyboard.IsPressed(Keys.S)) this.rot += rotVal;
            if (Keyboard.IsPressed(Keys.D)) this.rot = 0f;

            float scaleVal = 0.005f;
            if (Keyboard.IsPressed(Keys.E))
            {
                this.viewParams.ScaleX -= scaleVal;
                this.viewParams.ScaleY -= scaleVal;
            }
            if (Keyboard.IsPressed(Keys.R))
            {
                this.viewParams.ScaleX += scaleVal;
                this.viewParams.ScaleY += scaleVal;
            }
            if (Keyboard.IsPressed(Keys.T))
            {
                this.viewParams.ScaleX = 1f;
                this.viewParams.ScaleY = 1f;
            }

            bool animIndexChanged = false;
            if (Keyboard.HasBeenPressed(Keys.Q))
            {
                this.currentAnimPkgIndex--;
                animIndexChanged = true;
            }
            if (Keyboard.HasBeenPressed(Keys.W))
            {
                this.currentAnimPkgIndex++;
                animIndexChanged = true;
            }

            if (animIndexChanged)
            {
                if (this.currentAnimPkgIndex < 0) this.currentAnimPkgIndex = this.animPkgArray.Length - 1;
                if (this.currentAnimPkgIndex > this.animPkgArray.Length - 1) this.currentAnimPkgIndex = 0;
                this.AssignCurrentAnim();
            }

            this.UpdateAnimation();
            this.UpdateRects();
        }

        public bool AnimFinished { get { return this.currentAnimFrame.duration == 0; } }

        public void RewindAnim()
        {
            this.currentFrameIndex = 0;
            this.currentAnimFrame = this.currentAnim.frameArray[this.currentFrameIndex];
            this.currentFrameTimeLeft = this.currentAnimFrame.duration;
        }

        public void UpdateAnimation()
        {
            if (this.currentAnimFrame.duration == 0) return; // duration == 0 will stop the animation

            this.currentFrameTimeLeft--;
            if (this.currentFrameTimeLeft <= 0)
            {
                this.currentFrameIndex++;
                if (this.currentFrameIndex >= this.currentAnim.frameArray.Length) this.RewindAnim();
                this.currentAnimFrame = this.currentAnim.frameArray[this.currentFrameIndex];
                this.currentFrameTimeLeft = this.currentAnimFrame.duration;
            }
        }

        private void UpdateRects()
        {
            // TODO move to Sprite class

            this.colRect = this.currentAnimPkg.GetColRectForPos(this.pos);
            this.gfxRect = this.currentAnimFrame.GetGfxRectForPos(this.pos);
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            if (this.showEffect)
            {
                this.effect.Parameters["drawColor"].SetValue(Color.White.ToVector4());
                this.effect.Parameters["outlineColor"].SetValue(Color.White.ToVector4());
                this.effect.Parameters["outlineThickness"].SetValue(1);
                this.effect.Parameters["drawFill"].SetValue(true);
                this.effect.Parameters["textureSize"].SetValue(new Vector2(this.gfxRect.Width, this.gfxRect.Height));

                Rectangle cropRect = this.currentAnimFrame.cropRect;

                this.effect.Parameters["cropXMin"].SetValue((float)cropRect.Left / (float)this.currentAnimFrame.Texture.Width);
                this.effect.Parameters["cropXMax"].SetValue((float)cropRect.Right / (float)this.currentAnimFrame.Texture.Width);
                this.effect.Parameters["cropYMin"].SetValue((float)cropRect.Top / (float)this.currentAnimFrame.Texture.Height);
                this.effect.Parameters["cropYMax"].SetValue((float)cropRect.Bottom / (float)this.currentAnimFrame.Texture.Height);

                this.effect.CurrentTechnique.Passes[0].Apply();
            }

            this.currentAnimFrame.DrawWithRotation(position: this.pos, color: Color.White, rotation: this.rot, opacity: 1f);
            this.currentAnimFrame.Draw(destRect: this.gfxRect, color: Color.White, opacity: 0.5f);

            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.gfxRect, color: Color.White * 0.35f, thickness: 1f);
            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            string description = $"pos {(int)this.pos.X},{(int)this.pos.Y} rot: {Math.Round(this.rot, 2)} colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}\nAnimPkg: {this.currentAnimPkg.pkgName} texture: {this.currentAnimFrame.Texture.Name}";

            SonOfRobinGame.SpriteBatch.End();

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: description,
                position: new Vector2(4, 4),
                color: Color.White,
                scale: Vector2.One,
                effect: FontSystemEffect.Stroked,
                effectAmount: 1);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}