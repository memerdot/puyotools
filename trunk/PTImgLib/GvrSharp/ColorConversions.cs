// ColorConversions.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

using System;
using System.Collections.Generic;
using System.Text;

namespace GvrSharp
{
    public class ColorConversions
    {
        public static ushort swap16(ushort x)
        {
            return (ushort)((x << 8) | (x >> 8));
        }
        // - Color Conversion Conventions -
        // public static bool Get[ColorFormatInCamelCase](ref byte[] Src, int Offset, ref byte[] Dest)
        // Src = The bytes containing the color. Passed as reference for speed, obviously.
        // SOffset = The bytes into Src to get the color from.
        // Dest = The bytes to store the color
        // DOffset = The bytes into Dest to store the color at
        public static bool GetRgb565(ref byte[] Src, int SOffset, ref byte[] Dest, int DOffset)
        {
            ushort entry = swap16((ushort)(Src[SOffset + 0] | Src[SOffset + 1] << 8));
            Dest[DOffset + 0] = 0xFF;
            Dest[DOffset + 1] = (byte)(((entry >> 11) & 0x1f) * 256 / 32);
            Dest[DOffset + 2] = (byte)(((entry >> 5) & 0x3f) * 256 / 64);
            Dest[DOffset + 3] = (byte)(((entry >> 0) & 0x1f) * 256 / 32);
            return true;
        }
        public static bool ToRgb565(ref byte[] Src, int SOffset, ref byte[] Dest, int DOffset)
        {
            int r5 = Src[1] >> 3;
            int g6 = Src[2] >> 2;
            int b5 = Src[3] >> 3;
            int entry = b5 + (g6<<5) + (r5<<(5+6));
            Dest[DOffset + 1] = (byte)(entry >> 8 & 0xFF);
            Dest[DOffset + 0] = (byte)(entry >> 0 & 0xFF);
            return true;
        }
        public static bool GetRgb5a3(ref byte[] Src, int SOffset, ref byte[] Dest, int DOffset)
        {
            ushort entry = swap16((ushort)(Src[SOffset + 0] | Src[SOffset + 1] << 8));
            if ((entry & 0x8000) != 0)
            {
                Dest[DOffset + 0] = 0xFF;
                Dest[DOffset + 1] = (byte)(((entry >> 10) & 0x1f) * 256 / 32);
                Dest[DOffset + 2] = (byte)(((entry >> 5) & 0x1f) * 256 / 32);
                Dest[DOffset + 3] = (byte)(((entry >> 0) & 0x1f) * 256 / 32);
            }
            else
            {
                Dest[DOffset + 0] = (byte)(((entry >> 12) & 0x07) * 256 / 8);
                Dest[DOffset + 1] = (byte)(((entry >> 8) & 0x0f) * 256 / 16);
                Dest[DOffset + 2] = (byte)(((entry >> 4) & 0x0f) * 256 / 16);
                Dest[DOffset + 3] = (byte)(((entry >> 0) & 0x0f) * 256 / 16);
            }
            return true;
        }
    }
}
