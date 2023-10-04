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
        
    return
    (tex2D(InputSampler, input.UV + (float2(-blurX, -blurY) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(0, -blurY) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(blurX, -blurY) * uvPix)) +
    
    tex2D(InputSampler, input.UV + (float2(-blurX, 0) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(0, 0) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(blurX, 0) * uvPix)) +
    
    tex2D(InputSampler, input.UV + (float2(-blurX, blurY) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(0, blurY) * uvPix)) +
    tex2D(InputSampler, input.UV + (float2(blurX, blurY) * uvPix))
    ) / 9;
}

technique SpriteOutline
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
