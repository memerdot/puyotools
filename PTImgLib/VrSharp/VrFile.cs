// VrFile.cs
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

namespace VrSharp
{
    public class VrFile
    {
        // - Variables -
        static private byte[] GbixMagic = // GBIX Magic
            { 0x47, 0x42, 0x49, 0x58 };
        static private byte[] GcixMagic = // GCIX Magic
            { 0x47, 0x43, 0x49, 0x58 };

        static private byte[] GvrtMagic = // GVR Magic
            { 0x47, 0x56, 0x52, 0x54 };
        static private byte[] PvrtMagic = // SVR & PVR Magic
            { 0x50, 0x56, 0x52, 0x54 };

        static private byte[] GvrMagic = // GVR Magic
            { 0x47, 0x43, 0x49, 0x58 };
        static private byte[] SvrMagic = // SVR Magic
            { 0x47, 0x42, 0x49, 0x58 };
        private byte[] CompressedData;   // GVR Data
        private byte[] DecompressedData; // Regular, Decompressed Data
        private byte[] ExternalPalette = null;  // External Palette Data

        private int VrFileHeight;
        private int VrFileWidth;

        private uint VrPixelFormatCode;

        private int VrCodecHeaderLength;
        private int VrCodecChunkWidth;
        private int VrCodecChunkHeight;
        private int VrCodecChunkLength;

        private VrCodec VrCodec;
        private VrDecoder VrDecoder;
        private VrEncoder VrEncoder;

        private VrPaletteCodec VrPaletteCodec;
        private VrDataCodec VrDataCodec;

        private VrPaletteDecoder VrPaletteDecoder;
        private VrDataDecoder VrDataDecoder;

        private byte VrPaletteFormatCode;
        private byte VrDataFormatCode;

        private int VrFileOffset;

        private VrType VrType;

        public string FormatCodeString(uint fmtcode)
        {
            return fmtcode.ToString("X").PadLeft(8, '0');
        }


        // - Constructors -
        // VrFile(byte[] VrFile)
        // Parameters:
        //  VrFile: A byte array of the Vr file
        // Description: Loads a GVR from a byte array
        public VrFile(byte[] VrFile)
        {
            if (VrFile == null)
            {
                throw new ArgumentException("VrFile(byte[]): Argument 1, 'VrFile', Can not be null.");
            }
            SetCompressedData(VrFile);
        }
        public VrFile(byte[] VrFile, byte[] Palette)
        {
            if (VrFile == null)
            {
                throw new ArgumentException("VrFile(byte[]): Argument 1, 'VrFile', Can not be null.");
            }
            ExternalPalette = Palette;
            SetCompressedData(VrFile);
        }

        // VrFile(byte[] VrFile)
        // Parameters:
        //  VrFile: A byte array of the Vr file
        // Description: Loads a GVR from a byte array
        public VrFile(byte[] VrFile, int Width, int Height, VrFormat Format)
        {
            if (VrFile == null)
            {
                throw new ArgumentException("VrFile(byte[],int,int,VrFormat): Argument 1, 'VrFile', Can not be null.");
            }
            if (Width < 8)
            {
                throw new ArgumentException("VrFile(byte[],int,int,VrFormat): Argument 2, 'Width', Must be at least 8.");
            }
            if (Height < 8)
            {
                throw new ArgumentException("VrFile(byte[],int,int,VrFormat): Argument 3, 'Height', Must be at least 8.");
            }
            SetDecompressedData(VrFile, Width, Height, Format);
        }

        // VrFile(byte[] VrFile)
        // Parameters:
        //  VrFileName: A string to a filename of a Vr
        // Description: Loads a GVR from a file
        public VrFile(string VrFileName)
        {
            FileStream File = new FileStream(VrFileName, FileMode.Open);
            byte[] Data = new byte[File.Length];
            File.Read(Data, 0, Data.Length);
            File.Close();

            SetCompressedData(Data);
        }


        // public byte[] GetCompressedData()
        // Return Value: The byte array of the compressed data
        // Description: Gets the compressed GVR data
        public byte[] GetCompressedData()
        {
            return CompressedData;
        }
        // public byte[] GetCompressedData()
        // Return Value: The byte array of the decompressed data
        // Description: Gets the decompressed data
        public byte[] GetDecompressedData()
        {
            return DecompressedData;
        }
        // public byte[] GetExternalPaletteData()
        // Return Value: The byte array of the palette data
        // Description: Gets the raw external palette data
        //public byte[] GetExternalPaletteData()
        //{
        //    return ExternalPalData;
        //}


