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
float4 fgColor;
float4 bgColor;

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

	float4 color = tex2D(s0, input.TextureCoordinates);

	if (color.a < 0.5f)
	{
		color.a = 0.0f;
		return color;
	}

	float gray = (color.r + color.g + color.b) / 3.0f;
	float4 tonedGray;
	tonedGray.rgb = (gray * fgColor.rgb);

	float4 newColor;
	newColor = bgColor * (tonedGray * 2.7f) + (color * 0.23f);
	newColor.a = 1.0f;

	return newColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};