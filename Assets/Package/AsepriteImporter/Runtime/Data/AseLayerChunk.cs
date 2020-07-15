using AsepriteImporter.Runtime.Enums;
using System.IO;
using System.Text;

namespace AsepriteImporter.Runtime.Data
{
    public class AseLayerChunk
    {
        #region Properties

        public LayerFlags Flags { get; }
        public LayerType LayerType { get; }
        public int LayerChildLevel { get; }
        public BlendMode BlendMode { get; }
        public int Opacity { get; }
        public string Name { get; }

        #endregion

        public AseLayerChunk(BinaryReader reader, Aseprite file)
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
}
