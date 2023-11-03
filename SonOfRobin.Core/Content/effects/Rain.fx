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
float rainPower;
float4 drawColor;
float2 baseTextureSize;
float2 distortionTextureSize;
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
    
    int maxIterations = 3;
    float2 distortionOffset = float2(0, 0);
    for (int i = 1; i <= maxIterations; i++)
    {
        float2 distortionInputOffset = float2((i - 1) * 0.33, (float)currentUpdate * -distortionPixelSize.y * (0.03 + (i * 0.4)));
        float distortionSizeMultiplier = 1 + (i * 0.45);    
    
        float2 pixelPos = input.TextureCoordinates / basePixelSize * distortionPixelSize;
        float4 distortColor = tex2D(DistortTextureSampler, frac((pixelPos + distortionInputOffset) / distortionSizeMultiplier));
  
        float distortionPower = max(distortColor.r + rainPower - 1.0, 0) * ((i + 1) / maxIterations * rainPower);
        
        distortionOffset = distortionOffset + float2(distortColor.r, distortColor.r) * distortionPower * 0.1f;
    }
 
    float4 outputColor = tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset);
    
    if (abs(distortionOffset.x) > basePixelSize.x || abs(distortionOffset.y) > basePixelSize.y)
    {
        float4 reflectionColor = tex2D(BaseTextureSampler, float2(1, 1) - frac((input.TextureCoordinates + (distortionOffset * 0.35)) * float2(4.5, 2)));
        outputColor = (outputColor * 0.5) + (reflectionColor * 0.5);
    }
    
    return outputColor * drawColor;     
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};