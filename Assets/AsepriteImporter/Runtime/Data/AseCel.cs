using Assets.AsepriteImporter.Runtime.Enums;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Assets.AsepriteImporter.Runtime.Data
{
    public class AseCel
    {
        #region Properties

        public Color32[] Pixels { get; }

        #endregion

        #region Private Methods

        private Color32[] GetCelPixels(byte[] pixels, Rect cell, int textureWidth, int textureHeight, ColorDepth depth)
        {
            var colors = new Color32[textureWidth * textureHeight];
            var celPixelIndex = 0;

            for (var r = textureHeight - 1; r > -1; r--)
            {
                for (var c = 0; c < textureWidth; c++)
                {
                    if (r < textureHeight - cell.position.y && celPixelIndex < pixels.Length)
                    {
                        if (c < (cell.width + cell.position.x) && c >= cell.position.x)
                        {
                            //Todo : indexed and gray readings
                            colors[r * textureWidth + c] = new Color32(
                                pixels[celPixelIndex],
                                pixels[celPixelIndex + 1],
                                pixels[celPixelIndex + 2],
                                pixels[celPixelIndex + 3]);
                            celPixelIndex += 4;
                        }
                    }
                }
            }

            return colors;
        }

        #endregion

        #region Constructor

        public AseCel(BinaryReader reader, Aseprite file)
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
                    reader.Close();
                    throw new System.NotImplementedException("Raw cel not supported! Aborting!");

                case CelType.Linked:
                    reader.Close();
                    throw new System.NotImplementedException("Linked cel not supported! Aborting!");

                case CelType.CompressedImage:
                    var celWidth = reader.ReadUInt16();
                    var celHeight = reader.ReadUInt16();

                    var celData = new byte[celWidth * celHeight * 4];

                    reader.ReadBytes(2); //Skip Zlib header

                    var deflate = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                    deflate.Read(celData, 0, celWidth * celHeight * 4);

                    Pixels = GetCelPixels(celData, new Rect(xPosition, yPosition, celWidth, celHeight),
                        file.Header.Width, file.Header.Height, file.Header.ColorDepth);
                    break;

                default:
                    reader.Close();
                    throw new System.NotImplementedException("Undefined cel not supported! Aborting!");
            }
        }
    }

    #endregion
}
