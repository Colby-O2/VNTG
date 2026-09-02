using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXMaterialExtensions.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    public enum PSXTextureSampleMode { Point = 0, Bilinear = 1, N64 = 2 }
    public enum PSXLightingMethod { Unlit = 0, Lit = 1, TexelLit = 2, VertexLit = 3 }
    public enum PSXVertexJitterMode { Disabled = 0, ViewSpace = 1, ScreenSpace = 2 }
    public enum DitherMode { Full, TextureRelative }
    public enum PSXShadingMode { Default = 0, Flat = 1 }
    public enum SkyboxMode { Cubemap = 0, Panoramic = 1, Procedual = 2 }
    public enum ProcedualSkyboxMode { FlatGradient = 0, Space = 1 }

    public static class PSXMaterialExtensions
    {
        private static readonly int TextureSampleModeID = Shader.PropertyToID("_TEXTURESAMPLEMODE");
        private static readonly int LightingMethodID = Shader.PropertyToID("_LIGHTINGMETHOD");
        private static readonly int VertexJitterModeID = Shader.PropertyToID("_VERTEXJITTERMODE");
        private static readonly int DitherModeID = Shader.PropertyToID("_DITHERINGMETHOD");
        private static readonly int SkyboxModeID = Shader.PropertyToID("_SKYBOXMODE");
        private static readonly int ProcedualSkyboxModeID = Shader.PropertyToID("_PROCEDUALSKYBOXMODE");

        public static void SetTextureSampleMode(this Material mat, PSXTextureSampleMode mode)
        {
            if (mat == null) return;
            mat.SetFloat(TextureSampleModeID, (float)mode);
        }

        public static void SetLightingMethod(this Material mat, PSXLightingMethod method)
        {
            if (mat == null) return;
            mat.SetFloat(LightingMethodID, (float)method);
        }

        public static void SetVertexJitterMode(this Material mat, PSXVertexJitterMode mode)
        {
            if (mat == null) return;
            mat.SetFloat(VertexJitterModeID, (float)mode);
        }

        public static void SetDitherMode(this Material mat, DitherMode mode)
        {
            if (mat == null) return;
            mat.SetFloat(DitherModeID, (float)mode);
        }

        public static void SetSkyboxMode(this Material mat, SkyboxMode mode)
        {
            if (mat == null) return;
            mat.SetFloat(SkyboxModeID, (float)mode);
        }

        public static void SetProcedualSkyboxMode(this Material mat, ProcedualSkyboxMode mode)
        {
            if (mat == null) return;
            mat.SetFloat(ProcedualSkyboxModeID, (float)mode);
        }

        public static PSXTextureSampleMode GetTextureSampleMode(this Material mat)
        {
            if (mat == null) return PSXTextureSampleMode.Point;
            return (PSXTextureSampleMode)mat.GetFloat(TextureSampleModeID);
        }

        public static PSXLightingMethod GetLightingMethod(this Material mat)
        {
            if (mat == null) return PSXLightingMethod.Unlit;
            return (PSXLightingMethod)mat.GetFloat(LightingMethodID);
        }

        public static PSXVertexJitterMode GetVertexJitterMode(this Material mat)
        {
            if (mat == null) return PSXVertexJitterMode.Disabled;
            return (PSXVertexJitterMode)mat.GetFloat(VertexJitterModeID);
        }

        public static DitherMode GetDitherMode(this Material mat)
        {
            if (mat == null) return DitherMode.Full;
            return (DitherMode)mat.GetFloat(DitherModeID);
        }

        public static SkyboxMode GetSkybox(this Material mat)
        {
            if (mat == null) return SkyboxMode.Procedual;
            return (SkyboxMode)mat.GetFloat(SkyboxModeID);
        }

        public static ProcedualSkyboxMode GetProcedualSkyboxMode(this Material mat)
        {
            if (mat == null) return ProcedualSkyboxMode.FlatGradient;
            return (ProcedualSkyboxMode)mat.GetFloat(ProcedualSkyboxModeID);
        }
    }
}