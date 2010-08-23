#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

in vec4 position;
in vec2 texCoords;
out vec2 o_diffuseCoords;

void main()
{
	gl_Position = ModelViewProjection * position;
	o_diffuseCoords = texCoords;
}


#fragment
#version 130

uniform sampler2D Diffuse;
uniform float4 Color;
in vec2 o_diffuseCoords;
out vec4 output;
void main()
{
	output = texture2D(Diffuse, o_diffuseCoords);
	
	if (output.a < 0.5)
		discard;
}



