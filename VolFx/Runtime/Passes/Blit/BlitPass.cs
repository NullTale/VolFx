using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    public class BlitPass : VolFx.Pass
    {
        [SerializeField] [Tooltip("Used if need to gain the access to the pass in editor")]
        internal  bool       _showInInspector;
        [Tooltip("Invert draw matrix")]
        public bool          _invert;

        public Material      _mat;
        public Optional<int> _pass;

        protected override bool Invert => _invert;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            _material = _mat;
            return _mat != null;
        }
        
        public virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (_showInInspector && hideFlags != HideFlags.None || _showInInspector == false && hideFlags != (HideFlags.HideInInspector | HideFlags.HideInHierarchy))
                return;
            
            if (_showInInspector && hideFlags != HideFlags.None)
                hideFlags = HideFlags.None;
            else
                hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            
            _updateAsset();
            
            // =======================================================================
            async void _updateAsset()
            {
                await Task.Yield();
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Utils.Blit(cmd, source, dest, _mat, _pass.GetValueOrDefault(0), _invert);
        }
    }
}
