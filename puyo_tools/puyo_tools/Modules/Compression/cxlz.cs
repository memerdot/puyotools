using System;
using System.IO;

namespace puyo_tools
{
    public class CXLZ : CompressionClass
    {
        private int 
            compressedSize   = 0, // Compressed Size
            decompressedSize = 0; // Decompressed Sizes.

        private int
            Cpointer = 0x8, // Compressed Pointer
            Dpointer = 0;   // Decompressed Pointer

        private int
            Coffset = 0, // Offset for compressed data.
            Cpos    = 0, // Actual offset for compressed data.
            Ccount  = 0, // Count for compressed data.
            Cflag   = 0; // Control Flag.

        private byte[]
            decompressedData, // Decompressed Data
            compressedData;   // Compressed Data

        public CXLZ()
        {
        }

        /* Decompress */
        public override Stream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length; // Compressed Size
                uint decompressedSize = ObjectConverter.StreamToUShort(data, 0x5) + (uint)(65536 * ObjectConverter.StreamToBytes(data, 0x7, 1)[0]); // Decompressed Size

                uint Cpointer = 0x8; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData = ObjectConverter.StreamToBytes(data, 0x0, (int)compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 7; i >= 0; i--)
                    {
                        /* Is this data compressed? */
                        if ((Cflag & (1 << i)) > 0)
                        {
                            /* Yes it is! */
                            byte first       = compressedData[Cpointer];
                            byte second      = compressedData[Cpointer + 1];
                            int pos          = (second + (first & 0xF) * 256) + 1;
                            int amountToCopy = ((first >> 4) & 0xF) + 3;
                            Cpointer += 2;

                            /* Ok, copy the data now */
                            for (int j = 0; j < amountToCopy; j++)
                            {
                                /* Don't continue if our decompressed pointer is out of bounds */
                                if (Dpointer + j >= decompressedSize)
                                    break;

                                int Cpos = (int)(Dpointer + (j * 2) - pos);

                                /* Make sure the pointer is set at a correct position and copy */
                                if (Cpos >= Dpointer + j)
                                    Cpos -= 0x1000;

                                if (Cpos < 0)
                                    decompressedData[Dpointer + j] = 0x0;
                                else
                                    decompressedData[Dpointer + j] = decompressedData[Cpos];

                                pos++;
                            }
                            Dpointer += (uint)amountToCopy;
                        }
                        else
                        {
                            /* The data is not compressed, just copy it */
                            decompressedData[Dpointer] = compressedData[Cpointer];
                            Cpointer++;
                            Dpointer++;
                        }

                        /* Did we reach the end? */
                        if (Cpointer >= compressedSize || Dpointer >= decompressedSize)
                            break;
                    }
                }

                /* Finished decompression, now return the data */
                return new MemoryStream(decompressedData);
            }
            catch
            {
                /* An error occured, return the original data */
                return data;
            }
        }

        /* Compress */
        public override Stream Compress(ref Stream data)
        {
            return null;
        }

        /* Get Filename */
        public override string GetFilename(ref Stream data, string filename)
        {
            return filename;
        }

        public byte[] decompress2(byte[] compressedData)
        {
            try
            {
                /* Get the sizes of the compressed & decompressed files */
                compressedSize   = compressedData.Length;
                decompressedSize = BitConverter.ToUInt16(compressedData, 0x5) + (65536 * compressedData[0x7]);

                decompressedData = new byte[decompressedSize];

                /* Ok, let's attempt to decompress the data */
                while (Cpointer < compressedSize)
                {
                    Cflag = Cpointer;
                    Cpointer++;

                    for (int i = 7; i >= 0; i--)
                    {
                        /* Check for out of bounds */
                        if (Dpointer >= decompressedSize)
                            break;

                        /* Is the data uncompressed (Value of 0) */
                        if ((compressedData[Cflag] & (1 << i)) == 0)
                        {
                            decompressedData[Dpointer] = compressedData[Cpointer];
                            Cpointer++;
                            Dpointer++;
                        }

                        /* The data is compressed! */
                        else
                        {
                            Coffset = (compressedData[Cpointer + 1] + ((compressedData[Cpointer]) & 0xF) * 256) + 1;
                            Ccount  = ((compressedData[Cpointer] >> 4) & 0xF) + 3;

                            Cpointer += 2;

                            for (int j = 0; j < Ccount; j++)
                            {
                                if (Dpointer >= decompressedSize)
                                    break;

                                Cpos = Dpointer - Coffset + j;
                                
                                
                                if (Cpos >= Dpointer)
                                    Cpos -= 0x1000;
                                if (Cpos < 0)
                                    decompressedData[Dpointer] = 0x0;
                                else
                                    decompressedData[Dpointer] = decompressedData[Cpos];

                                if (Coffset - 1 >= Dpointer)
                                    Cpos = -1;
                                
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
                compressedSize   = Cpointer + decompressedSize + (int)Math.Ceiling((double)decompressedSize / 8);

                compressedData = new byte[compressedSize];

                /* Can we compress this? (Filesize is too big?) */
                if (decompressedSize >= Int16.MaxValue + (65536 * 0xFF) || compressedSize >= Int16.MaxValue + (65536 * 0xFF))
                    return new byte[0];

                /* Set CXLZ header. */
                Array.Copy(Header.CXLZ, 0, compressedData, 0, Header.CXLZ.Length);

                /* Set filesizes */
                Array.Copy(BitConverter.GetBytes(decompressedSize), 0, compressedData, 0x5, 3); // Decompressed Size

                /* Write compressed data, the easy way! */
                while (Cpointer < compressedSize)
                {
                    compressedData[Cpointer] = 0x0;
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
