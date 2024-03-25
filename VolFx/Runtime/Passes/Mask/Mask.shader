Shader "Hidden/VolFx/Mask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
        
        HLSLINCLUDE
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
        
            frag_in vert(vert_in v)
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
        ENDHLSL

        // =======================================================================
        Pass     // 0
        {
            name "Mask by Alpha"
                        
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D    _SourceTex;    // initial image
            sampler2D    _MainTex;      // processed content
            sampler2D    _MaskTex;      // mask
            float        _Weight;
            
            // =======================================================================
            half4 frag(frag_in i) : SV_Target
            {
                half4 color  = tex2D(_MainTex, i.uv);
                half4 source = tex2D(_SourceTex, i.uv);
                half4 mask   = tex2D(_MaskTex, i.uv);
                
                half4 result = lerp(source, color, mask.a);

                return lerp(source, result, _Weight);
            }
            ENDHLSL
        }
        
        Pass     // 1
        {
            name "Mask by Alpha (blending)"
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D    _SourceTex;
            sampler2D    _MainTex;
            sampler2D    _MaskTex;
            float        _Weight;
            
            // =======================================================================
            half4 frag(frag_in i) : SV_Target
            {
                half4 color  = tex2D(_MainTex, i.uv);
                half4 mask   = tex2D(_MaskTex, i.uv);
                
                color.a *= mask.a * _Weight;
                return color;
            }
            ENDHLSL
        }
        
        Pass     // 2
        {
            name "Mask Grayscale"
                        
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D    _SourceTex;
            sampler2D    _MainTex;
            sampler2D    _MaskTex;
            float        _Weight;
            
            // =======================================================================
            half4 frag(frag_in i) : SV_Target
            {
                half4 color  = tex2D(_MainTex, i.uv);
                half4 source = tex2D(_SourceTex, i.uv);
                half4 mask   = tex2D(_MaskTex, i.uv);
                
                half4 result = lerp(source, color, luma(mask.rgb) * mask.a);

                return lerp(source, result, _Weight);
            }
            ENDHLSL
        }
        
        Pass     // 3
        {
            name "Mask Grayscale (blending)"
            Blend SrcAlpha OneMinusSrcAlpha
                        
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D    _MainTex;
            sampler2D    _MaskTex;
            float        _Weight;
            
            // =======================================================================
            half4 frag(frag_in i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                half4 mask  = tex2D(_MaskTex, i.uv);
                color.a *= luma(mask.rgb) * mask.a * _Weight;
                
                return color;
            }
            ENDHLSL
        }
    }
}