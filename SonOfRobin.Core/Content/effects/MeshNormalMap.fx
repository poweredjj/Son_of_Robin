// Standard defines
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float3 LightPosition;
float3 LightColor = 1.0;
float3 AmbientColor = 0.5;

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 drawColor;
float effectPower;
float currentDraw;

Texture2D BaseTexture : register(t0);
Texture2D NormalTexture : register(t1);

sampler s0 : register(s0);
sampler s1 : register(s1);

sampler BaseTextureSampler : register(s0)
{
    Texture = <BaseTexture>;
};

sampler NormalTextureSampler : register(s1)
{
    Texture = <NormalTexture>;
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
    float4 PosWorld : POSITION1;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

// Actual shaders
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
    float4 viewPosition = mul(worldPosition, View);

    output.PosWorld = viewPosition; // handing over WorldSpace Coordinates to PS
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{    
    //float4 baseColor = tex2D(BaseTextureSampler, input.TexCoord);
    //float4 normalColor = 2 * NormalTexture.Sample(NormalTextureSampler, input.TexCoord) - 1;      
        
    //float lightAmount = saturate(dot(normalColor.xyz, LightDirection));
    //baseColor.rgb *= AmbientColor + (lightAmount * LightColor);
    
    //return baseColor * drawColor;
      
    
    // input.PosWorld how has the Position of this Pixel in World Space
    float3 lightdir = normalize(input.PosWorld - LightPosition); // this is now the direction of light for this pixel

    // Look up the texture value
    
    float4 tex = tex2D(BaseTextureSampler, input.TexCoord);
    
    // Look up the normalmap value
    
       
    float3 normal = normalize((2 * tex2D(NormalTextureSampler, input.TexCoord)) - 1);

    // Compute lighting
    float lightAmount = saturate(dot(normal, -lightdir));
    tex.rgb *= AmbientColor + (lightAmount * LightColor);
    
    return tex * drawColor;
    
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