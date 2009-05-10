using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class GNT : ArchiveClass
    {
        /*
         * GNT files are archives that contains GVR files.
         * No filenames are stored in the SNT file.
        */

        /* Main Method */
        public GNT()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = Endian.Swap(StreamConverter.ToUInt(data, 0x30));

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* See if the archive contains filenames */
                bool containsFilenames = (files > 0 && Endian.Swap(StreamConverter.ToUInt(data, 0x3C + (files * 0x14))) + 0x20 != 0x3C + (files * 0x1C) && StreamConverter.ToString(data, 0x3C + (files * 0x1C), 4) == "FLST");

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    fileList[i] = new object[] {
                        Endian.Swap(StreamConverter.ToUInt(data, 0x40 + (files * 0x14) + (i * 0x8))) + 0x20, // Offset
                        Endian.Swap(StreamConverter.ToUInt(data, 0x3C + (files * 0x14) + (i * 0x8))),        // Length
                        (containsFilenames ? StreamConverter.ToString(data, 0x40 + (files * 0x1C) + (i * 0x40), 64) : String.Empty) // Filename
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
                //blockSize         = 8;
                bool addFilenames = settings[0];

                /* Create the header data. */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(0x3C + (files.Length * 0x1C));

                /* Start with the NSIF/NUIF header */
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.NGIF, 4));
                header.AddRange(NumberConverter.ToByteList(0x18));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x01)));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x20)));

                /* Get the size for the NTL data */
                uint ntl_size = 0;
                foreach (string file in files)
                    ntl_size += Number.RoundUp((uint)new FileInfo(file).Length, blockSize);

                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x1C + ((uint)files.Length * 0x1C) + ntl_size)));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x3C + ((uint)files.Length * 0x1C) + ntl_size)));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x18 + ((uint)files.Length * 4))));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x01)));

                /* NTL Header */
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.NGTL, 4));
                header.AddRange(NumberConverter.ToByteList(0x14 + ((uint)files.Length * 0x1C) + ntl_size));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x10)));
                header.AddRange(NumberConverter.ToByteList(0x00));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap((uint)files.Length)));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(0x1C)));
                header.AddRange(NumberConverter.ToByteList(Endian.Swap(((uint)files.Length * 0x14) + 0x1C)));

                /* Start adding the crap bytes at the top */
                for (int i = 0; i < files.Length; i++)
                {
                    header.AddRange(NumberConverter.ToByteList(0x00));
                    header.AddRange(NumberConverter.ToByteList(0x00));
                    header.AddRange(ByteConverter.ToByteList(new byte[] { 0x0, 0x1, 0x0, 0x1 }));
                    header.AddRange(NumberConverter.ToByteList(Endian.Swap((uint)i)));
                    header.AddRange(NumberConverter.ToByteList(0x00));
                }

                /* Set the intial offset */
                if (addFilenames)
                    header.Capacity += 0x4 + (0x40 * files.Length);
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList.Add(offset);
                    header.AddRange(NumberConverter.ToByteList(Endian.Swap(length)));        // Length
                    header.AddRange(NumberConverter.ToByteList(Endian.Swap(offset - 0x20))); // Offset

                    /* Increment the offset */
                    offset += Number.RoundUp(length, blockSize);
                }

                /* Do we want to add filenames? */
                if (addFilenames)
                {
                    header.AddRange(StringConverter.ToByteList("FLST", 4));
                    for (int i = 0; i < files.Length; i++)
                        header.AddRange(StringConverter.ToByteList(archiveFilenames[i], 63, 64));
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
                //blockSize = 16;

                /* Create the footer */
                List<byte> footer = new List<byte>(Number.RoundUp(0x14 + (files.Length * 0x4), blockSize) + Number.RoundUp(0x4, blockSize));
                footer.AddRange(StringConverter.ToByteList("NOF0", 4));

                /* Write the crap data on the footer */
                footer.AddRange(NumberConverter.ToByteList(0x14 + (files.Length * 0x4)));
                footer.AddRange(NumberConverter.ToByteList(Endian.Swap((uint)files.Length + 2)));
                footer.AddRange(NumberConverter.ToByteList(0x00));
                footer.AddRange(NumberConverter.ToByteList(Endian.Swap(0x14)));

                /* Write this number for whatever reason */
                for (int i = 0; i < files.Length; i++)
                    footer.AddRange(NumberConverter.ToByteList(Endian.Swap(0x20 + ((uint)files.Length * 0x14) + ((uint)i * 0x8))));

                footer.AddRange(NumberConverter.ToByteList(Endian.Swap(0x18)));

                /* Pad data before NEND */
                while (footer.Count % blockSize != 0)
                    footer.Add(PaddingByte());

                /* Add the NEND stuff and then pad file */
                footer.AddRange(StringConverter.ToByteList("NEND", 4));
                while (footer.Count < footer.Capacity)
                    footer.Add(PaddingByte());

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