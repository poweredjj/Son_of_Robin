#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D BaseTexture : register(t0); // must match the texture used in SpriteBatch.Draw()
Texture2D DistortTexture : register(t1);
float distortPower;
float4 drawColor;
float2 baseTextureSize;
float2 distortionTextureOffset;
float2 drawScale;
float currentUpdate;

sampler s0 : register(s0);
sampler s1 : register(s1);

sampler BaseTextureSampler : register(s0)
{
    Texture = <BaseTexture>;
};

sampler DistortTextureSampler : register(s1)
{
    Texture = <DistortTexture>;
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
            
    float2 basePixelSize = 1.0 / baseTextureSize;
    float4 distortColor = tex2D(DistortTextureSampler, input.TextureCoordinates + distortionTextureOffset);
    float2 distortionOffset = float2(distortColor.r, distortColor.r) * basePixelSize * distortPower * 30;
    
    return tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset) * input.Color * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};