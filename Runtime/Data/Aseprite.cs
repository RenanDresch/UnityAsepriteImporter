using AsepriteImporter.Runtime.Enums;
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
        private Texture2D _colorPalette;

        [SerializeField]
        private AseLayer _mergedLayer;

        [SerializeField]
        private List<AseLayer> _layers = new List<AseLayer>();

        #endregion

        #region Properties

        public AseHeader Header { get; }
        public List<AseFrame> Frames => _frames;
        public AsePalette Palette { get; set; }

        public Texture2D ColorPalette => _colorPalette;
        public AseLayer MergedLayer => _mergedLayer;
        public AseLayer[] Layers => _layers.ToArray();

        #endregion

        #region Constructor

        public Aseprite(byte[] binary, AseImportOptions mergedLayerImportOptions = AseImportOptions.Animations,
            AseImportOptions separateLayersImportOptions = AseImportOptions.None, Vector2 pivotPosition = default
            , string fileName = "Aseprite")
        {

            var stream = new MemoryStream(binary);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                Header = new AseHeader(reader);

                for (var f = 0; f < Header.FrameCount; f++)
                {
                    var frame = new AseFrame(reader, this);
                    _frames.Add(frame);
                }
            }

            GeneratePaletteTexture();

            if ((int)separateLayersImportOptions > 0)
            {
                int layerCount = Frames[0].Layers.Length;
                var cels = new List<Color32[]>();

                for (var l = 0; l < layerCount; l++)
                {
                    cels.Clear();
                    for (var f = 0; f < Frames.Count; f++)
                    {
                        if (Frames[f].Cels.Length > l)
                        {
                            cels.Add(Frames[f].Cels[l].Pixels);
                        }
                        else
                        {
                            cels.Add(new Color32[Header.Width * Header.Height]);
                        }
                    }
                    var atlasResult = GenerateAtlas($"{fileName}_Layer_{l}", cels);
                    var layerSprites = SliceSprites(atlasResult.Item2, atlasResult.Item1, pivotPosition);

                    _layers.Add(new AseLayer(atlasResult.Item2, layerSprites));
                }
            }

            if ((int)mergedLayerImportOptions > 0)
            {
                var cels = new List<Color32[]>();
                foreach (var frame in Frames)
                {
                    cels.Add(frame.MergedFrame);
                }

                var atlasResult = GenerateAtlas(fileName, cels);
                var layerSprites = SliceSprites(atlasResult.Item2, atlasResult.Item1, pivotPosition);

                _mergedLayer = new AseLayer(atlasResult.Item2, layerSprites);
            }
        }


        #endregion

        #region Private Methods

        private void GeneratePaletteTexture()
        {
            if (Palette != null)
            {
                var tSize = 8;
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

        private (int, Texture2D) GenerateAtlas(string fileName, List<Color32[]> cels)
        {
            int tSize = 1;
            while (cels.Count > tSize * tSize)
            {
                tSize *= 2;
            }

            var texture = new Texture2D(Header.Width * tSize, Header.Height * tSize, TextureFormat.RGBA32, true);
            texture.name = $"{fileName}";
            texture.filterMode = FilterMode.Point;

            var texturePixels = new Color32[Header.Width * tSize * Header.Height * tSize];

            for (var p = 0; p < texturePixels.Length; p++)
            {
                var fRow = p / (tSize * Header.Width * Header.Height);
                var fColumn = (p - ((p / (tSize * Header.Width)) * tSize * Header.Width)) / Header.Width;
                var frameIndex = (tSize * tSize) - (fRow * tSize) - tSize + fColumn;

                if (frameIndex < cels.Count)
                {
                    int xCoord;
                    Math.DivRem(p - (fRow * tSize * Header.Width * Header.Height), Header.Width, out xCoord);

                    int yCoord = (p / (tSize * Header.Width)) - (fRow * Header.Height);
                    int celIndex = yCoord * Header.Width + xCoord;

                    texturePixels[p] = cels[frameIndex][celIndex];
                }
            }

            texture.SetPixels32(texturePixels);
            texture.Apply(true, false);

            return (tSize, texture);
        }

        private Sprite[] SliceSprites(Texture2D sourceTexture, int tSize, Vector2 pivotPosition)
        {
            var sprites = new List<Sprite>();

            for (var f = 0; f < tSize * tSize; f++)
            {
                var rectY = (Header.Height * tSize) - ((f / tSize) * Header.Height) - Header.Height;
                var rectX = (f * Header.Width) - ((f / tSize) * Header.Width * tSize);

                var spriteRect = new Rect(rectX, rectY, Header.Width, Header.Height);
                var newSprite = Sprite.Create(sourceTexture, spriteRect, pivotPosition);
                newSprite.name = $"{sourceTexture.name}_Frame_{f}";

                sprites.Add(newSprite);
            }

            return sprites.ToArray();
        }

        //private AnimationClip[] GenerateClips(AseTag tag, Sprite[] frames)
        //{
        //    //Keyframe
        //    //ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
        //    //for (int i = 0; i < (sprites.Length); i++)
        //    //{
        //    //    spriteKeyFrames[i] = new ObjectReferenceKeyframe();
        //    //    spriteKeyFrames[i].time = i;
        //    //    spriteKeyFrames[i].value = sprites[i];
        //    //}
        //    //AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);
        //}

        #endregion

        #region Public Methods

        //public AseAnimationKeyframe[] GetAnimationKeyframes()

        #endregion

    }
}
