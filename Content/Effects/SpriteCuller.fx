/**
* Owain Bell - 2026
*
* Sprite culler. Discards any pixel of the sprite that falls outside the
* horizontal screen span [startXScreen, endX] (in pixels). The engine feeds
* it one un-occluded run of columns at a time, so the parts of a sprite that
* are hidden behind walls are simply never drawn.
*/

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//--visible horizontal span, in screen pixels--//
float startXScreen;
float endX;

//--viewport width, used to convert NDC x (-1..1) -> pixels (0..viewWidth)--//
float viewWidth;

matrix WorldViewProjection;

//--SpriteBatch binds the current texture here (register s0)--//
Texture2D SpriteTexture;
sampler2D SpriteSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    //--normalised device coordinates (-1..1)--//
    output.ScreenPosition = output.Position.xy / output.Position.w;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    //--NDC x (-1..1) -> pixel x (0..viewWidth)--//
    float pixelX = (input.ScreenPosition.x * 0.5f + 0.5f) * viewWidth;

    //--cull any column outside the un-occluded span--//
    if (pixelX < startXScreen || pixelX > endX)
        discard;

    //--sample the sprite and apply the SpriteBatch tint--//
    return tex2D(SpriteSampler, input.TexCoord) * input.Color;
}

technique BasicColorDrawing
{
    pass P0
    {
#if OPENGL
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
#else
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
#endif
    }
};
