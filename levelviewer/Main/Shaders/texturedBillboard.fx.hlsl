float4x4 ModelView;
float4x4 Projection;

struct VS_INPUT
{
    float4 position : POSITION;
    float2 texCoords: TEXCOORD0;
    float2 texCoords2: TEXCOORD1;
};

struct VS_OUTPUT
{
    float4 position : POSITION;
    float2 texCoords : TEXCOORD0;
};

VS_OUTPUT vs_main(VS_INPUT input)
{
	float4 position = mul(ModelView, input.position);
    position.x += input.texCoords.x;
    position.y += input.texCoords.y;
    
	VS_OUTPUT output;
	output.position = mul(Projection, position);
	output.texCoords = input.texCoords2;
	
	return output;
}

texture Diffuse;
sampler2D DiffuseSampler = sampler_state {
    Texture = <Diffuse>;
};


float4 ps_main(float2 texCoord : TEXCOORD0) : COLOR
{
	return tex2D(DiffuseSampler, texCoord);
}

technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}

