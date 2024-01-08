using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers
{
    [ExecuteAlways]
    public class InBuffer : MonoBehaviour
    {
        public   Buffer    buffer;
        internal Renderer _renderer;
        
        // =======================================================================
        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            buffer._list.Add(_renderer);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isEditor && Equals(_renderer, null) == false)
                return;
            
            if (Application.isEditor && buffer == null)
                return;
#endif
            
            buffer._list.Remove(_renderer);
        }

        /*private void OnWillRenderObject()
        {
#if UNITY_EDITOR
            if (Application.isEditor && Equals(_renderer, null) == false)
            {
                if (TryGetComponent<Renderer>(out _renderer) == false)
                    return;
            }
            
            if (Application.isEditor && _layer == null)
            {
                return;
            }
#endif
            
            _layer._list.Add(_renderer);
        }*/
    }
}
