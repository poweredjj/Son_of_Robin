using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshBasicInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;

        public MeshBasicInstance(MeshDefinition meshDef, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectMeshBasic, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.meshDef.texture);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}