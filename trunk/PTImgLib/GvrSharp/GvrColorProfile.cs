using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BetterImageProcessorQuantization;
using System.Drawing.Imaging;

namespace GvrSharp
{
    class GvrColorQuantize
    {
        public byte[] QuantizedImage;
        public List<int> NewPalette;
        public GvrColorQuantize(ref byte[] Data, int Width, int Height)
        {
            Bitmap TmpBmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < TmpBmp.Height; y++)
            {
                for (int x = 0; x < TmpBmp.Width; x++)
                {
                    int a = Data[(y * Width + x) * 4 + 0];
                    int r = Data[(y * Width + x) * 4 + 1];
                    int g = Data[(y * Width + x) * 4 + 2];
                    int b = Data[(y * Width + x) * 4 + 3];
                    TmpBmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            Quantizer ImgQuant = new OctreeQuantizer(256, 8);
            Bitmap QuantBmp = ImgQuant.Quantize(TmpBmp);
            ColorPalette Pal = QuantBmp.Palette;

            QuantizedImage = new byte[QuantBmp.Width * QuantBmp.Height];

            for (int y = 0; y < QuantBmp.Height; y++)
            {
                for (int x = 0; x < QuantBmp.Width; x++)
                {
                    int pxc = QuantizedImage.GetPixel(x, y).ToArgb();
                    if (NewPalette.Contains(pxc))
                    {
                        NewPalette.Add(pxc);
                    }
                }
            }

            for (int y = 0; y < QuantBmp.Height; y++)
            {
                for (int x = 0; x < QuantBmp.Width; x++)
                {
                    int pxc = QuantizedImage.GetPixel(x, y).ToArgb();
                    QuantizedImage[y * Width + x] = NewPalette.BinarySearch(pxc);
                }
            }

        }
    }
}
