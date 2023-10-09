#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D BaseTexture : register(t0);
Texture2D DistortTexture : register(t1);
float4 drawColor;
float2 baseTextureOffset;
float2 baseTextureSize;
float2 baseTextureCorrection;
sampler s0: register(s0);
sampler s1: register(s1);
float distortionMultiplier;

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
    
    float2 pixelSize = 1.0 / baseTextureSize;
      
    float4 distortColor = tex2D(DistortTextureSampler, input.TextureCoordinates + (baseTextureOffset * pixelSize));
    float2 distortionOffset = float2(distortColor.r, distortColor.g) * distortionMultiplier * 0.2f;
    
    float2 baseOffset = frac(baseTextureOffset) * pixelSize;
       
    return tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset - baseTextureCorrection) * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};