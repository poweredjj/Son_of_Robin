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
float4 outlineColor;
bool drawFill;
float outlineThickness;
float4 drawColor;
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
               	
    float2 thicknessPix = outlineThickness * uvPix; // Calculate thickness in pixel coordinates
    
    float2 thickCRight = float2(thicknessPix.x + input.UV.x, input.UV.y);
    float2 thickCDown = float2(input.UV.x, thicknessPix.y + input.UV.y);
    float2 thickCLeft = float2(-thicknessPix.x + input.UV.x, input.UV.y);
    float2 thickCUp = float2(input.UV.x, -thicknessPix.y + input.UV.y);
    
    
    // bool testBool = thickCRight.x < cropXMin || thickCRight.x > cropXMax || thickCRight.y < cropYMin || thickCRight.y > cropYMax;

    
    bool thickPixelRightOn = tex2D(InputSampler, thickCRight).a >= threshold;
    bool thickPixelDownOn = tex2D(InputSampler, thickCDown).a >= threshold;
    bool thickPixelLeftOn = tex2D(InputSampler, thickCLeft).a >= threshold;
    bool thickPixelUpOn = tex2D(InputSampler, thickCUp).a >= threshold;
    
    float2 normalCRight = float2(uvPix.x + input.UV.x, input.UV.y);
    float2 normalCDown = float2(input.UV.x, uvPix.y + input.UV.y);
    float2 normalCLeft = float2((-1 * uvPix.x) + input.UV.x, input.UV.y);
    float2 normalCUp = float2(input.UV.x, (-1 * uvPix.y) + input.UV.y);
   
    bool normalPixelRightOn = tex2D(InputSampler, normalCRight).a >= threshold;
    bool normalPixelDownOn = tex2D(InputSampler, normalCDown).a >= threshold;
    bool normalPixelLeftOn = tex2D(InputSampler, normalCLeft).a >= threshold;
    bool normalPixelUpOn = tex2D(InputSampler, normalCUp).a >= threshold;
 
    bool isOutlinePixel = false;  
	
    if (currentPixelRaw.a > threshold && input.UV.x > uvPix.x && input.UV.y > uvPix.y)
    {
        // thick inside fill
        // checking non-transparent pixels for their non-transparent neighbours (and NOT first row / column)        
        
        if (outlineThickness > 1)
        {
            if (false
			    || !thickPixelRightOn
			    || !thickPixelDownOn
			    || !thickPixelLeftOn
			    || !thickPixelUpOn
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
			|| normalPixelRightOn
			|| normalPixelDownOn
			|| normalPixelLeftOn
			|| normalPixelUpOn
		)
        {
            isOutlinePixel = true;
        }
    }

    // return isOutlinePixel && !isOutsideCrop ? outlineColor * drawColor : (drawFill ? currentPixel : float4(0, 0, 0, 0)) * drawColor;
    return isOutlinePixel ? outlineColor * drawColor : (drawFill ? currentPixel : float4(0, 0, 0, 0)) * drawColor;
}

technique SpriteOutline
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
