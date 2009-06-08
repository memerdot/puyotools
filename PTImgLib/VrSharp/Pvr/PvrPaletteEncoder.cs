using System;

namespace VrSharp
{
    public abstract class PvrPaletteEncoder : VrPaletteEncoder
    {
    }

    public class PvrPaletteEncoder_Argb1555 : VrPaletteEncoder
    {
        public override int GetBpp()
        {
            return 16;
        }
        public override bool EncodePalette(ref byte[] Buf, int Pointer, int Colors, ref uint[] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                ushort entry = 0x0; // Entry

                entry |= (ushort)(((Buf[Pointer + (i * 4) + 0] / 0xFF) & 0x01) << 15);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 1] * 0x1F / 0xFF) & 0x1F) << 10);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 2] * 0x1F / 0xFF) & 0x1F) << 5);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 3] * 0x1F / 0xFF) & 0x1F));

                Palette[i] = entry;
            }

            return true;
        }
    }

    public class PvrPaletteEncoder_Rgb565 : VrPaletteEncoder
    {
        public override int GetBpp()
        {
            return 16;
        }
        public override bool EncodePalette(ref byte[] Buf, int Pointer, int Colors, ref uint[] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                ushort entry = 0x0; // Entry

                entry |= (ushort)(((Buf[Pointer + (i * 4) + 1] * 0x1F / 0xFF) & 0x1F) << 11);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 2] * 0x3F / 0xFF) & 0x3F) << 5);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 3] * 0x1F / 0xFF) & 0x1F));

                Palette[i] = entry;
            }

            return true;
        }
    }

    public class PvrPaletteEncoder_Argb4444 : VrPaletteEncoder
    {
        public override int GetBpp()
        {
            return 16;
        }
        public override bool EncodePalette(ref byte[] Buf, int Pointer, int Colors, ref uint[] Palette)
        {
            for (int i = 0; i < Colors; i++)
            {
                ushort entry = 0x0; // Entry

                entry |= (ushort)(((Buf[Pointer + (i * 4) + 0] * 0xF / 0xFF) & 0xF) << 12);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 1] * 0xF / 0xFF) & 0xF) << 8);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 2] * 0xF / 0xFF) & 0xF) << 4);
                entry |= (ushort)(((Buf[Pointer + (i * 4) + 3] * 0xF / 0xFF) & 0xF));

                Palette[i] = entry;
            }

            return true;
        }
    }
}