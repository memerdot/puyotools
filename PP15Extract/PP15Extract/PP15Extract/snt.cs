using System;
using System.IO;
using System.Globalization;

namespace pp15_fileTools
{
    public class SNT
    {
        /* File Data */
        private int[][] extractOffsets;   // Data that has been extracted.
        public string[] extractFileNames; // Extracted file names.
        private int dataLocation;         // Location where the locations of files are.
        private int files;                // Number of files in the SNT file.
        private int fileStart;            // Start offset of extracted file.
        private int fileLength;           // Length of extracted file.
        private int expectedStart;        // Location where the data should start.

        public SNT()
        {
        }

        public int[][] extract(byte[] data, bool findFileNames)
        {
            /* Let's try to find file locations */
            if (data.Length < 0x34)
                return new int[0][];

            files         = BitConverter.ToInt32(data, 0x30);
            dataLocation  = 0x48 + 0x8 + ((files - 1) * 0x14);
            expectedStart = dataLocation + (files * 0x8);

            extractOffsets   = new int[files][];
            extractFileNames = new string[files];

            /* Extract the data now */
            for (int i = 0; i < files; i++)
            {
                try
                {
                    fileStart  = BitConverter.ToInt32(data, dataLocation + (i * 0x8) + 0x4) + 0x20;
                    fileLength = BitConverter.ToInt32(data, dataLocation + (i * 0x8));

                    extractOffsets[i] = new int[] {fileStart, fileLength};

                    if (findFileNames)
                        extractFileNames[i] = findFileName(data, fileStart, fileLength, expectedStart);

                    expectedStart = fileStart + fileLength;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return extractOffsets;
        }

        private string findFileName(byte[] testData, int start, int length, int fileNameStart)
        {
            string fileName = "";
            /* There are 2 cases we can test. */
            /* SNT files created with this program may have filenames at the start. */
            if (fileNameStart != start)
            {
                /* We got a filename! */
                while (fileNameStart < start && fileNameStart < testData.Length)
                {
                    fileName += ((char)Int32.Parse(testData[fileNameStart].ToString("X2"), NumberStyles.AllowHexSpecifier));
                    fileNameStart++;
                }
            }

            /* PSP GIM files may have filenames stored inside the file. */
            else if (Header.isFile(testData, Header.MIG, start))
            {
                for (int i = Header.TGA.Length; i < 0x200; i++)
                {
                    /* See if we are out of bounds */
                    if (i + Header.TGA.Length >= length)
                        break;

                    /* Try to find a sign of a filename. */
                    if (Header.isFile(testData, Header.TGA, start + length - i))
                    {
                        /* Found a filename! */
                        i++;
                        while (length - i >= 0 && testData[start + length - i] != 0x0)
                        {
                            fileName = (char)testData[start + length - i] + fileName;
                            i++;
                        }
                        break;
                    }
                }

                /* Add the GIM extention if we need to, since this is a GIM file */
                if (fileName != String.Empty)
                    fileName += ".gim";
            }

            return fileName;
        }
    }
}