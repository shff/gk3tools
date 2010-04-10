float4x4 ModelViewProjection;

struct VS_INPUT
{
    float4 position : POSITION;
};

struct VS_OUTPUT
{
    float4 position : POSITION;
};

VS_OUTPUT vs_main(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.position = mul(ModelViewProjection, input.position);
    //output.position = mul(input.position, ModelViewProjection);
    
    return output;
}


float4 ps_main() : COLOR0
{
    return float4(1.0, 1.0, 1.0, 1.0);
}



technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}
