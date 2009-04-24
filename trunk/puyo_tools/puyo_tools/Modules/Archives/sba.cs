﻿using System;
using System.IO;

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
                ushort files = ObjectConverter.StreamToUShort(data, 0x0);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the filename */
                    string filename = ObjectConverter.StreamToString(data, 0xC + (i * 0x2C), 36);

                    fileInfo[i] = new object[] {
                        ObjectConverter.StreamToUInt(data, 0x4 + (i * 0x2C)), // Offset
                        ObjectConverter.StreamToUInt(data, 0x8 + (i * 0x2C)), // Length
                        filename, // Filename
                    };
                    //System.Windows.Forms.MessageBox.Show((uint)fileInfo[i][1] + "");
                }

                return fileInfo;
            }
            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show("EXTRACTION ERROR\n\n" + f.ToString());
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
                uint files = Endian.Swap(ObjectConverter.StreamToUInt(stream, 0x0));

                /* Now create the header */
                MemoryStream data = new MemoryStream();
                data.Write(BitConverter.GetBytes(files), 0, 4);

                /* Write each file in the header */
                uint offset = 0xC + (files * 0x2C);
                for (int i = 0; i < files; i++)
                {
                    uint length = Endian.Swap(ObjectConverter.StreamToUInt(stream, (uint)(0x3C + (i * 0x30))));

                    data.Write(BitConverter.GetBytes(offset), 0, 4); // Offset
                    data.Write(BitConverter.GetBytes(length), 0, 4); // Length
                    data.Write(ObjectConverter.StreamToBytes(stream, (uint)(0x10 + (i * 0x30)), 36), 0, 36); // Filename

                    /* Let's write the decompressed data */
                    uint sourceOffset = Endian.Swap(ObjectConverter.StreamToUInt(stream, 0x34 + (i * 0x30)));
                    uint sourceLength = Endian.Swap(ObjectConverter.StreamToUInt(stream, 0x38 + (i * 0x30)));
                    Stream compressedData = ObjectConverter.StreamToStream(stream, sourceOffset, sourceLength);

                    /* Decompress the data */
                    SBC decompressor = new SBC();
                    MemoryStream decompressedData = (MemoryStream)decompressor.Decompress(ref compressedData, length);
                    if (decompressedData == null)
                        throw new Exception();

                    /* Write the data */
                    data.Position = offset;
                    //System.Windows.Forms.MessageBox.Show(length + "");
                    data.Write(ObjectConverter.StreamToBytes(decompressedData, 0, (int)length), 0, (int)length);
                    data.Position = 0x30 + (i * 0x2C);
                    decompressedData.Close();

                    offset += length;
                }

                return data;
            }
            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show("TRANSLATION ERROR\n\n" + f.ToString());
                return null;
            }
        }

        /* Add a header to a blank archive */
        public override byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            try
            {
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}