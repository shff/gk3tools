#vertex
#version 130

uniform mat4 ModelViewProjection;
in vec4 position;
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
out vec4 output;
void main()
{
	output = texture(Diffuse, o_texCoords * 0.001) + vec4(0, 0, 0, 1.0);
}