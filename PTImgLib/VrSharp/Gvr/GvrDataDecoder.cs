using System;

namespace VrSharp
{
    public abstract class GvrDataDecoder : VrDataDecoder
    {
    }

    // Format 04
    public class GvrDataDecoder_04 : GvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[1][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
        public override int GetChunkWidth()
        {
            return 4;
        }
        public override int GetChunkHeight()
        {
            return 4;
        }
        public override int GetChunkBpp()
        {
            return 16;
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
            paletteDecoder = GvrCodecs.GetPaletteCodec(0x18).Decode; // Force RGB565
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

    // Format 05
    public class GvrDataDecoder_05 : GvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[1][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
        public override int GetChunkWidth()
        {
            return 4;
        }
        public override int GetChunkHeight()
        {
            return 4;
        }
        public override int GetChunkBpp()
        {
            return 16;
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
            paletteDecoder = GvrCodecs.GetPaletteCodec(0x28).Decode; // Force RGB5A3
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

    // Format 08
    public class GvrDataDecoder_08 : GvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[16][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
        public override int GetChunkWidth()
        {
            return 8;
        }
        public override int GetChunkHeight()
        {
            return 8;
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

            return true;
        }

        // Decode Chunk
        public override bool DecodeChunk(ref byte[] Input, ref int Pointer, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            int PixelPointer = 0;

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = (byte)((Input[Pointer + (PixelPointer >> 1)] >> (4 - (PixelPointer % 2) * 4)) & 0xF);

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

    // Format 09
    public class GvrDataDecoder_09 : GvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[256][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
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

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Get entry
                    byte entry = Input[Pointer];

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = Palette[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Palette[entry][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Palette[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Palette[entry][3];
                    Pointer++;
                }
            }

            return true;
        }
    }

    // Format 0E
    public class GvrDataDecoder_0E : GvrDataDecoder
    {
        // Set up variables
        bool init = false;
        byte[][] Palette = new byte[4][];
        int width, height;
        VrPaletteDecoder paletteDecoder;

        // Return a value functions
        public override int GetChunkWidth()
        {
            return 8;
        }
        public override int GetChunkHeight()
        {
            return 8;
        }
        public override int GetChunkBpp()
        {
            return 2;
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
            paletteDecoder = GvrCodecs.GetPaletteCodec(0x18).Decode; // Force RGB565
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

            for (int yBlock = 0; yBlock < 2; yBlock++)
            {
                for (int xBlock = 0; xBlock < 2; xBlock++)
                {
                    // Get palette entries for this 4x4 block
                    paletteDecoder.DecodePalette(ref Input, Pointer, 2, ref Palette);
                    ushort[] colors = new ushort[] {
                        ColorConversions.swap16(BitConverter.ToUInt16(Input, Pointer)),
                        ColorConversions.swap16(BitConverter.ToUInt16(Input, Pointer + 2))};
                    Pointer += 4;

                    if (colors[0] > colors[1])
                    {
                        // 4 color entries for this 4x4 color block
                        Palette[2] = new byte[] {
                            0xFF,
                            (byte)(((Palette[0][1] * 2) + Palette[1][1]) / 3),
                            (byte)(((Palette[0][2] * 2) + Palette[1][2]) / 3),
                            (byte)(((Palette[0][3] * 2) + Palette[1][3]) / 3)};

                        Palette[3] = new byte[] {
                            0xFF,
                            (byte)((Palette[0][1] + (Palette[1][1] * 2)) / 3),
                            (byte)((Palette[0][2] + (Palette[1][2] * 2)) / 3),
                            (byte)((Palette[0][3] + (Palette[1][3] * 2)) / 3)};
                    }
                    else
                    {
                        // 3 color entries; last entry is transparent
                        Palette[2] = new byte[] {
                            0xFF,
                            (byte)((Palette[0][1] + Palette[1][1]) / 2),
                            (byte)((Palette[0][2] + Palette[1][2]) / 2),
                            (byte)((Palette[0][3] + Palette[1][3]) / 2)};

                        Palette[3] = new byte[] {
                            0x00,
                            0x00,
                            0x00,
                            0x00};
                    }

                    for (int y2 = 0; y2 < GetChunkHeight() / 2; y2++)
                    {
                        for (int x2 = 0; x2 < GetChunkWidth() / 2; x2++)
                        {
                            // Get byte entry
                            byte entry = (byte)(Input[Pointer] >> (6 - (x2 * 2)) & 0x3);

                            Output[((y2 + y1 + (yBlock * (GetChunkHeight() / 2))) * width + (x1 + x2 + (xBlock * (GetChunkWidth() / 2)))) * 4 + 0] = Palette[entry][0];
                            Output[((y2 + y1 + (yBlock * (GetChunkHeight() / 2))) * width + (x1 + x2 + (xBlock * (GetChunkWidth() / 2)))) * 4 + 1] = Palette[entry][1];
                            Output[((y2 + y1 + (yBlock * (GetChunkHeight() / 2))) * width + (x1 + x2 + (xBlock * (GetChunkWidth() / 2)))) * 4 + 2] = Palette[entry][2];
                            Output[((y2 + y1 + (yBlock * (GetChunkHeight() / 2))) * width + (x1 + x2 + (xBlock * (GetChunkWidth() / 2)))) * 4 + 3] = Palette[entry][3];
                        }

                        Pointer++;
                    }
                }
            }

            return true;
        }
    }
}