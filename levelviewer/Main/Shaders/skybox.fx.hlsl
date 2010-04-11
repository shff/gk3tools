float4x4 ModelViewProjection;

texture DiffuseTexture;
samplerCUBE Diffuse = sampler_state {
    Texture = <DiffuseTexture>;
};

struct VS_INPUT
{
    float4 position : POSITION;
};

struct VS_OUTPUT
{
    float4 position : POSITION;
    float3 texCoords : TEXCOORD0;
};

VS_OUTPUT vs_main(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.position = mul(ModelViewProjection, input.position);
    output.texCoords = input.position.xyz;
    
    return output;
}

struct PS_INPUT
{
    float3 texCoords : TEXCOORD0;
};

float4 ps_main(PS_INPUT input) : COLOR0
{
    //return float4(1.0, input.texCoords.x * 0.001, input.texCoords.y * 0.001, 1.0);
    return texCUBE(Diffuse, input.texCoords.xyz * 0.001) + float4(0, 0, 0, 1.0);
}


technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}
