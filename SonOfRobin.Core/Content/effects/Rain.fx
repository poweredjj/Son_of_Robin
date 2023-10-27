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
float globalDistortionPower;
float4 drawColor;
float2 baseTextureSize;
float2 distortionTextureSize;
float2 distortionTextureOffset;
float currentUpdate;

sampler s0: register(s0);
sampler s1: register(s1);

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
    float2 distortionPixelSize = 1.0 / distortionTextureSize;
    
    float2 distortionInputOffset = float2(0, currentUpdate * distortionPixelSize.y);
    float4 distortPixel = tex2D(DistortTextureSampler, input.TextureCoordinates + distortionInputOffset);
  
    float distortionPower = max(globalDistortionPower + distortPixel.r - 1.0, 0);  
    float distortionSizeMultiplier = 1.0;   
     
    float4 distortColor = tex2D(DistortTextureSampler, input.TextureCoordinates + (distortionPower * distortionSizeMultiplier));
    float2 distortionOffset = float2(distortColor.r, distortColor.g) * globalDistortionPower * 0.2f;
       
    return tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset) * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};