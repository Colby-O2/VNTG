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

        private void UpdateMaterialWithSettings(Material mat, PSXEffectSettings settings)
        {
            mat.SetVector("_PixelResolution", 
                new Vector2(
                    Mathf.Max(1, settings.PixelResolution.value.x),
                    Mathf.Max(1, settings.PixelResolution.value.y)
                )
            );

            mat.SetFloat("_ColorPrecision", settings.ColorPrecision.value);

            mat.SetFloat("_EnableDither", (settings.EnableDither.value) ? 1 : 0);
            mat.SetFloat("_DitherMode", (int)settings.DitherMode.value);
            mat.SetInt("_DitherPattern", settings.DitherPattern.value);
            mat.SetFloat("_DitherPixelPerfect", settings.DitherPixelPerfect.value ? 1 : 0);
            mat.SetFloat("_DitherScale", Mathf.Lerp(1f, 10f, settings.DitherScale.value));
            mat.SetFloat("_DitherThreshold", settings.DitherThreshold.value);

            mat.SetInt("_EnableFog", (settings.EnableFog.value) ? 1 : 0);
            mat.SetColor("_FogColor", settings.FogColor.value);
            mat.SetFloat("_FogDensity", settings.FogDensity.value);
            mat.SetFloat("_FogEdgeSmoothness", settings.FogEdgeSmoothness.value);
            mat.SetFloat("_FogNoiseStrength", settings.FogNoiseStrength.value);
            mat.SetFloat("_FogNoiseScale", Mathf.Lerp(1f, 10f, settings.FogNoiseScale.value));
            mat.SetFloat("_FogNoiseStart", Mathf.Lerp(0f, 100f, settings.FogNoiseStart.value));
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
            if (settings == null || !settings.IsActive()) return;

            UpdateMaterialWithSettings(_material, settings);

            renderer.EnqueuePass(_psxEffectPass);
        }
    }
}
