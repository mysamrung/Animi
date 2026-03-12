Shader "Custom/Ripple"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
		{
            name "Default"

			ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            // Core.hlsl for XR dependencies
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            SAMPLER(sampler_BlitTexture);
            
            TEXTURE2D_X(_PrevFrameTexture);
            SAMPLER(sampler_PrevFrameTexture);

			float4 Fragment(Varyings IN) : COLOR
			{
                float3 e = float3(_BlitTexture_TexelSize.xy,0);
                float speed = 25.0f;

                float p10 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, (IN.texcoord - (e.zy * speed)));
                float p01 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, (IN.texcoord - (e.xz * speed)));
                float p21 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, (IN.texcoord + (e.xz * speed)));
                float p12 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, (IN.texcoord + (e.zy * speed)));

                float p11 = SAMPLE_TEXTURE2D(_PrevFrameTexture, sampler_PrevFrameTexture, IN.texcoord).x;

                float d = (p10 + p01 + p21 + p12)/2 - p11;
                d *= 0.99;

                return d;
			}

			ENDHLSL
		}

        Pass
        {
            ZWrite Off
            ZTest Always 
            Cull Off

            Name "Combine"

          HLSLPROGRAM

        // Core.hlsl for XR dependencies
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        // DebuggingFullscreen.hlsl for URP debug draw
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
        // Color.hlsl for color space conversion
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            SAMPLER(sampler_BlitTexture);

                #pragma vertex Vert
                #pragma fragment FragNearest
            ENDHLSL
        }
    }
}
