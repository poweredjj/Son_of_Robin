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

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// shaders use color value range 0.0f - 1.0f

	float4 fireColor = float4(0.8, 0.0, 0.0, 1.0) * (1.0 - input.TextureCoordinates.y);

	float4 originalColor = tex2D(s0, input.TextureCoordinates);
	if (originalColor.a <= 0.5) return originalColor;

	// Calculate burning effect
	float burn = sin((input.TextureCoordinates.y + time) * 5) * 0.1;
	float3 burnColor = float3(1.0, 0.5, 0.0) * burn * (1.0 - input.TextureCoordinates.y);

	// Modify color calculation to add burning effect
	float4 gray;
	gray.rgb = (originalColor.r + originalColor.g + originalColor.b) / 3.0 + burnColor;
	gray.a = 1;

	return (originalColor * (1 - intensity)) + (intensity * (gray + fireColor));
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};