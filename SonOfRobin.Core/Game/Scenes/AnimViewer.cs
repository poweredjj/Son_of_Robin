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
        public enum ControlMode : byte { GfxOffset, ShadowOriginFactor, ShadowPosOffset, ShadowHeightMultiplier, ColSize }

        public static readonly ControlMode[] allControlModes = (ControlMode[])Enum.GetValues(typeof(ControlMode));

        private const float baseScale = 0.5f; // 0.5f
        private const float originCorrStep = 0.01f;

        private readonly SpriteFontBase font;
        private readonly AnimPkg[] animPkgArray;
        private Anim CurrentAnim { get { return this.currentAnimPkg.AllAnimList[this.currentAnimIndex]; } }

        private int currentAnimPkgIndex;
        private int currentAnimIndex;

        private AnimPkg currentAnimPkg;
        private AnimFrame currentAnimFrame;
        private Vector2 pos;
        private float rot;
        private int playSpeed;

        private ControlMode controlMode;

        private int currentFrameIndex;
        private int currentFrameTimeLeft;

        private Rectangle gfxRect;
        private Rectangle colRect;

        private bool showColRect;
        private bool showGfxRect;
        private bool showEffect;
        private bool drawLightAndShadow;

        private readonly Effect effect;
        private int outlineThickness;

        public AnimViewer() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.controlMode = ControlMode.ShadowOriginFactor;

            this.font = SonOfRobinGame.FontPressStart2P.GetFont(8 * 1);
            this.pos = new Vector2(80, 80);
            this.rot = 0f;
            this.playSpeed = 1;
            this.showColRect = false;
            this.showGfxRect = false;
            this.showEffect = false;
            this.drawLightAndShadow = true;
            this.outlineThickness = 1;

            var animPkgList = new List<AnimPkg> { };

            AnimData.PrepareAllAnims();

            foreach (AnimData.PkgName pkgName in AnimData.allPkgNames)
            {
                animPkgList.Add(AnimData.pkgByName[pkgName]);
            }

            this.animPkgArray = animPkgList.OrderBy(a => a.name).ToArray();
            this.currentAnimPkgIndex = 0;
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
            if (Keyboard.HasBeenPressed(Keys.Escape))
            {
                this.Remove();
                if (GetTopSceneOfType(typeof(Menu)) == null) new InitialLoader();
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.PageUp))
            {
                this.controlMode--;
                if (!allControlModes.Contains(this.controlMode)) this.controlMode = allControlModes.Max();
            }

            if (Keyboard.HasBeenPressed(Keys.PageDown))
            {
                this.controlMode++;
                if (!allControlModes.Contains(this.controlMode)) this.controlMode = 0;
            }

            if (Keyboard.IsPressed(Keys.Left)) this.pos.X--;
            if (Keyboard.IsPressed(Keys.Right)) this.pos.X++;
            if (Keyboard.IsPressed(Keys.Up)) this.pos.Y--;
            if (Keyboard.IsPressed(Keys.Down)) this.pos.Y++;

            if (Keyboard.HasBeenPressed(Keys.Z)) this.showColRect = !this.showColRect;
            if (Keyboard.HasBeenPressed(Keys.X)) this.showGfxRect = !this.showGfxRect;
            if (Keyboard.HasBeenPressed(Keys.C)) this.showEffect = !this.showEffect;
            if (Keyboard.HasBeenPressed(Keys.B))
            {
                this.CurrentAnim.EditFlatShadow(!this.currentAnimFrame.hasFlatShadow);
                this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                this.UpdateAnimation();
            }
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

            Vector2 posValChange = Vector2.Zero;

            if (Keyboard.HasBeenPressed(Keys.NumPad4)) posValChange.X--;
            if (Keyboard.HasBeenPressed(Keys.NumPad6)) posValChange.X++;
            if (Keyboard.HasBeenPressed(Keys.NumPad8)) posValChange.Y--;
            if (Keyboard.HasBeenPressed(Keys.NumPad2)) posValChange.Y++;

            if (posValChange != Vector2.Zero)
            {
                switch (this.controlMode)
                {
                    case ControlMode.GfxOffset:

                        Vector2 gfxOffsetCorrection = (this.currentAnimFrame.gfxOffsetCorrection / this.currentAnimFrame.scale) + posValChange;
                        gfxOffsetCorrection.X = (float)Math.Round(gfxOffsetCorrection.X);
                        gfxOffsetCorrection.Y = (float)Math.Round(gfxOffsetCorrection.Y);

                        this.CurrentAnim.EditGfxOffsetCorrection(gfxOffsetCorrection);
                        this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                        this.UpdateAnimation();

                        break;

                    case ControlMode.ShadowOriginFactor:

                        Vector2 shadowOriginFactor = this.currentAnimFrame.shadowOriginFactor - (posValChange * originCorrStep);
                        shadowOriginFactor = new Vector2((float)Math.Round(shadowOriginFactor.X, 2), (float)Math.Round(shadowOriginFactor.Y, 2));

                        this.CurrentAnim.EditShadowOriginFactor(shadowOriginFactor);
                        this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                        this.UpdateAnimation();

                        break;

                    case ControlMode.ShadowPosOffset:

                        Vector2 shadowPosOffset = this.currentAnimFrame.shadowPosOffset + (posValChange * 0.5f);

                        this.CurrentAnim.EditShadowPosOffset(shadowPosOffset);
                        this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                        this.UpdateAnimation();

                        break;

                    case ControlMode.ShadowHeightMultiplier:

                        float shadowHeightMultiplier = this.currentAnimFrame.shadowHeightMultiplier - (posValChange.Y * 0.05f);
                        shadowHeightMultiplier = (float)Math.Round(shadowHeightMultiplier, 2);

                        this.CurrentAnim.EditShadowHeightMuliplier(shadowHeightMultiplier);
                        this.currentAnimFrame = this.CurrentAnim.frameArray[this.currentFrameIndex];

                        this.UpdateAnimation();

                        break;

                    case ControlMode.ColSize:

                        AnimPkg animPkg = this.currentAnimPkg.MakeCopyWithEditedColOffset(this.currentAnimPkg.colRect.Width + (int)posValChange.X, this.currentAnimPkg.colRect.Height - (int)posValChange.Y);

                        this.animPkgArray[this.currentAnimPkgIndex] = animPkg;
                        this.currentAnimPkg = animPkg;

                        break;

                    default:
                        throw new ArgumentException($"Unsupported controlMode - {this.controlMode}.");
                }
            }

            if (Keyboard.HasBeenPressed(Keys.P)) this.PrintFrameParams();

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
                this.effect.Parameters["textureSize"].SetValue(new Vector2(this.currentAnimFrame.imageObj.Width, this.currentAnimFrame.imageObj.Height));

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

            Vector2 lightPos = new(160, 70);
            int lightSize = 40;
            Rectangle lightRect = new(x: 0, y: 0, width: lightSize, height: lightSize);
            lightRect.Offset((int)(lightPos.X - (lightSize / 2)), (int)(lightPos.Y - (lightSize / 2)));

            if (this.drawLightAndShadow) this.DrawShadow(color: Color.Black * 0.5f, lightPos: lightPos, shadowAngle: Helpers.GetAngleBetweenTwoPoints(start: lightPos, end: this.pos));

            this.currentAnimFrame.Draw(position: this.pos, color: Color.White, rotation: this.rot, opacity: 1.0f, submergeCorrection: 0, rotationOriginOverride: rotationOriginOverride);

            if (this.drawLightAndShadow) Helpers.DrawTextureInsideRect(texture: TextureBank.GetTexture(textureName: TextureBank.TextureName.LightSphereWhite), rectangle: lightRect, color: Color.Yellow * 0.75f);

            if (this.showColRect) SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: this.colRect, color: Color.Red * 0.55f);
            if (this.showGfxRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.gfxRect, color: Color.White * 0.35f, thickness: 1f);

            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.Blue, thickness: 3f);
            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: new Rectangle((int)this.pos.X, (int)this.pos.Y, 1, 1), color: Color.White);

            string description = "";

            description += $"mode: {this.controlMode}, colRect: {this.colRect.Width}x{this.colRect.Height} gfxRect: {this.gfxRect.Width}x{this.gfxRect.Height}, gfxOffset: {(int)(this.currentAnimFrame.gfxOffsetCorrection.X / this.currentAnimFrame.scale)},{(int)(this.currentAnimFrame.gfxOffsetCorrection.Y / this.currentAnimFrame.scale)}\n";

            description += $"shadowOriginFactor: X {this.currentAnimFrame.shadowOriginFactor.X} Y {this.currentAnimFrame.shadowOriginFactor.Y}\n";

            description += $"shadowPosOffset:  X {this.currentAnimFrame.shadowPosOffset.X} Y {this.currentAnimFrame.shadowPosOffset.Y} shadow height mult: {this.currentAnimFrame.shadowHeightMultiplier}\n";
            description += "\n";
            description += $"pos {(int)this.pos.X},{(int)this.pos.Y} rot: {Math.Round(this.rot, 2)} speed: 1/{this.playSpeed}\n";
            description += $"AnimPkg: {this.currentAnimPkg.name} {this.currentAnimPkgIndex}/{this.animPkgArray.Length}, layer: {this.currentAnimFrame.layer} animSize: {this.CurrentAnim.size} animName: {this.CurrentAnim.name}\ntexture: {this.currentAnimFrame.Texture.Name}\n";

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

        private void PrintFrameParams()
        {
            // for easy copying and pasting params into AnimData

            AnimFrame frame = this.currentAnimFrame;

            var textList = new List<string>();
            textList.Add($"\n\n{this.currentAnimPkg.name} {frame.atlasName}\n");

            if (frame.gfxOffsetCorrection != Vector2.Zero) textList.Add($", gfxOffsetCorrection: new Vector2({(int)(frame.gfxOffsetCorrection.X / frame.scale)}f, {(int)(frame.gfxOffsetCorrection.Y / frame.scale)}f)");

            if (frame.shadowOriginFactor != AnimFrame.defaultShadowOriginFactor) textList.Add($", shadowOriginFactor: new Vector2({ConvertValueToString(frame.shadowOriginFactor.X)}f, {ConvertValueToString(frame.shadowOriginFactor.Y)}f)");

            if (frame.shadowPosOffset != Vector2.Zero) textList.Add($", shadowPosOffset: new Vector2({ConvertValueToString(frame.shadowPosOffset.X)}f, {ConvertValueToString(frame.shadowPosOffset.Y)}f)");

            if (frame.shadowHeightMultiplier != 1f) textList.Add($", shadowHeightMultiplier: {ConvertValueToString(frame.shadowHeightMultiplier)}f");

            if (!frame.hasFlatShadow) textList.Add($", hasFlatShadow: {frame.hasFlatShadow.ToString().ToLower()}");

            MessageLog.Add(debugMessage: true, text: String.Join("", textList));
        }

        private static string ConvertValueToString(float value)
        {
            return value.ToString().Replace(",", ".");
        }

        private void DrawShadow(Color color, Vector2 lightPos, float shadowAngle, float drawOffsetX = 0f, float drawOffsetY = 0f, float yScaleForce = 0f)
        {
            if (!this.currentAnimFrame.castsShadow) return;

            // must match Sprite.DrawShadow logic

            float distance = Vector2.Distance(lightPos, this.pos);

            AnimFrame frame = this.currentAnimFrame;
            float opacity = 1f;

            if (this.currentAnimFrame.hasFlatShadow)
            {
                Vector2 diff = this.pos - lightPos;
                Vector2 limit = new(this.gfxRect.Width / 8, this.gfxRect.Height / 8);
                Vector2 offset = Vector2.Clamp(value1: diff / 6f, min: -limit, max: limit);
                Rectangle simulRect = this.gfxRect;
                simulRect.Offset(offset);

                this.currentAnimFrame.Draw(position: this.pos + offset, color: color * opacity, rotation: this.rot, opacity: 1.0f, submergeCorrection: 0);
            }
            else
            {
                float xScale = frame.scale;
                float yScale = frame.scale / distance * 100f;
                yScale = yScaleForce == 0 ? Math.Min(yScale, frame.scale * 3f) : frame.scale * yScaleForce;
                yScale *= frame.shadowHeightMultiplier;
                yScale = Math.Max(yScale, frame.scale * 0.8f);

                SonOfRobinGame.SpriteBatch.Draw(
                    frame.Texture,
                    position: this.pos + frame.shadowPosOffset + new Vector2(drawOffsetX, drawOffsetY),
                    sourceRectangle: frame.cropRect,
                    color: color * opacity,
                    rotation: shadowAngle + (float)(Math.PI / 2f),
                    origin: frame.shadowOrigin,
                    scale: new Vector2(xScale, yScale),
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }
    }
}