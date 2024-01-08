using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace Buffers
{
    [DisallowMultipleRendererFeature("Vol Buffers")]
    public class VolBuffers : ScriptableRendererFeature
    {
        private static List<ShaderTagId> k_ShaderTags;

        private static List<BufferPass>   s_LayersExternal = new List<BufferPass>();

        public  SoCollection<Buffer>    _list         = new SoCollection<Buffer>();
        private List<BufferPass>        _bufferPasses = new List<BufferPass>();

        private static  Material _blit;
        internal static Material _adj;
        
        // =======================================================================
        private class BufferPass : ScriptableRenderPass
        {
            public  Buffer             _buffer;
            private RenderTarget       _output;
            private RendererListParams _rlp;
            private ProfilingSampler   _profiler;
            private int                _layerMask;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _buffer._event;
                _output         = new RenderTarget().Allocate(_buffer.GlobalTexName);
                _profiler       = new ProfilingSampler(_buffer.name);
                _initRenderList();
                
                _layerMask = _buffer._mask.value;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (_buffer == null)
                    return;
#endif
                // allocate resources
                var cmd  = CommandBufferPool.Get(nameof(VolBuffers));
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                
                desc.depthStencilFormat = _buffer._depth switch
                {
                    Buffer.DepthStencil.None           => GraphicsFormat.None,
                    Buffer.DepthStencil.Camera         => GraphicsFormat.None,
                    Buffer.DepthStencil.CameraReadOnly => GraphicsFormat.None,
                    Buffer.DepthStencil.D16S8          => GraphicsFormat.D16_UNorm_S8_UInt,
                    Buffer.DepthStencil.D24S8          => GraphicsFormat.D24_UNorm_S8_UInt,
                    Buffer.DepthStencil.D16            => GraphicsFormat.D16_UNorm,
                    Buffer.DepthStencil.D24            => GraphicsFormat.D24_UNorm,
                    _                                  => throw new ArgumentOutOfRangeException()
                };
                
                if (_buffer._format.Enabled)
                    desc.colorFormat = _buffer._format.Value;
                
                _profiler.Begin(cmd);
                
                _output.Get(cmd, in desc);
      
                if (_buffer._depth == Buffer.DepthStencil.Camera || _buffer._depth == Buffer.DepthStencil.CameraReadOnly)
                {
#if UNITY_2022_1_OR_NEWER
                    var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                    var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                        ? renderingData.cameraData.renderer.cameraColorTarget
                        : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                    cmd.SetRenderTarget(_output.Id, depth);

                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _buffer._clear, 1f, 0);
                }
                else
                {
                    cmd.SetRenderTarget(_output.Id);
                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _buffer._clear, 1f, 0);
                }
                    
                if (_buffer._mask.Enabled && _buffer._mask.Value != 0)
                {
#if UNITY_EDITOR
                    // editor validate fix
                    if (_layerMask != _buffer._mask.value)
                    {
                        _initRenderList();
                        _layerMask = _buffer._mask.Value;
                    }
#endif
                
                    ref var cameraData = ref renderingData.cameraData;
                    var     camera     = cameraData.camera;
                    camera.TryGetCullingParameters(out var cullingParameters);

                    _rlp.cullingResults = context.Cull(ref cullingParameters);
                    _rlp.drawSettings   = CreateDrawingSettings(k_ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);

                    var rl = context.CreateRendererList(ref _rlp);
                    cmd.DrawRendererList(rl);
                }

                try
                {
                    foreach (var rnd in _buffer._list)
                        cmd.DrawRenderer(rnd, rnd.sharedMaterial);
                }
                catch
                {
                    // ignored
                }

                //_layer._list.Clear();

                _profiler.End(cmd);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            private void _initRenderList()
            {
                _rlp = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, _buffer._mask.Value));
                if (_buffer._depth == Buffer.DepthStencil.CameraReadOnly)
                {
                    _rlp.tagValues   = new NativeArray<ShaderTagId>(k_ShaderTags.ToArray(), Allocator.Persistent);
                    var rsb          = new RenderStateBlock() { depthState = new DepthState(false), stencilState = new StencilState(false) };
                    rsb.mask         = RenderStateMask.Depth | RenderStateMask.Stencil;
                    _rlp.stateBlocks = new NativeArray<RenderStateBlock>(new[] { rsb, rsb, rsb }, Allocator.Persistent);
                }
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                _output.Release(cmd);
            }
        }
        
        // =======================================================================
        public override void Create()
        {
            _bufferPasses = _list
                      .Values
                      .Select(n => new BufferPass() { _buffer = n })
                      .Where(n => n._buffer != null)
                      .ToList();

            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
            
            foreach (var pass in _bufferPasses)
                pass.Init();
        }

        private void OnDestroy()
        {
            _list.Destroy();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // in game or scene view only (ignore inspector draw)
            if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView)
                return;
            
            foreach (var pass in _bufferPasses)
                renderer.EnqueuePass(pass);
            foreach (var pass in s_LayersExternal)
                renderer.EnqueuePass(pass);
        }
    }
}
    