using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectPass.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    public class PSXEffectPass : ScriptableRenderPass
    {
        private static readonly int AmbientColorID = Shader.PropertyToID("_VNTGAmbientColor");
        private const int LUT_DIM = 32;

        private const string _passName = "PSXEffectPass";
        private Material _material;

#if UNITY_6000_4_OR_NEWER
        private Dictionary<EntityId, CameraPaletteCache> _perCameraCache = new();
#else
        private Dictionary<int, CameraPaletteCache> _perCameraCache = new();
#endif

        public PSXEffectPass(Material mat)
        {
            _material = mat;
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material)
        {
            _material = material;
        }

        public void ClearCache()
        {
            foreach (var kvp in _perCameraCache)
            {
                if (kvp.Value.runtimeLUT != null) Object.DestroyImmediate(kvp.Value.runtimeLUT);
            }
            _perCameraCache.Clear();
        }

#if UNITY_6000_4_OR_NEWER
        private void UpdateMaterialWithSettings(Material material, PSXEffectSettings settings, EntityId camID)
#else

        private void UpdateMaterialWithSettings(Material material, PSXEffectSettings settings, int camID)
#endif
        {
            material.SetFloat("_EnablePixelation", (settings.EnablePixelation.value) ? 1 : 0);
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
            material.SetFloat("_DitherPixelPerfect", settings.DitherPixelPerfect.value ? 1 : 0);
            material.SetFloat("_DitherScale", Mathf.Lerp(1f, 10f, settings.DitherScale.value));
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
            material.SetFloat("_EnablePalette", usePalette ? 1f : 0f);

            if (!usePalette) return;

            if (!_perCameraCache.TryGetValue(camID, out CameraPaletteCache cache))
            {
                cache = new CameraPaletteCache();
                _perCameraCache[camID] = cache;
            }

            PSXPaletteInputMode currentMode = settings.PaletteInputMode.value;

            switch (currentMode)
            {
                case PSXPaletteInputMode.Texture:
                    Texture2D srcTex = settings.PaletteTexture.value as Texture2D;
                    if (srcTex != null && (cache.lastTexture == null || srcTex != cache.lastTexture || cache.lastInputMode != currentMode))
                    {
                        cache.lastTexture = srcTex;
                        BakeExternalTextureTo3DLUT(srcTex, cache);
                    }
                    break;

                case PSXPaletteInputMode.ColorList:
                    List<Color> colors = settings.PaletteColorList.value;
                    int listHash = GetColorListHash(colors);
                    if (listHash != cache.colorListHash || cache.lastInputMode != currentMode)
                    {
                        cache.colorListHash = listHash;
                        BakeColorsTo3DLUT(colors, cache);
                    }
                    break;

                case PSXPaletteInputMode.HexFile:
                    TextAsset fileAsset = settings.HexFileAsset.value;
                    if (fileAsset != null)
                    {
                        string fileText = fileAsset.text;
                        if (fileAsset != cache.lastHexAsset || fileText != cache.lastHexText || cache.lastInputMode != currentMode)
                        {
                            cache.lastHexAsset = fileAsset;
                            cache.lastHexText = fileText;
                            ParseAndBakeHex(fileText, cache);
                        }
                    }
                    break;
            }

            cache.lastInputMode = currentMode;
            if (cache.runtimeLUT != null)
            {
                material.SetTexture("_PaletteLUT", cache.runtimeLUT);
            }
        }

        private void ParseAndBakeHex(string hexData, CameraPaletteCache cache)
        {
            string[] tokens = hexData.Split(new char[] { ',', '\n', '\r', ' ', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<Color> colors = new List<Color>();
            foreach (string token in tokens)
            {
                string cleanHex = token.Trim().Replace("#", "");
                if (ColorUtility.TryParseHtmlString("#" + cleanHex, out Color color))
                {
                    colors.Add(color);
                }
            }
            BakeColorsTo3DLUT(colors, cache);
        }

        private void BakeExternalTextureTo3DLUT(Texture2D sourceTex, CameraPaletteCache cache)
        {
            if (sourceTex == null) return;

            Texture2D readableTex = sourceTex;
            bool wasUnreadable = !sourceTex.isReadable;
            if (wasUnreadable)
            {
                RenderTexture tmp = RenderTexture.GetTemporary(sourceTex.width, sourceTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                Graphics.Blit(sourceTex, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                readableTex = new Texture2D(sourceTex.width, sourceTex.height);
                readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                readableTex.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);
            }

            int width = readableTex.width;
            int height = readableTex.height;
            Color32[] rawPixels = readableTex.GetPixels32();

            List<Color> paletteColors = new List<Color>();
            HashSet<Color32> encounteredColors = new HashSet<Color32>();

            int targetY = height / 2;
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = targetY * width + x;
                Color32 c = rawPixels[pixelIndex];

                if (c.a == 0) continue;

                if (!encounteredColors.Contains(c))
                {
                    encounteredColors.Add(c);
                    paletteColors.Add(c);
                }
            }

            if (wasUnreadable) Object.DestroyImmediate(readableTex);

            BakeColorsTo3DLUT(paletteColors, cache);
        }

        private void BakeColorsTo3DLUT(List<Color> palette, CameraPaletteCache cache)
        {
            if (palette == null || palette.Count == 0) return;
            if (cache.runtimeLUT != null) Object.DestroyImmediate(cache.runtimeLUT);

            cache.runtimeLUT = new Texture3D(LUT_DIM, LUT_DIM, LUT_DIM, TextureFormat.RGBA32, false);
            cache.runtimeLUT.filterMode = FilterMode.Point;
            cache.runtimeLUT.wrapMode = TextureWrapMode.Clamp;

            Color32[] lutColors = new Color32[LUT_DIM * LUT_DIM * LUT_DIM];
            Color32[] palette32 = new Color32[palette.Count];
            for (int i = 0; i < palette.Count; i++) palette32[i] = palette[i];

            for (int r = 0; r < LUT_DIM; r++)
            {
                for (int g = 0; g < LUT_DIM; g++)
                {
                    for (int b = 0; b < LUT_DIM; b++)
                    {
                        float targetR = (float)r / (LUT_DIM - 1);
                        float targetG = (float)g / (LUT_DIM - 1);
                        float targetB = (float)b / (LUT_DIM - 1);

                        int bestIndex = 0;
                        float minDistance = float.MaxValue;

                        for (int i = 0; i < palette32.Length; i++)
                        {
                            float dR = targetR - (palette32[i].r / 255f);
                            float dG = targetG - (palette32[i].g / 255f);
                            float dB = targetB - (palette32[i].b / 255f);
                            float distSq = dR * dR + dG * dG + dB * dB;

                            if (distSq < minDistance)
                            {
                                minDistance = distSq;
                                bestIndex = i;
                            }
                        }

                        int index = r + (g * LUT_DIM) + (b * LUT_DIM * LUT_DIM);
                        lutColors[index] = palette32[bestIndex];
                    }
                }
            }

            cache.runtimeLUT.SetPixels32(lutColors);
            cache.runtimeLUT.Apply();
        }

        private int GetColorListHash(List<Color> colors)
        {
            if (colors == null) return 0;
            int hash = 17;
            foreach (Color c in colors) hash = hash * 31 + c.GetHashCode();
            return hash;
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

#if UNITY_6000_4_OR_NEWER
            EntityId camID = cameraData.camera.GetEntityId();
#else
            int camID = cameraData.camera.GetInstanceID();
#endif

            TextureHandle src = resourceData.activeColorTexture;
            TextureDesc dstDesc = renderGraph.GetTextureDesc(src);
            dstDesc.name = _passName;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(_passName, out PassData passData))
            {
                passData.src = src;
                passData.material = _material;
                passData.settings = settings;
                passData.camID = camID;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                   UpdateMaterialWithSettings(data.material, data.settings, data.camID);
                   Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = dst;
        }

        private class CameraPaletteCache
        {
            public Texture3D runtimeLUT;
            public Texture2D lastTexture;
            public int colorListHash;
            public TextAsset lastHexAsset;
            public string lastHexText;
            public PSXPaletteInputMode lastInputMode;
        }

        private class PassData
        {
            public TextureHandle src;
            public Material material;
            public PSXEffectSettings settings;
#if UNITY_6000_4_OR_NEWER
            public EntityId camID;
#else
            public int camID;
#endif
        }
    }
}