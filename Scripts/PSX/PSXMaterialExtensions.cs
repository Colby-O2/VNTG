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

    public static class PSXMaterialExtensions
    {
        private static readonly int TextureSampleModeID = Shader.PropertyToID("_TEXTURESAMPLEMODE");
        private static readonly int LightingMethodID = Shader.PropertyToID("_LIGHTINGMETHOD");
        private static readonly int VertexJitterModeID = Shader.PropertyToID("_VERTEXJITTERMODE");

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
    }
}
