Shader "Seasun/MultiplePassShader" 
{
    Properties 
    {
    }
    
    SubShader 
    {
        Tags 
		{ 
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
    	
    	HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		ENDHLSL
    	 
    	Pass
    	{
    		Name "XXXPass0"
    		Tags{"LightMode" = "SRPDefaultUnlit"}
			
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			TEXTURE2D(_ColorTex);
			SAMPLER(sampler_ColorTex);

			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};
 
			struct Varyings
			{
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
			};

			Varyings Vert(Attributes IN)
			{
				Varyings OUT;
				OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
				OUT.uv = IN.uv;
				return OUT;
			}
 
			half4 Frag(Varyings IN) : SV_Target
			{
				return half4(0, 1, 0, 1);
			}
			ENDHLSL
    	}
    	
    	Pass 
		{
			Name "XXXPass1"
			Tags{"LightMode" = "UniversalForward"}
			
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			TEXTURE2D(_ColorTex);
			SAMPLER(sampler_ColorTex);

			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};
 
			struct Varyings
			{
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
			};

			Varyings Vert(Attributes IN)
			{
				Varyings OUT;
				OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
				OUT.uv = IN.uv;
				return OUT;
			}
 
			half4 Frag(Varyings IN) : SV_Target
			{
				return half4(1, 0, 0, 0.5);
			}
			ENDHLSL
		}
    }
    FallBack "Diffuse"
}