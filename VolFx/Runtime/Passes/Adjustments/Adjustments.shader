Shader "Hidden/VolFx/Adjustments"
{
    Properties
    {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _Hue("Hue", Float) = 0.00
        _Saturation("Saturation", Float) = 0.00
        _Contrast("Contrast", Float) = 0.00
        _Alpha("Alpha", Float) = 0.00
        _Brightness("Brightness", Float) = 0.00
    }
    
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass    // 0
        {
            Name "Adjustments"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
	        uniform sampler2D _ValueTex;
            
            uniform float  _Hue;
            uniform float  _Saturation;
            uniform float  _Contrast;
            uniform float  _Brightness;
            uniform float  _Alpha;
            uniform float3 _Tint;
                
            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct frag_in
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            fixed luma(fixed3 rgb)
            {
                return dot(rgb.rgb, fixed3(0.299, 0.587, 0.114));
            }
            
            inline fixed3 applyHue(fixed3 aColor, fixed aHue)
            {
                fixed angle = aHue;
                fixed3 k = float3(0.57735, 0.57735, 0.57735);
                fixed cosAngle = cos(angle);
                //Rodrigues' rotation formula
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }

            inline fixed3 applyHSBEffect(fixed3 initial)
            {
                fixed3 result = initial;
                result.rgb = applyHue(result.rgb, _Hue);
                result.rgb = (result.rgb - 0.5f) * (_Contrast) + 0.5f;
                result.rgb = result.rgb + _Brightness;
                result.rgb = lerp(luma(result.rgb), result.rgb, _Saturation);
                return result;
            }
            
            fixed4 frag(frag_in i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed3 res = applyHSBEffect(col);
                
                return lerp(col, fixed4(res * _Tint.rgb, col.a * _Alpha), tex2D(_ValueTex, luma(col.rgb)));
            }
            ENDCG
        }
    }
}