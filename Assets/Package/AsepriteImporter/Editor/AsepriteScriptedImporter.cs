using UnityEditor.Experimental.AssetImporters;
using System.IO;
using AsepriteImporter.Runtime.Data;

[ScriptedImporter(1, new string[] { "aseprite", "ase" })]
public class AsepriteScriptedImporter : ScriptedImporter
{
    public Aseprite Aseprite;

    public override void OnImportAsset(AssetImportContext ase)
    {
        Aseprite = new Aseprite(File.ReadAllBytes(ase.assetPath));

        ase.AddObjectToAsset(Aseprite.Atlas.name, Aseprite.Atlas, Aseprite.Atlas);
        ase.SetMainObject(Aseprite.Atlas);

        ase.AddObjectToAsset("Palette", Aseprite.ColorPalette);

        foreach(var sprite in Aseprite.Sprites)
        {
            ase.AddObjectToAsset(sprite.name, sprite);
        }
    }
}