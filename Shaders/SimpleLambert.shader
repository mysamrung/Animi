Shader "Unlit/SimpleLambert"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 position = v.vertex;
                float3 normal = v.normal;
                
                o.vertex = UnityObjectToClipPos(position.xyz);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                  
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = max(0.5, dot(i.worldNormal, lightDir));
                float4 col = tex2D(_MainTex, i.uv);
                col.rgb *= NdotL;

                return col;
            }
            ENDCG
        }
    }
}
