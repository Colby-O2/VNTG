using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

using System.Collections.Generic;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTRendererPass.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT
{
    internal sealed class CRTRendererPass : ScriptableRenderPass
    {
        private const string kPassName = "CRT Effect Pass";
        private readonly Material _material;
        private Dictionary<int, RTHandle> _historyBuffers = new();
        private Dictionary<int, int> _lastInteralceOffset = new();

        public CRTRendererPass(Material material)
        {
            _material = material;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            CRTSettings settings = stack.GetComponent<CRTSettings>();
            if (settings == null || !settings.IsActive()) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();


            if ((!settings.ShowInSceneView.value && cameraData.cameraType == CameraType.SceneView) || cameraData.cameraType == CameraType.Preview)
            {
                return;
            }

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("Skipping render pass. CRT render requries an intermediate ColorTexture.");
                return;
            }

            TextureHandle src = resourceData.activeColorTexture;

            int camID = cameraData.camera.GetInstanceID();
            RTHandle historyRT = GetHistoryBuffer(camID, cameraData, cameraData.cameraTargetDescriptor);
            TextureHandle historyHandle = renderGraph.ImportTexture(historyRT);

            TextureDesc dstDesc = renderGraph.GetTextureDesc(src);
            dstDesc.name = "CRT_Output";
            dstDesc.clearBuffer = false;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(kPassName, out PassData passData))
            {
                if (!_lastInteralceOffset.ContainsKey(camID)) _lastInteralceOffset[camID] = 0;
                passData.src = src;
                passData.history = historyHandle;
                passData.material = _material;
                passData.interlaceOffset = ((int)(Time.time * (float)settings.RefreshRate.value)) % 2;
                passData.refresh = (_lastInteralceOffset[camID] != passData.interlaceOffset) ? 1 : 0;
                passData.deltaTime = Time.deltaTime;
                passData.iTime = Time.time;
                _lastInteralceOffset[camID] = passData.interlaceOffset;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.UseTexture(passData.history, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {

                    data.material.SetTexture("_PrevFrameTex", data.history);
                    data.material.SetInt("_Refresh", data.refresh);
                    data.material.SetInt("_InterlaceOffset", data.interlaceOffset);
                    data.material.SetFloat("_DeltaTime", data.deltaTime);
                    data.material.SetFloat("_iTime", data.iTime);

                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            renderGraph.AddBlitPass(dst, historyHandle, Vector2.one, Vector2.zero, passName: "Update History");

            resourceData.cameraColor = dst;
        }

        private RTHandle GetHistoryBuffer(int id, UniversalCameraData cameraData, RenderTextureDescriptor desc)
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

        private class PassData
        {
            public TextureHandle src;
            public TextureHandle history;
            public Material material;
            public int interlaceOffset;
            public int refresh;
            public float iTime;
            public float deltaTime;
        }
    }
}