using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly Texture2D normalMapTexture;
        private readonly Vector3 ambientColor;
        private readonly float normalYAxisMultiplier;
        private readonly float lightPowerMultiplier;

        public MeshNormalMapInstance(MeshDefinition meshDef, Texture2D normalMapTexture, float ambientColorVal = 1f, bool flippedNormalYAxis = false, float lightPowerMultiplier = 1f, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshNormalMap, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalMapTexture = normalMapTexture;
            this.ambientColor = new Color(ambientColorVal, ambientColorVal, ambientColorVal).ToVector3();
            this.normalYAxisMultiplier = flippedNormalYAxis ? -1f : 1f; // some normal maps have their Y axis flipped and must be corrected
            this.lightPowerMultiplier = lightPowerMultiplier;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyFirstPass = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["NormalTexture"].SetValue(this.normalMapTexture);
            this.effect.Parameters["ambientColor"].SetValue(this.ambientColor);
            this.effect.Parameters["normalYAxisMultiplier"].SetValue(this.normalYAxisMultiplier);
            this.effect.Parameters["lightPowerMultiplier"].SetValue(this.lightPowerMultiplier);

            Vector3 scale; Quaternion rot; Vector3 pos;
            SonOfRobinGame.BasicEffect.World.Decompose(out scale, out rot, out pos);

            this.effect.Parameters["worldScale"].SetValue(scale.X);

            World world = World.GetTopWorld();
            if (world != null && !world.demoMode) this.effect.Parameters["LightPos"].SetValue(world.Player.sprite.position);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}