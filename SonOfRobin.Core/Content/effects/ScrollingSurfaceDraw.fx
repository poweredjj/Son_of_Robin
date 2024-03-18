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
float2 scrollingSurfaceOffset;
float2 baseTextureSize;
float globalDistortionPower;
float distortionFromOffsetPower;
float distortionSizeMultiplier;
float currentUpdate;
float distortionOverTimePower;
float distortionOverTimeDuration;
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
    
    float2 pixelSize = 1.0 / baseTextureSize;
    
    float2 timeDistortionOffset = (0.5 + (0.5 * sin(2 * 3.14159265359 * currentUpdate / (60.0 * distortionOverTimeDuration)))) * distortionOverTimePower * float2(1, 1);
    float2 movementDistortionOffset = (scrollingSurfaceOffset * pixelSize) * distortionFromOffsetPower;
      
    float4 distortColor = tex2D(DistortTextureSampler, input.TextureCoordinates + ((movementDistortionOffset + timeDistortionOffset) * distortionSizeMultiplier));
    float2 distortionOffset = float2(distortColor.r, distortColor.g) * globalDistortionPower * 0.2f;
       
    return tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset + (baseTextureOffset * pixelSize)) * input.Color * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};