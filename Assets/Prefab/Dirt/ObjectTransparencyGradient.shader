Shader "Custom/ObjectTransparencyGradient"
{
    Properties
    {
        _TransparencyFactor ("Transparency Factor", Range(0, 1)) = 0.5
        _MainTex ("Base Texture", 2D) = "white" { }
        _FaceMeshDepth ("Face Mesh Depth", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        
        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Declare the transparency factor, main texture, and face mesh depth
            float _TransparencyFactor;
            sampler2D _MainTex;
            float _FaceMeshDepth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 worldPos : TEXCOORD0; // World position of the fragment
                float2 uv : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.worldPos;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                half4 col = tex2D(_MainTex, i.uv);
                
                // Get the depth of the dirt object (in world space)
                float dirtDepth = i.worldPos.z;
                
                // Calculate transparency based on depth comparison
                float transparency = saturate(1.0 - (dirtDepth - _FaceMeshDepth) * _TransparencyFactor);
                
                // Set the final color and alpha based on transparency
                col.a = transparency;
                
                // Return the color with modified transparency
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
