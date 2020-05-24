Shader "Unlit/SkBeamShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MaterialToggle] _Inner ("Inner?", Float) = 1
        _VerticalSpeed ("Vertical Speed", Float) = 1
        _HorizontalSpeed ("Horizontal Speed", Float) = 1
        _SpeedScale ("Global Speed Scale", Float) = 1
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MinAlpha ("Min Alpha", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Overlay+1" "RenderType"="Transparent" }
        
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
            fixed4 _Color;
            float _HorizontalSpeed;
            float _VerticalSpeed;
            float _MinAlpha;
            float _SpeedScale;

            v2f vert (appdata v)
            {
                v2f o;
                float add = sin(v.vertex.z * 60 - _Time.z * 15 * _SpeedScale);
                v.vertex.xy += float2(v.vertex.x, v.vertex.y) * (1 + add * 1.2) * 0.8;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 uv = i.uv + float2((_Time.y * _HorizontalSpeed * _SpeedScale) % 1000, -(_Time.y * _VerticalSpeed * _SpeedScale) % 1000);
                fixed4 col = tex2D(_MainTex, uv) * _Color;
                if(col.a < _MinAlpha)
                    col.a = _MinAlpha;

                return col;
            }
            ENDCG
        }
    }
}
