using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshSwampInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Vector2 baseTextureSize;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;

        public MeshSwampInstance(MeshDefinition meshDef, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshSwamp, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
            this.distortTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor);
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyFirstPass = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["effectPower"].SetValue(this.meshDef.tweenEffectPower);
            this.effect.Parameters["currentDraw"].SetValue((float)SonOfRobinGame.CurrentDraw);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}