using System;

namespace VrSharp.GvrTexture
{
    public abstract class GvrPixelCodec : VrPixelCodec
    {
        #region Intensity 8-bit with Alpha
        // Intensity 8-bit with Alpha
        public class IntensityA8 : GvrPixelCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp()     { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    clut[i, 3] = input[offset];
                    clut[i, 2] = input[offset + 1];
                    clut[i, 1] = input[offset + 1];
                    clut[i, 0] = input[offset + 1];

                    offset += 2;
                }

                return clut;
            }
        }
        #endregion

        #region Rgb565
        // Rgb565
        public class Rgb565 : GvrPixelCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    ushort pixel = SwapUShort(BitConverter.ToUInt16(input, offset));

                    clut[i, 3] = 0xFF;
                    clut[i, 2] = (byte)(((pixel >> 11) & 0x1F) * 0xFF / 0x1F);
                    clut[i, 1] = (byte)(((pixel >> 5)  & 0x3F) * 0xFF / 0x3F);
                    clut[i, 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                    offset += 2;
                }

                return clut;
            }
        }
        #endregion

        #region Rgb5a3
        // Rgb5a3
        public class Rgb5a3 : GvrPixelCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp() { return 16; }

            public override byte[,] GetClut(byte[] input, int offset, int entries)
            {
                byte[,] clut = new byte[entries, 4];

                for (int i = 0; i < entries; i++)
                {
                    ushort pixel = SwapUShort(BitConverter.ToUInt16(input, offset));

                    if ((pixel & 0x8000) != 0) // Rgb555
                    {
                        clut[i, 3] = 0xFF;
                        clut[i, 2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                        clut[i, 1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                        clut[i, 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);
                    }
                    else // Argb3444
                    {
                        clut[i, 3] = (byte)(((pixel >> 12) & 0x07) * 0xFF / 0x07);
                        clut[i, 2] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                        clut[i, 1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                        clut[i, 0] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);
                    }

                    offset += 2;
                }

                return clut;
            }
        }
        #endregion
    }
}