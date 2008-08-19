// GvrFile.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

// - GVR File Format -
// ----
//
//        0x00  Image Header 1
// 0x00 - 0x03   'GCIX'   - Magic Code, 4 bytes
// 0x04 - 0x07   Unknown  - 4 bytes // Possibly useless, Possibly offset to Image Header 2.
// 0x08 - 0x0F   Reserved - 8 bytes // Always Zero, Probably usable for metadata.
//
//        0x10  Image Header 2
// 0x10 - 0x13   'GVRT'   - Format Magic Code, 4 bytes
// 0x14 - 0x17   Size     - 4 bytes // Looks like the post-header filesize but I ignore it.
// 0x18 - 0x1B   Pix. Fmt - 4 bytes // The first two bytes aren't used as far as I know.
// 0x1C - 0x1D   Width    - 2 bytes (Note: Big Endian)
// 0x1E - 0x1F   Height   - 2 bytes (Note: Big Endian)
//
// Now, the remainder of the file is determined by Pix. Fmt.
// An important thing to remember is that we're dealing with a Big Endian file.
// Big endian only varies in byte order for types larger than one byte - It's just reverse.
// --
// Unfortunately, we have no way of telling if multiple pixels are packed into
// a wordsize larger than the bytes per pixel.
// --
// Paletted pixel formats likely have a format header. This does not include
// the chunk-interleaved headers for block based paletting.
// ----

using System;
using System.IO;
using System.Collections;

namespace GvrSharp
{
	public class GvrFile
	{
		// - Variables -
        static private byte[] GvrMagic = // GVR Magic
			{ 0x47, 0x43, 0x49, 0x58 }; 
		private byte[] CompressedData;   // GVR Data
		private byte[] DecompressedData; // Regular, Decompressed Data

        private int GvrFileHeight;
        private int GvrFileWidth;

        private ushort GvrPixelFormatCode;

        private int GvrCodecHeaderLength;
        private int GvrCodecChunkWidth;
        private int GvrCodecChunkHeight;
        private int GvrCodecChunkLength;

        private GvrCodec GvrCodec;
        private GvrDecoder GvrDecoder;
        private GvrEncoder GvrEncoder;
		
		
		// - Constructors -
		// GvrFile(byte[] GvrFile)
		// Parameters:
		//  GvrFile: A byte array of the Gvr file
		// Description: Loads a GVR from a byte array
		public GvrFile(byte[] GvrFile)
		{
			SetCompressedData(GvrFile);
        }

        // GvrFile(byte[] GvrFile)
        // Parameters:
        //  GvrFile: A byte array of the Gvr file
        // Description: Loads a GVR from a byte array
        public GvrFile(byte[] GvrFile, int Width, int Height, GvrFormat Format)
        {
            SetDecompressedData(GvrFile,Width,Height,Format);
        }
		
