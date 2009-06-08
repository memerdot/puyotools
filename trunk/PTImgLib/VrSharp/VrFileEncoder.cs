using System;

namespace VrSharp
{
    public abstract class VrFileEncoder : VrFile
    {
        public short VrFileWidth;
        public short VrFileHeight;
        public bool VrGbixHeader;
        public VrPaletteCodec VrPaletteCodec;
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

    public class PvrFileEncoder : VrFileEncoder
    {
        private PvrPaletteFormat PvrPaletteFormat;
        private PvrDataFormat PvrDataFormat;
        private VrPaletteEncoder PvrPaletteEncoder;
        private VrDataEncoder PvrDataEncoder;

        // Converts a bitmap to a PVR with the specified palette and data format and other options
        public PvrFileEncoder(byte[] BitmapFile, int Width, int Height, PvrPaletteFormat PaletteFormat, PvrDataFormat DataFormat, bool GbixHeader, uint GlobalIndex)
        {
            if (BitmapFile == null)
            {
                throw new ArgumentException("PvrFileEncoder(byte[]): Argument 1, 'BitmapFile', Can not be null.");
            }

            PvrPaletteFormat    = PaletteFormat;
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
            VrPaletteCodec = PvrCodecs.GetPaletteCodec(VrPaletteFormatCode);
            VrDataCodec    = PvrCodecs.GetDataCodec(VrDataFormatCode);

            // Throw an exception if the palette or data codec does not exist
            if (VrPaletteCodec == null && VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Palette Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0') + " and Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrPaletteCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Palette Format " + VrPaletteFormatCode.ToString("X").PadLeft(2, '0'));
            else if (VrDataCodec == null)
                throw new VrNoSuitableCodecException("No Acceptable Vr Codec Found For Data Format " + VrDataFormatCode.ToString("X").PadLeft(2, '0'));

            PvrPaletteEncoder = VrPaletteCodec.Encode;
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

            VrDataSize = VrFileWidth * VrFileHeight * (PvrPaletteEncoder.GetBpp() / 8);
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
            CompressedData[VrFileOffset + 0x08] = (byte)PvrPaletteFormat;
            CompressedData[VrFileOffset + 0x09] = (byte)PvrDataFormat;
            CompressedData[VrFileOffset + 0x0A] = 0x00;
            CompressedData[VrFileOffset + 0x0B] = 0x00;

            BitConverter.GetBytes(VrFileWidth).CopyTo(CompressedData,  VrFileOffset + 0x0C);  // Width
            BitConverter.GetBytes(VrFileHeight).CopyTo(CompressedData, VrFileOffset + 0x0E); // Height

            // Now write the image data
            PvrDataEncoder.Initialize(VrFileWidth, VrFileHeight, PvrPaletteEncoder);

            VrDataChunkWidth  = PvrDataEncoder.GetChunkWidth();
            VrDataChunkHeight = PvrDataEncoder.GetChunkHeight();
            int Pointer = 0x10 + VrFileOffset;
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