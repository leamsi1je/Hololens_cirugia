Shader "Custom/CrossSectionCap"
{
    Properties
    {
        _CapColor ("Cap Color", Color) = (0.65, 0.08, 0.06, 1)
        _CapDarkColor ("Cap Dark Color", Color) = (0.25, 0.015, 0.01, 1)

        _PatternScale ("Pattern Scale", Range(1,80)) = 22
        _CapEmission ("Cap Emission", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
        }

        Pass
        {
            Name "CAP_FILL"

            Cull Off

            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _CapColor;
            fixed4 _CapDarkColor;

            float _PatternScale;
            float _CapEmission;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldPos =
                    mul(
                        unity_ObjectToWorld,
                        v.vertex
                    ).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 p =
                    i.worldPos *
                    _PatternScale;

                float a =
                    sin(
                        p.x * 1.7 +
                        sin(p.y * 0.8)
                    );

                float b =
                    sin(
                        p.y * 1.3 +
                        p.z * 1.1
                    );

                float c =
                    sin(
                        p.z * 1.9 +
                        p.x * 0.6
                    );

                float pattern =
                    saturate(
                        0.5 +
                        (a + b + c) / 6.0
                    );

                fixed3 finalColor =
                    lerp(
                        _CapDarkColor.rgb,
                        _CapColor.rgb,
                        pattern
                    );

                finalColor +=
                    _CapColor.rgb *
                    _CapEmission;

                return fixed4(
                    finalColor,
                    1
                );
            }

            ENDCG
        }
    }

    FallBack Off
}
