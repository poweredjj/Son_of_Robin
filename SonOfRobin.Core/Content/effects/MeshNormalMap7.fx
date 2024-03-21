// Standard defines
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_3 // slightly higher version, allowing for 512 max instructions
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 ambientColor;
float worldScale;
float lightPowerMultiplier;

float3 sunPos;
float sunPower;
float sunYAxisCenterFactor;

float3 lightPosArray[7]; // line changed between 2-7 light versions
float4 lightColorArray[7]; // line changed between 2-7 light versions
float lightRadiusArray[7]; // line changed between 2-7 light versions
int noOfLights;

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
    float4 PosWorld : TEXCOORD1; // "TEXCOORD1" name is needed for Android, will crash otherwise
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
    output.PosWorld = mul(input.Position, World) / worldScale; // handing over WorldSpace Coordinates to PS
    output.TexCoord = input.TexCoord;
        
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 baseColor = tex2D(BaseTextureSampler, input.TexCoord);
    float3 normal = normalize((2 * tex2D(NormalTextureSampler, input.TexCoord)) - 1);
    
    float3 sunPosCalculated = sunPos;
    sunPosCalculated.y = lerp(sunPos.y, input.PosWorld.y - 500, sunYAxisCenterFactor);
    float sunlightAmount = saturate(max(0, dot(normal, -normalize((input.PosWorld - sunPosCalculated)))));
    float4 sunlight = baseColor * sunPower * sunlightAmount;
        
    float4 sumOfLights = float4(0, 0, 0, 0);
   
    for (int i = 0; i < 7; i++) // line changed between 2-7 light versions
    {                         
        float lightDistancePower = max(1 - (min((distance(input.PosWorld, lightPosArray[i])), lightRadiusArray[i]) / lightRadiusArray[i]), 0);
        float lightAmount = saturate(max(0, dot(normal, -normalize((input.PosWorld - lightPosArray[i])))));
        sumOfLights.rgb += baseColor * lightColorArray[i] * lightAmount * lightDistancePower;
        
        if (i + 1 == noOfLights) break;
    }
  
    return (baseColor * ambientColor) + sunlight + (sumOfLights * lightPowerMultiplier);
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