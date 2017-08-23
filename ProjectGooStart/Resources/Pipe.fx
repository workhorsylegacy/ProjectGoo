float4 diffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};

float4x4 WorldViewProj : WORLDVIEWPROJECTION;
float4x4 WorldMatrix : WORLD;
float4 DiffuseDirection;
float xMultiply;
float yMultiply;

Texture meshTexture;
sampler TextureSampler = sampler_state { texture = <meshTexture>; mipfilter = LINEAR; };

struct VS_OUTPUT
{
    float4 Pos : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Light : TEXCOORD1;
    float3 Normal : TEXCOORD2;
};

VS_OUTPUT Transform( float4 inputPosition : POSITION,
					 float3 inputNormal : NORMAL,
					 float2 inputTexCoord : TEXCOORD0)
{
    
    VS_OUTPUT Out = (VS_OUTPUT)0;
    Out.Pos = mul(inputPosition, WorldViewProj);
    Out.TexCoord = inputTexCoord;    
    Out.TexCoord.x = inputTexCoord.x * xMultiply;    
    Out.TexCoord.y = inputTexCoord.y * yMultiply;   
    Out.Light = DiffuseDirection;
    Out.Normal = normalize(mul(inputNormal, WorldMatrix));
    return Out;
}

float4 TextureColor( float2 textureCoords : TEXCOORD0,
					 float3 lightDirection : TEXCOORD1,
					 float3 normal : TEXCOORD2) : COLOR0
{    
    float4 textureColor = tex2D(TextureSampler, textureCoords); 
    return textureColor * (diffuseColor * saturate(dot(lightDirection, normal)));
};

technique PipeLight_1_1
{
    pass P0
    {
        VertexShader = compile vs_1_1 Transform();
        PixelShader  = compile ps_1_1 TextureColor();
    }
}
