using System;
using System.IO;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class LZ00 : CompressionModule
    {
        /* LZ00 Cracked by QPjDDYwQLI
             thanks to author of ps2dis
        */

        public LZ00()
        {
            Name          = "LZ00";
            CanCompress   = false;
            CanDecompress = true;
        }

        /* Decompress */
        public override MemoryStream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = data.ReadUInt(0x4) - 0x40; // Compressed Size (0x40 = size of header)
                uint decompressedSize = data.ReadUInt(0x30); // Decompressed Size

                long xValue = data.ReadUInt(0x34); //Magic Value

                uint Cpointer = 0x0; // Compressed Pointer
                uint Dpointer = 0x0;  // Decompressed Pointer

                byte[] compressedData = data.ReadBytes(0x40, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                long a2, a3;
                long v1;
                long t0, t3, t4, t5;

                byte[] WorkMem = new byte[0x1000];

                t4 = 0x0fee;
                t3 = 0;

                for (; ; )
                {
                    t3 >>= 1;

                    if ((t3 & 0x100) == 0)
                    {
                        if (Cpointer < compressedSize)
                        {
                            a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            t0 = ((uint)xValue >> 16) & 0x7fff;
                            v1 = v1 ^ ((uint)(((t0 << 8) - t0) >> 15));
                        }
                        else
                        {
                            v1 = -1;
                        }

                        if (v1 == -1) break;

                        t3 = v1 | 0xff00;
                    }

                    if ((t3 & 1) == 0)
                    {
                        if (Cpointer < compressedSize)
                        {
                            a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            t0 = ((uint)xValue >> 16) & 0x7fff;
                            v1 = v1 ^ ((uint)((t0 << 8) - t0) >> 15);
                        }
                        else
                        {
                            v1 = -1;
                        }

                        if (v1 == -1) break;

                        if (Cpointer < compressedSize)
                        {
                            a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;

                            t5 = compressedData[Cpointer];
                            Cpointer++;

                            t0 = ((uint)xValue >> 16) & 0x7fff;
                            t0 = t5 ^ ((uint)((t0 << 8) - t0) >> 15);
                        }
                        else
                        {
                            t0 = -1;
                        }

                        if (t0 == -1) break;

                        t5 = 0;
                        a3 = (t0 & 0xf) + 2;

                        t0 = v1 | ((t0 & 0xf0) << 4);

                        do
                        {
                            a2 = WorkMem[(t0 + t5) & 0xfff];

                            if (Dpointer < decompressedSize)
                            {
                                decompressedData[Dpointer] = (byte)a2;
                                Dpointer++;
                            }

                            WorkMem[t4] = (byte)a2;

                            t5++;
                            t4 = (t4 + 1) & 0xfff;
                        } while (t5 <= a3);
                    }
                    else
                    {
                        if (Cpointer < compressedSize)
                        {
                            a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            t0 = ((uint)xValue >> 16) & 0x7fff;
                            a2 = v1 ^ ((uint)((t0 << 8) - t0) >> 15);
                        }
                        else
                        {
                            a2 = -1;
                        }

                        if (a2 == -1) break;

                        if (Dpointer < decompressedSize)
                        {
                            decompressedData[Dpointer] = (byte)a2;
                            Dpointer++;
                        }

                        WorkMem[t4] = (byte)a2;

                        t4 = (t4 + 1) & 0xfff;
                    }
                }

                return new MemoryStream(decompressedData);
            }
            catch
            {
                /* An error occured */
                return null;
            }
        }

        /* Compress */
        public override MemoryStream Compress(ref Stream data, string filename)
        {
            return null;
        }

        // Get the filename
        public override string DecompressFilename(ref Stream data, string filename)
        {
            string EmbeddedFilename = data.ReadString(0x10, 32);
            return (EmbeddedFilename == String.Empty ? filename : EmbeddedFilename);
        }
        public override string CompressFilename(ref Stream data, string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".mrg": return Path.GetFileNameWithoutExtension(filename) + ".mrz";
                case ".tex": return Path.GetFileNameWithoutExtension(filename) + ".tez";
            }

            return filename;
        }

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            try
            {
                return (data.ReadString(0x0, 4) == "LZ00");
            }
            catch
            {
                return false;
            }
        }
    }
}