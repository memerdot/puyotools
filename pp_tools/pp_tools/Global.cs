using System;
using System.Windows.Forms;

namespace pp_tools
{
    public class Header
    {
        /* List of headers for files */

        /* Compression headers */
        public static byte[]
            CNX  = { 0x43, 0x4E, 0x58, 0x02 }, // CNX
            LZ01 = { 0x4C, 0x5A, 0x30, 0x31 }, // LZ01
            CXLZ = { 0x43, 0x58, 0x4C, 0x5A }; // CXLZ

        /* Graphic Formats */
        public static byte[]
            SVR = { 0x47, 0x42, 0x49, 0x58 }, // SVR
            SVP = { 0x50, 0x56, 0x50, 0x4C }, // SVP (Pallete for SVR.)
            GVR = { 0x47, 0x43, 0x49, 0x58 }, // GVR
            GIM = { 0x2E, 0x47, 0x49, 0x4D }, // GIM (Little Endian)
            MIG = { 0x4D, 0x49, 0x47, 0x2E }, // GIM (Big Endian)
            TGA = { 0x2E, 0x74, 0x67, 0x61 }; // TGA (Actually an extension.)

        /* Archive Formats */
        public static byte[]
            ACX = { 0x00, 0x00, 0x00, 0x00 }, // ACX
            AFS = { 0x41, 0x46, 0x53, 0x00 }, // AFS
            MRG = { 0x4D, 0x52, 0x47, 0x30 }, // MRG
            GNT = { 0x4E, 0x47, 0x49, 0x46 }, // GNT
            SNT = { 0x4E, 0x55, 0x49, 0x46 }, // SNT
            TEX = { 0x54, 0x45, 0x58, 0x30 }; // TEX


        /* Special */
        public static byte[]
            SNT2 = { 0x4E, 0x55, 0x54, 0x4C }, // SNT file (not SNC)
            GNT2 = { 0x4E, 0x47, 0x54, 0x4C }; // GNT file (not GNCS)


        /* Checks if it is the following file format */
        public static bool isFile(byte[] data, byte[] header, int offset)
        {
            if (offset < 0 || offset + header.Length >= data.Length)
                return false;

            for (int i = 0; i < header.Length; i++)
            {
                if (data[offset + i] != header[i])
                    return false;
            }

            return true;
        }
    }

    /* Extraction Directories */
    public class ExtractDir
    {
        /* Archives */
        public static string
            ACX = "ACX Extracted Files", // ACX Archive
            AFS = "AFS Extracted Files", // AFS Archive
            GNT = "GNT Extracted Files", // GNT Archive
            MRG = "MRG Extracted Files", // MRG Archive
            SNT = "SNT Extracted Files", // SNT Archive
            TEX = "TEX Extracted Files", // TEX Archive
            VDD = "VDD Extracted Files"; // VDD Archive


        /* Decompression */
        public static string
            LZ01 = "LZ01 Decompressed Files", // LZ01 Compression
            CXLZ = "CXLZ Decompressed Files"; // CXLZ Compression
    }

    /* Endian Swaps */
    public class Endian
    {
        /* Swap integer endian. */
        public static uint swapInt(uint x)
        {
            x = (x >> 24) |
                ((x << 8) & 0x00FF0000) |
                ((x >> 8) & 0x0000FF00) |
                (x << 24);

            return x;
        }
    }

    /* File Selections */
    public class Files
    {
        /* Select Files */
        public static string[] selectFiles(string title, string filter)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title            = title;
            ofd.Multiselect      = true;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists  = true;
            ofd.CheckPathExists  = true;
            ofd.AddExtension     = true;
            ofd.Filter           = filter;
            ofd.DefaultExt       = "";
            ofd.ShowDialog();

            return ofd.FileNames;
        }
    }

}