#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform int startX;
uniform int endX;

matrix WorldViewProjection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;
	output.TexCoord = output.Position.xy;

	return output;
}

float4 main(VertexShaderOutput input) : COLOR0
{
	
	int pixelX = input.TexCoord.x;
	if (pixelX < startX)
	{
		input.Color.rgba *= 0.0f;
	}

	return input.Color;

}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 MainVS();
		PixelShader = compile ps_4_0_level_9_1 main();
	}
};