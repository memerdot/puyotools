using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class AFS : ArchiveClass
    {
        /*
         * AFS files are archives that contains files.
         * File names can be up to 32 characters in length.
        */

        /* Main Method */
        public AFS()
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

                /* Find the metadata location */
                uint metadataLocation = StreamConverter.ToUInt(data, (files * 0x8) + 0x8);
                if (metadataLocation == 0x0)
                    metadataLocation = StreamConverter.ToUInt(data, StreamConverter.ToUInt(data, 0x8) - 0x8);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        StreamConverter.ToUInt(data, 0x8 + (i * 0x8)), // Offset
                        StreamConverter.ToUInt(data, 0xC + (i * 0x8)), // Length
                        StreamConverter.ToString(data, metadataLocation + (i * 0x30), 32) // Filename
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
                //blockSize = 2048;
                bool v1 = settings[0];

                /* Create the header data. */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0x10 + (files.Length * 0x8), blockSize));
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.AFS, 4));
                header.AddRange(NumberConverter.ToByteList(files.Length));

                /* Set the intial offset */
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(offset)); // Offset
                    header.AddRange(NumberConverter.ToByteList(length)); // Length

                    /* Increment the offset */
                    offset += Number.RoundUp(length, blockSize);
                }

                /* Add the location to the metadata */
                if (v1) // AFS v1
                {
                    header.InsertRange(header.Capacity - 0x8, NumberConverter.ToByteList(offset));
                    header.InsertRange(header.Capacity - 0x4, NumberConverter.ToByteList(files.Length * 0x30));
                }
                else // AFS v2
                {
                    header.AddRange(NumberConverter.ToByteList(offset));
                    header.AddRange(NumberConverter.ToByteList(files.Length * 0x30));
                }

                return header;
            }
            catch
            {
                offsetList = null;
                return null;
            }
        }

        /* Create a footer for the archive */
        public override List<byte> CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref List<byte> header)
        {
            try
            {
                /* Create variables from settings */
                //blockSize = 2048;
                bool v1                = settings[0];
                bool storeCreationTime = settings[1];

                /* Create the footer */
                List<byte> footer = new List<byte>(Number.RoundUp(files.Length * 0x30, blockSize));

                for (int i = 0; i < files.Length; i++)
                {
                    DateTime fileDate = new FileInfo(files[i]).CreationTime;

                    /* Write the filename and file info */
                    footer.AddRange(StringConverter.ToByteList(archiveFilenames[i], 31, 32));

                    if (storeCreationTime)
                    {
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Year));
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Month));
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Day));
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Hour));
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Minute));
                        footer.AddRange(NumberConverter.ToByteList((short)fileDate.Second));
                    }
                    else
                    {
                        for (int j = 0; j < 12; j++)
                            footer.Add(0x0);
                    }

                    /* Store this useless byte for some reason */
                    if (v1) // AFS v1
                        footer.AddRange(header.GetRange(0x8 + (i * 0x8), 4));
                    else // AFS v2
                        footer.AddRange(header.GetRange(0x4 + (i * 0x4), 4));
                }

                return footer;
            }
            catch
            {
                /* An error occured */
                return null;
            }
        }
    }
}