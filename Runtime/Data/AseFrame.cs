using AsepriteImporter.Runtime.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AsepriteImporter.Runtime.Data
{
    public class AseFrame
    {
        #region Properties

        public int FrameSize { get; }
        public int MagicNumber { get; }
        public int ChunkCount { get; }
        public int FrameDuration { get; }

        public AseCel[] Cels { get; }
        public Color32[] MergedFrame { get; }
        public AseLayerChunk[] Layers { get; }
        public AseTag[] Tags { get; }

        #endregion

        #region Private Methods

        private int MUL_UN8(int a, int b)
        {
            var t = (a * b) + 128;
            var result = (((t >> 8) + t) >> 8); //((t/(2^8) + t) / (2^8))
            return result;
        }


        private Color32 NormalBlend(Color32 source, Color32 target)
        {
            Color32 result = new Color32();

            if (target.a == 0)
            {
                result = source;
            }
            else if (source.a == 0)
            {
                result = target;
            }
            else
            {
                result.a = (byte)(source.a + target.a - MUL_UN8(target.a, source.a));

                result.r = (byte)(target.r + (source.r - target.r) * source.a / result.a);
                result.g = (byte)(target.g + (source.g - target.g) * source.a / result.a);
                result.b = (byte)(target.b + (source.b - target.b) * source.a / result.a);
            }

            return result;
        }


        private Color32[] MergeCels()
        {
            var pixels = new List<Color32>(Cels[Cels.Length - 1].Pixels);

            for (var c = Cels.Length - 1; c > 0; c--)
            {
                for (var p = 0; p < pixels.Count; p++)
                {
                    pixels[p] = NormalBlend(pixels[p], Cels[c - 1].Pixels[p]);
                }
            }

            return pixels.ToArray();
        }

        #endregion

        public AseFrame(BinaryReader reader, Aseprite file)
        {
            FrameSize = (int)reader.ReadUInt32();
            MagicNumber = reader.ReadUInt16();

            if (MagicNumber != 0xF1FA)
            {
                throw new FormatException("Corrupted Aseprite file!");
            }

            var oldChunkCount = reader.ReadUInt16();
            FrameDuration = reader.ReadUInt16();
            reader.ReadBytes(2); //Set to 0
            var newChunkCount = reader.ReadUInt32();
            var chunkCount = oldChunkCount == 0xFFFF ? newChunkCount : oldChunkCount;

            var celIndex = 0;

            var cels = new List<AseCel>();
            var layers = new List<AseLayerChunk>();

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
                    case ChunkType.UserDataChunk:
                    case ChunkType.SliceChunk:
                        //Skip unsuported chunk
                        break;

                    case ChunkType.PaletteChunk:
                        file.Palette = new AsePalette(reader);
                        break;

                    case ChunkType.CelChunk:
                        cels.Add(new AseCel(reader, file));
                        celIndex++;
                        break;

                    case ChunkType.LayerChunk:
                        var layer = new AseLayerChunk(reader, file);
                        if (layer.LayerType == LayerType.Normal)
                            layers.Add(layer);
                        break;

                    case ChunkType.TagsChunk:
                        var numberOfTags = reader.ReadUInt16();
                        reader.ReadBytes(8); //For future
                        var tags = new List<AseTag>();
                        for(var t=0; t<numberOfTags; t++)
                        {
                            tags.Add(new AseTag(reader));
                        }
                        Tags = tags.ToArray();
                        break;
                }

                reader.BaseStream.Position = position + chunkSize;
            }

            Layers = layers.ToArray();
            Cels = cels.ToArray();

            if (Cels.Length > 1)
            {
                MergedFrame = MergeCels();
            }
            else if (Cels.Length == 1)
            {
                MergedFrame = Cels[0].Pixels;
            }
            else
            {
                MergedFrame = new Color32[file.Header.Width * file.Header.Height];
            }
        }
    }
}
