﻿using System;

namespace VrSharp.GvrTexture
{
    public abstract class GvrDataCodec : VrDataCodec
    {
        #region Intensity 4-bit
        // Intensity 4-bit
        public class Intensity4 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 4; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 8)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 8; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                byte entry = (byte)((input[offset] >> ((x & 0x01) * 4)) & 0x0F);

                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = (byte)(entry * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = (byte)(entry * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = (byte)(entry * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = (byte)(entry * 0xFF / 0x0F);

                                if ((x & 0x01) != 0)
                                    offset++;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Intensity 8-bit
        // Intensity 8-bit
        public class Intensity8 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 8; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = input[offset];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = input[offset];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = input[offset];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = input[offset];

                                offset++;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Intensity 4-bit with Alpha
        // Intensity 4-bit with Alpha
        public class IntensityA4 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 8; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = (byte)(((input[offset] >> 4) & 0x0F) * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = (byte)((input[offset] & 0x0F) * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = (byte)((input[offset] & 0x0F) * 0xFF / 0x0F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = (byte)((input[offset] & 0x0F) * 0xFF / 0x0F);

                                offset++;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Intensity 8-bit with Alpha
        // Intensity 8-bit with Alpha
        public class IntensityA8 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 16; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = input[offset];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = input[offset + 1];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = input[offset + 1];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = input[offset + 1];

                                offset += 2;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Rgb565
        // Rgb565
        public class Rgb565 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 16; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                ushort pixel = SwapUShort(BitConverter.ToUInt16(input, offset));

                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = 0xFF;
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = (byte)(((pixel >> 11) & 0x1F) * 0xFF / 0x1F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = (byte)(((pixel >> 5)  & 0x3F) * 0xFF / 0x3F);
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);

                                offset += 2;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Rgb5a3
        // Rgb5a3
        public class Rgb5a3 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 16; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                ushort pixel = SwapUShort(BitConverter.ToUInt16(input, offset));

                                if ((pixel & 0x8000) != 0) // Rgb555
                                {
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 3] = 0xFF;
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 1] = (byte)(((pixel >> 5)  & 0x1F) * 0xFF / 0x1F);
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 0] = (byte)(((pixel >> 0)  & 0x1F) * 0xFF / 0x1F);
                                }
                                else // Argb3444
                                {
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 3] = (byte)(((pixel >> 12) & 0x07) * 0xFF / 0x07);
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 2] = (byte)(((pixel >> 8)  & 0x0F) * 0xFF / 0x0F);
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 1] = (byte)(((pixel >> 4)  & 0x0F) * 0xFF / 0x0F);
                                    output[((((y + y2) * width) + (x + x2)) * 4) + 0] = (byte)(((pixel >> 0)  & 0x0F) * 0xFF / 0x0F);
                                }

                                offset += 2;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Argb8888
        // Argb8888
        public class Argb8888 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 32; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = input[offset + 0];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = input[offset + 1];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = input[offset + 32];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = input[offset + 33];

