﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class MeshNormalMapInstance : EffInstance
    {
        private readonly MeshDefinition meshDef;
        private readonly Texture2D baseTexture;
        private readonly TextureBank.TextureName normalTextureName;
        private readonly float lightPowerMultiplier;
        private readonly float sunPowerMultiplier;

        public MeshNormalMapInstance(MeshDefinition meshDef, TextureBank.TextureName normalTextureName, float lightPowerMultiplier = 0.16f, float sunPowerMultiplier = 14f, int framesLeft = 1, int priority = 1) :
            base(effect: SonOfRobinGame.EffectMeshBasic, framesLeft: framesLeft, priority: priority)
        {
            // EffectMeshBasic is the default one, used only when there is no light at all

            this.meshDef = meshDef;
            this.baseTexture = this.meshDef.texture;
            this.normalTextureName = normalTextureName; // normal texture should only be loaded when needed (to avoid loading when low terrain detail is set)
            this.lightPowerMultiplier = lightPowerMultiplier;
            this.sunPowerMultiplier = sunPowerMultiplier * (SonOfRobinGame.platform == Platform.Mobile ? 0.1f : 1f);
        }

        public void TurnOnAlternative(Vector3 normalizedSunPos, AmbientLight.SunLightData sunLightData, LightData[] lightDataArray)
        {
            // use when there is sun or point lights

            int maxLightCount = SonOfRobinGame.platform == Platform.Mobile ? 7 : 32;

            int arraySize = Math.Min(lightDataArray.Length, maxLightCount);

            var lightPosArray = new Vector3[arraySize];
            var lightColorArray = new Vector4[arraySize];
            var lightRadiusArray = new float[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                lightPosArray[i] = lightDataArray[i].pos;
                lightColorArray[i] = lightDataArray[i].color;
                lightRadiusArray[i] = lightDataArray[i].radius;
            }

            Effect effInstance;

            if (arraySize == 0) effInstance = SonOfRobinGame.EffectMeshNormalMap0;
            else if (arraySize <= 2) effInstance = SonOfRobinGame.EffectMeshNormalMap2;
            else if (arraySize <= 7) effInstance = SonOfRobinGame.EffectMeshNormalMap7;
            else effInstance = SonOfRobinGame.EffectMeshNormalMap32;

            if (arraySize > 0)
            {
                effInstance.Parameters["lightPosArray"].SetValue(lightPosArray);
                effInstance.Parameters["lightColorArray"].SetValue(lightColorArray);
                effInstance.Parameters["lightRadiusArray"].SetValue(lightRadiusArray);
                effInstance.Parameters["noOfLights"].SetValue(arraySize);
                effInstance.Parameters["lightPowerMultiplier"].SetValue(this.lightPowerMultiplier);
            }

            effInstance.Parameters["sunPos"].SetValue(normalizedSunPos);
            effInstance.Parameters["sunPower"].SetValue(sunLightData.sunShadowsOpacity * this.sunPowerMultiplier);
            effInstance.Parameters["sunYAxisCenterFactor"].SetValue(1f - ((sunLightData.sunShadowsLength - 1f) / 2f));

            effInstance.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            effInstance.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            effInstance.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            effInstance.Parameters["BaseTexture"].SetValue(this.baseTexture);
            effInstance.Parameters["NormalTexture"].SetValue(TextureBank.GetTexture(this.normalTextureName));
            SonOfRobinGame.BasicEffect.World.Decompose(out Vector3 scale, out _, out _);
            effInstance.Parameters["worldScale"].SetValue(scale.X);

            effInstance.CurrentTechnique.Passes[0].Apply();
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyFirstPass = true)
        {
            // use only when there is no sun and no point lights are present

            this.effect.Parameters["Projection"].SetValue(SonOfRobinGame.BasicEffect.Projection);
            this.effect.Parameters["World"].SetValue(SonOfRobinGame.BasicEffect.World);
            this.effect.Parameters["View"].SetValue(SonOfRobinGame.BasicEffect.View);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: Color.White);
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

            int rectSize = Math.Max(lightSprite.lightEngine.Width, lightSprite.lightEngine.Height);
            this.radius = rectSize / 2;

            this.rect = new Rectangle(
                x: (int)(lightSprite.position.X - (rectSize / 2)),
                y: (int)(lightSprite.position.Y - (rectSize / 2)),
                width: (int)rectSize,
                height: (int)rectSize);
        }
    }
}