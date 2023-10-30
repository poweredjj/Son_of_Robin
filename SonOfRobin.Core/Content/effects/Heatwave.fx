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
float heatPower;
float4 drawColor;
float2 baseTextureSize;
float2 distortionTextureSize;
float2 distortionTextureOffset;
float2 drawScale;
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
    
    float2 distortionInputOffset = distortionTextureOffset * drawScale + float2(0, (float) currentUpdate * distortionPixelSize.y * 0.45 * heatPower);
        
    float2 pixelPos = input.TextureCoordinates / basePixelSize * distortionPixelSize;
    float4 distortColor = tex2D(DistortTextureSampler, pixelPos + distortionInputOffset);
    
    // return (tex2D(BaseTextureSampler, input.TextureCoordinates) + distortColor) / 2 * drawColor; // for testing
          
    float2 distortionOffset = float2(distortColor.r - 0.5, distortColor.g - 0.5) * heatPower * 0.02f;       
    return tex2D(BaseTextureSampler, input.TextureCoordinates + distortionOffset) * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};