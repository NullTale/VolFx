using UnityEngine;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    public class RenderTargetFlip
    {
        public RenderTarget From => _isFlipped ? _a : _b;
        public RenderTarget To   => _isFlipped ? _b : _a;
			
        private bool         _isFlipped;
        private RenderTarget _a;
        private RenderTarget _b;
			
        // =======================================================================
        public RenderTargetFlip(string name)
        {
            _a = new RenderTarget().Allocate($"{name}_a");
            _b = new RenderTarget().Allocate($"{name}_b");;
        }
        
        public RenderTargetFlip(RenderTarget a, RenderTarget b)
        {
            _a = a;
            _b = b;
        }
			
        public void Flip()
        {
            _isFlipped = !_isFlipped;
        }
			
        public void Release(CommandBuffer cmd)
        {
            _a.Release(cmd);
            _b.Release(cmd);
        }
        
        public void Release()
        {
            RTHandles.Release(_a.Handle);
            RTHandles.Release(_b.Handle);
        }

        public void Get(CommandBuffer cmd, in RenderTextureDescriptor desc)
        {
            _a.Get(cmd, desc);
            _b.Get(cmd, desc);
        }
        
        public void Get(CommandBuffer cmd, in RenderTextureDescriptor desc, FilterMode filter)
        {
            _a.Get(cmd, desc, filter);
            _b.Get(cmd, desc, filter);
        }
    }
}