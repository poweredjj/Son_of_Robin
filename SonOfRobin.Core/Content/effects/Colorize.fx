﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
float4 colorizeColor;
float opacity;
bool checkAlpha;
float4 drawColor;
float minAlpha;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// shaders use color value range 0.0f - 1.0f

	float4 originalColor = tex2D(s0, input.TextureCoordinates);

    if (checkAlpha && originalColor.a <= minAlpha)
        return originalColor * drawColor;
	
	float4 gray;
	gray.rgb = (originalColor.r + originalColor.g + originalColor.b) / 3.0;
	gray.a = 1;

	float4 newColor = gray + (colorizeColor * 0.8);
    return ((newColor * opacity) + (originalColor * (1 - opacity))) * drawColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};