using System;

namespace VrSharp
{
    public abstract class VrPaletteDecoder
    {
        // This decodes the palette.
        // Buf can either be the actual image file containing the palette,
        // or the palette file itself.
        public abstract bool DecodePalette(ref byte[] Buf, int Pointer, int Colors, ref byte[][] Palette);

        // Get bits per pixel of the palette
        public abstract int GetBpp();
    }

    public abstract class VrPaletteEncoder
    {
    }
}