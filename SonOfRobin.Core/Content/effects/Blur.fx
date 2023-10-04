#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float2 textureSize : VPOS;
int blurX;
int blurY;

sampler2D InputSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// shaders use color value range 0.0f - 1.0f

    float2 uvPix = float2(1 / textureSize.x, 1 / textureSize.y);

    float4 currentPixel =
    tex2D(InputSampler, float2(input.UV.x - (uvPix.x * blurX), input.UV.y - (uvPix.y * blurY))) +
    tex2D(InputSampler, float2(input.UV.x, input.UV.y - (uvPix.y * blurY))) +
    tex2D(InputSampler, float2(input.UV.x + (uvPix.x * blurX), input.UV.y - (uvPix.y * blurY))) +
    
    tex2D(InputSampler, float2(input.UV.x - (uvPix.x * blurX), input.UV.y)) +
    tex2D(InputSampler, float2(input.UV.x, input.UV.y)) +
    tex2D(InputSampler, float2(input.UV.x + (uvPix.x * blurX), input.UV.y)) +
    
    tex2D(InputSampler, float2(input.UV.x - (uvPix.x * blurX), input.UV.y + (uvPix.y * blurY))) +
    tex2D(InputSampler, float2(input.UV.x, input.UV.y + (uvPix.y * blurY))) +
    tex2D(InputSampler, float2(input.UV.x + (uvPix.x * blurX), input.UV.y + (uvPix.y * blurY)));
      
    return currentPixel / 9;
}

technique SpriteOutline
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
