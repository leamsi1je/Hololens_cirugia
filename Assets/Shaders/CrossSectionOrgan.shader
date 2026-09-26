Shader "Custom/CrossSectionOrgan"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color (
            "Color",
            Color
        ) = (0.85, 0.6, 0.65, 1)

        _PlanePos (
            "Plane Position (World)",
            Vector
        ) = (0,0,0,0)

        _PlaneNormal (
            "Plane Normal (World)",
            Vector
        ) = (0,1,0,0)

        _CutColor (
            "Cut Color",
            Color
        ) = (0.8, 0.15, 0.1, 1)

        _CutEmission (
            "Cut Emission",
            Range(0,5)
        ) = 0.4

        _CutBorderWidth (
            "Cut Border Width",
            Range(0,0.1)
        ) = 0.005

        _CutEnabled (
            "Cut Enabled",
            Float
        ) = 1

        _SurgicalMode ("Surgical Gray Mode", Float) = 0

        _SurgicalGrayStrength ("Surgical Gray Strength", Range(0,1)) = 0.9
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        // =====================================
        // PASS 1
        // CARAS TRASERAS -> STENCIL
        // =====================================

        Pass
        {
            Name "BACK_STENCIL"

            Cull Front
            ZWrite On
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _PlanePos;
            float4 _PlaneNormal;

            float _CutEnabled;

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

                o.worldPos =
                    mul(
                        unity_ObjectToWorld,
                        v.vertex
                    ).xyz;

                o.pos =
                    UnityObjectToClipPos(
                        v.vertex
                    );

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d =
                    dot(
                        i.worldPos -
                        _PlanePos.xyz,

                        normalize(
                            _PlaneNormal.xyz
                        )
                    );

                if (_CutEnabled > 0.5)
                {
                    clip(-d);
                }

                return 0;
            }

            ENDCG
        }

        // =====================================
        // PASS 2
        // SUPERFICIE DEL ÓRGANO
        // =====================================

        Pass
        {
            Name "FRONT_MAIN"

            Cull Back

            Stencil
            {
                Ref 0
                Comp Always
                Pass Replace
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _CutColor;

            float4 _PlanePos;
            float4 _PlaneNormal;

            float _CutEmission;
            float _CutBorderWidth;
            float _CutEnabled;

            float _SurgicalMode;
            float _SurgicalGrayStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;

                float3 worldPos :
                    TEXCOORD0;

                float3 worldNormal :
                    TEXCOORD1;

                float2 uv :
                    TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.worldPos =
                    mul(
                        unity_ObjectToWorld,
                        v.vertex
                    ).xyz;

                o.worldNormal =
                    UnityObjectToWorldNormal(
                        v.normal
                    );

                o.pos =
                    UnityObjectToClipPos(
                        v.vertex
                    );

                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 planeNormal =
                    normalize(
                        _PlaneNormal.xyz
                    );

                float d =
                    dot(
                        i.worldPos -
                        _PlanePos.xyz,

                        planeNormal
                    );

                // ------------------------
                // CORTE
                // ------------------------

                if (_CutEnabled > 0.5)
                {
                    clip(-d);
                }

                // ------------------------
                // TEXTURA
                // ------------------------

                fixed4 tex =
                    tex2D(
                        _MainTex,
                        i.uv
                    );

                float3 normal =
                    normalize(
                        i.worldNormal
                    );

                // ------------------------
                // ILUMINACIÓN
                // ------------------------

                float3 lightDir =
                    normalize(
                        UnityWorldSpaceLightDir(
                            i.worldPos
                        )
                    );

                float ndotl =
                    saturate(
                        dot(
                            normal,
                            lightDir
                        )
                    );

                // ------------------------
                // RIM LIGHT SUAVE
                // ------------------------

                float3 viewDir =
                    normalize(
                        _WorldSpaceCameraPos -
                        i.worldPos
                    );

                float rim =
                    pow(
                        1.0 -
                        saturate(
                            dot(
                                normal,
                                viewDir
                            )
                        ),
                        3.0
                    );

                fixed3 baseColor =
                    tex.rgb *
                    _Color.rgb;

                // ==================================
                // MODO QUIRÚRGICO
                // ==================================

                if (_SurgicalMode > 0.5)
                {
                    float luminance =
                        dot(
                            baseColor,
                            float3(
                                0.299,
                                0.587,
                                0.114
                            )
                        );

                    fixed3 grayColor =
                        fixed3(
                            luminance,
                            luminance,
                            luminance
                        );

                    baseColor =
                        lerp(
                            baseColor,
                            grayColor,
                            _SurgicalGrayStrength
                        );

                    // Reducimos ligeramente el brillo
                    // para que el órgano objetivo destaque.
                    baseColor *= 0.82;
                }


                fixed3 lighting =
                    baseColor *
                    (
                        0.35 +
                        ndotl * 0.65
                    );

                lighting +=
                    baseColor *
                    rim *
                    0.15;

                // ==================================
                // RESALTE DEL BORDE DEL CORTE
                // ==================================

                if (_CutEnabled > 0.5)
                {
                    float borderWidth =
                        max(
                            _CutBorderWidth,
                            0.00001
                        );

                    float cutEdge =
                        1.0 -
                        smoothstep(
                            0.0,
                            borderWidth,
                            abs(d)
                        );

                    lighting +=
                        _CutColor.rgb *
                        cutEdge *
                        _CutEmission;
                }

                return fixed4(
                    lighting,
                    1
                );
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
