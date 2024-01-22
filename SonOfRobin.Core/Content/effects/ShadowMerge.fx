#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D ShadowTexture : register(t0);
Texture2D LightTexture : register(t1);

float4 drawColor;

sampler s0: register(s0);
sampler s1: register(s1);

sampler ShadowTextureSampler : register(s0)
{
    Texture = <ShadowTexture>;
};

sampler LightTextureSampler : register(s1)
{
    Texture = <LightTexture>;
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
                
    float4 shadowColor = tex2D(ShadowTextureSampler, input.TextureCoordinates);
    float4 lightColor = tex2D(LightTextureSampler, input.TextureCoordinates);      
         
    float4 mergedColor;
    mergedColor.rgb = 0;
    
    float shadowVal = (shadowColor.r + shadowColor.g + shadowColor.b) / 3;
    
    if (shadowVal < 0.01 && shadowColor.a > 0.2)
    {
        mergedColor.a = shadowVal;
    }
    else
    {   
        mergedColor.a = lightColor.a;
    }
    
    return mergedColor * drawColor;        
 }

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};