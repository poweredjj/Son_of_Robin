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
float borderThickness;

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

	float threshold = 0.4f;
	if (currentPixel.a > threshold)
	{
		float2 uvPix = float2(1 / textureSize.x, 1 / textureSize.y);

		// Calculate thickness in pixel coordinates
		float2 thicknessPix = borderThickness * uvPix;

		// Calculate offsets for neighboring pixels
		float2 offsets[8] = {
			float2(thicknessPix.x, 0),
			float2(-thicknessPix.x, 0),
			float2(0, thicknessPix.y),
			float2(0, -thicknessPix.y),
			float2(thicknessPix.x, thicknessPix.y),
			float2(-thicknessPix.x, thicknessPix.y),
			float2(thicknessPix.x, -thicknessPix.y),
			float2(-thicknessPix.x, -thicknessPix.y)
		};

		// Check if any of the neighboring pixels exceed the threshold
		bool isOutlinePixel = false;
		for (int i = 0; i < 8; i++)
		{
			if (tex2D(InputSampler, input.UV + offsets[i]).a <= threshold)
			{
				isOutlinePixel = true;
				break;
			}
		}

		if (isOutlinePixel) return outlineColor;	
	}

    if (!drawFill) return float4(0, 0, 0, 0);
	else return currentPixel;
}

technique SpriteOutline
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
