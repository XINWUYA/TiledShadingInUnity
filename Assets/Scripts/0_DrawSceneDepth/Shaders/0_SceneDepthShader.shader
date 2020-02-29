Shader "TiledShadingInUnity/0_SceneDepthShader"
{
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Tags { "RenderType"="Opaque" }

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

            sampler2D _CameraDepthTexture;
            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            //float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //#if UNITY_UV_STARTS_AT_TOP //处于DX
				//	if(_MainTex_TexelSize.y < 0)
				//		o.uv = float2(v.uv.x, 1-v.uv.y);
				//#else
					o.uv = v.uv;
				//#endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
                float linear01Depth = Linear01Depth(depth);
                return float4(linear01Depth, linear01Depth, linear01Depth, 1.0f);// linear01Depth;
            }
            ENDCG
        }
    }
}
