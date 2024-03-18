// Standard defines
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 drawColor;
float2 baseTextureSize;
float effectPower;
float currentDraw;

Texture2D BaseTexture : register(t0);
Texture2D DistortTexture : register(t1);

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

// Required attributes of the input vertices
struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

// Semantics for output of vertex shader / input of pixel shader
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

// Actual shaders
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float2 basePixelSize = 1.0 / baseTextureSize;
               
    float distortVal1 = tex2D(DistortTextureSampler, (input.TexCoord + float2(0, frac(currentDraw / 1200))) * 4).r;
    float distortVal2 = (tex2D(DistortTextureSampler, input.TexCoord + float2(0, frac(currentDraw / 800))) * 5).g * effectPower;
        
    float2 baseSampleOffset1 = float2(distortVal1, distortVal1) * basePixelSize * 8;
    float2 baseSampleOffset2 = float2(distortVal2, distortVal2) * basePixelSize * 6;
    
    float4 newColor = tex2D(BaseTextureSampler, input.TexCoord + baseSampleOffset1 + baseSampleOffset2); 
    return newColor * drawColor;
}

// Technique and passes within the technique
technique ColorEffect
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}