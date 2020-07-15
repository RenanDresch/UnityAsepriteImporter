using AsepriteImporter.Runtime.Enums;
using System.IO;
using System.Text;

namespace AsepriteImporter.Runtime.Data
{
    public class AseTag
    {
        #region Properties

        public int FromFrame { get; }
        public int ToFrame { get; }
        public LoopAnimationDirection LoopAnimationDirection { get; }
        public string Name { get; }

        #endregion

        public AseTag(BinaryReader reader)
        {
            FromFrame = reader.ReadUInt16();
            ToFrame = reader.ReadUInt16();
            LoopAnimationDirection = (LoopAnimationDirection)reader.ReadByte();
            reader.ReadBytes(8); //For future
            var tagColor = reader.ReadBytes(3);
            var extraByte = reader.ReadByte();
            var nameLenght = reader.ReadUInt16();
            var name = reader.ReadBytes(nameLenght);
            Name = Encoding.UTF8.GetString(name, 0, nameLenght);
        }
    }
}
