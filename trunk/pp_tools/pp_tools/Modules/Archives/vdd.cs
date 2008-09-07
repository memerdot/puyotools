using System;

namespace pp_tools
{
    public class VDD
    {
        /*
         * VDD files are archives that contains files.
         * File names can be up to 16 characters in length.
        */

        /* Main Method */
        public VDD()
        {
        }

        /* Extract files from the VDD archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = BitConverter.ToUInt32(data, 0x0); // Number of Files

                /* Obtain a list of file offsets, lengths, and names. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];
                string[] fileName = new string[files];

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i]  = BitConverter.ToUInt32(data, 0x4 + (i * 0x18) + 0x10) * 0x800; // Start Offset
                    fileLength[i] = BitConverter.ToUInt32(data, 0x4 + (i * 0x18) + 0x14);         // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = new string[files];
                if (returnFileNames)
                    fileNames = getFileNames(data, files);

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

        /* Attempt to find filenames in the VDD archive. */
        private string[] getFileNames(byte[] data, uint files)
        {
            string[] fileNames = new string[files]; // Set up an array of filenames.

            for (int i = 0; i < files; i++)
            {
                /* Get the file name */
                for (int j = 0; j < 16; j++)
                {
                    if (data[0x4 + (i * 0x18) + j] == 0x0)
                        break;

                    fileNames[i] += (char)data[0x4 + (i * 0x18) + j];
                }
            }

            return fileNames;
        }

    }
}