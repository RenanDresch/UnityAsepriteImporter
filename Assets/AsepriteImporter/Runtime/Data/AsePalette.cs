
using System;
using System.Collections.Generic;
using System.IO;

namespace Assets.AsepriteImporter.Runtime.Data
{
    public class AsePalette
    {
        #region Fields

        private List<AseColor> _colors = new List<AseColor>();

        #endregion

        #region Properties

        public int PaletteSize { get; }
        public List<AseColor> Colors => _colors;

        #endregion

        public AsePalette(BinaryReader reader)
        {
            PaletteSize = (int)reader.ReadUInt32();
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
}
