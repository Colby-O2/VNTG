using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VNTGShaderSetup.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    public static class VNTGShaderSetup
    {
        private const string Key = "VNTGShaderSetup_Asked";
        private static readonly string[] TargetShaderNames = { "Hidden/PSXMaster_URP", "Hidden/CRTFilter_URP" };

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.delayCall += CheckShaderSetup;
        }

        private static void CheckShaderSetup()
        {
            if (EditorUserSettings.GetConfigValue(Key) == "true")
                return;

            AddShadersToAlwaysIncluded();

            EditorUserSettings.SetConfigValue(Key, "true");
        }

        private static void AddShadersToAlwaysIncluded()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (graphicsSettingsObj == null) return;

            SerializedObject graphicsSettings = new SerializedObject(graphicsSettingsObj);
            SerializedProperty alwaysIncluded = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");

            bool changed = false;

            foreach (string shaderName in TargetShaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogWarning($"VNTG Setup: Could not find shader '{shaderName}'");
                    continue;
                }

                bool alreadyPresent = false;
                for (int i = 0; i < alwaysIncluded.arraySize; i++)
                {
                    if (alwaysIncluded.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        alreadyPresent = true;
                        break;
                    }
                }

                if (!alreadyPresent)
                {
                    int index = alwaysIncluded.arraySize;
                    alwaysIncluded.InsertArrayElementAtIndex(index);
                    alwaysIncluded.GetArrayElementAtIndex(index).objectReferenceValue = shader;
                    changed = true;
                }
            }

            if (changed)
            {
                graphicsSettings.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log("VNTG: Shaders added to Always Included list successfully.");
            }
            else
            {
                Debug.Log("VNTG: Shaders were already in the list.");
            }
        }
    }
}