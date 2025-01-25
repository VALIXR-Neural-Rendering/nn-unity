#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// The Blit.hlsl file provides the vertex shader (Vert),
// input structure (Attributes) and output strucutre (Varyings)
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"


TEXTURE2D(_NeuralPPTexture);
SAMPLER(sampler_NeuralPPTexture);

TEXTURE2D(_DCDepthTexture);
SAMPLER(sampler_DCDepthTexture);

TEXTURE2D(_NonOvColorTexture);
SAMPLER(sampler_NonOvColorTexture);

TEXTURE2D(_NonOvDepthTexture);
SAMPLER(sampler_NonOvDepthTexture);


half4 SampleCompositeColor(float2 uv)
{
    float4 ovDepth = SAMPLE_TEXTURE2D(_DCDepthTexture, sampler_DCDepthTexture, uv);
    float4 nonOvDepth = SAMPLE_TEXTURE2D(_NonOvDepthTexture, sampler_NonOvDepthTexture, uv);
    if (ovDepth.r > nonOvDepth.r)
    {
        half4 ovColor = SAMPLE_TEXTURE2D(_NeuralPPTexture, sampler_NeuralPPTexture, uv);
        return ovColor;
    }
    else
    {
        half4 nonOvColor = SAMPLE_TEXTURE2D(_NonOvColorTexture, sampler_NonOvColorTexture, uv);
        return nonOvColor;
    }
}

half4 frag(Varyings input) : SV_Target
{
    half4 color = SampleCompositeColor(input.texcoord);
	return color;
}