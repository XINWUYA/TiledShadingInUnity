Shader "TiledShadingInUnity/1_DrawSceneShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            sampler2D _NormalAndDepthTex;
            sampler2D _PositionTex;
            float4 _MainTex_ST;

            float3 _PointLightPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 FragAlbedo = tex2D(_MainTex, i.uv);
                float3 FragPos = tex2D(_PositionTex, i.uv).xyz;
                float4 NormalAndDepth = tex2D(_NormalAndDepthTex, i.uv);
                float3 FragNormal = normalize(NormalAndDepth.xyz);

                float3 Frag2PointLightDir = _PointLightPos - FragPos;
                float DistanceFrag2PointLight = length(Frag2PointLightDir);

                float3 DiffuseColor = max(dot(normalize(Frag2PointLightDir), FragNormal), 0.0f) * FragAlbedo / (DistanceFrag2PointLight * DistanceFrag2PointLight);

                float3 ResultColor = DiffuseColor;
                //float4 ResultColor = float4(NormalAndDepth.w, NormalAndDepth.w, NormalAndDepth.w, 1.0f);
                return float4(ResultColor, 1.0f);
            }
            ENDCG
        }
    }
}
