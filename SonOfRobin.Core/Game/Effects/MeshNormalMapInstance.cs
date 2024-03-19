﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly Texture2D normalMapTexture;
        private readonly Vector3 ambientColor;

        public MeshNormalMapInstance(MeshDefinition meshDef, Texture2D normalMapTexture, float ambientColorVal = 1f, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshNormalMap, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalMapTexture = normalMapTexture;
            this.ambientColor = new Color(ambientColorVal, ambientColorVal, ambientColorVal).ToVector3();
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["NormalTexture"].SetValue(this.normalMapTexture);
            this.effect.Parameters["ambientColor"].SetValue(this.ambientColor);

            Vector3 scale; Quaternion rot; Vector3 pos;
            SonOfRobinGame.BasicEffect.World.Decompose(out scale, out rot, out pos);

            this.effect.Parameters["worldScale"].SetValue(scale.X);

            World world = World.GetTopWorld();
            if (world != null && !world.demoMode) this.effect.Parameters["LightPos"].SetValue(world.Player.sprite.position);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}