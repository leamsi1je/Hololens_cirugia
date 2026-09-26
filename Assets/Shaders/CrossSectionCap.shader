Shader "Custom/CrossSectionCap"
{
    Properties
    {
        _CapColor ("Cap Color", Color) = (0.75, 0.1, 0.15, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }

        // Solo se dibuja donde el stencil quedo marcado (el hueco del corte)
        Pass
        {
            Name "CAP_FILL"
            Cull Off
            Stencil { Ref 1  Comp Equal  Pass Keep }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _CapColor;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _CapColor;
            }
            ENDCG
        }
    }
}
