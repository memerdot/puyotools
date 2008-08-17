using System;
using System.Collections.Generic;
using System.Text;

namespace GvrSharp
{
    public abstract class GvrEncoder
    {
        // The width of a chunk
        abstract public int GetChunkWidth();

        // The height of a chunk
        abstract public int GetChunkHeight();

        // The bytes of a chunk
        abstract public int GetChunkSize();

        // The format header size
        abstract public int FormatHeaderSize();

        // Initialization, always called first.
        // Passed to this function is the entire image header (0x20 bytes to be exact)
        // This will allow a GvrDecoder to support multiple similar formats.
        abstract public bool Initialize(ref byte[] Data, byte[] AuxData, int Width, int Height);

        // Decode the format header
        // Passed to this function is the format header 
        abstract public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer);

        // Decode a single chunk
        // Passed to this function is the chunk and chunk pointer
        // You must move the chunk pointer and return true for this to work.
        abstract public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1);
    }

    public class Pal_565_8x4_Encode : GvrEncoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;
        public void GeneratePalette(ref byte[] Data)
        {
            GvrColorQuantize ImgQuantize = new GvrColorQuantize(ref Data, width, height);
        }
        override public int GetChunkWidth()
        {
            return 8;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkSize()
        {
            return 8 * 4;
        }
        override public int FormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool Initialize(ref byte[] Data, byte[] AuxData, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;

            if (AuxData == null)
            {
                GeneratePalette(ref Data);
            }
            else
            {
                int Pointer;
                for (int i = 0; i < 256; i++)
                {
                    ushort entry = ColorConversions.swap16((ushort)(AuxData[Pointer] + AuxData[Pointer + 1] * 256));
                    PaletteARGB[i][0] = 0xFF;
                    PaletteARGB[i][1] = (byte)((((entry) >> 8) & 0xf8) | ((entry) >> 13));
                    PaletteARGB[i][2] = (byte)((((entry) >> 3) & 0xfc) | (((entry) >> 9) & 0x03));
                    PaletteARGB[i][3] = (byte)((((entry) << 3) & 0xf8) | (((entry) >> 2) & 0x07));
                    Pointer += 2;
                }
            }

            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 256; i++)
            {
                PaletteARGB[i] = new byte[4];

                // Get 16-bit palette Entry
                ushort entry = ColorConversions.swap16((ushort)(FormatHeader[Pointer] + FormatHeader[Pointer + 1] * 256));
                PaletteARGB[i][0] = 0xFF;
                PaletteARGB[i][1] = (byte)((((entry) >> 8) & 0xf8) | ((entry) >> 13));
                PaletteARGB[i][2] = (byte)((((entry) >> 3) & 0xfc) | (((entry) >> 9) & 0x03));
                PaletteARGB[i][3] = (byte)((((entry) << 3) & 0xf8) | (((entry) >> 2) & 0x07));
                Pointer += 2;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < 4; y2++)
            {
                for (int x2 = 0; x2 < 8; x2++)
                {
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr]][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr]][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr]][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr]][3];
                    InPtr++;
                }
            }
            return true;
        }
    }
}
