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
float4 newColor;

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
	float4 color = tex2D(s0, input.TextureCoordinates);

	float gray = (color.r + color.g + color.b) / 2;

	if (color.a < 0.5) return bgColor;

	newColor.r = bgColor.r * (gray * fgColor.r) * (color.r * 0.7);
	newColor.g = bgColor.g * (gray * fgColor.r) * (color.g * 0.7);
	newColor.b = bgColor.b * (gray * fgColor.r) * (color.b * 0.7);

	return newColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};