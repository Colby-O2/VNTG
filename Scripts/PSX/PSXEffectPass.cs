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

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            PSXEffectSettings settings = stack.GetComponent<PSXEffectSettings>();
            if (settings == null || !settings.IsActive()) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();


            if ((!settings.ShowInSceneView.value && cameraData.cameraType == CameraType.SceneView) || cameraData.cameraType == CameraType.Preview)
            {
                return;
            }

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

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = dst;
        }

        private class PassData
        {
            public TextureHandle src;
            public Material material;
        }
    }
}