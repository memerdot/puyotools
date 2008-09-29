using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace puyo_tools
{
    public class GMP
    {
        /* GMP Images */
        public GMP()
        {
        }

        /* Unpack a GMP into a Bitmap */
        public Bitmap unpack(byte[] data)
        {
            try
            {
                /* Set image variables */
                int imageWidth  = BitConverter.ToInt32(data, 0xC);  // Width
                int imageHeight = BitConverter.ToInt32(data, 0x8);  // Height
                int bitDepth    = BitConverter.ToInt16(data, 0x1E); // Bit Depth
                int colors      = BitConverter.ToInt16(data, 0x1C); // Pallete Entries

                /* Set up the new image. */
                Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format8bppIndexed);
                BitmapData imageData = image.LockBits(
                    new Rectangle(0, 0, imageWidth, imageHeight),
                    ImageLockMode.WriteOnly, image.PixelFormat);

                /* Read the data from the GMP */
                unsafe
                {
                    /* Is this an 8-bit image? */
                    if (bitDepth == 8)
                    {
                        /* Write the pallete. */
                        ColorPalette palette = image.Palette;
                        for (int i = 0; i < colors; i++)
                            palette.Entries[i] = Color.FromArgb(data[0x20 + (i * 0x4) + 0x2], data[0x20 + (i * 0x4) + 0x1], data[0x20 + (i * 0x4)]);

                        image.Palette = palette;

                        for (int y = 0; y < imageHeight; y++)
                        {
                            for (int x = 0; x < imageWidth; x++)
                            {
                                byte* rowData = (byte*)imageData.Scan0 + (y * imageData.Stride);

                                /* Set the image colors now. Copy bytes from GMP. */
                                rowData[x] = data[0x20 + (colors * 0x4) + (imageWidth * imageHeight) - ((y + 1) * imageWidth) + x];
                            }
                        }
                    }
                }

                /* Unlock the bits now. */
                image.UnlockBits(imageData);

                return image;
            }

            catch
            {
                return new Bitmap(0, 0);
            }
        }

        /* Pack a Bitmap into a GMP */
        public byte[] pack(Bitmap image)
        {
            return new byte[0];
        }
    }
}