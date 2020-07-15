using UnityEditor.Experimental.AssetImporters;
using System.IO;
using AsepriteImporter.Runtime.Data;
using UnityEngine;
using AsepriteImporter.Runtime.Enums;
using UnityEditor;

[ScriptedImporter(1, new string[] { "aseprite", "ase" })]
public class AsepriteScriptedImporter : ScriptedImporter
{
    #region Properties

    public PivotPositionEnum Pivot = PivotPositionEnum.MiddleCenter;
    public Vector2 PivotPosition = new Vector2(0.5f, 0.5f);
    public Aseprite Aseprite;
    public AseImportOptions MergedLayersImportOption = AseImportOptions.Animations;
    public AseImportOptions SeparateLayersImportOption = AseImportOptions.None;

    #endregion

    private void AddAnimation(string prefix, Sprite[] sprites, AssetImportContext ase)
    {
        if (Aseprite.Frames[0].Tags != null)
        {
            foreach (var tag in Aseprite.Frames[0].Tags)
            {
                var clip = new AnimationClip();
                var keyframes = new ObjectReferenceKeyframe[tag.ToFrame - tag.FromFrame + 2];

                clip.name = prefix + tag.Name;

                clip.frameRate = 100;

                AnimationClipSettings clipSettings = new AnimationClipSettings();
                clipSettings.loopTime = true;

                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

                EditorCurveBinding spriteBinding = new EditorCurveBinding();
                spriteBinding.type = typeof(SpriteRenderer);
                spriteBinding.path = "";
                spriteBinding.propertyName = "m_Sprite";

                var kfIndex = 0;
                float currentTime = 0;

                for (var f = tag.FromFrame; f <= tag.ToFrame; f++)
                {
                    keyframes[kfIndex] = new ObjectReferenceKeyframe();
                    keyframes[kfIndex].time = currentTime;
                    keyframes[kfIndex].value = sprites[f];
                    currentTime += (float)Aseprite.Frames[f].FrameDuration / 1000;

                    kfIndex++;

                    if (f == tag.ToFrame)
                    {
                        currentTime -= (1 / 1000);
                        keyframes[kfIndex] = new ObjectReferenceKeyframe();
                        keyframes[kfIndex].time = currentTime;
                        keyframes[kfIndex].value = sprites[f];
                    }
                }

                AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
                ase.AddObjectToAsset(clip.name, clip);
            }
        }
    }

    public override void OnImportAsset(AssetImportContext ase)
    {
        switch (Pivot)
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

        if ((int)SeparateLayersImportOption > 0)
        {
            for (var l = 0; l < Aseprite.Layers.Length; l++)
            {
                ase.AddObjectToAsset(Aseprite.Layers[l].Texture.name, Aseprite.Layers[l].Texture);
                if ((int)SeparateLayersImportOption > 1)
                {
                    foreach (var sprite in Aseprite.Layers[l].Sprites)
                    {
                        ase.AddObjectToAsset(sprite.name, sprite);
                    }
                    if ((int)SeparateLayersImportOption > 2)
                    {
                        AddAnimation($"Layer_{l}_", Aseprite.Layers[l].Sprites, ase);
                    }
                }
            }
        }

        if (Aseprite.ColorPalette != null)
        {
            ase.AddObjectToAsset("Palette", Aseprite.ColorPalette);
        }

        if ((int)MergedLayersImportOption > 0)
        {
            ase.AddObjectToAsset(Aseprite.MergedLayer.Texture.name, Aseprite.MergedLayer.Texture);
            ase.SetMainObject(Aseprite.MergedLayer.Texture);
            if ((int)MergedLayersImportOption > 1)
            {
                foreach (var sprite in Aseprite.MergedLayer.Sprites)
                {
                    ase.AddObjectToAsset(sprite.name, sprite);
                }
                if ((int)MergedLayersImportOption > 2)
                {
                    AddAnimation("", Aseprite.MergedLayer.Sprites, ase);
                }
            }
        }
        else
        {
            if (Aseprite.ColorPalette != null)
            {
                ase.SetMainObject(Aseprite.ColorPalette);
            }
        }
    }
}