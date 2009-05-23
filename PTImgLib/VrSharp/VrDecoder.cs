// VrDecoder.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

using System;
using System.Collections.Generic;
using System.Text;
using VrSharp;

namespace VrSharp
{
    public abstract class VrDecoder
    {
        // The Width of a chunk
        abstract public int GetChunkWidth();

        // The Height of a chunk
        abstract public int GetChunkHeight();

        // The BPP of a chunk
        abstract public int GetChunkBpp();

        // Do we need an external palette?
        abstract public bool NeedExternalPalette();

        // Pass an external palette
        abstract public bool SendExternalPalette(byte[] ExtPal, int PalSize);

        // The bytes of a chunk
        public int GetChunkSize()
        {
            return (GetChunkWidth() * GetChunkHeight() * GetChunkBpp())/8;
        }

        // The format header size
        abstract public int GetFormatHeaderSize();

        // Initialization, always called first.
        // Passed to this function is the entire image header (0x20 bytes to be exact)
        // This will allow a VrDecoder to support multiple similar formats.
        abstract public bool Initialize(byte[] ImageHeader, int Width, int Height);

        // Decode the format header
        // Passed to this function is the format header 
        abstract public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer);

        // Decode a single chunk
        // Passed to this function is the chunk and chunk pointer
        // You must move the chunk pointer and return true for this to work.
        abstract public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1);
    }

    public class VrDecoder_00000004 : VrDecoder
    {
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 4;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkBpp()
        {
            return 2;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < 4; y2++)
            {
                for (int x2 = 0; x2 < 4; x2++)
                {
                    ushort entry = ColorConversions.swap16((ushort)(Input[InPtr] + Input[InPtr+1] * 256));

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = 0xFF;
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = (byte)((((entry) >> 8) & 0xf8) | ((entry) >> 13));
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = (byte)((((entry) >> 3) & 0xfc) | (((entry) >> 9) & 0x03));
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = (byte)((((entry) << 3) & 0xf8) | (((entry) >> 2) & 0x07));
                    InPtr+=2;
                }
            }
            return true;
        }
    }
    public class VrDecoder_00000005 : VrDecoder
    {
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 4;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkBpp()
        {
            return 16;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < 4; y2++)
            {
                for (int x2 = 0; x2 < 4; x2++)
                {
                    ushort entry = ColorConversions.swap16((ushort)(Input[InPtr] + Input[InPtr + 1] * 256));

                    if ((entry & 0x8000) != 0)
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = (byte)0xFF;
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = (byte)(((entry >> 10) & 0x1f) * 255 / 32);
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = (byte)(((entry >> 5) & 0x1f) * 255 / 32);
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = (byte)(((entry >> 0) & 0x1f) * 255 / 32);
                    }
                    else
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = (byte)(((entry >> 8) & 0x0f) * 255 / 16);
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = (byte)(((entry >> 4) & 0x0f) * 255 / 16);
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = (byte)(((entry >> 0) & 0x0f) * 255 / 16);
                    }

                    InPtr += 2;
                }
            }
            return true;
        }
    }
    public class VrDecoder_00000006 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;

        override public int GetChunkWidth()
        {
            return 4;
        }
        override public int GetChunkHeight()
        {
            return 2;
        }
        override public int GetChunkBpp()
        {
            return 32;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < 2; y2++)
            {
                try
                {
                    Output[((y2 + y1) * width + (x1 + 0)) * 4 + 0] = Input[InPtr + 0];
                    Output[((y2 + y1) * width + (x1 + 0)) * 4 + 1] = Input[InPtr + 1];
                    Output[((y2 + y1) * width + (x1 + 0)) * 4 + 2] = Input[InPtr + 2];
                    Output[((y2 + y1) * width + (x1 + 0)) * 4 + 3] = Input[InPtr + 3];

                    Output[((y2 + y1) * width + (x1 + 2)) * 4 + 0] = Input[InPtr + 4];
                    Output[((y2 + y1) * width + (x1 + 2)) * 4 + 1] = Input[InPtr + 5];
                    Output[((y2 + y1) * width + (x1 + 2)) * 4 + 2] = Input[InPtr + 6];
                    Output[((y2 + y1) * width + (x1 + 2)) * 4 + 3] = Input[InPtr + 7];

                    Output[((y2 + y1) * width + (x1 + 1)) * 4 + 0] = Input[InPtr + 8];
                    Output[((y2 + y1) * width + (x1 + 1)) * 4 + 1] = Input[InPtr + 9];
                    Output[((y2 + y1) * width + (x1 + 1)) * 4 + 2] = Input[InPtr + 10];
                    Output[((y2 + y1) * width + (x1 + 1)) * 4 + 3] = Input[InPtr + 11];

                    Output[((y2 + y1) * width + (x1 + 3)) * 4 + 0] = Input[InPtr + 12];
                    Output[((y2 + y1) * width + (x1 + 3)) * 4 + 1] = Input[InPtr + 13];
                    Output[((y2 + y1) * width + (x1 + 3)) * 4 + 2] = Input[InPtr + 14];
                    Output[((y2 + y1) * width + (x1 + 3)) * 4 + 3] = Input[InPtr + 15];
                }
                catch
                {
                    return true;
                }

                InPtr += 16;
            }
            InPtr += 32;
            return true;
        }
    }
    public class VrDecoder_00001808 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 8;
        }
        override public int GetChunkHeight()
        {
            return 8;
        }
        override public int GetChunkBpp()
        {
            return 4;
        }
        override public int GetFormatHeaderSize()
        {
            return 16 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 16; i++)
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
            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); )
                {
                    try
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] >> 4][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] >> 4][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] >> 4][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] >> 4][3];
                        x2++;
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] & 0xF][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] & 0xF][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] & 0xF][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] & 0xF][3];
                        x2++;
                    }
                    catch
                    {
                        x2 += 2;
                        return true;
                    }
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_00001809 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 8;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkBpp()
        {
            return 1;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
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
    public class VrDecoder_00002808 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 8;
        }
        override public int GetChunkHeight()
        {
            return 8;
        }
        override public int GetChunkBpp()
        {
            return 4;
        }
        override public int GetFormatHeaderSize()
        {
            return 16 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 16; i++)
            {
                PaletteARGB[i] = new byte[4];

                // Get 16-bit palette Entry
                ushort entry = ColorConversions.swap16((ushort)(FormatHeader[Pointer] + FormatHeader[Pointer + 1] * 256));

                if ((entry & 0x8000) != 0)
                {
                    PaletteARGB[i][0] = (byte)0xFF;
                    PaletteARGB[i][1] = (byte)(((entry >> 10) & 0x1f) * 255 / 32);
                    PaletteARGB[i][2] = (byte)(((entry >> 5) & 0x1f) * 255 / 32);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x1f) * 255 / 32);
                }
                else
                {
                    PaletteARGB[i][0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                    PaletteARGB[i][1] = (byte)(((entry >> 8) & 0x0f) * 255 / 16);
                    PaletteARGB[i][2] = (byte)(((entry >> 4) & 0x0f) * 255 / 16);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x0f) * 255 / 16);
                }
                Pointer += 2;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); )
                {
                    try
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] >> 4][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] >> 4][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] >> 4][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] >> 4][3];
                        x2++;
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] & 0xF][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] & 0xF][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] & 0xF][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] & 0xF][3];
                        x2++;
                    }
                    catch
                    {
                        x2 += 2;
                        return true;
                    }
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_09600000 : VrDecoder
    {
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return width;
        }
        override public int GetChunkHeight()
        {
            return height;
        }
        override public int GetChunkBpp()
        {
            return 32;
        }
        override public int GetFormatHeaderSize()
        {
            return 0;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = 0xFF;
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = Input[InPtr + 0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = Input[InPtr + 1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = Input[InPtr + 2];
                    InPtr += 4;
                }
            }
            return true;
        }
    }
    public class VrDecoder_00002809 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height;
        override public int GetChunkWidth()
        {
            return 8;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkBpp()
        {
            return 1;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
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

                if ((entry & 0x8000) != 0)
                {
                    PaletteARGB[i][0] = (byte)0xFF;
                    PaletteARGB[i][1] = (byte)(((entry >> 10) & 0x1f) * 255 / 32);
                    PaletteARGB[i][2] = (byte)(((entry >> 5) & 0x1f) * 255 / 32);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x1f) * 255 / 32);
                }
                else
                {
                    PaletteARGB[i][0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                    PaletteARGB[i][1] = (byte)(((entry >> 8) & 0x0f) * 255 / 16);
                    PaletteARGB[i][2] = (byte)(((entry >> 4) & 0x0f) * 255 / 16);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x0f) * 255 / 16);
                }
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
    public class VrDecoder_09680000 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[16][];
        private bool init = false;
        private int width, height, outptr;
        override public int GetChunkWidth()
        {
            return width;
        }
        override public int GetChunkHeight()
        {
            return height;
        }
        override public int GetChunkBpp()
        {
            return 4;
        }
        override public int GetFormatHeaderSize()
        {
            return 16 * 4;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 16; i++)
            {
                PaletteARGB[i] = new byte[4];

                uint entry = (uint)(FormatHeader[Pointer + 0] << 24 | FormatHeader[Pointer + 1] << 16 | FormatHeader[Pointer + 2] << 8 | FormatHeader[Pointer + 3]);

                PaletteARGB[i][0] = (byte)((entry & 0xFF) * 0xFF / 0x80);
                PaletteARGB[i][1] = (byte)((entry >> 24) & 0xFF);
                PaletteARGB[i][2] = (byte)((entry >> 16) & 0xFF);
                PaletteARGB[i][3] = (byte)((entry >> 8)  & 0xFF);

                Pointer += 4;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle the image if we have not yet started work on it
            if (x1 == 0 && y1 == 0)
                PS2GsSwizzle.UnSwizzle4(Input, width, height, InPtr);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); )
                {
                    try
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] >> 4][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] >> 4][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] >> 4][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] >> 4][3];
                        x2++;
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] & 0xF][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] & 0xF][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] & 0xF][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] & 0xF][3];
                        x2++;
                    }
                    catch
                    {
                        x2 += 2;
                        return true;
                    }
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_09690000 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[16][];
        private bool init = false;
        private int width, height, outptr;
        override public int GetChunkWidth()
        {
            return 4;
        }
        override public int GetChunkHeight()
        {
            return 4;
        }
        override public int GetChunkBpp()
        {
            return 4;
        }
        override public int GetFormatHeaderSize()
        {
            return 16 * 4;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 16; i++)
            {
                PaletteARGB[i] = new byte[4];

                uint entry = (uint)(FormatHeader[Pointer + 0] << 24 | FormatHeader[Pointer + 1] << 16 | FormatHeader[Pointer + 2] << 8 | FormatHeader[Pointer + 3]);

                PaletteARGB[i][0] = (byte)((entry & 0xFF) * 0xFF / 0x80);
                PaletteARGB[i][1] = (byte)((entry >> 24) & 0xFF);
                PaletteARGB[i][2] = (byte)((entry >> 16) & 0xFF);
                PaletteARGB[i][3] = (byte)((entry >> 8)  & 0xFF);

                Pointer += 4;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle the image if we have not yet started work on it
            if (x1 == 0 && y1 == 0)
                PS2GsSwizzle.UnSwizzle4(Input, width, height, InPtr);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); )
                {
                    try
                    {
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] >> 4][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] >> 4][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] >> 4][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] >> 4][3];
                        x2++;
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[Input[InPtr] & 0xF][0];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[Input[InPtr] & 0xF][1];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[Input[InPtr] & 0xF][2];
                        Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[Input[InPtr] & 0xF][3];
                        x2++;
                    }
                    catch
                    {
                        x2 += 2;
                        return true;
                    }
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_096C0000 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height, outptr;
        override public int GetChunkWidth()
        {
            return width;
        }
        override public int GetChunkHeight()
        {
            return height;
        }
        override public int GetChunkBpp()
        {
            return 8;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 4;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 256; i++)
            {
                PaletteARGB[i] = new byte[4];

                uint entry = (uint)(FormatHeader[Pointer + 0] << 24 | FormatHeader[Pointer + 1] << 16 | FormatHeader[Pointer + 2] << 8 | FormatHeader[Pointer + 3]);

                PaletteARGB[i][0] = (byte)((entry & 0xFF) * 0xFF / 0x80);
                PaletteARGB[i][1] = (byte)((entry >> 24) & 0xFF);
                PaletteARGB[i][2] = (byte)((entry >> 16) & 0xFF);
                PaletteARGB[i][3] = (byte)((entry >> 8)  & 0xFF);

                Pointer += 4;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle the image if we have not yet started work on it
            if (x1 == 0 && y1 == 0)
                PS2GsSwizzle.UnSwizzle8(Input, width, height, InPtr);
            
            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Swap 4th and 5th bit in entry
                    byte entry = (byte)((Input[InPtr] & 0xE7) | ((Input[InPtr] >> 4 & 0x1) << 3) | ((Input[InPtr] >> 3 & 0x1) << 4));

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[entry][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[entry][3];
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_09640000 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height, outptr;
        override public int GetChunkWidth()
        {
            return width;
        }
        override public int GetChunkHeight()
        {
            return height;
        }
        override public int GetChunkBpp()
        {
            return 8;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 4;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 256; i++)
            {
                PaletteARGB[i] = new byte[4];

                uint entry = (uint)(FormatHeader[Pointer + 0] << 24 | FormatHeader[Pointer + 1] << 16 | FormatHeader[Pointer + 2] << 8 | FormatHeader[Pointer + 3]);

                PaletteARGB[i][0] = (byte)((entry & 0xFF) * 0xFF / 0x80);
                PaletteARGB[i][1] = (byte)((entry >> 24) & 0xFF);
                PaletteARGB[i][2] = (byte)((entry >> 16) & 0xFF);
                PaletteARGB[i][3] = (byte)((entry >> 8) & 0xFF);

                Pointer += 4;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle the image if we have not yet started work on it
            if (x1 == 0 && y1 == 0)
                PS2GsSwizzle.UnSwizzle8(Input, width, height, InPtr);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Swap 4th and 5th bit in entry
                    byte entry = (byte)((Input[InPtr] & 0xE7) | ((Input[InPtr] >> 4 & 0x1) << 3) | ((Input[InPtr] >> 3 & 0x1) << 4));

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[entry][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[entry][3];
                    InPtr++;
                }
            }
            return true;
        }
    }
    public class VrDecoder_08640000 : VrDecoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool init = false;
        private int width, height, outptr;
        override public int GetChunkWidth()
        {
            return width;
        }
        override public int GetChunkHeight()
        {
            return height;
        }
        override public int GetChunkBpp()
        {
            return 8;
        }
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool NeedExternalPalette()
		{
			return false;
		}
        override public bool SendExternalPalette(byte[] ExtPal, int PalSize)
		{
			return false;
		}
        override public bool Initialize(byte[] ImageHeader, int Width, int Height)
        {
            init = true;
            width = Width;
            height = Height;
            return true;
        }
        override public bool DecodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!init) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 256; i++)
            {
                PaletteARGB[i] = new byte[4];

                // Get 16-bit palette Entry
                ushort entry = BitConverter.ToUInt16(FormatHeader, Pointer);
                if ((entry & 0x8000) != 0)
                {
                    PaletteARGB[i][0] = (byte)0xFF;
                    PaletteARGB[i][1] = (byte)(((entry >> 10) & 0x1f) * 255 / 32);
                    PaletteARGB[i][2] = (byte)(((entry >> 5) & 0x1f) * 255 / 32);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x1f) * 255 / 32);
                }
                else
                {
                    PaletteARGB[i][0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                    PaletteARGB[i][1] = (byte)(((entry >> 8) & 0x0f) * 255 / 16);
                    PaletteARGB[i][2] = (byte)(((entry >> 4) & 0x0f) * 255 / 16);
                    PaletteARGB[i][3] = (byte)(((entry >> 0) & 0x0f) * 255 / 16);
                }
                Pointer += 2;
            }

            return true;
        }
        override public bool DecodeChunk(ref byte[] Input, ref int InPtr, ref byte[] Output, int x1, int y1)
        {
            if (!init) throw new Exception("Could not decode chunk because you have not initalized yet.");

            // Unswizzle the image if we have not yet started work on it
            if (x1 == 0 && y1 == 0)
                PS2GsSwizzle.UnSwizzle8(Input, width, height, InPtr);

            for (int y2 = 0; y2 < GetChunkHeight(); y2++)
            {
                for (int x2 = 0; x2 < GetChunkWidth(); x2++)
                {
                    // Swap 4th and 5th bit in entry
                    //byte entry = Input[InPtr];
                    byte entry = (byte)((Input[InPtr] & 0xE7) | ((Input[InPtr] >> 4 & 0x1) << 3) | ((Input[InPtr] >> 3 & 0x1) << 4));

                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 0] = PaletteARGB[entry][0];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 1] = PaletteARGB[entry][1];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 2] = PaletteARGB[entry][2];
                    Output[((y2 + y1) * width + (x1 + x2)) * 4 + 3] = PaletteARGB[entry][3];
                    InPtr++;
                }
            }
            return true;
        }
    }
}
