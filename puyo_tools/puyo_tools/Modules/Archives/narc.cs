using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class NARC : ArchiveClass
    {
        /*
         * NARC files are archives used on the Nintendo DS.
         * NARC files may or may not contain filenames.
        */

        /* Main Method */
        public NARC()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the offset of each section of the NARC file */
                uint offset_fatb = StreamConverter.ToUShort(data, 0xC);
                uint offset_fntb = offset_fatb + StreamConverter.ToUInt(data, offset_fatb + 0x4);
                uint offset_fimg = offset_fntb + StreamConverter.ToUInt(data, offset_fntb + 0x4);

                /* Stuff for filenames */
                bool containsFilenames = (StreamConverter.ToUInt(data, offset_fntb + 0x8) == 8);
                uint offset_filename   = offset_fntb + 0x10;

                /* Get the number of files */
                uint files = StreamConverter.ToUInt(data, offset_fatb + 0x8);

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the offset & length */
                    uint offset = StreamConverter.ToUInt(data, offset_fatb + 0x0C + (i * 0x8));
                    uint length = StreamConverter.ToUInt(data, offset_fatb + 0x10 + (i * 0x8)) - offset;

                    /* Get the filename, if the NARC contains filenames */
                    string filename = String.Empty;
                    if (containsFilenames)
                    {
                        /* Ok, since the NARC contains filenames, let's go grab it now */
                        byte filename_length = StreamConverter.ToByte(data, offset_filename);
                        filename             = StreamConverter.ToString(data, offset_filename + 1, filename_length);
                        offset_filename     += (uint)(filename_length + 1);
                    }

                    fileList[i] = new object[] {
                        offset + offset_fimg + 0x8, // Offset
                        length,  // Length
                        filename // Filename
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

        /* Add a header to a blank archive */
        public override List<byte> CreateHeader(string[] files, string[] storedFilenames, int blockSize, bool[] settings, out List<uint> offsetList)
        {
            try
            {
                /* Create variables from settings */
                //blockSize         = 4;
                bool addFilenames = settings[0];

                /* Get the sizes for each section of the narc */
                uint size_fatb = 12 + (uint)(files.Length * 8);
                uint size_fntb = 16;
                uint size_fimg = 8;

                /* Add the size of the filenames, if we are adding filenames */
                if (addFilenames)
                {
                    foreach (string file in storedFilenames)
                        size_fntb += (1 + Math.Min((uint)file.Length, 255));

                    size_fntb++;
                    size_fntb = Number.RoundUp(size_fntb, blockSize);
                }

                /* Add the size for the files */
                foreach (string file in files)
                    size_fimg += Number.RoundUp((uint)new FileInfo(file).Length, blockSize);

                /* Ok, get the offsets for each section */
                uint offset_fatb = 0x10;
                uint offset_fntb = offset_fatb + size_fatb;
                uint offset_fimg = offset_fntb + size_fntb;

                /* Create the header */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>((int)offset_fimg + 8);

                /* Write out the NARC header */
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.NARC, 4)); // NARC
                header.AddRange(ByteConverter.ToByteList(new byte[] { 0xFE, 0xFF, 0x00, 0x01 })); // Fixed Values
                header.AddRange(NumberConverter.ToByteList(0x10 + size_fatb + size_fntb + size_fimg)); // File Size
                header.AddRange(ByteConverter.ToByteList(new byte[] { 0x10, 0x00, 0x30, 0x00 })); // Fixed Values

                /* Write our the FATB header */
                header.AddRange(StringConverter.ToByteList("BTAF", 4)); // FATB
                header.AddRange(NumberConverter.ToByteList(size_fatb)); // FATB Size
                header.AddRange(NumberConverter.ToByteList(files.Length)); // Number of Files

                /* Get the offset & length for the file */
                uint offset = offset_fimg + 8;
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(offset)); // Offset
                    header.AddRange(NumberConverter.ToByteList(offset + length)); // Length

                    offset += Number.RoundUp(length, 4);
                }

                /* Write out the FNTB header */
                header.AddRange(StringConverter.ToByteList("BTNF", 4)); // FNTB
                header.AddRange(NumberConverter.ToByteList(size_fntb)); // FNTB Size
                header.AddRange(NumberConverter.ToByteList((addFilenames ? 8 : 4))); // NARC contains filenames (8) or not (4)
                header.AddRange(ByteConverter.ToByteList(new byte[] { 0x00, 0x00, 0x01, 0x00 })); // Fixed Values

                /* Write out the filenames */
                if (addFilenames)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        header.Add((byte)Math.Min(storedFilenames[i].Length, 255)); // Length of filename
                        header.AddRange(StringConverter.ToByteList(storedFilenames[i], Math.Min(storedFilenames[i].Length, 255))); // Filename
                    }

                    /* Pad the file if we are not at the FIMG section yet */
                    while (header.Count < offset_fimg)
                        header.Add(PaddingByte());
                }

                /* Write out the FIMG header */
                header.AddRange(StringConverter.ToByteList("GMIF", 4)); // FIMG
                header.AddRange(NumberConverter.ToByteList(size_fimg)); // FIMG Size

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                offsetList = null;
                return null;
            }
        }

        /* Padding byte */
        public override byte PaddingByte()
        {
            return 0xFF;
        }
    }
}