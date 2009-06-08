using System;

namespace VrSharp
{
    public static class PvrTwiddle
    {
        // Untwiddling routine for 8, 16, and 32 bit textures
        public static void UnTwiddle(ref byte[] Buf, int Pointer, int Width, int Height, int Bpp)
        {
            // Make a copy of the twiddled input
            byte[] Twiddled = new byte[Width * Height * (Bpp / 8)];
            Array.Copy(Buf, Pointer, Twiddled, 0, Twiddled.Length);

            // Get the size of the square
            int Size      = (Width > Height ? Height : Width);
            int PixelSize = (Bpp / 8);
            int Power     = (int)Math.Log(Size, 2);
            int PowerPixelSize = (int)Math.Log(Size * PixelSize, 2);
            int PowerWidth  = (int)Math.Log(Width, 2);
            int PowerHeight = (int)Math.Log(Height, 2);

            for (int y = 0; y < Height; y++)
            {
                // Get y twiddled position
                int TwiddlePositionY = 0;
                for (int i = 0; i <= Power; i++)
                    TwiddlePositionY |= (((y % Width) & (1 << i)) << i);

                for (int x = 0; x < Width; x++)
                {
                    for (int p = 0; p < PixelSize; p++)
                    {
                        // Get x twiddled position
                        int TwiddlePositionX = 0;
                        for (int i = 0; i <= PowerPixelSize; i++)
                            TwiddlePositionX |= (((((x % Height) * PixelSize) + p) & (1 << i)) << i);

                        // Get twiddled offset
                        int TwiddleOffset = ((x >> PowerHeight) | (y >> PowerWidth)) * Size * Size * PixelSize;

                        Buf[Pointer + (y * Width * PixelSize) + (x * PixelSize) + p] = Twiddled[TwiddleOffset + (TwiddlePositionX | (TwiddlePositionY << 1))];
                    }
                }
            }
        }

        // Untwiddling routine for 4 bit textures
        public static void UnTwiddle4(ref byte[] Buf, int Pointer, int Width, int Height)
        {
            // Make a copy of the twiddled input
            byte[] Twiddled = new byte[Width * Height / 2];
            Array.Copy(Buf, Pointer, Twiddled, 0, Twiddled.Length);

            // Get the size of the square
            int Size  = (Width > Height ? Height : Width);
            int Power = (int)Math.Log(Size, 2);
            int PowerWidth  = (int)Math.Log(Width, 2);
            int PowerHeight = (int)Math.Log(Height, 2);

            for (int y = 0; y < Height; y++)
            {
                // Get y twiddled position
                int TwiddlePositionY = 0;
                for (int i = 0; i <= Power; i++)
                    TwiddlePositionY |= (((y % Width) & (1 << i)) << i);

                for (int x = 0; x < Width; x++)
                {
                    // Get x twiddled position
                    int TwiddlePositionX = 0;
                    for (int i = 0; i <= Power; i++)
                        TwiddlePositionX |= (((x % Height) & (1 << i)) << i);

                    // Get twiddled offset
                    int BufferOffset  = ((y * Width) + x) >> 1;
                    int TwiddleOffset = (((x >> PowerHeight) | (y >> PowerWidth)) * Size * Size) >> 1;

                    Buf[Pointer + BufferOffset] = (byte)((Buf[Pointer + BufferOffset] & (0xF0 >> ((x % 2) * 4))) | (Twiddled[TwiddleOffset + ((TwiddlePositionX | (TwiddlePositionY << 1)) >> 1)] & (0xF << ((x % 2) * 4))));
                }
            }
        }

        // Twiddling routine for 8, 16, and 32 bit textures
        public static void Twiddle(ref byte[] Buf, int Pointer, int Width, int Height, int Bpp)
        {
            // Make a copy of the twiddled input
            byte[] UnTwiddled = new byte[Width * Height * (Bpp / 8)];
            Array.Copy(Buf, Pointer, UnTwiddled, 0, UnTwiddled.Length);

            // Get the size of the square
            int Size      = (Width > Height ? Height : Width);
            int PixelSize = (Bpp / 8);
            int Power     = (int)Math.Log(Size, 2);
            int PowerPixelSize = (int)Math.Log(Size * PixelSize, 2);
            int PowerWidth     = (int)Math.Log(Width, 2);
            int PowerHeight    = (int)Math.Log(Height, 2);

            for (int y = 0; y < Height; y++)
            {
                // Get y twiddled position
                int TwiddlePositionY = 0;
                for (int i = 0; i <= Power; i++)
                    TwiddlePositionY |= (((y % Width) & (1 << i)) << i);

                for (int x = 0; x < Width; x++)
                {
                    for (int p = 0; p < PixelSize; p++)
                    {
                        // Get x twiddled position
                        int TwiddlePositionX = 0;
                        for (int i = 0; i <= PowerPixelSize; i++)
                            TwiddlePositionX |= (((((x % Height) * PixelSize) + p) & (1 << i)) << i);

                        // Get twiddled offset
                        int TwiddleOffset = ((x >> PowerHeight) | (y >> PowerWidth)) * Size * Size * PixelSize;

                        Buf[Pointer + TwiddleOffset + (TwiddlePositionX | (TwiddlePositionY << 1))] = UnTwiddled[(y * Width * PixelSize) + (x * PixelSize) + p];
                    }
                }
            }
        }
    }
}