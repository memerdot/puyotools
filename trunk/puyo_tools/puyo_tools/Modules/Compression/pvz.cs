using System;
using System.IO;
using System.Collections.Generic;
using Extensions;
using VrSharp;

namespace puyo_tools
{
    public class PVZ : CompressionClass
    {
        public PVZ()
        {
        }

        /* Decompress */
        public override MemoryStream Decompress(ref Stream data)
        {
            try
            {
                /* Set variables */
                uint compressedSize   = (uint)data.Length;    // Compressed Size
                uint decompressedSize = data.ReadUInt(0x0); // Decompressed Size

                uint Cpointer = 0x4; // Compressed Pointer
                uint Dpointer = 0x0; // Decompressed Pointer

                byte[] compressedData   = data.ReadBytes(0x0, compressedSize); // Compressed Data
                byte[] decompressedData = new byte[decompressedSize]; // Decompressed Data

                /* This file heavily relies on VrSharp */
                int ChunkSize = 1;
                if (data.ReadString(0x4, 4) == GraphicHeader.GBIX && data.ReadString(0x14, 4) == GraphicHeader.PVRT)
                {
                    PvrPaletteCodec PaletteCodec = PvrCodecs.GetPaletteCodec(data.ReadByte(0x1C));
                    PvrDataCodec DataCodec       = PvrCodecs.GetDataCodec(data.ReadByte(0x1D));

                    if (PaletteCodec != null && DataCodec != null)
                    {
                        DataCodec.Decode.Initialize(0, 0, PaletteCodec.Decode);
                        ChunkSize = (DataCodec.Decode.GetChunkBpp() / 8);
                    }
                }

                /* Copy the first 32 bytes */
                for (int i = 0; i < 0x20; i++)
                {
                    decompressedData[Dpointer] = compressedData[Cpointer];
                    Dpointer++;
                    Cpointer++;
                }

                /* Ok, let's decompress the data */
                while (Cpointer < compressedSize && Dpointer < decompressedSize)
                {
                    int copyAmount = compressedData[Cpointer + ChunkSize] + 1;

                    for (int i = 0; i < copyAmount; i++)
                        Array.Copy(compressedData, Cpointer, decompressedData, Dpointer + (i * ChunkSize), ChunkSize);

                    Cpointer += (uint)(ChunkSize + 1);
                    Dpointer += (uint)(copyAmount * ChunkSize);
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
                /* Make sure this is a PVR with the GBIX header */
                Images images = new Images(data, filename);
                if (images.Format != GraphicFormat.PVR || data.ReadString(0x0, 4) != GraphicHeader.GBIX)
                    throw new Exception();

                /* Set variables */
                uint decompressedSize = (uint)data.Length; // Decompressed Size

                uint Dpointer = 0x0; // Decompressed Pointer

                MemoryStream compressedData = new MemoryStream(); // Compressed Data
                byte[] decompressedData     = data.ReadBytes(0x0, decompressedSize); // Decompressed Data

                /* This file heavily relies on VrSharp */
                int ChunkSize = 1;
                if (data.ReadString(0x0, 4) == GraphicHeader.GBIX)
                {
                    PvrPaletteCodec PaletteCodec = PvrCodecs.GetPaletteCodec(data.ReadByte(0x18));
                    PvrDataCodec DataCodec       = PvrCodecs.GetDataCodec(data.ReadByte(0x19));

                    if (PaletteCodec != null && DataCodec != null)
                    {
                        DataCodec.Decode.Initialize(0, 0, PaletteCodec.Decode);
                        ChunkSize = (DataCodec.Decode.GetChunkBpp() / 8);
                    }
                }

                /* Add the decompressed size */
                compressedData.Write(decompressedSize);

                /* Write the first 32 bytes */
                compressedData.Write(decompressedData, 0x0, 32);
                Dpointer += 0x20;

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
            catch
            {
                /* Something went wrong */
                return null;
            }
        }
    }
}