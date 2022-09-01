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
float4 newColor;
float4 gray;

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

	if (color.a <= 0.5) return color;
	
	gray.rgb = (color.r + color.g + color.b) / 3.0f;
	return gray + (newColor * 0.8);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};