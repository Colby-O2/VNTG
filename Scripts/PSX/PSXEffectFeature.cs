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
        private PSXSetGlobalParamters _psxSetGlobalParamtersPass;

        public override void Create()
        {
            if (_psxEffectShader == null)
                _psxEffectShader = Shader.Find("Hidden/PSXMaster_URP");

            if (_material == null)
                _material = CoreUtils.CreateEngineMaterial(_psxEffectShader);

            _psxSetGlobalParamtersPass ??= new PSXSetGlobalParamters()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };
            
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
                _psxSetGlobalParamtersPass.Setup(settings.AmbientColor.value);

                _psxEffectPass.Setup(_material);
                renderer.EnqueuePass(_psxSetGlobalParamtersPass);
                renderer.EnqueuePass(_psxEffectPass);
            }
            else
            {
                _psxSetGlobalParamtersPass.Setup(Color.black);
                renderer.EnqueuePass(_psxSetGlobalParamtersPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
                _material = null;
            }

            base.Dispose(disposing);
        }
    }
}