using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable]
    public class CurveValue
    {
        public const int k_Width = 32;
        
        public AnimationCurve _curve;
        public Color[]        _pixels;
        private bool          _build;

        // =======================================================================
        internal void SetValue(CurveValue val)
        {
            if (val._build == false)
                val.Build();
            
            _curve = val._curve;
            val._pixels.CopyTo(_pixels, 0);
        }
        public void Blend(CurveValue a, CurveValue b, float t)
        {
            for (var x = 0; x < k_Width; x++)
                _pixels[x] = Color.LerpUnclamped(a._pixels[x], b._pixels[x], t);
        }

        public CurveValue(AnimationCurve curve)
        {
            _curve  = curve; 
            _pixels = new Color[k_Width];
            for (var n = 0; n < k_Width; n++)
            {
                var val = _curve.Evaluate(n / (float)(k_Width - 1));
                _pixels[n] = new Color(val, val, val, val);
            }
        }
        
        public Texture2D GetTexture(ref Texture2D tex)
        {
            if (tex == null)
            {
                tex = new Texture2D(k_Width, 1, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
            }
            
            tex.SetPixels(_pixels);
            tex.Apply();
            
            return tex;
        }
        
        public void Build()
        {
            if (_build)
                return;
            
            _build = true;
            
            _pixels = new Color[k_Width];
            for (var n = 0; n < k_Width; n++)
            {
                var val = _curve.Evaluate(n / (float)(k_Width - 1));
                _pixels[n] = new Color(val, val, val, val);
            }
        }
    }
}