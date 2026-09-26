Shader "Custom/PlaneBorder"
{
    Properties
    {
        _BorderColor ("Border Color", Color) = (0.2, 0.8, 1, 1)
        _BorderWidth ("Border Width (0-0.5)", Range(0.01, 0.2)) = 0.03
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _BorderColor;
            float _BorderWidth;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // distancia al borde mas cercano del UV (0..1)
                float distToEdge = min(min(i.uv.x, 1 - i.uv.x), min(i.uv.y, 1 - i.uv.y));

                if (distToEdge > _BorderWidth)
                    discard; // centro transparente, no dibuja nada

                return _BorderColor;
            }
            ENDCG
        }
    }
}
