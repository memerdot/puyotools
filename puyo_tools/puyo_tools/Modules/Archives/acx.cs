using System;

namespace puyo_tools
{
    public class ACX
    {
        /*
         * ACX files are archives that contains ADX files.
         * No filenames are stored in the ACX file.
        */

        /* Main Method */
        public ACX()
        {
        }

        /* Extract files from the ACX archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = Endian.swapInt(BitConverter.ToUInt32(data, 0x4)); // Number of Files

                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i]  = Endian.swapInt(BitConverter.ToUInt32(data, 0x8 + (i * 0x8))); // Start Offset
                    fileLength[i] = Endian.swapInt(BitConverter.ToUInt32(data, 0xC + (i * 0x8))); // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = new string[files];
                //if (returnFileNames)
                    fileNames = getFileNames(data, files, fileStart, fileLength);


                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create ACX archive. */
        public byte[] create(byte[][] data, string[] fileNames, bool addFileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int fileSize = Header.ACX.Length + 0x4 + (data.Length * 0x8); // Filesize of Header

                /* Get the size for the files that will be added in the ACX archive. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Set file offset and length. */
                    fileStart[i]  = (uint)fileSize;
                    fileLength[i] = (uint)data[i].Length;

                    if (addFileNames)
                        fileSize += PadInteger.multipleLength(fileNames[i].Length, 4);

                    fileSize += PadInteger.multipleLength(data[i].Length, 4);
                }

                /* Now that we have the filesize, start writing the data. */
                byte[] archiveData = new byte[fileSize];

                /* Set up the header */
                Array.Copy(Header.ACX, 0, archiveData, 0, Header.ACX.Length); // ACX Header
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)data.Length)), 0, archiveData, 0x4, 0x4); // Number of Files

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file size & length. */
                    uint headerFileStart = fileStart[i];
                    if (addFileNames)
                        headerFileStart += (uint)PadInteger.multipleLength(fileNames[i].Length, 4);

                    Array.Copy(BitConverter.GetBytes(Endian.swapInt(headerFileStart)), 0, archiveData, Header.ACX.Length + 0x4 + (i * 0x8), 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(Endian.swapInt(fileLength[i])),   0, archiveData, Header.ACX.Length + 0x8 + (i * 0x8), 0x4); // File Length

                    /* Add the filename. */
                    if (addFileNames)
                    {
                        byte[] fileName = PadString.multipleToBytes(fileNames[i], 4);
                        Array.Copy(fileName, 0, archiveData, fileStart[i], fileName.Length);
                    }

                    /* Add the data now. */
                    Array.Copy(data[i], 0, archiveData, headerFileStart, fileLength[i]);
                }

                return archiveData;
            }
            catch
            {
                return new byte[0];
            }
        }

        /* Attempt to find filenames in the ACX archive. */
        private string[] getFileNames(byte[] data, uint files, uint[] fileStart, uint[] fileLength)
        {
            string[] fileNames = new string[files];   // Set up an array of filenames.
            uint expectedStart = 0x8 + (files * 0x8); // Set the initial expected start.

            for (int i = 0; i < files; i++)
            {
                /* Is the expected start before the actual file start? */
                if (expectedStart < fileStart[i])
                {
                    for (uint j = expectedStart; j < fileStart[i]; j++)
                        fileNames[i] += (char)data[j];
                }

                expectedStart = fileStart[i] + fileLength[i]; // New initial start.
                expectedStart += expectedStart % 4;
            }

            return fileNames;
        }

    }
}