using System;

namespace pp_tools
{
    public class CNX
    {
        private int 
            compressedSize   = 0, // Compressed Size
            decompressedSize = 0; // Decompressed Sizes.

        private int
            Cpointer = 0x10, // Compressed Pointer
            Dpointer = 0;    // Decompressed Pointer

        private int
            Coffset = 0, // Offset for compressed data.
            Cpos    = 0, // Actual offset for compressed data.
            Ccount  = 0, // Count for compressed data.
            Cflag   = 0; // Control Flag.

        private byte[]
            decompressedData, // Decompressed Data
            compressedData;   // Compressed Data


        public CNX()
        {
        }

        /* Decompress */
        public byte[] decompress(byte[] compressedData)
        {
            try
            {
                /* Get the sizes of the compressed & decompressed files */
                compressedSize   = BitConverter.ToInt32(compressedData, 0x4);
                decompressedSize = BitConverter.ToInt32(compressedData, 0x8);

                decompressedData = new byte[decompressedSize];

                /* Ok, let's attempt to decompress the data */
                while (Cpointer < compressedSize)
                {
                    Cflag = Cpointer;
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        /* Check for out of bounds */
                        if (Dpointer >= decompressedSize)
                            break;

                        /* Is the data uncompressed (Value of 1) */
                        if ((compressedData[Cflag] & (1 << i)) > 0)
                        {
                            decompressedData[Dpointer] = compressedData[Cpointer];
                            Cpointer++;
                            Dpointer++;
                        }

                        /* The data is compressed! */
                        else
                        {
                            Coffset = (compressedData[Cpointer] + ((compressedData[Cpointer + 1] >> 4) & 0xF) * 256) + 18;
                            Ccount  = (compressedData[Cpointer + 1] & 0xF) + 3;

                            Cpointer += 2;

                            for (int j = 0; j < Ccount; j++)
                            {
                                if (Dpointer >= decompressedSize) break;

                                Cpos = Coffset + (Dpointer / 0x1000) * 0x1000;

                                while (Cpos >= Dpointer)
                                    Cpos -= 0x1000;
                                if (Cpos < 0)
                                    decompressedData[Dpointer] = 0x0;
                                else
                                    decompressedData[Dpointer] = decompressedData[Cpos];

                                Dpointer++;
                                Coffset++;
                            }
                        }
                    }
                }

                return decompressedData;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        /* Compress */
        public byte[] compress(byte[] decompressedData)
        {
            try
            {
                /* Set the sizes of the compressed & decompressed files */
                decompressedSize = decompressedData.Length;
                compressedSize   = Cpointer + decompressedSize + (int) Math.Ceiling((double) decompressedSize / 8);

                compressedData = new byte[compressedSize];

                /* Can we compress this? (Filesize is too big?) */
                if (decompressedSize >= Int32.MaxValue || compressedSize >= Int32.MaxValue)
                    return new byte[0];

                /* Set CNX header. */
                Array.Copy(Header.LZ01, 0, compressedData, 0, Header.LZ01.Length);

                /* Set filesizes */
                Array.Copy(BitConverter.GetBytes(compressedSize),   0, compressedData, 0x4, 4); // Compressed Size
                Array.Copy(BitConverter.GetBytes(decompressedSize), 0, compressedData, 0x8, 4); // Decompressed Size

                /* Write compressed data, the easy way! */
                while (Cpointer < compressedSize)
                {
                    compressedData[Cpointer] = 0xFF;
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        compressedData[Cpointer] = decompressedData[Dpointer];
                        Cpointer++;
                        Dpointer++;
                    }
                }

                return compressedData;
            }
            catch (Exception)
            {
                return new byte[0];
            }
                
        }
    }
}
