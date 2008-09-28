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
            }

            return String.Empty;
        }
    }
}