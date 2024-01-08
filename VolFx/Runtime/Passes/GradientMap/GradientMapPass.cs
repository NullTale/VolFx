using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/GradientMap")]
    public class GradientMapPass : VolFx.Pass
    {
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_Gradient  = Shader.PropertyToID("_GradientTex");
        private static readonly int s_Weights   = Shader.PropertyToID("_Weights");
        private static readonly int s_Mask      = Shader.PropertyToID("_Mask");
        
        private Texture2D         _tex;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<GradientMapVol>();

            if (settings.IsActive() == false)
                return false;
            
            if (_tex == null)
            {
                _tex = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                _tex.wrapMode = TextureWrapMode.Clamp;
            }
            
            var grad = settings.m_Gradient.value;
            _tex.filterMode = grad._grad.mode == GradientMode.Fixed ? FilterMode.Point : FilterMode.Bilinear;
            _tex.SetPixels(grad._pixels);
            _tex.Apply();
            
            mat.SetTexture(s_Gradient, _tex);
            mat.SetFloat(s_Intensity, settings.m_Weight.value);
            var mask = settings.m_Mask.value;
            if (mask.x == mask.y)
                mask.x += 0.01f;
            mat.SetVector(s_Mask, mask);

            return true;
        }
    }
}