#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

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
in vec2 o_diffuseCoords;
in vec2 o_lightmapCoords;

out vec4 outputColor;
void main()
{
	outputColor = texture2D(Lightmap, o_lightmapCoords);
}
