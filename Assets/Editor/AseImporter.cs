using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine.Rendering;
using System.Text;

public enum ColorDepth : UInt16
{
    Undefined = 0,
    RGBA = 32,
    Grayscale = 16,
    Indexed = 8
}

public enum CelType : UInt16
{
    Raw = 0,
    Linked = 1,
    CompressedImage = 2,
}

public enum ChunkType : UInt16
{
    Undefined = 0,
    RealyOldPaletteChunk = 0x0004,
    OldPaletteChunk = 0x0011,
    LayerChunk = 0x2004,
    CelChunk = 0x2005,
    CelExtraChunk = 0x2006,
    ColorProfileChunk = 0x2007,
    MaskChunkDeprecated = 0x2016,
    PathChunk = 0x2017,
    TagsChunk = 0x2018,
    PaletteChunk = 0x2019,
    UserDataChunk = 0x2020,
    SliceChunk = 0x2022
}

public enum LayerFlags : UInt16
{
    Undefined = 0,
    Visible = 1,
    Editable = 2,
    LockMovement = 3,
    Background = 4,
    PreferLinkedCels = 5,
    Collapsed = 32,
    RefLayer = 64
}

public enum LayerType : UInt16
{
    Normal = 0,
    Group = 1
}

public enum BlendMode : UInt16
{
    Normal = 0,
    Multiply = 1,
    Screen = 2,
    Overlay = 3,
    Darken = 4,
    Lighten = 5,
    ColorDodge = 6,
    ColorBurn = 7,
    HardLight = 8,
    SoftLight = 9,
    Difference = 10,
    Exclusion = 11,
    Hue = 12,
    Saturation = 13,
    Color = 14,
    Luminosity = 15,
    Addition = 16,
    Subtract = 17,
    Divide = 18,
}

public class AseRawCel
{
    public AseRawCel(byte[] pixels, int cellWidth, int cellHeight, int xOffset, int yOffset,
        int textureWidth, int textureHeight, ColorDepth depth, int frameIndex, int celIndex, AssetImportContext ase)
    {
        var celTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        celTexture.name = $"Frame_{frameIndex}_Cel_{celIndex}";
        celTexture.filterMode = FilterMode.Point;

        var colors = new List<Color32>();

        for (var r = cellHeight - 1; r > -1; r--)
        {
            for (var c = 0; c < textureWidth; c++)
            {
                if (r > textureHeight - yOffset)
                {
                    colors.Add(new Color32(0, 0, 0, 0));
                }
                else
                {
                    if (c >= (cellWidth+xOffset) || c < xOffset)
                    {
                        colors.Add(new Color32(0, 0, 0, 0));
                    }
                    else
                    {
                        colors.Add(new Color32(pixels[(r * (4 * cellWidth)) + ((c-xOffset) * 4) + 0],
                            pixels[(r * (4 * cellWidth)) + ((c - xOffset) * 4) + 1],
                            pixels[(r * (4 * cellWidth)) + ((c - xOffset) * 4) + 2],
                            pixels[(r * (4 * cellWidth)) + ((c - xOffset) * 4) + 3]));
                    }
                }
            }
        }

        while (colors.Count < textureWidth * textureHeight)
        {
            colors.Add(new Color32(0, 0, 0, 0));
        }

        celTexture.SetPixels32(colors.ToArray());
        ase.AddObjectToAsset(celTexture.name, celTexture);

    }
}

public class AseCel
{
    public AseCel(BinaryReader reader, Aseprite file, int frameIndex, int celIndex, AssetImportContext ase)
    {
        var layerIndex = reader.ReadUInt16();
        var xPosition = reader.ReadInt16();
        var yPosition = reader.ReadInt16();
        var opacity = reader.ReadByte();
        var cellType = (CelType)reader.ReadUInt16();

        reader.ReadBytes(7); //For future

        switch (cellType)
        {
            case CelType.Raw:
                Debug.Log("Raw Cel");
                //cel = new AseRawCel(reader, file.ColorDepth, file.Width, file.Height, ase);
                break;

            case CelType.Linked:
                Debug.Log("Linked Cel");
                reader.Close();
                throw new System.NotImplementedException("Linked cel not supported! Aborting!");
                break;

            case CelType.CompressedImage:
                Debug.Log("Compressed Image Cel");

                var celWidth = reader.ReadUInt16();
                var celHeight = reader.ReadUInt16();

                byte[] celData = new byte[celWidth * celHeight * 4];

                reader.ReadBytes(2);

                var deflate = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                deflate.Read(celData, 0, celWidth * celHeight * 4);

                new AseRawCel(celData, celWidth, celHeight, xPosition, yPosition, file.Header.Width, file.Header.Height, file.Header.ColorDepth, frameIndex, celIndex, ase);

                break;

            default:
                reader.Close();
                throw new System.NotImplementedException("Undefined cel not supported! Aborting!");
                break;
        }
    }
}