		// GvrFile(byte[] GvrFile)
		// Parameters:
		//  GvrFileName: A string to a filename of a Gvr
		// Description: Loads a GVR from a file
		public GvrFile(string GvrFileName)
		{
			FileStream File = new FileStream(GvrFileName, FileMode.Open);
            byte [] Data = new byte[File.Length];
            File.Read(Data, 0, Data.Length);
            File.Close();
			
			SetCompressedData(Data);
		}
		
		
		// public byte[] GetCompressedData()
		// Return Value: The byte array of the decompressed data
		// Description: Gets the decompressed data
		public byte[] GetDecompressedData()
		{
			return DecompressedData;
		}
		// public byte[] GetCompressedData()
		// Return Value: The byte array of the compressed data
		// Description: Gets the compressed GVR data
		public byte[] GetCompressedData()
		{
			return CompressedData;
		}
		
		
		// - Data Input -
		// Description: Inputs compressed GVR data (And unpacks it to RGBA8888)
		// Parameters:
		//  byte[] Compressed: Compressed Data to load
		// Return Value: True if the data was properly loaded.
		public bool SetCompressedData(byte[] Compressed)
		{
			CompressedData = Compressed;

            if (!IsGvr()) return false;
			
			// Get Format Code
            GvrPixelFormatCode = (ushort)(Compressed[0x1A] << 8 | Compressed[0x1B]);
            Console.WriteLine("Format Code: 0x" + GvrPixelFormatCode.ToString("X"));

            GvrCodec = GvrCodecs.GetCodec(GvrPixelFormatCode.ToString("X"));
            GvrDecoder = GvrCodec.Decode;
            GvrEncoder = GvrCodec.Encode;

            GvrFileWidth  = Compressed[0x1C] << 8 | Compressed[0x1D];
            GvrFileHeight = Compressed[0x1E] << 8 | Compressed[0x1F];

            DecompressedData = new byte[GvrFileWidth * GvrFileHeight * 4];

            GvrDecoder.Initialize(CompressedData, GvrFileWidth, GvrFileHeight);

            GvrCodecChunkWidth = GvrDecoder.GetChunkWidth();
            GvrCodecChunkHeight = GvrDecoder.GetChunkHeight();
            GvrCodecChunkLength = GvrDecoder.GetChunkSize();

            if ((GvrFileWidth / GvrCodecChunkWidth) * GvrCodecChunkWidth != GvrFileWidth)
                Console.WriteLine("Warning: Image GvrFileWidth is not divisible by " + GvrCodecChunkWidth);

            if ((GvrFileHeight / GvrCodecChunkHeight) * GvrCodecChunkHeight != GvrFileHeight)
                Console.WriteLine("Warning: Image GvrFileHeight is not divisible by " + GvrCodecChunkHeight);

            int ptr = 0x20;

            GvrDecoder.DecodeFormatHeader(ref CompressedData, ref ptr);

            for (int y = 0; y < GvrFileHeight / GvrCodecChunkHeight; y++)
            {
                for (int x = 0; x < GvrFileWidth / GvrCodecChunkWidth; x++)
                {
                    GvrDecoder.DecodeChunk(ref CompressedData, ref ptr, ref DecompressedData, x * GvrCodecChunkWidth, y * GvrCodecChunkHeight);
                }
            }

			return true;
		}
		// public bool SetDecompressedData(byte[] Decompressed, short FormatCode)
		// Parameters:
		//  byte[] Decompressed: Decompressed Data to load
		//  int Width: Width of the RGBA8888 image
		//  int Height: Height of the RGBA8888 image
		//  GvrFormat FormatCode: The format the GVR data should be stored in
		// Return Value: True if the data was properly loaded.
		// Description: Inputs decompressed, RGBA8888 data (And packs it to GVR with the specified format code)
		public bool SetDecompressedData(byte[] Decompressed, int Width, int Height, GvrFormat FormatCode)
		{
            DecompressedData = Decompressed;

            // Set the data passed to us
            GvrPixelFormatCode = (ushort)FormatCode;
            GvrFileWidth = Width;
            GvrFileHeight = Height;

            // Get the codec
            Console.WriteLine("Format Code: 0x" + GvrPixelFormatCode.ToString("X"));
            GvrCodec = GvrCodecs.GetCodec(FormatCode.ToString("X"));
            GvrDecoder = GvrCodec.Decode;
            GvrEncoder = GvrCodec.Encode;

            // Get relevant data
            GvrCodecHeaderLength = GvrEncoder.GetFormatHeaderSize();
            GvrCodecChunkWidth = GvrEncoder.GetChunkWidth();
            GvrCodecChunkHeight = GvrEncoder.GetChunkHeight();
            GvrCodecChunkLength = GvrEncoder.GetChunkSize();

            // Size calculation
            int GvrSize = (GvrFileWidth / GvrCodecChunkWidth * GvrFileHeight / GvrCodecChunkHeight) * GvrCodecChunkLength + GvrCodecHeaderLength + 0x20;

            // Allocate the buffers
            CompressedData = new byte[GvrSize];

            // Set the file properties
            //47 56 52 54
            CompressedData[0x00] = 0x47;
            CompressedData[0x01] = 0x43;
            CompressedData[0x02] = 0x49;
            CompressedData[0x03] = 0x58;

            CompressedData[0x04] = 0x08;

            CompressedData[0x10] = 0x47;
            CompressedData[0x11] = 0x56;
            CompressedData[0x12] = 0x52;
            CompressedData[0x13] = 0x54;

            CompressedData[0x14] = 0x08;
            CompressedData[0x15] = (byte)((GvrSize >> 8) & 0xFF);
            CompressedData[0x16] = (byte)((GvrSize >> 16) & 0xFF);
            CompressedData[0x17] = (byte)((GvrSize >> 24) & 0xFF);

            CompressedData[0x04] = 0x08;
            CompressedData[0x1A] = (byte)((GvrPixelFormatCode >> 8) & 0xFF);
            CompressedData[0x1B] = (byte)((GvrPixelFormatCode >> 0) & 0xFF);
            CompressedData[0x1C] = (byte)((GvrFileWidth >> 8) & 0xFF);
            CompressedData[0x1D] = (byte)((GvrFileWidth >> 0) & 0xFF);
            CompressedData[0x1E] = (byte)((GvrFileHeight >> 8) & 0xFF);
            CompressedData[0x1F] = (byte)((GvrFileHeight >> 0) & 0xFF);

            // Initialize encoder
            GvrEncoder.Initialize(ref DecompressedData, null, GvrFileWidth, GvrFileHeight);

            // Warn if the width and height are inappropriete
            if ((GvrFileWidth / GvrCodecChunkWidth) * GvrCodecChunkWidth != GvrFileWidth)
                Console.WriteLine("Warning: Image GvrFileWidth is not divisible by " + GvrCodecChunkWidth);

            if ((GvrFileHeight / GvrCodecChunkHeight) * GvrCodecChunkHeight != GvrFileHeight)
                Console.WriteLine("Warning: Image GvrFileHeight is not divisible by " + GvrCodecChunkHeight);

            int ptr = 0x20;

            GvrEncoder.EncodeFormatHeader(ref CompressedData, ref ptr);

            for (int y = 0; y < GvrFileHeight / GvrCodecChunkHeight; y++)
            {
                for (int x = 0; x < GvrFileWidth / GvrCodecChunkWidth; x++)
                {
                    GvrEncoder.EncodeChunk(ref CompressedData, ref ptr, ref DecompressedData, x * GvrCodecChunkWidth, y * GvrCodecChunkHeight);
                }
            }
			
			return true;
		}
		
		

