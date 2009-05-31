using System;

namespace VrSharp
{
    public static class PvrTwiddle
    {
        public static void UnTwiddle(ref byte[] Buf, int Pointer, int Width, int Height)
        {
            // Make a copy of the twiddled input
            byte[] Twiddled = new byte[Buf.Length - Pointer];
            Array.Copy(Buf, Pointer, Twiddled, 0, Twiddled.Length);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    Buf[Pointer + (y * Width) + x] = Twiddled[TwiddlePosition(x) | (TwiddlePosition(y) << 1)];
            }
        }

        public static int TwiddlePosition(int x)
        {
            int pos = 0;

            for (int i = 0; i < 11; i++)
                pos |= ((x & (1 << i)) << i);

            return pos;
        }
    }
}