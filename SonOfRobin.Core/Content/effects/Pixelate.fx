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
float2 effectSize;
float4 drawColor;

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

    // Calculate the grid size separately for X and Y
    float2 gridPos = floor(input.TextureCoords / (pixelSize * effectSize));

    // Calculate the center of the pixel in UV coordinates
    float2 uv = (gridPos + 0.5) * pixelSize * effectSize;

    // Sample the color from the original texture
    return tex2D(s0, uv) * drawColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
