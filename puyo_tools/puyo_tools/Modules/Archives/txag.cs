using System;
using System.IO;
using Extensions;

namespace puyo_tools
{
    public class TXAG : ArchiveClass
    {
        /*
         * TXAG archives are TXD archives in the Sonic Storybook Series.
         * They are named TXAG to reduce confusion with Renderware TXD files.
         * They contain GVR images.
        */

        /* Main Method */
        public TXAG()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override ArchiveFileList GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                uint files = data.ReadUInt(0x4).SwapEndian();

                /* Create the array of files now */
                ArchiveFileList fileList = new ArchiveFileList(files);

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    string filename = data.ReadString(0x10 + (i * 0x28), 32);

                    fileList.Entry[i] = new ArchiveFileList.FileEntry(
                        data.ReadUInt(0x08 + (i * 0x28)).SwapEndian(), // Offset
                        data.ReadUInt(0x0C + (i * 0x28)).SwapEndian(), // Length
                        (filename == string.Empty ? string.Empty : filename + ".gvr") // Filename
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
                //blockSize = 64;

                /* Create the header data. */
                offsetList          = new uint[files.Length];
                MemoryStream header = new MemoryStream(Number.RoundUp(0x8 + (files.Length * 0x28), blockSize));
                header.Write(ArchiveHeader.TXAG, 4);
                header.Write(files.Length.SwapEndian());

                /* Set the intial offset */
                uint offset = (uint)header.Capacity;

                for (int i = 0; i < files.Length; i++)
                {
                    uint length = (uint)new FileInfo(files[i]).Length;

                    /* Write out the information */
                    offsetList[i] = offset;
                    header.Write(offset.SwapEndian()); // Offset
                    header.Write(length.SwapEndian()); // Length
                    header.Write(Path.GetFileNameWithoutExtension(archiveFilenames[i]), 31, 32); // Filename

                    /* Increment the offset */
                    offset += length.RoundUp(blockSize);
                }

                return header;
            }
            catch
            {
                offsetList = null;
                return null;
            }
        }

        /* Checks to see if the input stream is a TXAG archive */
        public override bool Check(ref Stream input, string filename)
        {
            try
            {
                return (input.ReadString(0x0, 4) == ArchiveHeader.TXAG);
            }
            catch
            {
                return false;
            }
        }

        /* Archive Information */
        public override Archive.Information Information()
        {
            string Name   = "TXAG";
            string Ext    = ".txd";
            string Filter = "TXAG Archive (*.txd)|*.txd";

            bool Extract = true;
            bool Create  = true;

            int[] BlockSize   = { 64 };
            string[] Settings = null;
            bool[] DefaultSettings = null;

            return new Archive.Information(Name, Extract, Create, Ext, Filter, BlockSize, Settings, DefaultSettings);
        }
    }
}