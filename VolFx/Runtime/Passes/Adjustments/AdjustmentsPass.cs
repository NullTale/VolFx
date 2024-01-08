using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Adjustments")]
    public class AdjustmentsPass : VolFx.Pass
    {
        private static readonly int s_Contrast   = Shader.PropertyToID("_Contrast");
        private static readonly int s_Hue        = Shader.PropertyToID("_Hue");
        private static readonly int s_Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int s_Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int s_Tint       = Shader.PropertyToID("_Tint");
        private static readonly int s_Value      = Shader.PropertyToID("_Value");
        private static readonly int s_Alpha      = Shader.PropertyToID("_Alpha");
        private static readonly int s_ValueTex  = Shader.PropertyToID("_ValueTex");
        
        private Texture2D _valueTex;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<AdjustmentsVol>();

            if (settings.IsActive() == false)
                return false;
            
            mat.SetFloat(s_Contrast, settings.m_Contrast.value + 1f);
            mat.SetFloat(s_Hue, settings.m_Hue.value * Mathf.PI);
            mat.SetFloat(s_Saturation, settings.m_Saturation.value + 1f);
            mat.SetFloat(s_Brightness, settings.m_Brightness.value);
            mat.SetFloat(s_Alpha, settings.m_Alpha.value <= 0 ? settings.m_Alpha.value + 1f : Mathf.Pow(settings.m_Alpha.value + 1, 7));
            
            mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            mat.SetColor(s_Tint, settings.m_Tint.value);
            
            return true;
        }
    }
}