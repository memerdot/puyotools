using System;

namespace VrSharp
{
    public abstract class PvrDataEncoder : VrDataEncoder
    {
    }

    // Format 01
    public class PvrDataEncoder_01 : PvrDataEncoder
    {
        uint[] Palette = new uint[1];

        public override int GetChunkWidth()
        {
            return width;
        }
        public override int GetChunkHeight()
        {
            return height;
        }
        public override int GetChunkBpp()
        {
            return paletteEncoder.GetBpp();
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool Initialize(int Width, int Height, VrPaletteEncoder PaletteEncoder)
        {
            width  = Width;
            height = Height;
            paletteEncoder = PaletteEncoder;

            // Make sure width and height are the same, and a power of 2
            if (width != height)
                throw new Exception("Width and Height must be the same for a Square Twiddled PVR.");
            if (width == 0 || (width & (width - 1)) != 0)
                throw new Exception("Width and Height must be a power of 2.");

            init = true;
            return true;
        }
        public override bool EncodePalette(ref byte[] Input, int Pointer, ref byte[][] Palette)
        {
            return true;
        }
        public override bool EncodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
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
            // Twiddle texture
            PvrTwiddle.Twiddle(ref Input, Pointer, width, height, GetChunkBpp());

            return true;
        }
    }

    // Format 09
    public class PvrDataEncoder_09 : PvrDataEncoder
    {
        uint[] Palette = new uint[1];

        public override int GetChunkWidth()
        {
            return width;
        }
        public override int GetChunkHeight()
        {
            return height;
        }
        public override int GetChunkBpp()
        {
            return paletteEncoder.GetBpp();
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool Initialize(int Width, int Height, VrPaletteEncoder PaletteEncoder)
        {
            width  = Width;
            height = Height;
            paletteEncoder = PaletteEncoder;

            // Make sure width are a power of 2
            if (width == 0 || (width & (width - 1)) != 0)
                throw new Exception("Width and Height must be a power of 2.");

            init = true;
            return true;
        }
        public override bool EncodePalette(ref byte[] Input, int Pointer, ref byte[][] Palette)
        {
            return true;
        }
        public override bool EncodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
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

    // Format 0D
    public class PvrDataEncoder_0D : PvrDataEncoder
    {
        uint[] Palette = new uint[1];

        public override int GetChunkWidth()
        {
            return width;
        }
        public override int GetChunkHeight()
        {
            return height;
        }
        public override int GetChunkBpp()
        {
            return paletteEncoder.GetBpp();
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool Initialize(int Width, int Height, VrPaletteEncoder PaletteEncoder)
        {
            width  = Width;
            height = Height;
            paletteEncoder = PaletteEncoder;

            // Make sure width and height are a power of 2
            if (width == 0 || (width & (width - 1)) != 0)
                throw new Exception("Width and Height must be a power of 2.");

            init = true;
            return true;
        }
        public override bool EncodePalette(ref byte[] Input, int Pointer, ref byte[][] Palette)
        {
            return true;
        }
        public override bool EncodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
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
            // Twiddle texture
            PvrTwiddle.Twiddle(ref Input, Pointer, width, height, GetChunkBpp());

            return true;
        }
    }
}