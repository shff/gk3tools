float4 Viewport;

struct VS_INPUT
{
    float4 position : POSITION;
    float2 texCoords: TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 position : POSITION;
    float2 texCoords : TEXCOORD0;
};

VS_OUTPUT vs_main(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.position = float4(
        ((input.position.x - Viewport.z * 0.5) / Viewport.z) * 2.0,
        ((input.position.y - Viewport.w * 0.5) / Viewport.w) * -2.0,
        0.0, 1.0);
        
    output.texCoords = input.texCoords;
    
    return output;
}

struct PS_INPUT
{
    float2 texCoords : TEXCOORD0;
};

texture Diffuse;
sampler2D DiffuseSampler = sampler_state {
    Texture = <Diffuse>;
};

float4 Color;

float4 ps_main(PS_INPUT input) : COLOR0
{
    return tex2D(DiffuseSampler, input.texCoords) * Color;
}


technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}