        // - Data Input -
        // Description: Inputs compressed GVR data (And unpacks it to RGBA8888)
        // Parameters:
        //  byte[] Compressed: Compressed Data to load
        // Return Value: True if the data was properly loaded.
        public bool SetCompressedData(byte[] Compressed)
        {
            if (Compressed == null)
            {
                throw new ArgumentException("SetCompressedData: Argument 1, 'Compressed', Can not be null.");
            }
            else
            {
                CompressedData = Compressed;
            }
            if (!IsGvr() && !IsSvr()) throw new NotVrException("The file sent to SetCompressedData() is not a Vr file.");


            // Get Format Code
            VrPixelFormatCode = (uint)(Compressed[0x18] << 24 | Compressed[0x19] << 16 | Compressed[0x1A] << 8 | Compressed[0x1B]);

            if (IsGvr())
            {
                //VrFileWidth = Compressed[0x1C] << 8 | Compressed[0x1D];
                //VrFileHeight = Compressed[0x1E] << 8 | Compressed[0x1F];
                if (IsMagic(Compressed, GvrtMagic, 0x0))
                    VrFileOffset = 0x0;
                else
                    VrFileOffset = 0x10;

                VrPaletteFormatCode = Compressed[0xA + VrFileOffset];
                VrDataFormatCode    = Compressed[0xB + VrFileOffset];

                VrPaletteCodec = GvrCodecs.GetPaletteCodec(VrPaletteFormatCode);
                VrDataCodec    = GvrCodecs.GetDataCodec(VrDataFormatCode);

                VrFileWidth  = Compressed[0xC + VrFileOffset] << 8 | Compressed[0xD + VrFileOffset];
                VrFileHeight = Compressed[0xE + VrFileOffset] << 8 | Compressed[0xF + VrFileOffset];
            }
            else
            {
                if (IsMagic(Compressed, PvrtMagic, 0x0))
                    VrFileOffset = 0x0;
                else
                    VrFileOffset = 0x10;

                VrPaletteFormatCode = Compressed[0x8 + VrFileOffset];
                VrDataFormatCode    = Compressed[0x9 + VrFileOffset];

                VrPaletteCodec = SvrCodecs.GetPaletteCodec(VrPaletteFormatCode);
                VrDataCodec    = SvrCodecs.GetDataCodec(VrDataFormatCode);

                VrFileWidth  = BitConverter.ToUInt16(Compressed, 0xC + VrFileOffset);
                VrFileHeight = BitConverter.ToUInt16(Compressed, 0xE + VrFileOffset);

                //VrFileWidth = Compressed[0x1C] | Compressed[0x1D] << 8;
                //VrFileHeight = Compressed[0x1E] | Compressed[0x1F] << 8;
            }

            DecompressedData = new byte[VrFileWidth * VrFileHeight * 4];

            //VrCodec = VrCodecs.GetCodec(FormatCodeString(VrPixelFormatCode));
            //if (VrCodec == null)
            if (VrPaletteCodec == null || VrDataCodec == null)
            {
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Format: 0x" + FormatCodeString(VrPixelFormatCode));
            }

            try
            {
                //VrDecoder = VrCodec.Decode;
                //VrEncoder = VrCodec.Encode;

                VrPaletteDecoder = VrPaletteCodec.Decode;
                VrDataDecoder    = VrDataCodec.Decode;

                VrDataDecoder.Initialize(VrFileWidth, VrFileHeight, VrPaletteDecoder);

                //VrDecoder.Initialize(CompressedData, VrFileWidth, VrFileHeight);
            }
            catch (Exception e)
            {
                throw new VrCodecLoadingException("The codec for format 0x" + FormatCodeString(VrPixelFormatCode) + " could not be loaded.", e);
            }

            /*if (VrDataDecoder.NeedExternalPalette())
            {
                if (ExternalPalette != null)
                {
                    VrDecoder.SendExternalPalette(ExternalPalData);
                }
                else
                {
                    throw new VrCodecNeedsPaletteException("The codec for format 0x" + FormatCodeString(VrPixelFormatCode) + " requires an external palette. The application must catch and handle this exception in order to use Vr files which need external palette data.");
                }
            }*/

            if (VrDataDecoder.NeedExternalPalette() && ExternalPalette == null)
            {
                throw new VrCodecNeedsPaletteException("The codec for format 0x" + FormatCodeString(VrPixelFormatCode) + " requires an external palette. The application must catch and handle this exception in order to use Vr files which need external palette data.");
            }

            //VrCodecChunkWidth = VrDecoder.GetChunkWidth();
            //VrCodecChunkHeight = VrDecoder.GetChunkHeight();
            //VrCodecChunkLength = VrDecoder.GetChunkSize();

            VrCodecChunkWidth  = VrDataDecoder.GetChunkWidth();
            VrCodecChunkHeight = VrDataDecoder.GetChunkHeight();
            VrCodecChunkLength = VrDataDecoder.GetChunkSize();

            if ((VrFileWidth / VrCodecChunkWidth) * VrCodecChunkWidth != VrFileWidth)
                Console.WriteLine("Warning: Image VrFileWidth is not divisible by " + VrCodecChunkWidth);

            if ((VrFileHeight / VrCodecChunkHeight) * VrCodecChunkHeight != VrFileHeight)
                Console.WriteLine("Warning: Image VrFileHeight is not divisible by " + VrCodecChunkHeight);

            //int ptr = 0x20;

            //VrDecoder.DecodeFormatHeader(ref CompressedData, ref ptr);

            int Pointer = 0x10 + VrFileOffset;

            // Decode Palette
            if (VrDataDecoder.NeedExternalPalette())
            {
                VrDataDecoder.DecodePalette(ref ExternalPalette, 0x10);
            }
            else
            {
                VrDataDecoder.DecodePalette(ref CompressedData, Pointer);
                Pointer += VrDataDecoder.GetPaletteSize();
            }

            for (int y = 0; y < VrFileHeight / VrCodecChunkHeight; y++)
            {
                for (int x = 0; x < VrFileWidth / VrCodecChunkWidth; x++)
                {
                    VrDataDecoder.DecodeChunk(ref CompressedData, ref Pointer, ref DecompressedData, x * VrCodecChunkWidth, y * VrCodecChunkHeight);
                    //VrDecoder.DecodeChunk(ref CompressedData, ref ptr, ref DecompressedData, x * VrCodecChunkWidth, y * VrCodecChunkHeight);
                }
            }

            return true;
        }
        // public bool SetDecompressedData(byte[] Decompressed, short FormatCode)
        // Parameters:
        //  byte[] Decompressed: Decompressed Data to load
        //  int Width: Width of the RGBA8888 image
        //  int Height: Height of the RGBA8888 image
        //  VrFormat FormatCode: The format the GVR data should be stored in
        // Return Value: True if the data was properly loaded.
        // Description: Inputs decompressed, RGBA8888 data (And packs it to GVR with the specified format code)
        public bool SetDecompressedData(byte[] Decompressed, int Width, int Height, VrFormat FormatCode)
        {
            DecompressedData = Decompressed;

            // Set the data passed to us
            VrPixelFormatCode = (ushort)FormatCode;
            VrFileWidth = Width;
            VrFileHeight = Height;

            // Get the codec
            Console.WriteLine("Format Code: 0x" + FormatCodeString(VrPixelFormatCode));
            VrCodec = VrCodecs.GetCodec(FormatCode.ToString("X"));
            VrDecoder = VrCodec.Decode;
            VrEncoder = VrCodec.Encode;

            // Get relevant data
            VrCodecHeaderLength = VrEncoder.GetFormatHeaderSize();
            VrCodecChunkWidth = VrEncoder.GetChunkWidth();
            VrCodecChunkHeight = VrEncoder.GetChunkHeight();
            VrCodecChunkLength = VrEncoder.GetChunkSize();

            // Size calculation
            int VrSize = (VrFileWidth / VrCodecChunkWidth * VrFileHeight / VrCodecChunkHeight) * VrCodecChunkLength + VrCodecHeaderLength + 0x20;

            // Allocate the buffers
            CompressedData = new byte[VrSize];

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
            CompressedData[0x15] = (byte)((VrSize >> 8) & 0xFF);
            CompressedData[0x16] = (byte)((VrSize >> 16) & 0xFF);
            CompressedData[0x17] = (byte)((VrSize >> 24) & 0xFF);

            CompressedData[0x04] = 0x08;
            CompressedData[0x1A] = (byte)((VrPixelFormatCode >> 8) & 0xFF);
            CompressedData[0x1B] = (byte)((VrPixelFormatCode >> 0) & 0xFF);
            CompressedData[0x1C] = (byte)((VrFileWidth >> 8) & 0xFF);
            CompressedData[0x1D] = (byte)((VrFileWidth >> 0) & 0xFF);
            CompressedData[0x1E] = (byte)((VrFileHeight >> 8) & 0xFF);
            CompressedData[0x1F] = (byte)((VrFileHeight >> 0) & 0xFF);

            // Initialize encoder
            VrEncoder.Initialize(ref DecompressedData, null, VrFileWidth, VrFileHeight);

            // Warn if the width and height are inappropriete
            if ((VrFileWidth / VrCodecChunkWidth) * VrCodecChunkWidth != VrFileWidth)
                Console.WriteLine("Warning: Image VrFileWidth is not divisible by " + VrCodecChunkWidth);

            if ((VrFileHeight / VrCodecChunkHeight) * VrCodecChunkHeight != VrFileHeight)
                Console.WriteLine("Warning: Image VrFileHeight is not divisible by " + VrCodecChunkHeight);

            int ptr = 0x20;

            VrEncoder.EncodeFormatHeader(ref CompressedData, ref ptr);

            for (int y = 0; y < VrFileHeight / VrCodecChunkHeight; y++)
            {
                for (int x = 0; x < VrFileWidth / VrCodecChunkWidth; x++)
                {
                    VrEncoder.EncodeChunk(ref CompressedData, ref ptr, ref DecompressedData, x * VrCodecChunkWidth, y * VrCodecChunkHeight);
                }
            }

            return true;
        }
        // public bool SetExternalPaletteData(byte[] ExternalPal)
        // Parameters:
        //  byte[] ExternalPal: External Palette
        // Return Value: True if the data was properly loaded.
        //public bool SetExternalPaletteData(byte[] ExternalPal)
        //{
        //    if (ExternalPal == null) return false;

