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
    float2 gridPos = floor(input.TextureCoords / (pixelSize * blurSize));

    // Calculate the center of the pixel in UV coordinates
    float2 uv = (gridPos + 0.5) * pixelSize * blurSize;
        
    float2 bigPixelSize = pixelSize * blurSize;
    
    // Calculate the offsets for sampling the four corners
    float2 offsetLeftTop = float2(-bigPixelSize.x, -bigPixelSize.y) / 2;
    float2 offsetRightTop = float2(bigPixelSize.x, -bigPixelSize.y) / 2;
    float2 offsetLeftBottom = float2(-bigPixelSize.x, bigPixelSize.y) / 2;
    float2 offsetRightBottom = float2(bigPixelSize.x, bigPixelSize.y) / 2;
    
    // Sample the colors from the four corners
    float4 leftTop = tex2D(s0, uv + offsetLeftTop);
    float4 rightTop = tex2D(s0, uv + offsetRightTop);
    float4 leftBottom = tex2D(s0, uv + offsetLeftBottom);
    float4 rightBottom = tex2D(s0, uv + offsetRightBottom);
    
    // Calculate the position within the corner rectangle
    float2 cornerPos = frac(input.TextureCoords / (bigPixelSize));
    
    // Linearly interpolate between the four corners based on position
    float4 interpolatedColor = lerp(
        lerp(leftTop, rightTop, cornerPos.x),
        lerp(leftBottom, rightBottom, cornerPos.x),
        cornerPos.y
    );
    
    return interpolatedColor * drawColor;
}


technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
