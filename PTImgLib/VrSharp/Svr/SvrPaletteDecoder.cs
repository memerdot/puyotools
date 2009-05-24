using System;

namespace VrSharp
{
    public abstract class SvrPaletteDecoder : VrPaletteDecoder
    {
    }

    // Format 08 (RGB5A3)
    public class SvrPaletteDecoder_08 : SvrPaletteDecoder
    {
        public override int GetBpp()
        {
            return 16;
        }

        public override bool DecodePalette(ref byte[] Buf, int Pointer, int Colors, ref byte[][] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                Palette[i] = new byte[4];

                // Get Palette Entry
                ushort entry = BitConverter.ToUInt16(Buf, Pointer);

                if ((entry & 0x8000) != 0)
                {
                    Palette[i][0] = (byte)0xFF;
                    Palette[i][1] = (byte)(((entry >> 10) & 0x1f) * 255 / 32);
                    Palette[i][2] = (byte)(((entry >> 5) & 0x1f) * 255 / 32);
                    Palette[i][3] = (byte)(((entry >> 0) & 0x1f) * 255 / 32);
                }
                else
                {
                    Palette[i][0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                    Palette[i][1] = (byte)(((entry >> 8) & 0x0f) * 255 / 16);
                    Palette[i][2] = (byte)(((entry >> 4) & 0x0f) * 255 / 16);
                    Palette[i][3] = (byte)(((entry >> 0) & 0x0f) * 255 / 16);
                }
                Pointer += 2;
            }

            return true;
        }
    }

    // Format 09 (RGBA8888)
    public class SvrPaletteDecoder_09 : SvrPaletteDecoder
    {
        public override int GetBpp()
        {
            return 32;
        }

        public override bool DecodePalette(ref byte[] Buf, int Pointer, int Colors, ref byte[][] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                Palette[i] = new byte[4];

                // Get Palette Entry
                uint entry = BitConverter.ToUInt32(Buf, Pointer);

                Palette[i][0] = (byte)(((entry >> 24) & 0xFF) == 0x80 ? 0xFF : (((entry >> 24) & 0x7F) << 1));
                Palette[i][1] = (byte)(entry & 0xFF);
                Palette[i][2] = (byte)((entry >> 8)  & 0xFF);
                Palette[i][3] = (byte)((entry >> 16) & 0xFF);

                Pointer += 4;
            }

            return true;
        }
    }
}