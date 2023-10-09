#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D WaterTexture : register(t0);
Texture2D DistortTexture : register(t1);
float4 drawColor;
sampler s0: register(s0);
sampler s1: register(s1);
float2 baseOffset;

sampler WaterTextureSampler : register(s0)
{
    Texture = <WaterTexture>;
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
    
    float4 distortColor = tex2D(DistortTextureSampler, input.TextureCoordinates);
      
    //float4 textureColor = tex2D(WaterTextureSampler, input.TextureCoordinates); // for testing
    //return ((textureColor * 0.3) + (distortColor * 0.7)) * drawColor; // for testing
               
    float2 distortionOffset = distortColor.g * float2(0.2, 0.2);
    float2 readCoords = input.TextureCoordinates + (baseOffset * 3) + distortionOffset;
       
    return (tex2D(WaterTextureSampler, readCoords) / 2) * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};