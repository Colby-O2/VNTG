using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTRendererPass.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT
{
    internal sealed class CRTRendererPass : ScriptableRenderPass
    {
        private const string kPassName = "CRT Effect Pass";

#if UNITY_6000_4_OR_NEWER
        private Dictionary<EntityId, RTHandle> _historyBuffers = new();
#else
        private Dictionary<int, RTHandle> _historyBuffers = new();
#endif

#if UNITY_6000_4_OR_NEWER
        private Dictionary<EntityId, CameraRefreshState> _cameraLastTimes = new();
#else
        private Dictionary<int, CameraRefreshState> _cameraLastTimes = new();
#endif

        private Material _material;

        public CRTRendererPass(Material material)
        {
            _material = material;
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material)
        {
            _material = material;
        }

        private void UpdateMaterialWithSettings(Material mat, CRTSettings settings)
        {
            mat.SetFloat("_RefreshRate", settings.EnableScreenSettings.value ? settings.RefreshRate.value : 50);
            mat.SetFloat("_DecayRate", settings.EnableScreenSettings.value ? settings.DecayRate.value : 1f);
            mat.SetVector("_ScreenResolution", settings.EnableScreenSettings.value ? settings.ScreenResolution.value : new Vector2(Screen.width, Screen.height));
            mat.SetInt("_EnableInterlacedRendering", settings.EnableScreenSettings.value ? (settings.EnableInterlacedRendering.value ? 1 : 0) : 0);

            mat.SetInt("_EnableScreenBend", settings.EnableScreenBend.value ? 1 : 0);
            mat.SetFloat("_ScreenBend", settings.ScreenBend.value);

            mat.SetFloat("_ScreenRoundness", settings.EnableVignette.value ? settings.ScreenRoundness.value : 0f);
            mat.SetFloat("_VignetteOpacity", settings.EnableVignette.value ? settings.VignetteOpacity.value : 1f);

            mat.SetVector("_ScanLineOpacity", settings.EnableScanlines.value ? new Vector2(settings.ScanLineVerticalOpacity.value, settings.ScanLineHorizontalOpacity.value) : new Vector2(0f, 0f));
            mat.SetVector("_ScanLineSpeed", settings.EnableScanlines.value ? new Vector2(settings.ScanLineVerticalSpeed.value, settings.ScanLineHorizontalSpeed.value) : new Vector2(0f, 0f));
            mat.SetFloat("_ScanLineStrength", settings.EnableScanlines.value ? settings.ScanLineStrength.value : 0f);

            mat.SetFloat("_NoiseSpeed", settings.EnableNoise.value ? settings.NoiseSpeed.value : 0f);
            mat.SetFloat("_NoiseScale", settings.EnableNoise.value ? settings.NoiseScale.value : 0f);
            mat.SetVector("_NoiseRGBOffset", settings.EnableNoise.value ? new Vector2(settings.NoiseRBGOffsetX.value, settings.NoiseRBGOffsetY.value) : new Vector2(0f, 0f));
            mat.SetFloat("_NoiseFade", settings.EnableNoise.value ? settings.NoiseFade.value : 0f);

            mat.SetFloat("_VHSSmear", settings.EnableVhs.value ? Mathf.Lerp(1f, 0.05f, settings.VhsSmear.value) : 1f);

            mat.SetFloat("_UnsharpAmount", settings.EnableUnsharp.value ? settings.UnsharpAmount.value : 0f);
            mat.SetFloat("_UnsharpRadius", settings.EnableUnsharp.value ? settings.UnsharpRadius.value : 0f);
            mat.SetFloat("_UnsharpThreshold", settings.EnableUnsharp.value ? settings.UnsharpThreshold.value : 0f);

            mat.SetFloat("_ClampBlack", settings.EnableClampColor.value ? settings.ClampBlack.value : 0f);
            mat.SetFloat("_ClampWhite", settings.EnableClampColor.value ? settings.ClampWhite.value : 1f);
            mat.SetColor("_TintShadowsColor", settings.EnableClampColor.value ? settings.ShadowTint.value : Color.black);

            mat.SetInt("_EnableTrackerLine", settings.EnableTrackerLine.value ? 1 : 0);
            mat.SetFloat("_TrackingSpeed", settings.TrackingSpeed.value);
            mat.SetFloat("_TrackingJitter", settings.TrackingJitter.value);

            mat.SetInt("_EnableSignalInterference", settings.EnableSignalInterference.value ? 1 : 0);
            mat.SetFloat("_InterferenceFrequency", settings.InterferenceFrequency.value);
            mat.SetFloat("_InterferenceAmplitude", settings.InterferenceAmplitude.value);

            mat.SetFloat("_ChromaticOffset", settings.EnableChromaticAberration.value ? settings.ChromaticOffset.value : 0f);
            mat.SetFloat("_ChromaticSpeed", settings.EnableChromaticAberration.value ? settings.ChromaticOffsetSpeed.value : 0f);

            mat.SetFloat("_Brightness", settings.EnableColorAdjustment.value ? settings.Brightness.value : 0.5f);
            mat.SetFloat("_Contrast", settings.EnableColorAdjustment.value ? settings.Contrast.value : 0.5f);
            mat.SetFloat("_Saturation", settings.EnableColorAdjustment.value ? settings.Saturation.value : 0.5f);
            mat.SetFloat("_Gamma", settings.EnableColorAdjustment.value ? settings.Gamma.value : 0.5f);
            mat.SetFloat("_Hue", settings.EnableColorAdjustment.value ? settings.Hue.value : 0f);
            mat.SetFloat("_RedShift", settings.EnableColorAdjustment.value ? settings.RedShift.value : 0f);
            mat.SetFloat("_GreenShift", settings.EnableColorAdjustment.value ? settings.GreenShift.value : 0f);
            mat.SetFloat("_BlueShift", settings.EnableColorAdjustment.value ? settings.BlueShift.value : 0f);
            mat.SetInt("_IsMonochrome", settings.EnableColorAdjustment.value ? (settings.IsMonochrome.value ? 1 : 0) : 0);

            mat.SetInt("_SubPixelMode", settings.EnableSubPixels.value ? (int)settings.SubPixelMode.value : 0);
            mat.SetFloat("_SubPixelDesnity", settings.SubPixelDensity.value);

            mat.SetFloat("_GlitchChance", settings.EnableGlitch.value ? settings.GlitchChance.value : 0f);
            mat.SetFloat("_GlitchLength", settings.GlitchLength.value);
        }

#if UNITY_6000_4_OR_NEWER
        private (bool shouldRefresh, int interlaceOffset) ShouldRefresh(EntityId camID, CRTSettings settings)
#else
        private (bool shouldRefresh, int interlaceOffset) ShouldRefresh(int camID, CRTSettings settings)
#endif
        {
            if (!_cameraLastTimes.TryGetValue(camID, out CameraRefreshState state))
            {
                state = new CameraRefreshState { accumulatedTime = 0f, interlaceOffset = 0 };
            }

            if (!settings.EnableScreenSettings.value || settings.UseMaxFPS.value)
            {
                state.interlaceOffset = (state.interlaceOffset + 1) % 2;
                state.accumulatedTime = 0f;

                _cameraLastTimes[camID] = state;

                return (true, state.interlaceOffset);
            }

            float refreshRate = settings.RefreshRate.value;
            float targetInterval = refreshRate > 0f ? (1.0f / refreshRate) : 0.016f;

            state.accumulatedTime += Time.deltaTime;

            bool shouldRefresh = false;

            if (state.accumulatedTime >= targetInterval)
            {
                shouldRefresh = true;
                state.interlaceOffset = (state.interlaceOffset + 1) % 2;
                state.accumulatedTime %= targetInterval;
            }

            _cameraLastTimes[camID] = state;

            return (shouldRefresh, state.interlaceOffset);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            CRTSettings settings = stack.GetComponent<CRTSettings>();
            if (settings == null || !settings.IsActive()) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("Skipping render pass. CRT render requries an intermediate ColorTexture.");
                return;
            }

            TextureHandle src = resourceData.activeColorTexture;

#if UNITY_6000_4_OR_NEWER
            EntityId camID = cameraData.camera.GetEntityId();
#else
            int camID = cameraData.camera.GetInstanceID();
#endif
            (bool shouldRefresh, int interlaceOffset) = ShouldRefresh(camID, settings);

            float forceRefresh = _historyBuffers.ContainsKey(camID) ? 0.0f : 1.0f;
            RTHandle historyRT = GetHistoryBuffer(camID, cameraData, cameraData.cameraTargetDescriptor);
            TextureHandle historyHandle = renderGraph.ImportTexture(historyRT);

            TextureDesc dstDesc = renderGraph.GetTextureDesc(src);
            dstDesc.name = "CRT_Output";
            dstDesc.clearBuffer = false;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(kPassName, out PassData passData))
            {
                passData.src = src;
                passData.history = historyHandle;
                passData.material = _material;
                passData.settings = settings;
                passData.forceRefresh = forceRefresh;
                passData.shouldRefresh = shouldRefresh ? 1.0f : 0.0f;
                passData.interlaceOffset = interlaceOffset;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.UseTexture(passData.history, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {

                    data.material.SetTexture("_PrevFrameTex", data.history);
                    data.material.SetFloat("_ForceRefresh", data.forceRefresh);
                    data.material.SetFloat("_ShouldRefresh", data.shouldRefresh);
                    data.material.SetInt("_InterlaceOffset", data.interlaceOffset);

                    UpdateMaterialWithSettings(data.material, data.settings);

                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            renderGraph.AddBlitPass(dst, historyHandle, Vector2.one, Vector2.zero, passName: "Update History");

            resourceData.cameraColor = dst;
        }

#if UNITY_6000_4_OR_NEWER
        private RTHandle GetHistoryBuffer(EntityId id, UniversalCameraData cameraData, RenderTextureDescriptor desc)
#else
        private RTHandle GetHistoryBuffer(int id, UniversalCameraData cameraData, RenderTextureDescriptor desc)
#endif
        {
            if (!_historyBuffers.TryGetValue(id, out RTHandle historyRT) ||
                historyRT == null ||
                historyRT.rt.width != cameraData.cameraTargetDescriptor.width ||
                historyRT.rt.height != cameraData.cameraTargetDescriptor.height)
            {
                historyRT?.Release();

                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;

                historyRT = RTHandles.Alloc(
                    desc.width,
                    desc.height,
                    colorFormat: desc.graphicsFormat,
                    depthBufferBits: DepthBits.None,
                    name: $"_CRT_History_{id}"
                );

                _historyBuffers[id] = historyRT;
            }
            return _historyBuffers[id];
        }

#if UNITY_6000_4_OR_NEWER
        public void Cleanup(EntityId camID)
#else
        public void Cleanup(int camID)
#endif
        {
            if (_historyBuffers.ContainsKey(camID))
            {
                _historyBuffers[camID]?.Release();
                _historyBuffers.Remove(camID);
            }

            if (_cameraLastTimes.ContainsKey(camID))
            {
                _cameraLastTimes.Remove(camID);
            }
        }

        public void Cleanup()
        {
            foreach (RTHandle rt in _historyBuffers.Values)
            {
                rt?.Release();
            }
            _historyBuffers.Clear();
        }

        private struct CameraRefreshState
        {
            public float accumulatedTime;
            public int interlaceOffset;
        }

        private class PassData
        {
            public TextureHandle src;
            public TextureHandle history;
            public Material material;
            public CRTSettings settings;
            public float forceRefresh;
            public float shouldRefresh;
            public int interlaceOffset;
        }
    }
}