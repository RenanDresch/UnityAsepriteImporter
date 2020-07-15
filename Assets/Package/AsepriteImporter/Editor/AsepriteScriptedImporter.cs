using UnityEditor.Experimental.AssetImporters;
using System.IO;
using AsepriteImporter.Runtime.Data;
using UnityEngine;
using AsepriteImporter.Runtime.Enums;

[ScriptedImporter(1, new string[] { "aseprite", "ase" })]
public class AsepriteScriptedImporter : ScriptedImporter
{
    #region Properties

    public PivotPositionEnum Pivot;
    public Vector2 PivotPosition;
    public Aseprite Aseprite;
    public AseImportOptions MergedLayersImportOption;
    public AseImportOptions SeparateLayersImportOption;

    #endregion

    public override void OnImportAsset(AssetImportContext ase)
    {
        switch(Pivot)
        {
            case PivotPositionEnum.TopLeft:
                PivotPosition = new Vector2(0, 1);
                break;
            case PivotPositionEnum.TopCenter:
                PivotPosition = new Vector2(.5f, 1);
                break;
            case PivotPositionEnum.TopRight:
                PivotPosition = new Vector2(1, 1);
                break;
            case PivotPositionEnum.MidleLeft:
                PivotPosition = new Vector2(0, .5f);
                break;
            case PivotPositionEnum.MiddleCenter:
                PivotPosition = new Vector2(.5f, .5f);
                break;
            case PivotPositionEnum.MiddleRight:
                PivotPosition = new Vector2(1, .5f);
                break;
            case PivotPositionEnum.BottomLeft:
                PivotPosition = new Vector2(0, 0);
                break;
            case PivotPositionEnum.BottomCenter:
                PivotPosition = new Vector2(.5f, 0);
                break;
            case PivotPositionEnum.BottomRight:
                PivotPosition = new Vector2(1, 0);
                break;
        }

        Aseprite = new Aseprite(File.ReadAllBytes(ase.assetPath), MergedLayersImportOption,
            SeparateLayersImportOption, PivotPosition, Path.GetFileNameWithoutExtension(ase.assetPath));

        if((int)SeparateLayersImportOption > 0)
        {
            foreach(var texture in Aseprite.LayersAtlas)
            {
                ase.AddObjectToAsset(texture.name, texture);
            }
            if ((int)SeparateLayersImportOption > 1)
            {
                foreach (var sprite in Aseprite.LayerSprites)
                {
                    ase.AddObjectToAsset(sprite.name, sprite);
                }
            }
        }

        ase.AddObjectToAsset("Palette", Aseprite.ColorPalette);

        if ((int)MergedLayersImportOption > 0)
        {
            ase.AddObjectToAsset(Aseprite.Atlas.name, Aseprite.Atlas, Aseprite.Atlas);
            ase.SetMainObject(Aseprite.Atlas);
            if ((int)MergedLayersImportOption > 1)
            {
                foreach (var sprite in Aseprite.Sprites)
                {
                    ase.AddObjectToAsset(sprite.name, sprite);
                }
            }
        }
        else
        {
            ase.SetMainObject(Aseprite.ColorPalette);
        }
    }
}