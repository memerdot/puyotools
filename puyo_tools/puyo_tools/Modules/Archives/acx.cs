using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class ACX : ArchiveClass
    {
        /*
         * ACX files are archives that contains ADX files.
         * No filenames are stored in the ACX file.
        */

        /* Main Method */
        public ACX()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = Endian.Swap(StreamConverter.ToUInt(data, 0x4));

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* See if the archive contains filenames */
                bool containsFilenames = (files > 0 && StreamConverter.ToUInt(data, 0x8) != 0x8 + (files * 0x8) && StreamConverter.ToString(data, 0x8 + (files * 0x8), 4) == "FLST");

                /* Now we can get the file offsets, lengths, and filenames */
                for (int i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        Endian.Swap(StreamConverter.ToUInt(data, 0x8 + (i * 0x8))), // Offset
                        Endian.Swap(StreamConverter.ToUInt(data, 0xC + (i * 0x8))), // Length
                        (containsFilenames ? StreamConverter.ToString(data, 0xC + (files * 0x8) + (i * 0x40), 64) : String.Empty) // Filename
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
                /* Seperate settings */
                //blockSize = 4;
                bool addFilenames = settings[0];

                /* Create the header and offset list */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(0x8 + (files.Length * 0x8));
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.ACX, 4));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap((uint)files.Length)));

                /* Set the intial offset */
                if (addFilenames)
                    header.Capacity += 0x4 + (0x40 * files.Length);
                header.Capacity = Number.RoundUp(header.Capacity, blockSize);
                uint offset     = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(Endian.Swap(offset))); // Offset
                    header.AddRange(NumberConverter.ToByteList(Endian.Swap(length))); // Length

                    /* Increment the offset */
                    offset += Number.RoundUp(length, blockSize);
                }

                /* Do we want to add filenames? */
                if (addFilenames)
                {
                    header.AddRange(StringConverter.ToByteList("FLST", 4));
                    for (int i = 0; i < files.Length; i++)
                        header.AddRange(StringConverter.ToByteList(archiveFilenames[i], 63, 64));
                }

                return header;
            }
            catch
            {
                /* An error occured. */
                offsetList = null;
                return null;
            }
        }
    }
}