		// - File Property Retrieval -
		// public bool IsGvr()
		// Return Value: True if the Data Magic is equivalant to that of a GVR file
		//               False if not.
        // Description: This function will allow you to validate that the file you have is GVR.
        static public bool IsGvr(byte[] FileContents)
        {
            if (FileContents.Length < GvrMagic.Length) return false;

            for (int i = 0; i < GvrMagic.Length; i++)
                if (FileContents[i] != GvrMagic[i]) return false;

            return true;
        }
		public bool IsGvr()
		{
            return IsGvr(CompressedData);
        }
        static public bool IsGvr(string Filename)
        {
            FileStream File = new FileStream(Filename, FileMode.Open);
            byte[] FileContents = new byte[GvrMagic.Length];
            File.Read(FileContents, 0, FileContents.Length);
            File.Close();

            return IsGvr(FileContents);
        }

		public GvrFormat GetFormatCode()
		{
			return (GvrFormat)(CompressedData[0x1A] << 8 + CompressedData[0x1B]);
		}

        public int GetHeight()
        {
            return GvrFileHeight;
        }

        public int GetWidth()
        {
            return GvrFileWidth;
        }

        public int Length()
        {
            return DecompressedData.Length;
        }

        public int CompressedLength()
        {
            return CompressedData.Length;
        }
    }
}