public class AseColor
{
    #region Properties

    public Color32 Color { get; }
    public string Name { get; }

    #endregion

    public AseColor(byte r, byte g, byte b, byte a, byte[] name)
    {
        Color = new Color32(r, g, b, a);
        if (name != null)
        {
            Name = BitConverter.ToString(name);
        }
    }
}

public class AseHeader
{
    #region Properties

    public UInt16 MagicNumber { get; }
    public UInt16 FrameCount { get; }
    public UInt16 Width { get; }
    public UInt16 Height { get; }
    public ColorDepth ColorDepth { get; }
    public Byte PaletteEntryIndex { get; }
    public UInt16 Colors { get; }

    #endregion

    public AseHeader(BinaryReader reader, AssetImportContext ase)
    {
        var fileSize = reader.ReadUInt32();

        MagicNumber = reader.ReadUInt16();
        if (MagicNumber != 0xA5E0)
        {
            return;
        }

        FrameCount = reader.ReadUInt16();
        Width = reader.ReadUInt16();
        Height = reader.ReadUInt16();
        ColorDepth = (ColorDepth)reader.ReadUInt16();
        var flags = reader.ReadUInt32();
        var deprecatedSpeed = reader.ReadUInt16();
        reader.ReadUInt32(); //Set to 0
        reader.ReadUInt32(); //Set to 0 
        PaletteEntryIndex = reader.ReadByte();
        reader.ReadBytes(3); //Ignore
        Colors = reader.ReadUInt16();
        var pixelWidth = reader.ReadByte();
        var pixelHeight = reader.ReadByte();
        var gridXPosition = reader.ReadInt16();
        var gridYPosition = reader.ReadInt16();
        var gridWidth = reader.ReadUInt16();
        var gridHeight = reader.ReadUInt16();
        reader.ReadBytes(84); //For future
    }
}


public class AseLayer
{
    #region Properties

    public LayerFlags Flags { get; }
    public LayerType LayerType { get; }
    public int LayerChildLevel { get; }
    public BlendMode BlendMode { get; }
    public int Opacity { get; }
    public string Name { get; }

    #endregion

    public AseLayer(BinaryReader reader, Aseprite file, AssetImportContext ase)
    {
        Flags = (LayerFlags)reader.ReadUInt16();
        LayerType = (LayerType)reader.ReadUInt16();
        LayerChildLevel = reader.ReadUInt16();
        var layerWidth = reader.ReadUInt16(); //Ignored;
        var layerHeight = reader.ReadUInt16(); //Ignored;
        BlendMode = (BlendMode)reader.ReadUInt16();
        Opacity = reader.ReadByte();
        reader.ReadBytes(3); //For Future
        var nameSize = reader.ReadUInt16();
        var name = reader.ReadBytes(nameSize);
        Name = Encoding.UTF8.GetString(name, 0, nameSize);
    }
}

public class AseFrame
{
    #region Properties

    public int FrameSize { get; }
    public int MagicNumber { get; }
    public int ChunkCount { get; }
    public int FrameDuration { get; }

    #endregion

