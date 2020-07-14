using System;

namespace AsepriteImporter.Runtime.Enums
{
    public enum ChunkType : UInt16
    {
        Undefined = 0,
        RealyOldPaletteChunk = 0x0004,
        OldPaletteChunk = 0x0011,
        LayerChunk = 0x2004,
        CelChunk = 0x2005,
        CelExtraChunk = 0x2006,
        ColorProfileChunk = 0x2007,
        MaskChunkDeprecated = 0x2016,
        PathChunk = 0x2017,
        TagsChunk = 0x2018,
        PaletteChunk = 0x2019,
        UserDataChunk = 0x2020,
        SliceChunk = 0x2022
    }
}