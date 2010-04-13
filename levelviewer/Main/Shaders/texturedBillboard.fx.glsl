#vertex
#version 130

uniform mat4 ModelView;
uniform mat4 Projection;

in vec4 position;
in vec2 texCoords;
in vec2 texCoords2;
out vec2 o_diffuseCoords;

void main()
{
	vec4 pos = ModelView * position;
	pos.x += texCoords.x;
	pos.y += texCoords.y;
	
	gl_Position = Projection * pos;
	o_diffuseCoords = texCoords2;
}

#fragment
#version 130

uniform sampler2D Diffuse;
in vec2 o_diffuseCoords;
out vec4 output;
void main()
{
	output = texture2D(Diffuse, o_diffuseCoords);
}
