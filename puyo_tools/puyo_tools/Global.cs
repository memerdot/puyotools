using System;
using System.Windows.Forms;

namespace puyo_tools
{
    public class Header
    {
        /* List of headers for files */

        /* Compression headers */
        public static byte[]
            CNX  = { 0x43, 0x4E, 0x58, 0x02 }, // CNX
            LZ00 = { 0x4C, 0x5A, 0x30, 0x30 }, // LZ01
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
        SNT_PS2 = { 0x4E, 0x53, 0x49, 0x46 }, // SNT (PS2)
        SNT_PSP = { 0x4E, 0x55, 0x49, 0x46 }, // SNT (PSP)
            SPK = { 0x53, 0x4E, 0x44, 0x30 }, // SPK
            TEX = { 0x54, 0x45, 0x58, 0x30 }; // TEX


        /* Special */
        public static byte[]
        SNT_SUB_PS2 = { 0x4E, 0x53, 0x54, 0x4C }, // SNT file (PS2) (not SNC)
        SNT_SUB_PSP = { 0x4E, 0x55, 0x54, 0x4C }, // SNT file (PSP) (not SNC)
            GNT_SUB = { 0x4E, 0x47, 0x54, 0x4C }; // GNT file (not GNCS)


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
            SPK = "SPK Extracted Files", // SPK Archive
            TEX = "TEX Extracted Files", // TEX Archive
            VDD = "VDD Extracted Files"; // VDD Archive


        /* Decompression */
        public static string
            CNX  = "CNX Decompressed Files",  // CNX Compression
            CXLZ = "CXLZ Decompressed Files", // CXLZ Compression
            LZ00 = "LZ00 Decompressed Files", // LZ00 Compression
            LZ01 = "LZ01 Decompressed Files"; // LZ01 Compression

    }

    /* Endian Swaps */
    public class Endian
    {
        /* Swap short endian. */
        public static ushort swapShort(ushort x)
        {
            x = (ushort)((x >> 8) |
                (x << 8));

            return x;
        }

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

        /* Save File */
        public static string saveFile(string title, string filter)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title            = title;
            sfd.RestoreDirectory = true;
            sfd.AddExtension     = true;
            sfd.Filter           = filter;
            sfd.DefaultExt       = "";
            sfd.ShowDialog();

            return sfd.FileName;
        }
    }

    /* Padding for String */
    public class PadString
    {
        /* Pad a string to a certain multiple and convert it to an array of bytes. */
        public static byte[] multipleToBytes(string str, int multiple)
        {
            byte[] output = new byte[str.Length + (str.Length % multiple)];

            for (int i = 0; i < str.Length; i++)
                output[i] = (byte)str[i];

            return output;
        }

        /* Converts a filename to byte array. */
        public static byte[] fileNameToBytes(string str, int length)
        {
            byte[] output = new byte[length];

            for (int i = 0; i < str.Length && i < length; i++)
                output[i] = (byte)str[i];

            return output;
        }

        /* Get string from bytes. */
        public static string getStringFromBytes(byte[] data, int offset, int length)
        {
            string fileName = String.Empty;

            for (int i = 0; i < length; i++)
            {
                if (data[offset + i] == 0x0)
                    break;

                fileName += (char)data[offset + 1];
            }

            return fileName;
        }
    }

    /* Padding for Integer */
    public class PadInteger
    {
        /* Get the length of a number padded to a multiple. */
        public static int multipleLength(int length, int multiple)
        {
            return length + ((length % multiple == 0) ? 0 : multiple - (length % multiple));
        }
    }
}