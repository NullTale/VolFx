using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    public class VolFx : ScriptableRendererFeature
    {
        private static List<ShaderTagId> k_ShaderTags;
        
        [Tooltip("When to execute")]
        public RenderPassEvent               _event  = RenderPassEvent.BeforeRenderingPostProcessing;
        [Tooltip("If not set camera format will be used")]
        public Optional<RenderTextureFormat> _format = new Optional<RenderTextureFormat>(RenderTextureFormat.ARGB32, true);
        [Tooltip("Wich volumes to use")]
        public Optional<LayerMask>           _volumeMask = new Optional<LayerMask>(false);
        [Tooltip("To which source apply post-processing")]
        public SourceOptions                 _source = new SourceOptions();
        [Tooltip("RenderPasses and his execution order")]
        [SocUnique(typeof(BlitPass))] [SocFormat(_regexClear = "Pass$")]
        public SoCollection<Pass>            _passes = new SoCollection<Pass>();
        
        [HideInInspector]
        public Shader _blitShader;

        [NonSerialized]
        public Material _blit;

        [NonSerialized]
        public PassExecution _execution;

        public VolumeStack Stack
        {
            get
            {
                if (_execution._stack == null)
                    _execution._stack = _volumeMask.Enabled ? VolumeManager.instance.CreateStack() : VolumeManager.instance.stack;
                
                return _execution._stack;
            }
        }

        // =======================================================================
        [Serializable]
        public class SourceOptions
        {
            public Source         _source;
            public string         _sourceTex = "_inputTex";
            public RenderTexture  _renderTex;
            public Buffers.Buffer _buffer;
            public MaskOutput     _output;
            [Tooltip("Draw result in camera view")]
            public bool           _screenOutput;
            public string         _outputTex = "_outputTex";
            
            public enum Source
            {
                Camera,
                LayerMask,
                GlobalTex,
                RenderTex,
                Buffer
            }
            
            public enum MaskOutput
            {
                Camera,
                Texture
            } 
        }
        
        [Serializable]
        public abstract class Pass : ScriptableObject
        {
            [NonSerialized]
            public VolFx               _owner;
            [SerializeField]
            internal  bool             _active = true;
            [SerializeField] [HideInInspector]
            private  Shader            _shader;
            protected Material         _material;
            private   bool             _isActive;
            
            protected         VolumeStack Stack  => _owner.Stack;
            protected virtual bool        Invert => false;

            // =======================================================================
            internal bool IsActive
            {
                get => _isActive && _active && _material != null;
                set => _isActive = value;
            }
            
            public void SetActive(bool isActive)
            {
                _active = isActive;
            }
            
            internal void _init()
            {
#if UNITY_EDITOR
                if (_shader == null || _material == null)
                {
                    var shaderName = GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute;
                    if (shaderName != null)
                    {
                        _shader   = Shader.Find(shaderName._name);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (_editorValidate && string.IsNullOrEmpty(assetPath) == false) 
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));

                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
#endif
                
                if (_shader != null)
                    _material = new Material(_shader);
                
                Init();
            }

            /// <summary>
            /// called to perform rendering
            /// </summary>
            public virtual void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Utils.Blit(cmd, source, dest, _material, 0, Invert);
            }

            public void Validate()
            {
#if UNITY_EDITOR
                if (_shader == null || _editorValidate)
                {
                    var shaderName = GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute;
                    if (shaderName != null)
                    {
                        _shader = Shader.Find(shaderName._name);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (string.IsNullOrEmpty(assetPath) == false)
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));
                        
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                
                if ((_material == null || _material.shader != _shader) && _shader != null)
                {
                    _material = new Material(_shader);
                    Init();
                }
#endif
                
                IsActive = Validate(_material);
            }

            /// <summary>
            /// called to initialize pass when material is created
            /// </summary>
            public virtual void Init()
            {
            }

            /// <summary>
            /// called each frame to check is render is required and setup render material
            /// </summary>
            public abstract bool Validate(Material mat);
            
            /// <summary>
            /// frame clean up function used if implemented custom Invoke function to release resources
            /// </summary>
            public virtual void Cleanup(CommandBuffer cmd)
            {
            }
            
            /// <summary>
            /// used for optimization purposes, returns true if we need to call _editorSetup function
            /// </summary>
            protected virtual bool _editorValidate => false;
            
            /// <summary>
            /// editor validation function, used to gather additional references 
            /// </summary>
            protected virtual void _editorSetup(string folder, string asset)
            {
            }
        }

        public class PassExecution : ScriptableRenderPass
        {
            public   VolFx              _owner;
            internal RenderTargetFlip   _renderTarget;
            private  Pass[]             _passes;
            internal VolumeStack        _stack;
            
            private RenderTarget        _output;
            private RendererListParams  _rlp;
            
            private ProfilingSampler    _profiler;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _owner._event;
                
                _renderTarget = new RenderTargetFlip(nameof(_renderTarget));
               
                _output    = new RenderTarget().Allocate(_owner._source._source == SourceOptions.Source.LayerMask && _owner._source._output == SourceOptions.MaskOutput.Texture ? _owner._source._outputTex : "rt_out");
                _rlp       = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, _owner._volumeMask.GetValueOrDefault().value));
                
                _profiler  = new ProfilingSampler(_owner.name);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_owner._volumeMask.Enabled && _stack != null)
                    VolumeManager.instance.Update(_stack, null, _owner._volumeMask.Value);
            
                foreach (var pass in _owner._passes.Values.Where(n => n != null))
                    pass.Validate();
                
                _passes = _owner._passes.Values.Where(n => n != null && n.IsActive).ToArray();
                if (_passes.Length == 0 && _isDrawler() == false)
                    return;
                
                // allocate stuff
                var cmd = CommandBufferPool.Get(_owner.name);
                ref var cameraData = ref renderingData.cameraData;
                ref var desc = ref cameraData.cameraTargetDescriptor;
                if (_owner._format.Enabled)
                    desc.colorFormat = _owner._format.Value;
                _renderTarget.Get(cmd, in desc);

