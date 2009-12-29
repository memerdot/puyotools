using System;

namespace VrSharp.PvrTexture
{
    public abstract class PvrPixelCodec : VrPixelCodec
    {
        #region Argb1555
        // Argb1555
        public class Argb1555 : PvrPixelCodec
        {
            public override bool CanDecode() { return true; }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    ushort pixel = BitConverter.ToUInt16(input, offset);

                    clut[i, 3] = (byte)(((pixel >> 15) & 0x01) * 0xFF);
                    clut[i, 2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                    clut[i, 1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                    clut[i, 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                    offset += 2;
                }

                return clut;
            }

            public override byte[] GetPixelPalette(byte[] input, int offset)
            {
                ushort pixel   = BitConverter.ToUInt16(input, offset);
                byte[] palette = new byte[4];

                palette[3] = (byte)(((pixel >> 15) & 0x01) * 0xFF);
                palette[2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                palette[1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                palette[0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                return palette;
            }
        }
        #endregion

        #region Rgb565
        // Rgb565
        public class Rgb565 : PvrPixelCodec
        {
            public override bool CanDecode() { return true; }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    ushort pixel = BitConverter.ToUInt16(input, offset);

                    clut[i, 3] = 0xFF;
                    clut[i, 2] = (byte)(((pixel >> 11) & 0x1F) * 0xFF / 0x1F);
                    clut[i, 1] = (byte)(((pixel >> 5)  & 0x3F) * 0xFF / 0x3F);
                    clut[i, 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                    offset += 2;
                }

                return clut;
            }

            public override byte[] GetPixelPalette(byte[] input, int offset)
            {
                ushort pixel   = BitConverter.ToUInt16(input, offset);
                byte[] palette = new byte[4];

                palette[3] = 0xFF;
                palette[2] = (byte)(((pixel >> 11) & 0x1F) * 0xFF / 0x1F);
                palette[1] = (byte)(((pixel >> 5)  & 0x3F) * 0xFF / 0x3F);
                palette[0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                return palette;
            }
        }
        #endregion

        #region Argb4444
        // Argb4444
        public class Argb4444 : PvrPixelCodec
        {
            public override bool CanDecode() { return true; }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    ushort pixel = BitConverter.ToUInt16(input, offset);

                    clut[i, 3] = (byte)(((pixel >> 12) & 0x0F) * 0xFF / 0x0F);
                    clut[i, 2] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                    clut[i, 1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                    clut[i, 0] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);

                    offset += 2;
                }

                return clut;
            }

            public override byte[] GetPixelPalette(byte[] input, int offset)
            {
                ushort pixel   = BitConverter.ToUInt16(input, offset);
                byte[] palette = new byte[4];

                palette[3] = (byte)(((pixel >> 12) & 0x0F) * 0xFF / 0x0F);
                palette[2] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                palette[1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                palette[0] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);

                return palette;
            }
        }
        #endregion
    }
}