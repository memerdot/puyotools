using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class LZ01 : CompressionClass
    {
        public LZ01()
        {
        }

        /* Decompress */
        public override Stream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = StreamConverter.ToUInt(data, 0x4); // Compressed Size
                uint decompressedSize = StreamConverter.ToUInt(data, 0x8); // Decompressed Size

                uint Cpointer = 0x10; // Compressed Pointer
                uint Dpointer = 0x0;  // Decompressed Pointer

                byte[] compressedData   = StreamConverter.ToByteArray(data, 0x0, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        /* Is this data compressed? */
                        if ((Cflag & (1 << i)) == 0)
                        {
                            /* Yes it is! */
                            byte first  = compressedData[Cpointer];
                            byte second = compressedData[Cpointer + 1];
                            int pos     = (first + ((second >> 4) & 0xF) * 256) + 18;
                            int amountToCopy = (second & 0xF) + 3;
                            Cpointer += 2;

                            /* Ok, copy the data now */
                            for (int j = 0; j < amountToCopy; j++)
                            {
                                /* Don't continue if our decompressed pointer is out of bounds */
                                if (Dpointer + j >= decompressedSize)
                                    break;

                                int Cpos = (int)(pos + ((Dpointer + j) / 0x1000) * 0x1000);

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

                /* Add the header */
                compressedData.AddRange(StringConverter.ToByteList(CompressionHeader.LZ01, 4));

                /* Add the compressed and decompressed size. */
                compressedData.AddRange(NumberConverter.ToByteList(0)); // Set to 0 for now.
                compressedData.AddRange(NumberConverter.ToByteList(decompressedSize));
                compressedData.AddRange(NumberConverter.ToByteList(0));

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
                            //byte add = (byte)((((searchResult[0] - 3) & 0xF) << 4) + (((searchResult[1] - 1) >> 8) & 0xF));
                            byte add = (byte)((searchResult[1] - 18) & 0xFF);
                            tempList.Add(add);

                            //add = (byte)((searchResult[1] - 1) & 0xFF);
                            //add = (byte)((((searchResult[0] - 3) & 0xF)) + (((searchResult[1] - 1) >> 8) & 0xF));
                            add = (byte)(((((searchResult[1] - 1) >> 8) & 0xF)) + ((searchResult[0] - 3) & 0xF));
                            tempList.Add(add);

                            Dpointer += (uint)searchResult[0];
                            //Cflag |= (byte)(1 << (7 - i));
                        }
                        else if (searchResult[0] >= 0)
                        {
                            tempList.Add(decompressedData[Dpointer]);
                            Cflag |= (byte)(1 << i);
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

                /* Let's go back and add the compressed filesize */
                uint compressedSize = (uint)compressedData.Count;
                compressedData.RemoveRange(0x4, 4);
                compressedData.InsertRange(0x4, NumberConverter.ToByteList(compressedSize));

                return new MemoryStream(compressedData.ToArray());
            }
            catch
            {
                /* Something went wrong */
                return null;
            }
        }
    }
}
