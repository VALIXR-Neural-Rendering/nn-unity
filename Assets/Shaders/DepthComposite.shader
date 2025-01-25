Shader "Overlays/DepthComposite"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "DepthComposite.hlsl"

            ENDHLSL
        }
    }
}
