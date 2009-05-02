using System;
using System.IO;

namespace puyo_tools
{
    public class MDL : ArchiveClass
    {
        /*
         * MDL files are archives that contain PVM, NJ, and NM files.
         * No filenames are stored in the MDL file.
        */

        /* Main Method */
        public MDL()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = StreamConverter.ToUShort(data, 0x2);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Use this to find filenames */
                uint expectedStart = NumberData.RoundUpToMultiple((files * 0xC) + 0x10, 4096);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the offset & length */
                    uint offset = StreamConverter.ToUInt(data, 0x10 + (i * 0xC));
                    uint length = StreamConverter.ToUInt(data, 0x0C + (i * 0xC));

                    /* Check for filenames, if the offset of the file is bigger we expected it to be */
                    string filename = String.Empty;
                    if (offset > expectedStart)
                        filename = StreamConverter.ToString(data, expectedStart, offset - expectedStart);

                    /* Now update the expected start. */
                    expectedStart = NumberData.RoundUpToMultiple(offset + length, 4096);

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
                return null;
            }
        }

        /* Add a header to a blank archive */
        public override byte[] CreateHeader(string[] files, string[] archiveFilenames, uint blockSize, object[] settings)
        {
            try
            {
                /* Create variables from settings */
                bool addFilenames = (bool)settings[0];

                /* Create the header data. */
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x8) + 0x8, 4)];

                /* Write out the header and number of files. */
                //Array.Copy(BitConverter.GetBytes((uint)ArchiveHeader.ACX), 0, header, 0x0, 4); // ACX
                //Array.Copy(BitConverter.GetBytes(Endian.Swap((uint)files.Length)), 0, header, 0x4, 4); // Files

                /* Set the offset */
                uint offset = (uint)header.Length;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)(new FileInfo(files[i]).Length);

                    /* Increment offsets for filenames */
                    if (addFilenames)
                        offset += NumberData.RoundUpToMultiple((uint)ObjectConverter.StringToBytes(archiveFilenames[i], 63).Length, 4);

                    /* Write the offsets and lengths */
                    Array.Copy(BitConverter.GetBytes(Endian.Swap(offset)), 0, header, 0x8 + (i * 0x8), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(Endian.Swap(length)), 0, header, 0xC + (i * 0x8), 4); // Length

                    /* Now increment the offset */
                    offset += NumberData.RoundUpToMultiple(length, 4);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }

        /* Get offset for the file and the filename */
        public uint getOffset(byte[] header, uint file)
        {
            try
            {
                /* See if it is the first file. */
                if (file == 0)
                    return (uint)header.Length;
                else
                    return NumberData.RoundUpToMultiple(Endian.Swap(BitConverter.ToUInt32(header, 0x8 + (((int)file - 1) * 0x8))) + Endian.Swap(BitConverter.ToUInt32(header, 0xC + ((int)(file - 1) * 0x8))), 4);
            }
            catch
            {
                /* Something went wrong, so return the offset 0x0 */
                return 0x0;
            }
        }
    }
}