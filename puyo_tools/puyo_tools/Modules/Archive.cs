using System;
using System.IO;
using Extensions;
using System.Windows.Forms;
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

        /* Dictionary */
        private Dictionary<ArchiveFormat, ArchiveClass> dictionary = null;

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
            MemoryStream translatedData = Archiver.TranslateData(ref Data);
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
        public ArchiveFileList GetFileList()
        {
            return Archiver.GetFileList(ref Data);
        }

        /* Create archive header */
        public MemoryStream CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out uint[] offsetList)
        {
            return Archiver.CreateHeader(files, archiveFilenames, blockSize, settings, out offsetList);
        }

        /* Create archive footer */
        public MemoryStream CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref MemoryStream header)
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
                /* Set up the archive table */
                InitalizeDictionary();

                foreach (KeyValuePair<ArchiveFormat, ArchiveClass> value in dictionary)
                {
                    /* This is a supported archive */
                    if (dictionary[value.Key].Check(ref data, filename))
                    {
                        Information info = dictionary[value.Key].Information();

                        /* We can extract this archive */
                        if (info.Extract)
                        {
                            format   = value.Key;
                            archiver = value.Value;
                            name     = info.Name;
                            ext      = info.Ext;
                            return;
                        }
                    }
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

        /* Get Archive Creation Information */
        public void CreationInformation(ArchiveFormat format, out ArchiveClass archiver, out string name, out string filter, out int[] blockSize, out CheckBox[] settings)
        {
            switch (format)
            {
                case ArchiveFormat.ACX: // ACX Archive
                    archiver       = new ACX();
                    name           = "ACX";
                    filter         = "ACX Archive (*.acx)|*.acx";
                    blockSize      = new int[] { 4, 2048 };
                    settings = new CheckBox[] {
                        new CheckBox() {
                            Text     = "Add Filenames",
                            Checked  = false,
                    }};
                    return;

                case ArchiveFormat.AFS: // AFS Archive
                    archiver  = new AFS();
                    name      = "AFS";
                    filter    = "AFS Archive (*.afs)|*.afs";
                    blockSize = new int[] { 2048, 32 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text     = "Use AFS v1",
                            Checked  = false,
                        },
                        new CheckBox() {
                            Text     = "Store Creation Time",
                            Checked  = true,
                        }};
                    return;

                case ArchiveFormat.GNT: // GNT Archive
                    archiver  = new GNT();
                    name      = "GNT";
                    filter    = "GNT Archive (*.gnt)|*.gnt";
                    blockSize = new int[] { 8, -1 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text     = "Add Filenames",
                            Checked  = false,
                        }};
                    return;

                case ArchiveFormat.GVM: // GVM Archive
                    archiver  = new GVM();
                    name      = "GVM";
                    filter    = "GVM Archive (*.gvm)|*.gvm";
                    blockSize = new int[] { 16, -1 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text = "Add Filenames",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text = "Add GVR Pixel Format",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text = "Add GVR Dimensions",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text = "Add GVR Global Index",
                            Checked = true,
                        }};
                    return;

                case ArchiveFormat.MDL: // MDL Archive
                    archiver  = new MDL();
                    name      = "MDL";
                    filter    = "MDL Archive (*.mdl)|*.mdl";
                    blockSize = new int[] { 4096 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text     = "Add Filenames",
                            Checked  = false,
                        }};
                    return;

                case ArchiveFormat.MRG: // MRG Archive
                    archiver  = new MRG();
                    name      = "MRG";
                    filter    = "MRG Archive (*.mrg)|*.mrg";
                    blockSize = new int[] { 16 };
                    settings  = null;
                    return;

                case ArchiveFormat.NARC: // NARC Archive
                    archiver  = new NARC();
                    name      = "NARC";
                    filter    = "NARC Archive (*.narc)|*.narc|CARC Archive (*.carc)|*.carc";
                    blockSize = new int[] { 4, -1 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text     = "Add Filenames",
                            Checked  = true,
                        }};
                    return;

                case ArchiveFormat.ONE: // ONE Archive
                    archiver  = new ONE();
                    name      = "ONE";
                    filter    = "ONE Archive (*.one)|*.one|ONZ Archive (*.onz)|*.onz";
                    blockSize = new int[] { 32 };
                    settings  = null;
                    return;

                case ArchiveFormat.PVM: // PVM Archive
                    archiver  = new PVM();
                    name      = "PVM";
                    filter    = "PVM Archive (*.pvm)|*.pvm";
                    blockSize = new int[] { 16, -1 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text    = "Add Filenames",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text    = "Add PVR Pixel Format",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text    = "Add PVR Dimensions",
                            Checked = true,
                        },
                        new CheckBox() {
                            Text    = "Add PVR Global Index",
                            Checked = true,
                        }};
                    return;

                case ArchiveFormat.SNT: // SNT Archive
                    archiver  = new SNT();
                    name      = "SNT";
                    filter    = "SNT Archive (*.snt)|*.snt";
                    blockSize = new int[] { 8, -1 };
                    settings  = new CheckBox[] {
                        new CheckBox() {
                            Text    = "PSP SNT Archive",
                            Checked = false,
                        },
                        new CheckBox() {
                            Text     = "Add Filenames",
                            Checked  = false,
                        }};
                    return;

                case ArchiveFormat.SPK: // SPK Archive
                    archiver  = new SPK();
                    name      = "SPK";
                    filter    = "SPK Archive (*.spk)|*.spk";
                    blockSize = new int[] { 16 };
                    settings  = null;
                    return;

                case ArchiveFormat.TEX: // TEX Archive
                    archiver  = new TEX();
                    name      = "TEX";
                    filter    = "TEX Archive (*.tex)|*.tex";
                    blockSize = new int[] { 16 };
                    settings  = null;
                    return;

                case ArchiveFormat.TXAG: // TXAG Archive
                    archiver  = new TXAG();
                    name      = "TXAG";
                    filter    = "TXAG Archive (*.txd)|*.txd";
                    blockSize = new int[] { 64 };
                    settings  = null;
                    return;

                case ArchiveFormat.VDD: // VDD Archive
                    archiver  = new VDD();
                    name      = "VDD";
                    filter    = "VDD Archive (*.vdd)|*.vdd";
                    blockSize = new int[] { 2048, -1 };
                    settings  = null;
                    return;
            }

            archiver  = null;
            name      = null;
            filter    = null;
            blockSize = new int[0];
            settings  = null;
            return;
        }

        /* Initalize Dictionary */
        private void InitalizeDictionary()
        {
            dictionary = new Dictionary<ArchiveFormat, ArchiveClass>();

            /* Add entries to the dictionary */
            dictionary.Add(ArchiveFormat.ACX,  new ACX());
            dictionary.Add(ArchiveFormat.AFS,  new AFS());
            dictionary.Add(ArchiveFormat.GNT,  new GNT());
            dictionary.Add(ArchiveFormat.GVM,  new GVM());
            dictionary.Add(ArchiveFormat.MDL,  new MDL());
            dictionary.Add(ArchiveFormat.MRG,  new MRG());
            dictionary.Add(ArchiveFormat.NARC, new NARC());
            dictionary.Add(ArchiveFormat.ONE,  new ONE());
            dictionary.Add(ArchiveFormat.PVM,  new PVM());
            dictionary.Add(ArchiveFormat.SBA,  new SBA());
            dictionary.Add(ArchiveFormat.SNT,  new SNT());
            dictionary.Add(ArchiveFormat.SPK,  new SPK());
            dictionary.Add(ArchiveFormat.TEX,  new TEX());
            dictionary.Add(ArchiveFormat.TXAG, new TXAG());
            dictionary.Add(ArchiveFormat.VDD,  new VDD());
        }

        /* Archive Information */
        public class Information
        {
            public string Name   = null;
            public string Ext    = null;
            public string Filter = null;

            public bool Extract = false;
            bool Create  = false;

            int[] BlockSize        = new int[0];
            string[] Settings      = null;
            bool[] DefaultSettings = null;

            public Information(string name, bool extract, bool create, string ext, string filter, int[] blockSize, string[] settings, bool[] defaultSettings)
            {
                Name   = name;
                Ext    = ext;
                Filter = filter;

                Extract = extract;
                Create  = create;

                BlockSize       = blockSize;
                Settings        = settings ?? new string[0];
                DefaultSettings = defaultSettings ?? new bool[0];
            }
        }
    }

    public abstract class ArchiveClass
    {
        /* Archive Functions */
        public abstract ArchiveFileList GetFileList(ref Stream data); // Get Stored Files
        public abstract MemoryStream CreateHeader(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, out uint[] offsetList);
        public virtual MemoryStream CreateFooter(string[] files, string[] archiveFilenames, int blockSize, bool[] settings, ref MemoryStream header)
        {
            return null;
        }
        public virtual MemoryStream TranslateData(ref Stream data) // Translate Data
        {
            return null;
        }
        public virtual Stream FormatFileToAdd(ref Stream data) // Format File to Add
        {
            return data;
        }
        public virtual byte PaddingByte() // Padding Byte
        {
            return 0x0;
        }
        public virtual bool Check(ref Stream input, string filename)
        {
            return false;
        }
        public virtual Archive.Information Information()
        {
            return null;
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

    /* This contains a list of entries for an archive */
    /* Archives must be decompressed and translated first */
    public class ArchiveFileList
    {
        private FileEntry[] _Entry;
        private int _Entries;

        public ArchiveFileList(uint entries)
        {
            _Entry   = new FileEntry[entries];
            _Entries = (int)entries;
        }
        
        /* Return values */
        public FileEntry[] Entry
        {
            get { return _Entry; }
        }
        public int Entries
        {
            get { return _Entries; }
        }

        /* Archive File Entry */
        public class FileEntry
        {
            private string _FileName = null;
            private uint _Offset     = 0;
            private uint _Length     = 0;

            public FileEntry(uint offset, uint length, string filename)
            {
                _Offset   = offset;
                _Length   = length;
                _FileName = filename;
            }

            /* Return Values */
            public string FileName
            {
                get { return _FileName; }
            }
            public uint Offset
            {
                get { return _Offset; }
            }
            public uint Length
            {
                get { return _Length; }
            }
        }
    }
}