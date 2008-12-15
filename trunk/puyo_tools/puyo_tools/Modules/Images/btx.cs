using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace puyo_tools
{
    public class BTX
    {
        /* GMP Images */
        public BTX()
        {
        }

        /* Unpack a GMP into a Bitmap */
        public Bitmap unpack(byte[] data)
        {
            try
            {
                /* Set image variables */
                int imageWidth  = 128;  // Width
                int imageHeight = 90;  // Height
                int bitDepth    = 8; // Bit Depth
                //int colors      = 256; // Pallete Entries

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
                        /* Is it the palette? */
                        //FileStream file = new FileStream("SLPS_031.14", FileMode.Open, FileAccess.Read);
                        //byte[] newData = new byte[0x400];
                        //file.Position = 0x1DFC4;
                        //file.Read(newData, 0, 0x400);
                        //file.Close();

                        /* Write the pallete. */
                        //ColorPalette palette = image.Palette;
                        //for (int i = 0; i < colors; i++)
                        //    palette.Entries[i] = Color.FromArgb(newData[(i * 0x4) + 0], newData[(i * 0x4) + 1], newData[(i * 0x4) + 2]);
                            //palette.Entries[i] = Color.FromArgb(data[0x20 + (i * 0x4) + 0x2], data[0x20 + (i * 0x4) + 0x1], data[0x20 + (i * 0x4)]);

                        //image.Palette = palette;
                        //int pos = 0;

                        for (int y = 0; y < imageHeight; y++)
                        {
                            for (int x = 0; x < imageWidth; x++)
                            {
                                byte* rowData = (byte*)imageData.Scan0 + (y * imageData.Stride);

                                /* Set the image colors now. Copy bytes from GMP. */
                                //rowData[x] = data[(imageWidth * imageHeight) - ((y + 1) * imageWidth) + x];
                                rowData[x] = data[(y * imageWidth) + x];
                                //rowData[x]     = (byte)((data[pos] >> 2));
                                //rowData[x + 1] = (byte)((data[pos] << 2) | (data[pos] >> 4));
                                //rowData[x + 2] = (byte)((data[pos] << 4) | (data[pos] >> 6));
                                //rowData[x + 3] = (byte)((data[pos] << 6) | (data[pos] >> 8));
                                //pos += 4;
                            }
                        }
                    }
                }

                /* Unlock the bits now. */
                image.UnlockBits(imageData);

                return image;
            }

            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show(f.ToString());
                return new Bitmap(0, 0);
            }
        }

        /* Pack a Bitmap into a GMP */
        public byte[] pack(Bitmap image)
        {
            try
            {
                /* Set image variables */
                int imageWidth = image.Width;  // Width
                int imageHeight = image.Height; // Height
                PixelFormat format = image.PixelFormat; // Pixel Format

                uint headerSize = 0x20;  // Header Size
                uint imageDataStart = 0x420; // Image Data Start

                short colors = 256; // Colors in Pallete
                short bitDepth = 8;   // Bit Depth

                /* Is the image an 8-bit image? */
                if (format == PixelFormat.Format8bppIndexed)
                {
                    /* Create the data. */
                    byte[] data = new byte[imageDataStart + (imageWidth * imageHeight)];

                    /* Write the header */
                    Array.Copy(BitConverter.GetBytes((uint)GraphicHeader.GMP), 0x0, data, 0x0, 4); // GMP-
                    Array.Copy(BitConverter.GetBytes((uint)GraphicHeader.GMP2), 0x0, data, 0x4, 4); // 200

                    Array.Copy(BitConverter.GetBytes(imageHeight), 0x0, data, 0x8, 4); // Image Height
                    Array.Copy(BitConverter.GetBytes(imageWidth), 0x0, data, 0xC, 4); // Image Width

                    Array.Copy(BitConverter.GetBytes(headerSize), 0x0, data, 0x14, 4); // Header Size
                    Array.Copy(BitConverter.GetBytes(imageDataStart), 0x0, data, 0x18, 4); // Image Data Start

                    Array.Copy(BitConverter.GetBytes(colors), 0x0, data, 0x1C, 2); // Colors in Palette
                    Array.Copy(BitConverter.GetBytes(bitDepth), 0x0, data, 0x1E, 2); // Bit Depth

                    /* Set up the palette. */
                    for (int i = 0; i < colors; i++)
                    {
                        data[headerSize + (i * 4)] = image.Palette.Entries[i].B;
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

                /* Not an 8-bit image. */
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