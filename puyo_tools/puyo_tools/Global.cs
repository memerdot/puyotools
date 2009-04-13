using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

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
            ONE = { 0x6F, 0x6E, 0x65, 0x2E }, // ONE
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
            ONE = "ONE Extracted Files", // ONE Archive
            SNT = "SNT Extracted Files", // SNT Archive
            SPK = "SPK Extracted Files", // SPK Archive
            TEX = "TEX Extracted Files", // TEX Archive
            VDD = "VDD Extracted Files"; // VDD Archive


        /* Decompression */
        public static string
            CNX  = "CNX Decompressed Files",  // CNX Compression
            CXLZ = "CXLZ Decompressed Files", // CXLZ Compression
            LZ00 = "LZ00 Decompressed Files", // LZ00 Compression
            LZ01 = "LZ01 Decompressed Files", // LZ01 Compression
            ONZ  = "ONZ Decompressed Files";  // ONZ Compression
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

        /* Swap short endian. */
        public static ushort Swap(ushort x)
        {
            x = (ushort)((x >> 8) |
                (x << 8));

            return x;
        }

        /* Swap integer endian. */
        public static uint Swap(uint x)
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

        /* Select Directories */
        public static string SelectDirectory(string title)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.Description         = title;
            fbd.SelectedPath        = Directory.GetCurrentDirectory();
            fbd.ShowNewFolderButton = false;
            DialogResult result     = fbd.ShowDialog();

            return (result == DialogResult.OK ? fbd.SelectedPath : null);
        }

        /* Return a list of files from a directory and subdirectories */
        public static string[] FindFilesInDirectory(string initialDirectory, bool searchSubDirectories)
        {
            /* Create the file list and sub directory list */
            List<string> fileList = new List<string>();
            List<string> subDirectoryList = new List<string>();

            /* Add files from initial directory */
            foreach (string file in Directory.GetFiles(initialDirectory))
                fileList.Add(file);

            /* Search subdirectories? */
            if (searchSubDirectories)
            {
                /* Get a list of subdirectories */
                foreach (string directory in Directory.GetDirectories(initialDirectory))
                    subDirectoryList.Add(directory);

                /* Search the sub directories */
                for (int i = 0; i < subDirectoryList.Count; i++)
                {
                    /* Add files from the directory */
                    foreach (string file in Directory.GetFiles(subDirectoryList[i]))
                        fileList.Add(file);

                    /* Add the sub directories in this directory */
                    foreach (string directory in Directory.GetDirectories(subDirectoryList[i]))
                        subDirectoryList.Add(directory);
                }
            }

            return fileList.ToArray();
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

                fileName += (char)data[offset + i];
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

    /* Number Data */
    public class NumberData
    {
        /* Round a number up to a mutliple */
        public static uint RoundUpToMultiple(uint number, uint multiple)
        {
            /* If the number is already divisible by the multiple, do nothing */
            if (number % multiple == 0)
                return number;

            return number + (multiple - (number % multiple));
        }

        /* Get the number of digits in a number */
        public static int Digits(int number)
        {
            return number.ToString().Length;
        }

        /* Format filename */
        public static string FormatFilename(uint number, int total)
        {
            return number.ToString().PadLeft(NumberData.Digits(total), '0');
        }
    }

    /* String Data */
    public class StringData
    {
        /* Limit Length */
        public static string LimitLength(string str, int length)
        {
            /* If the string length is smaller than we need it to be, return it */
            if (str.Length <= length)
                return str;

            return str.Substring(0, length);
        }
    }

    /* Object Conversions */
    public class ObjectConverter
    {
        /* Return an array of bytes from a stream */
        public static byte[] StreamToBytes(System.IO.Stream stream, uint offset, int length)
        {
            byte[] temp = new byte[length];

            stream.Position = offset;
            stream.Read(temp, 0, length);

            return temp;
        }

        /* Convert a stream to another stream */
        public static Stream StreamToStream(Stream stream)
        {
            return new MemoryStream(ObjectConverter.StreamToBytes(stream, 0x0, (int)stream.Length));
        }

        public static Stream StreamToStream(Stream stream, uint offset, long length)
        {
            return new MemoryStream(ObjectConverter.StreamToBytes(stream, offset, (int)length));
        }

        /* Convert a stream to an unsigned integer */
        public static uint StreamToUInt(Stream stream, uint offset)
        {
            /* Convert to bytes and then to an unsigned integer */
            return BitConverter.ToUInt32(StreamToBytes(stream, offset, 4), 0);
        }

        /* Convert a stream to an unsigned short */
        public static ushort StreamToUShort(Stream stream, uint offset)
        {
            /* Convert to bytes and then to an unsigned integer */
            return BitConverter.ToUInt16(StreamToBytes(stream, offset, 2), 0);
        }

        /* Convert stream to string */
        public static string StreamToString(Stream stream, uint offset, int maxLength)
        {
            string str = String.Empty;
            char letter;

            stream.Position = offset;

            for (int i = 0; i < maxLength; i++)
            {
                letter = (char)stream.ReadByte();
                if (letter == 0x0)
                    break;
                else
                    str += letter;
            }

            return str;
        }

        /* Convert string to bytes */
        public static byte[] StringToBytes(string str, int maxLength)
        {
            /* Create the byte array */
            byte[] bytes = new byte[maxLength];

            for (int i = 0; i < maxLength && i < str.Length; i++)
                bytes[i] = (byte)str[i];

            return bytes;
        }
    }

    /* Pad Data */
    public class PadData
    {
        public static Stream FillStream(byte str, int length)
        {
            MemoryStream stream = new MemoryStream(length);

            for (int i = 0; i < length; i++)
                stream.WriteByte(str);

            return stream;
        }

        public static byte[] Fill(byte str, int length)
        {
            byte[] temp = new byte[length];

            for (int i = 0; i < length; i++)
                temp[i] = str;

            return temp;
        }
    }
}