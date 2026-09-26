Shader "Custom/PlaneBorder"
{
    Properties
    {
        _BorderColor (
            "Border Color",
            Color
        ) = (0.65, 0.9, 1, 0.9)

        _FillColor (
            "Fill Color",
            Color
        ) = (0.5, 0.8, 1, 1)

        _BorderWidth (
            "Border Width",
            Range(0.001,0.25)
        ) = 0.035

        _FillOpacity (
            "Fill Opacity",
            Range(0,1)
        ) = 0.025
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

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
            fixed4 _FillColor;

            float _BorderWidth;
            float _FillOpacity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos =
                    UnityObjectToClipPos(
                        v.vertex
                    );

                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float left =
                    i.uv.x;

                float right =
                    1.0 - i.uv.x;

                float bottom =
                    i.uv.y;

                float top =
                    1.0 - i.uv.y;

                float edgeDistance =
                    min(
                        min(left, right),
                        min(bottom, top)
                    );

                float border =
                    1.0 -
                    smoothstep(
                        _BorderWidth,
                        _BorderWidth * 1.5,
                        edgeDistance
                    );

                fixed4 fill =
                    _FillColor;

                fill.a *=
                    _FillOpacity;

                fixed4 finalColor =
                    lerp(
                        fill,
                        _BorderColor,
                        border
                    );

                return finalColor;
            }

            ENDCG
        }
    }

    FallBack Off
}