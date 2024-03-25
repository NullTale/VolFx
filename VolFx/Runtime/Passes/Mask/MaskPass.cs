using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Buffer = VolFx.Tools.Buffer;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Mask")]
    public class MaskPass : VolFxProc.Pass
    {
        private static readonly int s_MaskTex   = Shader.PropertyToID("_MaskTex");
        private static readonly int s_SourceTex = Shader.PropertyToID("_SourceTex");
        private static readonly int s_Weight    = Shader.PropertyToID("_Weight");
        
        public  Mode   _mode;
        [Tooltip("If false Mask won't be applied if has no volume overrides")]
        public bool    _persistent = true;
        [Tooltip("Mask texture source if not set in volume")]
        public  Buffer _mask;
        private bool   _blending;

        protected override int MatPass => _mode switch
        {
            Mode.Alpha => 0 + (_blending ? 1 : 0),
            Mode.Luma  => 2 + (_blending ? 1 : 0),
            _          => throw new ArgumentOutOfRangeException()
        };

        // =======================================================================
        public enum Mode
        {
            Alpha,
            Luma
        }
        
        // =======================================================================
        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // use blending blit call to avoid draw in source texture
            _blending = Source == dest;
            
            base.Invoke(cmd, source, dest, context, ref renderingData);
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<MaskVol>();
            if (_persistent == false && settings.IsActive() == false)
                return false;

            var mask = settings.m_Mask.value?.Texture;
            if (mask == null)
                mask = _mask?.Texture;
            
            var weight = settings.m_Weight.value;
            
            if (mask == null)
                return false;
            
            // apply this shit
            mat.SetFloat(s_Weight, weight);
            mat.SetTexture(s_SourceTex, Source.rt);
            mat.SetTexture(s_MaskTex, _mask.Texture);
            
            return true;
        }
        
    }
}