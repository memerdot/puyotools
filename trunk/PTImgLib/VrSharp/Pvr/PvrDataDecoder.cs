using System;

namespace VrSharp
{
    public abstract class PvrDataDecoder : VrDataDecoder
    {
    }

    // Format 01
    public class PvrDataDecoder_01 : PvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[1][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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
            return 0;
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool NeedExternalPalette()
        {
            return false;
        }

        // Initalize
        public override bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder)
        {
            width  = Width;
            height = Height;
            paletteDecoder = PaletteDecoder;
            init = true;

            return true;
        }

        // Decode Palette
        public override bool DecodePalette(ref byte[] Input, int Pointer)
        {
            if (!init) throw new Exception("Could not decode palette because you have not initalized yet.");

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Untwiddle image
            PvrTwiddle.UnTwiddle(ref Input, Pointer, width * 2, height);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get palette for this pixel
                    paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[0][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[0][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[0][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[0][3];
                    Pointer += (paletteDecoder.GetBpp() / 8);
                }
            }

            return true;
        }
    }

    // Format 05
    public class PvrDataDecoder_05 : PvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[16][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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
            return 4;
        }
        public override int GetPaletteSize()
        {
            return Palette.Length * (paletteDecoder.GetBpp() / 8);
        }
        public override bool NeedExternalPalette()
        {
            return false;
        }

        // Initalize
        public override bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder)
        {
            width  = Width;
            height = Height;
            paletteDecoder = PaletteDecoder;
            init = true;

            return true;
        }

        // Decode Palette
        public override bool DecodePalette(ref byte[] Input, int Pointer)
        {
            if (!init) throw new Exception("Could not decode palette because you have not initalized yet.");

            paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Untwiddle image
            PvrTwiddle.UnTwiddle4(ref Input, Pointer, width / 2, height);

            int PixelPointer = 0;

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = (byte)((Input[Pointer + (PixelPointer >> 1)] >> ((PixelPointer % 2) * 4)) & 0xF);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[entry][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[entry][3];
                    PixelPointer++;
                }
            }

            Pointer += (PixelPointer >> 1);

            return true;
        }
    }

    // Format 07
    public class PvrDataDecoder_07 : PvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[256][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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
            return 8;
        }
        public override int GetPaletteSize()
        {
            return Palette.Length * (paletteDecoder.GetBpp() / 8);
        }
        public override bool NeedExternalPalette()
        {
            return true;
        }

        // Initalize
        public override bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder)
        {
            width = Width;
            height = Height;
            paletteDecoder = PaletteDecoder;
            init = true;

            return true;
        }

        // Decode Palette
        public override bool DecodePalette(ref byte[] Input, int Pointer)
        {
            if (!init) throw new Exception("Could not decode palette because you have not initalized yet.");

            // Set correct number of entries
            Palette = new byte[BitConverter.ToInt16(Input, 0xE)][];

            paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Untwiddle image
            PvrTwiddle.UnTwiddle(ref Input, Pointer, width, height);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    Output[((x2 + x1) * width + (y1 + y2)) * 4 + 0] = Palette[Input[Pointer]][0];
                    Output[((x2 + x1) * width + (y1 + y2)) * 4 + 1] = Palette[Input[Pointer]][1];
                    Output[((x2 + x1) * width + (y1 + y2)) * 4 + 2] = Palette[Input[Pointer]][2];
                    Output[((x2 + x1) * width + (y1 + y2)) * 4 + 3] = Palette[Input[Pointer]][3];
                    Pointer++;
                }
            }

            return true;
        }
    }

    // Format 09
    public class PvrDataDecoder_09 : PvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[1][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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
            return 0;
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool NeedExternalPalette()
        {
            return false;
        }

        // Initalize
        public override bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder)
        {
            width  = Width;
            height = Height;
            paletteDecoder = PaletteDecoder;
            init = true;

            return true;
        }

        // Decode Palette
        public override bool DecodePalette(ref byte[] Input, int Pointer)
        {
            if (!init) throw new Exception("Could not decode palette because you have not initalized yet.");

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get palette for this pixel
                    paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[0][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[0][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[0][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[0][3];
                    Pointer += (paletteDecoder.GetBpp() / 8);
                }
            }

            return true;
        }
    }

    // Format 0D
    public class PvrDataDecoder_0D : PvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[1][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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
            return 0;
        }
        public override int GetPaletteSize()
        {
            return 0;
        }
        public override bool NeedExternalPalette()
        {
            return false;
        }

        // Initalize
        public override bool Initialize(int Width, int Height, VrPaletteDecoder PaletteDecoder)
        {
            width  = Width;
            height = Height;
            paletteDecoder = PaletteDecoder;
            init = true;

            return true;
        }

        // Decode Palette
        public override bool DecodePalette(ref byte[] Input, int Pointer)
        {
            if (!init) throw new Exception("Could not decode palette because you have not initalized yet.");

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Untwiddle image
            PvrTwiddle.UnTwiddle(ref Input, Pointer, width * 2, height);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get palette for this pixel
                    paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[0][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[0][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[0][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[0][3];
                    Pointer += (paletteDecoder.GetBpp() / 8);
                }
            }

            return true;
        }
    }
}