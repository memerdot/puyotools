using System;

namespace VrSharp
{
    public abstract class VrDataDecoder
    {
        // The Width of a chunk
        abstract public int GetChunkWidth();

        // The Height of a chunk
        abstract public int GetChunkHeight();

        // The BPP of a chunk
        abstract public int GetChunkBpp();

        // The bytes of a chunk
        public int GetChunkSize()
        {
            return (GetChunkWidth() * GetChunkHeight() * GetChunkBpp()) / 8;
        }

        // Size of the palette
        abstract public int GetPaletteSize();

        // Do we need an external palette?
        abstract public bool NeedExternalPalette();

        // Initialization, always called first.
        // Passed to this function is the entire image header (0x20 bytes to be exact)
        // This will allow a VrDecoder to support multiple similar formats.
        abstract public bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder);

        // Decode the palette
        // Passed to this function is the palette
        abstract public bool DecodePalette(ref byte[] Input, int Pointer);

        // Decode a single chunk
        // Passed to this function is the chunk and chunk pointer
        // You must move the chunk pointer and return true for this to work.
        abstract public bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1);
    }

    public abstract class VrDataEncoder
    {
    }
}