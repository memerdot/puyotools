using System;

namespace VrSharp
{
    public abstract class GvrPaletteDecoder : VrPaletteDecoder
    {
    }

    // Format 18 (RGB565)
    public class GvrPaletteDecoder_18 : GvrPaletteDecoder
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
                ushort entry = ColorConversions.swap16(BitConverter.ToUInt16(Buf, Pointer));

                Palette[i][0] = 0xFF;
                Palette[i][1] = (byte)((((entry) >> 8) & 0xF8) | ((entry) >> 13));
                Palette[i][2] = (byte)((((entry) >> 3) & 0xFC) | (((entry) >> 9) & 0x03));
                Palette[i][3] = (byte)((((entry) << 3) & 0xF8) | (((entry) >> 2) & 0x07));

                Pointer += 2;
            }

            return true;
        }
    }

    // Format 28 (RGB5A3)
    public class GvrPaletteDecoder_28 : GvrPaletteDecoder
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
                ushort entry = ColorConversions.swap16(BitConverter.ToUInt16(Buf, Pointer));

                if ((entry & 0x8000) != 0)
                {
                    Palette[i][0] = (byte)0xFF;
                    Palette[i][1] = (byte)(((entry >> 10) & 0x1F) * 255 / 32);
                    Palette[i][2] = (byte)(((entry >> 5)  & 0x1F) * 255 / 32);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 255 / 32);
                }
                else
                {
                    Palette[i][0] = (byte)(((entry >> 12) & 0x07) * 255 / 8);
                    Palette[i][1] = (byte)(((entry >> 8)  & 0x0F) * 255 / 16);
                    Palette[i][2] = (byte)(((entry >> 4)  & 0x0F) * 255 / 16);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x0F) * 255 / 16);
                }
                Pointer += 2;
            }

            return true;
        }
    }
}