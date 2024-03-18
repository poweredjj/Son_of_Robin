using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshBasicInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;

        public MeshBasicInstance(MeshDefinition meshDef, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshBasic, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}