// SR.fx

// Empty shader, simply registers and should be enabled to ensure we can weave

#include "ReShade.fxh"

// Empty pixel shader
float3 PS_Main(float4 vois : SV_Position, float2 texcoord : TexCoord)  : SV_Target
{
    return tex2D(ReShade::BackBuffer, texcoord).rgb;
}

technique SR
{
    pass EmptyPass
    {
		VertexShader = PostProcessVS;
		PixelShader = PS_Main;
    }
}
