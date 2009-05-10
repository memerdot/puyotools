using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class TXAG : ArchiveClass
    {
        /*
         * TXAG archives are TXD archives in the Sonic Storybook Series.
         * They are named TXAG to reduce confusion with Sonic Heroes TXD files.
         * They contain GVR images.
        */

        /* Main Method */
        public TXAG()
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

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    string filename = StreamConverter.ToString(data, 0x10 + (i * 0x28), 32);

                    fileList[i] = new object[] {
                        Endian.Swap(StreamConverter.ToUInt(data, 0x08 + (i * 0x28))), // Offset
                        Endian.Swap(StreamConverter.ToUInt(data, 0x0C + (i * 0x28))), // Length
                        (filename == String.Empty ? String.Empty : filename + ".gvr") // Filename
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
                //blockSize = 64;

                /* Create the header data. */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0x8 + (files.Length * 0x28), blockSize));
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.TXAG, 4));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap((uint)files.Length)));

                /* Set the intial offset */
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(offset)); // Offset
                    header.AddRange(NumberConverter.ToByteList(length)); // Length
                    header.AddRange(StringConverter.ToByteList(Path.GetFileNameWithoutExtension(archiveFilenames[i]), 31, 32)); // Filename

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