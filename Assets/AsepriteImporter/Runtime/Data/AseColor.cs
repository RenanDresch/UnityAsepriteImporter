using System.Text;
using UnityEngine;

namespace Assets.AsepriteImporter.Runtime.Data
{
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
                Name = Encoding.UTF8.GetString(name, 0, name.Length);
            }
        }
    }
}
