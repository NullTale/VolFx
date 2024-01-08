using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Grayscale Sample")]
    public sealed class GrayscaleVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1);

        // =======================================================================
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}