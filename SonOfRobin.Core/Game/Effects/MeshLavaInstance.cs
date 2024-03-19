using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshLavaInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;

        public MeshLavaInstance(MeshDefinition meshDef, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshLava, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.distortTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingLavaDistortion);
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["effectPower"].SetValue(this.meshDef.tweenEffectPower);

            float distortPower = (float)(SonOfRobinGame.CurrentDraw % 600) / 600;
            this.effect.Parameters["distortTextureOffset"].SetValue(new Vector2(distortPower)); // values from 0 to 1

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}