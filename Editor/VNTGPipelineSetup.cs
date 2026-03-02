using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VNTGPipelineSetup.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    public static class VNTGPipelineSetup
    {
        private const string Key = "VNTGPipelineSetup_Asked";

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.delayCall += CheckPipeline;
        }

        private static void CheckPipeline()
        {
            bool alreadySetup = EditorUserSettings.GetConfigValue(Key) == "true";

            if (alreadySetup)
                return;

            bool install = EditorUtility.DisplayDialog(
                "VNTG Render Pipeline Setup",
                "Would you like to switch to the included Render Pipeline asset.",
                "Assign Pipeline",
                "Not Now"
            );

            if (install) AssignPipeline();


            EditorUserSettings.SetConfigValue(Key, "true");
        }

        private static void AssignPipeline()
        {
            string[] guids = AssetDatabase.FindAssets("t:RenderPipelineAsset");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);

                if (pipeline.name != "VNTG_RPAsset") continue;

                if (pipeline != null)
                {

                    Debug.Log(pipeline.name);
                    GraphicsSettings.defaultRenderPipeline = pipeline;
                    QualitySettings.renderPipeline = pipeline;

                    Debug.Log("VNTG Render Pipeline assigned successfully.");
                    return;
                }
            }

            Debug.LogError("No RenderPipelineAsset found in project.");
        }
    }
}