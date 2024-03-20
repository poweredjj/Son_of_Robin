using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly TextureBank.TextureName normalTextureName;
        private readonly Vector4 ambientColor;
        private readonly float normalYAxisMultiplier;
        private readonly float lightPowerMultiplier;
        private readonly float sunPowerMultiplier;

        public MeshNormalMapInstance(MeshDefinition meshDef, TextureBank.TextureName normalTextureName, float ambientColorVal = 1f, bool flippedNormalYAxis = false, float lightPowerMultiplier = 1f, float sunPowerMultiplier = 70f, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshNormalMap, framesLeft: framesLeft, priority: priority)
        {
            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalTextureName = normalTextureName; // normal texture should only be loaded when needed (to avoid loading when low terrain detail is set)
            this.ambientColor = new Vector4(1f, 1f, 1f, ambientColorVal);
            this.normalYAxisMultiplier = flippedNormalYAxis ? -1f : 1f; // some normal maps have their Y axis flipped and must be corrected
            this.lightPowerMultiplier = lightPowerMultiplier;
            this.sunPowerMultiplier = sunPowerMultiplier * (SonOfRobinGame.platform == Platform.Mobile ? 0.1f : 1f);
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyFirstPass = true)
        {
            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["NormalTexture"].SetValue(TextureBank.GetTexture(this.normalTextureName));
            this.effect.Parameters["ambientColor"].SetValue(this.ambientColor);
            this.effect.Parameters["lightPowerMultiplier"].SetValue(this.lightPowerMultiplier);
            this.effect.Parameters["normalYAxisMultiplier"].SetValue(this.normalYAxisMultiplier);

            Vector3 scale; Quaternion rot; Vector3 pos;
            SonOfRobinGame.BasicEffect.World.Decompose(out scale, out rot, out pos);
            this.effect.Parameters["worldScale"].SetValue(scale.X);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }

        public void SetLights(LightData[] lightDataArray, Vector3 normalizedSunPos, AmbientLight.SunLightData sunLightData)
        {
            int maxLightCount = 6;

            int arraySize = Math.Min(lightDataArray.Length, maxLightCount);

            if (lightDataArray.Length > maxLightCount) MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} lightDataArray size: {lightDataArray.Length}");

            var lightPosArray = new Vector3[arraySize];
            var lightColorArray = new Vector4[arraySize];
            var lightRadiusArray = new float[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                lightPosArray[i] = lightDataArray[i].pos;
                lightColorArray[i] = lightDataArray[i].color;
                lightRadiusArray[i] = lightDataArray[i].radius;
            }

            this.effect.Parameters["sunPos"].SetValue(normalizedSunPos);
            this.effect.Parameters["sunPower"].SetValue(sunLightData.sunShadowsOpacity * this.sunPowerMultiplier);
            this.effect.Parameters["sunYAxisCenterFactor"].SetValue(1f - ((sunLightData.sunShadowsLength - 1f) / 2f));
            this.effect.Parameters["lightPosArray"].SetValue(lightPosArray);
            this.effect.Parameters["lightColorArray"].SetValue(lightColorArray);
            this.effect.Parameters["lightRadiusArray"].SetValue(lightRadiusArray);
            this.effect.Parameters["noOfLights"].SetValue(arraySize);
        }
    }

    public readonly struct LightData
    {
        public readonly Vector3 pos;
        public readonly Vector4 color;
        public readonly float radius;
        public readonly Rectangle rect;

        public LightData(Sprite lightSprite)
        {
            this.pos = new Vector3(lightSprite.position.X, lightSprite.position.Y, 0);
            Color color = lightSprite.lightEngine.Color * lightSprite.lightEngine.Opacity;
            this.color = new Vector4(color.R, color.G, color.B, color.A);
            this.radius = Math.Max(lightSprite.lightEngine.Width, lightSprite.lightEngine.Height) / 2;

            this.rect = new Rectangle(
                x: (int)(lightSprite.position.X - (radius / 2)),
                y: (int)(lightSprite.position.Y - (radius / 2)),
                width: (int)radius,
                height: (int)radius);
        }
    }
}