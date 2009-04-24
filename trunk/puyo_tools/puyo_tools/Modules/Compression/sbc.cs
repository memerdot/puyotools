using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class SBC : CompressionClass
    {
        public SBC()
        {
        }

        /* Decompress */
        public Stream Decompress(ref Stream data, uint decompressedSize)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length; // Compressed Size
                // Decompressed Size already set

                uint Cpointer = 0x0; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData   = ObjectConverter.StreamToBytes(data, 0x0, (int)compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        /* Is the data compressed? */
                        if ((Cflag & (1 << i)) > 0)
                        {
                            /* No */
                            decompressedData[Dpointer] = compressedData[Cpointer];
                            Cpointer++;
                            Dpointer++;
                        }
                        else
                        {
                            /* Yes */
                            i++;
                            if (i >= 8)
                            {
                                i = 0;
                                Cflag = compressedData[Cpointer];
                                Cpointer++;
                            }

                            /* How will we copy this? */
                            uint offset, amountToCopy;
                            if ((Cflag & (1 << i)) > 0)
                            {
                                byte first  = compressedData[Cpointer];
                                byte second = compressedData[Cpointer + 1];
                                Cpointer   += 2;

                                offset       = (uint)((second << 8 | first) >> 3) | 0xFFFFE000;
                                amountToCopy = first & (uint)0x7;

                                if (amountToCopy == 0)
                                {
                                    amountToCopy = (uint)compressedData[Cpointer] + 1;
                                    Cpointer++;
                                }
                                else
                                    amountToCopy += 2;
                            }
                            else
                            {
                                amountToCopy = 0;
                                for (int j = 0; j < 2; j++)
                                {
                                    i++;
                                    if (i >= 8)
                                    {
                                        i = 0;
                                        Cflag = compressedData[Cpointer];
                                        Cpointer++;
                                    
                                    }
                                    offset       = (amountToCopy << 1);
                                    amountToCopy = offset | (uint)((Cflag & (1 << i)) > 0 ? 0x1 : 0x0);
                                }
                                offset = (compressedData[Cpointer] | 0xFFFFFF00);
                                amountToCopy += 2;
                                Cpointer++;
                            }

                            /* Now copy the data */
                            for (int j = 0; j < amountToCopy; j++)
                                decompressedData[Dpointer + j] = decompressedData[Dpointer + offset + j];

                            Dpointer += amountToCopy;
                        }

                        /* Make sure we are not out of range */
                        if (Cpointer >= compressedSize || Dpointer >= decompressedSize)
                            break;
                    }
                }

                return new MemoryStream(decompressedData);
            }
            catch
            {
                return null;
            }
        }
        public override Stream Decompress(ref Stream data)
        {
            return null;
        }

        /* Compress */
        public override Stream Compress(ref Stream data, string filename)
        {
            try
            {
                return null;
            }
            catch
            {
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