using System;

namespace pp_tools
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
                if (returnFileNames)
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
        public byte[] create(object[][][] data, bool addFileNames)
        {
            try
            {
                int fileSize = Header.ACX.Length + (data[0].Length * 0x8);
                /* Get the size of the ACX archive. */
                for (int i = 0; i < data[0].Length; i++)
                    fileSize += (data[0][i].Length + data[1][i].Length);

                /* Now that we have the filesize, create the data. */
                byte[] returnData = new byte[fileSize];

                /* Set up the header */
                int fileStart = Header.ACX.Length + (data[0].Length * 0x8);
                Array.Copy(Header.ACX, 0, returnData, 0, Header.ACX.Length);

                /* Add the file data and the header. */
                for (int i = 0; i < data[0].Length; i++)
                {
                    /* Add the file name. */
                    //if (addFileNames)
                    /* Add the filename to the file */
                    //dataStart += data[1][i].Length;
                }
                    //returnData[Header.ACX.Length + (i * 

                return returnData;
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