using FontStashSharp;
using Microsoft.Xna.Framework;
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
        private AnimFrameNew currentAnimFrame;
        private Vector2 pos;
        private float rot;

        private Rectangle gfxRect;
        private Rectangle colRect;

        private bool showColRect;
        private bool showGfxRect;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(150, 150);
            this.rot = 0f;
            this.showColRect = true;
            this.showGfxRect = true;

            var animPkgList = new List<AnimPkg> { };

            animPkgList.Add(new(pkgName: AnimData.PkgName.FoxWhite, colWidth: 16, colHeight: 15));
            animPkgList.LastOrDefault().AddAnim(
                new(animPkg: this.currentAnimPkg, size: 1,
                frameArray: [new(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 3, y: 48 * 2, width: 48, height: 48))]));

            animPkgList.Add(new(pkgName: AnimData.PkgName.PlantPoison, colWidth: 16, colHeight: 15));
            animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray: [new(atlasName: "_processed_plant_poison", layer: 1)]));

            this.animPkgArray = animPkgList.ToArray();
            this.currentAnimPkgIndex = 0;

            this.AssignCurrentAnim();
            this.UpdateRects();
        }

        private void AssignCurrentAnim()
        {
            this.currentAnimPkg = this.animPkgArray[this.currentAnimPkgIndex];
            this.currentAnimFrame = this.currentAnimPkg.GetAnim(size: 1, name: "default").frameArray[0];
        }

        public override void Update()
        {
            if (Keyboard.IsPressed(Keys.Left)) this.pos.X--;
            if (Keyboard.IsPressed(Keys.Right)) this.pos.X++;
            if (Keyboard.IsPressed(Keys.Up)) this.pos.Y--;
            if (Keyboard.IsPressed(Keys.Down)) this.pos.Y++;

            if (Keyboard.HasBeenPressed(Keys.Z)) this.showColRect = !this.showColRect;
            if (Keyboard.HasBeenPressed(Keys.X)) this.showGfxRect = !this.showGfxRect;

            if (Keyboard.IsPressed(Keys.A)) this.rot -= 0.03f;
            if (Keyboard.IsPressed(Keys.S)) this.rot += 0.03f;

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

            this.UpdateRects();
        }

        private void UpdateRects()
        {
            // TODO move to Sprite class

            this.colRect = new(
                x: (int)(this.pos.X + this.currentAnimPkg.colRect.X),
                y: (int)(this.pos.Y + this.currentAnimPkg.colRect.Y),
                width: this.currentAnimPkg.colRect.Width,
                height: this.currentAnimPkg.colRect.Height);

            this.gfxRect = new(
                x: (int)(this.pos.X + this.currentAnimFrame.GfxOffset.X),
                y: (int)(this.pos.Y + this.currentAnimFrame.GfxOffset.Y),
                width: this.currentAnimFrame.CropRect.Width,
                height: this.currentAnimFrame.CropRect.Height);
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            this.currentAnimFrame.DrawWithRotation(position: this.pos, color: Color.White, rotation: this.rot, opacity: 1f);
            this.currentAnimFrame.Draw(destRect: this.gfxRect, color: Color.White, opacity: 0.5f);

            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.gfxRect, color: Color.White * 0.35f, thickness: 1f);
            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            string description = $"pos {(int)this.pos.X},{(int)this.pos.Y} rot: {Math.Round(this.rot, 2)} colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}\nAnimPkg: {this.currentAnimPkg.pkgName} texture: {this.currentAnimFrame.Texture.Name}";

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