using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class CXLZ : CompressionClass
    {
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
                uint decompressedSize = Endian.Swap(StreamConverter.ToUInt(data, 0x5)) >> 8; // Decompressed Size

                uint Cpointer = 0x8; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData   = StreamConverter.ToByteArray(data, 0x0, compressedSize); // Compressed Data
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
                            //int pos = (((first & 0xF) << 8) | second) + 1;
                            //int amountToCopy = (first >> 4) + 3;
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
                byte[] decompressedData   = ObjectConverter.StreamToBytes(data, 0x0, (int)decompressedSize); // Decompressed Data

                /* Add the header */
                compressedData.AddRange(ObjectConverter.StringToByteList(FileHeader.CXLZ, 4));
                compressedData.Add(0x10);

                /* Add the decompressed size */
                for (int i = 0; i < 3; i++)
                    compressedData.Add(BitConverter.GetBytes(Endian.Swap(decompressedSize) >> 8)[i]);

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
            return filename;
        }
    }
}
