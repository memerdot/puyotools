using System;
using System.IO;
using System.Collections.Generic;

namespace puyo_tools
{
    public class PVM : ArchiveClass
    {
        /*
         * PVM files are archives that contain PVR files only.
        */

        /* Main Method */
        public PVM()
        {
        }

        /* Get the offsets, lengths, and filenames of all the files */
        public override object[][] GetFileList(ref Stream data)
        {
            try
            {
                /* Get the number of files */
                ushort files = StreamConverter.ToUShort(data, 0x0);

                /* Create the array of files now */
                object[][] fileList = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (int i = 0; i < files; i++)
                {
                    /* Get the filename */
                    string filename = StreamConverter.ToString(data, 0xA + (i * 0x24), 28);

                    fileList[i] = new object[] {
                        StreamConverter.ToUInt(data, 0x2 + (i * 0x24)), // Offset
                        StreamConverter.ToUInt(data, 0x6 + (i * 0x24)), // Length
                        (filename == String.Empty ? String.Empty : filename + ".pvr") // Filename
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

        /* To simplify the process greatly, we are going to convert
         * the PVM to a new format */
        public override Stream TranslateData(ref Stream stream)
        {
            try
            {
                /* Get the number of files, and format type in the stream */
                ushort files    = StreamConverter.ToUShort(stream, 0xA);
                byte formatType = StreamConverter.ToByte(stream, 0x8);

                /* Now let's see what information is contained inside the metadata */
                bool containsMDLN        = (formatType & (1 << 4)) > 0;
                bool containsFilename    = (formatType & (1 << 3)) > 0;
                bool containsPixelFormat = (formatType & (1 << 2)) > 0;
                bool containsDimensions  = (formatType & (1 << 1)) > 0;
                bool containsGlobalIndex = (formatType & (1 << 0)) > 0;

                /* Let's figure out the metadata size */
                int size_filename = 0, size_pixelFormat = 0, size_dimensions = 0, size_globalIndex = 0;
                if (containsFilename)    size_filename    = 28;
                if (containsPixelFormat) size_pixelFormat = 2;
                if (containsDimensions)  size_dimensions  = 2;
                if (containsGlobalIndex) size_globalIndex = 4;
                int metaDataSize = 2 + size_filename + size_pixelFormat + size_dimensions + size_globalIndex;

                /* Now create the header */
                MemoryStream data = new MemoryStream();
                data.Write(BitConverter.GetBytes(files), 0, 2);

                /* Ok, try to find out data */
                uint sourceOffset = StreamConverter.ToUInt(stream, 0x4) + 0x8;

                /* Find a PVR file if the offset refers to a MDLN file */
                if (containsMDLN)
                    sourceOffset = Number.RoundUp(sourceOffset + StreamConverter.ToUInt(stream, sourceOffset + 0x4), 16);

                /* Write each file in the header */
                uint offset = 0x2 + ((uint)files * 0x24);
                for (int i = 0; i < files; i++)
                {
                    /* Ok, get the size of the PVR file */
                    uint length = StreamConverter.ToUInt(stream, sourceOffset + 0x4) + 24;

                    /* Write the offset, file length, and filename */
                    data.Write(BitConverter.GetBytes(offset), 0, 4); // Offset
                    data.Write(BitConverter.GetBytes(length), 0, 4); // Length

                    if (containsFilename)
                        data.Write(StreamConverter.ToByteArray(stream, (uint)(0xE + (i * metaDataSize)), 28), 0, 28); // Filename
                    else
                        data.Position += 28;

                    /* Add the GBIX header */
                    data.Position = offset;
                    data.Write(StringConverter.ToByteArray(GraphicHeader.GBIX, 4), 0, 4);
                    data.Write(BitConverter.GetBytes((int)0x8), 0, 4);

                    /* Copy the global index */
                    if (containsGlobalIndex)
                        data.Write(StreamConverter.ToByteArray(stream, 0xE + size_filename + size_pixelFormat + size_dimensions + (i * metaDataSize), 4), 0, 4);
                    else
                        data.Position += 4;

                    /* Write out the 0x20 in the header */
                    data.Write(new byte[] { 0x20, 0x20, 0x20, 0x20 }, 0, 4);

                    /* Now copy the file */
                    ((MemoryStream)StreamConverter.Copy(stream, sourceOffset, length)).WriteTo(data);
                    data.Position = 0x26 + (i * 0x24);

                    sourceOffset += Number.RoundUp((length - 24), 16);

                    /* Increment the offset */
                    offset += length;
                }

                return data;
            }
            catch
            {
                /* Something went wrong, so send as blank stream */
                return null;
            }
        }

        /* Format File */
        public override Stream FormatFileToAdd(ref Stream data)
        {
            /* Check to see if this is a PVR */
            Images images = new Images(data, null);
            if (images.Format == GraphicFormat.PVR)
            {
                /* Does the file start with PVRT? */
                if (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.PVRT)
                    return data;

                /* Otherwise strip off the first 16 bytes */
                else
                    return StreamConverter.Copy(data, 0x10, data.Length - 0x10);
            }

            /* Can't add this file! */
            return null;
        }

        public override List<byte> CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out List<uint> offsetList)
        {
            try
            {
                /* Let's get out settings now */
                blockSize = 24;
                bool addFilename    = settings[0];
                bool addPixelFormat = settings[1];
                bool addDimensions  = settings[2];
                bool addGlobalIndex = settings[3];

                /* Let's figure out the metadata size, so we can create the header properly */
                int metaDataSize = 2;
                if (addFilename)    metaDataSize += 28;
                if (addPixelFormat) metaDataSize += 2;
                if (addDimensions)  metaDataSize += 2;
                if (addGlobalIndex) metaDataSize += 4;

                /* Create the header now */
                offsetList        = new List<uint>(files.Length);
                List<byte> header = new List<byte>(Number.RoundUp(0xC + (files.Length * metaDataSize), blockSize));
                header.AddRange(StringConverter.ToByteList(ArchiveHeader.PVM, 4));
                header.AddRange(NumberConverter.ToByteList(header.Capacity));
                
                /* Set up format type */
                byte formatType = 0x0;
                if (addFilename)    formatType |= (1 << 3);
                if (addPixelFormat) formatType |= (1 << 2);
                if (addDimensions)  formatType |= (1 << 1);
                if (addGlobalIndex) formatType |= (1 << 0);
                header.Add(formatType);
                header.Add(0x0);

                /* Write number of files */
                header.AddRange(NumberConverter.ToByteList((ushort)files.Length));

                uint offset = (uint)header.Capacity + 8;

                /* Start writing the information in the header */
                for (int i = 0; i < files.Length; i++)
                {
                    using (FileStream data = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                    {
                        /* Make sure this is a PVR */
                        Images images = new Images(data, files[i]);
                        if (images.Format != GraphicFormat.PVR)
                            throw new IncorrectGraphicFormat();

                        /* Get the header offset */
                        int headerOffset = (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.PVRT ? 0x0 : 0x10);

                        offsetList.Add(offset);
                        header.AddRange(NumberConverter.ToByteList((ushort)i));

                        if (addFilename)
                            header.AddRange(StringConverter.ToByteList(Path.GetFileNameWithoutExtension(archiveFilenames[i]), 27, 28));
                        if (addPixelFormat)
                            header.AddRange(StreamConverter.ToByteList(data, headerOffset + 0x8, 2));
                        if (addDimensions)
                        {
                            /* Get the width and height */
                            int width  = (int)Math.Min(Math.Log(StreamConverter.ToUShort(data, headerOffset + 0xC), 2) - 2, 9);
                            int height = (int)Math.Min(Math.Log(StreamConverter.ToUShort(data, headerOffset + 0xE), 2) - 2, 9);
                            header.Add((byte)((width << 4) | height));
                            header.Add(0x0);
                        }
                        if (addGlobalIndex)
                        {
                            if (headerOffset == 0x0)
                                header.AddRange(ByteConverter.ToByteList(new byte[] {0x0, 0x0, 0x0, 0x0}));
                            else
                                header.AddRange(StreamConverter.ToByteList(data, 0x8, 4));
                        }

                        offset += Number.RoundUp((uint)(data.Length - headerOffset), blockSize);
                    }
                }

                return header;
            }
            catch (IncorrectGraphicFormat)
            {
                offsetList = null;
                return null;
            }
            catch
            {
                offsetList = null;
                return null;
            }
        }
    }
}