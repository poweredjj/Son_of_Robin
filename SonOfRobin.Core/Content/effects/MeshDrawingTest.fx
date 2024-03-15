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

Texture2D BaseTexture;
sampler s0 : register(s0);

sampler BaseTextureSampler : register(s0)
{
    Texture = <BaseTexture>;
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
    float2 TextureCoordinates : TEXCOORD0;
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
    float4 originalColor = tex2D(BaseTextureSampler, input.TextureCoordinates); 
    return originalColor * drawColor;
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