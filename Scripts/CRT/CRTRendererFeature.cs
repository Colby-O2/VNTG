using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTRendererFeature.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT
{
    public sealed class CRTRendererFeature : ScriptableRendererFeature
    {
        [Header("References")]
        [SerializeField] private Shader _shader;

        [Header("Options")]
        [SerializeField] private RenderPassEvent _injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;

        private Material _mat;
        private CRTRendererPass _rp;

        public override void Create()
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Hidden/CRTFilter_URP");
            }

            if (_mat == null && _shader != null)
            {
                _mat = CoreUtils.CreateEngineMaterial(_shader);
            }

            if (_rp == null)
            {
                _rp = new CRTRendererPass(_mat);
                _rp.renderPassEvent = _injectionPoint;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_mat != null) CoreUtils.Destroy(_mat);
            if (_rp != null) _rp.Cleanup();
            _mat = null;
            _rp = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_mat == null || _rp == null) return;

            _rp.Setup(_mat);
            _rp.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_rp);
        }
    }
}