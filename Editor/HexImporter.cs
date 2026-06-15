using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    HexImporter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Editor
{
    [ScriptedImporter(1, "hex")]
    public class HexImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string text = File.ReadAllText(ctx.assetPath);

            TextAsset subAsset = new TextAsset(text);

            ctx.AddObjectToAsset("main obj", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
