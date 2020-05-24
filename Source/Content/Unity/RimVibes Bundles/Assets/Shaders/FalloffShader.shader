Shader "Unlit/FalloffShader"
{
    Properties
    {
        _VerticalSpeed ("Speed", Float) = 1
        _ColorA ("Color A", Color) = (1, 1, 1, 1)
        _ColorB ("Color B", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _SpeedScale ("Global Speed Scale", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay-1" "RenderType"="Transparent" }
        
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

                struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorA;
            fixed4 _ColorB;
            float _HorizontalSpeed;
            float _VerticalSpeed;
            float _MinAlpha;
            float _SpeedScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 a = _ColorA;
                fixed4 b = _ColorB;
                float t = length(float2(0.5, 0.5) - i.uv) * 2;
                if (t > 1)
                    t = 1;

                float s = 1 - sin(t * 8 - _Time.z * 18 * _SpeedScale);
                s *= s * s;
                if (s < 0)
                    s = 0;
                t += s * 0.15 * (1 - t);
                if (t > 1)
                    t = 1;
            
                fixed4 col = lerp(a, b, t);
                return col;
            }
            ENDCG
        }
    }
}