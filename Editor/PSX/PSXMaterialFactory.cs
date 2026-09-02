using System.IO;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXMaterialFactory.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX.Editor
{
    public static class PSXMaterialFactory
    {
        private const string PSX_SHADER_NAME = "VNTG/PSXMaster_URP";
        private const string PSX_TERRIAN_SHADER_NAME = "VNTG/PSXTerrian_URP";
        private const string PSX_WATER_SHADER_NAME = "VNTG/PSXWater_URP";
        private const string PSX_DECAL_SHADER_NAME = "VNTG/PSXDecal_URP";
        private const string PSX_SKYBOX_SHADER_NAME = "VNTG/PSXSkybox_URP";

        [MenuItem("Assets/Create/VNTG/PSX Material", false)]
        public static void CreatePSXMaterial()
        {
            CreateMateral(PSX_SHADER_NAME);
        }

#if UNITY_6000_3_OR_NEWER
        [MenuItem("Assets/Create/VNTG/PSX Terrian Material", false)]
        public static void CreateTerrianMaterial()
        {
            CreateMateral(PSX_TERRIAN_SHADER_NAME);
        }
#endif

        [MenuItem("Assets/Create/VNTG/PSX Water Material", false)]
        public static void CreateWaterMaterial()
        {
            CreateMateral(PSX_WATER_SHADER_NAME);
        }

        [MenuItem("Assets/Create/VNTG/PSX Decal Material", false)]
        public static void CreateDecalMaterial()
        {
            CreateMateral(PSX_DECAL_SHADER_NAME);
        }

        [MenuItem("Assets/Create/VNTG/PSX Skybox Material", false)]
        public static void CreateSkyboxMaterial()
        {
            CreateMateral(PSX_SKYBOX_SHADER_NAME);
        }

        [MenuItem("Assets/VNTG/Convert to PSX Material", false)]
        public static void ConvertToPSXMaterial()
        {
            Material selectedMaterial = Selection.activeObject as Material;

            Shader psxShader = Shader.Find(PSX_SHADER_NAME);
            if (psxShader == null)
            {
                Debug.LogError($"PSXFactory: Could not find shader: '{PSX_SHADER_NAME}'.");
                return;
            }

            string originalShaderName = selectedMaterial.shader.name;
            bool isLit = false;

            if (originalShaderName.Contains("Lit") && !originalShaderName.Contains("Unlit"))
            {
                isLit = true;
            }

            string texturePropertyName = "_MainTex";
            Vector2 originalTiling = Vector2.one;
            Vector2 originalOffset = Vector2.zero;

            if (selectedMaterial.HasProperty(texturePropertyName))
            {
                originalTiling = selectedMaterial.GetTextureScale(texturePropertyName);
                originalOffset = selectedMaterial.GetTextureOffset(texturePropertyName);
            }

            float originalCutoff = 0.5f;
            bool isAlphaClipEnabled = false;

            if (selectedMaterial.HasProperty("_Cutoff"))
            {
                originalCutoff = selectedMaterial.GetFloat("_Cutoff");
            }

            if (selectedMaterial.IsKeywordEnabled("_ALPHATEST_ON"))
            {
                isAlphaClipEnabled = true;
            }

            bool receiveShadows = true;

            if (selectedMaterial.IsKeywordEnabled("_RECEIVE_SHADOWS_OFF"))
            {
                receiveShadows = false;
            }
            else if (selectedMaterial.HasProperty("_ReceiveShadows"))
            {
                receiveShadows = selectedMaterial.GetFloat("_ReceiveShadows") == 1.0f;
            }

            float originalSmoothness = 0.0f;

            if (selectedMaterial.HasProperty("_Smoothness"))
            {
                originalSmoothness = selectedMaterial.GetFloat("_Smoothness");
            }
            else if (selectedMaterial.HasProperty("_Glossiness"))
            {
                originalSmoothness = selectedMaterial.GetFloat("_Glossiness");
            }

            float originalMetallic = 0.0f;
            if (selectedMaterial.HasProperty("_Metallic"))
            {
                originalMetallic = selectedMaterial.GetFloat("_Metallic");
            }

            Undo.RecordObject(selectedMaterial, "Convert Material to PSX");

            selectedMaterial.shader = psxShader;

            if (isLit)
            {
                selectedMaterial.SetLightingMethod(PSXLightingMethod.Lit);
                selectedMaterial.SetFloat("_UseSpecular", 1.0f);
                selectedMaterial.SetFloat("_Smoothness", originalSmoothness);
                selectedMaterial.SetFloat("_Metalness", originalMetallic);
            }
            else
            {
                selectedMaterial.SetLightingMethod(PSXLightingMethod.Unlit);
            }

            selectedMaterial.SetVector("_Tiling", originalTiling);
            selectedMaterial.SetVector("_Offset", originalOffset);

            selectedMaterial.SetFloat("_AlphaClipThreshold", isAlphaClipEnabled ? originalCutoff : 0.0f);

            selectedMaterial.SetFloat("_EnableShadows", receiveShadows ? 1.0f : 0.0f);
            

            EditorUtility.SetDirty(selectedMaterial);
            AssetDatabase.SaveAssets();

            Debug.Log($"Successfully converted '{selectedMaterial.name}' to PSX shader.", selectedMaterial);
        }

        [MenuItem("Assets/VNTG/Convert to PSX Material", true)]
        public static bool ValidateConvertToPSXMaterial()
        {
            if (Selection.activeObject is Material mat)
            {
                return mat.shader.name != PSX_SHADER_NAME;
            }
            return false;
        }

        private static string GetDefaultMaterialName(string shaderName)
        {
            return shaderName switch
            {
                "VNTG/PSXMaster_URP" => "New PSX Material",
                "VNTG/PSXTerrian_URP" => "New PSX Terrain Material",
                "VNTG/PSXWater_URP" => "New PSX Water Material",
                "VNTG/PSXDecal_URP" => "New PSX Decal Material",
                "VNTG/PSXSkybox_URP" => "New PSX Skybox Material",
                _ => "New Material"
            };
        }

        private static void CreateMateral(string shaderName)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            string fullPath = Path.Combine(path, $"{GetDefaultMaterialName(shaderName)}.mat");
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            Shader psxShader = Shader.Find(shaderName);
            if (psxShader == null)
            {
                Debug.LogError($"PSXFactory: Could not find shader: '{shaderName}'. Please check the shader path string in the script.");
                return;
            }

            Material newMat = new Material(psxShader);
            AssetDatabase.CreateAsset(newMat, fullPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newMat;
        }
    }
}