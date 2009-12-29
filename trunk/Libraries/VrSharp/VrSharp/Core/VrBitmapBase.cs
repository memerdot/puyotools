// Not implimented at the moment
// Once work on converting to a vr format
// is started then this code will be uncommented.
// Otherwise it will stay commented to save some
// bytes in the build of VrSharp.

/*
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace VrSharp
{
    public class VrBitmapBase
    {
        // Converts a bitmap to a raw image
        protected byte[] ConvertBitmapToRaw(Bitmap bitmap)
        {
            byte[] output = new byte[bitmap.Width * bitmap.Height * 4];

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    output[(((y * bitmap.Width) + x) * 4) + 3] = pixel.A;
                    output[(((y * bitmap.Width) + x) * 4) + 2] = pixel.R;
                    output[(((y * bitmap.Width) + x) * 4) + 1] = pixel.G;
                    output[(((y * bitmap.Width) + x) * 4) + 0] = pixel.B;
                }
            }

            return output;
        }

        // Converts a stream (containing a bitmap) to a raw image
        protected byte[] ConvertStreamToRaw(Stream stream)
        {
            Bitmap bitmap = null;
            try   { bitmap = new Bitmap(stream); } // In case this isn't an image.
            catch { return null; }

            return ConvertBitmapToRaw(bitmap);
        }

        // Opens a file (containg the bitmap) and converts it to a raw image
        protected byte[] ConvertFileToRaw(string file)
        {
            Bitmap bitmap = null;
            try   { bitmap = new Bitmap(file); } // In case this isn't an image.
            catch { return null; }

            return ConvertBitmapToRaw(bitmap);
        }

        // Converts a byte array (containing the bitmap) and converts it to a raw image
        protected byte[] ConvertArraytoRaw(byte[] array)
        {
            return ConvertStreamToRaw(new MemoryStream(array));
        }

        // Swap an unsigned short
        protected ushort SwapUShort(ushort x)
        {
            return (ushort)((x << 8) | (x >> 8));
        }

        // Swap an unsigned integer
        protected uint SwapUInt(uint x)
        {
            return (x >> 24) |
                ((x << 8) & 0x00FF0000) |
                ((x >> 8) & 0x0000FF00) |
                (x << 24);
        }

        // Copies a string to an array
        protected void CopyToArray(byte[] array, int offset, string str)
        {
            for (int i = 0; i < str.Length && offset + i < array.Length; i++)
                array[offset + i] = (byte)str[i];
        }

        // Copies an unsigned short to an array
        protected void CopyToArray(byte[] array, int offset, ushort n)
        {
            Array.Copy(BitConverter.GetBytes(n), 0, array, offset, 2);
        }

        // Copies an unsigned integer to an array
        protected void CopyToArray(byte[] array, int offset, uint n)
        {
            Array.Copy(BitConverter.GetBytes(n), 0, array, offset, 4);
        }
    }
}
*/