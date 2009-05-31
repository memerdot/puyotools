using System;

namespace VrSharp
{
    public abstract class GvrPaletteDecoder : VrPaletteDecoder
    {
    }

    // Format 02 (8-bit Lum with Alpha)
    public class GvrPaletteDecoder_02 : GvrPaletteDecoder
    {
        public override int GetBpp()
        {
            return 8;
        }

        public override bool DecodePalette(ref byte[] Buf, int Pointer, int Colors, ref byte[][] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                Palette[i] = new byte[4];

                Palette[i][0] = (byte)((Buf[Pointer] >> 4)  * 0xFF / 0xF);
                Palette[i][1] = (byte)((Buf[Pointer] & 0xF) * 0xFF / 0xF);
                Palette[i][2] = (byte)((Buf[Pointer] & 0xF) * 0xFF / 0xF);
                Palette[i][3] = (byte)((Buf[Pointer] & 0xF) * 0xFF / 0xF);
                Pointer++;
            }

            return true;
        }
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
                Palette[i][1] = (byte)(((entry >> 11) & 0x1F) * 0xFF / 0x1F);
                Palette[i][2] = (byte)(((entry >> 5)  & 0x3F) * 0xFF / 0x3F);
                Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 0xFF / 0x1F);

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
                    Palette[i][0] = 0xFF;
                    Palette[i][1] = (byte)(((entry >> 10) & 0x1F) * 0xFF / 0x1F);
                    Palette[i][2] = (byte)(((entry >> 5)  & 0x1F) * 0xFF / 0x1F);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 0xFF / 0x1F);
                }
                else
                {
                    Palette[i][0] = (byte)(((entry >> 12) & 0x07) * 0xFF / 0x7);
                    Palette[i][1] = (byte)(((entry >> 8)  & 0x0F) * 0xFF / 0xF);
                    Palette[i][2] = (byte)(((entry >> 4)  & 0x0F) * 0xFF / 0xF);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x0F) * 0xFF / 0xF);
                }
                Pointer += 2;
            }

            return true;
        }
    }
}