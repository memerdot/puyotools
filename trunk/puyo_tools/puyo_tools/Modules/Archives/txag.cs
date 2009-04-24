using System;
using System.IO;

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
                uint files = Endian.Swap(ObjectConverter.StreamToUInt(data, 0x4));

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileInfo[i] = new object[] {
                        Endian.Swap(ObjectConverter.StreamToUInt(data,   0x08 + (i * 0x28))), // Offset
                        Endian.Swap(ObjectConverter.StreamToUInt(data,   0x0C + (i * 0x28))), // Length
                        ObjectConverter.StreamToString(data, 0x10 + (i * 0x28), 32) + ".gvr"  // Filename
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
        public override byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            try
            {
                /* Create the header data. */
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x40) + 0x8, 32)];

                /* Write out the header and number of files. */
                Array.Copy(BitConverter.GetBytes((uint)ArchiveHeader.ONE), 0, header, 0x0, 4); // ONE
                Array.Copy(BitConverter.GetBytes(files.Length), 0, header, 0x4, 4); // Files

                /* Set the offset */
                uint offset = (uint)header.Length;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)(new FileInfo(files[i]).Length);

                    /* Write the filename */
                    Array.Copy(ObjectConverter.StringToBytes(storedFilenames[i], 55), 0, header, 0x8 + (i * 0x40), 55);

                    /* Write the offsets and lengths */
                    Array.Copy(BitConverter.GetBytes(offset), 0, header, 0x40 + (i * 0x40), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(length), 0, header, 0x44 + (i * 0x40), 4); // Length

                    /* Now increment the offset */
                    offset += NumberData.RoundUpToMultiple(length, 32);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }

        /* Get offset for the file*/
        public uint getOffset(byte[] header, uint file)
        {
            try
            {
                /* Return the offset that we can add the file to. */
                return BitConverter.ToUInt32(header, (int)(0x40 + (file * 0x40)));
            }
            catch
            {
                /* Something went wrong, so return the offset 0x0 */
                return 0x0;
            }
        }
    }
}