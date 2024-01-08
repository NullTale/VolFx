Shader "Hidden/VolFx/GradientMap"
{
    Properties
    {
        _Intensity("Intensity", Float) = 1
        _Mask("Mask", Vector) = (0,0,0,0)
        [NoScaleOffset]
        _GradientTex("Gradient", 2D) = "red" {}
    }
    
    SubShader
    {
        name "Gradient Map"
        
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float  _Intensity;
            float4 _Mask;

            sampler2D    _MainTex;
            sampler2D    _GradientTex;

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

            frag_in vert (vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .587, .114));
            }

            half4 frag (frag_in i) : SV_Target
            {
                half4 initial  = tex2D(_MainTex, i.uv);
                half val       = luma(initial.rgb);
                half4 result   = tex2D(_GradientTex, float2(val, 0));
                half mask      = step(_Mask.x, val) * step(val, _Mask.y);

                return half4(lerp(initial.rgb, result.rgb, _Intensity * result.a * mask), initial.a);
            }
            ENDHLSL
        }
    }
}