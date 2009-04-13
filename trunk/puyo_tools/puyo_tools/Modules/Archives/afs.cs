using System;
using System.IO;

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
                uint files = ObjectConverter.StreamToUInt(data, 0x4);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Find the metadata location */
                uint metadataLocation = ObjectConverter.StreamToUInt(data, (files * 0x8) + 0x8);
                if (metadataLocation == 0x0)
                    metadataLocation = ObjectConverter.StreamToUInt(data, ObjectConverter.StreamToUInt(data, 0x8) - 0x8);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileInfo[i] = new object[] {
                        ObjectConverter.StreamToUInt(data, 0x8 + (i * 0x8)), // Offset
                        ObjectConverter.StreamToUInt(data, 0xC + (i * 0x8)), // Length
                        ObjectConverter.StreamToString(data, metadataLocation + (i * 0x30), 32) // Filename
                    };
                }

                return fileInfo;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new object[0][];
            }
        }

        /* Add a header to a blank archive */
        public override byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            try
            {
                /* Create variables from settings */
                bool v1        = (bool)settings[0];

                /* Create the header data. */
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x8) + 0x10, blockSize)];

                /* Write out the header and number of files. */
                Array.Copy(ObjectConverter.StringToBytes(FileHeader.AFS, 3), 0, header, 0x0, 3); // AFS
                Array.Copy(BitConverter.GetBytes(files.Length), 0, header, 0x4, 4); // Files

                /* Set the offset */
                uint offset = (uint)header.Length;

                /* Now add the file offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)(new FileInfo(files[i]).Length);

                    /* Write the offsets and lengths */
                    Array.Copy(BitConverter.GetBytes(offset), 0, header, 0x8 + (i * 0x8), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(length), 0, header, 0xC + (i * 0x8), 4); // Length;

                    /* Now increment the offset */
                    offset += NumberData.RoundUpToMultiple(length, blockSize);
                }

                /* Now where do we write the metadata location? */
                if (v1) // AFS v1
                {
                    Array.Copy(BitConverter.GetBytes(offset),              0, header, header.Length - 0x8, 4); // Metadata Offset
                    Array.Copy(BitConverter.GetBytes(files.Length * 0x30), 0, header, header.Length - 0x4, 4); // Metadata Length
                }
                else // AFS v2
                {
                    Array.Copy(BitConverter.GetBytes(offset),              0, header, 0x8 + (files.Length * 0x8), 4); // Metadata Offset
                    Array.Copy(BitConverter.GetBytes(files.Length * 0x30), 0, header, 0xC + (files.Length * 0x8), 4); // Metadata Length
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }

        /* Get offset for the file or metadata */
        public uint getOffset(byte[] header, uint file)
        {
            try
            {
                /* Return the offset that we can add the file to. */
                uint offset = BitConverter.ToUInt32(header, (int)(0x8 + (file * 0x8)));

                /* See if the offset was 0 and we are trying to get the metadata */
                if (offset == 0x0 && file == BitConverter.ToUInt32(header, 0x4))
                    offset = BitConverter.ToUInt32(header, header.Length - 0x8);

                return offset;
            }
            catch
            {
                /* Something went wrong, so return the offset 0x0 */
                return 0x0;
            }
        }

        /* Create the metadata */
        public override byte[] CreateFooter(string[] files, string[] storedFilenames, ref byte[] header, uint blockSize, object[] settings)
        {
            try
            {
                /* Create variables from settings */
                bool v1        = (bool)settings[0];

                /* Create the metadata array */
                byte[] metadata = new byte[NumberData.RoundUpToMultiple((uint)(files.Length * 0x30), blockSize)];

                /* Write the metadata for each file */
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(files[i]);

                    Array.Copy(ObjectConverter.StringToBytes(storedFilenames[i], 31), 0, metadata, (i * 0x30), 31); // Filename

                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Year),   0, metadata, (i * 0x30) + 0x20, 2); // Year of Creation
                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Month),  0, metadata, (i * 0x30) + 0x22, 2); // Month of Creation
                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Day),    0, metadata, (i * 0x30) + 0x24, 2); // Day of Creation
                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Hour),   0, metadata, (i * 0x30) + 0x26, 2); // Hour of Creation
                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Minute), 0, metadata, (i * 0x30) + 0x28, 2); // Minute of Creation
                    Array.Copy(BitConverter.GetBytes((ushort)fileInfo.CreationTime.Second), 0, metadata, (i * 0x30) + 0x2A, 2); // Second of Creation

                    if (v1) // AFS v1
                        Array.Copy(header, 0x8 + (i * 0x8), metadata, (i * 0x30) + 0x2C, 4);
                    else // AFS v2
                        Array.Copy(header, 0x4 + (i * 0x4), metadata, (i * 0x30) + 0x2C, 4);
                }

                return metadata;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }
    }
}