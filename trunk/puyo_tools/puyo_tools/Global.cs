using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    /* Endian Swaps */
    public class Endian
    {
        /* Swap short endian. */
        public static ushort Swap(ushort x)
        {
            x = (ushort)((x >> 8) |
                (x << 8));

            return x;
        }
        public static ushort Swap(short x)
        {
            return Swap((ushort)x);
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
        public static uint Swap(int x)
        {
            return Swap((uint)x);
        }
    }

    /* File Selections */
    public class Files
    {
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

    /* File Data */
    public static class FileData
    {
        /* Get the file extension for the file */
        public static string GetFileExtension(ref Stream data)
        {
            /* Check based on archive format */
            Archive archive = new Archive(data, null);
            if (archive.Format != ArchiveFormat.NULL)
                return archive.FileExtension;

            /* Check based on image format */
            Images images = new Images(data, null);
            if (images.Format != GraphicFormat.NULL)
                return images.FileExtension;

            /* Special check for ADX files */
            if (data.Length > 4 && StreamConverter.ToUShort(data, 0x0) == 0x8000 &&
                data.Length > StreamConverter.ToUShort(data, 0x2) + 4 &&
                StreamConverter.ToString(data, StreamConverter.ToUShort(data, 0x2) - 2, 6) == "(c)CRI")
                return ".adx";

            /* Unknown extension */
            return String.Empty;
        }
        public static string GetFileExtension(ref MemoryStream data)
        {
            Stream stream = (Stream)data;
            return GetFileExtension(ref stream);
        }

        /* Get the file type for the file */
        public static string GetFileType(ref Stream data)
        {
            /* Check based on archive format */
            Archive archive = new Archive(data, null);
            if (archive.Format != ArchiveFormat.NULL)
                return archive.ArchiveName + " Archive";

            /* Check based on image format */
            Images images = new Images(data, null);
            if (images.Format != GraphicFormat.NULL)
                return images.ImageName + " Image";

            /* Special check for ADX files */
            if (data.Length > 4 && StreamConverter.ToUShort(data, 0x0) == 0x8000 &&
                data.Length > StreamConverter.ToUShort(data, 0x2) + 4 &&
                StreamConverter.ToString(data, StreamConverter.ToUShort(data, 0x2) - 2, 6) == "(c)CRI")
                return "ADX Audio";

            /* Unknown extension */
            return String.Empty;
        }
        public static string GetFileType(ref MemoryStream data)
        {
            Stream stream = (Stream)data;
            return GetFileType(ref stream);
        }
    }

    /* Number Data */
    public static class Number
    {
        /* Round number up to a multiple */
        public static uint RoundUp(uint number, int multiple)
        {
            /* Return the same number if it is already a multiple */
            if (number % multiple == 0)
                return number;

            return (uint)(number + (multiple - (number % multiple)));
        }
        public static int RoundUp(int number, int multiple)
        {
            /* Return the same number if it is already a multiple */
            if (number % multiple == 0)
                return number;

            return number + (multiple - (number % multiple));
        }

        /* Get the amount of digits in the number */
        public static int Digits(long number)
        {
            return number.ToString().Length;
        }
    }

}