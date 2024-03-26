using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Buffer = VolFx.Tools.Buffer;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Mask")]
    public sealed class MaskVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(1, 0, 1);
        public BufferParameter       m_Mask   = new BufferParameter();

        // =======================================================================
        [Serializable]
        public class BufferParameter : VolumeParameter<Buffer> { }
        
        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Weight.overrideState || m_Mask.overrideState);

        public bool IsTileCompatible() => false;
    }
}