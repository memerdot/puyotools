using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class TEX : ArchiveClass
    {
        /*
         * TEX files are archives that contain GIM, SVP, and SVR files.
        */

        /* Main Method */
        public TEX()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = StreamConverter.ToUInt(data, 0x4);

                /* NDS files with the same magic code are a different format */
                if (files == data.Length - 4)
                    throw new Exception();

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get filename and extension */
                    string filename = StreamConverter.ToString(data, 0x1C + (i * 0x20), 20); // Name
                    string fileext  = StreamConverter.ToString(data, 0x10 + (i * 0x20), 4);  // Extension

                fileList[i] = new object[] {
                        StreamConverter.ToUInt(data,  0x14 + (i * 0x20)), // Offset
                        StreamConverter.ToUInt(data,  0x18 + (i * 0x20)), // Length
                        (filename == String.Empty && fileext == String.Empty ? String.Empty : filename + "." + fileext) // Filename
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
                //blockSize = 16;

                /* Create the header data. */
                offsetList = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0x10 + (files.Length * 0x20), blockSize));

                /* Write out the identifier and number of files */
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.TEX, 4));
                header.AddRange(NumberConverter.ToByteList(files.Length));

                /* Set the offset */
                uint offset = (uint)header.Capacity;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write the file extension */
                    string fileext = Path.GetExtension(archiveFilenames[i]);
                    header.AddRange(StringConverter.ToByteList((fileext == String.Empty ? String.Empty : fileext.Substring(1)), 3, 4));

                    /* Write the offsets and lengths */
                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(offset));
                    header.AddRange(NumberConverter.ToByteList(length));

                    /* Write the filename */
                    header.AddRange(StringConverter.ToByteList(Path.GetFileNameWithoutExtension(archiveFilenames[i]), 19, 20));

                    /* Now increment the offset */
                    offset += Number.RoundUp(length, blockSize);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                offsetList = null;
                return null;
            }
        }
    }
}