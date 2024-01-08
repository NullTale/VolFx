using System;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable]
    public class CurveParameter : VolumeParameter<CurveValue> 
    {
        public CurveParameter(CurveValue value, bool overrideState) : base(value, overrideState) { }

        public override void Interp(CurveValue from, CurveValue to, float t)
        {
            m_Value.Blend(from, to, t);
        }

        public override void SetValue(VolumeParameter parameter)
        {
            m_Value.SetValue(((VolumeParameter<CurveValue>)parameter).value);
        }
    }
}