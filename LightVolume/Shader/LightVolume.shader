Shader "Hidden/LightVolume"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
	SubShader
	{
		Tags { "RenderType" = "Qpaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 100
		Cull Off ZWrite Off ZTest Always


		Pass
		{
            name "LightVolume"
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

            // Universal Pipeline shadow keywords
           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
           #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
           #pragma multi_compile _ _SHADOWS_SOFT

           #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
           
            SAMPLER(sampler_BlitTexture);
          

           half4 _ColorMin;
           half4 _ColorMax;

           float _Extinction;
           float _Intensity;
           float _IntensityAmp;
           float _MaxIntensity;
           int _StepCount;

           texture2D _CameraDepthTexture;
           sampler sampler_CameraDepthTexture;

           matrix _CameraVP;
           matrix _CameraIVP;

           float4 LightVolumeIntensity(float3 origin, float3 rayLenght, int stepCount)
           {
               float3 stepDirection = rayLenght / stepCount;
               float3 currentWorldPos = origin;

               float extinction = 0.0f;
               float lightWeight = 0.0f;

               float extinctionStep = _Extinction / stepCount;
               float intensityStep = _Intensity / stepCount;

               [loop]
               for (int i = 0; i < stepCount; i++)
               {
                   half4 shadowCoord = TransformWorldToShadowCoord(currentWorldPos);
                   shadowCoord.z = abs(shadowCoord.z);
                   float attenuation = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord.xyz);
                   extinction += extinctionStep;

                   float shadowExtinction = attenuation * extinction;
                   float fadeOut = 1 - (i / stepCount);

                   lightWeight += (shadowExtinction * intensityStep) * fadeOut;
                   currentWorldPos += stepDirection;
               }

               return lightWeight;
           }


           float4 Fragment(Varyings IN) : COLOR
           {
               float4 depthNormal = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, IN.texcoord);
               float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depthNormal.r, _CameraIVP);


               float3 rayLenght = worldPos.xyz - _WorldSpaceCameraPos.xyz;
               float3 origin = _WorldSpaceCameraPos.xyz;

               float4 volumeIntensity = LightVolumeIntensity(origin, rayLenght, _StepCount);
               volumeIntensity.r = pow(volumeIntensity.r, _IntensityAmp);
               volumeIntensity.r = clamp(volumeIntensity, 0, _MaxIntensity);

               return volumeIntensity.r * lerp(_ColorMin, _ColorMax, clamp(volumeIntensity.r, 0, 1));
           }
			ENDHLSL
		}

        Pass 
        {
            name "Blur"
            //Blend One One
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

            float2 _LightDirectionScreenSpace;
            float _BlurOffset;
            int _SampleCount;


            float4 Fragment(Varyings IN) : COLOR
            {
                float4 output = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.texcoord);

                for (int i = 1; i <= _SampleCount; i++)
                {
                    float4 blur = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.texcoord + (_LightDirectionScreenSpace * i * _BlurOffset));
                    blur *= 1 - ((i - 1) * 1.0 / _SampleCount);
                    output += blur;
                }
                output /= _SampleCount + 1;

                return output;
            }
            ENDHLSL
        }

		Pass
		{
            name "Default"
		    Blend One One

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

			float4 Fragment(Varyings IN) : COLOR
			{
				float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.texcoord);

				return color;
			}

			ENDHLSL
		}
	}
}