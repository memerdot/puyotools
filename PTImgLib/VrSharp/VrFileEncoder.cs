using System;

namespace VrSharp
{
    public abstract class VrFileEncoder : VrFile
    {
        public short VrFileWidth;
        public short VrFileHeight;
        public bool VrGbixHeader;
        public VrPaletteCodec VrPixelCodec;
        public VrDataCodec VrDataCodec;
        public byte VrPaletteFormatCode;
        public byte VrDataFormatCode;
        public uint VrGlobalIndex;
        public int VrFileOffset;
        public int VrHeaderSize;
        public int VrDataSize;
        public int VrDataChunkWidth;
        public int VrDataChunkHeight;
    }

    public class GvrFileEncoder : VrFileEncoder
    {
        private GvrPaletteFormat GvrPaletteFormat;
        private GvrDataFormat GvrDataFormat;
        private VrPaletteEncoder GvrPaletteEncoder;
        private VrDataEncoder GvrDataEncoder;
        private bool GvrGamecubeGbix;

        // Converts a bitmap to a GVR with the specified palette and data format and other options
        public GvrFileEncoder(byte[] BitmapFile, int Width, int Height, GvrPaletteFormat PaletteFormat, GvrDataFormat DataFormat, bool GbixHeader, uint GlobalIndex, bool GamecubeGvr)
        {
            if (BitmapFile == null)
            {
                throw new ArgumentException("GvrFileEncoder(byte[]): Argument 1, 'BitmapFile', Can not be null.");
            }

            GvrPaletteFormat    = PaletteFormat;
            GvrDataFormat       = DataFormat;
            VrPaletteFormatCode = (byte)PaletteFormat;
            VrDataFormatCode    = (byte)DataFormat;

            VrFileWidth     = (short)Width;
            VrFileHeight    = (short)Height;
            VrGbixHeader    = GbixHeader;
            VrGlobalIndex   = GlobalIndex;
            GvrGamecubeGbix = GamecubeGvr;

            SetDecompressedData(BitmapFile);
        }

        public bool SetDecompressedData(byte[] Decompressed)
        {
            // Set up our encoders
            VrPixelCodec = GvrCodecs.GetPaletteCodec(VrPaletteFormatCode);
            VrDataCodec    = GvrCodecs.GetDataCodec(VrDataFormatCode);

            // Throw an exception if the palette or data codec does not exist
            if (VrPixelCodec == null && VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0') + " and Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrPixelCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));

            GvrPaletteEncoder = VrPixelCodec.Encode;
            GvrDataEncoder    = VrDataCodec.Encode;

            // Get size of header and file offset
            if (VrGbixHeader)
            {
                VrHeaderSize = 0x20;
                VrFileOffset = 0x10;
            }
            else
            {
                VrHeaderSize = 0x10;
                VrFileOffset = 0x00;
            }

            VrDataSize     = VrFileWidth * VrFileHeight * (GvrPaletteEncoder.GetBpp() / 8);
            CompressedData = new byte[VrHeaderSize + VrDataSize];

            // Start setting the file properties
            if (VrGbixHeader)
            {
                if (GvrGamecubeGbix)
                    GbixMagic.CopyTo(CompressedData, 0x00); // Gbix (Gamecube)
                else
                    GcixMagic.CopyTo(CompressedData, 0x00); // Gcix (Wii)

                BitConverter.GetBytes(0x08).CopyTo(CompressedData, 0x04); // Gbix Size
                BitConverter.GetBytes(swap32(VrGlobalIndex)).CopyTo(CompressedData, 0x08); // Global Index

                // Padding bytes (0x00)
                CompressedData[0x0C] = 0x00;
                CompressedData[0x0D] = 0x00;
                CompressedData[0x0E] = 0x00;
                CompressedData[0x0F] = 0x00;
            }

            PvrtMagic.CopyTo(CompressedData, VrFileOffset + 0x00); // Pvrt
            BitConverter.GetBytes(VrDataSize + 8).CopyTo(CompressedData, VrFileOffset + 0x04); // Filesize

            // Palette and Data Formats
            CompressedData[VrFileOffset + 0x08] = 0x00;
            CompressedData[VrFileOffset + 0x09] = 0x00;
            CompressedData[VrFileOffset + 0x0A] = VrPaletteFormatCode;
            CompressedData[VrFileOffset + 0x0B] = VrDataFormatCode;

            BitConverter.GetBytes(swap16((ushort)VrFileWidth)).CopyTo(CompressedData,  VrFileOffset + 0x0C); // Width
            BitConverter.GetBytes(swap16((ushort)VrFileHeight)).CopyTo(CompressedData, VrFileOffset + 0x0E); // Height

            // Now write the image data
            int Pointer = 0x10 + VrFileOffset;
            GvrDataEncoder.Initialize(ref Decompressed, Pointer, VrFileWidth, VrFileHeight, GvrPaletteEncoder);

            VrDataChunkWidth  = GvrDataEncoder.GetChunkWidth();
            VrDataChunkHeight = GvrDataEncoder.GetChunkHeight();
            
            for (int y = 0; y < VrFileHeight / VrDataChunkHeight; y++)
            {
                for (int x = 0; x < VrFileWidth / VrDataChunkWidth; x++)
                {
                    GvrDataEncoder.EncodeChunk(ref Decompressed, ref Pointer, ref CompressedData, x * VrDataChunkWidth, y * VrDataChunkHeight);
                }
            }
            GvrDataEncoder.Finalize(ref CompressedData, ref VrHeaderSize);

            return true;
        }

        private ushort swap16(ushort x)
        {
            return (ushort)((x << 8) | (x >> 8));
        }

        private uint swap32(uint x)
        {
            return (x >> 24) |
                ((x << 8) & 0x00FF0000) |
                ((x >> 8) & 0x0000FF00) |
                (x << 24);
        }
    }

