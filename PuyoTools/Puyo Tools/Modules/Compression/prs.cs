using System;
using System.IO;
using System.Collections.Generic;
using Extensions;

namespace PuyoTools
{
    public class PRS : CompressionModule
    {
        public PRS()
        {
            Name = "PRS";
            CanCompress   = false;
            CanDecompress = true;
        }

        // Decompress
        public override MemoryStream Decompress(ref Stream data)
        {
            return Decompress(ref data, 0);
        }

        public MemoryStream Decompress(ref Stream data, uint decompressedSize)
        {
            try
            {
                /* Set variables */
                uint compressedSize = (uint)data.Length; // Compressed Size
                // Decompressed Size is not known

                uint Cpointer = 0x0; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData       = data.ReadBytes(0x0, compressedSize); // Compressed Data
                List<byte> decompressedData = new List<byte>(); // Decompressed Data

                while (Cpointer < compressedSize && (Dpointer < decompressedSize || decompressedSize == 0))
                {
                    byte Cflag = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        /* Is the data compressed? */
                        if ((Cflag & (1 << i)) > 0)
                        {
                            /* No */
                            decompressedData.Add(compressedData[Cpointer]);
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
                                Cpointer += 2;

                                /* Make sure we are not out of range */
                                if (Cpointer >= compressedSize)
                                    break;

                                offset = (uint)((second << 8 | first) >> 3) | 0xFFFFE000;
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
                                    offset = (amountToCopy << 1);
                                    amountToCopy = offset | (uint)((Cflag & (1 << i)) > 0 ? 0x1 : 0x0);
                                }
                                offset = (compressedData[Cpointer] | 0xFFFFFF00);
                                amountToCopy += 2;
                                Cpointer++;
                            }

                            /* Now copy the data */
                            for (int j = 0; j < amountToCopy; j++)
                                decompressedData.Add(decompressedData[(int)(Dpointer + offset + j)]);

                            Dpointer += amountToCopy;
                        }

                        /* Make sure we are not out of range */
                        if (Cpointer >= compressedSize || (Dpointer >= decompressedSize && decompressedSize != 0))
                            break;
                    }
                }

                return new MemoryStream(decompressedData.ToArray());
            }
            catch
            {
                return null;
            }
        }

        /* Compress */
        public override MemoryStream Compress(ref Stream data, string filename)
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

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            try
            {
                return (Path.GetExtension(filename) == ".prs");
            }
            catch
            {
                return false;
            }
        }
    }
}