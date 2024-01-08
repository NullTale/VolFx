using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    public class BlitPass : VolFx.Pass
    {
        public bool _invert;
        
        public Material      _mat;
        public Optional<int> _pass;

        protected override bool Invert => _invert;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            _material = _mat;
            return _mat != null;
        }

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Utils.Blit(cmd, source, dest, _mat, _pass.GetValueOrDefault(0), _invert);
        }
    }
}