#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

in vec4 position : POSITION0;
in vec2 texCoords : TEXCOORD0;
in vec3 normals : NORMAL0;
out vec2 o_diffuseCoords;
out vec3 o_normals;

void main()
{
	gl_Position = ModelViewProjection * position;
	o_diffuseCoords = texCoords;
	o_normals = normals;
}


#fragment
#version 130

uniform sampler2D Diffuse;
uniform vec4 Color;

in vec2 o_diffuseCoords;
in vec3 o_normals;
out vec4 outputColor;
void main()
{
	outputColor = texture2D(Diffuse, o_diffuseCoords);// * Color;

	if (outputColor.a < 0.5)
		discard;
}



