using System;

namespace VrSharp
{
    public static class PvrTwiddle
    {
        public static void UnTwidle(ref byte[] Buf, int Pointer, int Width, int Height)
        {
            // Make a copy of the unswizzled input
            byte[] UnSwizzled = new byte[Buf.Length - Pointer];
            Array.Copy(Buf, Pointer, UnSwizzled, 0, UnSwizzled.Length);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int blockX = x / 8;
                    int blockY = y / 8;
                }
            }
        }
    }
}