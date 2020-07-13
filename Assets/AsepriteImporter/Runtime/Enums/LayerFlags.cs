
using System;

namespace AsepriteImporter.Runtime.Enums
{
    public enum LayerFlags : UInt16
    {
        Undefined = 0,
        Visible = 1,
        Editable = 2,
        LockMovement = 3,
        Background = 4,
        PreferLinkedCels = 5,
        Collapsed = 32,
        RefLayer = 64
    }
}
