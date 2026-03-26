using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectFeature.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    public sealed class PSXEffectFeature : ScriptableRendererFeature
    {
        [Header("Settings")]
        [SerializeField] private RenderPassEvent _renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        private Shader _psxEffectShader;

        private Material _material;
        private PSXEffectPass _psxEffectPass;

        public override void Create()
        {
            if (_psxEffectShader == null)
                _psxEffectShader = Shader.Find("Hidden/PSXMaster_URP");

            if (_material == null)
                _material = CoreUtils.CreateEngineMaterial(_psxEffectShader);

            _psxEffectPass ??= new PSXEffectPass(_material)
            {
                renderPassEvent = _renderEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _psxEffectPass == null)
            {
                Debug.LogWarning("PSX Feature missing material or pass.");
                return;
            }

            VolumeStack stack = VolumeManager.instance.stack;
            PSXEffectSettings settings = stack.GetComponent<PSXEffectSettings>();

            bool isGameCamera = renderingData.cameraData.cameraType == CameraType.Game;
            bool isSceneView = renderingData.cameraData.cameraType == CameraType.SceneView && settings.ShowInSceneView.value;

            if (
                settings != null &&
                settings.IsActive() &&
                (isGameCamera || isSceneView)
            )
            {
                _psxEffectPass.Setup(_material);
                renderer.EnqueuePass(_psxEffectPass);
            }
        }
    }
}