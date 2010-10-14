﻿using System;
using System.IO;
using Extensions;

namespace PuyoTools
{
    public class SNT : ArchiveModule
    {
        /*
         * SNT files are archives that contains SVR or GIM files.
         * No filenames are stored in the SNT file.
        */

        /* Main Method */
        public SNT()
        {
            Name       = "SNT";
            Extension  = ".snt";
            CanPack    = true;
            CanExtract = true;
            Translate  = false;

            Filter       = new string[] { Name + " Archive", "*.snt" };
            PaddingByte  = 0x00;
            PackSettings = new ArchivePackSettings.SNT();
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override ArchiveFileList GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = data.ReadUInt(0x30);

                /* Create the array of files now */
                ArchiveFileList fileList = new ArchiveFileList(files);

                /* See if the archive contains filenames */
                bool containsFilenames = (files > 0 && data.ReadUInt(0x3C + (files * 0x14)) + 0x20 != 0x3C + (files * 0x1C) && data.ReadString(0x3C + (files * 0x1C), 4) == "FLST");

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the offset & length */
                    uint offset = data.ReadUInt(0x40 + (files * 0x14) + (i * 0x8)) + 0x20;
                    uint length = data.ReadUInt(0x3C + (files * 0x14) + (i * 0x8));

                    /* Check for filenames */
                    string filename = String.Empty;
                    if (containsFilenames)
                        filename = data.ReadString(0x40 + (files * 0x1C) + (i * 0x40), 64);

                    /* GIM files can also contain their original filename in the footer */
                    if (filename == string.Empty && length > 40 && data.ReadString(offset, 12, false) == GraphicHeader.MIG)
                    {
                        uint filenameOffset = data.ReadUInt(offset + 0x24) + 0x30;
                        if (filenameOffset < length)
                            filename = Path.GetFileNameWithoutExtension(data.ReadString(offset + filenameOffset, (int)(length - filenameOffset)));

                        if (filename != String.Empty)
                            filename += (filename.IsAllUpperCase() ? ".GIM" : ".gim");
                    }

                    fileList.Entries[i] = new ArchiveFileList.Entry(
                        offset,  // Offset
                        length,  // Length
                        filename // Filename
                    );
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
        public override MemoryStream CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out uint[] offsetList)
        {
            try
            {
                /* Create variables from settings */
                blockSize           = 8;
                bool ps2Snt         = settings[0];
                bool storeFilenames = settings[1];

                /* Create the header data. */
                offsetList          = new uint[files.Length];
                MemoryStream header = new MemoryStream(0x3C + (files.Length * 0x1C));

                /* Start with the NIF header */
                header.Write((ps2Snt ? ArchiveHeader.NSIF : ArchiveHeader.NUIF), 4);
                header.Write(0x18);
                header.Write(0x01);
                header.Write(0x20);

                /* Get the size for the NTL data */
                uint ntl_size = 0;
                foreach (string file in files)
                    ntl_size += Number.RoundUp((uint)new FileInfo(file).Length, blockSize);

                header.Write(0x1C + ((uint)files.Length * 0x1C) + ntl_size);
                header.Write(0x3C + ((uint)files.Length * 0x1C) + ntl_size);
                header.Write(0x18 + (files.Length * 4));
                header.Write(0x01);

                /* NTL Header */
                header.Write((ps2Snt ? ArchiveHeader.NSTL : ArchiveHeader.NUTL), 4);
                header.Write(0x14 + ((uint)files.Length * 0x1C) + ntl_size);
                header.Write(0x10);
                header.Write(0x00);
                header.Write(files.Length);
                header.Write(0x1C);
                header.Write((files.Length * 0x14) + 0x1C);

                /* Start adding the crap bytes at the top */
                for (int i = 0; i < files.Length; i++)
                {
                    header.Write(0x01);
                    header.Write(0x00);
                    header.Write(new byte[] {0x1, 0x0, 0x1, 0x0});
                    header.Write(i);
                    header.Write(0x00);
                }

                /* Set the intial offset */
                if (storeFilenames)
                    header.Capacity += 0x4 + (0x40 * files.Length);
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList[i] = offset;
                    header.Write(length);        // Length
                    header.Write(offset - 0x20); // Offset

                    /* Increment the offset */
                    offset += length.RoundUp(blockSize);
                }

                /* Do we want to add filenames? */
                if (storeFilenames)
                {
                    header.Write("FLST");
                    for (int i = 0; i < files.Length; i++)
                        header.Write(archiveFilenames[i], 63, 64);
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
        public override MemoryStream CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref MemoryStream header)
        {
            try
            {
                /* Create variables from settings */
                //blockSize = 16;

                /* Create the footer */
                MemoryStream footer = new MemoryStream(Number.RoundUp(0x14 + (files.Length * 0x4), blockSize) + Number.RoundUp(0x4, blockSize));
                footer.Write("NOF0");

                /* Write the crap data on the footer */
                footer.Write(0x14 + (files.Length * 0x4));
                footer.Write(files.Length + 2);
                footer.Write(0x00);
                footer.Write(0x14);

                /* Write this number for whatever reason */
                for (int i = 0; i < files.Length; i++)
                    footer.Write(0x20 + (files.Length * 0x14) + (i * 0x8));

                footer.Write(0x18);

                /* Pad data before NEND */
                while (footer.Position % blockSize != 0)
                    footer.WriteByte(PaddingByte);

                /* Add the NEND stuff and then pad file */
                footer.Write("NEND");
                while (footer.Position < footer.Capacity)
                    footer.Write(PaddingByte);

                return footer;
            }
            catch
            {
                /* An error occured */
                return null;
            }
        }

        /* Checks to see if the input stream is a SNT archive */
        public override bool Check(ref Stream input, string filename)
        {
            try
            {
                return ((input.ReadString(0x0, 4) == ArchiveHeader.NSIF &&
                    input.ReadString(0x20, 4) == ArchiveHeader.NSTL) ||
                    (input.ReadString(0x0, 4) == ArchiveHeader.NUIF && 
                    input.ReadString(0x20, 4) == ArchiveHeader.NUTL));
            }
            catch
            {
                return false;
            }
        }
    }
}