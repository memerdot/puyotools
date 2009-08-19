using System;
using System.IO;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class ONZ : CompressionModule
    {
        public ONZ()
        {
            Name = "LZ11"; // Also known as ONZ and LZ77 Format 11
            CanCompress   = true;
            CanDecompress = true;
        }

        /* Decompress */
        public override MemoryStream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length; // Compressed Size
                uint decompressedSize = data.ReadUInt(0x0) >> 8; // Decompressed Size

                uint Cpointer = 0x4; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                /* Some files (Let's Tap LZ7 files) may have their decompressed size stored in a different place */
                if (decompressedSize == 0)
                {
                    decompressedSize = data.ReadUInt(0x4);
                    Cpointer = 0x8;
                }

                byte[] compressedData   = data.ReadBytes(0x0, compressedSize); // Compressed Data
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
        public override MemoryStream Compress(ref Stream data, string filename)
        {
            try
            {
                uint DecompressedSize = (uint)data.Length;

                MemoryStream CompressedData = new MemoryStream();
                byte[] DecompressedData     = data.ToByteArray();

                uint SourcePointer = 0x0;
                uint DestPointer   = 0x4;

                // Test if the file is too large to be compressed
                if (data.Length > (1L << 32))
                    throw new Exception("Input file is too large to compress.");

                // Set up the Lz Compression Dictionary
                LzCompressionDictionary LzDictionary = new LzCompressionDictionary();
                LzDictionary.SetWindowSize(0x1000);
                LzDictionary.SetMaxMatchAmount(0xFFFF + 273);

                // Figure out where we are going to write the decompressed file size
                if (data.Length < (1 << 24))
                    CompressedData.Write((uint)('\x11' | (DecompressedSize << 8)));
                else
                {
                    CompressedData.Write((uint)('\x11'));
                    CompressedData.Write(DecompressedSize);
                    DestPointer += 0x4;
                }

                // Start compression
                while (SourcePointer < DecompressedSize)
                {
                    byte Flag = 0x0;
                    uint FlagPosition = DestPointer;
                    CompressedData.WriteByte(Flag); // It will be filled in later
                    DestPointer++;

                    for (int i = 7; i >= 0; i--)
                    {
                        int[] LzSearchMatch = LzDictionary.Search(DecompressedData, SourcePointer, DecompressedSize);
                        if (LzSearchMatch[1] > 0) // There is a compression match
                        {
                            Flag |= (byte)(1 << i);

                            // Write the distance/length pair
                            if (LzSearchMatch[1] <= 0xF + 1) // 2 bytes
                            {
                                CompressedData.WriteByte((byte)((((LzSearchMatch[1] - 1) & 0xF) << 4) | (((LzSearchMatch[0] - 1) & 0xFFF) >> 8)));
                                CompressedData.WriteByte((byte)((LzSearchMatch[0] - 1) & 0xFF));
                                DestPointer += 2;
                            }
                            else if (LzSearchMatch[1] <= 0xFF + 17) // 3 bytes
                            {
                                CompressedData.WriteByte((byte)(((LzSearchMatch[1] - 17) & 0xFF) >> 4));
                                CompressedData.WriteByte((byte)((((LzSearchMatch[1] - 17) & 0xF) << 4) | (((LzSearchMatch[0] - 1) & 0xFFF) >> 8)));
                                CompressedData.WriteByte((byte)((LzSearchMatch[0] - 1) & 0xFF));
                                DestPointer += 3;
                            }
                            else // 4 bytes
                            {
                                CompressedData.WriteByte((byte)((1 << 4) | (((LzSearchMatch[1] - 273) & 0xFFFF) >> 12)));
                                CompressedData.WriteByte((byte)(((LzSearchMatch[1] - 273) & 0xFFF) >> 4));
                                CompressedData.WriteByte((byte)((((LzSearchMatch[1] - 273) & 0xF) << 4) | (((LzSearchMatch[0] - 1) & 0xFFF) >> 8)));
                                CompressedData.WriteByte((byte)((LzSearchMatch[0] - 1) & 0xFF));
                                DestPointer += 4;
                            }

                            LzDictionary.AddEntryRange(DecompressedData, (int)SourcePointer, LzSearchMatch[1]);
                            LzDictionary.SlideWindow(LzSearchMatch[1]);

                            SourcePointer += (uint)LzSearchMatch[1];
                        }
                        else // There wasn't a match
                        {
                            Flag |= (byte)(0 << i);

                            CompressedData.WriteByte(DecompressedData[SourcePointer]);

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
                    CompressedData.WriteByte(Flag);
                    CompressedData.Seek(DestPointer, SeekOrigin.Begin);
                }

                return CompressedData;
            }
            catch
            {
                return null; // An error occured while compressing
            }
        }

        // Get Filename
        public override string DecompressFilename(ref Stream data, string filename)
        {
            /* Only return a different extension if the current one is onz */
            if (Path.GetExtension(filename).ToLower() == ".onz")
                return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".ONE" : ".one");

            return filename;
        }
        public override string CompressFilename(ref Stream data, string filename)
        {
            if (Path.GetExtension(filename).ToLower() == ".one")
                return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".ONZ" : ".onz");

            return filename;
        }

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            try
            {
                // Because this can conflict with other compression formats we are going to add a check them too
                return (data.ReadString(0x0, 1) == "\x11" &&
                    !Compression.Dictionary[CompressionFormat.PRS].Check(ref data, filename) &&
                    !Compression.Dictionary[CompressionFormat.PVZ].Check(ref data, filename));
            }
            catch
            {
                return false;
            }
        }
    }
}