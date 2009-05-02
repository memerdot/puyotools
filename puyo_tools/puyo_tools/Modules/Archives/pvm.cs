﻿using System;
using System.IO;

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
                object[][] fileInfo = new object[files][];

                /* Now we can get the file offsets, lengths, and filenames */
                for (uint i = 0; i < files; i++)
                {
                    /* Get the filename */
                    string filename = StreamConverter.ToString(data, 0xA + (i * 0x24), 28);

                    fileInfo[i] = new object[] {
                        StreamConverter.ToUInt(data, 0x2 + (i * 0x24)), // Offset
                        StreamConverter.ToUInt(data, 0x6 + (i * 0x24)), // Length
                        (filename == String.Empty ? NumberData.FormatFilename(i, (int)files) : filename) + ".pvr" // Filename
                    };
                }

                return fileInfo;
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
                int metaDataSize = 2 +
                    (containsFilename    ? 28 : 0) +
                    (containsPixelFormat ?  2 : 0) +
                    (containsDimensions  ?  2 : 0) +
                    (containsGlobalIndex ?  4 : 0);

                /* Now create the header */
                byte[] header   = new byte[0x2 + (files * 0x24)];
                uint fileOffset = (uint)header.Length;
                int arcLength   = header.Length;
                Array.Copy(BitConverter.GetBytes(files), 0, header, 0x0, 2);

                /* Ok, try to find out data */
                uint offset       = StreamConverter.ToUInt(stream, 0x4) + 0x8;
                byte[][] fileData = new byte[files][];

                /* Find a PVR file if the offset refers to a MDLN file */
                if (containsMDLN)
                    offset = NumberData.RoundUpToMultiple(offset + StreamConverter.ToUInt(stream, offset + 0x4), 16);

                for (int i = 0; i < files; i++)
                {
                    /* Ok, get the size of the PVR file */
                    int length = (int)StreamConverter.ToUInt(stream, offset + 0x4) + 24;

                    /* Create the byte array for this file */
                    fileData[i] = new byte[length];

                    /* Write the GBIX header */
                    Array.Copy(StringConverter.ToByteArray(FileHeader.GBIX, 4), 0, fileData[i], 0x0, 4); // GBIX
                    Array.Copy(BitConverter.GetBytes((uint)8), 0, fileData[i], 0x4, 4);

                    if (formatType == 0x9)
                        Array.Copy(StreamConverter.ToByteArray(stream, 0x2A + (i * 0x22), 4), 0, fileData[i], 0x8, 4); // Global Index (Type 09)
                    else
                        Array.Copy(StreamConverter.ToByteArray(stream, 0x2E + (i * 0x26), 4), 0, fileData[i], 0x8, 4); // Global Index (Type 0F)

                    /* Write out the four blank spaces (0x20) */
                    for (int j = 0; j < 4; j++)
                        fileData[i][0xC + j] = 0x20;

                    /* Now copy the file */
                    Array.Copy(StreamConverter.ToByteArray(stream, offset, length - 16), 0, fileData[i], 0x10, length - 16);

                    /* Now write this information to the new format */
                    Array.Copy(BitConverter.GetBytes(arcLength), 0, header, 0x2 + (i * 0x24), 4); // Offset
                    Array.Copy(BitConverter.GetBytes(length),    0, header, 0x6 + (i * 0x24), 4);

                    if (formatType == 0x9)
                        Array.Copy(StreamConverter.ToByteArray(stream, (uint)(0xE + (i * 0x22)), 28), 0, header, 0xA + (i * 0x24), 28); // Filename (Type 09)
                    else
                        Array.Copy(StreamConverter.ToByteArray(stream, (uint)(0xE + (i * 0x26)), 28), 0, header, 0xA + (i * 0x24), 28); // Filename (Type 0F)

                    /* Now increase the filesize for the stream */
                    arcLength += length;

                    /* And finally set the new offset */
                    offset += NumberData.RoundUpToMultiple((uint)(length - 24), 16);
                }

                /* Now create the memory stream and return it */
                MemoryStream newData = new MemoryStream(arcLength);
                newData.Write(header, 0x0, header.Length);

                for (int i = 0; i < files; i++)
                    newData.Write(fileData[i], 0x0, fileData[i].Length);

                return newData;
            }
            catch
            {
                /* Something went wrong, so send as blank stream */
                return new MemoryStream();
            }
        }

        /* Add a header to a blank archive */
        public override byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            try
            {
                /* Create the header data. */
                byte[] header = new byte[NumberData.RoundUpToMultiple(((uint)files.Length * 0x26) + 0xC, 16)];

                /* Write out the header and other information. */
                Array.Copy(ObjectConverter.StringToBytes(FileHeader.PVM, 4), 0, header, 0x0, 4); // PVMH
                Array.Copy(BitConverter.GetBytes(header.Length - 0x8),       0, header, 0x4, 4); // Offset of first file
                Array.Copy(BitConverter.GetBytes((ushort)0xF),               0, header, 0x8, 2); // What metadata is included (make it 0xF for now)
                Array.Copy(BitConverter.GetBytes((ushort)files.Length),      0, header, 0xA, 2); // Number of files

                for (int i = 0; i < files.Length; i++)
                {
                    /* Write out the file number */
                    Array.Copy(BitConverter.GetBytes((ushort)i), 0, header, 0xC + (i * 0x26), 2); // File Number

                    /* Next, let's make sure this is a PVR first */
                    FileStream pvr;
                    using (pvr = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                    {
                        if (FileFormat.Image(pvr, Path.GetExtension(files[i])) != GraphicFormat.PVR)
                            continue;

                        /* Ok, we have confirmed it's a PVR. Now get some information about the PVR. */
                        uint headerOffset = (uint)(ObjectConverter.StreamToString(pvr, 0x0, 4) == FileHeader.PVRT ? 0x0 : 0x10);
                        int width         = (int)Math.Min(Math.Log(ObjectConverter.StreamToUShort(pvr, headerOffset + 0xC), 2) - 2, 9);
                        int height        = (int)Math.Min(Math.Log(ObjectConverter.StreamToUShort(pvr, headerOffset + 0xE), 2) - 2, 9);
                        uint globalIndex  = (headerOffset == 0x10 ? ObjectConverter.StreamToUInt(pvr, 0x8) : 0);

                        /* Ok, let's write that info to the header now */
                        Array.Copy(ObjectConverter.StringToBytes(Path.GetFileNameWithoutExtension(storedFilenames[i]), 27), 0, header, 0xC + (i * 0x26) + 0x2, 27); // Filename
                        Array.Copy(ObjectConverter.StreamToBytes(pvr, headerOffset + 0x8, 2), 0, header, 0xC + (i * 0x26) + 0x1E, 2); // Image Format Details
                        Array.Copy(BitConverter.GetBytes((ushort)((width << 4) | height)),    0, header, 0xC + (i * 0x26) + 0x20, 2); // Width & Height
                        Array.Copy(BitConverter.GetBytes(globalIndex), 0, header, 0xC + (i * 0x26) + 0x22, 4); // Global Index
                    }
                }

                return header;
            }
            catch
            {
                /* Something went wrong, so return nothing */
                return new byte[0];
            }
        }

        /* Format File */
        public override Stream FormatFile(ref Stream data)
        {
            /* Check to see if this is a PVR */
            Images images = new Images(data, null);
            if (images.Format == GraphicFormat.PVR)
            {
                /* Does the file start with GVRT? */
                if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.PVRT)
                    return data;

                /* Otherwise strip off the first 16 bytes */
                else
                    return ObjectConverter.StreamToStream(data, 0x10, (int)data.Length - 0x10);
            }

            /* Can't add this file! */
            return null;
        }
    }
}