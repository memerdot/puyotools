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

                uint xValue = data.ReadUInt(0x34); //Magic Value

                uint Cpointer = 0x0; // Compressed Pointer
                uint Dpointer = 0x0;  // Decompressed Pointer

                byte[] compressedData = data.ReadBytes(0x40, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                long a2, a3;
                long v1;
                long t0, t3, t4, t5;

                byte[] WorkMem = new byte[0x1000];

                //t4 = 0x0fee;
                t4 = 0;
                t3 = 0;

                for (; ; )
                {
                    t3 >>= 1;

                    if ((t3 & 0x100) == 0)
                    {
                        if (Cpointer < compressedSize)
                        {
                            /*a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;*/
                            xValue = GetNewMagicValue(xValue);

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            /*t0 = ((uint)xValue >> 16) & 0x7fff;
                            v1 = v1 ^ ((uint)(((t0 << 8) - t0) >> 15));*/
                            v1 = DecryptByte((byte)v1, xValue);
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
                            /*a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;*/
                            xValue = GetNewMagicValue(xValue);

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            /*t0 = ((uint)xValue >> 16) & 0x7fff;
                            v1 = v1 ^ ((uint)((t0 << 8) - t0) >> 15);*/
                            v1 = DecryptByte((byte)v1, xValue);
                        }
                        else
                        {
                            v1 = -1;
                        }

                        if (v1 == -1) break;

                        if (Cpointer < compressedSize)
                        {
                            /*a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;*/
                            xValue = GetNewMagicValue(xValue);

                            t5 = compressedData[Cpointer];
                            Cpointer++;

                            /*t0 = ((uint)xValue >> 16) & 0x7fff;
                            t0 = t5 ^ ((uint)((t0 << 8) - t0) >> 15);*/
                            t0 = DecryptByte((byte)t5, xValue);
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

                            WorkMem[(t4 + 0x0fee) & 0xFFF] = (byte)a2;

                            t5++;
                            t4 = (t4 + 1) & 0xfff;
                        } while (t5 <= a3);
                    }
                    else
                    {
                        if (Cpointer < compressedSize)
                        {
                            /*a2 = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
                            a2 = (a2 << 6) - a2;
                            a2 = (a2 << 4) - a2;
                            xValue = ((a2 << 2) - a2) + 0x3039;*/
                            xValue = GetNewMagicValue(xValue);

                            v1 = compressedData[Cpointer];
                            Cpointer++;

                            /*t0 = ((uint)xValue >> 16) & 0x7fff;
                            a2 = v1 ^ ((uint)((t0 << 8) - t0) >> 15);*/
                            a2 = DecryptByte((byte)v1, xValue);
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

                        WorkMem[(t4 + 0x0fee) & 0xFFF] = (byte)a2;

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
            try
            {
                uint DecompressedSize = (uint)data.Length;

                MemoryStream CompressedData = new MemoryStream();
                byte[] DecompressedData     = data.ToByteArray();

                uint SourcePointer = 0x0;
                uint DestPointer   = 0x40;

                uint MagicValue = (uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

                // Test if the file is too large to be compressed
                if (data.Length > 0xFFFFFFFF)
                    throw new Exception("Input file is too large to compress.");

                // Set up the Lz Compression Dictionary
                LzCompressionDictionary LzDictionary = new LzCompressionDictionary();
                LzDictionary.SetWindowSize(0x1000);
                LzDictionary.SetMaxMatchAmount(0xF + 2);

                // Start compression
                CompressedData.Write("LZ00");
                CompressedData.Write((uint)0); // Will be filled in later
                CompressedData.Seek(8, SeekOrigin.Current); // Advance 8 bytes

                CompressedData.Write(filename, 31, 32);
                CompressedData.Write(DecompressedSize);
                CompressedData.Write(MagicValue);
                CompressedData.Seek(8, SeekOrigin.Current); // Advance 8 bytes

                while (SourcePointer < DecompressedSize)
                {
                    MagicValue = GetNewMagicValue(MagicValue);

                    byte Flag = 0x0;
                    uint FlagPosition   = DestPointer;
                    uint FlagMagicValue = MagicValue; // Since it won't be filled in now
                    CompressedData.WriteByte(Flag); // It will be filled in later
                    DestPointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        int[] LzSearchMatch = LzDictionary.Search(DecompressedData, SourcePointer, DecompressedSize);
                        if (LzSearchMatch[1] > 0) // There is a compression match
                        {
                            Flag |= (byte)(0 << i);

                            MagicValue = GetNewMagicValue(MagicValue);
                            CompressedData.WriteByte(EncryptByte((byte)((LzSearchMatch[0] - 1) & 0xFF), MagicValue));
                            MagicValue = GetNewMagicValue(MagicValue);
                            CompressedData.WriteByte(EncryptByte((byte)((((LzSearchMatch[0] - 1) & 0xF00) >> 4) | ((LzSearchMatch[1] - 2) & 0xF)), MagicValue));

                            LzDictionary.AddEntryRange(DecompressedData, (int)SourcePointer, LzSearchMatch[1]);
                            LzDictionary.SlideWindow(LzSearchMatch[1]);

                            SourcePointer += (uint)LzSearchMatch[1];
                            DestPointer   += 2;
                        }
                        else // There wasn't a match
                        {
                            Flag |= (byte)(1 << i);

                            MagicValue = GetNewMagicValue(MagicValue);
                            CompressedData.WriteByte(EncryptByte(DecompressedData[SourcePointer], MagicValue));

                            LzDictionary.AddEntry(DecompressedData, (int)SourcePointer);
                            LzDictionary.SlideWindow(1);

                            SourcePointer++;
                            DestPointer++;
                        }

                        // Check for out of bounds
                        if (SourcePointer >= DecompressedSize)
                            break;
                    }

                    // Write the flag.
                    // Note that the original position gets reset after writing.
                    CompressedData.Seek(FlagPosition, SeekOrigin.Begin);
                    CompressedData.WriteByte(EncryptByte(Flag, FlagMagicValue));
                    CompressedData.Seek(DestPointer, SeekOrigin.Begin);
                }

                CompressedData.Seek(0x4, SeekOrigin.Begin);
                CompressedData.Write((uint)CompressedData.Length);
                CompressedData.Seek(0, SeekOrigin.End);

                return CompressedData;
            }
            catch
            {
                return null; // An error occured while compressing
            }
        }

        // Get the new magic value
        private uint GetNewMagicValue(uint xValue)
        {
            uint x;

            x = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
            x = (x << 6) - x;
            x = (x << 4) - x;

            return ((x << 2) - x) + 0x3039;
        }

        // Decrypt & Encrypt bytes (they are the same function really)
        private byte DecryptByte(byte value, uint xValue)
        {
            uint t0 = ((uint)xValue >> 16) & 0x7fff;
            return (byte)(value ^ ((uint)(((t0 << 8) - t0) >> 15)));
        }
        private byte EncryptByte(byte value, uint xValue)
        {
            uint t0 = ((uint)xValue >> 16) & 0x7fff;
            return (byte)(value ^ ((uint)(((t0 << 8) - t0) >> 15)));
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