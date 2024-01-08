using UnityEngine;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    public class RenderTarget
    {
        public  RTHandle Handle;
        public  int      Id;
        private bool     _allocated;

        // =======================================================================
        public RenderTarget Allocate(RenderTexture rt, string name)
        {
            Handle = RTHandles.Alloc(rt, name);
            Id     = Shader.PropertyToID(name);
            return this;
        }

        public RenderTarget Allocate(string name)
        {
            Handle = RTHandles.Alloc(name, name: name);
            Id     = Shader.PropertyToID(name);
            return this;
        }

        public void Get(CommandBuffer cmd, in RenderTextureDescriptor desc)
        {
            _allocated = true;
            cmd.GetTemporaryRT(Id, desc);
        }
        
        public void Get(CommandBuffer cmd, in RenderTextureDescriptor desc, FilterMode filter)
        {
            _allocated = true;
            cmd.GetTemporaryRT(Id, desc, filter);
        }

        public void Release(CommandBuffer cmd)
        {
            if (_allocated == false) 
                return;
            
            _allocated = false;
            cmd.ReleaseTemporaryRT(Id);
        }
        
        public static implicit operator RTHandle(RenderTarget rt) => rt.Handle;
    }
}