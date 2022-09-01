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
float gray;
float4 newColor;
float4 tonedGray;

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
	float gray = (color.r + color.g + color.b) / 3.0f;

	if (color.a < 0.5 || gray > 0.8) return bgColor;

	tonedGray.rgb = (gray * fgColor.rgb);


	newColor = bgColor * (tonedGray * 3.0f) + (color * 0.2f);
	newColor.a = 255;

	return newColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};