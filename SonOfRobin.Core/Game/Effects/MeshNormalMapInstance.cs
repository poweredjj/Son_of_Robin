using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly Texture2D normalMapTexture;

        public MeshNormalMapInstance(MeshDefinition meshDef, Texture2D normalMapTexture, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshNormalMap, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalMapTexture = normalMapTexture;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["NormalTexture"].SetValue(this.normalMapTexture);
            this.effect.Parameters["currentDraw"].SetValue(SonOfRobinGame.CurrentDraw);

            World world = World.GetTopWorld();
            if (world != null && !world.demoMode)
            {
                Vector2 lightOffset = world.camera.CurrentPos - new Vector2((float)world.camera.viewRect.Width / 2f, (float)world.camera.viewRect.Height / 2f);
                this.effect.Parameters["LightPos"].SetValue(world.Player.sprite.position);
            }

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}