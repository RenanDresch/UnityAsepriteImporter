using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AsepriteImporter.Runtime.Data
{
    [Serializable]
    public class Aseprite
    {
        #region Fields

        private List<AseFrame> _frames = new List<AseFrame>();

        [SerializeField]
        private Texture2D _atlas;

        [SerializeField]
        private Texture2D _colorPalette;

        [SerializeField]
        private Sprite[] _sprites;

        #endregion

        #region Properties

        public AseHeader Header { get; }
        public List<AseFrame> Frames => _frames;
        public AsePalette Palette { get; set; }

        public Texture2D Atlas => _atlas;
        public Texture2D ColorPalette => _colorPalette;
        public Sprite[] Sprites => _sprites;

        #endregion

        public Aseprite(byte[] binary)
        {

            var stream = new MemoryStream(binary);

            using (BinaryReader reader = new BinaryReader(stream))
            {

                Header = new AseHeader(reader);

                for (var f = 0; f < Header.FrameCount; f++)
                {
                    var frame = new AseFrame(reader, this, f);
                    _frames.Add(frame);
                }
            }

            int tSize = 1;
            while (_frames.Count > tSize * tSize)
            {
                tSize *= 2;
            }

            var mainTexture = new Texture2D(Header.Width * tSize, Header.Height * tSize, TextureFormat.RGBA32, true);
            mainTexture.name = $"Texture";
            mainTexture.filterMode = FilterMode.Point;

            var mainTexturePixels = new Color32[Header.Width * tSize * Header.Height * tSize];

            for (var p = 0; p < mainTexturePixels.Length; p++)
            {
                var fRow = p / (tSize * Header.Width * Header.Height);
                var fColumn = (p - ((p / (tSize * Header.Width)) * tSize * Header.Width)) / Header.Width;
                var frameIndex = (tSize * tSize) - (fRow * tSize) - tSize + fColumn;

                if (frameIndex < _frames.Count)
                {
                    int xCoord;
                    Math.DivRem(p - (fRow * tSize * Header.Width * Header.Height), Header.Width, out xCoord);

                    int yCoord = (p / (tSize * Header.Width)) - (fRow * Header.Height);
                    int celIndex = yCoord * Header.Width + xCoord;

                    mainTexturePixels[p] = _frames[frameIndex].MergedFrame[celIndex];
                }
            }

            mainTexture.SetPixels32(mainTexturePixels);

            _atlas = mainTexture;

            mainTexture.Apply(true, false);

            var sprites = new List<Sprite>();

            for (var f = 0; f < tSize * tSize; f++)
            {
                var rectY = (Header.Height * tSize) - ((f / tSize) * Header.Height) - Header.Height;
                var rectX = (f * Header.Width) - ((f / tSize) * Header.Width * tSize);

                var spriteRect = new Rect(rectX, rectY, Header.Width, Header.Height);
                var newSprite = Sprite.Create(mainTexture, spriteRect, new Vector2(0.5f, 0.5f));
                newSprite.name = $"Frame {f}";

                sprites.Add(newSprite);
            }

            _sprites = sprites.ToArray();

            if (Palette != null)
            {
                tSize = 8;
                while (Palette.PaletteSize - 1 > tSize * tSize)
                {
                    tSize *= 2;
                }

                var paletteTexture = new Texture2D(tSize, tSize, TextureFormat.RGB24, true);
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

                paletteTexture.Apply();
                _colorPalette = paletteTexture;
            }
        }
    }
}
