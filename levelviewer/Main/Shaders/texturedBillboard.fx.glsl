#vertex
#version 130

uniform mat4 ModelView;
uniform mat4 Projection;

in vec4 position;
in vec2 texCoords;
in vec2 texCoords2;

void main()
{
	vec4 pos = ModelView * position;
	pos.x += texCoords.x;
	pos.y += texCoords.y;
	
	gl_Position = Projection * pos;
	gl_TexCoord[0] = vec4(texCoords2, 0, 0);
}

#fragment
#version 130

uniform sampler2D Diffuse;
out vec4 output;
void main()
{
	output = texture2D(Diffuse, gl_TexCoord[0].st);
}
