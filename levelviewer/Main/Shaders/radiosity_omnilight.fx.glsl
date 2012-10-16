#vertex
#version 130

uniform mat4 ModelView;
uniform mat4 Projection;

in vec4 position : POSITION0;
in vec2 texCoords : TEXCOORD0;
in vec2 texCoords2 : TEXCOORD1;
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
uniform sampler2D Alpha;
in vec2 o_diffuseCoords;
out vec4 outputColor;
void main()
{
	vec4 alpha = texture(Alpha, o_diffuseCoords);
	if (alpha.a < 0.5)
		discard;

	outputColor = texture2D(Diffuse, o_diffuseCoords);
}
