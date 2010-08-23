float4 Viewport;
float4x4 ModelViewProjection;

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
    
    output.position = mul(ModelViewProjection, input.position);
    output.texCoords = input.texCoords;
    
    return output;
}

struct PS_INPUT
{
    float2 texCoords : TEXCOORD0;
};

float4 Color;
texture Diffuse;
sampler2D DiffuseSampler = sampler_state {
    Texture = <Diffuse>;
};


float4 ps_main(PS_INPUT input) : COLOR0
{
    float4 color = tex2D(DiffuseSampler, input.texCoords) * Color;
    clip(color.a - 0.5);

    return color;
}

technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}