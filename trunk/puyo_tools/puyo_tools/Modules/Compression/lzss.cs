using System;
using System.IO;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class LZSS : CompressionModule
    {
        public LZSS()
        {
            Name = "LZSS";
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

                byte[] compressedData   = data.ReadBytes(0x0, compressedSize); // Compressed Data
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
                if (data.Length > (1 << 24))
                    throw new Exception("Input file is too large to compress.");

                // Set up the Lz Compression Dictionary
                LzCompressionDictionary LzDictionary = new LzCompressionDictionary();
                LzDictionary.SetWindowSize(0x1000);
                LzDictionary.SetMaxMatchAmount(0xF + 3);

                // Start compression
                CompressedData.Write((uint)('\x10' | (DecompressedSize << 8)));
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

                            CompressedData.WriteByte((byte)((((LzSearchMatch[1] - 3) & 0xF) << 4) | (((LzSearchMatch[0] - 1) & 0xFFF) >> 8)));
                            CompressedData.WriteByte((byte)((LzSearchMatch[0] - 1) & 0xFF));

                            LzDictionary.AddEntryRange(DecompressedData, (int)SourcePointer, LzSearchMatch[1]);
                            LzDictionary.SlideWindow(LzSearchMatch[1]);

                            SourcePointer += (uint)LzSearchMatch[1];
                            DestPointer   += 2;
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

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            try
            {
                // Because this can conflict with other compression formats we are going to add a check them too
                return (data.ReadString(0x0, 1) == "\x10" &&
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