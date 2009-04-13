using System;
using System.IO;

namespace puyo_tools
{
    public class VDD : ArchiveClass
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
                string[] fileNames = getFileNames(data, files);

                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create VDD archive. */
        public byte[] create(byte[][] data, string[] fileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int fileSize = PadInteger.multipleLength(0x4 + (data.Length * 0x18), 0x800); // Filesize of Header

                /* Get the size for the files that will be added in the VDD archive. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Set file offset and length. */
                    fileStart[i]  = (uint)fileSize;
                    fileLength[i] = (uint)data[i].Length;

                    fileSize += PadInteger.multipleLength(data[i].Length, 0x800);
                }

                /* Now that we have the filesize, start writing the data. */
                byte[] archiveData = new byte[fileSize];

                /* Set up the header */
                Array.Copy(BitConverter.GetBytes((uint)data.Length), 0, archiveData, 0x0, 0x4); // Number of Files

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file size & length. */
                    Array.Copy(BitConverter.GetBytes(fileStart[i] / 0x800),  0, archiveData, 0x4 + (i * 0x18) + 0x10, 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(fileLength[i]), 0, archiveData, 0x4 + (i * 0x18) + 0x14, 0x4); // File Length

                    /* Add the filename. */
                    byte[] fileName = PadString.fileNameToBytes(fileNames[i], 0x10);
                    Array.Copy(fileName, 0, archiveData, 0x4 + (i * 0x18), fileName.Length);

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

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = ObjectConverter.StreamToUInt(data, 0x0);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileInfo[i] = new object[] {
                        ObjectConverter.StreamToUInt(data,   0x14 + (i * 0x18)) * 0x800, // Offset
                        ObjectConverter.StreamToUInt(data,   0x18 + (i * 0x18)),         // Length
                        ObjectConverter.StreamToString(data, 0x04 + (i * 0x18), 16)      // Filename
                    };
                }

                return fileInfo;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new object[0][];
            }
        }

        /* Add a header to a blank archive */
        public override byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            try
            {
                /* Create the header data. */
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x18) + 0x4, 2048)];

                /* Write out the number of files. */
                Array.Copy(BitConverter.GetBytes(files.Length), 0, header, 0x0, 4); // Files

                /* Set the offset */
                uint offset = (uint)header.Length;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)(new FileInfo(files[i]).Length);

                    /* Write the filename */
                    Array.Copy(ObjectConverter.StringToBytes(Path.GetFileName(storedFilenames[i]), 15), 0, header, 0x4 + (i * 0x18), 15);

                    /* Write the offsets and lengths */
                    Array.Copy(BitConverter.GetBytes(offset / 0x800), 0, header, 0x14 + (i * 0x18), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(length),         0, header, 0x18 + (i * 0x18), 4); // Length

                    /* Now increment the offset */
                    offset += NumberData.RoundUpToMultiple(length, 2048);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }

        /* Get offset for the file */
        public uint getOffset(byte[] header, uint file)
        {
            try
            {
                /* Return the offset that we can add the file to. */
                return BitConverter.ToUInt32(header, (int)(0x14 + (file * 0x18)));
            }
            catch
            {
                /* Something went wrong, so return the offset 0x0 */
                return 0x0;
            }
        }

    }
}