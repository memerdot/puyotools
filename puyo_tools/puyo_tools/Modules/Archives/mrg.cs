using System;
using System.IO;
using Extensions;
using System.Collections.Generic;

namespace puyo_tools
{
    public class MRG : ArchiveClass
    {
        /*
         * MRG files are archives that contain files (duh!).
        */

        /* Main Method */
        public MRG()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override ArchiveFileList GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = data.ReadUInt(0x4);

                /* Create the array of files now */
                ArchiveFileList fileList = new ArchiveFileList(files);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get filename and extension */
                    string filename = data.ReadString(0x20 + (i * 0x30), 32); // Name
                    string fileext  = data.ReadString(0x10 + (i * 0x30), 4);  // Extension

                    fileList.Entry[i] = new ArchiveFileList.FileEntry(
                        data.ReadUInt(0x14 + (i * 0x30)), // Offset
                        data.ReadUInt(0x18 + (i * 0x30)), // Length
                        (filename == string.Empty ? string.Empty : filename) + (fileext == string.Empty ? string.Empty : '.' + fileext) // Filename
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
                //blockSize = 16;

                /* Create the header data. */
                offsetList          = new uint[files.Length];
                MemoryStream header = new MemoryStream(Number.RoundUp(0x10 + (files.Length * 0x30), blockSize));

                /* Write out the identifier and number of files */
                header.Write(ArchiveHeader.MRG, 4);
                header.Write(files.Length);

                /* Set the offset */
                uint offset = (uint)header.Capacity;

                /* Now add the filenames, offsets and lengths */
                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write the file extension */
                    string fileext = Path.GetExtension(archiveFilenames[i]);
                    header.Write((fileext == String.Empty ? String.Empty : fileext.Substring(1)), 3, 4);

                    /* Write the offsets and lengths */
                    offsetList[i] = offset;
                    header.Write(offset);
                    header.Write(length);

                    /* Write the filename */
                    header.Write(Path.GetFileNameWithoutExtension(archiveFilenames[i]), 31, 32);

                    /* Now increment the offset */
                    offset += length.RoundUp(blockSize);
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                offsetList = null;
                return null;
            }
        }

        /* Checks to see if the input stream is a MRG archive */
        public override bool Check(ref Stream input, string filename)
        {
            try
            {
                return (input.ReadString(0x0, 4) == ArchiveHeader.MRG);
            }
            catch
            {
                return false;
            }
        }

        /* Archive Information */
        public override Archive.Information Information()
        {
            string Name   = "MRG";
            string Ext    = ".mrg";
            string Filter = "MRG Archive (*.mrg)|*.mrg|MRZ Archive (*.mrz)|*.mrz";

            bool Extract = true;
            bool Create  = true;

            int[] BlockSize   = { 32 };
            string[] Settings = null;
            bool[] DefaultSettings = null;

            return new Archive.Information(Name, Extract, Create, Ext, Filter, BlockSize, Settings, DefaultSettings);
        }
    }
}