                                offset += 2;
                            }
                        }

                        offset += 32;
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                int offset = 0;
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                output[offset + 0]  = input[((((y + y2) * width) + (x + x2)) * 4) + 3];
                                output[offset + 1]  = input[((((y + y2) * width) + (x + x2)) * 4) + 2];
                                output[offset + 32] = input[((((y + y2) * width) + (x + x2)) * 4) + 1];
                                output[offset + 33] = input[((((y + y2) * width) + (x + x2)) * 4) + 0];

                                offset += 2;
                            }
                        }

                        offset += 32;
                    }
                }

                return output;
            }
        }
        #endregion

        #region 4-bit Texture with Clut
        // 4-bit Texture with Clut
        public class Index4 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 4; }
            public override int GetNumClutEntries() { return 16; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];
                byte[,] clut  = ClutData;

                for (int y = 0; y < height; y += 8)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 8; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                byte entry = (byte)((input[offset] >> ((x & 0x01) * 4)) & 0x0F);

                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = clut[entry, 3];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = clut[entry, 2];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = clut[entry, 1];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = clut[entry, 0];

                                if ((x & 0x01) != 0)
                                    offset++;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region 8-bit Texture with Clut
        // 8-bit Texture with Clut
        public class Index8 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 8; }
            public override int GetNumClutEntries() { return 256; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];
                byte[,] clut  = ClutData;

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                output[((((y + y2) * width) + (x + x2)) * 4) + 3] = clut[input[offset], 3];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 2] = clut[input[offset], 2];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 1] = clut[input[offset], 1];
                                output[((((y + y2) * width) + (x + x2)) * 4) + 0] = clut[input[offset], 0];

                                offset++;
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion

        #region Dxt1 Texture Compression
        // Dxt1 Texture Compression
        public class Dxt1 : GvrDataCodec
        {
            public override bool CanDecode() { return true;  }
            public override bool CanEncode() { return false; }
            public override int GetBpp(VrPixelCodec PixelCodec) { return 0; }

            public override byte[] Decode(byte[] input, int offset, int width, int height, VrPixelCodec PixelCodec)
            {
                byte[] output = new byte[width * height * 4];

                for (int y = 0; y < height; y += 8)
                {
                    for (int x = 0; x < width; x += 8)
                    {
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                // Determine the colors for the 4x4 clut
                                byte[,] Clut   = new byte[4, 4];
                                ushort[] pixel = new ushort[2];

                                // Get the first two colors
                                pixel[0] = SwapUShort(BitConverter.ToUInt16(input, offset));
                                pixel[1] = SwapUShort(BitConverter.ToUInt16(input, offset + 2));

                                Clut[0, 3] = 0xFF;
                                Clut[0, 2] = (byte)(((pixel[0] >> 11) & 0x1F) * 0xFF / 0x1F);
                                Clut[0, 1] = (byte)(((pixel[0] >> 5)  & 0x3F) * 0xFF / 0x3F);
                                Clut[0, 0] = (byte)(((pixel[0] >> 0)  & 0x1F) * 0xFF / 0x1F);

                                Clut[1, 3] = 0xFF;
                                Clut[1, 2] = (byte)(((pixel[1] >> 11) & 0x1F) * 0xFF / 0x1F);
                                Clut[1, 1] = (byte)(((pixel[1] >> 5)  & 0x3F) * 0xFF / 0x3F);
                                Clut[1, 0] = (byte)(((pixel[1] >> 0)  & 0x1F) * 0xFF / 0x1F);

                                // Determine the next two colors based on how the first two are stored
                                if (pixel[0] > pixel[1])
                                {
                                    Clut[2, 3] = 0xFF;
                                    Clut[2, 2] = (byte)(((Clut[0, 2] * 2) + Clut[1, 2]) / 3);
                                    Clut[2, 1] = (byte)(((Clut[0, 1] * 2) + Clut[1, 1]) / 3);
                                    Clut[2, 0] = (byte)(((Clut[0, 0] * 2) + Clut[1, 0]) / 3);

                                    Clut[3, 3] = 0xFF;
                                    Clut[3, 2] = (byte)(((Clut[1, 2] * 2) + Clut[0, 2]) / 3);
                                    Clut[3, 1] = (byte)(((Clut[1, 1] * 2) + Clut[0, 1]) / 3);
                                    Clut[3, 0] = (byte)(((Clut[1, 0] * 2) + Clut[0, 0]) / 3);
                                }
                                else
                                {
                                    Clut[2, 3] = 0xFF;
                                    Clut[2, 2] = (byte)((Clut[0, 2] + Clut[1, 2]) / 2);
                                    Clut[2, 1] = (byte)((Clut[0, 1] + Clut[1, 1]) / 2);
                                    Clut[2, 0] = (byte)((Clut[0, 0] + Clut[1, 0]) / 2);

                                    Clut[3, 3] = 0x00;
                                    Clut[3, 2] = 0x00;
                                    Clut[3, 1] = 0x00;
                                    Clut[3, 0] = 0x00;
                                }

                                offset += 4;

                                for (int y3 = 0; y3 < 4; y3++)
                                {
                                    for (int x3 = 0; x3 < 4; x3++)
                                    {
                                        output[((((y + y2 + y3) * width) + (x + x2 + x3)) * 4) + 3] = Clut[((input[offset] >> (6 - (x3 * 2))) & 0x03), 3];
                                        output[((((y + y2 + y3) * width) + (x + x2 + x3)) * 4) + 2] = Clut[((input[offset] >> (6 - (x3 * 2))) & 0x03), 2];
                                        output[((((y + y2 + y3) * width) + (x + x2 + x3)) * 4) + 1] = Clut[((input[offset] >> (6 - (x3 * 2))) & 0x03), 1];
                                        output[((((y + y2 + y3) * width) + (x + x2 + x3)) * 4) + 0] = Clut[((input[offset] >> (6 - (x3 * 2))) & 0x03), 0];
                                    }

                                    offset++;
                                }
                            }
                        }
                    }
                }

                return output;
            }

            public override byte[] Encode(byte[] input, int width, int height, VrPixelCodec PixelCodec)
            {
                return null;
            }
        }
        #endregion
    }
}