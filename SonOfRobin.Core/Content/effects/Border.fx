﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float2 textureSize : VPOS;
float4 outlineColor;
bool drawFill;
float outlineThickness;
float4 drawColor;

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
 
    bool isOutlinePixel = false;
	
    if (currentPixelRaw.a > threshold && input.UV.x > uvPix.x && input.UV.y > uvPix.y)
    {
        // thick inside fill
        // checking non-transparent pixels for their non-transparent neighbours (and NOT first row / column)
        
        if (outlineThickness > 1)
        {
          	// Calculate thickness in pixel coordinates
            float2 thicknessPix = outlineThickness * uvPix;
		
            if (false
			    || tex2D(InputSampler, float2(thicknessPix.x + input.UV.x, input.UV.y)).a <= threshold
			    || tex2D(InputSampler, float2(input.UV.x, thicknessPix.y + input.UV.y)).a <= threshold
			    || tex2D(InputSampler, float2((-1 * thicknessPix.x) + input.UV.x, input.UV.y)).a <= threshold
			    || tex2D(InputSampler, float2(input.UV.x, (-1 * thicknessPix.y) + input.UV.y)).a <= threshold
		    )
            {
                isOutlinePixel = true;
            }            
        }
    }
    else
    {	
        // thin outline fill
        // checking transparent pixels for their non-transparent neighbours (and ALWAYS first row / column, regardless of transparency)  
      
        if (false
			|| tex2D(InputSampler, float2(uvPix.x + input.UV.x, input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2(input.UV.x, uvPix.y + input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2((-1 * uvPix.x) + input.UV.x, input.UV.y)).a > threshold
			|| tex2D(InputSampler, float2(input.UV.x, (-1 * uvPix.y) + input.UV.y)).a > threshold
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
