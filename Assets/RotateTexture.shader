Shader "Hidden/RotateTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RotationAngle ("Rotation Angle", Float) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RotationAngle;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Rotate UVs around the center
                float2 center = float2(0.5, 0.5);
                float2 uv = v.uv - center;
                float cosTheta = cos(radians(_RotationAngle));
                float sinTheta = sin(radians(_RotationAngle));
                float2 rotatedUV = float2(
                    uv.x * cosTheta - uv.y * sinTheta,
                    uv.x * sinTheta + uv.y * cosTheta
                );
                o.uv = rotatedUV + center;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
