Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center("CenterPos", Vector) = (0,0.5,0) 
        _Scale("Scale",Float) = 0.5
    }
    SubShader
    {
        //Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Tags {
             "RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        

        Pass
        {
            Name "FirstPass"
            Stencil{
                Ref 1
                Comp Always
                Pass Replace
            }
            //ZTest Always
            //ZWrite False
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Center;
            float _Scale;
            fixed4 vertexScaleAndTranslateToCenter(float4 toTransPosition)
            {
                toTransPosition *= _Scale;
                toTransPosition += _Center;
                return toTransPosition;
            } 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertexScaleAndTranslateToCenter(v.vertex));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                
                fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 col = fixed4(length(i.uv),0,0,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            Name "SecondPass"
            ColorMask RGB
            Cull Off
            Tags{"LightMode" = "UniversalForward"}
			//ZWrite Off
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Replace
            }
            ZClip False
            ZTest Always
            ZWrite On
            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert2 (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag2 (v2f i) : SV_Target
            {
                // sample the texture
                
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = fixed4(0,1,0,1);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        
    }
}
