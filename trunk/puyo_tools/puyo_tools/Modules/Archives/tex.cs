using System;

namespace puyo_tools
{
    public class TEX
    {
        /*
         * TEX files are archives that contain GIM, SVP, and SVR files.
        */

        /* Main Method */
        public TEX()
        {
        }

        /* Extract files from the TEX archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = BitConverter.ToUInt32(data, 0x4); // Number of Files

                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i]  = BitConverter.ToUInt32(data, 0x14 + (i * 0x20)); // Start Offset
                    fileLength[i] = BitConverter.ToUInt32(data, 0x18 + (i * 0x20)); // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = new string[files];
                //if (returnFileNames)
                    fileNames = getFileNames(data, files);


                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create TEX archive. */
        public byte[] create(byte[][] data, string[] fileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int fileSize = Header.TEX.Length + 0xC + (data.Length * 0x20); // Filesize of Header

                /* Get the size for the files that will be added in the TEX archive. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Set file offset and length. */
                    fileStart[i]  = (uint)fileSize;
                    fileLength[i] = (uint)data[i].Length;

                    fileSize += PadInteger.multipleLength(data[i].Length, 2);
                }

                /* Now that we have the filesize, start writing the data. */
                byte[] archiveData = new byte[fileSize];

                /* Set up the header */
                Array.Copy(Header.TEX, 0, archiveData, 0, Header.TEX.Length); // TEX Header
                Array.Copy(BitConverter.GetBytes((uint)data.Length), 0, archiveData, 0x4, 0x4); // Number of Files

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file extension. */
                    byte[] fileExt = PadString.fileNameToBytes(System.IO.Path.GetExtension(fileNames[i]), 0x5);
                    if (fileExt.Length > 0)
                        Array.Copy(fileExt, 1, archiveData, Header.TEX.Length + 0xC + (i * 0x20), fileExt.Length - 1);

                    /* Add the file size & length. */
                    Array.Copy(BitConverter.GetBytes(fileStart[i]),  0, archiveData, Header.TEX.Length + 0x10 + (i * 0x20), 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(fileLength[i]), 0, archiveData, Header.TEX.Length + 0x14 + (i * 0x20), 0x4); // File Length

                    /* Add the filename. */
                    byte[] fileName = PadString.fileNameToBytes(System.IO.Path.GetFileNameWithoutExtension(fileNames[i]), 0x14);
                    if (fileName.Length > 0)
                        Array.Copy(fileName, 0, archiveData, Header.TEX.Length + 0x18 + (i * 0x20), fileName.Length);

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

        /* Attempt to find filenames in the TEX archive. */
        private string[] getFileNames(byte[] data, uint files)
        {
            string[] fileNames = new string[files]; // Set up an array of filenames.
            int start          = 0x10;              // Start of file name data.

            for (int i = 0; i < files; i++)
            {
                /* Set the start position */
                int pos = start + (i * 0x20);

                /* Get the file extension. */
                string fileExt = String.Empty;

                for (int j = 0; j < 0x4; j++)
                {
                    if (data[pos + j] == 0x0)
                        break;
                    else
                        fileExt += (char)data[pos + j];
                }

                /* Now jump to the filename. */
                pos += 0xC;

                /* Get the file name. */
                for (int j = 0; j < 0x14; j++)
                {
                    if (data[pos + j] == 0x0)
                        break;
                    else
                        fileNames[i] += (char)data[pos + j];
                }

                /* Add the file extension */
                fileNames[i] += "." + fileExt;
            }

            return fileNames;
        }

    }
}