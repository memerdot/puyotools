using System;

namespace VrSharp
{
    public static class SvrSwizzle
    {
        // Unswizzle a 4-bit PS2 texture
        public static void UnSwizzle4(byte[] Buf, int Width, int Height, int Where)
        {
            // Make a copy of the swizzled input
            byte[] Swizzled = new byte[Buf.Length - Where];
            Array.Copy(Buf, Where, Swizzled, 0, Swizzled.Length);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // get the pen
                    int index = y * Width + x;
                    byte uPen = (byte)(Buf[Where + (index >> 1)] >> ((index & 1) * 4) & 0xf);

                    // swizzle
                    int pageX = x & (~0x7f);
                    int pageY = y & (~0x7f);

                    int pages_horz = (Width + 127) / 128;
                    int pages_vert = (Height + 127) / 128;

                    int page_number = (pageY / 128) * pages_horz + (pageX / 128);

                    int page32Y = (page_number / pages_vert) * 32;
                    int page32X = (page_number % pages_vert) * 64;

                    int page_location = page32Y * Height * 2 + page32X * 4;

                    int locX = x & 0x7f;
                    int locY = y & 0x7f;

                    int block_location = ((locX & (~0x1f)) >> 1) * Height + (locY & (~0xf)) * 2;
                    int swap_selector = (((y + 2) >> 2) & 0x1) * 4;
                    int posY = (((y & (~3)) >> 1) + (y & 1)) & 0x7;

                    int column_location = posY * Height * 2 + ((x + swap_selector) & 0x7) * 4;

                    int byte_num = (x >> 3) & 3;     // 0,1,2,3
                    int bits_set = (y >> 1) & 1;     // 0,1            (lower/upper 4 bits)

                    //byte setPix = Swizzled[page_location + block_location + column_location + byte_num];

                    if ((index & 1) == 0)
                        Buf[Where + (index >> 1)] = 0x0;

                    Buf[Where + (index >> 1)] |= (byte)((Swizzled[page_location + block_location + column_location + byte_num] & -bits_set));

                    //Buf[Where + (index >> 1)] |= (byte)((setPix & (-bits_set)) | (uPen << (bits_set * 4)));

                    //Buf[Where + ((setPix & (-bits_set)) | (uPen << (bits_set * 4)))] = setPix;
                }
            }
        }

        // Unswizzle an 8-bit PS2 texture
        public static void UnSwizzle8(byte[] Buf, int Width, int Height, int Where)
        {
            // Make a copy of the swizzled input
            byte[] Swizzled = new byte[Buf.Length - Where];
            Array.Copy(Buf, Where, Swizzled, 0, Swizzled.Length);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int block_location = (y & (~0xf)) * Width + (x & (~0xf)) * 2;
                    int swap_selector = (((y + 2) >> 2) & 0x1) * 4;
                    int posY = (((y & (~3)) >> 1) + (y & 1)) & 0x7;
                    int column_location = posY * Width * 2 + ((x + swap_selector) & 0x7) * 4;

                    int byte_num = ((y >> 1) & 1) + ((x >> 2) & 2);     // 0,1,2,3

                    Buf[Where + (y * Width) + x] = Swizzled[block_location + column_location + byte_num];
                }
            }
        }
    }
}