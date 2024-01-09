using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimEditor : Scene
    {
        private readonly SpriteFontBase font;

        private readonly AnimPkg[] animPkgArray;

        private Anim CurrentAnim
        { get { return this.currentAnimPkg.AllAnimList[this.currentAnimIndex]; } }

        private int currentAnimPkgIndex;
        private int currentAnimIndex;

        private AnimPkg currentAnimPkg;
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
        private int outlineThickness;

        public AnimEditor() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(80, 80);
            this.rot = 0f;
            this.showColRect = true;
            this.showGfxRect = true;
            this.showEffect = false;
            this.outlineThickness = 1;

            var animPkgList = new List<AnimPkg> { };

            animPkgList.Add(AnimPkg.AddRPGMakerPackageV2ForSizeDict(pkgName: AnimData.PkgName.FoxGinger, atlasName: "characters/fox", colWidth: 15, colHeight: 19, gfxOffsetCorrection: new Vector2(0, -9), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } }));

            animPkgList.Add(AnimPkg.MakePackageForRpgMakerV1Data(pkgName: AnimData.PkgName.PlayerGirl, scale: 1f, animSize: 1, colWidth: 14, colHeight: 14, altasName: "characters/recolor_pt2", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0));

            animPkgList.Add(AnimPkg.MakePackageForRpgMakerV1Data(pkgName: AnimData.PkgName.PlayerBoy, scale: 1f, animSize: 1, colWidth: 14, colHeight: 14, altasName: "characters/actor29rec4", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0));

            //animPkgList.Add(AnimPkg.MakePackageForSingleImage(pkgName: AnimData.PkgName.GrassRegular, width: 24, height: 20, scale: 3f, layer: 1, animSize: 1, altasName: "_processed_grass_s1", hasOnePixelMargin: true));

            //animPkgList.Add(AnimPkg.MakePackageForSingleImage(pkgName: AnimData.PkgName.PlantPoison, width: 32, height: 33, scale: 4f, layer: 0, animSize: 1, altasName: "_processed_plant_poison", hasOnePixelMargin: true));

            //animPkgList.Add(new(pkgName: AnimData.PkgName.FoxWhite, colWidth: 20, colHeight: 20));
            //animPkgList.LastOrDefault().AddAnim(
            //    new(animPkg: this.currentAnimPkg, size: 1,
            //    frameArray:
            //    [
            //        new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: false, scale: 4f),
            //        new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: true, mirrorY: false, scale: 4f),
            //        new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: true, scale: 4f),
            //        new AnimFrameNew(atlasName: "characters/fox", layer: 1, cropRect: new Rectangle(x: 48 * 3, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: true, mirrorY: true, scale: 4f),
            //    ]
            //    ));

            //animPkgList.Add(new(pkgName: AnimData.PkgName.WoodLogHard, colWidth: 16, colHeight: 15));
            //animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
            //    [new AnimFrameNew(atlasName: "_processed_wood_hard", layer: 1, scale: 5f, cropRect: new Rectangle(0, 0, 44, 44))]));

            //animPkgList.Add(new(pkgName: AnimData.PkgName.BearBrown, colWidth: 16, colHeight: 15));
            //animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
            //    [new AnimFrameNew(atlasName: "characters/bear", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: false, scale: 4f)]));

            //animPkgList.Add(new(pkgName: AnimData.PkgName.RabbitBlack, colWidth: 16, colHeight: 15));
            //animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
            //    [new AnimFrameNew(atlasName: "characters/rabbits", layer: 1, cropRect: new Rectangle(x: 48 * 4, y: 48 * 2, width: 48, height: 48), duration: 60, mirrorX: false, mirrorY: false, scale: 4f)]));

            //animPkgList.Add(new(pkgName: AnimData.PkgName.Flame, colWidth: 8, colHeight: 4));
            //animPkgList.LastOrDefault().AddAnim(new(animPkg: this.currentAnimPkg, size: 1, frameArray:
            //    [
            //        new AnimFrameNew(atlasName: "_processed_flame_small_1", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 40, 46)),
            //        new AnimFrameNew(atlasName: "_processed_flame_small_2", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 41, 43)),
            //        new AnimFrameNew(atlasName: "_processed_flame_small_3", layer: 1, duration: 6, cropRect: new Rectangle(0, 0, 45, 44)),
            //    ]));

            this.animPkgArray = animPkgList.ToArray();
            this.currentAnimPkgIndex = 0;
            this.currentAnimIndex = 0;

            this.AssignCurrentAnim();
            this.UpdateRects();

            this.viewParams.ScaleX = 0.5f;
            this.viewParams.ScaleY = 0.5f;

            this.effect = SonOfRobinGame.ContentMgr.Load<Effect>("effects/Border");
        }

        private void AssignCurrentAnim()
        {
            this.currentAnimIndex = 0;
            this.currentAnimPkg = this.animPkgArray[this.currentAnimPkgIndex];
            this.RewindAnim();
            this.currentAnimFrame = this.CurrentAnim.frameArray[0];
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
            if (Keyboard.IsPressed(Keys.F))
            {
                this.viewParams.ScaleX -= scaleVal;
                this.viewParams.ScaleY -= scaleVal;
            }
            if (Keyboard.IsPressed(Keys.G))
            {
                this.viewParams.ScaleX += scaleVal;
                this.viewParams.ScaleY += scaleVal;
            }
            if (Keyboard.IsPressed(Keys.H))
            {
                this.viewParams.ScaleX = 0.5f;
                this.viewParams.ScaleY = 0.5f;
            }

            bool animPkgIndexChanged = false;
            if (Keyboard.HasBeenPressed(Keys.Q))
            {
                this.currentAnimPkgIndex--;
                animPkgIndexChanged = true;
            }
            if (Keyboard.HasBeenPressed(Keys.W))
            {
                this.currentAnimPkgIndex++;
                animPkgIndexChanged = true;
            }

            if (animPkgIndexChanged)
            {
                if (this.currentAnimPkgIndex < 0) this.currentAnimPkgIndex = this.animPkgArray.Length - 1;
                if (this.currentAnimPkgIndex > this.animPkgArray.Length - 1) this.currentAnimPkgIndex = 0;
                this.AssignCurrentAnim();
            }

            if (Keyboard.HasBeenPressed(Keys.E))
            {
                this.currentAnimIndex--;

                if (this.currentAnimIndex < 0) this.currentAnimIndex = this.currentAnimPkg.AllAnimList.Count - 1;
                this.RewindAnim();
            }
            if (Keyboard.HasBeenPressed(Keys.R))
            {
                this.currentAnimIndex++;

                if (this.currentAnimIndex >= this.currentAnimPkg.AllAnimList.Count) this.currentAnimIndex = 0;
                this.RewindAnim();
            }

            // TODO add controls for gfxOffsetCorrection

            int colWidthChange = 0;
            int colHeightChange = 0;

            if (Keyboard.HasBeenPressed(Keys.OemMinus)) colWidthChange--;
            if (Keyboard.HasBeenPressed(Keys.OemPlus)) colWidthChange++;
            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets)) colHeightChange--;
            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) colHeightChange++;

            if (colWidthChange != 0 || colHeightChange != 0)
            {
                AnimPkg animPkg = this.currentAnimPkg.MakeCopyWithEditedOffset(this.currentAnimPkg.colRect.Width + colWidthChange, this.currentAnimPkg.colRect.Height + colHeightChange);

                this.animPkgArray[this.currentAnimPkgIndex] = animPkg;
                this.currentAnimPkg = animPkg;
            }

            foreach (var kvp in new Dictionary<Keys, int> {
                { Keys.D1, 1 },
                { Keys.D2, 2 },
                { Keys.D3, 3 },
                { Keys.D4, 4 },
                { Keys.D5, 5 },
                { Keys.D6, 6 },
                { Keys.D7, 7 },
                { Keys.D8, 8 }})
            {
                if (Keyboard.HasBeenPressed(kvp.Key))
                {
                    this.outlineThickness = kvp.Value;
                    break;
                }
            }

            this.UpdateAnimation();
            this.UpdateRects();
        }

        public bool AnimFinished { get { return this.currentAnimFrame.duration == 0; } }

        public void RewindAnim()
        {
            this.currentFrameIndex = 0;
            this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];
            this.currentFrameTimeLeft = this.currentAnimFrame.duration;
        }

        public void UpdateAnimation()
        {
            if (this.currentAnimFrame.duration == 0) return; // duration == 0 will stop the animation

            this.currentFrameTimeLeft--;
            if (this.currentFrameTimeLeft <= 0)
            {
                this.currentFrameIndex++;
                if (this.currentFrameIndex >= this.CurrentAnim.frameArray.Length) this.RewindAnim();
                this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];
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
                Rectangle cropRect = this.currentAnimFrame.cropRect;
                Texture2D texture = this.currentAnimFrame.Texture;

                this.effect.Parameters["drawColor"].SetValue(Color.White.ToVector4());

                // border

                this.effect.Parameters["outlineColor"].SetValue(Color.White.ToVector4());
                this.effect.Parameters["outlineThickness"].SetValue(this.outlineThickness);
                this.effect.Parameters["drawFill"].SetValue(true);
                this.effect.Parameters["textureSize"].SetValue(new Vector2(this.currentAnimFrame.Texture.Width, this.currentAnimFrame.Texture.Height));

                this.effect.Parameters["cropXMin"].SetValue((float)cropRect.Left / (float)texture.Width);
                this.effect.Parameters["cropXMax"].SetValue((float)cropRect.Right / (float)texture.Width);
                this.effect.Parameters["cropYMin"].SetValue((float)cropRect.Top / (float)texture.Height);
                this.effect.Parameters["cropYMax"].SetValue((float)cropRect.Bottom / (float)texture.Height);

                // burn

                //this.effect.Parameters["intensity"].SetValue(1f);
                //this.effect.Parameters["time"].SetValue(SonOfRobinGame.CurrentUpdate / 35f);
                //this.effect.Parameters["phaseModifier"].SetValue(1f);
                //this.effect.Parameters["checkAlpha"].SetValue(true);

                // colorize

                //this.effect.Parameters["colorizeColor"].SetValue(Color.Blue.ToVector4());
                //this.effect.Parameters["opacity"].SetValue(1f);
                //this.effect.Parameters["checkAlpha"].SetValue(true);

                this.effect.CurrentTechnique.Passes[0].Apply();
            }

            Vector2 rotationOriginOverride = default;
            //rotationOriginOverride = new Vector2(this.currentAnimFrame.cropRect.Width * 0.5f, this.currentAnimFrame.cropRect.Height); // emulating SwayEvent

            this.currentAnimFrame.Draw(destRect: this.gfxRect, color: Color.White, rotation: this.rot, opacity: 1f, submergeCorrection: 0, rotationOriginOverride: rotationOriginOverride);

            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);
            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.gfxRect, color: Color.White * 0.35f, thickness: 1f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            string description = $"pos {(int)this.pos.X},{(int)this.pos.Y} rot: {Math.Round(this.rot, 2)} colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}\nAnimPkg: {this.currentAnimPkg.pkgName} animName: {this.CurrentAnim.name}\ntexture: {this.currentAnimFrame.Texture.Name} offset: {this.currentAnimFrame.gfxOffsetCorrection.X / this.currentAnimFrame.scale},{this.currentAnimFrame.gfxOffsetCorrection.Y / this.currentAnimFrame.scale}";

            SonOfRobinGame.SpriteBatch.End();

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            this.font.DrawText(
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