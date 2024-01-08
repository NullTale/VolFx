using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Gradient Map")]
    public sealed class GradientMapVol : VolumeComponent, IPostProcessComponent
    { 
        public static GradientValue Default 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }

        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1f);
        public GradientParameter     m_Gradient = new GradientParameter(Default, false);
        public FloatRangeParameter   m_Mask = new FloatRangeParameter(new Vector2(0, 1), 0, 1);

        // =======================================================================
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}