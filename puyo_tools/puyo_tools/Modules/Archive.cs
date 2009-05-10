using System;
using System.IO;
using System.Collections.Generic;

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
        public string ArchiveName     = null;
        public string FileExt         = null;

        /* Archive Object for extraction */
        public Archive(Stream dataStream, string dataFilename)
        {
            /* Set up our archive information */
            Data     = dataStream;
            Filename = dataFilename;

            ArchiveInformation(ref Data, ref Filename, out Format, out Archiver, out ArchiveName, out FileExt);

            /* Stop if we don't have a supported archive */
            if (Format == ArchiveFormat.NULL)
                return;

            /* Translate the data before we work with it */
            MemoryStream translatedData = (MemoryStream)Archiver.TranslateData(ref Data);
            if (translatedData != null)
                Data = translatedData;
        }

        /* Archive object for creation */
        public Archive(ArchiveFormat format, string dataFilename, ArchiveClass archiver)
        {
            /* Set up our compression information */
            Filename = dataFilename;
            Format   = format;
            Archiver = archiver;
        }

        /* Blank archive class, so you can access methods */
        public Archive()
        {
        }

        /* Get file list */
        public object[][] GetFileList()
        {
            return Archiver.GetFileList(ref Data);
        }

        /* Create archive header */
        public List<byte> CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out List<uint> offsetList)
        {
            return Archiver.CreateHeader(files, archiveFilenames, blockSize, settings, out offsetList);
        }

        /* Create archive footer */
        public List<byte> CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref List<byte> header)
        {
            return Archiver.CreateFooter(files, archiveFilenames, blockSize, settings, ref header);
        }

        /* Format file to add to the archive */
        public Stream FormatFileToAdd(ref Stream data)
        {
            return Archiver.FormatFileToAdd(ref data);
        }

        /* Output Directory */
        public string OutputDirectory
        {
            get
            {
                return (ArchiveName == null ? null : ArchiveName + " Extracted");
            }
        }

        /* File Extension */
        public string FileExtension
        {
            get
            {
                return (FileExt == null ? String.Empty : FileExt);
            }
        }

        /* Padding Byte */
        public byte PaddingByte
        {
            get
            {
                return Archiver.PaddingByte();
            }
        }

        /* Get Archive information */
        private void ArchiveInformation(ref Stream data, ref string filename, out ArchiveFormat format, out ArchiveClass archiver, out string name, out string ext)
        {
            try
            {
                /* Let's check for archive formats based on the headers first */
                string header = StreamConverter.ToString(data, 0x0, 4, true);
                switch (header)
                {
                    case ArchiveHeader.AFS: // AFS
                        format   = ArchiveFormat.AFS;
                        archiver = new AFS();
                        name     = "AFS";
                        ext      = ".afs";
                        return;
                    case ArchiveHeader.GVM: // GVM
                        format   = ArchiveFormat.GVM;
                        archiver = new GVM();
                        name     = "GVM";
                        ext      = ".gvm";
                        return;
                    case ArchiveHeader.MRG: // MRG
                        format   = ArchiveFormat.MRG;
                        archiver = new MRG();
                        name     = "MRG";
                        ext      = ".mrg";
                        return;
                    case ArchiveHeader.NARC: // NARC
                        format   = ArchiveFormat.NARC;
                        archiver = new NARC();
                        name     = "NARC";
                        ext      = ".narc";
                        return;
                    case ArchiveHeader.ONE: // ONE
                        format   = ArchiveFormat.ONE;
                        archiver = new ONE();
                        name     = "ONE";
                        ext      = ".one";
                        return;
                    case ArchiveHeader.PVM: // PVM
                        format   = ArchiveFormat.PVM;
                        archiver = new PVM();
                        name     = "PVM";
                        ext      = ".pvm";
                        return;
                    case ArchiveHeader.SPK: // SPK
                        format   = ArchiveFormat.SPK;
                        archiver = new SPK();
                        name     = "SPK";
                        ext      = ".spk";
                        return;
                    case ArchiveHeader.TEX:  // TEX
                        format   = ArchiveFormat.TEX;
                        archiver = new TEX();
                        name     = "TEX";
                        ext      = ".tex";
                        return;

                    case ArchiveHeader.TXAG: // TXAG
                        format   = ArchiveFormat.TXAG;
                        archiver = new TXAG();
                        name     = "TXAG";
                        ext      = ".txd";
                        return;
                }

                /* Check based on file extension */
                if (Path.GetExtension(filename) != String.Empty)
                {
                    switch (Path.GetExtension(filename).Substring(1).ToLower())
                    {
                        case "acx": // ACX
                            format   = ArchiveFormat.ACX;
                            archiver = new ACX();
                            name     = "ACX";
                            ext      = ".acx";
                            return;
                        case "vdd": // VDD
                            format   = ArchiveFormat.VDD;
                            archiver = new VDD();
                            name     = "VDD";
                            ext      = ".vdd";
                            return;
                    }
                }

                /* GNT File */
                if (header == ArchiveHeader.NGIF && StreamConverter.ToString(data, 0x20, 4) == ArchiveHeader.NGTL)
                {
                    format   = ArchiveFormat.GNT;
                    archiver = new GNT();
                    name     = "GNT";
                    ext      = ".gnt";
                    return;
                }

                /* MDL File */
                if (StreamConverter.ToUShort(data, 0x0) == 0x2)
                {
                    format   = ArchiveFormat.MDL;
                    archiver = new MDL();
                    name     = "MDL";
                    ext      = ".mdl";
                    return;
                }

                /* SNT File */
                if ((header == ArchiveHeader.NSIF && StreamConverter.ToString(data, 0x20, 4) == ArchiveHeader.NSTL) ||
                    (header == ArchiveHeader.NUIF && StreamConverter.ToString(data, 0x20, 4) == ArchiveHeader.NUTL))
                {
                    format   = ArchiveFormat.SNT;
                    archiver = new SNT();
                    name     = "SNT";
                    ext      = ".snt";
                    return;
                }

                /* Storybook Archive */
                if (Endian.Swap(StreamConverter.ToUInt(data, 0x4)) == 0x10 &&
                   (Endian.Swap(StreamConverter.ToUInt(data, 0xC)) == 0xFFFFFFFF ||
                    Endian.Swap(StreamConverter.ToUInt(data, 0xC)) == 0x00000000))
                {
                    format   = ArchiveFormat.SBA;
                    archiver = new SBA();
                    name     = "Storybook Archive";
                    ext      = ".one";
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
                ext      = null;
                return;
            }
            catch
            {
                /* An error occured. */
                format   = ArchiveFormat.NULL;
                archiver = null;
                name     = null;
                ext      = null;
                return;
            }
        }

        /* Get Archive Information, used for archive creation */
        public void ArchiveInformation(ArchiveFormat format, out ArchiveClass archiver, out string name, out string filter, out int[] blockSize, out int filenameLength)
        {
            switch (format)
            {
                case ArchiveFormat.ACX:
                    archiver       = new ACX();
                    name           = "ACX";
                    filter         = filter = "ACX Archive (*.acx)|*.acx";
                    blockSize      = new int[] {4, 2048};
                    filenameLength = 63;
                    return;
                case ArchiveFormat.AFS:
                    archiver       = new AFS();
                    name           = "AFS";
                    filter         = "AFS Archive (*.afs)|*.afs";
                    blockSize      = new int[] {2048, 32};
                    filenameLength = 31;
                    return;
                case ArchiveFormat.GNT:
                    archiver       = new GNT();
                    name           = "GNT";
                    filter         = "GNT Archive (*.gnt)|*.gnt";
                    blockSize      = new int[] {8};
                    filenameLength = 64;
                    return;
                case ArchiveFormat.GVM:
                    archiver       = new GVM();
                    name           = "GVM";
                    filter         = "GVM Archive (*.gvm)|*.gvm";
                    blockSize      = new int[] {16, -1};
                    filenameLength = 28;
                    return;
                case ArchiveFormat.MDL:
                    archiver       = new MDL();
                    name           = "MDL";
                    filter         = "MDL Archive (*.mdl)|*.mdl";
                    blockSize      = new int[] {4096};
                    filenameLength = 63;
                    return;
                case ArchiveFormat.MRG:
                    archiver       = new MRG();
                    name           = "MRG";
                    filter         = "MRG Archive (*.mrg)|*.mrg";
                    blockSize      = new int[] { 16 };
                    filenameLength = 32;
                    return;
                case ArchiveFormat.NARC:
                    archiver       = new NARC();
                    name           = "NARC";
                    filter         = "NARC Archive (*.narc)|*.narc|CARC Archive (*.carc)|*.carc";
                    blockSize      = new int[] {4};
                    filenameLength = 255;
                    return;
                case ArchiveFormat.ONE:
                    archiver       = new ONE();
                    name           = "ONE";
                    filter         = "ONE Archive (*.one)|*.one|ONZ Archive (*.onz)|*.onz";
                    blockSize      = new int[] {32};
                    filenameLength = 55;
                    return;
                case ArchiveFormat.PVM:
                    archiver       = new PVM();
                    name           = "PVM";
                    filter         = "PVM Archive (*.pvm)|*.pvm";
                    blockSize      = new int[] {16, -1};
                    filenameLength = 28;
                    return;
                case ArchiveFormat.SNT:
                    archiver       = new SNT();
                    name           = "SNT";
                    filter         = "SNT Archive (*.snt)|*.snt";
                    blockSize      = new int[] {8};
                    filenameLength = 64;
                    return;
                case ArchiveFormat.SPK:
                    archiver       = new SPK();
                    name           = "SPK";
                    filter         = "SPK Archive (*.spk)|*.spk";
                    blockSize      = new int[] { 16 };
                    filenameLength = 20;
                    return;
                case ArchiveFormat.TEX:
                    archiver       = new TEX();
                    name           = "TEX";
                    filter         = "TEX Archive (*.tex)|*.tex";
                    blockSize      = new int[] { 16 };
                    filenameLength = 20;
                    return;
                case ArchiveFormat.TXAG:
                    archiver       = new TXAG();
                    name           = "TXAG";
                    filter         = "TXAG Archive (*.txd)|*.txd";
                    blockSize      = new int[] {64};
                    filenameLength = 31;
                    return;
                case ArchiveFormat.VDD:
                    archiver       = new VDD();
                    name           = "VDD";
                    filter         = "VDD Archive (*.vdd)|*.vdd";
                    blockSize      = new int[] { 2048, -1 };
                    filenameLength = 15;
                    return;
            }

            archiver       = null;
            name           = null;
            filter         = null;
            blockSize      = new int[0];
            filenameLength = 0;
            return;
        }
    }

    public abstract class ArchiveClass
    {
        /* Archive Functions */
        public abstract object[][] GetFileList(ref Stream data); // Get Stored Files
        public abstract List<byte> CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out List<uint> offsetList);
        public virtual List<byte> CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref List<byte> header)
        {
            return null;
        }
        public virtual Stream TranslateData(ref Stream data) // Translate Data
        {
            return null;
        }
        public virtual Stream FormatFileToAdd(ref Stream data) // Format File to Add
        {
            return null;
        }
        public virtual byte PaddingByte() // Padding Byte
        {
            return 0x0;
        }
    }

    /* Archive format ID */
    public enum ArchiveFormat : byte
    {
        NULL, // Unknown Archive Format
        ACX,  // ACX
        AFS,  // AFS
        GNT,  // GNT
        GVM,  // GVM
        MDL,  // MDL
        MRG,  // MRG
        NARC, // NARC
        ONE,  // ONE
        PVM,  // PVM
        SBA,  // Storybook Archive
        SNT,  // SNT
        SPK,  // SPK
        TEX,  // TEX
        TXAG, // TXAG (Sonic Storybook TXD)
        VDD,  // VDD
    }

    /* Archive File Header */
    public static class ArchiveHeader
    {
        public const string
            NULL = null,
            ACX  = "\x00\x00\x00\x00",
            AFS  = "AFS\x00",
            GVM  = "GVMH",
            MDL  = "\x02\x00\x00\x00",
            MRG  = "MRG0",
            NARC = "NARC",
            NGIF = "NGIF",
            NGTL = "NGTL",
            NSIF = "NSIF",
            NSTL = "NSTL",
            NUIF = "NUIF",
            NUTL = "NUTL",
            ONE  = "one.",
            PVM  = "PVMH",
            SPK  = "SPK0",
            TEX  = "TEX0",
            TXAG = "TXAG";
    } 
}