    public class SvrFileEncoder : VrFileEncoder
    {
        private SvrPaletteFormat SvrPaletteFormat;
        private SvrDataFormat SvrDataFormat;
        private VrPaletteEncoder SvrPaletteEncoder;
        private VrDataEncoder SvrDataEncoder;

        // Converts a bitmap to a PVR with the specified palette and data format and other options
        public SvrFileEncoder(byte[] BitmapFile, int Width, int Height, SvrPaletteFormat PaletteFormat, SvrDataFormat DataFormat, bool GbixHeader, uint GlobalIndex)
        {
            if (BitmapFile == null)
            {
                throw new ArgumentException("SvrFileEncoder(byte[]): Argument 1, 'BitmapFile', Can not be null.");
            }

            SvrPaletteFormat    = PaletteFormat;
            SvrDataFormat       = DataFormat;
            VrPaletteFormatCode = (byte)PaletteFormat;
            VrDataFormatCode    = (byte)DataFormat;

            VrFileWidth   = (short)Width;
            VrFileHeight  = (short)Height;
            VrGbixHeader  = GbixHeader;
            VrGlobalIndex = GlobalIndex;

            SetDecompressedData(BitmapFile);
        }

        public bool SetDecompressedData(byte[] Decompressed)
        {
            // Set up our encoders
            VrPixelCodec = SvrCodecs.GetPaletteCodec(VrPaletteFormatCode);
            VrDataCodec  = SvrCodecs.GetDataCodec(VrDataFormatCode);

            // Throw an exception if the palette or data codec does not exist
            if (VrPixelCodec == null && VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0') + " and Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrPixelCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));

            SvrPaletteEncoder = VrPixelCodec.Encode;
            SvrDataEncoder    = VrDataCodec.Encode;

            // Get size of header and file offset
            if (VrGbixHeader)
            {
                VrHeaderSize = 0x20;
                VrFileOffset = 0x10;
            }
            else
            {
                VrHeaderSize = 0x10;
                VrFileOffset = 0x00;
            }

            VrDataSize     = VrFileWidth * VrFileHeight * (SvrPaletteEncoder.GetBpp() / 8);
            CompressedData = new byte[VrHeaderSize + VrDataSize];

            // Start setting the file properties
            if (VrGbixHeader)
            {
                GbixMagic.CopyTo(CompressedData, 0x00); // Gbix
                BitConverter.GetBytes(0x08).CopyTo(CompressedData, 0x04); // Gbix Size
                BitConverter.GetBytes(VrGlobalIndex).CopyTo(CompressedData, 0x08); // Global Index

                // Padding bytes (0x00)
                CompressedData[0x0C] = 0x00;
                CompressedData[0x0D] = 0x00;
                CompressedData[0x0E] = 0x00;
                CompressedData[0x0F] = 0x00;
            }

            PvrtMagic.CopyTo(CompressedData, VrFileOffset + 0x00); // Pvrt
            BitConverter.GetBytes(VrDataSize + 8).CopyTo(CompressedData, VrFileOffset + 0x04); // Filesize

            // Palette and Data Formats
            CompressedData[VrFileOffset + 0x08] = VrPaletteFormatCode;
            CompressedData[VrFileOffset + 0x09] = VrDataFormatCode;
            CompressedData[VrFileOffset + 0x0A] = 0x00;
            CompressedData[VrFileOffset + 0x0B] = 0x00;

            BitConverter.GetBytes(VrFileWidth).CopyTo(CompressedData,  VrFileOffset + 0x0C);  // Width
            BitConverter.GetBytes(VrFileHeight).CopyTo(CompressedData, VrFileOffset + 0x0E); // Height

            // Now write the image data
            int Pointer = 0x10 + VrFileOffset;
            SvrDataEncoder.Initialize(ref Decompressed, Pointer, VrFileWidth, VrFileHeight, SvrPaletteEncoder);

            VrDataChunkWidth  = SvrDataEncoder.GetChunkWidth();
            VrDataChunkHeight = SvrDataEncoder.GetChunkHeight();

            for (int y = 0; y < VrFileHeight / VrDataChunkHeight; y++)
            {
                for (int x = 0; x < VrFileWidth / VrDataChunkWidth; x++)
                {
                    SvrDataEncoder.EncodeChunk(ref Decompressed, ref Pointer, ref CompressedData, x * VrDataChunkWidth, y * VrDataChunkHeight);
                }
            }
            SvrDataEncoder.Finalize(ref CompressedData, ref VrHeaderSize);

            return true;
        }
    }

