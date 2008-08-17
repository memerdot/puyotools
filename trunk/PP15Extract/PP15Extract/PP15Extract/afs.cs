using System;
using System.IO;
using System.Globalization;

namespace pp15_fileTools
{
    public class AFS
    {
        /* File Data */
        private int[][] extractOffsets;   // Data that has been extracted.
        public string[] extractFileNames; // Extracted file names.
        private int fileNameLocation;     // Location where the file names are stored.
        private int files;                // Number of files in the SNT file.
        private int fileStart;            // Start offset of extracted file.
        private int fileLength;           // Length of extracted file.

        public AFS()
        {
        }

        /* Extract AFS Archive */
        public int[][] extract(byte[] data)
        {
            try
            {
                files            = BitConverter.ToInt32(data, 0x4);
                fileNameLocation = BitConverter.ToInt32(data, 0x8 + (files * 0x8));
                extractOffsets   = new int[files][];
                extractFileNames = new string[files];

                /* Extract the data now */
                for (int i = 0; i < files; i++)
                {
                    try
                    {
                        fileStart  = BitConverter.ToInt32(data, 0x8 + (i * 0x8));
                        fileLength = BitConverter.ToInt32(data, 0x8 + 0x4 + (i * 0x8));

                        extractOffsets[i]   = new int[] { fileStart, fileLength };
                        extractFileNames[i] = getFileName(data, fileNameLocation, i);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                return new int[0][];
            }

            return extractOffsets;
        }

        /* Create AFS archive */
        public byte[] create(string[] addFileNames)
        {
            //byte[] data   = new byte[0x8 + 0x10 + (fileNames.Length * 0x8) + (fileNames.Length * 0x30)];
            int offset = 0x8 + 0x10 + (addFileNames.Length * 0x8);
            int fileLength = offset;
            byte[][] fileData = new byte[addFileNames.Length][];
            string[] fileNames = new string[addFileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                try
                {
                    /* Read all of the files first */
                    FileStream file = new FileStream(addFileNames[i], FileMode.Open);
                    fileData[i] = new byte[file.Length];
                    file.Read(fileData[i], 0, (int)file.Length);
                    fileLength += fileData[i].Length;
                    fileNames[i] = Path.GetFileName(addFileNames[i]);
                }
                catch (Exception)
                {
                    return new byte[0];
                }
            }

            /* Now let's build the AFS file */
            byte[] data = new byte[fileLength + (fileData.Length * 0x30)];
            int fileNameOffset = fileLength;

            /* Write the Header */
            Array.Copy(Header.AFS, 0, data, 0, Header.AFS.Length);
            Array.Copy(BitConverter.GetBytes(fileData.Length), 0, data, 0x4, 0x4);
            Array.Copy(BitConverter.GetBytes(fileNameOffset), 0, data, 0x8 + (fileData.Length * 0x8), 0x4);
            Array.Copy(BitConverter.GetBytes(fileData.Length * 0x30), 0, data, 0x8 + (fileData.Length * 0x8) + 0x4, 0x4);

            try
            {
                for (int i = 0; i < fileData.Length; i++)
                {
                    /* Write File Start, Length, and Name for the file */
                    int fileStart = offset;
                    int fileSize = fileData[i].Length;
                    string fileName = fileNames[i];

                    Array.Copy(BitConverter.GetBytes(fileStart), 0, data, 0x8 + (i * 0x8), 0x4);
                    Array.Copy(BitConverter.GetBytes(fileSize), 0, data, 0x8 + (i * 0x8) + 0x4, 0x4);
                    Array.Copy(fileData[i], 0, data, fileStart, fileSize);

                    if (fileName.Length >= 0x30)
                        fileName = "file_" + i + ".bin";

                    for (int j = 0; j < fileName.Length; j++)
                        data[fileNameOffset + (i * 0x30) + j] = (byte)fileName[j];

                    offset += fileSize;
                }

                return data;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }


        private string getFileName(byte[] testData, int fileNameLocation, int j)
        {
            string fileName = "";
            /* Files inside AFS archives will always have file names. */
            for (int i = 0; i < 0x30; i++)
            {
                if (testData[fileNameLocation + (j * 0x30) + i] == 0x0)
                    break;

                fileName += (char)testData[fileNameLocation + (j * 0x30) + i];
            }

            return fileName;
        }
    }
}