// VrColorProfile.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BetterImageProcessorQuantization;
using System.Drawing.Imaging;

namespace VrSharp
{
    public class VrColorQuantize
    {
        // Quantize the Image (reduce the amount of colors)
        public byte[] QuantizeImage(ref byte[] Data, int Width, int Height, int Colors, int Bpp)
        {
            // Initalize the Image Quantizer
            OctreeQuantizer Quantizer = new OctreeQuantizer(Colors, Bpp);
            return BitmapToRawImage(Quantizer.Quantize(RawImageToBitmap(Data, Width, Height)));
        }

        // Generate a new palette for the image
        public byte[][] BuildPalette(ref byte[] Data, int Width, int Height, int Colors, out int[] PaletteMap)
        {
            List<byte[]> PaletteList = new List<byte[]>(Colors);
            PaletteMap = new int[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte PixelColorA = Data[(((y * Width) + x) * 4) + 0];
                    byte PixelColorR = Data[(((y * Width) + x) * 4) + 1];
                    byte PixelColorG = Data[(((y * Width) + x) * 4) + 2];
                    byte PixelColorB = Data[(((y * Width) + x) * 4) + 3];

                    // Add the entry to the list and the map
                    byte[] PaletteEntry = new byte[] {PixelColorA, PixelColorR, PixelColorG, PixelColorB};
                    int PaletteEntryIndex = PaletteList.IndexOf(PaletteEntry);
                    if (PaletteEntryIndex == -1)
                    {
                        PaletteMap[(y * Width) + x] = PaletteList.Count;
                        PaletteList.Add(PaletteEntry);
                    }
                    else
                        PaletteMap[(y * Width) + x] = PaletteEntryIndex;

                    if (PaletteList.Count == Colors) // Did we reach the max amount of colors?
                    {
                        x = Width;
                        y = Height;
                        break;
                    }
                }
            }

            return PaletteList.ToArray();
        }

        // Build a Bitmap from ARGB8888 Uncompressed Image Data
        private Bitmap RawImageToBitmap(byte[] RawImage, int Width, int Height)
        {
            Bitmap Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            BitmapData BitmapData = Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe // Uh oh, we are doing naughty pointer things ;)
            {
                byte* BitmapPointer = (byte*)BitmapData.Scan0;
                int RowJunkSize = BitmapData.Stride - (BitmapData.Width * 4);

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        BitmapPointer[0] = RawImage[(((y * Width) + x) * 4) + 3];
                        BitmapPointer[1] = RawImage[(((y * Width) + x) * 4) + 2];
                        BitmapPointer[2] = RawImage[(((y * Width) + x) * 4) + 1];
                        BitmapPointer[3] = RawImage[(((y * Width) + x) * 4) + 0];
                        BitmapPointer += 4;
                    }

                    BitmapPointer += RowJunkSize;
                }
            }

            Bitmap.UnlockBits(BitmapData);
            return Bitmap;
        }

        // Build a RGBA8888 Image from a Bitmap
        private byte[] BitmapToRawImage(Bitmap Bitmap)
        {
            byte[] RawImage = new byte[Bitmap.Width * Bitmap.Height * 4];

            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    Color PixelColor = Bitmap.GetPixel(x, y);
                    RawImage[(((y * Bitmap.Width) + x) * 4) + 0] = PixelColor.A;
                    RawImage[(((y * Bitmap.Width) + x) * 4) + 1] = PixelColor.R;
                    RawImage[(((y * Bitmap.Width) + x) * 4) + 2] = PixelColor.G;
                    RawImage[(((y * Bitmap.Width) + x) * 4) + 3] = PixelColor.B;
                }
            }

            return RawImage;
        }

        public VrColorQuantize()
        {
        }
    }
}