#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

in vec4 position;
in vec2 texCoords;
in vec2 lightmapCoords;

void main()
{
	gl_Position = ModelViewProjection * position;
	gl_TexCoord[0] = vec4(texCoords, 0, 0);
	gl_TexCoord[1] = vec4(lightmapCoords, 0, 0);
}

#fragment
#version 130

uniform sampler2D Diffuse;
uniform sampler2D Lightmap;
out vec4 output;
void main()
{
	output = texture2D(Lightmap, gl_TexCoord[1].st);
}
