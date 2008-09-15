// GvrEncoder.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

using System;
using System.Collections.Generic;
using System.Text;

namespace GvrSharp
{
    public abstract class GvrEncoder
    {
        // The GvrFileWidth of a chunk
        abstract public int GetChunkWidth();

        // The GvrFileHeight of a chunk
        abstract public int GetChunkHeight();

        // The bytes of a chunk
        abstract public int GetChunkSize();

        // The format header size
        abstract public int GetFormatHeaderSize();

        /// <summary>  
        /// <para>Initializes the GvrCodec.</para> 
        /// </summary>  
        /// <param name="Data">A ref to the output byte array</param>  
        /// <param name="Pointer">A ref to the output pointer</param>
        /// <returns>True if the header was encoded, False if not.</returns> 
        abstract public bool Initialize(ref byte[] Data, byte[] AuxData, int Width, int Height);

        /// <summary>  
        /// <para>Encodes the format header. The pointer should be moved to the
        /// next part of the file.</para> 
        /// </summary>  
        /// <param name="FormatHeader">A ref to the output byte array</param>  
        /// <param name="Pointer">A ref to the output pointer</param>
        /// <returns>True if the header was encoded, False if not.</returns> 
        abstract public bool EncodeFormatHeader(ref byte[] FormatHeader, ref int Pointer);

        /// <summary>  
        /// <para>Encodes a single chunk in the GvrFile. Upon success,
        /// the output pointer will be moved and true will be returned.</para> 
        /// </summary>  
        /// <param name="Output">A ref to the output byte array</param>  
        /// <param name="OutPtr">A ref to the output pointer</param>  
        /// <param name="Input">A ref to the input byte array in ARGB8888</param>  
        /// <param name="x1">The X coordinate of the current chunk</param> 
        /// <param name="y1">The Y coordinate of the current chunk</param> 
        /// <returns>True if the chunk was encoded, False if not.</returns> 
        abstract public bool EncodeChunk(ref byte[] Output, ref int OutPtr, ref byte[] Input, int x1, int y1);
    }

    public class GvrEncoder_1809 : GvrEncoder
    {
        private byte[][] PaletteARGB = new byte[256][];
        private bool Initialized = false;
        private int GvrWidth, GvrHeight;
        bool AutoQuantize = false;
        byte[] AutoQuantizeBitmap;
        public void GeneratePalette(ref byte[] Data)
        {
            AutoQuantize = true;
            GvrColorQuantize ImgQuantize = new GvrColorQuantize(ref Data, GvrWidth, GvrHeight);
            for (int i = 0; i < 256; i++)
            {
                PaletteARGB[i] = new byte[4];
                if (i >= ImgQuantize.NewPalette.Count) { Console.WriteLine("No palette entry for " + i); continue; }
                PaletteARGB[i][0] = (byte)(ImgQuantize.NewPalette[i] >> 24 & 0xFF);
                PaletteARGB[i][1] = (byte)(ImgQuantize.NewPalette[i] >> 16 & 0xFF);
                PaletteARGB[i][2] = (byte)(ImgQuantize.NewPalette[i] >> 8 & 0xFF);
                PaletteARGB[i][3] = (byte)(ImgQuantize.NewPalette[i] >> 0 & 0xFF);
            }
            AutoQuantizeBitmap = ImgQuantize.QuantizedImage;
        }
        public byte GetClosestPaletteEntry(int a, int r, int g, int b)
        {
            int ClosestEntryID=0;
            int ClosestEntryDelta=1024;
            for (int i = 0; i < PaletteARGB.Length / 4; i++)
            {
                int CurrentEntryDelta = 0;
                CurrentEntryDelta += Math.Abs(((int)PaletteARGB[i][0]) - a);
                CurrentEntryDelta += Math.Abs(((int)PaletteARGB[i][1]) - r);
                CurrentEntryDelta += Math.Abs(((int)PaletteARGB[i][2]) - g);
                CurrentEntryDelta += Math.Abs(((int)PaletteARGB[i][3]) - b);
                if (CurrentEntryDelta < ClosestEntryDelta)
                {
                    ClosestEntryID = i;
                    ClosestEntryDelta = CurrentEntryDelta;
                }
                if (CurrentEntryDelta == 0)
                {
                    break;
                }
            }
            return (byte)ClosestEntryID;
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
        override public int GetFormatHeaderSize()
        {
            return 256 * 2;
        }
        override public bool Initialize(ref byte[] Data, byte[] AuxData, int Width, int Height)
        {
            Initialized = true;
            GvrWidth = Width;
            GvrHeight = Height;

            if (AuxData == null)
            {
                GeneratePalette(ref Data);
            }
            else
            {
                int Pointer=0x20;
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
        override public bool EncodeFormatHeader(ref byte[] FormatHeader, ref int Pointer)
        {
            if (!Initialized) throw new Exception("Could not decode format header because you have not initalized yet.");

            for (int i = 0; i < 256; i++)
            {
                ColorConversions.ToRgb565(ref PaletteARGB[i], 0, ref FormatHeader, Pointer);
                Pointer += 2;
            }

            return true;
        }
        override public bool EncodeChunk(ref byte[] Output, ref int OutPtr, ref byte[] Input, int x1, int y1)
        {
            if (!Initialized) throw new Exception("Could not encode chunk because you have not initalized yet.");
            for (int y2 = 0; y2 < 4; y2++)
            {
                for (int x2 = 0; x2 < 8; x2++)
                {
                    if (OutPtr >= Output.Length) break;
                    if (AutoQuantize)
                    {
                        Output[OutPtr] = AutoQuantizeBitmap[(y2 + y1) * GvrWidth + (x1 + x2)];
                        OutPtr++;
                    }
                    else
                    {
                        int a = Input[((y2 + y1) * GvrWidth + (x1 + x2)) * 4 + 0];
                        int r = Input[((y2 + y1) * GvrWidth + (x1 + x2)) * 4 + 1];
                        int g = Input[((y2 + y1) * GvrWidth + (x1 + x2)) * 4 + 2];
                        int b = Input[((y2 + y1) * GvrWidth + (x1 + x2)) * 4 + 3];
                        byte pal = GetClosestPaletteEntry(Input[a], Input[r], Input[g], Input[b]);
                        Output[OutPtr] = pal;
                        OutPtr++;
                    }
                }
            }
            return true;
        }
    }
}
