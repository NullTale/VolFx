using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Bloom")]
    public class BloomPass : VolFx.Pass
    {
        private static readonly int s_ValueTex  = Shader.PropertyToID("_ValueTex");
        private static readonly int s_ColorTex  = Shader.PropertyToID("_ColorTex");
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_BloomTex  = Shader.PropertyToID("_BloomTex");
        private static readonly int s_DownTex   = Shader.PropertyToID("_DownTex");
        private static readonly int s_Blend     = Shader.PropertyToID("_Blend");
        
        [Range(3, 14)]
        public  int              _samples = 7;
        [CurveRange(0, 0, 1, 3)]
        public AnimationCurve    _scatter = new AnimationCurve(new Keyframe(0.0f, 0.8594512939453125f, 0.0f, 0.1687847524881363f, 0f, .9040920734405518f),
                                                               new Keyframe(1.0f, 2.1807241439819338f, 7.417094707489014f, 1.3360401391983033f, 0.06010228395462036f, 0f));
        public ValueMode         _mode = ValueMode.Luma;
        [CurveRange(0, 0, 1, 1)]
        public  AnimationCurve   _flicker = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(.5f, .77f), new Keyframe(1, 1) }) { postWrapMode = WrapMode.Loop };
        public float             _flickerPeriod = 7f;
        public bool              _bloomOnly;
        
        private float            _time;
        private float            _intensity;
        private ProfilingSampler _sampler;
        private Texture2D        _valueTex;
        private Texture2D        _colorTex;
        
        private RenderTarget[]   _mipDown;
        private RenderTarget[]   _mipUp;
        private float            _scatterLerp;

        public int Samples
        {
            get => _samples;
            set 
            { 
                _samples = value;
                Init();
            }
        }

        // =======================================================================
        public enum ValueMode
        {
            Luma,
            Brightness
        }

        // =======================================================================
        public override void Init()
        {
            _mipDown = new RenderTarget[_samples];
            _mipUp = new RenderTarget[_samples - 1];
            
            for (var n = 0; n < _samples; n++)
                _mipDown[n] = new RenderTarget().Allocate($"bloom_{name}_down_{n}");
            for (var n = 0; n < _samples - 1; n++)
                _mipUp[n] = new RenderTarget().Allocate($"bloom_{name}_up_{n}");
            
            _sampler = new ProfilingSampler(name);
            
            _validateMaterial();
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<BloomVol>();

            if (settings.IsActive() == false)
                return false;
            
            _time += Time.deltaTime;
            
            mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            mat.SetTexture(s_ColorTex, settings.m_Color.value.GetTexture(ref _colorTex));
            _intensity = settings.m_Intencity.value * Mathf.Lerp(1, _flicker.Evaluate(_time / _flickerPeriod), settings.m_Flicker.value);
            _scatterLerp = settings.m_Scatter.value;
            
            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying == false)
            {
                _mipDown = new RenderTarget[_samples];
                _mipUp   = new RenderTarget[_samples - 1];
                
                for (var n = 0; n < _samples; n++)
                    _mipDown[n] = new RenderTarget().Allocate($"bloom_{name}_down_{n}");
                
                for (var n = 0; n < _samples - 1; n++)
                    _mipUp[n]   = new RenderTarget().Allocate($"bloom_{name}_up_{n}");
            }

            _validateMaterial();
        }

        private void _validateMaterial()
        {
            if (_material != null)
            {
                _material.DisableKeyword("_BRIGHTNESS");
                _material.DisableKeyword("_LUMA");
                _material.DisableKeyword("_BLOOM_ONLY");

                _material.EnableKeyword(_mode switch
                {
                    ValueMode.Luma       => "_LUMA",
                    ValueMode.Brightness => "_BRIGHTNESS",
                    _                    => throw new ArgumentOutOfRangeException()
                });

                if (_bloomOnly)
                    _material.EnableKeyword("_BLOOM_ONLY");
            }
        }

        public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            _sampler.Begin(cmd);
            
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.colorFormat        = RenderTextureFormat.ARGB32;
            desc.depthStencilFormat = GraphicsFormat.None;

            for (var n = 0; n < _samples - 1; n++)
            {
                desc.width = Mathf.Max(1, desc.width >> 1);
                desc.height = Mathf.Max(1, desc.height >> 1);
                
                _mipDown[n].Get(cmd, in  desc, FilterMode.Bilinear);
                _mipUp[n].Get(cmd, in  desc, FilterMode.Bilinear);
            }
            
            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
            
            _mipDown[_samples - 1].Get(cmd, in  desc, FilterMode.Bilinear);

            Utils.Blit(cmd, source, _mipDown[0], _material);
                
            for (var n = 1; n < _samples; n++)
                Utils.Blit(cmd, _mipDown[n - 1], _mipDown[n], _material, 1);
            
            var totalBlend = 0f;
            var blend = Mathf.Lerp(_scatter.Evaluate(0f), _scatter.Evaluate(1f), _scatterLerp);
            totalBlend += blend;
            cmd.SetGlobalFloat(s_Blend, blend);
            cmd.SetGlobalTexture(s_DownTex, _mipDown[_samples - 1].Handle.nameID);
            Utils.Blit(cmd, _mipDown[_samples - 2].Handle, _mipUp[_samples - 2].Handle, _material, 2);
            for (var n = _samples - 3; n >= 0; n--)
            {
                var t = n / (float)(_samples - 2);
                blend =  Mathf.Lerp(_scatter.Evaluate(t), _scatter.Evaluate(t), _scatterLerp);
                totalBlend += blend;
                cmd.SetGlobalFloat(s_Blend, blend);
                cmd.SetGlobalTexture(s_DownTex, _mipDown[n].Handle.nameID);
                Utils.Blit(cmd, _mipUp[n + 1].Handle, _mipUp[n].Handle, _material, 2);
            }
            
            _material.SetFloat(s_Intensity, _intensity / totalBlend);
            cmd.SetGlobalTexture(s_BloomTex, _mipUp[0].Handle);
            Utils.Blit(cmd, source, dest, _material, 3);
            
            _sampler.End(cmd);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            foreach (var rt in _mipDown)
                rt.Release(cmd);
            
            foreach (var rt in _mipUp)
                rt.Release(cmd);
        }
    }
}