using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteImporterEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    [CustomEditor(typeof(PaletteImporter))]
    public class PaletteImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Extract to Editable Palette", GUILayout.Height(30)))
            {
                ExtractToStandaloneAsset();
            }

            ApplyRevertGUI();
        }

        private void ExtractToStandaloneAsset()
        {
            ScriptedImporter importer = target as ScriptedImporter;
            if (importer == null) return;

            Palette sourceAsset = AssetDatabase.LoadAssetAtPath<Palette>(importer.assetPath);
            if (sourceAsset == null)
            {
                Debug.LogError("Could not find a valid Palette to duplicate.");
                return;
            }

            string directory = Path.GetDirectoryName(importer.assetPath);
            string fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
            string targetPath = Path.Combine(directory, $"{fileName}_Clone.asset");

            targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);

            Palette editableClone = Instantiate(sourceAsset);

            AssetDatabase.CreateAsset(editableClone, targetPath);

            if (sourceAsset.LUT != null)
            {
                Texture3D lutClone = Instantiate(sourceAsset.LUT);
                lutClone.name = sourceAsset.LUT.name;

                AssetDatabase.AddObjectToAsset(lutClone, editableClone);

                editableClone.UpdateLUT(lutClone, sourceAsset.RawLUT);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ProjectWindowUtil.ShowCreatedAsset(editableClone);

            Debug.Log($"Successfully created editable asset at: {targetPath}");
        }
    }
}