//  Goo Shader Effect
//  Based On Brandon Furtwangler's Parallax Effect
//  Visit his blog at: 
//  http://www.brandonfurtwangler.com/
//
/////////////////////////////////////////////////////

// Transformation Matrices.
float4x3 WorldView  : WORLDVIEW;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;

// Value of 0 to 6 that moves the goo around.
float fMixTime; 

// View space direction toward the light
float3 LightDir = {-0.6, 0.6, -0.6};

// Specular and Diffuse Constants
float Ks = .5;
float Kd  = 1.0;

// Level of Bump and Parallax Shading
float ParallaxAmount = 0.025;
float BumpAmount = 1.0;

// Color Texture
texture ColorTexture;
sampler ColorSampler = sampler_state
{ 
    Texture = (ColorTexture);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// Tangent Space of Normal Map of Color Texture
texture NormalsTexture;
sampler NormalsSampler = sampler_state
{ 
    Texture = (NormalsTexture);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// Height(Bump) Map of Color Texture
texture HeightTexture;
sampler HeightSampler= sampler_state
{ 
    Texture = (HeightTexture);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


void Parallax_VS(   float4 inPosition : POSITION,
                    float3 inNormal   : NORMAL,
                    float3 inTangent  : TANGENT,
                    float2 inTexCoord : TEXCOORD0,
                    out float4 outPosition  : POSITION,
                    out float2 outTexCoord  : TEXCOORD0,
                    out float3 outLightVec  : TEXCOORD1,
                    out float3 outViewVec   : TEXCOORD2,
                    out float3 outHalfAngle : TEXCOORD3)
{
    outPosition = mul(inPosition, WorldViewProjection);    
    outTexCoord = inTexCoord;
    float3 Pos = mul( inPosition, WorldView );
    float3 ViewDir = -normalize(Pos);
    float3 ViewSpaceNormal  = mul( inNormal, (float3x3)WorldView );
    float3 ViewSpaceTangent = mul( inTangent, (float3x3)WorldView );
    float3 Binorm = cross( ViewSpaceNormal, ViewSpaceTangent );
    float3x3 TangentMat = float3x3( ViewSpaceTangent, Binorm, ViewSpaceNormal );
    outLightVec = mul(TangentMat, LightDir);
    outViewVec  = mul(TangentMat, ViewDir);
    outHalfAngle = outLightVec + outViewVec;
}


void Bump_VS(   float4 inPosition : POSITION,
                float3 inNormal   : NORMAL,
                float3 inTangent  : TANGENT,
                float2 inTexCoord : TEXCOORD0,
                out float4 outPosition  : POSITION,
                out float2 outTexCoord  : TEXCOORD0,
                out float3 outLightVec  : TEXCOORD1,
                out float3 outViewVec   : TEXCOORD2,
                out float3 outHalfAngle : TEXCOORD3)
{
    outPosition = mul(inPosition, WorldViewProjection);
    outTexCoord = inTexCoord;    
    float3 Pos = mul( inPosition, WorldView );
    float3 ViewDir = -normalize(Pos);
    float3 ViewSpaceNormal  = mul( inNormal, (float3x3)WorldView );
    float3 ViewSpaceTangent = mul( inTangent, (float3x3)WorldView );
    float3 Binorm = cross( ViewSpaceNormal, ViewSpaceTangent );
    float3x3 TangentMat = float3x3( ViewSpaceTangent, Binorm, ViewSpaceNormal );
    outLightVec = mul(TangentMat, LightDir);
    outViewVec  = mul(TangentMat, ViewDir);
    outHalfAngle = normalize(outLightVec + outViewVec);
}


float2 ParallaxTexCoord(float2 oldcoord, sampler heightmap, float3 eye_vect, float parallax_amount)
{
   return (tex2D(heightmap, oldcoord) 
                     * parallax_amount - parallax_amount * 0.25)
                     * eye_vect + oldcoord;
}


float4 Parallax_PS( float4 inPosition  : POSITION,
                    float2 inTexCoord  : TEXCOORD0,
                    float3 inLightVec  : TEXCOORD1,
                    float3 inViewVec   : TEXCOORD2,
                    float3 inHalfAngle : TEXCOORD3) : COLOR
{
	inTexCoord.xy = inTexCoord.xy * 3.5;
    inTexCoord = ParallaxTexCoord(inTexCoord, HeightSampler, inViewVec, ParallaxAmount);
    inTexCoord.xy = inTexCoord.xy + sin(fMixTime);
    float4 Color = tex2D(ColorSampler, inTexCoord);  
    float3 N = tex2D(NormalsSampler, inTexCoord) * 2.0 - 1.0;
    float3 smooth = float3(0.0,0.0,1.0);
    N = lerp( smooth, N, BumpAmount );
    float3 NN = normalize(N);    
    float3 H = normalize(inHalfAngle);
    float4 DiffusePart  = Kd * dot( N, inLightVec );
    float4 SpecularPart = Ks * pow( dot(H, NN ), 64 );
    return Color * ( DiffusePart + SpecularPart );
}


float4 Bump_PS( float4 inPosition  : POSITION,
                float2 inTexCoord  : TEXCOORD0,
                float3 inLightVec  : TEXCOORD1,
                float3 inViewVec   : TEXCOORD2,
                float3 inHalfAngle : TEXCOORD3) : COLOR
{
    float4 Color = tex2D(ColorSampler, inTexCoord);    
    float3 N = tex2D(NormalsSampler, inTexCoord) * 2.0 - 1.0;    
    float3 H = inHalfAngle;
    float4 DiffusePart  = Kd * dot( N, inLightVec );
    float4 SpecularPart = Ks * pow( dot(H, N ), 64 );
    return Color * ( DiffusePart + SpecularPart );
}


technique Goo_Parallax_3_0
{
    pass p0
    {
        vertexshader = compile vs_2_0 Parallax_VS();
        pixelshader  = compile ps_3_0 Parallax_PS();
    }
}

technique Goo_Parallax_2_b
{
    pass p0
    {
        vertexshader = compile vs_2_0 Parallax_VS();
        pixelshader  = compile ps_2_b Parallax_PS();
    }
}

technique Goo_Parallax_2
{
    pass p0
    {
        vertexshader = compile vs_2_0 Parallax_VS();
        pixelshader  = compile ps_2_0 Parallax_PS();
    }
}

// Only does bumpmapping, no parallax reflection.
technique Goo_Bump_1_4
{
    pass p0
    {
        vertexshader = compile vs_1_1 Bump_VS();
        pixelshader  = compile ps_1_4 Bump_PS();
    }
}

