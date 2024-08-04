using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Tools
{
    [DisallowMultipleRendererFeature("VolFx Buffers")]
    public class VolFxBuffers : ScriptableRendererFeature
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
            private RenderTarget       _depth;
            private RendererListParams _rlp;
            private ProfilingSampler   _profiler;
            private int                _layerMask;

            // =======================================================================
            public void Init()
            {
                renderPassEvent = _buffer._event;
                _output         = new RenderTarget().Allocate(_buffer.GlobalTexName);
                _depth      = new RenderTarget().Allocate($"{_buffer.GlobalTexName}_Depth");
                _profiler       = new ProfilingSampler(_buffer.name);
                _initRenderList();
                
                if (SystemInfo.copyTextureSupport == CopyTextureSupport.None && _buffer._depth == Buffer.DepthStencil.Copy)
                    Debug.LogError($"\'{_buffer.name}\' buffer depth mode is set to copy, but texture copy functionality is not supported by the platform");
                
                _layerMask = _buffer._mask.value;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
                /*colorDesc.depthStencilFormat = _buffer._depth switch
                {
                    Buffer.DepthStencil.None   => GraphicsFormat.None,
                    Buffer.DepthStencil.Camera => GraphicsFormat.None,
                    Buffer.DepthStencil.Clean  => GraphicsFormat.D32_SFloat_S8_UInt,
                    Buffer.DepthStencil.Copy   => GraphicsFormat.None,
                    _                          => throw new ArgumentOutOfRangeException()
                };*/
                
                if (_buffer._format.Enabled)
                    colorDesc.colorFormat = _buffer._format.Value;

                _output.Get(cmd, in colorDesc);
                
#if UNITY_2022_1_OR_NEWER
                var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                var depth = renderingData.cameraData.renderer.cameraDepthTarget == BuiltinRenderTextureType.CameraTarget
                    ? renderingData.cameraData.renderer.cameraColorTarget
                    : renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                switch (_buffer._depth)
                {
                    case Buffer.DepthStencil.None:
                        ConfigureTarget(_output);
                        break;
                    case Buffer.DepthStencil.Copy:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        if (SystemInfo.copyTextureSupport != CopyTextureSupport.None)
                            cmd.CopyTexture(depth, _depth.Id);
                        else
                            Utils.Blit(cmd, _depth, _blit, 2);  // fix of hope (if api does not support CopyTexture functionality very unlikely it can handle SV_Depth)
                        
                        ConfigureTarget(_output, _depth);
                    } break;
                    case Buffer.DepthStencil.Camera:
                        ConfigureTarget(_output, depth);
                        break;
                    case Buffer.DepthStencil.Clean:
                    {
                        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
                        depthDesc.colorFormat        = RenderTextureFormat.Depth;
                        depthDesc.graphicsFormat     = GraphicsFormat.None;
                        depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
                        
                        _depth.Get(cmd, in depthDesc);
                        
                        ConfigureTarget(_output, _depth);
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (_buffer == null)
                    return;
#endif
                // allocate resources
                var cmd = CommandBufferPool.Get(nameof(VolFxBuffers));
                _profiler.Begin(cmd);
                
                if (_buffer._depth == Buffer.DepthStencil.Camera)
                {
                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _buffer._clear, 1f, 0);
                }
                else
                if (_buffer._depth == Buffer.DepthStencil.Copy)
                {
                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color, _buffer._clear, 1f, 0);
                }
                else
                if (_buffer._depth == Buffer.DepthStencil.None)
                {
                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _buffer._clear, 1f, 0);
                }
                else
                if (_buffer._depth == Buffer.DepthStencil.Clean)
                {
                    if (_buffer._clear.Enabled)
                        cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, _buffer._clear, 1f, 0);
                    else
                        cmd.ClearRenderTarget(RTClearFlags.Depth, Color.clear, 1f, 0);
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

                _profiler.End(cmd);
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            private void _initRenderList()
            {
                _rlp = new RendererListParams(new CullingResults(), new DrawingSettings(), new FilteringSettings(RenderQueueRange.all, _buffer._mask.Value));
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (_buffer._depth == Buffer.DepthStencil.Copy)
                    _depth.Release(cmd);
                
                if (_buffer._depth == Buffer.DepthStencil.Clean)
                    _depth.Release(cmd);
                
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
            
            if (_blit == null)
                _blit = new Material(Shader.Find("Hidden/VolFx/Blit"));
        }

        private void OnDestroy()
        {
            _list.Destroy();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // in game view only (ignore inspector draw)
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;
            
            foreach (var pass in _bufferPasses)
                renderer.EnqueuePass(pass);
            foreach (var pass in s_LayersExternal)
                renderer.EnqueuePass(pass);
        }
    }
}
    