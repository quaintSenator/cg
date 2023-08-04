Shader "Seasun/XXXShader" 
{
    Properties 
    {
        _ColorTex ("ColorTexture", 2D) = "white" {}
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
			Name "XXXPass"
			
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
				return half4(SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, IN.uv).xyz, 0.5);
			}
			ENDHLSL
		}
    }
    FallBack "Diffuse"
}