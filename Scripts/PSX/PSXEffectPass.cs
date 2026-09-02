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

            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        ~PSXEffectPass()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        public void Setup(Material material)
        {
            _material = material;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            PSXEffectSettings settings = stack.GetComponent<PSXEffectSettings>();

            if (camera.cameraType == CameraType.Game || (settings.ShowInSceneView.value && camera.cameraType == CameraType.SceneView))
            {
                if (settings != null && settings.IsActive() && settings.EnablePixelation.value && settings.EnableLetterbox.value)
                {
                    if (settings.PixelResolution.value.y > 0)
                    {
                        float targetAspect = settings.PixelResolution.value.x / settings.PixelResolution.value.y;

                        camera.projectionMatrix = Matrix4x4.Perspective(
                            camera.fieldOfView,
                            targetAspect,
                            camera.nearClipPlane,
                            camera.farClipPlane
                        );
                    }
                }
                else
                {
                    camera.ResetProjectionMatrix();
                }
            }
        }

        private void UpdateMaterialWithSettings(Material material, PSXEffectSettings settings, Vector2 screenParams)
        {
            material.SetVector("_VNTGScreenParams", screenParams);

            material.SetFloat("_EnablePixelation", settings.EnablePixelation.value ? 1 : 0);
            material.SetFloat("_EnableLetterbox", settings.EnableLetterbox.value ? 1 : 0);
            material.SetVector("_PixelResolution",
                new Vector2(
                    Mathf.Max(1, settings.PixelResolution.value.x),
                    Mathf.Max(1, settings.PixelResolution.value.y)
                )
            );

            material.SetFloat("_EnableColorPrecision", (settings.EnableColorPrecision.value) ? 1 : 0);
            material.SetFloat("_ColorPrecision", settings.ColorPrecision.value);

            material.SetFloat("_EnableDither", (settings.EnableDither.value) ? 1 : 0);
            material.SetFloat("_DitherMode", (int)settings.DitherMode.value);
            material.SetInt("_DitherPattern", settings.DitherPattern.value);
            material.SetFloat("_DitherPixelPerfect", settings.DitherDisplayMode.value == PSXDitherDisplayMode.PixelResolutionRelative ? 1 : 0);
            material.SetFloat("_DitherRelativeScale", settings.DitherDisplayMode.value == PSXDitherDisplayMode.RelativeScale ? 1 : 0);
            material.SetVector("_DitherReferenceResolution", settings.DitherReferenceRes.value);
            material.SetFloat("_DitherScale", Mathf.Lerp(1, 50, settings.DitherScale.value));
            material.SetFloat("_DitherThreshold", settings.DitherThreshold.value);

            material.SetInt("_EnableFog", (settings.EnableFog.value) ? 1 : 0);
            material.SetFloat("_IgnoreSkybox", (settings.IgnoreSkybox.value) ? 1 : 0);
            material.SetColor("_FogColor", settings.FogColor.value);
            material.SetFloat("_FogDensity", settings.FogDensity.value);
            material.SetFloat("_FogEdgeSmoothness", settings.FogEdgeSmoothness.value);
            material.SetFloat("_FogNoiseStrength", settings.FogNoiseStrength.value);
            material.SetFloat("_FogNoiseScale", Mathf.Lerp(1f, 10f, settings.FogNoiseScale.value));
            material.SetFloat("_FogNoiseStart", Mathf.Lerp(0f, 100f, settings.FogNoiseStart.value));

            bool usePalette = settings.EnableColorPalette.value;
            material.SetFloat("_EnablePalette", (usePalette && settings.PaletteAsset.value != null) ? 1f : 0f);
            material.SetFloat("_PaletteNormalizeLuminance", settings.NormalizeLuminanceBeforeSampling.value ? 1f : 0f);
            material.SetFloat("_PreserveLighting", settings.PreserveLighting.value ? 1f : 0f);

            if (usePalette && settings.PaletteAsset.value != null && settings.PaletteAsset.value.LUT != null)
            {
                material.SetTexture("_PaletteLUT", settings.PaletteAsset.value.LUT);
            }
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
            dstDesc.filterMode = FilterMode.Point;
            dstDesc.width = Screen.width;
            dstDesc.height = Screen.height;
            dstDesc.name = _passName;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            Vector2 screenParams = new Vector2(Screen.width, Screen.height);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(_passName, out PassData passData))
            {
                passData.src = src;
                passData.material = _material;
                passData.settings = settings;
                passData.screenParams = screenParams;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                   UpdateMaterialWithSettings(data.material, data.settings, data.screenParams);
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
            public Vector2 screenParams;
        }
    }
}