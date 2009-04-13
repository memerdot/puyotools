using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
            return null;
        }
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
                    /* Is this an 8-bit indexed image? */
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

        public override Stream Pack(ref Bitmap image)
        {
            return null;
        }

        /* Pack a Bitmap into a GMP */
        public byte[] pack(Bitmap image)
        {
            try
            {
                /* Set image variables */
                int imageWidth     = image.Width;       // Width
                int imageHeight    = image.Height;      // Height
                PixelFormat format = image.PixelFormat; // Pixel Format

                short colors   = (short)image.Palette.Entries.Length; // Colors in Pallete
                short bitDepth = 8;                                   // Bit Depth

                uint headerSize     = 0x20;                            // Header Size
                uint imageDataStart = headerSize + (uint)(colors * 4); // Image Data Start

                /* Is the image an 8-bit indexed image? */
                if (format == PixelFormat.Format8bppIndexed)
                {
                    /* Create the data. */
                    byte[] data = new byte[imageDataStart + (imageWidth * imageHeight)];

                    /* Write the header */
                    Array.Copy(BitConverter.GetBytes((uint)GraphicHeader.GMP),  0x0, data, 0x0, 4); // GMP-
                    //Array.Copy(BitConverter.GetBytes((uint)GraphicHeader.GMP2), 0x0, data, 0x4, 4); // 200

                    Array.Copy(BitConverter.GetBytes(imageHeight), 0x0, data, 0x8, 4); // Image Height
                    Array.Copy(BitConverter.GetBytes(imageWidth),  0x0, data, 0xC, 4); // Image Width

                    Array.Copy(BitConverter.GetBytes(headerSize),     0x0, data, 0x14, 4); // Header Size
                    Array.Copy(BitConverter.GetBytes(imageDataStart), 0x0, data, 0x18, 4); // Image Data Start

                    Array.Copy(BitConverter.GetBytes(colors),   0x0, data, 0x1C, 2); // Colors in Palette
                    Array.Copy(BitConverter.GetBytes(bitDepth), 0x0, data, 0x1E, 2); // Bit Depth

                    /* Set up the palette. */
                    for (int i = 0; i < colors; i++)
                    {
                        data[headerSize + (i * 4)]     = image.Palette.Entries[i].B;
                        data[headerSize + (i * 4) + 1] = image.Palette.Entries[i].G;
                        data[headerSize + (i * 4) + 2] = image.Palette.Entries[i].R;
                    }

                    unsafe
                    {
                        /* Set up the old image. */
                        BitmapData imageData = image.LockBits(
                            new Rectangle(0, 0, imageWidth, imageHeight),
                            ImageLockMode.ReadOnly, image.PixelFormat);
                        

                        /* Write the new data. */
                        for (int y = 0; y < imageHeight; y++)
                        {
                            for (int x = 0; x < imageWidth; x++)
                            {
                                byte* rowData = (byte*)imageData.Scan0 + (y * imageData.Stride);

                                /* Set the image colors now. Copy bytes from GMP. */
                                data[imageDataStart + (imageWidth * imageHeight) - ((y + 1) * imageWidth) + x] = rowData[x];
                            }
                        }

                        /* Unlock the image */
                        image.UnlockBits(imageData);
                    }

                    return data;
                }

                /* Not an 8-bit indexed image. */
                else
                    return new byte[0];
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}