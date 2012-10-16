#vertex
#version 130

uniform vec4 Viewport;
uniform mat4 ModelViewProjection;

in vec4 position : POSITION0;
in vec2 texCoords : TEXCOORD0;
in vec2 lightmapCoords : TEXCOORD1;

out vec2 o_diffuseCoords;
out vec2 o_lightmapCoords;

void main()
{
	gl_Position = ModelViewProjection * position;
	o_diffuseCoords = texCoords;
	o_lightmapCoords = lightmapCoords;
}


#fragment
#version 130

uniform sampler2D Lightmap;
uniform sampler2D Diffuse;
uniform float LightmapMultiplier;

in vec2 o_diffuseCoords;
in vec2 o_lightmapCoords;

out vec4 outputColor;
void main()
{
	// do alpha testing
	vec4 diffuseColor = texture(Diffuse, o_diffuseCoords);
	if (diffuseColor.a < 0.5)
		discard;

	if (gl_FrontFacing)
		outputColor = texture2D(Lightmap, o_lightmapCoords);
	else
		outputColor = vec4(0, 0, 0, 0);
}



