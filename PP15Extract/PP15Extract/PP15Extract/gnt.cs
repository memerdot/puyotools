using System;
using System.IO;
using System.Globalization;

namespace pp15_fileTools
{
    public class GNT
    {
        /* File Data */
        private int[][] extractOffsets;   // Data that has been extracted.
        public string[] extractFileNames; // Extracted file names.
        private int dataLocation;         // Location where the locations of files are.
        private int files;                // Number of files in the SNT file.
        private int fileStart;            // Start offset of extracted file.
        private int fileLength;           // Length of extracted file.
        private int expectedStart;        // Location where the data should start.

        public GNT()
        {
        }

        public int[][] extract(byte[] data, bool findFileNames)
        {
            /* Let's try to find file locations */
            if (data.Length < 0x34)
                return new int[0][];

            files         = byteSwap(BitConverter.ToUInt32(data, 0x30));
            dataLocation  = 0x48 + 0x8 + ((files - 1) * 0x14);
            expectedStart = dataLocation + (files * 0x8);

            extractOffsets   = new int[files][];
            extractFileNames = new string[files];

            /* Extract the data now */
            for (int i = 0; i < files; i++)
            {
                try
                {
                    fileStart  = byteSwap(BitConverter.ToUInt32(data, dataLocation + (i * 0x8) + 0x4)) + 0x20;
                    fileLength = byteSwap(BitConverter.ToUInt32(data, dataLocation + (i * 0x8)));

                    extractOffsets[i] = new int[] { fileStart, fileLength };

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
            /* GNT files created with this program may have filenames at the start. */
            if (fileNameStart != start)
            {
                /* We got a filename! */
                while (fileNameStart < start && fileNameStart < testData.Length)
                {
                    fileName += (char)testData[fileNameStart];
                    fileNameStart++;
                }
            }

            return fileName;
        }

        private int byteSwap(uint num)
        {
            num = (num >> 24) |
                  ((num << 8) & 0x00FF0000) |
                  ((num >> 8) & 0x0000FF00) |
                  (num << 24);

            return (int) num;
        }

    }
}