    public class PvrFileEncoder : VrFileEncoder
    {
        private PvrPixelFormat PvrPixelFormat;
        private PvrDataFormat PvrDataFormat;
        private VrPaletteEncoder PvrPaletteEncoder;
        private VrDataEncoder PvrDataEncoder;

        // Converts a bitmap to a PVR with the specified palette and data format and other options
        public PvrFileEncoder(byte[] BitmapFile, int Width, int Height, PvrPixelFormat PaletteFormat, PvrDataFormat DataFormat, bool GbixHeader, uint GlobalIndex)
        {
            if (BitmapFile == null)
            {
                throw new ArgumentException("PvrFileEncoder(byte[]): Argument 1, 'BitmapFile', Can not be null.");
            }

            PvrPixelFormat      = PaletteFormat;
            PvrDataFormat       = DataFormat;
            VrPaletteFormatCode = (byte)PaletteFormat;
            VrDataFormatCode    = (byte)DataFormat;

            VrFileWidth   = (short)Width;
            VrFileHeight  = (short)Height;
            VrGbixHeader  = GbixHeader;
            VrGlobalIndex = GlobalIndex;

            SetDecompressedData(BitmapFile);
        }

        public bool SetDecompressedData(byte[] Decompressed)
        {
            // Set up our encoders
            VrPixelCodec = PvrCodecs.GetPixelCodec(VrPaletteFormatCode);
            VrDataCodec  = PvrCodecs.GetDataCodec(VrDataFormatCode);

            // Throw an exception if the palette or data codec does not exist
            if (VrPixelCodec == null && VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0') + " and Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrPixelCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Pixel Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));

            PvrPaletteEncoder = VrPixelCodec.Encode;
            PvrDataEncoder    = VrDataCodec.Encode;

            // Get size of header and file offset
            if (VrGbixHeader)
            {
                VrHeaderSize = 0x20;
                VrFileOffset = 0x10;
            }
            else
            {
                VrHeaderSize = 0x10;
                VrFileOffset = 0x00;
            }

            VrDataSize     = VrFileWidth * VrFileHeight * (PvrPaletteEncoder.GetBpp() / 8);
            CompressedData = new byte[VrHeaderSize + VrDataSize];

            // Start setting the file properties
            if (VrGbixHeader)
            {
                GbixMagic.CopyTo(CompressedData, 0x00); // Gbix
                BitConverter.GetBytes(0x08).CopyTo(CompressedData, 0x04); // Gbix Size
                BitConverter.GetBytes(VrGlobalIndex).CopyTo(CompressedData, 0x08); // Global Index

                // Padding bytes (0x20)
                CompressedData[0x0C] = 0x20;
                CompressedData[0x0D] = 0x20;
                CompressedData[0x0E] = 0x20;
                CompressedData[0x0F] = 0x20;
            }

            PvrtMagic.CopyTo(CompressedData, VrFileOffset + 0x00); // Pvrt
            BitConverter.GetBytes(VrDataSize + 8).CopyTo(CompressedData, VrFileOffset + 0x04); // Filesize

            // Palette and Data Formats
            CompressedData[VrFileOffset + 0x08] = VrPaletteFormatCode;
            CompressedData[VrFileOffset + 0x09] = VrDataFormatCode;
            CompressedData[VrFileOffset + 0x0A] = 0x00;
            CompressedData[VrFileOffset + 0x0B] = 0x00;

            BitConverter.GetBytes(VrFileWidth).CopyTo(CompressedData,  VrFileOffset + 0x0C);  // Width
            BitConverter.GetBytes(VrFileHeight).CopyTo(CompressedData, VrFileOffset + 0x0E); // Height

            // Now write the image data
            int Pointer = 0x10 + VrFileOffset;
            PvrDataEncoder.Initialize(ref Decompressed, Pointer, VrFileWidth, VrFileHeight, PvrPaletteEncoder);

            VrDataChunkWidth  = PvrDataEncoder.GetChunkWidth();
            VrDataChunkHeight = PvrDataEncoder.GetChunkHeight();

            for (int y = 0; y < VrFileHeight / VrDataChunkHeight; y++)
            {
                for (int x = 0; x < VrFileWidth / VrDataChunkWidth; x++)
                {
                    PvrDataEncoder.EncodeChunk(ref Decompressed, ref Pointer, ref CompressedData, x * VrDataChunkWidth, y * VrDataChunkHeight);
                }
            }
            PvrDataEncoder.Finalize(ref CompressedData, ref VrHeaderSize);

            return true;
        }
    }
}