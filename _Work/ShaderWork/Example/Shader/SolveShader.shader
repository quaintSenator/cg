Shader "Unlit/SolveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise("NoiseMap",2D) = "white"{}
        _Speed("SolveSpeed",Range(0,10)) = 0
        _Border("BorderEdgeWidth",Float) = 0
    }
    SubShader
    {
        
        Tags { "RenderType"="Transpatent"
            "Queue" = "Transparent"
        }
        LOD 100

        Pass
        {
            Name "FirstPass"
            Blend SrcAlpha OneMinusSrcAlpha
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
            sampler2D _Noise;
            float4 _MainTex_ST;
            float _Speed;
            float _Border;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 noiseCol = tex2D(_Noise,i.uv);

                const float Threshold = (_Time.y % _Speed) / _Speed;//Increasing in TimeRound

                //float isFaded = clamp(noiseCol - Threshold, 0, 1);//这样写 无论如何 每个像素都会是半透明的
                float isFaded = Threshold > noiseCol ? 0 : 1; //这样写 没能去掉分支

                float isOnFadedEdge = abs(Threshold - noiseCol) < _Border ? 1 : 0;

                col.a = isFaded;
                col.g += isOnFadedEdge;
                //col.a = 1 - (time_In_Round * i.uv.x);
                return col;
            }
            ENDCG
        }
    }
}
