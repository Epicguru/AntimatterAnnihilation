Shader "Unlit/SphereShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1, 0, 0, 1)
        _FillColor ("Fill Color", Color) = (1, 1, 0, 1)
        _SolidColor ("Solid Color", Color) = (1, 0, 1, 1)
        [MaterialToggle] _Transparent("Transparent", Float) = 1
        _Luminosity ("Luminosity", Float) = 1
        _Stretch ("Stretch", Float) = 1
        _LengthSpeed ("Length Speed", Float) = 1
        _RotSpeed ("Rotation Speed", Float) = 1
        _SpeedScale ("Global Speed Scale", Float) = 1

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
            fixed4 _EdgeColor;
            fixed4 _FillColor;
            fixed4 _SolidColor;
            float _Transparent;
            float _Luminosity;
            float _Stretch;
            float _RotSpeed;
            float _LengthSpeed;
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
                float2 uv = i.uv * float2(0.2, _Stretch * 0.5) + float2((_Time.x * _RotSpeed * _SpeedScale) % 1000, (_Time.x * _LengthSpeed * _SpeedScale) % 1000);
                uv += float2(sin(_Time.x * _SpeedScale + uv.y * 40) * 0.05, cos(_Time.y * _SpeedScale + uv.x * 30) * 0.05);

                fixed4 col = tex2D(_MainTex, uv);
                float alpha = col.b * col.b * col.b * 2;
                col *= _Luminosity;
                if(_Transparent > 0){                
                    col.a = alpha;
				}else{
                    col = _SolidColor;
				}
                if(alpha > 0.3 && alpha < 0.35)
                    col = _EdgeColor;
                else if(alpha > 0.35)
                    col = _FillColor;

                return col;
            }
            ENDCG
        }
    }
}
