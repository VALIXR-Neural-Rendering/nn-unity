Shader "Overlays/BufferSampler"
{
    SubShader
    {
        // No culling or depth
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off

        Pass
        {
            Name "RGBDBufferPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_OvColorTexture);
            SAMPLER(sampler_OvColorTexture);

            TEXTURE2D(_OvDepthTexture);
            SAMPLER(sampler_OvDepthTexture);

            float4 frag(Varyings input) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_OvColorTexture, sampler_OvColorTexture, input.texcoord);
                float4 depth = SAMPLE_TEXTURE2D(_OvDepthTexture, sampler_OvDepthTexture, input.texcoord);
    
                return float4(color.r, color.g, color.b, depth.r);
    
            }
            ENDHLSL
        }

        Pass
        {
            Name "GenericBufferPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_OvBufferTexture);
            SAMPLER(sampler_OvBufferTexture);

            float4 frag(Varyings input) : SV_Target
            {
                float4 bufData = SAMPLE_TEXTURE2D(_OvBufferTexture, sampler_OvBufferTexture, input.texcoord);
                bufData = normalize(bufData);
                float4 output = float4(
                                bufData.r,  // x-component
                                bufData.g,  // y-component
                                atan2(bufData.g, bufData.r),    // 2d vector orientation
                                sqrt(bufData.r * bufData.r + bufData.g * bufData.g));   // 2d vector magnitude

                return output;
            }
            ENDHLSL
        }
    }
}
