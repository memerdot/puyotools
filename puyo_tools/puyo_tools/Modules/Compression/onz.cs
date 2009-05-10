using System;
using System.Collections.Generic;
using System.IO;

namespace puyo_tools
{
    public class ONZ : CompressionClass
    {
        public ONZ()
        {
        }

        /* Decompress */
        public override Stream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length; // Compressed Size
                uint decompressedSize = StreamConverter.ToUInt(data, 0x0) >> 8; // Decompressed Size

                uint Cpointer = 0x4; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                /* Some files (Let's Tap LZ7 files) may have their decompressed size stored in a different place */
                if (decompressedSize == 0)
                {
                    decompressedSize = StreamConverter.ToUInt(data, 0x4);
                    Cpointer = 0x8;
                }

                byte[] compressedData   = StreamConverter.ToByteArray(data, 0x0, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    //for (int i = 0; i < 8; i++)
                    for (int i = 7; i >= 0; i--)
                    {
                        /* Is the data compressed */
                        if ((Cflag & (1 << i)) > 0)
                        {
                            /* Yes it is! */
                            byte first  = compressedData[Cpointer];
                            byte second = compressedData[Cpointer + 1];

                            /* How many bytes does the offset & length take up? */
                            uint pos;
                            uint amountToCopy;
                            if (first < 0x20)
                            {
                                byte third = compressedData[Cpointer + 2];

                                if (first >= 0x10)
                                {
                                    /* 4 bytes */
                                    byte fourth = compressedData[Cpointer + 3];

                                    pos          = (uint)(((third & 0xF) << 8) | fourth) + 1;
                                    amountToCopy = (uint)((second << 4) | ((first & 0xF) << 12) | (third >> 4)) + 273;

                                    Cpointer += 4;
                                }
                                else
                                {
                                    /* 3 bytes */
                                    pos          = (uint)(((second & 0xF) << 8) | third) + 1;
                                    amountToCopy = (uint)(((first & 0xF) << 4) | (second >> 4)) + 17;

                                    Cpointer += 3;
                                }
                            }
                            else
                            {
                                /* 2 bytes */
                                pos          = (uint)(((first & 0xF) << 8) | second) + 1;
                                amountToCopy = (uint)(first >> 4) + 1;

                                Cpointer += 2;
                            }

                            /* Ok, copy the data now */
                            for (int j = 0; j < amountToCopy; j++)
                                decompressedData[Dpointer + j] = decompressedData[Dpointer - pos + j];

                            Dpointer += amountToCopy;
                        }
                        else
                        {
                            /* The data is not compressed, so just copy the byte */
                            decompressedData[Dpointer] = compressedData[Cpointer];

                            Cpointer++;
                            Dpointer++;
                        }

                        /* Did we reach the end? */
                        if (Cpointer >= compressedSize || Dpointer >= decompressedSize)
                            break;
                    }
                }

                /* Alright, return the stream now */
                return new MemoryStream(decompressedData);
            }
            catch
            {
                /* An error occured */
                return null;
            }
        }

        /* Compress */
        public override Stream Compress(ref Stream data, string filename)
        {
            try
            {
                /* Set variables */
                uint decompressedSize = (uint)data.Length; // Decompressed Size

                uint Dpointer = 0x0; // Decompressed Pointer

                List<byte> compressedData = new List<byte>(); // Compressed Data
                byte[] decompressedData   = StreamConverter.ToByteArray(data, 0x0, (int)decompressedSize); // Decompressed Data

                /* Add the header byte */
                compressedData.Add(0x10);

                /* Add the decompressed size */
                for (int i = 0; i < 3; i++)
                    compressedData.Add(BitConverter.GetBytes(decompressedSize)[i]);

                /* Ok, now let's start creating the compressed data */
                while (Dpointer < decompressedSize)
                {
                    byte Cflag = 0;
                    List<byte> tempList = new List<byte>();

                    for (int i = 0; i < 8; i++)
                    {
                        /* Let's do a search to see what we can compress */
                        int[] searchResult = LZsearch(ref decompressedData, Dpointer, decompressedSize);

                        /* Did we get any results? */
                        if (searchResult[0] > 2)
                        {
                            /* Add stuff to our lists */
                            byte add = (byte)((((searchResult[0] - 3) & 0xF) << 4) + (((searchResult[1] - 1) >> 8) & 0xF));
                            tempList.Add(add);

                            add = (byte)((searchResult[1] - 1) & 0xFF);
                            tempList.Add(add);

                            Dpointer += (uint)searchResult[0];
                            Cflag |= (byte)(1 << (7 - i));
                        }
                        else if (searchResult[0] >= 0)
                        {
                            tempList.Add(decompressedData[Dpointer]);
                            Dpointer++;
                        }
                        else
                            break;

                        /* Check to see if we are out of range */
                        if (Dpointer >= decompressedSize)
                            break;
                    }

                    /* Ok, add our results to the compressed data */
                    compressedData.Add(Cflag);
                    compressedData.AddRange(tempList);
                }

                return new MemoryStream(compressedData.ToArray());
            }
            catch
            {
                /* Something went wrong */
                return null;
            }
        }

        /* Get Filename */
        public override string GetFilename(ref Stream data, string filename)
        {
            /* Only return a different extension if the current one is onz */
            if (Path.GetExtension(filename).Substring(1).ToLower() == "onz")
                return Path.GetFileNameWithoutExtension(filename) + ".one";

            return filename;
        }
    }
}