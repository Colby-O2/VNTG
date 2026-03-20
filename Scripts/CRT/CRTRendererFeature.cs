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


        private void UpdateMaterialWithSettings(Material mat, CRTSettings settings)
        {
            mat.SetFloat("_RefreshRate", settings.RefreshRate.value);
            mat.SetFloat("_DecayRate", settings.DecayRate.value);
            mat.SetVector("_ScreenResolution", settings.ScreenResolution.value);
            mat.SetInt("_EnableInterlacedRendering", settings.EnableInterlacedRendering.value ? 1 : 0);

            mat.SetInt("_EnableScreenBend", settings.EnableScreenBend.value ? 1 : 0);
            mat.SetFloat("_ScreenBend", settings.ScreenBend.value);
            mat.SetFloat("_ScreenRoundness", settings.ScreenRoundness.value);
            mat.SetFloat("_VignetteOpacity", settings.VignetteOpacity.value);

            mat.SetVector("_ScanLineOpacity", new Vector2(settings.ScanLineVerticalOpacity.value, settings.ScanLineHorizontalOpacity.value));
            mat.SetVector("_ScanLineSpeed", new Vector2(settings.ScanLineVerticalSpeed.value, settings.ScanLineHorizontalSpeed.value));
            mat.SetFloat("_ScanLineStrength", settings.ScanLineStrength.value);

            mat.SetFloat("_NoiseSpeed", settings.NoiseSpeed.value);
            mat.SetFloat("_NoiseScale", settings.NoiseScale.value);
            mat.SetVector("_NoiseRGBOffset", new Vector2(settings.NoiseRBGOffsetX.value, settings.NoiseRBGOffsetY.value));
            mat.SetFloat("_NoiseFade", settings.NoiseFade.value);

            mat.SetFloat("_VHSSmear", Mathf.Lerp(1, 0.05f, settings.VhsSmear.value));
            mat.SetFloat("_UnsharpAmount", settings.UnsharpAmount.value);
            mat.SetFloat("_UnsharpRadius", settings.UnsharpRadius.value);
            mat.SetFloat("_UnsharpThreshold", settings.UnsharpThreshold.value);
            mat.SetFloat("_ClampBlack", settings.ClampBlack.value);
            mat.SetFloat("_ClampWhite", settings.ClampWhite.value);
            mat.SetColor("_TintShadowsColor", settings.ShadowTint.value);

            mat.SetInt("_EnableTrackerLine", settings.EnableTrackerLine.value ? 1 : 0);
            mat.SetFloat("_TrackingSpeed", settings.TrackingSpeed.value);
            mat.SetFloat("_TrackingJitter", settings.TrackingJitter.value);
            mat.SetInt("_EnableSignalInterference", settings.EnableSignalInterference.value ? 1 : 0);
            mat.SetFloat("_InterferenceFrequency", settings.InterferenceFrequency.value);
            mat.SetFloat("_InterferenceAmplitude", settings.InterferenceAmplitude.value);

            mat.SetFloat("_ChromaticOffset", settings.ChromaticOffset.value);
            mat.SetFloat("_ChromaticSpeed", settings.ChromaticOffsetSpeed.value);

            mat.SetFloat("_Brightness", settings.Brightness.value);
            mat.SetFloat("_Contrast", settings.Contrast.value);
            mat.SetFloat("_Saturation", settings.Saturation.value);
            mat.SetFloat("_Gamma", settings.Gamma.value);
            mat.SetFloat("_Hue", settings.Hue.value);
            mat.SetFloat("_RedShift", settings.RedShift.value);
            mat.SetFloat("_GreenShift", settings.GreenShift.value);
            mat.SetFloat("_BlueShift", settings.BlueShift.value);
            mat.SetInt("_IsMonochrome", settings.IsMonochrome.value ? 1 : 0);

            mat.SetInt("_SubPixelMode", (int)settings.SubPixelMode.value);
            mat.SetFloat("_SubPixelDesnity", settings.SubPixelDensity.value);

            mat.SetFloat("_GlitchChance", settings.GlitchChance.value);
            mat.SetFloat("_GlitchLength", settings.GlitchLength.value);
        }

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
            _mat = null;
            _rp = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_mat == null || _rp == null) return;

            VolumeStack stack = VolumeManager.instance.stack;
            CRTSettings settings = stack.GetComponent<CRTSettings>();
            if (settings == null || !settings.IsActive()) return;

            UpdateMaterialWithSettings(_mat, settings);

            _rp.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_rp);
        }
    }
}