using System;

namespace puyo_tools
{
    public class AFS
    {
        /*
         * AFS files are archives that contains files.
         * File names can be up to 44 characters in length.
        */

        /* Main Method */
        public AFS()
        {
        }

        /* Extract files from the AFS archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = BitConverter.ToUInt32(data, 0x4); // Number of Files

                /* Obtain a list of file offsets, lengths, and names. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i]  = BitConverter.ToUInt32(data, 0x8 + (i * 0x8)); // Start Offset
                    fileLength[i] = BitConverter.ToUInt32(data, 0xC + (i * 0x8)); // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = getFileNames(data, files);

                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create AFS archive. */
        public byte[] create(byte[][] data, string[] fileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int fileSize = Header.AFS.Length + 0x4 + (data.Length * 0x8) + 0x8; // Filesize of Header

                /* Get the size for the files that will be added in the AFS archive. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Set file offset and length. */
                    fileStart[i]  = (uint)fileSize;
                    fileLength[i] = (uint)data[i].Length;

                    fileSize += PadInteger.multipleLength(data[i].Length, 2);
                }

                /* Add the filename footer. */
                uint fileNameStart  = (uint)fileSize;
                uint fileNameLength = (uint)(data.Length * 0x30);
                fileSize += (int)fileNameLength;

                /* Now that we have the filesize, start writing the data. */
                byte[] archiveData = new byte[fileSize];

                /* Set up the header */
                Array.Copy(Header.AFS, 0, archiveData, 0, Header.AFS.Length); // AFS Header
                Array.Copy(BitConverter.GetBytes((uint)data.Length), 0, archiveData, 0x4, 0x4); // Number of Files

                Array.Copy(BitConverter.GetBytes(fileNameStart),  0, archiveData, Header.AFS.Length + 0x4 + (data.Length * 0x8), 0x4); // Start of filenames
                Array.Copy(BitConverter.GetBytes(fileNameLength), 0, archiveData, Header.AFS.Length + 0x8 + (data.Length * 0x8), 0x4); // Size of filenames

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file size & length. */
                    Array.Copy(BitConverter.GetBytes(fileStart[i]),  0, archiveData, Header.AFS.Length + 0x4 + (i * 0x8), 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(fileLength[i]), 0, archiveData, Header.AFS.Length + 0x8 + (i * 0x8), 0x4); // File Length

                    /* Add the filename. */
                    byte[] fileName = PadString.fileNameToBytes(fileNames[i], 0x20);
                    Array.Copy(fileName, 0, archiveData, fileNameStart + (i * 0x30), fileName.Length);

                    /* Add random thing from header. I don't know its purpose. */
                    Array.Copy(archiveData, Header.AFS.Length + (i * 0x4), archiveData, fileNameStart + (i * 0x30) + 0x2C, 0x4);

                    /* Add the data now. */
                    Array.Copy(data[i], 0, archiveData, fileStart[i], fileLength[i]);
                }

                return archiveData;
            }
            catch
            {
                return new byte[0];
            }
        }

        /* Attempt to find filenames in the AFS archive. */
        private string[] getFileNames(byte[] data, uint files)
        {
            string[] fileNames    = new string[files];                                       // Set up an array of filenames.
            uint fileNameLocation = BitConverter.ToUInt32(data, (int)(0x8 + (files * 0x8))); // Start of file name data.

            if (fileNameLocation == 0x0) // This is AFS v1
                fileNameLocation = BitConverter.ToUInt32(data, BitConverter.ToInt32(data, Header.AFS.Length + 0x4) - 0x8); // Start of file name data.

            for (int i = 0; i < files; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (data[fileNameLocation + (i * 0x30) + j] == 0x0)
                        break;

                    fileNames[i] += (char)data[fileNameLocation + (i * 0x30) + j];
                }
            }

            return fileNames;
        }

    }
}