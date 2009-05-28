using System;
using System.IO;
using Extensions;
using System.Collections.Generic;

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
        private bool compress               = false;

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
            compress   = true;
        }

        /* Blank compression class, so you can access methods */
        public Compression()
        {
        }

        /* Decompress */
        public MemoryStream Decompress()
        {
            return Compressor.Decompress(ref Data);
        }

        /* Compress */
        public MemoryStream Compress()
        {
            return Compressor.Compress(ref Data, Filename);
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
                return (CompressionName == null ? null : CompressionName + (compress ? " Compressed" : " Decompressed"));
            }
        }

        /* Get compression information */
        private void CompressionInformation(ref Stream data, out CompressionFormat format, out CompressionClass compressor, out string name)
        {
            try
            {
                /* Check based on the first 4 bytes */
                switch (data.ReadString(0x0, 4))
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
                switch (data.ReadString(0x0, 1))
                {
                    case CompressionHeader.LZSS: // LZSS
                        format     = CompressionFormat.LZSS;
                        compressor = new LZSS();
                        name       = "LZSS";
                        return;

                    case CompressionHeader.ONZ: // ONZ
                        format     = CompressionFormat.ONZ;
                        compressor = new ONZ();
                        name       = "ONZ";
                        return;
                }

                /* Check file extension */
                switch (Path.GetExtension(Filename).ToLower())
                {
                    case ".prs": // PRS
                        format     = CompressionFormat.PRS;
                        compressor = new PRS();
                        name       = "PRS";
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

        /* Get compression information, used for compressing */
        public void CompressorInformation(CompressionFormat format, out CompressionClass compressor, out string name)
        {
            switch (format)
            {
                case CompressionFormat.CXLZ:
                    compressor = new CXLZ();
                    name       = "CXLZ";
                    return;
                case CompressionFormat.LZSS:
                    compressor = new LZSS();
                    name       = "LZSS";
                    return;
            }

            compressor = null;
            name       = null;
            return;
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
        ONZ,
        PRS,
    }

    /* Compression Header */
    public static class CompressionHeader
    {
        public const string
            CNX  = "CNX\x02",
            CXLZ = "CXLZ",
            LZ01 = "LZ01",
            LZSS = "\x10",
            ONZ  = "\x11";
    }

    public abstract class CompressionClass
    {
        /* Compression Functions */
        public abstract MemoryStream Decompress(ref Stream data); // Decompress Data
        public abstract MemoryStream Compress(ref Stream data, string filename); // Compress Data
        public virtual string GetFilename(ref Stream data, string filename) // Get Filname
        {
            return filename;
        }

        /* Search for data that can be compressed (LZ compression formats) */
        public int[] LZsearch(ref byte[] decompressedData, uint pos, uint decompressedSize)
        {
            /* Set variables */
            int slidingWindowSize   = 4096; // Sliding Window Size
            int readAheadBufferSize = 18;   // Read Ahead Buffer Size

            /* Create a list of our results */
            List<int> results = new List<int>();

            if (pos < 3 || decompressedSize - pos < 3)
                return new int[] { 0, 0 };
            if (pos >= decompressedSize)
                return new int[] { -1, 0 };

            /* Ok, search for data now */
            for (int i = 1; i < slidingWindowSize && i < pos; i++)
            {
                if (decompressedData[pos - i - 1] == decompressedData[pos])
                    results.Add(i + 1);
            }

            /* Did we get any results? */
            if (results.Count == 0)
                return new int[] { 0, 0 };

            bool finish = false;
            int amountOfBytes = 0;

            while (amountOfBytes < readAheadBufferSize && !finish)
            {
                amountOfBytes++;
                for (int i = 0; i < results.Count; i++)
                {
                    /* Make sure we aren't out of range */
                    if (pos + amountOfBytes >= decompressedSize)
                    {
                        finish = true;
                        break;
                    }

                    if (decompressedData[pos + amountOfBytes] != decompressedData[pos - results[i] + (amountOfBytes % results[i])])
                    {
                        if (results.Count > 1)
                        {
                            results.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            finish = true;
                            break;
                        }
                    }
                }
            }

            /* Ok, return our results now */
            return new int[] { amountOfBytes, results[0] };
        }
    }
}