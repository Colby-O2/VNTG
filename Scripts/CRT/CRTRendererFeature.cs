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

        private Material _material;
        private CRTRendererPass _crtPass;

        public override void Create()
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Hidden/CRTFilter_URP");
            }

            if (_material == null && _shader != null)
            {
                _material = CoreUtils.CreateEngineMaterial(_shader);
            }

            if (_crtPass == null)
            {
                _crtPass = new CRTRendererPass(_material);
                _crtPass.renderPassEvent = _injectionPoint;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_material != null) CoreUtils.Destroy(_material);
            _crtPass?.Cleanup();
            _material = null;
            _crtPass = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _crtPass == null) return;

            VolumeStack stack = VolumeManager.instance.stack;
            CRTSettings settings = stack.GetComponent<CRTSettings>();

            bool isGameCamera = renderingData.cameraData.cameraType == CameraType.Game;
            bool isSceneView = renderingData.cameraData.cameraType == CameraType.SceneView && settings.ShowInSceneView.value;

            if (
                settings != null &&
                settings.IsActive() &&
                (isGameCamera || isSceneView)
            )
            {
                _crtPass.Setup(_material);
                _crtPass.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_crtPass);
            }
            else
            {
#if UNITY_6000_4_OR_NEWER
                _crtPass.Cleanup(renderingData.cameraData.camera.GetEntityId());
#else
                _crtPass.Cleanup(renderingData.cameraData.camera.GetInstanceID());
#endif
            }
        }
    }
}