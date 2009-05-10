using System;
using System.IO;
using System.Collections.Generic;

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

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = StreamConverter.ToUInt(data, 0x0);

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (int i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        StreamConverter.ToUInt(data,   0x14 + (i * 0x18)) * 0x800, // Offset
                        StreamConverter.ToUInt(data,   0x18 + (i * 0x18)),         // Length
                        StreamConverter.ToString(data, 0x04 + (i * 0x18), 16)      // Filename
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
                blockSize = 2048;

                /* Create the header data. */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0x4 + (files.Length * 0x18), blockSize));
                header.AddRange(NumberConverter.ToByteList(files.Length));

                /* Set the intial offset */
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(StringConverter.ToByteList(archiveFilenames[i], 15, 16)); // Filename
                    header.AddRange(NumberConverter.ToByteList(offset / 0x800)); // Offset
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