using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class CNX : CompressionClass
    {
        /* CNX cracked by drx (Luke Zapart)
         * <thedrx@gmail.com> */

        public CNX()
        {
        }

        /* Decompress */
        public override Stream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = Endian.Swap(StreamConverter.ToUInt(data, 0x8)) + 16; // Compressed Size
                uint decompressedSize = Endian.Swap(StreamConverter.ToUInt(data, 0xC));      // Decompressed Size

                uint Cpointer = 0x10; // Compressed Pointer
                uint Dpointer = 0x0;  // Decompressed Pointer

                byte[] compressedData   = StreamConverter.ToByteArray(data, 0x0, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 4; i++)
                    {
                        /* Check for the mode */
                        switch ((Cflag >> (i * 2)) & 3)
                        {
                            /* Padding Mode
					         * All CNX archives seem to be packed in 0x800 chunks. when nearing
					         * a 0x800 cutoff, there usually is a padding command at the end to skip
					         * a few bytes (to the next 0x800 chunk, i.e. 0x4800, 0x7000, etc.) */
                            case 0:
                                byte temp_byte = compressedData[Cpointer];
                                Cpointer      += (uint)(temp_byte & 0xFF) + 1;

                                i = 3;
                                break;

                            /* Single Byte Copy Mode */
                            case 1:
                                decompressedData[Dpointer] = compressedData[Cpointer];
                                Cpointer++;
                                Dpointer++;
                                break;

                            /* Copy from destination buffer to current position */
                            case 2:
                                uint temp_word = Endian.Swap(compressedData[Cpointer]);
                                //uint temp_word = Endian.Swap(ObjectConverter.StreamToUShort(data, Cpointer));

                                uint off = (temp_word >> 5) + 1;
                                uint len = (temp_word & 0x1F) + 4;

                                Cpointer += 2;

                                for (int j = 0; j < len; j++)
                                {
                                    decompressedData[Dpointer] = decompressedData[Dpointer - off];
                                    Dpointer++;
                                }

                                break;

                            /* Direct Block Copy (first byte signifies length of copy) */
                            case 3:
                                byte blockLength = compressedData[Cpointer];
                                Cpointer++;

                                for (int j = 0; j < blockLength; j++)
                                {
                                    decompressedData[Dpointer] = compressedData[Cpointer];
                                    Cpointer++;
                                    Dpointer++;
                                }

                                break;
                        }
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

                uint Cpointer = 0x10; // Compressed Pointer
                uint Dpointer = 0x0;  // Decompressed Pointer

                List<byte> compressedData = new List<byte>(); // Compressed Data
                byte[] decompressedData   = StreamConverter.ToByteArray(data, 0x0, (int)decompressedSize); // Decompressed Data

                /* Add the header */
                compressedData.AddRange(StringConverter.ToByteList(CompressionHeader.CNX, 4));
                compressedData.AddRange(StringConverter.ToByteList(Path.GetExtension(filename).PadRight(4, '\x00').Substring(1), 3));
                compressedData.Add(0x10);
                compressedData.AddRange(NumberConverter.ToByteList(0)); // Set to 0 for now (Compressed file size).
                compressedData.AddRange(NumberConverter.ToByteList(Endian.Swap(decompressedSize)));

                /* Ok, now let's start creating the compressed data */
                while (Dpointer < decompressedSize)
                {
                    byte Cflag = 0;
                    List<byte> tempList = new List<byte>();

                    Cpointer++;
                    for (int i = 0; i < 4; i++)
                    {
                        /* Are we at the end of a block (less than 4 bytes?) */
                        if (Cpointer % 0x800 >= 0x7FD)
                        {
                            tempList.Add((byte)(0x800 - 1 - (Cpointer % 0x800)));
                            Cpointer++;

                            while (Cpointer % 0x800 != 0)
                            {
                                tempList.Add(0);
                                Cpointer++;
                            }
                            break;
                        }

                        /* Let's do a search to see what we can compress */
                        int[] searchResult = search(ref decompressedData, Dpointer, decompressedSize);

                        /* Did we get any results? */
                        //if (searchResult[0] > 3)
                        //{
                            /* Add stuff to our lists */
                        //    ushort add = (ushort)(((searchResult[1] - 1) << 5) + ((searchResult[0] - 4) & 0x1F));
                        //    tempList.AddRange(ObjectConverter.UShortToByteList(Endian.Swap(add)));

                        //    Cpointer += 2;
                        //    Dpointer += (uint)searchResult[0];
                        //    Cflag    |= (byte)(2 << (i * 2));
                        //}
                        if (searchResult[0] >= 0)
                        {
                            /* Let's see if we should do a direct block copy or not */
                            int blockLength = searchResult[0];
                            while ((Cpointer % 0x800) + blockLength >= 0x7FF)
                                blockLength--;

                            while (blockLength < 256 && (Cpointer % 0x800) + blockLength < 0x7FE && Dpointer + blockLength < decompressedSize)
                            {
                                int[] result = search(ref decompressedData, Dpointer + (uint)blockLength, decompressedSize);
                                if (result[0] >= 0 && result[0] < 4)
                                    blockLength++;
                                else
                                    break;
                            }

                            /* Do a direct block copy? */
                            if (blockLength > 2)
                            {
                                tempList.Add((byte)blockLength);

                                for (int j = 0; j < blockLength; j++)
                                    tempList.Add(decompressedData[Dpointer + j]);

                                Cpointer += (uint)blockLength + 1;
                                Dpointer += (uint)blockLength;
                                Cflag    |= (byte)(3 << (i * 2));
                            }

                            /* Just write the byte */
                            else
                            {
                                tempList.Add(decompressedData[Dpointer]);
                                Cpointer++;
                                Dpointer++;

                                Cflag |= (byte)(1 << (i * 2));
                            }
                        }
                        else
                            break;

                        /* Check to see if we are out of range */
                        if (Dpointer >= decompressedSize)
                        {
                            tempList.Add(0);
                            Cpointer++;
                            break;
                        }
                    }

                    /* Ok, add our results to the compressed data */
                    compressedData.Add(Cflag);
                    compressedData.AddRange(tempList);
                }

                /* Let's go back and add the compressed filesize */
                uint compressedSize = (uint)compressedData.Count;
                compressedData.RemoveRange(0x8, 4);
                compressedData.InsertRange(0x8, NumberConverter.ToByteList(Endian.Swap(compressedSize - 16)));

                return new MemoryStream(compressedData.ToArray());
            }
            catch (Exception f)
            {
                /* Something went wrong */
                System.Windows.Forms.MessageBox.Show(f.ToString());
                return null;
            }
        }

        /* Get Filename */
        public override string GetFilename(ref Stream data, string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + '.' + StreamConverter.ToString(data, 0x4, 3);
        }

        /* Search for data that can be compressed */
        private int[] search(ref byte[] decompressedData, uint pos, uint decompressedSize)
        {
            /* Set variables */
            int slidingWindowSize   = 4096; // Sliding Window Size
            int readAheadBufferSize = 18;   // Read Ahead Buffer Size

            /* Create a list of our results */
            List<int> results = new List<int>();

            if (pos < 4 || decompressedSize - pos < 4)
                return new int[] { 0, 0 };
            if (pos >= decompressedSize)
                return new int[] { -1, 0 };

            /* Ok, search for data now */
            for (int i = 1; i < slidingWindowSize && i < pos; i++)
            {
                if (decompressedData[pos - i - 1] == decompressedData[pos])
                    results.Add(i + 1);
            }

            /* Did we get any results? */
            if (results.Count == 0)
                return new int[] { 0, 0 };

            bool finish = false;
            int amountOfBytes = 0;

            while (amountOfBytes < readAheadBufferSize && !finish)
            {
                amountOfBytes++;
                for (int i = 0; i < results.Count; i++)
                {
                    /* Make sure we aren't out of range */
                    if (pos + amountOfBytes >= decompressedSize)
                    {
                        finish = true;
                        break;
                    }

                    if (decompressedData[pos + amountOfBytes] != decompressedData[pos - results[i] + (amountOfBytes % results[i])])
                    {
                        if (results.Count > 1)
                        {
                            results.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            finish = true;
                            break;
                        }
                    }
                }
            }

            /* Ok, return our results now */
            return new int[] { amountOfBytes, results[0] };
        }
    }
}