#vertex
#version 130
uniform vec4 Viewport;
in vec2 position;
in vec2 texCoords;
out vec4 o_texCoords;

void main()
{
	o_texCoords = vec4(texCoords, 0, 0);
	gl_Position = vec4(
		((position.x - Viewport.z * 0.5) / Viewport.z) * 2.0,
		((position.y - Viewport.w * 0.5) / Viewport.w) * -2.0,
		0, 1.0);
} 


#fragment
#version 130
uniform sampler2D Diffuse;
uniform vec4 Color;
in vec4 o_texCoords;
out vec4 outputColor;
void main()
{
	outputColor = texture2D(Diffuse, o_texCoords.st) * Color;
}