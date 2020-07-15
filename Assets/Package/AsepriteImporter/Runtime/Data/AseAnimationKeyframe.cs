using UnityEngine;

namespace AsepriteImporter.Runtime.Data
{
    public class AseAnimationKeyframe
    {
        #region Properties

        public Sprite Sprite { get; }
        public float FrameTime { get; }

        #endregion

        public AseAnimationKeyframe(Sprite sprite, float frameTime)
        {
            Sprite = sprite;
            FrameTime = frameTime;
        }
    }
}