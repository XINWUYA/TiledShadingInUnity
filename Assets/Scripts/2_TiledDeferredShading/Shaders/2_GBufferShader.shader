Shader "TiledShadingInUnity/2_GBufferShader"
{
    Properties
    {
        _MainTex("Albedo", 2D) = "white" {}
        _NormalTex("Normal", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "GBuffer"="Opaque" }

        Pass
        {
            ZTest LEQual Cull Back ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 posWS : TEXCOORD2;
            };

            struct OutRenderTarget
            {
                float4 AlbedoTarget : COLOR0;
                float4 NormalTarget : COLOR1;
                float4 PositionTarget : COLOR2;
            };

            sampler2D _MainTex;
            sampler2D _NormalTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.posWS = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            OutRenderTarget frag (v2f i)
            {
                OutRenderTarget ort;

                ort.AlbedoTarget = tex2D(_MainTex, i.uv);
                ort.NormalTarget.xyz = normalize(i.normal);// normalize(tex2D(_NormalTex, i.uv));
                ort.NormalTarget.w = 1.0f;
                ort.PositionTarget = i.posWS;
                return ort;
            }
            ENDCG
        }
    }
}
