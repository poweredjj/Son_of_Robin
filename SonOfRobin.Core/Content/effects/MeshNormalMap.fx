// Standard defines
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_3 // slightly higher version, allowing for 512 max instructions
#define PS_SHADERMODEL ps_4_0_level_9_3 // slightly higher version, allowing for 512 max instructions
#endif

float3 ambientColor;
float worldScale;
float normalYAxisMultiplier;
float lightPowerMultiplier;

float3 lightPosArray[6];
float3 lightColorArray[6];
float lightRadiusArray[6];

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
    output.TexCoord = input.TexCoord;
    
    return output;    
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{       
    float4 tex = tex2D(BaseTextureSampler, input.TexCoord);
    float3 normal = normalize((2 * tex2D(NormalTextureSampler, input.TexCoord)) - 1);
    
    for (int i = 0; i < 6; i++)
    {
        float4 lightPos = mul(float4(lightPosArray[i], 1), worldScale);
        float3 lightColor = lightColorArray[i];
        float lightRadius = lightRadiusArray[i];
                
        float3 lightdir = normalize((input.PosWorld - lightPos)); // this is now the direction of light for this pixel
    
        normal.y *= normalYAxisMultiplier;
        float lightAmount = saturate(max(0, dot(normal, -lightdir)));
        
        float lightDistance = min((distance(input.PosWorld, lightPos) / worldScale), lightRadius) / lightRadius;
        
        float combinedLightPower = ambientColor * lightAmount * (1 - lightDistance) * lightPowerMultiplier;
        if (combinedLightPower > 0) tex.rgb *= combinedLightPower * lightColor;
    }
  
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