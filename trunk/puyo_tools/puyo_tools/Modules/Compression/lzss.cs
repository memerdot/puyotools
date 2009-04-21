﻿using System;
using System.Collections.Generic;
using System.IO;

namespace puyo_tools
{
    public class LZSS : CompressionClass
    {
        public LZSS()
        {
        }

        /* Decompress a compressed LZSS/LZ77 file */
        public override Stream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length; // Compressed Size
                uint decompressedSize = (ObjectConverter.StreamToUInt(data, 0x0) >> 8); // Decompressed Size

                uint Cpointer = 0x4; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData = ObjectConverter.StreamToBytes(data, 0x0, (int)data.Length); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        /* Is the data compressed */
                        //if ((Cflag & (1 << i)) > 0)
                        if ((Cflag & 0x80) != 0)
                        {
                            /* Yes it is! */
                            byte first  = compressedData[Cpointer];
                            byte second = compressedData[Cpointer + 1];
                            ushort pos  = (ushort)((((first << 8) + second) & 0xFFF) + 1);
                            byte amountToCopy = (byte)(3 + ((first >> 4) & 0xF));

                            /* Ok, copy the data now */
                            for (int j = 0; j < amountToCopy; j++)
                                decompressedData[Dpointer + j] = decompressedData[Dpointer - pos + (j % pos)];

                            Cpointer += 2;
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

                        Cflag <<= 1;
                    }
                }

                /* Alright, return the stream now */
                return new MemoryStream(decompressedData);
            }
            catch
            {
                /* Something went wrong */
                return new MemoryStream();
            }
        }

        /* Compress */
        public override Stream Compress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint decompressedSize = (uint)data.Length; // Decompressed Size

                uint Dpointer = 0x0; // Decompressed Pointer

                List<byte> compressedData = new List<byte>(); // Compressed Data
                byte[] decompressedData = ObjectConverter.StreamToBytes(data, 0x0, (int)decompressedSize); // Decompressed Data

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
                        int[] searchResult = search(decompressedData, Dpointer, decompressedSize);

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
                return new MemoryStream();
            }
        }

        /* Get Filename */
        public override string GetFilename(ref Stream data, string filename)
        {
            return filename;
        }

        /* Create a LZSS/LZ77 compressed file */
        public Stream compress(Stream data)
        {
            try
            {
                /* Set variables */
                uint decompressedSize = (uint)data.Length; // Decompressed Size

                uint Dpointer = 0x0; // Decompressed Pointer

                List<byte> compressedData = new List<byte>(); // Compressed Data
                byte[] decompressedData   = ObjectConverter.StreamToBytes(data, 0x0, (int)decompressedSize); // Decompressed Data

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
                        int[] searchResult = search(decompressedData, Dpointer, decompressedSize);

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
                return new MemoryStream();
            }
        }
  
        /* Do a search for data we can compress */
        private int[] search(byte[] decompressedData, uint pos, uint decompressedSize)
        {
            /* Set variables */
            int slidingWindowSize   = 4096; // Sliding Window Size
            int readAheadBufferSize = 18;   // Read Ahead Buffer Size

            /* Create a list of our results */
            List<int> results = new List<int>();

            if (pos < 3 || decompressedSize - pos < 3)
                return new int[] {0, 0};
            if (pos >= decompressedSize)
                return new int[] {-1, 0};

            /* Ok, search for data now */
            for (int i = 1; i < slidingWindowSize && i < pos; i++)
            {
                if (decompressedData[pos - i - 1] == decompressedData[pos])
                    results.Add(i + 1);
            }

            /* Did we get any results? */
            if (results.Count == 0)
                return new int[] {0, 0};

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
            return new int[] {amountOfBytes, results[0]};
        }
    }
}