        //    ExternalPalData = ExternalPal;

        //    return true;
        //}



        // - File Property Retrieval -
        // public bool IsGvr()
        // Return Value: True if the Data Magic is equivalant to that of a GVR file
        //               False if not.
        // Description: This function will allow you to validate that the file you have is GVR.
        static public bool IsGvr(byte[] FileContents)
        {
            if (FileContents.Length < 0x10) return false;

            if (IsMagic(FileContents, GbixMagic, 0x0) && FileContents.Length >= 0x20 && IsMagic(FileContents, GvrtMagic, 0x10))
                return true;
            if (IsMagic(FileContents, GcixMagic, 0x0) && FileContents.Length >= 0x20 && IsMagic(FileContents, GvrtMagic, 0x10))
                return true;
            if (IsMagic(FileContents, GvrtMagic, 0x0))
                return true;

            return false;
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
        // public bool IsSvr()
        // Return Value: True if the Data Magic is equivalant to that of a SVR file
        //               False if not.
        // Description: This function will allow you to validate that the file you have is SVR.
        static public bool IsSvr(byte[] FileContents)
        {
            if (FileContents.Length < 0x10) return false;

            if (IsMagic(FileContents, GbixMagic, 0x0) && FileContents.Length >= 0x20 && IsMagic(FileContents, PvrtMagic, 0x10) && FileContents[0x19] >= 0x60)
                return true;
            if (IsMagic(FileContents, PvrtMagic, 0x0) && FileContents[0x9] >= 0x60)
                return true;

            return false;
        }
        public bool IsSvr()
        {
            return IsSvr(CompressedData);
        }
        static public bool IsSvr(string Filename)
        {
            FileStream File = new FileStream(Filename, FileMode.Open);
            byte[] FileContents = new byte[SvrMagic.Length];
            File.Read(FileContents, 0, FileContents.Length);
            File.Close();

            return IsSvr(FileContents);
        }

        // public bool IsMagic()
        // Return Value: True if the magic is equal.
        //               False if not.
        // Description: This function will allow you to validate the magic matches.
        public static bool IsMagic(byte[] FileContents, byte[] Magic, int Position)
        {
            for (int i = 0; i < Magic.Length; i++)
                if (FileContents[i + Position] != Magic[i]) return false;

            return true;
        }

        public VrFormat GetFormatCode()
        {
            return (VrFormat)(CompressedData[0x1A] << 8 + CompressedData[0x1B]);
        }
        public uint GetUFormatCode()
        {
            return VrPixelFormatCode;
        }

        public int GetHeight()
        {
            return VrFileHeight;
        }

        public int GetWidth()
        {
            return VrFileWidth;
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