float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 Rotation;
float4 AmbientColor;
float4 LightColor;
float4 PointLight;
float4 ViewerPosition;

Texture xColorTexture;

//Texture sampling structure
sampler ColorTextureSampler = sampler_state
{ 
	texture = <xColorTexture>;
	magFilter = LINEAR;
	minFilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture bumpMapTexture;
float bumpLevel = 1.0;

//Bump map sampling structure
sampler BumpTextureSampler = sampler_state
{
	texture = <bumpMapTexture>;
	magFilter = LINEAR;
	minFilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
    float2 textureCoords: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 textureCoords: TEXCOORD0;
    float4 usablePosition: TEXCOORD1;
    float3 Normal : TEXCOORD2;
	float3 Tangent : TEXCOORD3;
	float3 Binormal : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	//Set the world and view vectors
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

	//Calculate the screen and texture coordinates
    output.Position = mul(viewPosition, Projection);
    output.textureCoords = input.textureCoords;
	
	//Use the world position and input normal
	output.usablePosition = worldPosition;
	output.Normal = normalize(mul(input.Normal,Rotation));
	output.Tangent = normalize(input.Tangent);
	output.Binormal = normalize(input.Binormal);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate the normal, including the information in the bump map
    float3 bump = bumpLevel * (tex2D(BumpTextureSampler, input.textureCoords) - (0.5, 0.5, 0.5));
    float3 bumpNormal = input.Normal + (bump.x * input.Tangent + bump.y * input.Binormal);
    bumpNormal = normalize(bumpNormal);

	//Load the texture into output
    float4 output = tex2D (ColorTextureSampler, input.textureCoords);

	//Light direction is the vertex position - light position
	float4 lightdir = PointLight - input.usablePosition;

	//Camera direction is the vertex position minus the camera position
	float4 camdir = ViewerPosition - input.usablePosition;

	//Normalize the vectors
	camdir = normalize(camdir);
	lightdir = normalize(lightdir);

	//Get the normal and calculate the diffuse lighting
	float3 norm = normalize(bumpNormal);
	float diffuse = dot (lightdir, norm);

	//Get the half angle and calculate the specular lighting
	float4 halfAngle = (lightdir + camdir)/2.0;
	float specular = dot (halfAngle,norm);

	//Clamp the specular between 0 and 1, then decrease it
	specular = saturate(specular);
	specular = pow(specular, 4.);

	//Color is the texture times ambient, plus texture times diffues,
	//plus light times specular
	return output*AmbientColor + output*diffuse + LightColor*specular;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
