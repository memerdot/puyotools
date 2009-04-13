using System;
using System.IO;

/* Compression Module */
namespace puyo_tools
{
    public class Compression
    {
        /* Compression format */
        private CompressionClass Compressor = null;
        public CompressionFormat Format     = CompressionFormat.NULL;
        private Stream Data                 = null;
        private string Filename             = null;
        private string CompressionName      = null;

        /* Compression Object for decompression */
        public Compression(Stream dataStream, string dataFilename)
        {
            /* Set up our compression information */
            Data              = dataStream;
            Filename          = dataFilename;

            CompressionInformation(ref Data, out Format, out Compressor, out CompressionName);
        }

        /* Compression object for compression */
        public Compression(Stream dataStream, string dataFilename, CompressionFormat format, CompressionClass compressor)
        {
            /* Set up our compression information */
            Data       = dataStream;
            Filename   = dataFilename;
            Format     = format;
            Compressor = compressor;
        }

        /* Decompress */
        public Stream Decompress()
        {
            return Compressor.Decompress(ref Data);
        }

        /* Compress */
        public Stream Compress()
        {
            return Compressor.Compress(ref Data);
        }

        /* Get filename */
        public string GetFilename()
        {
            return Compressor.GetFilename(ref Data, Filename);
        }

        /* Output Directory */
        public string OutputDirectory
        {
            get
            {
                return (CompressionName == null ? null : CompressionName + " Decompressed");
            }
        }

        /* Get compression information */
        private void CompressionInformation(ref Stream data, out CompressionFormat format, out CompressionClass compressor, out string name)
        {
            try
            {
                /* Check based on the first 4 bytes */
                switch ((CompressionHeader)ObjectConverter.StreamToUInt(data, 0x0))
                {
                    case CompressionHeader.CNX: // CNX
                        format     = CompressionFormat.CNX;
                        compressor = new CNX();
                        name       = "CNX";
                        return;

                    case CompressionHeader.CXLZ: // CXLZ
                        format     = CompressionFormat.CXLZ;
                        compressor = new CXLZ();
                        name       = "CXLZ";
                        return;

                    case CompressionHeader.LZ01: // LZ01
                        format     = CompressionFormat.LZ01;
                        compressor = new LZ01();
                        name       = "LZ01";
                        return;
                }

                /* Check compression based on the first byte */
                switch ((CompressionHeader)ObjectConverter.StreamToBytes(data, 0x0, 1)[0])
                {
                    case CompressionHeader.LZSS: // LZSS
                        format     = CompressionFormat.LZSS;
                        compressor = new LZSS();
                        name       = "LZSS";
                        return;
                }

                /* Unknown or unsupported compression */
                throw new CompressionFormatNotSupported();
            }
            catch (CompressionFormatNotSupported)
            {
                /* Unknown or unsupported compression */
                format     = CompressionFormat.NULL;
                compressor = null;
                name       = null;
                return;
            }
            catch
            {
                /* An error occured. */
                format     = CompressionFormat.NULL;
                compressor = null;
                name       = null;
                return;
            }

        }
    }

    /* Compression Format */
    public enum CompressionFormat : byte
    {
        NULL,
        CNX,
        CXLZ,
        LZ01,
        LZSS,
    }

    /* Compression Header */
    public enum CompressionHeader : uint
    {
        NULL = 0x00000000,
        CNX  = 0x02584E43,
        CXLZ = 0x5A4C5843,
        LZ01 = 0x31305A4C,
        LZSS = 0x00000010,
    }

    public abstract class CompressionClass
    {
        /* Compression Functions */
        public abstract Stream Decompress(ref Stream data); // Decompress Data
        public abstract Stream Compress(ref Stream data);   // Compress Data
        public abstract string GetFilename(ref Stream data, string filename); // Get Filname
    }
}