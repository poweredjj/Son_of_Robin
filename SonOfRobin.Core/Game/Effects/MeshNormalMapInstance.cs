using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly Texture2D normalMapTexture;
        private readonly Vector4 ambientColor;
        private readonly float normalYAxisMultiplier;
        private readonly float lightPowerMultiplier;
        public Sprite[] lightSprites;

        public MeshNormalMapInstance(MeshDefinition meshDef, Texture2D normalMapTexture, float ambientColorVal = 1f, bool flippedNormalYAxis = false, float lightPowerMultiplier = 1f, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshNormalMap, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalMapTexture = normalMapTexture;
            this.ambientColor = new Vector4(1f, 1f, 1f, ambientColorVal);
            this.normalYAxisMultiplier = flippedNormalYAxis ? -1f : 1f; // some normal maps have their Y axis flipped and must be corrected
            this.lightPowerMultiplier = lightPowerMultiplier;
            this.lightSprites = [];
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

            int arraySize = 6;

            var lightPosArray = new Vector3[arraySize];
            var lightColorArray = new Vector3[arraySize];
            var lightRadiusArray = new float[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                Vector3 lightPos = Vector3.Zero;
                Color lightColor = Color.Transparent;
                float lightRadius = 0f;

                if (i <= this.lightSprites.Length - 1)
                {
                    Sprite lightSprite = this.lightSprites[i];

                    lightPos = new Vector3(lightSprite.position.X, lightSprite.position.Y, 0);
                    lightColor = lightSprite.lightEngine.Color * lightSprite.lightEngine.Opacity;
                    lightRadius = Math.Max(lightSprite.lightEngine.Width, lightSprite.lightEngine.Height) / 2;
                }

                lightPosArray[i] = lightPos;
                lightColorArray[i] = new Vector3(lightColor.R, lightColor.G, lightColor.B);
                lightRadiusArray[i] = lightRadius;
            }

            this.effect.Parameters["lightPosArray"].SetValue(lightPosArray);
            this.effect.Parameters["lightColorArray"].SetValue(lightColorArray);
            this.effect.Parameters["lightRadiusArray"].SetValue(lightRadiusArray);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}