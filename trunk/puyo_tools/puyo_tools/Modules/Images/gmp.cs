using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;

namespace puyo_tools
{
    public class GMP : ImageClass
    {
        /* GMP Images */
        public GMP()
        {
        }

        /* Unpack a GMP into a Bitmap */
        public override Bitmap Unpack(ref Stream data)
        {
            try
            {
                /* Get and set image variables */
                int width      = data.ReadInt(0xC); // Width
                int height     = data.ReadInt(0x8); // Height
                short bitDepth = data.ReadShort(0x1E); // Bit Depth
                short colors   = data.ReadShort(0x1C); // Pallete Entries

                /* Throw an exception if this is not an 8-bit gmp (for now) */
                if (bitDepth != 8)
                    throw new Exception();

                /* Set up the image */
                Bitmap image = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                BitmapData imageData = image.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly, image.PixelFormat);

                /* Read the data from the GMP */
                unsafe
                {
                    /* Write the palette */
                    ColorPalette palette = image.Palette;
                    for (int i = 0; i < colors; i++)
                        palette.Entries[i] = Color.FromArgb(data.ReadByte(0x20 + (i * 0x4) + 0x2), data.ReadByte(0x20 + (i * 0x4) + 0x1), data.ReadByte(0x20 + (i * 0x4)));

                    image.Palette = palette;

                    /* Start getting the pixels from the source image */
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte* rowData = (byte*)imageData.Scan0 + (y * imageData.Stride);
                            rowData[x] = data.ReadByte(0x20 + (colors * 0x4) + (width * height) - ((y + 1) * width) + x);
                        }
                    }
                }

                /* Unlock the bits now. */
                image.UnlockBits(imageData);

                return image;
            }
            catch
            {
                return null;
            }
        }

        /* Check to see if this is a GMP */
        public override bool Check(ref Stream input)
        {
            try
            {
                return (input.ReadString(0x0, 8, false) == GraphicHeader.GMP);
            }
            catch
            {
                return false;
            }
        }

        /* Image Information */
        public override Images.Information Information()
        {
            string Name   = "GMP";
            string Ext    = ".gmp";
            string Filter = "GMP Image (*.cnx;*.gmp)|*.cnx;*.gmp";

            bool Unpack = true;
            bool Pack   = false;

            return new Images.Information(Name, Unpack, Pack, Ext, Filter);
        }

        public override Stream Pack(ref Bitmap image)
        {
            return null;
        }
    }
}