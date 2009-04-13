using System;
using System.IO;

namespace puyo_tools
{
    public class SPK : ArchiveClass
    {
        /*
         * SPK files are archives that contain LST, HD, and BD files.
        */

        /* Main Method */
        public SPK()
        {
        }

        /* Extract files from the SPK archive */
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
                string[] fileNames = getFileNames(data, files);


                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create SPK archive. */
        public byte[] create(byte[][] data, string[] fileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int fileSize = Header.SPK.Length + 0xC + (data.Length * 0x20); // Filesize of Header

                /* Get the size for the files that will be added in the SPK archive. */
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
                Array.Copy(Header.SPK, 0, archiveData, 0, Header.SPK.Length); // SPK Header
                Array.Copy(BitConverter.GetBytes((uint)data.Length), 0, archiveData, 0x4, 0x4); // Number of Files

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file extension. */
                    byte[] fileExt = PadString.fileNameToBytes(System.IO.Path.GetExtension(fileNames[i]), 0x5);
                    if (fileExt.Length > 0)
                        Array.Copy(fileExt, 1, archiveData, Header.SPK.Length + 0xC + (i * 0x20), fileExt.Length - 1);

                    /* Add the file size & length. */
                    Array.Copy(BitConverter.GetBytes(fileStart[i]),  0, archiveData, Header.SPK.Length + 0x10 + (i * 0x20), 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(fileLength[i]), 0, archiveData, Header.SPK.Length + 0x14 + (i * 0x20), 0x4); // File Length

                    /* Add the filename. */
                    byte[] fileName = PadString.fileNameToBytes(System.IO.Path.GetFileNameWithoutExtension(fileNames[i]), 0x14);
                    if (fileName.Length > 0)
                        Array.Copy(fileName, 0, archiveData, Header.SPK.Length + 0x18 + (i * 0x20), fileName.Length);

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

        /* Attempt to find filenames in the SPK archive. */
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

                    fileExt += (char)data[pos + j];
                }

                /* Now jump to the filename. */
                pos += 0xC;

                /* Get the file name. */
                for (int j = 0; j < 0x14; j++)
                {
                    if (data[pos + j] == 0x0)
                        break;

                    fileNames[i] += (char)data[pos + j];
                }

                /* Add the file extension */
                fileNames[i] += "." + fileExt;
            }

            return fileNames;
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = ObjectConverter.StreamToUInt(data, 0x4);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get filename and extension */
                    string filename = ObjectConverter.StreamToString(data, 0x1C + (i * 0x20), 20); // Name
                    string fileext  = ObjectConverter.StreamToString(data, 0x10 + (i * 0x20), 4);  // Extension

                    fileInfo[i] = new object[] {
                        ObjectConverter.StreamToUInt(data, 0x14 + (i * 0x20)), // Offset
                        ObjectConverter.StreamToUInt(data, 0x18 + (i * 0x20)), // Length
                        filename + "." + fileext // Filename
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
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x20) + 0x10, 16)];

                /* Write out the identifier and number of files */
                Array.Copy(ObjectConverter.StringToBytes(FileHeader.SPK, 4), 0, header, 0x0, 4);
                Array.Copy(BitConverter.GetBytes(files.Length),              0, header, 0x4, 4); // Files

                /* Set the offset */
                uint offset = (uint)header.Length;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)(new FileInfo(files[i]).Length);

                    /* Write the filename & file extension */
                    Array.Copy(ObjectConverter.StringToBytes(Path.GetExtension(storedFilenames[i]).Substring(1), 3),    0, header, 0x10 + (i * 0x20), 3);
                    Array.Copy(ObjectConverter.StringToBytes(Path.GetFileNameWithoutExtension(storedFilenames[i]), 19), 0, header, 0x1C + (i * 0x20), 19);

                    /* Write the offsets and lengths */
                    Array.Copy(BitConverter.GetBytes(offset), 0, header, 0x14 + (i * 0x20), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(length), 0, header, 0x18 + (i * 0x20), 4); // Length

                    /* Now increment the offset */
                    offset += NumberData.RoundUpToMultiple(length, 16);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }
    }
}