using System;
using System.IO;

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
                uint offset_fatb = ObjectConverter.StreamToUShort(data, 0xC);
                uint offset_fntb = offset_fatb + ObjectConverter.StreamToUInt(data, offset_fatb + 0x4);
                uint offset_fimg = offset_fntb + ObjectConverter.StreamToUInt(data, offset_fntb + 0x4);

                /* Stuff for filenames */
                bool containsFilenames = ObjectConverter.StreamToUInt(data, offset_fntb + 0x8) == 8;
                uint offset_filename   = offset_fntb + 0x10;

                /* Get the number of files */
                uint files = ObjectConverter.StreamToUInt(data, offset_fatb + 0x8);

                /* Create the array of files now */
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the offset & length */
                    uint offset = ObjectConverter.StreamToUInt(data, offset_fatb + 0x0C + (i * 0x8));
                    uint length = ObjectConverter.StreamToUInt(data, offset_fatb + 0x10 + (i * 0x8)) - offset;

                    /* Get the filename, if the NARC contains filenames */
                    string filename = String.Empty;
                    if (containsFilenames)
                    {
                        /* Ok, since the NARC contains filenames, let's go grab it now */
                        byte filename_length = ObjectConverter.StreamToBytes(data, offset_filename, 1)[0];
                        filename             = ObjectConverter.StreamToString(data, offset_filename + 1, filename_length);
                        offset_filename     += (uint)(filename_length + 1);
                    }

                    fileInfo[i] = new object[] {
                        offset + offset_fimg + 0x8, // Offset
                        length,  // Length
                        filename // Filename
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
                bool addFilenames = (bool)settings[0];

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
                    size_fntb = NumberData.RoundUpToMultiple(size_fntb, 4);
                }

                /* Add the size for the files */
                foreach (string file in files)
                    size_fimg += NumberData.RoundUpToMultiple((uint)new FileInfo(file).Length, 4);

                /* Ok, get the offsets for each section */
                uint offset_fatb = 0x10;
                uint offset_fntb = offset_fatb + NumberData.RoundUpToMultiple(size_fatb, 4);
                uint offset_fimg = offset_fntb + NumberData.RoundUpToMultiple(size_fntb, 4);

                /* Create the header */
                byte[] header = new byte[offset_fimg + 8];

                /* Write out the NARC header */
                Array.Copy(ObjectConverter.StringToBytes(FileHeader.NARC, 4), 0, header, 0x0, 4); // NARC
                Array.Copy(new byte[] { 0xFE, 0xFF, 0x00, 0x01 }, 0, header, 0x4, 4); // Fixed Values
                Array.Copy(BitConverter.GetBytes(0x10 + size_fatb + size_fntb + size_fimg), 0, header, 0x8, 4); // File size
                Array.Copy(new byte[] { 0x10, 0x00, 0x30, 0x00 }, 0, header, 0xC, 4); // Fixed Values

                /* Write our the FATB header */
                Array.Copy(ObjectConverter.StringToBytes("BTAF", 4), 0, header, offset_fatb,       4); // FATB
                Array.Copy(BitConverter.GetBytes(size_fatb),         0, header, offset_fatb + 0x4, 4); // FATB Size
                Array.Copy(BitConverter.GetBytes(files.Length),      0, header, offset_fatb + 0x8, 4); // Number of Files
                
                /* Get the offset & length for the file */
                uint offset = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    Array.Copy(BitConverter.GetBytes(offset),          0, header, offset_fatb + 0x0C + (i * 0x8), 4); // File Offset
                    Array.Copy(BitConverter.GetBytes(offset + length), 0, header, offset_fatb + 0x10 + (i * 0x8), 4); // File Length

                    offset += NumberData.RoundUpToMultiple(length, 4);
                }

                /* Write out the FNTB header */
                Array.Copy(ObjectConverter.StringToBytes("BTNF", 4),   0, header, offset_fntb,       4); // FNTB
                Array.Copy(BitConverter.GetBytes(size_fntb),           0, header, offset_fntb + 0x4, 4); // FNTB Size
                Array.Copy(BitConverter.GetBytes(addFilenames ? 8 : 4), 0, header, offset_fntb + 0x8, 4); // Determines if the NARC contains filenames.
                Array.Copy(new byte[] { 0x00, 0x00, 0x01, 0x00 },      0, header, offset_fntb + 0xC, 4); // Fixed Values

                /* Write out the filenames */
                if (addFilenames)
                {
                    uint filename_offset = 0x10;
                    for (int i = 0; i < files.Length; i++)
                    {
                        header[offset_fntb + filename_offset] = (byte)Math.Min(storedFilenames[i].Length, 255);
                        Array.Copy(ObjectConverter.StringToBytes(storedFilenames[i], 255), 0, header, offset_fntb + filename_offset + 1, storedFilenames[i].Length);

                        filename_offset += (1 + (uint)storedFilenames[i].Length);
                    }

                    filename_offset++;

                    /* Pad the ending if the section size isn't a multiple of 4 */
                    if (filename_offset < size_fntb)
                        Array.Copy(PadData.Fill(0xFF, (int)(size_fntb - filename_offset)), 0, header, offset_fntb + filename_offset, size_fntb - filename_offset);
                }

                /* Write out the FIMG header */
                Array.Copy(ObjectConverter.StringToBytes("GMIF", 4), 0, header, offset_fimg,       4); // FIMG
                Array.Copy(BitConverter.GetBytes(size_fimg),         0, header, offset_fimg + 0x4, 4); // FIMG Size

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }
    }
}