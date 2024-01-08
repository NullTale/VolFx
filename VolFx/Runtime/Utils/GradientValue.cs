using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable]
    public class GradientValue
    {
        public const int k_Width = 32;
        
        public Gradient _grad;
        public Color[]  _pixels;

        internal bool _build;
        
        public static GradientValue White
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        // =======================================================================
        public void Build(GradientMode _mode)
        {
            _build = true;
            
            _grad.mode = _mode;
            _pixels    = new Color[k_Width];
            
            for (var x = 0; x < k_Width; x++)
                _pixels[x] = _grad.Evaluate(x / (float)(k_Width - 1));   
        }
        
        internal void SetValue(GradientValue val)
        {
            if (val._build == false)
                val.Build(val._grad.mode);
            
            _grad = val._grad;
            val._pixels.CopyTo(_pixels, 0);
        }
        
        public void Blend(GradientValue a, GradientValue b, float t)
        {
            _build = true;
            
            for (var x = 0; x < k_Width; x++)
                _pixels[x] = Color.LerpUnclamped(a._pixels[x], b._pixels[x], t);
            
            _grad.mode = t < .5f ? a._grad.mode : b._grad.mode;
        }
        
        public Texture2D GetTexture(ref Texture2D tex)
        {
            if (tex == null)
            {
                tex = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
            }
            
            tex.SetPixels(_pixels);
            tex.Apply();
            
            return tex;
        }

        public GradientValue(Gradient grad)
        {
            _grad   = grad; 
            _pixels = new Color[k_Width];
            for (var n = 0; n < k_Width; n++)
                _pixels[n] = grad.Evaluate(n / (float)(k_Width - 1));
        }
    }
}