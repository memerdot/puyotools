using System;

namespace VrSharp
{
    public abstract class SvrDataDecoder : VrDataDecoder
    {
    }

    // Format 60
    public class SvrDataDecoder_60 : SvrDataDecoder
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
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[0][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[0][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[0][1];
                    Pointer += (paletteDecoder.GetBpp() / 8);
                }
            }

            return true;
        }
    }

    // Format 62
    public class SvrDataDecoder_62 : SvrDataDecoder
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
            return true;
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

            int PixelPointer = 0;

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle4(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = (byte)((Input[Pointer + (PixelPointer >> 1)] >> ((PixelPointer % 2) * 4)) & 0xF);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[entry][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[entry][1];
                    PixelPointer++;
                }
            }

            Pointer += (PixelPointer >> 1);

            return true;
        }
    }

    // Format 64
    public class SvrDataDecoder_64 : SvrDataDecoder
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
            SvrPaletteDecoder.ReorderPalette(ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle8(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[Input[Pointer]][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[Input[Pointer]][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[Input[Pointer]][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[Input[Pointer]][1];
                    Pointer++;
                }
            }

            return true;
        }
    }

    // Format 68
    public class SvrDataDecoder_68 : SvrDataDecoder
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

            int PixelPointer = 0;

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle4(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = (byte)((Input[Pointer + (PixelPointer >> 1)] >> ((PixelPointer % 2) * 4)) & 0xF);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[entry][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[entry][1];
                    PixelPointer++;
                }
            }

            Pointer += (PixelPointer >> 1);

            return true;
        }
    }

    // Format 69
    public class SvrDataDecoder_69 : SvrDataDecoder
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

            paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            int PixelPointer = 0;

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle4(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = (byte)((Input[Pointer + (PixelPointer >> 1)] >> ((PixelPointer % 2) * 4)) & 0xF);

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[entry][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[entry][1];
                    PixelPointer++;
                }
            }

            Pointer += (PixelPointer >> 1);

            return true;
        }
    }

    // Format 6A
    public class SvrDataDecoder_6A : SvrDataDecoder
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
            return false;
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

            paletteDecoder.DecodePalette(ref Input, Pointer, Palette.Length, ref Palette);
            SvrPaletteDecoder.ReorderPalette(ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle8(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[Input[Pointer]][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[Input[Pointer]][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[Input[Pointer]][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[Input[Pointer]][1];
                    Pointer++;
                }
            }

            return true;
        }
    }

    // Format 6C
    public class SvrDataDecoder_6C : SvrDataDecoder
    {
        // Set up variables
        bool init          = false;
        byte[][] Palette   = new byte[256][];
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
            SvrPaletteDecoder.ReorderPalette(ref Palette);

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle SVR
            SvrSwizzle.UnSwizzle8(Input, width, height, Pointer);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[Input[Pointer]][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[Input[Pointer]][3];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[Input[Pointer]][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[Input[Pointer]][1];
                    Pointer++;
                }
            }

            return true;
        }
    }
}