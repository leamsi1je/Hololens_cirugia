Shader "Custom/CrossSectionOrgan"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.85, 0.6, 0.65, 1)
        _PlanePos ("Plane Position (World)", Vector) = (0,0,0,0)
        _PlaneNormal ("Plane Normal (World)", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        // PASADA 1: caras traseras -> marca el stencil = 1 donde el corte "abre" el modelo
        Pass
        {
            Name "BACK_STENCIL"
            Cull Front
            ZWrite On
            ColorMask 0
            Stencil { Ref 1  Comp Always  Pass Replace }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _PlanePos;
            float4 _PlaneNormal;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 worldPos : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = dot(i.worldPos - _PlanePos.xyz, normalize(_PlaneNormal.xyz));
                clip(-d); // descarta el lado "cortado"
                return 0;
            }
            ENDCG
        }

        // PASADA 2: caras frontales normales -> "limpia" el stencil donde sí hay superficie visible
        Pass
        {
            Name "FRONT_MAIN"
            Cull Back
            Stencil { Ref 0  Comp Always  Pass Replace }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _PlanePos;
            float4 _PlaneNormal;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = dot(i.worldPos - _PlanePos.xyz, normalize(_PlaneNormal.xyz));
                clip(-d);

                fixed4 tex = tex2D(_MainTex, i.uv);
                float ndotl = saturate(dot(normalize(i.worldNormal), _WorldSpaceLightPos0.xyz));
                fixed3 lighting = _LightColor0.rgb * ndotl + unity_AmbientSky.rgb * 0.6;
                return fixed4(tex.rgb * _Color.rgb * lighting, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
