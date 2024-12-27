Shader "Custom/FaceShader"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Name "DepthMask"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha // For transparent blending
            ZWrite On                       // Write to depth buffer
            ColorMask 0                     // Do not write any color

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(0, 0, 0, 0); // Fully transparent
            }
            ENDHLSL
        }
    }
}
