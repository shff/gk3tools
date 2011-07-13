#vertex
#version 130

uniform vec4 Viewport;
uniform mat4x4 ModelViewProjection;

in vec4 position;
in vec2 texCoords;
in vec2 lightmapCoords;
out vec2 o_diffuseCoords;
out vec2 o_lightmapCoords;

void main()
{
	gl_Position = ModelViewProjection * position;
	o_diffuseCoords = texCoords;
	o_lightmapCoords = lightmapCoords;
}


#fragment
#version 130

uniform sampler2D Diffuse;
uniform sampler2D Lightmap;
uniform float LightmapMultiplier;

in vec2 o_diffuseCoords;
in vec2 o_lightmapCoords; 

out vec4 outputColor;

void main()
{
	vec4 diffuse = texture2D(Diffuse, o_diffuseCoords);
	
	// do alpha testing
	if (diffuse.a < 0.9) discard;
	
    vec4 lightmap = texture2D(Lightmap, o_lightmapCoords);
    
    outputColor = vec4(diffuse.rgb * lightmap.rgb * LightmapMultiplier, diffuse.a);
}