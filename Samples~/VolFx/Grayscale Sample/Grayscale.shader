Shader "Hidden/VolFx/Grayscale"
{
    Properties
    {
		_Weight("Weight", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 0
        
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            name "Draw Color Sample"
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            
            float  _Weight;

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(0.299, 0.587, 0.114));
            }

            frag_in vert(const vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            half4 frag(const frag_in i) : SV_Target
            {
                half4 main = tex2D(_MainTex, i.uv);
                half4 col  = luma(main.rgb);
                
                return half4(lerp(main.rgb, luma(col.rgb), _Weight), main.a);
            }
            ENDHLSL
        }
    }
}
