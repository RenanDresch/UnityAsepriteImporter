using UnityEngine;

namespace AsepriteImporter.Runtime.Data
{
    [System.Serializable]
    public class AseLayer
    {
        #region Fields

        [SerializeField]
        private Texture2D _texture = default;

        [SerializeField]
        private Sprite[] _sprites = default;

        #endregion

        #region Properties

        public Texture2D Texture => _texture;
        public Sprite[] Sprites => _sprites;

        #endregion

        public AseLayer(Texture2D texture, Sprite[] sprites)
        {
            _texture = texture;
            _sprites = sprites;
        }
    }
}
