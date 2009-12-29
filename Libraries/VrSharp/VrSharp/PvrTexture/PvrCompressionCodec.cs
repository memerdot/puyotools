using System;
using System.IO;

namespace VrSharp.PvrTexture
{
    public abstract class PvrCompressionCodec
    {
        #region Rle Compression
        // Rle Compression
        public class Rle : PvrCompressionCodec
        {
            public override byte[] Decompress(byte[] input, int DataOffset, VrPixelCodec PixelCodec, VrDataCodec DataCodec)
            {
                byte[] output     = new byte[BitConverter.ToUInt32(input, 0x00)];
                int SourcePointer = DataOffset;
                int DestPointer   = 0x00;
                int PixelSize     = (DataCodec.GetBpp(PixelCodec) / 8);

                // Copy the header
                if (DataOffset - 4 > 0)
                {
                    Array.Copy(input, 0x04, output, 0x00, DataOffset - 4);
                    DestPointer += (DataOffset - 4);
                }

                // Decompress
                while (SourcePointer < input.Length && DestPointer < output.Length)
                {
                    int amount = input[SourcePointer + PixelSize] + 1;
                    for (int i = 0; i < amount; i++)
                    {
                        Array.Copy(input, SourcePointer, output, DestPointer, PixelSize);
                        DestPointer += PixelSize;
                    }

                    SourcePointer += PixelSize + 1;
                }

                return output;
            }

            public override byte[] Compress(byte[] input, int DataOffset, VrPixelCodec PixelCodec, VrDataCodec DataCodec)
            {
                byte[] output = new byte[0];

                return output;
            }
        }
        #endregion

        public abstract byte[] Decompress(byte[] input, int DataOffset, VrPixelCodec PixelCodec, VrDataCodec DataCodec);
        public abstract byte[] Compress(byte[] input, int DataOffset, VrPixelCodec PixelCodec, VrDataCodec DataCodec);
    }
}