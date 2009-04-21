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
    class VrColorQuantize
    {
        public byte[] QuantizedImage;
        public List<int> NewPalette = new List<int>(256);
        public VrColorQuantize(ref byte[] Data, int Width, int Height)
        {
            //Bitmap TmpBmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if ((y * Width + x) * 4 + 4 > Data.Length) break;
                    int a = Data[(y * Width + x) * 4 + 0];
                    int r = Data[(y * Width + x) * 4 + 1];
                    int g = Data[(y * Width + x) * 4 + 2];
                    int b = Data[(y * Width + x) * 4 + 3];
                    int argb = a << 24 | r << 16 | g << 8 | b;
                    for (int i = 0; i < 256; i++)
                    {
                        if (i >= NewPalette.Count)
                        {
                            NewPalette.Add(argb);
                            string ustr = ((uint)argb).ToString("X").PadLeft(8, '0');
                            //Console.WriteLine("Palette [" + i + "] = " + ustr);
                        }
                        if (argb == NewPalette[i])
                        {
                            i = 256;
                        }
                    }
                    //TmpBmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            /*Quantizer ImgQuant = new OctreeQuantizer(255, 8);
            Bitmap QuantBmp = ImgQuant.Quantize(TmpBmp);
            ColorPalette Pal = QuantBmp.Palette;

            QuantizedImage = new byte[QuantBmp.Width * QuantBmp.Height];


            for (int i = 0; i < QuantBmp.Palette.Entries.Length; i++)
            {
                NewPalette.Add(QuantBmp.Palette.Entries[i].ToArgb());
            }

            for (int y = 0; y < QuantBmp.Height; y++)
            {
                for (int x = 0; x < QuantBmp.Width; x++)
                {
                    int pxc = TmpBmp.GetPixel(x, y).ToArgb();
                    QuantizedImage[y * QuantBmp.Width + x] = (byte)NewPalette.BinarySearch(pxc);
                }
            }*/
            /*
            for (int y = 0; y < QuantBmp.Height; y++)
            {
                for (int x = 0; x < QuantBmp.Width; x++)
                {
                    if (y * Width + x > QuantizedImage.Length) break;
                    int pxc = QuantBmp.GetPixel(x, y).ToArgb();
                    QuantizedImage[y * Width + x] = (byte)NewPalette.BinarySearch(pxc);
                }
            }
            */
        }
    }
}
