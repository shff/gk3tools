float4 Viewport;
float4x4 ModelViewProjection;
float LightmapMultiplier;

struct VS_INPUT
{
    float4 position : POSITION;
    float2 texCoords: TEXCOORD0;
    float2 lightmapCoords : TEXCOORD1;
};

struct VS_OUTPUT
{
    float4 position : POSITION;
    float2 texCoords : TEXCOORD0;
    float2 lightmapCoords : TEXCOORD1;
};

VS_OUTPUT vs_main(VS_INPUT input)
{
    VS_OUTPUT output;
    
    output.position = mul(ModelViewProjection, input.position);
    output.texCoords = input.texCoords;
    output.lightmapCoords = input.lightmapCoords;
    
    return output;
}

struct PS_INPUT
{
    float2 texCoords : TEXCOORD0;
    float2 lightmapCoords : TEXCOORD1;
};

texture Diffuse;
sampler2D DiffuseSampler = sampler_state {
    Texture = <Diffuse>;
};

texture Lightmap;
sampler2D LightmapSampler = sampler_state {
    Texture = <Lightmap>;
};

float4 ps_main(PS_INPUT input) : COLOR0
{
    float4 diffuse = tex2D(DiffuseSampler, input.texCoords);
    clip(diffuse.a - 0.5); 

    float4 lightmap = tex2D(LightmapSampler, input.lightmapCoords);
    
    //return lightmap;
    //return diffuse;
    return float4(diffuse.rgb * lightmap.rgb * LightmapMultiplier, diffuse.a);
    //return float4(1.0, 0, 0, 1.0);
}

technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}