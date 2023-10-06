#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
float2 textureSize : VPOS;
float2 blurSize;

sampler2D InputSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoords : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // shaders use color value range 0.0f - 1.0f
        
    float2 pixelSize = 1.0 / textureSize;

    return
    (tex2D(s0, input.TextureCoords + (float2(-blurSize.x, -blurSize.y) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(0, -blurSize.y) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(blurSize.x, -blurSize.y) * pixelSize)) +
    
    tex2D(s0, input.TextureCoords + (float2(-blurSize.x, 0) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(0, 0) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(blurSize.x, 0) * pixelSize)) +
    
    tex2D(s0, input.TextureCoords + (float2(-blurSize.x, blurSize.y) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(0, blurSize.y) * pixelSize)) +
    tex2D(s0, input.TextureCoords + (float2(blurSize.x, blurSize.y) * pixelSize))
    ) / 9;
}


technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
