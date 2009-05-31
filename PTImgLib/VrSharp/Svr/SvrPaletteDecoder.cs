using System;

namespace VrSharp
{
    public abstract class SvrPaletteDecoder : VrPaletteDecoder
    {
        public static void ReorderPalette(ref byte[][] Palette)
        {
            // Make a copy of the palette
            byte[][] PaletteCopy = new byte[256][];
            for (int i = 0; i < 256; i++)
                PaletteCopy[i] = Palette[i];

            // Now reorder the palette
            for (int i = 0; i < 256; i++)
            {
                byte entry = (byte)((i & 0xE7) | ((i >> 4 & 0x1) << 3) | ((i >> 3 & 0x1) << 4));
                Palette[i] = PaletteCopy[entry];
            }
        }
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
                    Palette[i][0] = 0xFF;
                    Palette[i][1] = (byte)(((entry >> 10) & 0x1F) * 0xFF / 0x1F);
                    Palette[i][2] = (byte)(((entry >> 5)  & 0x1F) * 0xFF / 0x1F);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x1F) * 0xFF / 0x1F);
                }
                else
                {
                    Palette[i][0] = (byte)(((entry >> 12) & 0x07) * 0xFF / 0x07);
                    Palette[i][1] = (byte)(((entry >> 8)  & 0x0F) * 0xFF / 0x0F);
                    Palette[i][2] = (byte)(((entry >> 4)  & 0x0F) * 0xFF / 0x0F);
                    Palette[i][3] = (byte)(((entry >> 0)  & 0x0F) * 0xFF / 0x0F);
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