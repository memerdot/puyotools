using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class SBA : ArchiveClass
    {
        /*
         * Storybook Archives contain any type of file that is PRS compressed.
         * Only used in the Sonic Storybook Series.
        */

        /* Main Method */
        public SBA()
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
                for (uint i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        StreamConverter.ToUInt(data, 0x4 + (i * 0x2C)), // Offset
                        StreamConverter.ToUInt(data, 0x8 + (i * 0x2C)), // Length
                        StreamConverter.ToString(data, 0xC + (i * 0x2C), 36) // Filename
                    };
                }

                return fileList;
            }
            catch
            {
                return null;
            }
        }

        /* To simplify the process greatly, we are going to convert
         * the Storybook Archive to a new format */
        public override Stream TranslateData(ref Stream stream)
        {
            try
            {
                /* Get the number of files */
                uint files = Endian.Swap(StreamConverter.ToUInt(stream, 0x0));

                /* Now create the header */
                MemoryStream data = new MemoryStream();
                data.Write(BitConverter.GetBytes(files), 0, 4);

                /* Write each file in the header */
                uint offset = 0xC + (files * 0x2C);
                for (int i = 0; i < files; i++)
                {
                    uint length = Endian.Swap(StreamConverter.ToUInt(stream, 0x3C + (i * 0x30)));

                    data.Write(BitConverter.GetBytes(offset), 0, 4); // Offset
                    data.Write(BitConverter.GetBytes(length), 0, 4); // Length
                    data.Write(StreamConverter.ToByteArray(stream, (uint)(0x10 + (i * 0x30)), 36), 0, 36); // Filename

                    /* Let's write the decompressed data */
                    uint sourceOffset     = Endian.Swap(StreamConverter.ToUInt(stream, 0x34 + (i * 0x30)));
                    uint sourceLength     = Endian.Swap(StreamConverter.ToUInt(stream, 0x38 + (i * 0x30)));
                    Stream compressedData = StreamConverter.Copy(stream, sourceOffset, sourceLength);

                    /* Decompress the data */
                    SBC decompressor = new SBC();
                    MemoryStream decompressedData = (MemoryStream)decompressor.Decompress(ref compressedData, length);
                    if (decompressedData == null)
                        throw new Exception();

                    /* Write the data */
                    data.Position = offset;
                    decompressedData.WriteTo(data);
                    data.Position = 0x30 + (i * 0x2C);
                    decompressedData.Close();

                    offset += length;
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        public override List<byte> CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out List<uint> offsetList)
        {
            offsetList = null;
            return null;
        }
    }
}