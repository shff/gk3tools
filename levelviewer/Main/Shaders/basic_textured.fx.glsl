#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

in vec4 position;
in vec2 texCoords;

void main()
{
	gl_Position = ModelViewProjection * position;
	gl_TexCoord[0] = vec4(texCoords, 0, 0);
}


#fragment
#version 130

uniform sampler2D Diffuse;
out vec4 output;
void main()
{
	output = texture2D(Diffuse, gl_TexCoord[0].st);
}