#if UNITY_EDITOR
                if (Application.isPlaying == false && _canGetSourceTex() == false)
                {
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                    return; 
                }
#endif
                var source = _getSourceTex(ref renderingData);
                var output = _getOutputTex(ref renderingData);
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                
                // draw post process chain
                if (_passes.Length != 0)
                {
                    if (_passes.Length == 1)
                    {
                        Utils.Blit(cmd, source, _renderTarget.From, _owner._blit);
                        _passes[0].Invoke(cmd, _renderTarget.From, output, context, ref renderingData);
                    }
                    else
                    {
                        _passes[0].Invoke(cmd, source, _renderTarget.From, context, ref renderingData);
                        for (var n = 1; n < _passes.Length - 1; n++)
                        {
                            var pass = _passes[n];
                            pass.Invoke(cmd, _renderTarget.From, _renderTarget.To, context, ref renderingData);
                            _renderTarget.Flip();
                        }

                        _passes[_passes.Length - 1].Invoke(cmd, _renderTarget.From, output, context, ref renderingData);
                    }
                }
                
                if (_isCameraOverlay())
                    Utils.Blit(cmd, output, _getCameraOutput(ref renderingData), _owner._blit, 1);

                cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
#if UNITY_EDITOR
                bool _canGetSourceTex()
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return true;
                        case SourceOptions.Source.GlobalTex:
                            return true;
                        case SourceOptions.Source.RenderTex:
                            return _owner._source._renderTex != null;
                        case SourceOptions.Source.LayerMask:
                            return true;
                        case SourceOptions.Source.Buffer:
                            return _owner._source._buffer != null;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
#endif
                
                RTHandle _getSourceTex(ref RenderingData renderingData)
                {
                    switch (_owner._source._source)
                    {
                        case SourceOptions.Source.Camera:
                            return _getCameraOutput(ref renderingData);
                        case SourceOptions.Source.GlobalTex:
                            return RTHandles.Alloc(_owner._source._sourceTex);
                        case SourceOptions.Source.RenderTex:
                            return RTHandles.Alloc(_owner._source._renderTex);
                        case SourceOptions.Source.LayerMask:
                        {
                            var desc = renderingData.cameraData.cameraTargetDescriptor;
                            if (_owner._format.Enabled)
                                desc.colorFormat = _owner._format.Value;
                            
                            _output.Get(cmd, in desc);
                            
#if UNITY_2022_1_OR_NEWER
                            var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                            var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                                ? renderingData.cameraData.renderer.cameraColorTarget
                                : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                            cmd.SetRenderTarget(_output.Id, depth);
                            cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1f, 0);
                        
                            ref var cameraData = ref renderingData.cameraData;
                            var     camera     = cameraData.camera;
                            camera.TryGetCullingParameters(out var cullingParameters);

                            _rlp.cullingResults = context.Cull(ref cullingParameters);
                            _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
                            
                            var rl = context.CreateRendererList(ref _rlp);
                            cmd.DrawRendererList(rl);
                            
                            return _output.Handle;
                        }
                        case SourceOptions.Source.Buffer:
                            return RTHandles.Alloc(_owner._source._buffer.GlobalTexName);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                RTHandle _getOutputTex(ref RenderingData renderingData)
                {
                    if (_owner._source._source != SourceOptions.Source.LayerMask)
                        return source;
                    
                    switch (_owner._source._output)
                    {
                        case SourceOptions.MaskOutput.Camera:
                        case SourceOptions.MaskOutput.Texture:
                            return _output.Handle;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                bool _isCameraOverlay()
                {
                    return (_owner._source._source == SourceOptions.Source.LayerMask && _owner._source._output == SourceOptions.MaskOutput.Camera)
                           || (_owner._source._source != SourceOptions.Source.Camera && _owner._source._screenOutput);
                }
                
                bool _isDrawler()
                {
                    return _owner._source._source == SourceOptions.Source.LayerMask;
                }
                
                RTHandle _getCameraOutput(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }
            
            public override void FrameCleanup(CommandBuffer cmd)
            {
                _renderTarget.Release(cmd);
                _output.Release(cmd);
                foreach (var pass in _passes)
                    pass.Cleanup(cmd);
            }
        }

        // =======================================================================
        public override void Create()
        {
#if UNITY_EDITOR
            _blitShader = Shader.Find("Hidden/VolFx/Blit");
            
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            _blit      = new Material(_blitShader);
            _execution = new PassExecution() { _owner = this};
            _execution.Init();
            
            foreach (var pass in _passes)
            {
                if (pass == null)
                    continue;
                
                pass._owner = this;
                pass._init();
            }
            
            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
        }

        private void OnDestroy()
        {
            _passes.Destroy();
        }
        
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            _execution.ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;
#if UNITY_EDITOR
            if (_blit == null)
                _blit = new Material(_blitShader);
#endif
            renderer.EnqueuePass(_execution);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == false)
                return;
            
            if (_volumeMask.Enabled && _execution != null && _execution._stack != null)
                 VolumeManager.instance.DestroyStack(_execution._stack);
        }
    }
}
