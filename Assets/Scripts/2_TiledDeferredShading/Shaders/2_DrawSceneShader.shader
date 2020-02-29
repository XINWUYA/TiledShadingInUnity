Shader "TiledShadingInUnity/2_DrawSceneShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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

            sampler2D _ResultTex;
            float4 _ResultTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ResultTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 ResultColor = tex2D(_ResultTex, i.uv);
                return float4(ResultColor, 1.0f);
            }
            ENDCG
        }
    }
}
