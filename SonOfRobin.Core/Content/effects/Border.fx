#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float2 textureSize : VPOS;
float4 outlineColor;
bool drawFill;

sampler2D InputSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// shaders use color value range 0.0f - 1.0f

	float4 currentPixel = tex2D(InputSampler, input.UV) * input.Color;
	float4 output = currentPixel;

	if (!drawFill) output = float4(0, 0, 0, 0);

	float threshold = 0.4f;

	if (currentPixel.a <= threshold)
	{
		float2 uvPix = float2(1 / textureSize.x, 1 / textureSize.y);

		if (false
			|| tex2D(InputSampler, float2((1 * uvPix.x) + input.UV.x, (0 * uvPix.y) + input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2((0 * uvPix.x) + input.UV.x, (1 * uvPix.y) + input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2((-1 * uvPix.x) + input.UV.x, (0 * uvPix.y) + input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2((0 * uvPix.x) + input.UV.x, (-1 * uvPix.y) + input.UV.y)).a > threshold
		)
		{
			output = outlineColor;
		}
	}

	return output;
}

technique SpriteOutline
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};