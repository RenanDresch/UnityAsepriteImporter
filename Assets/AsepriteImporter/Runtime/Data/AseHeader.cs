using Assets.AsepriteImporter.Runtime.Enums;
using System;
using System.IO;

namespace Assets.AsepriteImporter.Runtime.Data
{
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

        public AseHeader(BinaryReader reader)
        {
            var fileSize = reader.ReadUInt32();

            MagicNumber = reader.ReadUInt16();

            if (MagicNumber != 0xA5E0)
            {
                throw new FormatException("Not an Aseprite file!");
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
}
