using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class ONE : ArchiveClass
    {
        /*
         * ONE Archives are used in PS2 & Wii Sonic Unleashed
        */

        /* Main Method */
        public ONE()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = StreamConverter.ToUInt(data, 0x4);

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        StreamConverter.ToUInt(data,   0x40 + (i * 0x40)),    // Offset
                        StreamConverter.ToUInt(data,   0x44 + (i * 0x40)),    // Length
                        StreamConverter.ToString(data, 0x08 + (i * 0x40), 56) // Filename
                    };
                }

                return fileList;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return null;
            }
        }

        /* Create a header for an archive */
        public override List<byte> CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out List<uint> offsetList)
        {
            try
            {
                /* Create variables from settings */
                //blockSize = 32;

                /* Create the header data. */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0x8 + (files.Length * 0x40), blockSize));
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.ONE, 4));
                header.AddRange(NumberConverter.ToByteList(files.Length));

                /* Set the intial offset */
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(StringConverter.ToByteList(archiveFilenames[i], 55, 56)); // Filename
                    header.AddRange(NumberConverter.ToByteList(offset)); // Offset
                    header.AddRange(NumberConverter.ToByteList(length)); // Length

                    /* Increment the offset */
                    offset += Number.RoundUp(length, blockSize);
                }

                return header;
            }
            catch
            {
                offsetList = null;
                return null;
            }
        }
    }
}