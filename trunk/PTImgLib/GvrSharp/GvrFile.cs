// GvrFile.cs
// By Nmn / For PuyoNexus.net
// --
// I Hearby release this code under Public Domain.
// This code comes with absolutely no warrenty.

using System;
using System.IO;
using System.Collections;

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
// -- How to create a new codec --
// ----
// Quite simple really.
// 1. Add a new entry in "GvrFormat"
// 2. Create your derivitive of "GvrDecoder" (See comments and existing implementatins for hints)
// 3. Add to the "SetDecompressedData()" switch statement in "GvrFile"
// And now you have a brand new Gvr codec capable of decoding.
// ----


namespace GvrSharp
{
	public class GvrFile
	{
		// - Variables -
        static private byte[] GvrMagic = // GVR Magic
			{ 0x47, 0x43, 0x49, 0x58 }; 
		private byte[] CompressedData;   // GVR Data
		private byte[] DecompressedData; // Regular, Decompressed Data

        private int height;
        private int width;

        private ushort fmtcode;

        private int chunkwidth;
        private int chunkheight;
        private int chunksize;

        private GvrDecoder codec;
		
		
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
			
			// Decompress
            fmtcode = (ushort)(Compressed[0x1A] << 8 | Compressed[0x1B]);
            Console.WriteLine("Format Code: 0x" + fmtcode.ToString("X"));

            switch(fmtcode)
            {
                case (ushort)GvrFormat.Pal_565_8x4:
                    codec = new Pal_565_8x4_Decode();
                    break;
            }

            width  = Compressed[0x1C] << 8 | Compressed[0x1D];
            height = Compressed[0x1E] << 8 | Compressed[0x1F];

            DecompressedData = new byte[width * height * 4];

            codec.Initialize(CompressedData, width, height);

            chunkwidth = codec.GetChunkWidth();
            chunkheight = codec.GetChunkHeight();
            chunksize = codec.GetChunkSize();

            if ((width / chunkwidth) * chunkwidth != width)
                Console.WriteLine("Warning: Image width is not divisible by " + chunkwidth);

            if ((height / chunkheight) * chunkheight != height)
                Console.WriteLine("Warning: Image height is not divisible by " + chunkheight);

            int ptr = 0x20;

            codec.DecodeFormatHeader(ref CompressedData, ref ptr);

            for (int y = 0; y < height / chunkheight; y++)
            {
                for (int x = 0; x < width / chunkwidth; x++)
                {
                    codec.DecodeChunk(ref CompressedData, ref ptr, ref DecompressedData, x * chunkwidth, y * chunkheight);
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

			// Compress
			
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
            IsGvr(CompressedData);

            return true;
        }
        static public bool IsGvr(string Filename)
        {
            FileStream File = new FileStream(Filename, FileMode.Open);
            byte[] FileContents = new byte[GvrMagic.Length];
            File.Read(FileContents, 0, FileContents.Length);
            File.Close();
            IsGvr(FileContents);

            return true;
        }
		public GvrFormat GetFormatCode()
		{
			return (GvrFormat)(CompressedData[0x1A] * 256 + CompressedData[0x1B]);
		}

        public int GetHeight()
        {
            return height;
        }

        public int GetWidth()
        {
            return width;
        }
    }
}
