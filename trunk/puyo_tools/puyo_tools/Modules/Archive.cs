using System;
using System.IO;

/* Archive Module */
namespace puyo_tools
{
    public class Archive
    {
        /* Archive format */
        private ArchiveClass Archiver = null;
        public ArchiveFormat Format   = ArchiveFormat.NULL;
        public Stream Data            = null;
        private string Filename       = null;
        private string ArchiveName    = null;

        /* Archive Object for extraction */
        public Archive(Stream dataStream, string dataFilename)
        {
            /* Set up our archive information */
            Data     = dataStream;
            Filename = dataFilename;

            ArchiveInformation(ref Data, ref Filename, out Format, out Archiver, out ArchiveName);

            /* Stop if we don't have a supported archive */
            if (Format == ArchiveFormat.NULL)
                return;

            /* Translate the data before we work with it */
            Data = Archiver.TranslateData(ref Data);
        }

        /* Archive object for creation */
        public Archive(Stream dataStream, string dataFilename, ArchiveFormat format, ArchiveClass archiver)
        {
            /* Set up our compression information */
            Data     = dataStream;
            Filename = dataFilename;
            Format   = format;
            Archiver = archiver;
        }

        /* Get file list */
        public object[][] GetFileList()
        {
            return Archiver.GetFileList(ref Data);
        }

        /* Create archive header */
        public byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings)
        {
            return Archiver.CreateHeader(files, storedFilenames, blockSize, settings);
        }

        /* Create archive footer */
        public byte[] CreateFooter(string[] files, string[] storedFilenames, ref byte[] header, uint blockSize, object[] settings)
        {
            return Archiver.CreateFooter(files, storedFilenames, ref header, blockSize, settings);
        }

        /* Output Directory */
        public string OutputDirectory
        {
            get
            {
                return (ArchiveName == null ? null : ArchiveName + " Extracted");
            }
        }

        /* Format file, for adding a file to an archive */
        /*public Stream FormatFileToAdd(Stream data)
        {
            switch (format)
            {
                case ArchiveFormat.GVM: // Strip off the GBIX/GCIX header
                    if (FileFormat.Image(data, String.Empty) == GraphicFormat.GVR)
                    {
                        if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GVRT)
                            return data;
                        else
                            return ObjectConverter.StreamToStream(data, 0x10, (int)data.Length - 16);
                    }
                    else
                        return new MemoryStream();

                case ArchiveFormat.PVM: // Strip off the GBIX header
                    if (FileFormat.Image(data, String.Empty) == GraphicFormat.PVR)
                    {
                        if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.PVRT)
                            return data;
                        else
                            return ObjectConverter.StreamToStream(data, 0x10, (int)data.Length - 16);
                    }
                    else
                        return new MemoryStream();
            }

            return data;
        }*/

        /* Add padding, for creating the archive */
        /*public Stream AddPadding(uint length, int blockSize)
        {
            switch (format)
            {
                case ArchiveFormat.NARC: return PadData.FillStream(0xFF, blockSize - (int)(length % blockSize));
            }

            return PadData.FillStream(0x0, blockSize - (int)(length % blockSize));
        }*/

        /* Does the archive contain filenames */
        /*public bool ContainsFilenames()
        {
            switch (format)
            {
                case ArchiveFormat.ACX:
                case ArchiveFormat.GNT:
                case ArchiveFormat.NSIF:
                case ArchiveFormat.NUIF: return false;
            }

            return true;
        }*/

