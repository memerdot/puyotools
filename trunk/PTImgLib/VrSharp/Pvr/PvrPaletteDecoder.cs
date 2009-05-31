using System;

namespace VrSharp
{
    public abstract class PvrPaletteDecoder : VrPaletteDecoder
    {
    }

    // Format 00 (ARGB1555)
    public class PvrPaletteDecoder_Argb1555 : PvrPaletteDecoder
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

                Palette[i][0] = (byte)(((entry >> 15) & 0x01) * 0xFF);
                Palette[i][1] = (byte)(((entry >> 10) & 0x1F) * 0xFF / 0x1F);
                Palette[i][2] = (byte)(((entry >> 5)  & 0x1F) * 0xFF / 0x1F);
                Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 0xFF / 0x1F);

                Pointer += 2;
            }

            return true;
        }
    }

    // Format 01 (RGB565)
    public class PvrPaletteDecoder_Rgb565 : PvrPaletteDecoder
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

                Palette[i][0] = 0xFF;
                Palette[i][1] = (byte)(((entry >> 11) & 0x1F) * 0xFF / 0x1F);
                Palette[i][2] = (byte)(((entry >> 5)  & 0x3F) * 0xFF / 0x3F);
                Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 0xFF / 0x1F);

                Pointer += 2;
            }

            return true;
        }
    }

    // Format 02 (ARGB4444)
    public class PvrPaletteDecoder_Argb4444 : PvrPaletteDecoder
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

                Palette[i][0] = (byte)(((entry >> 12) & 0xF) * 0xFF / 0xF);
                Palette[i][1] = (byte)(((entry >> 8)  & 0xF) * 0xFF / 0xF);
                Palette[i][2] = (byte)(((entry >> 4)  & 0xF) * 0xFF / 0xF);
                Palette[i][3] = (byte)(((entry >> 0)  & 0xF) * 0xFF / 0xF);

                Pointer += 2;
            }

            return true;
        }
    }
}