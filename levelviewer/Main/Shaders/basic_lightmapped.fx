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

/*texture DiffuseTexture;
sampler2D Diffuse = sampler_state {
    Texture = <DiffuseTexture>;
};*/

sampler2D Diffuse;
sampler2D Lightmap;

/*texture LightmapTexture;
sampler2D Lightmap = sampler_state {
    Texture = <LightmapTexture>;
};*/

float4 ps_main(PS_INPUT input) : COLOR0
{
    float4 diffuse = tex2D(Diffuse, input.texCoords);
    float4 lightmap = tex2D(Lightmap, input.lightmapCoords);
    
    //return lightmap;
    //return diffuse;
    return float4(diffuse.rgb * lightmap.rgb * LightmapMultiplier, diffuse.a);
    //return float4(1.0, 0, 0, 1.0);
}


technique GL
{
    pass P0
    {
        VertexProgram = compile arbvp1 vs_main();
        FragmentProgram = compile arbfp1 ps_main();
    }
}


technique D3D
{
    pass P0
    {
        VertexShader = compile vs_2_0 vs_main();
        PixelShader = compile ps_2_0 ps_main();
    }
}