        /* Get Archive information */
        private void ArchiveInformation(ref Stream data, ref string filename, out ArchiveFormat format, out ArchiveClass archiver, out string name)
        {
            try
            {
                /* Let's check for archive formats based on the headers first */
                ArchiveHeader header = (ArchiveHeader)ObjectConverter.StreamToUInt(data, 0x0);
                switch (header)
                {
                    case ArchiveHeader.AFS: // AFS
                        format   = ArchiveFormat.AFS;
                        archiver = new AFS();
                        name     = "AFS";
                        return;
                    case ArchiveHeader.GVM: // GVM
                        format   = ArchiveFormat.GVM;
                        archiver = new GVM();
                        name     = "GVM";
                        return;
                    case ArchiveHeader.MRG: // MRG
                        format   = ArchiveFormat.MRG;
                        archiver = new MRG();
                        name     = "MRG";
                        return;
                    case ArchiveHeader.NARC: // NARC
                        format   = ArchiveFormat.NARC;
                        archiver = new NARC();
                        name     = "NARC";
                        return;
                    case ArchiveHeader.ONE: // ONE
                        format   = ArchiveFormat.ONE;
                        archiver = new ONE();
                        name     = "ONE";
                        return;
                    case ArchiveHeader.PVM: // PVM
                        format   = ArchiveFormat.PVM;
                        archiver = new PVM();
                        name     = "PVM";
                        return;
                    case ArchiveHeader.SPK: // SPK
                        format   = ArchiveFormat.SPK;
                        archiver = new SPK();
                        name     = "SPK";
                        return;
                    case ArchiveHeader.TEX:  // TEX
                        format   = ArchiveFormat.TEX;
                        archiver = new TEX();
                        name     = "TEX";
                        return;

                    case ArchiveHeader.TXAG: // TXAG
                        format   = ArchiveFormat.TXAG;
                        archiver = new TXAG();
                        name     = "TXAG";
                        return;
                }

                /* GNT File */
                if (header == ArchiveHeader.GNT && ObjectConverter.StreamToString(data, 0x20, 4) == FileHeader.NGTL)
                {
                    format   = ArchiveFormat.GNT;
                    archiver = new GNT();
                    name     = "GNT";
                    return;
                }

                /* Check based on file extension */
                switch (Path.GetExtension(filename).Substring(1).ToLower())
                {
                    case "acx": // ACX
                        format   = ArchiveFormat.ACX;
                        archiver = new ACX();
                        name     = "ACX";
                        return;
                    case "vdd": // VDD
                        format   = ArchiveFormat.VDD;
                        archiver = new VDD();
                        name     = "VDD";
                        return;
                }

                /* SNT File */
                if ((header == ArchiveHeader.NSIF && ObjectConverter.StreamToString(data, 0x20, 4) == FileHeader.NSTL) ||
                    (header == ArchiveHeader.NUIF && ObjectConverter.StreamToString(data, 0x20, 4) == FileHeader.NUTL))
                {
                    format   = ArchiveFormat.SNT;
                    archiver = new SNT();
                    name     = "SNT";
                    return;
                }

                /* Storybook Archive */
                if (Endian.Swap(ObjectConverter.StreamToUInt(data, 0x4)) == 0x10 &&
                   (Endian.Swap(ObjectConverter.StreamToUInt(data, 0xC)) == 0xFFFFFFFF ||
                    Endian.Swap(ObjectConverter.StreamToUInt(data, 0xC)) == 0x00000000))
                {
                    format   = ArchiveFormat.SBA;
                    archiver = new SBA();
                    name     = "Storybook Archive";
                    return;
                }

                /* Unknown or unsupported archive */
                throw new ArchiveFormatNotSupported();
            }
            catch (ArchiveFormatNotSupported)
            {
                /* Unknown or unsupported archive */
                format   = ArchiveFormat.NULL;
                archiver = null;
                name     = null;
                return;
            }
            catch
            {
                /* An error occured. */
                format   = ArchiveFormat.NULL;
                archiver = null;
                name     = null;
                return;
            }
        }
    }

    public abstract class ArchiveClass
    {
        /* Archive Functions */
        public abstract object[][] GetFileList(ref Stream data); // Get Stored Files
        public abstract byte[] CreateHeader(string[] files, string[] storedFilenames, uint blockSize, object[] settings);              // Create Header
        public virtual byte[] CreateFooter(string[] files, string[] storedFilenames, ref byte[] header, uint blockSize, object[] settings) // Create Footer
        {
            return null;
        }
        public virtual Stream TranslateData(ref Stream data) // Translate Data
        {
            return data;
        }
        public virtual Stream FormatFile(ref Stream data) // Format Data
        {
            return data;
        }
    }
}