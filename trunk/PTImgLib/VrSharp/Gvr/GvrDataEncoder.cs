using System;
using System.Drawing;
using BetterImageProcessorQuantization;

namespace VrSharp
{
    public abstract class GvrDataEncoder : VrDataEncoder
    {
    }

    // Format 09
    public class PvrDataEncoder_Index8 : GvrDataEncoder
    {
        uint[] Palette = new uint[256];

        public override int GetChunkWidth()
        {
            return 8;
        }
        public override int GetChunkHeight()
        {
            return 4;
        }
        public override int GetChunkBpp()
        {
            return 8;
        }
        public override int GetPaletteSize()
        {
            return Palette.Length * (paletteEncoder.GetBpp() / 8);
        }
        public override bool Initialize(ref byte[] Input, int Pointer, int Width, int Height, VrPaletteEncoder PaletteEncoder)
        {
            width  = Width;
            height = Height;
            paletteEncoder = PaletteEncoder;

            // Make sure width and height are a power of 2
            if (width == 0 || (width & (width - 1)) != 0)
                throw new Exception("Width and Height must be a power of 2.");

            // Quantitize this image
            VrColorQuantize.QuantizeImage(ref Input, Palette.Length, GetChunkBpp());

            init = true;
            return true;
        }
        public override bool EncodePalette(ref byte[] Input, int Pointer, ref byte[][] Palette)
        {
            return true;
        }
        public override bool EncodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            int StartPointer = Pointer;

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get color entry
                    paletteEncoder.EncodePalette(ref Input, ((y2 + y1) * width + (x1 + x2)) * 4, Palette.Length, ref Palette);

                    Array.Copy(BitConverter.GetBytes(Palette[0]), 0, Output, Pointer, (GetChunkBpp() / 8));
                    Pointer += (GetChunkBpp() / 8);
                }
            }

            return true;
        }
        public override bool Finalize(ref byte[] Input, ref int Pointer)
        {
            return true;
        }
    }
}