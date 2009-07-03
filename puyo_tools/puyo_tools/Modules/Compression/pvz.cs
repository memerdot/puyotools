using System;
using System.IO;
using System.Collections.Generic;
using Extensions;
using VrSharp;

namespace puyo_tools
{
    public class PVZ : CompressionModule
    {
        public PVZ()
        {
            Name          = "PVZ";
            CanCompress   = true;
            CanDecompress = true;
        }

        // Decompress
        public override MemoryStream Decompress(ref Stream data)
        {
            try
            {
                // Set variables
                uint compressedSize   = (uint)data.Length;  // Compressed Size
                uint decompressedSize = data.ReadUInt(0x0); // Decompressed Size

                uint Cpointer = 0x4; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData   = data.ReadBytes(0x0, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize];          // Decompressed Data

                // This file heavily relies on VrSharp
                // Check to see if this is a valid PVR file
                Images images = new Images(data.Copy(0x4, (int)data.Length - 4), null);
                if (images.Format != GraphicFormat.PVR)
                    throw new Exception();

                // Get correct file offset
                int FileOffset = (data.ReadString(0x4, 4) == "GBIX" ? 0x10 : 0x0);

                PvrPixelCodec PaletteCodec = PvrCodecs.GetPixelCodec(data.ReadByte(FileOffset + 0xC));
                PvrDataCodec DataCodec     = PvrCodecs.GetDataCodec(data.ReadByte(FileOffset + 0xD));
                if (PaletteCodec == null || DataCodec == null)
                    throw new Exception();

                DataCodec.Decode.Initialize(0, 0, PaletteCodec.Decode);
                int ChunkSize = (DataCodec.Decode.GetChunkBpp() / 8);

                // Copy the first 16/32 bytes
                for (int i = 0; i < FileOffset + 0x10; i++)
                {
                    decompressedData[Dpointer] = compressedData[Cpointer];
                    Dpointer++;
                    Cpointer++;
                }

                // Ok, let's decompress the data
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    int copyAmount = compressedData[Cpointer + ChunkSize] + 1;

                    for (int i = 0; i < copyAmount; i++)
                        Array.Copy(compressedData, Cpointer, decompressedData, Dpointer + (i * ChunkSize), ChunkSize);

                    Cpointer += (uint)(ChunkSize + 1);
                    Dpointer += (uint)(copyAmount * ChunkSize);
                }

                // Alright, return the stream now 
                return new MemoryStream(decompressedData);
            }
            catch
            {
                // Something went wrong
                return null;
            }
        }

        /* Compress */
        public override MemoryStream Compress(ref Stream data, string filename)
        {
            try
            {
                // Make sure this is a PVR
                Images images = new Images(data, filename);
                if (images.Format != GraphicFormat.PVR)
                    throw new Exception();

                // Get the file offset
                int FileOffset = 0x0;
                if (data.ReadString(0x0, 4) == "GBIX")
                    FileOffset = 0x10;

                /* Set variables */
                uint decompressedSize = (uint)data.Length; // Decompressed Size

                uint Dpointer = 0x0; // Decompressed Pointer

                MemoryStream compressedData = new MemoryStream(); // Compressed Data
                byte[] decompressedData     = data.ReadBytes(0x0, decompressedSize); // Decompressed Data

                // This file relies heavily on VrSharp
                PvrPixelCodec PixelCodec = PvrCodecs.GetPixelCodec(data.ReadByte(FileOffset + 0x8));
                PvrDataCodec DataCodec   = PvrCodecs.GetDataCodec(data.ReadByte(FileOffset + 0x9));

                if (PixelCodec == null || DataCodec == null)
                    throw new Exception();

                DataCodec.Decode.Initialize(0, 0, PixelCodec.Decode);
                // We can't compress it if the chunk size is less than 8
                if (DataCodec.Decode.GetChunkBpp() < 8)
                    throw new Exception("Can't compress 4-bit PVR textures.");
                int ChunkSize = (DataCodec.Decode.GetChunkBpp() / 8);

                // Start writing the compressed data
                // Add the decompressed size
                compressedData.Write(decompressedSize);

                /* Write the first 16/32 bytes */
                compressedData.Write(decompressedData, 0x0, 0x10 + FileOffset);
                Dpointer += (uint)FileOffset + 0x10;

                /* Ok, now let's start creating the compressed data */
                while (Dpointer < decompressedSize)
                {
                    byte[] pixelData = new byte[ChunkSize];
                    Array.Copy(decompressedData, Dpointer, pixelData, 0, ChunkSize);
                    compressedData.Write(pixelData);
                    Dpointer += (uint)ChunkSize;

                    /* Check for matches after this pixel */
                    byte copyAmount = 0;
                    while (Dpointer + ChunkSize <= decompressedSize && copyAmount < 255)
                    {
                        bool match = true;

                        for (int i = 0; i < ChunkSize && match; i++)
                        {
                            if (decompressedData[Dpointer + i] != pixelData[i])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            copyAmount++;
                            Dpointer += (uint)ChunkSize;
                        }
                        else
                            break;
                    }

                    /* Write the amount now */
                    compressedData.WriteByte(copyAmount);
                }

                return compressedData;
            }
            catch (Exception f)
            {
                /* Something went wrong */
                System.Windows.Forms.MessageBox.Show(f.ToString());
                return null;
            }
        }

        // Get the filename
        public override string DecompressFilename(ref Stream data, string filename)
        {
            // Only return a different extension if the current one is pvz
            if (Path.GetExtension(filename).ToLower() == ".pvz")
                return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".PVR" : ".pvr");

            return filename;
        }
        public override string CompressFilename(ref Stream data, string filename)
        {
            // Since we can only compress PVR files, add a pvz extension
            return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".PVZ" : ".pvz");
        }

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            try
            {
                return ((data.ReadString(0x4, 4) == "GBIX" && data.ReadString(0x14, 4) == "PVRT") ||
                    data.ReadString(0x4, 4) == "PVRT");
            }
            catch
            {
                return false;
            }
        }
    }
}