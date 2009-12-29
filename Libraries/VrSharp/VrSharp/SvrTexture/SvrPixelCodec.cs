using System;

namespace VrSharp.SvrTexture
{
    public abstract class SvrPixelCodec : VrPixelCodec
    {
        #region Rgb5a3
        // Rgb5a3
        public class Rgb5a3 : SvrPixelCodec
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

                    if ((pixel & 0x8000) != 0) // Rgb555
                    {
                        clut[i, 3] = 0xFF;
                        clut[i, 2] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);
                        clut[i, 1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                        clut[i, 0] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                    }
                    else // Argb3444
                    {
                        clut[i, 3] = (byte)(((pixel >> 12) & 0x07) * 0xFF / 0x07);
                        clut[i, 2] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);
                        clut[i, 1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                        clut[i, 0] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                    }

                    offset += 2;
                }

                return clut;
            }

            public override byte[] GetPixelPalette(byte[] input, int offset)
            {
                ushort pixel   = BitConverter.ToUInt16(input, offset);
                byte[] palette = new byte[4];

                if ((pixel & 0x8000) != 0) // Rgb555
                {
                    palette[3] = 0xFF;
                    palette[2] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);
                    palette[1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                    palette[0] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                }
                else // Argb3444
                {
                    palette[3] = (byte)(((pixel >> 12) & 0x07) * 0xFF / 0x07);
                    palette[2] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);
                    palette[1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                    palette[0] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                }

                return palette;
            }
        }
        #endregion

        #region Argb8888
        // Argb8888
        public class Argb8888 : SvrPixelCodec
        {
            public override bool CanDecode() { return true; }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 32; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    uint pixel = BitConverter.ToUInt32(input, offset);

                    if ((pixel & 0x80000000) != 0) // Rgb888
                    {
                        clut[i, 3] = 0xFF;
                        clut[i, 2] = (byte)((pixel >> 0)  & 0xFF);
                        clut[i, 1] = (byte)((pixel >> 8)  & 0xFF);
                        clut[i, 0] = (byte)((pixel >> 16) & 0xFF);
                    }
                    else // Argb7888
                    {
                        clut[i, 3] = (byte)(((pixel >> 24) & 0x7F) << 1);
                        clut[i, 2] = (byte)((pixel  >> 0)  & 0xFF);
                        clut[i, 1] = (byte)((pixel  >> 8)  & 0xFF);
                        clut[i, 0] = (byte)((pixel  >> 16) & 0xFF);
                    }

                    offset += 4;
                }

                return clut;
            }

            public override byte[] GetPixelPalette(byte[] input, int offset)
            {
                uint pixel     = BitConverter.ToUInt32(input, offset);
                byte[] palette = new byte[4];

                if ((pixel & 0x80000000) != 0) // Rgb888
                {
                    palette[3] = 0xFF;
                    palette[2] = (byte)((pixel >> 0)  & 0xFF);
                    palette[1] = (byte)((pixel >> 8)  & 0xFF);
                    palette[0] = (byte)((pixel >> 16) & 0xFF);
                }
                else // Argb7888
                {
                    palette[3] = (byte)(((pixel >> 24) & 0x7F) << 1);
                    palette[2] = (byte)((pixel  >> 0)  & 0xFF);
                    palette[1] = (byte)((pixel  >> 8)  & 0xFF);
                    palette[0] = (byte)((pixel  >> 16) & 0xFF);
                }

                return palette;
            }
        }
        #endregion
    }
}