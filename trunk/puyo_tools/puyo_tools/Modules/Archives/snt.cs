using System;
using System.IO;

namespace puyo_tools
{
    public class SNT : ArchiveClass
    {
        /*
         * SNT files are archives that contains SVR or GIM files.
         * No filenames are stored in the SNT file.
        */

        /* Main Method */
        public SNT()
        {
        }

        /* Extract files from the SNT archive */
        public object[][] extract(byte[] data, bool returnFileNames)
        {
            try
            {
                uint files = BitConverter.ToUInt32(data, 0x30); // Number of Files

                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[files];
                uint[] fileLength = new uint[files];

                /* Start of the header data. */
                uint headerStart = BitConverter.ToUInt32(data, 0x34)
                                 + BitConverter.ToUInt32(data, 0x38) + 0x4;

                /* Obtain the data for each file */
                byte[][] returnData = new byte[files][];

                for (int i = 0; i < files; i++)
                {
                    /* Get the file offset & length. */
                    fileStart[i] =  BitConverter.ToUInt32(data, (int)(headerStart + (i * 0x8) + 0x4)) + 0x20; // Start Offset
                    fileLength[i] = BitConverter.ToUInt32(data, (int)(headerStart + (i * 0x8)));              // File Length

                    returnData[i] = new byte[fileLength[i]];
                    Array.Copy(data, fileStart[i], returnData[i], 0, fileLength[i]);
                }

                /* Attempt to file filenames for all of the files */
                string[] fileNames = getFileNames(data, files, headerStart, fileStart, fileLength);


                /* Return all of the data now */
                return new object[][] { returnData, fileNames };
            }
            catch (Exception)
            {
                return new object[0][];
            }
        }

