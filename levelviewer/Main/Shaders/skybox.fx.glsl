#vertex
#version 130

uniform mat4 ModelViewProjection;
in vec4 position : POSITION0;
out vec3 o_texCoords;
void main()
{
	gl_Position = ModelViewProjection * position;
	o_texCoords = position.xyz;
}


#fragment
#version 130

uniform samplerCube Diffuse;
in vec3 o_texCoords;
out vec4 outputColor;
void main()
{
	outputColor = texture(Diffuse, o_texCoords * 0.001);
}