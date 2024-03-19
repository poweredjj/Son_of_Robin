// Standard defines
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float3 LightPos;
float3 LightColor = 2;
float3 ambientColor;
float worldScale;

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 drawColor;
float effectPower;

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
    float4 PosLight : POSITION2;
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
    output.PosWorld = mul(input.Position, World); // handing over WorldSpace Coordinates to PS
    output.PosLight = mul(float4(LightPos, 1), worldScale);
  
    output.TexCoord = input.TexCoord;
    
    return output;    
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{    
    // input.PosWorld how has the Position of this Pixel in World Space
    float3 lightdir = normalize((input.PosWorld - input.PosLight)); // this is now the direction of light for this pixel
    
    float4 tex = tex2D(BaseTextureSampler, input.TexCoord);    
    float3 normal = normalize((2 * tex2D(NormalTextureSampler, input.TexCoord)) - 1); 
    float lightAmount = saturate(dot(normal.xyz, -lightdir));
    
    float minDistance = 250;
    float lightDistance = min((distance(input.PosWorld, input.PosLight) / worldScale), minDistance) / minDistance;
    float lightPowerFromDistance = 1 - lightDistance;
  
    tex.rgb *= ambientColor + (lightAmount * lightPowerFromDistance * LightColor);
  
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