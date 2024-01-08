using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Bloom")]
    public sealed class BloomVol : VolumeComponent, IPostProcessComponent
    {
        public static GradientValue WhiteClean
        {
            get
            {
                var grad = new UnityEngine.Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        public ClampedFloatParameter m_Intencity = new ClampedFloatParameter(0, 0, 21);
        [HideInInspector]
        public ClampedFloatParameter m_Scatter   = new ClampedFloatParameter(0, 0, 1);
        public CurveParameter        m_Threshold = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                         new Keyframe(.57f, 0f, 8f, 8f, 0f, 0.1732f),
                                                                                         new Keyframe(1f, 1f, .3f, .3f, .32f, 0.0f))), false);
        [Tooltip("Color replacement for initial bloom color by threshold evaluation")]
        public GradientParameter     m_Color     = new GradientParameter(WhiteClean, false);
        [HideInInspector]
        public ClampedFloatParameter m_Flicker = new ClampedFloatParameter(1, 0, 1);


        // =======================================================================
        public bool IsActive() => active && m_Intencity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}