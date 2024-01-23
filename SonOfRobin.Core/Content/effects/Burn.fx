#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
float intensity;
float time;
float phaseModifier;
bool checkAlpha;
float4 drawColor;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// shaders use color value range 0.0f - 1.0f

	float4 originalColor = tex2D(s0, input.UV);
    if (checkAlpha && originalColor.a <= 0.4) return originalColor * drawColor;

	// Calculate burning effect
    float burnVal = sin((input.UV.y + time + phaseModifier) * 5) * 0.1;
	float3 burnColor = float3(1.0, 1.0, 0.0) * burnVal;

    float3 fireColor = float3(1.0, 0.0, 0.0) * (1.0 - input.UV.y);

	// Modify color calculation to add burning effect
	float4 finalColor;
	finalColor.rgb = ((originalColor.r + originalColor.g + originalColor.b) / 3.0) + burnColor + fireColor;
	finalColor.a = originalColor.a;

    return lerp(originalColor, finalColor, intensity) * drawColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};