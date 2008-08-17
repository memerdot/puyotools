// PngFile.cs
// By Nmn / For PuyoNexus.net
// --
// I Hearby release this code under Public Domain.
// This code comes with absolutely no warrenty.

using System;
using System.IO;
using System.Drawing;

namespace ImgSharp
{
	
	public class ImgFile
	{
		// - Variables -
        private byte[] PngMagic  =       // Png Header Magic
			{ 0x89, 0x50, 0x4E, 0x47}; 
		private byte[] CompressedData;   // Png Data
		private byte[] DecompressedData; // Regular, Decompressed Data
        private int width;
        private int height;
        private System.Drawing.Imaging.ImageFormat fmt;
		
		// - Constructors -
		// PngFile(byte[] PngFile)
		// Parameters:
		//  PngFile: A byte array of the Png file
		// Description: Loads a Png from a byte array
		public ImgFile(byte[] PngFile)
		{
			SetCompressedData(PngFile);
        }
        // - Constructors -
        // PngFile(byte[] PngFile)
        // Parameters:
        //  PngFile: A byte array of ARGB8888
        // Description: Loads a raw ARGB8888 array
        public ImgFile(byte[] RgbaArray, int Width, int Height, System.Drawing.Imaging.ImageFormat OutputFormat)
        {
            fmt = OutputFormat;
            SetDecompressedData(RgbaArray, Width, Height);
        }
		
		// - Constructors -
		// PngFile(byte[] PngFile)
		// Parameters:
		//  PngFileName: A string to a filename of a Png
		// Description: Loads a Png from a file
		public ImgFile(string PngFileName)
		{
			FileStream File = new FileStream(PngFileName, FileMode.Open);
            byte [] Data = new byte[File.Length];
            File.Read(Data, 0, Data.Length);
            File.Close();

            if (Data.Length < 1) return;

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
		// Description: Gets the compressed Png data
		public byte[] GetCompressedData()
		{
			return CompressedData;
		}
		
		
		
		
		// - Data Input -
		// Description: Inputs compressed Png data (And unpacks it to ARGB8888)
		// Parameters:
		//  byte[] Compressed: Compressed Data to load
		// Return Value: True if the data was properly loaded.
		public bool SetCompressedData(byte[] Compressed)
		{
			CompressedData = Compressed;
			
			// Decompress
            Bitmap TmpBmp = new Bitmap(ImageConverter.byteArrayToImage(CompressedData, ref fmt));

            DecompressedData = new byte[TmpBmp.Width * TmpBmp.Height * 4];

            width = TmpBmp.Width;
            height = TmpBmp.Height;

			for(int y=0; y<TmpBmp.Height; y++)
			{
				for(int x=0; x<TmpBmp.Width; x++)
				{
					Color pxc = TmpBmp.GetPixel(x,y);
                    DecompressedData[(y * TmpBmp.Width + x) * 4 + 0] = pxc.A;
                    DecompressedData[(y * TmpBmp.Width + x) * 4 + 1] = pxc.R;
                    DecompressedData[(y * TmpBmp.Width + x) * 4 + 2] = pxc.G;
                    DecompressedData[(y * TmpBmp.Width + x) * 4 + 3] = pxc.B;
				}
			}
			
			return true;
		}
		// public bool SetDecompressedData(byte[] Decompressed, short FormatCode)
		// Parameters:
		//  byte[] Decompressed: Decompressed ARGB8888 image to load
		//  int Width: Width of the ARGB8888 image
		//  int Height: Height of the ARGB8888 image
		// Return Value: True if the data was properly loaded.
		// Description: Inputs decompressed, ARGB8888 data (And packs it to an image)
		public bool SetDecompressedData(byte[] Decompressed, int Width, int Height)
		{
            DecompressedData = Decompressed;

            width = Width;
            height = Height;

            // Compress
            Bitmap TmpBmp = new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < TmpBmp.Height; y++)
            {
                for (int x = 0; x < TmpBmp.Width; x++)
                {
                    int a = DecompressedData[(y * Width + x) * 4 + 0];
                    int r = DecompressedData[(y * Width + x) * 4 + 1];
                    int g = DecompressedData[(y * Width + x) * 4 + 2];
                    int b = DecompressedData[(y * Width + x) * 4 + 3];
                    TmpBmp.SetPixel(x,y,Color.FromArgb(a, r, g, b));
                }
            }
            CompressedData = ImageConverter.imageToByteArray(TmpBmp, fmt);
			
			return true;
		}
		
		
		
		
		// - File Property Retrieval -
		// public bool IsPng()
		// Return Value: True if the Data Magic is equivalant to that of a Png file
		//               False if not.
		// Description: This function will allow you to validate that the file you have is Png.
		public bool IsPng()
		{
            if (CompressedData.Length < PngMagic.Length) return false;
			
            for (int i = 0; i < PngMagic.Length; i++)
				if (CompressedData[CompressedData.Length] != PngMagic[i]) return false;

            return true;
		}
		public int Length()
		{
			return CompressedData.Length;
        }
        public int GetWidth()
        {
            return width;
        }
        public int GetHeight()
        {
            return height;
        }
	}
}