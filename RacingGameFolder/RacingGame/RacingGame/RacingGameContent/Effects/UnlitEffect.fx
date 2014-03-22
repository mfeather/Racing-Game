float4x4 World;
float4x4 View;
float4x4 Projection;

Texture xColorTexture;

sampler ColorTextureSampler = sampler_state
{ texture = <xColorTexture>;
 magFilter = LINEAR; minFilter = LINEAR; mipfilter = LINEAR;
 AddressU = wrap; AddressV = wrap;};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 textureCoords: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 textureCoords: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.textureCoords = input.textureCoords;

	output.Color = (1,1,1,1);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
    float4 output = tex2D (ColorTextureSampler, input.textureCoords);

    return ((output * input.Color.x) + ((1.0 - output) * (1.0 - input.Color.x))) ;  

}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
