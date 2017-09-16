/**
* Owain Bell - 2017
* */
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float startXScreen;
uniform float endX;
uniform float viewWidth;

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
	float2 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;
	output.ScreenPosition = output.Position.xy / output.Position.w;

	return output;
}

float4 main(VertexShaderOutput input) : COLOR0
{
	/*
//	float2 scCoord = input.Position.xy / input.Position.w;
	float2 tCoord = (scCoord + 1.0f) / 2.0f;//tex coord
	tCoord.y = 1.0f - tCoord.y;//--

	float2 pixel = tCoord * viewWidth;

	clip(startX - pixel.x);
	*/
	if (input.ScreenPosition.x < startXScreen)
		return 0;
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 main();
	}
};