Shader "Hidden/VolFx/Bloom"
{
    HLSLINCLUDE
    
    struct vert_in
    {
        float4 pos : POSITION;
        float2 uv  : TEXCOORD0;
    };

    struct frag_in
    {
        float2 uv     : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };
                
    frag_in vert(vert_in input)
    {
        frag_in output;
        output.vertex = input.pos;
        output.uv     = input.uv;
        
        return output;
    }
    
    half luma(half3 rgb)
    {
        return dot(rgb.rgb, half3(0.299, 0.587, 0.114));
    }
    
    half bright(half3 rgb)
    {
        return max(max(rgb.r, rgb.g), rgb.b);
    }
    
    ENDHLSL

	SubShader 
	{
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
		
		Pass	// 0
		{
			name "Filter"
			
	        HLSLPROGRAM
	        
            #pragma multi_compile_local _LUMA _BRIGHTNESS _
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        sampler2D	 _ValueTex;
	        sampler2D	 _ColorTex;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 col  = tex2D(_MainTex, i.uv);
	        	half  val = 0;
#ifdef _LUMA
	        	val  = luma(col.rgb);
#endif
#ifdef _BRIGHTNESS
	        	val  = bright(col.rgb);
#endif
	        	// evaluate threshold
	        	val = tex2D(_ValueTex, half2(val, 0)).r;
	        	
	        	// get color replacement
	        	half4 tint = tex2D(_ColorTex, half2(val, 0));
	            return lerp(col * val, col * val * tint, tint.a);
	        }
			
			ENDHLSL
		}
		
		Pass	// 1
		{
			name "Down Sample"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        float4		 _MainTex_TexelSize;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
				float4 offset = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

			    half4 s;
				s  = tex2D(_MainTex, i.uv + offset.xy);
				s += tex2D(_MainTex, i.uv + offset.zy);
				s += tex2D(_MainTex, i.uv + offset.xw);
			    s += tex2D(_MainTex, i.uv + offset.zw);

			    return s * (1.0 / 4.0);
	        }
			
			ENDHLSL
		}
		
		Pass	// 2
		{
			name "Up Sample"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        sampler2D    _DownTex;
	        
	        float		 _Blend;
	        float4		 _DownTex_TexelSize;
	        float4		 _MainTex_TexelSize;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
				float4 offset = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);
			    half4 s;
			    s  = tex2D(_MainTex, i.uv + offset.xy);
			    s += tex2D(_MainTex, i.uv + offset.zy);
			    s += tex2D(_MainTex, i.uv + offset.xw);
			    s += tex2D(_MainTex, i.uv + offset.zw);

			    s = s * (1.0 / 4);
	            half4 down = tex2D(_DownTex, i.uv);
	            return s + down * _Blend;
	        }
			
			ENDHLSL
		}
		
		Pass	// 3
		{
			name "Combine"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
            #pragma multi_compile_local _BLOOM_ONLY _
	        
	        sampler2D    _MainTex;
	        sampler2D    _BloomTex;
	        
	        float		 _Intensity;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 col  = tex2D(_MainTex, i.uv);
	            half4 bloom = tex2D(_BloomTex, i.uv) * _Intensity;
	        	
#ifdef _BLOOM_ONLY
	        	return bloom;
#endif
				return col + bloom;
				return half4(col.rgb + bloom.rgb, col.a);
	        }
			
			ENDHLSL
		}
	}
}