#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_3 // slightly higher version, allowing for 512 max instructions
#define PS_SHADERMODEL ps_4_0_level_9_3 // slightly higher version, allowing for 512 max instructions
#endif

Texture2D SpriteTexture;
float4 outlineColor;
bool drawFill;
float outlineThickness;
float4 drawColor;
float2 textureSize;
float cropXMin;
float cropXMax;
float cropYMin;
float cropYMax;

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

    float4 currentPixelRaw = tex2D(InputSampler, input.UV); // without color, needed for measuring alpha correctly
    float4 currentPixel = currentPixelRaw * input.Color;
    float2 uvPix = float2(1 / textureSize.x, 1 / textureSize.y);
    float threshold = 0.4f;
    
    //bool xInRange = input.UV.x >= cropXMin && input.UV.x <= cropXMax;
    //bool yInRange = input.UV.y >= cropYMin && input.UV.y <= cropYMax;
    
    bool leftEdge = input.UV.x <= cropXMin;
    bool rightEdge = input.UV.x >= cropXMax;
    bool topEdge = input.UV.y <= cropYMin;
    bool bottomEdge = input.UV.y >= cropYMax;
               	
    bool isOutlinePixel = false;  
	
    if (currentPixelRaw.a > threshold)
    {
        // thick inside fill
        // checking non-transparent pixels for their transparent neighbours       
        
        if (outlineThickness > 1)
        {
            float2 thicknessPix = outlineThickness * uvPix; // Calculate thickness in pixel coordinates
            
            float2 cLeft = float2(input.UV.x - thicknessPix.x, input.UV.y);
            float2 cRight = float2(input.UV.x + thicknessPix.x, input.UV.y);
            float2 cTop = float2(input.UV.x, input.UV.y - thicknessPix.y);
            float2 cBottom = float2(input.UV.x, input.UV.y + thicknessPix.y);
                        
            if ((!cLeft.x >= cropXMin && tex2D(InputSampler, cLeft).a <= threshold) ||
                (!cRight.x <= cropXMax && tex2D(InputSampler, cRight).a <= threshold) ||
                (!cTop.x >= cropYMin && tex2D(InputSampler, cTop).a <= threshold) ||
                (!cBottom.x <= cropYMax && tex2D(InputSampler, cBottom).a <= threshold)
		    )
            {
                isOutlinePixel = true;
            }       
        }
    }
    else
    {
        // thin outline fill
        // checking transparent pixels for their non-transparent neighbours
                      
        if ((!rightEdge && tex2D(InputSampler, float2(uvPix.x + input.UV.x, input.UV.y)).a > threshold) ||
			(!bottomEdge && tex2D(InputSampler, float2(input.UV.x, uvPix.y + input.UV.y)).a > threshold) ||
			(!leftEdge && tex2D(InputSampler, float2(-uvPix.x + input.UV.x, input.UV.y)).a > threshold) ||
			(!topEdge && tex2D(InputSampler, float2(input.UV.x, -uvPix.y + input.UV.y)).a > threshold)
		)
        {
            isOutlinePixel = true;
        }
    }

    return isOutlinePixel ? outlineColor * drawColor : (drawFill ? currentPixel : float4(0, 0, 0, 0)) * drawColor;
}

technique SpriteOutline
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