        /* Create SNT archive. */
        public byte[] create(byte[][] data, string[] fileNames, bool addFileNames, bool pspVer)
        {
            try
            {
                /* Obtain a list of file offsets and lengths. */
                uint[] fileStart  = new uint[data.Length];
                uint[] fileLength = new uint[data.Length];

                /* Set initial data. */
                int headerFileSize = Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (data.Length * 0x8); // Filesize of Header
                int fileSize       = headerFileSize;

                /* Get the size for the files that will be added in the AFS archive. */
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
                if (pspVer) // PSP SNT Files
                {
                    Array.Copy(Header.SNT_PSP,     0, archiveData, 0x0,  Header.SNT_PSP.Length);     // SNT Header
                    Array.Copy(Header.SNT_SUB_PSP, 0, archiveData, 0x20, Header.SNT_SUB_PSP.Length); // Sub SNT Header
                }
                else // PS2 SNT Files
                {
                    Array.Copy(Header.SNT_PS2,     0, archiveData, 0x0,  Header.SNT_PS2.Length);     // SNT Header
                    Array.Copy(Header.SNT_SUB_PS2, 0, archiveData, 0x20, Header.SNT_SUB_PS2.Length); // Sub SNT Header
                }

                Array.Copy(BitConverter.GetBytes((uint)(fileSize - footerFileSize - 0x20)),       0, archiveData, 0x10, 0x4); // Data between NUTL (NSTL) and NOF0 (+ 4 bytes)
                Array.Copy(BitConverter.GetBytes((uint)(fileSize - footerFileSize)),              0, archiveData, 0x14, 0x4); // Data from start of archive to NOF0 (+ 4 bytes)
                Array.Copy(BitConverter.GetBytes((uint)((data.Length * 4) + 24)),                 0, archiveData, 0x18, 0x4); // (Number of Files * 4) + 24
                Array.Copy(BitConverter.GetBytes((uint)(fileSize - footerFileSize - 0x28)),       0, archiveData, 0x24, 0x4); // Data from 0x28 to NOF0
                Array.Copy(BitConverter.GetBytes((uint)data.Length),                              0, archiveData, 0x30, 0x4); // Number of Files
                Array.Copy(BitConverter.GetBytes((uint)(headerFileSize - footerFileSize - 0x20)), 0, archiveData, 0x38, 0x4); // Header Size - Footer Size - 0x20

                archiveData[0x4] = 0x18;
                archiveData[0x8] = 0x1;
                archiveData[0xC] = 0x20;

                archiveData[0x1C] = 0x1;

                archiveData[0x28] = 0x10;

                archiveData[0x34] = 0x1C;

                archiveData[0x3C] = 0x1;
                archiveData[0x44] = 0x1;
                archiveData[0x46] = 0x1;

                for (int i = 0; i < data.Length - 1; i++)
                {
                    /* Add some things. */
                    Array.Copy(BitConverter.GetBytes((uint)i), 0, archiveData, Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + (i * 0x14), 0x4); // i as a 4 byte array
                    archiveData[Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + (i * 0x14) + 0x8]  = 0x1;
                    archiveData[Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + (i * 0x14) + 0x10] = 0x1;
                    archiveData[Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + (i * 0x14) + 0x12] = 0x1;
                }

                Array.Copy(BitConverter.GetBytes((uint)(data.Length - 1)), 0, archiveData, Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + ((data.Length - 1) * 0x14), 0x4); // i as a 4 byte array

                /* Add the footer. */
                byte[] footer     = { 0x4E, 0x4F, 0x46, 0x30 };
                byte[] footer_sub = { 0x4E, 0x45, 0x4E, 0x44 };

                Array.Copy(footer, 0, archiveData, footerStart, footer.Length); // Start of footer
                Array.Copy(BitConverter.GetBytes((uint)((data.Length * 4) + (data.Length % 2 == 0 ? 16 : 28))), 0, archiveData, footerStart + 0x4, 0x4); // Files * 4 + (16 if files is even, 28 if files is odd)
                Array.Copy(BitConverter.GetBytes((uint)data.Length + 2), 0, archiveData, footerStart + 0x8, 0x4); // Number of files + 2

                archiveData[footerStart + 0x10] = 0x14;

                for (int i = 0; i < data.Length; i++)
                    Array.Copy(BitConverter.GetBytes((data.Length * 20) + 0x20 + (i * 8)), 0, archiveData, footerStart + 0x14 + (i * 0x4), 0x4); // (Files in archive + 20) + 32 + (i * 8)

                archiveData[footerStart + 0x4 + 0x10 + (data.Length * 0x4)] = 0x18;

                /* End of the footer. */
                Array.Copy(footer_sub, 0, archiveData, footerStart + footerFileSize - 0x10, 0x4);

                /* Add the file data and the header. */
                for (int i = 0; i < data.Length; i++)
                {
                    /* Add the file size & length. */
                    uint headerFileStart = fileStart[i];
                    if (addFileNames)
                        headerFileStart += (uint)PadInteger.multipleLength(fileNames[i].Length, 4);

                    Array.Copy(BitConverter.GetBytes(headerFileStart - 0x20), 0, archiveData, Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (i * 0x8) + 0x4, 0x4); // Start Offset
                    Array.Copy(BitConverter.GetBytes(fileLength[i]),          0, archiveData, Header.SNT_PS2.Length + 0x1C + Header.SNT_SUB_PS2.Length + 0x24 + ((data.Length - 1) * 0x14) + 0x8 + (i * 0x8), 0x4);       // File Length

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

        /* Attempt to find filenames in the SNT archive. */
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
                    {
                        /* Is there a null byte? */
                        if (data[j] == 0x0)
                            break;

                        fileNames[i] += (char)data[j];
                    }
                }
                else
                {
                    /* PSP GIM files may contain a filename at the end of the file. */
                    if (Header.isFile(data, Header.GIM, (int)fileStart[i]) ||
                        Header.isFile(data, Header.MIG, (int)fileStart[i]))
                    {
                        for (int j = 0; j < 0x100; j++)
                        {
                            /* Stop if we are out of bounds. */
                            if (j + Header.TGA.Length >= fileLength[i])
                                break;

                            /* We found a sign of a filename? */
                            if (Header.isFile(data, Header.TGA, (int)(fileStart[i] + fileLength[i] - j)))
                            {
                                j++;

                                while (fileLength[i] - j >= 0 && data[fileStart[i] + fileLength[i] - j] != 0x0)
                                {
                                    fileNames[i] = (char)(data[fileStart[i] + fileLength[i] - j]) + fileNames[i];
                                    j++;
                                }
                                fileNames[i] += ".gim";
                                break;
                            }
                        }
                    }
                }

                expectedStart = fileStart[i] + fileLength[i]; // New initial start.
            }

            return fileNames;
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = ObjectConverter.StreamToUInt(data, 0x30);

                /* This is where the header should start */
                uint headerStart = ObjectConverter.StreamToUInt(data, 0x34) + ObjectConverter.StreamToUInt(data, 0x38) + 0x4;

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Use this to find filenames */
                uint expectedStart = NumberData.RoundUpToMultiple(headerStart + (files * 0x8), 4);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the offset & length */
                    uint offset = ObjectConverter.StreamToUInt(data, 0x4 + (i * 0x8) + headerStart) + 0x20;
                    uint length = ObjectConverter.StreamToUInt(data, 0x0 + (i * 0x8) + headerStart);

                    /* Check for filenames, if the offset of the file is bigger we expected it to be */
                    string filename = String.Empty;
                    if (offset > expectedStart)
                        filename = ObjectConverter.StreamToString(data, expectedStart, (int)(offset - expectedStart));

                    /* For PP15 PSP, we can check the GIM file for filenames */
                    else if (length > 40 && ObjectConverter.StreamToString(data, offset, 8) == FileHeader.MIG)
                    {
                        uint metadataOffset = ObjectConverter.StreamToUInt(data, offset + 0x24) + 0x30;
                        if (metadataOffset < offset + length)
                            filename = Path.GetFileNameWithoutExtension(ObjectConverter.StreamToString(data, offset + metadataOffset, (int)(length - metadataOffset))) + ".gim";
                    }

                    /* Now update the expected start. */
                    expectedStart = NumberData.RoundUpToMultiple(offset + length, 4);

                    fileInfo[i] = new object[] {
                        offset,  // Offset
                        length,  // Length
                        filename // Filename
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
            return null;
        }
    }
}