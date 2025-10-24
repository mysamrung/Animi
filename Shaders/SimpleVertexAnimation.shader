Shader "Unlit/SimpleVertexAnimation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AnimationTex ("Animation Texture", 2D) = "white" {}
        _Frame ("Frame", Int) = 1
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
                uint vertexID : SV_VertexID; 
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _AnimationTex;
            float4 _AnimationTex_TexelSize;
            
            UNITY_INSTANCING_BUFFER_START(Props)
            int _Frame;
            UNITY_INSTANCING_BUFFER_END(Props)


            sampler2D _MainTex;
            float4 _MainTex_ST;

            half3 NormalUnpack(float v){
                uint ix = asuint(v);
                uint x = (ix >> 20) & 1023;
                uint y = (ix >> 10) & 1023;
                uint z = ix & 1023;
                half3 normal = half3(x, y, z) / 1023.0h;
                return (normal - 0.5h) * 2.0h; // [0,1]��[-1,1]
            }

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;

                float2 animationTexUV = float2((v.vertexID + 0.5) * _AnimationTex_TexelSize.x, (_Time.y * _Frame) * _AnimationTex_TexelSize.y);
                float4 animationColor = tex2Dlod(_AnimationTex, float4(animationTexUV, 0, 0));
                float3 position = animationColor.xyz;
                float3 normal = NormalUnpack(animationColor.w);

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
