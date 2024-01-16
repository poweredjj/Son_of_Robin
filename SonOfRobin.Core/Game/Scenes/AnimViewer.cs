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
    public class AnimViewer : Scene
    {
        private const float baseScale = 0.5f; // 0.5f

        private readonly SpriteFontBase font;
        private readonly AnimPkg[] animPkgArray;
        private Anim CurrentAnim { get { return this.currentAnimPkg.AllAnimList[this.currentAnimIndex]; } }

        private int currentAnimPkgIndex;
        private int currentAnimIndex;

        private AnimPkg currentAnimPkg;
        private AnimFrameNew currentAnimFrame;
        private Vector2 pos;
        private float rot;
        private int playSpeed;

        private int currentFrameIndex;
        private int currentFrameTimeLeft;

        private Rectangle gfxRect;
        private Rectangle colRect;

        private bool showColRect;
        private bool showGfxRect;
        private bool showEffect;
        private bool flatShadow;
        private bool drawLightAndShadow;

        private readonly Effect effect;
        private int outlineThickness;

        public AnimViewer() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(80, 80);
            this.rot = 0f;
            this.playSpeed = 1; // 1
            this.showColRect = true;
            this.showGfxRect = false;
            this.showEffect = false;
            this.flatShadow = true;
            this.drawLightAndShadow = false;
            this.outlineThickness = 1;

            var animPkgList = new List<AnimPkg> { };

            foreach (AnimData.PkgName pkgName in AnimDataNew.allPkgNames)
            {
                AnimDataNew.LoadPackage(pkgName); // TODO move to InitialLoader
                if (AnimDataNew.pkgByName[pkgName] != null) animPkgList.Add(AnimDataNew.pkgByName[pkgName]);
            }

            //animPkgList.Add(AnimDataNew.MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: AnimData.PkgName.FoxGinger, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } }));

            //animPkgList.Add(AnimDataNew.MakePackageForRpgMakerV1Data(pkgName: AnimData.PkgName.PlayerGirl, scale: 1f, animSize: 1, colWidth: 14, colHeight: 14, altasName: "characters/recolor_pt2", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0));

            //animPkgList.Add(AnimDataNew.MakePackageForRpgMakerV1Data(pkgName: AnimData.PkgName.PlayerBoy, scale: 1f, animSize: 1, colWidth: 14, colHeight: 14, altasName: "characters/actor29rec4", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0));

            //animPkgList.Add(AnimDataNew.MakePackageForSingleImage(pkgName: AnimData.PkgName.GrassRegular, width: 24, height: 20, scale: 3f, layer: 1, animSize: 1, altasName: "_processed_grass_s1", hasOnePixelMargin: true));

            //animPkgList.Add(AnimDataNew.MakePackageForSingleImage(pkgName: AnimData.PkgName.PlantPoison, width: 32, height: 33, scale: 1f, layer: 0, animSize: 1, altasName: "_processed_plant_poison", hasOnePixelMargin: true));

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

            this.animPkgArray = animPkgList.OrderBy(a => a.pkgName).ToArray();
            this.currentAnimPkgIndex = this.animPkgArray.Length - 1;
            this.currentAnimIndex = 0;

            this.AssignCurrentAnim();
            this.UpdateRects();

            this.viewParams.ScaleX = baseScale;
            this.viewParams.ScaleY = baseScale;

            this.effect = SonOfRobinGame.ContentMgr.Load<Effect>("effects/Border");
        }

        private void AssignCurrentAnim()
        {
            this.currentAnimPkg = this.animPkgArray[this.currentAnimPkgIndex];
            if (this.currentAnimIndex > this.currentAnimPkg.AllAnimList.Count - 1) this.currentAnimIndex = 0;
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
            if (Keyboard.HasBeenPressed(Keys.B)) this.flatShadow = !this.flatShadow;
            if (Keyboard.HasBeenPressed(Keys.N)) this.drawLightAndShadow = !this.drawLightAndShadow;

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
                this.viewParams.ScaleX = baseScale;
                this.viewParams.ScaleY = baseScale;
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

            if (Keyboard.HasBeenPressed(Keys.Insert)) this.playSpeed--;
            if (Keyboard.HasBeenPressed(Keys.Delete)) this.playSpeed++;
            this.playSpeed = Math.Clamp(value: this.playSpeed, min: 1, max: 50);

            int colWidthChange = 0;
            int colHeightChange = 0;

            if (Keyboard.HasBeenPressed(Keys.OemMinus)) colWidthChange--;
            if (Keyboard.HasBeenPressed(Keys.OemPlus)) colWidthChange++;
            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets)) colHeightChange--;
            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets)) colHeightChange++;

            if (colWidthChange != 0 || colHeightChange != 0)
            {
                AnimPkg animPkg = this.currentAnimPkg.MakeCopyWithEditedColOffset(this.currentAnimPkg.colRect.Width + colWidthChange, this.currentAnimPkg.colRect.Height + colHeightChange);

                this.animPkgArray[this.currentAnimPkgIndex] = animPkg;
                this.currentAnimPkg = animPkg;
            }

            Vector2 gfxOffsetCorrectionChange = Vector2.Zero;

            if (Keyboard.HasBeenPressed(Keys.NumPad4)) gfxOffsetCorrectionChange.X--;
            if (Keyboard.HasBeenPressed(Keys.NumPad6)) gfxOffsetCorrectionChange.X++;
            if (Keyboard.HasBeenPressed(Keys.NumPad8)) gfxOffsetCorrectionChange.Y--;
            if (Keyboard.HasBeenPressed(Keys.NumPad2)) gfxOffsetCorrectionChange.Y++;

            if (gfxOffsetCorrectionChange != Vector2.Zero)
            {
                Vector2 gfxOffsetCorrection = (this.currentAnimFrame.gfxOffsetCorrection / this.currentAnimFrame.scale) + gfxOffsetCorrectionChange;
                gfxOffsetCorrection.X = (float)Math.Round(gfxOffsetCorrection.X);
                gfxOffsetCorrection.Y = (float)Math.Round(gfxOffsetCorrection.Y);

                this.CurrentAnim.EditGfxOffsetCorrection(gfxOffsetCorrection);
                this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                this.UpdateAnimation();
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
            if (this.currentAnimFrame.duration == 0)
            {
                if (this.CurrentAnim.switchWhenComplete)
                {
                    string switchName = this.CurrentAnim.switchName;
                    var allAnimList = this.currentAnimPkg.AllAnimList;

                    for (int i = 0; i < allAnimList.Count; i++)
                    {
                        Anim anim = allAnimList[i];
                        if (anim.name == switchName)
                        {
                            this.currentAnimIndex = i;
                            this.RewindAnim();
                        }
                    }
                }

                return; // duration == 0 will stop the animation
            }

            this.currentFrameTimeLeft--;
            if (this.currentFrameTimeLeft <= 0)
            {
                this.currentFrameIndex++;
                if (this.currentFrameIndex >= this.CurrentAnim.frameArray.Length) this.RewindAnim();
                this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];
                this.currentFrameTimeLeft = this.currentAnimFrame.duration * this.playSpeed;
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

            Vector2 lightPos = new(40, 40);
            int lightSize = 40;
            Rectangle lightRect = new(x: 0, y: 0, width: lightSize, height: lightSize);
            lightRect.Offset((int)(lightPos.X - (lightSize / 2)), (int)(lightPos.Y - (lightSize / 2)));

            if (this.drawLightAndShadow) this.DrawShadow(color: Color.Black * 0.5f, lightPos: lightPos, shadowAngle: Helpers.GetAngleBetweenTwoPoints(start: lightPos, end: this.pos));

            this.currentAnimFrame.Draw(position: this.pos, color: Color.White, rotation: this.rot, opacity: 1.0f, submergeCorrection: 0, rotationOriginOverride: rotationOriginOverride);

            if (this.drawLightAndShadow) Helpers.DrawTextureInsideRect(texture: TextureBank.GetTexture(textureName: TextureBank.TextureName.LightSphereWhite), rectangle: lightRect, color: Color.Yellow * 0.8f);

            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);
            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.gfxRect, color: Color.White * 0.35f, thickness: 1f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            string description = "";

            description += $"colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}\noffset: {(int)(this.currentAnimFrame.gfxOffsetCorrection.X / this.currentAnimFrame.scale)},{(int)(this.currentAnimFrame.gfxOffsetCorrection.Y / this.currentAnimFrame.scale)}\n";
            description += "\n";
            description += $"pos {(int)this.pos.X},{(int)this.pos.Y} rot: {Math.Round(this.rot, 2)} speed: 1/{this.playSpeed}\n";
            description += $"AnimPkg: {this.currentAnimPkg.pkgName} layer: {this.currentAnimFrame.layer} animName: {this.CurrentAnim.name}\ntexture: {this.currentAnimFrame.Texture.Name}\n";

            SonOfRobinGame.SpriteBatch.End();

            SonOfRobinGame.SpriteBatch.Begin();

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

        private void DrawShadow(Color color, Vector2 lightPos, float shadowAngle, float drawOffsetX = 0f, float drawOffsetY = 0f, float yScaleForce = 0f)
        {
            float distance = Vector2.Distance(lightPos, this.pos);

            AnimFrameNew animFrame = this.currentAnimFrame;
            Vector2 spritePos = this.pos;
            float opacity = 1f;

            if (this.flatShadow)
            {
                Vector2 diff = this.pos - lightPos;
                Vector2 limit = new(this.gfxRect.Width / 8, this.gfxRect.Height / 8);
                Vector2 offset = Vector2.Clamp(value1: diff / 6f, min: -limit, max: limit);
                Rectangle simulRect = this.gfxRect;
                simulRect.Offset(offset);

                // if (!this.world.camera.viewRect.Intersects(simulRect)) return;

                this.currentAnimFrame.Draw(position: this.pos + offset, color: color * opacity, rotation: this.rot, opacity: 1.0f, submergeCorrection: 0);
            }
            else
            {
                float xScale = animFrame.scale;
                float yScale = Math.Max(animFrame.scale / distance * 100f, animFrame.scale * 0.3f);
                yScale = yScaleForce == 0 ? Math.Min(yScale, animFrame.scale * 3f) : animFrame.scale * yScaleForce;

                SonOfRobinGame.SpriteBatch.Draw(
                    animFrame.Texture,
                    position: new Vector2(spritePos.X + drawOffsetX, spritePos.Y + drawOffsetY),
                    sourceRectangle: animFrame.cropRect,
                    color: color * opacity,
                    rotation: shadowAngle + (float)(Math.PI / 2f),
                    origin: new Vector2((float)animFrame.cropRect.Width * 0.5f, animFrame.cropRect.Height * 0.98f),
                    scale: new Vector2(xScale, yScale),
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }
    }
}