    public AseFrame(BinaryReader reader, Aseprite file, int frameIndex, AssetImportContext ase)
    {
        FrameSize = (int)reader.ReadUInt32();
        MagicNumber = (int)reader.ReadUInt16();

        if (MagicNumber != 0xF1FA)
        {
            return;
        }

        var oldChunkCount = reader.ReadUInt16();
        FrameDuration = reader.ReadUInt16();
        reader.ReadBytes(2); //Set to 0
        var newChunkCount = reader.ReadUInt32();
        var chunkCount = oldChunkCount == 0xFFFF ? newChunkCount : oldChunkCount;

        var celIndex = 0;

        for (var c = 0; c < chunkCount; c++)
        {
            var position = reader.BaseStream.Position;

            var chunkSize = reader.ReadUInt32();
            var chunkType = (ChunkType)reader.ReadUInt16();

            switch (chunkType)
            {
                default:
                case ChunkType.Undefined:
                case ChunkType.RealyOldPaletteChunk:
                case ChunkType.OldPaletteChunk:
                case ChunkType.CelExtraChunk:
                case ChunkType.ColorProfileChunk:
                case ChunkType.MaskChunkDeprecated:
                case ChunkType.PathChunk:
                case ChunkType.TagsChunk:
                case ChunkType.UserDataChunk:
                case ChunkType.SliceChunk:
                    //Skip unsuported chunk
                    break;

                case ChunkType.PaletteChunk:
                    file.Palette = new PaletteChunk(reader);
                    break;

                case ChunkType.CelChunk:
                    new AseCel(reader, file, frameIndex, celIndex, ase);
                    celIndex++;
                    break;

                case ChunkType.LayerChunk:
                    new AseLayer(reader, file, ase);
                    break;
            }

            reader.BaseStream.Position = position + chunkSize;
        }
    }
}

public class PaletteChunk
{
    #region Fields

    private List<AseColor> _colors = new List<AseColor>();

    #endregion

    #region Properties

    public UInt32 PaletteSize { get; private set; }
    public List<AseColor> Colors => _colors;

    #endregion

    public PaletteChunk(BinaryReader reader)
    {
        PaletteSize = reader.ReadUInt32();
        var from = reader.ReadUInt32();
        var to = reader.ReadUInt32();
        reader.ReadBytes(8); //For future
        for (var c = 0; c < (to - from) + 1; c++)
        {
            var hasName = reader.ReadUInt16() == 1;
            var red = reader.ReadByte();
            var green = reader.ReadByte();
            var blue = reader.ReadByte();
            var alpha = reader.ReadByte();
            byte[] name = null;
            if (hasName)
            {
                var nameLenght = reader.ReadUInt16();
                name = reader.ReadBytes(nameLenght);
            }
            Colors.Add(new AseColor(red, green, blue, alpha, name));
        }
    }
}

[ScriptedImporter(1, "ase")]
public class AseImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ase)
    {
        var aseprite = new Aseprite(ase);
    }
}

[ScriptedImporter(1, "aseprite")]
public class AsepriteImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ase)
    {
        var aseprite = new Aseprite(ase);
    }
}

[Serializable]
public class Aseprite
{
    #region Fields

    private List<AseFrame> _frames = new List<AseFrame>();

    #endregion

    #region Properties

    public AseHeader Header { get; }
    public List<AseFrame> Frames => _frames;
    public PaletteChunk Palette { get; set; }

    #endregion

    public Aseprite(AssetImportContext ase)
    {
        using (BinaryReader reader = new BinaryReader(File.Open(ase.assetPath, FileMode.Open)))
        {

            Header = new AseHeader(reader, ase);

            if (Header.MagicNumber != 0xA5E0)
            {
                Debug.LogError($"{ase.assetPath} not an Aseprite file!");
                return;
            }

            for (var f = 0; f < Header.FrameCount; f++)
            {
                var position = reader.BaseStream.Position;

                var frame = new AseFrame(reader, this, f, ase);
                if (frame.MagicNumber != 0xF1FA)
                {
                    Debug.LogError("Corrupted Aseprite file!", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ase.assetPath));
                    return;
                }
                _frames.Add(frame);

                reader.BaseStream.Position = position + frame.FrameSize;
            }
        }

        if (Palette != null)
        {
            int tSize = 8;
            while (Palette.PaletteSize - 1 > tSize * tSize)
            {
                tSize *= 2;
            }

            var paletteTexture = new Texture2D(tSize, tSize, TextureFormat.RGB24, false);
            paletteTexture.name = "Color Palette";
            paletteTexture.filterMode = FilterMode.Point;

            var row = tSize - 1;
            var column = 0;

            for (var i = 1; i < Palette.PaletteSize; i++)
            {
                paletteTexture.SetPixel(column, row, Palette.Colors[i].Color);
                column++;
                if (column >= tSize)
                {
                    column = 0;
                    row--;
                }
            }

            ase.AddObjectToAsset("Color Palette", paletteTexture);
            ase.SetMainObject(paletteTexture);
        }
    }
}