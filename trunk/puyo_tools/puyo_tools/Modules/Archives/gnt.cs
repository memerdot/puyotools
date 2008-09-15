using System;

namespace puyo_tools
{
    public class GNT
    {
        /*
         * GNT files are archives that contains GVR files.
         * No filenames are stored in the SNT file.
        */

        /* Main Method */
        public GNT()
        {
        }

        /* Extract files from the GNT archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = Endian.swapInt(BitConverter.ToUInt32(data, 0x30)); // Number of Files

                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];

                /* Start of the header data. */
                uint headerStart = Endian.swapInt(BitConverter.ToUInt32(data, 0x34))
                                 + Endian.swapInt(BitConverter.ToUInt32(data, 0x38)) + 0x4;

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i]  = Endian.swapInt(BitConverter.ToUInt32(data, (int)(headerStart + (i * 0x8) + 0x4))) + 0x20; // Start Offset
                    fileLength[i] = Endian.swapInt(BitConverter.ToUInt32(data, (int)(headerStart + (i * 0x8))));              // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = new string[files];
                if (returnFileNames)
                    fileNames = getFileNames(data, files, headerStart, fileStart, fileLength);


                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create GNT archive. */
        public byte[] create(byte[][] data, string[] fileNames, bool addFileNames)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int headerFileSize = Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (data.Length * 0x8); // Filesize of Header
                int fileSize = headerFileSize;

                /* Get the size for the files that will be added in the GNT archive. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Set file offset and length. */
                    fileStart[i]  = (uint)fileSize;
                    fileLength[i] = (uint)data[i].Length;

                    if (addFileNames)
                        fileSize += PadInteger.multipleLength(fileNames[i].Length, 2);

                    fileSize += PadInteger.multipleLength(data[i].Length, 2);
                }

                fileSize = PadInteger.multipleLength(fileSize, 16);

                /* Size of the footer. */
                int footerStart    = fileSize;
                int footerFileSize = PadInteger.multipleLength(0x4 + 0x10 + (data.Length * 0x4) + 0x4, 16) + 0x10;

                fileSize += footerFileSize;

                /* Now that we have the filesize, start writing the data. */
                byte[] archiveData = new byte[fileSize];

                /* Set up the header */
                Array.Copy(Header.GNT,     0, archiveData, 0x0,  Header.GNT.Length);     // GNT Header
                Array.Copy(Header.GNT_SUB, 0, archiveData, 0x20, Header.GNT_SUB.Length); // Sub GNT Header

                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)(fileSize - footerFileSize - 0x20))),       0, archiveData, 0x10, 0x4); // Data between NGTL and NOF0 (+ 4 bytes)
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)(fileSize - footerFileSize))),              0, archiveData, 0x14, 0x4); // Data from start of archive to NOF0 (+ 4 bytes)
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)((data.Length * 4) + 24))),                 0, archiveData, 0x18, 0x4); // (Number of Files * 4) + 24
                Array.Copy(BitConverter.GetBytes((uint)(fileSize - footerFileSize - 0x28)),                       0, archiveData, 0x24, 0x4); // Data from 0x28 to NOF0
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)data.Length)),                              0, archiveData, 0x30, 0x4); // Number of Files
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)(headerFileSize - footerFileSize - 0x20))), 0, archiveData, 0x38, 0x4); // Header Size - Footer Size - 0x20

                archiveData[0x4] = 0x18;
                archiveData[0xB] = 0x1;
                archiveData[0xF] = 0x20;

                archiveData[0x1F] = 0x1;

                archiveData[0x2B] = 0x10;

                archiveData[0x37] = 0x1C;

                archiveData[0x45] = 0x1;
                archiveData[0x47] = 0x1;

                for (int i = 0; i < data.Length - 1; i++)
                {
                    /* Add some things. */
                    Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)i)), 0, archiveData, Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + (i * 0x14), 0x4); // i as a 4 byte array
                    archiveData[Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + (i * 0x14) + 0x11] = 0x1;
                    archiveData[Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + (i * 0x14) + 0x13] = 0x1;
                }

                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)(data.Length - 1))), 0, archiveData, Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + ((data.Length - 1) * 0x14), 0x4); // i as a 4 byte array

                /* Add the footer. */
                byte[] footer     = { 0x4E, 0x4F, 0x46, 0x30 };
                byte[] footer_sub = { 0x4E, 0x45, 0x4E, 0x44 };

                Array.Copy(footer, 0, archiveData, footerStart, footer.Length); // Start of footer
                Array.Copy(BitConverter.GetBytes((uint)((data.Length * 4) + (data.Length % 2 == 0 ? 16 : 28))), 0, archiveData, footerStart + 0x4, 0x4); // Files * 4 + (16 if files is even, 28 if files is odd)
                Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)data.Length + 2)), 0, archiveData, footerStart + 0x8, 0x4); // Number of files + 2

                archiveData[footerStart + 0x13] = 0x14;

                for (int i = 0; i < data.Length; i++)
                    Array.Copy(BitConverter.GetBytes(Endian.swapInt((uint)((data.Length * 20) + 0x20 + (i * 8)))), 0, archiveData, footerStart + 0x14 + (i * 0x4), 0x4); // (Files in archive + 20) + 32 + (i * 8)

                archiveData[footerStart + 0x4 + 0x10 + (data.Length * 0x4) + 0x3] = 0x18;

                /* End of the footer. */
                Array.Copy(footer_sub, 0, archiveData, footerStart + footerFileSize - 0x10, 0x4);

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file size & length. */
                    uint headerFileStart = fileStart[i];
                    if (addFileNames)
                        headerFileStart += (uint)PadInteger.multipleLength(fileNames[i].Length, 4);

                    Array.Copy(BitConverter.GetBytes(Endian.swapInt(headerFileStart - 0x20)), 0, archiveData, Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (i * 0x8) + 0x4, 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(Endian.swapInt(fileLength[i])),          0, archiveData, Header.GNT.Length + 0x1C + Header.GNT_SUB.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (i * 0x8), 0x4);       // File Length

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

        /* Attempt to find filenames in the GNT archive. */
        private string[] getFileNames(byte[] data, uint files, uint headerStart, uint[] fileStart, uint[] fileLength)
        {
            string[] fileNames = new string[files];           // Set up an array of filenames.
            uint expectedStart = headerStart + (files * 0x8); // Set the initial expected start.

            for (int i = 0; i < files; i++)
            {
                /* Is the expected start before the actual file start? */
                if (expectedStart < fileStart[i])
                {
                    for (uint j = expectedStart; j < fileStart[i]; j++)
                        fileNames[i] += (char)data[j];
                }

                expectedStart = fileStart[i] + fileLength[i]; // New initial start.
            }

            return fileNames;
        }

    }
}