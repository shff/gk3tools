#vertex
#version 130

uniform mat4 ModelViewProjection;
in vec4 position;
void main()
{
	gl_Position = ModelViewProjection * position;
}

#fragment
#version 130

out vec4 outputColor;

void main()
{
	outputColor = vec4(1.0, 1.0, 1.0, 1.0);
}