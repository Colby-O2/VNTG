using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectPass.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    public class PSXEffectPass : ScriptableRenderPass
    {
        private const string _passName = "PSXEffectPass";
        private Material _material;

        public PSXEffectPass(Material mat)
        {
            _material = mat;
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material)
        {
            _material = material;
        }

        private void UpdateMaterialWithSettings(Material material, PSXEffectSettings settings)
        {
            material.SetVector("_PixelResolution",
                new Vector2(
                    Mathf.Max(1, settings.PixelResolution.value.x),
                    Mathf.Max(1, settings.PixelResolution.value.y)
                )
            );

            material.SetFloat("_ColorPrecision", settings.ColorPrecision.value);

            material.SetFloat("_EnableDither", (settings.EnableDither.value) ? 1 : 0);
            material.SetFloat("_DitherMode", (int)settings.DitherMode.value);
            material.SetInt("_DitherPattern", settings.DitherPattern.value);
            material.SetFloat("_DitherPixelPerfect", settings.DitherPixelPerfect.value ? 1 : 0);
            material.SetFloat("_DitherScale", Mathf.Lerp(1f, 10f, settings.DitherScale.value));
            material.SetFloat("_DitherThreshold", settings.DitherThreshold.value);

            material.SetInt("_EnableFog", (settings.EnableFog.value) ? 1 : 0);
            material.SetColor("_FogColor", settings.FogColor.value);
            material.SetFloat("_FogDensity", settings.FogDensity.value);
            material.SetFloat("_FogEdgeSmoothness", settings.FogEdgeSmoothness.value);
            material.SetFloat("_FogNoiseStrength", settings.FogNoiseStrength.value);
            material.SetFloat("_FogNoiseScale", Mathf.Lerp(1f, 10f, settings.FogNoiseScale.value));
            material.SetFloat("_FogNoiseStart", Mathf.Lerp(0f, 100f, settings.FogNoiseStart.value));
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            PSXEffectSettings settings = stack.GetComponent<PSXEffectSettings>();
            if (settings == null || !settings.IsActive()) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("Skipping render pass. PSX Effect render requries an intermediate ColorTexture.");
                return;
            }

            TextureHandle src = resourceData.activeColorTexture;
            TextureDesc dstDesc = renderGraph.GetTextureDesc(src);
            dstDesc.name = _passName;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(_passName, out PassData passData))
            {
                passData.src = src;
                passData.material = _material;
                passData.settings = settings;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                   UpdateMaterialWithSettings(data.material, data.settings);
                   Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = dst;
        }

        private class PassData
        {
            public TextureHandle src;
            public Material material;
            public PSXEffectSettings settings;
        }
    }
}