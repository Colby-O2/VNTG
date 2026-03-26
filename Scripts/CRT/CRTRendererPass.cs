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

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.UseTexture(passData.history, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {

                    data.material.SetTexture("_PrevFrameTex", data.history);
                    data.material.SetFloat("_ForceRefresh", data.forceRefresh);
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
        }

        public void Cleanup()
        {
            foreach (RTHandle rt in _historyBuffers.Values)
            {
                rt?.Release();
            }
            _historyBuffers.Clear();
        }

        private class PassData
        {
            public TextureHandle src;
            public TextureHandle history;
            public Material material;
            public CRTSettings settings;
            public float forceRefresh;
        }
    }
}