using System;
using System.IO;

/* Compression Module */
namespace puyo_tools
{
    public class Compression
    {
        public Compression()
        {
        }

        /* Decompress a file. */
        public byte[] decompress(byte[] data)
        {
            switch (FileFormat.getCompressionFormat(data))
            {
                case CompressionFormat.CNX: // CNX Compression
                    CNX CNX_decompressor = new CNX();
                    return CNX_decompressor.decompress(data);

                case CompressionFormat.CXLZ: // CXLZ Compression
                    CXLZ CXLZ_decompressor = new CXLZ();
                    return CXLZ_decompressor.decompress(data);

                case CompressionFormat.LZ01: // LZ01 Compression
                    LZ01 LZ01_decompressor = new LZ01();
                    return LZ01_decompressor.decompress(data);

                //case CompressionFormat.ONZ: // ONZ Compression
                    //ONZ ONZ_decompressor = new ONZ();
                    //return ONZ_decompressor.decompress(data);
            }
            return data;
        }

        /* Get the output directory. */
        public string getOutputDirectory(byte[] data)
        {
            switch (FileFormat.getCompressionFormat(data))
            {
                case CompressionFormat.CNX:  return ExtractDir.CNX;
                case CompressionFormat.CXLZ: return ExtractDir.CXLZ;
                case CompressionFormat.LZ00: return ExtractDir.LZ00;
                case CompressionFormat.LZ01: return ExtractDir.LZ01;
                //case CompressionFormat.ONZ:  return ExtractDir.ONZ;
            }

            return String.Empty;
        }

        /* Get the output filename. */
        public string getFileName(byte[] data, string fileName)
        {
            switch (FileFormat.getCompressionFormat(data))
            {
                case CompressionFormat.CNX:
                    return Path.GetFileNameWithoutExtension(fileName) + "." + PadString.getStringFromBytes(data, 0x4, 3);
                //case CompressionFormat.ONZ:
                    //return Path.GetFileNameWithoutExtension(fileName) + ".one";
            }

            return fileName;
        }
    }
}