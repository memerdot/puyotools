using System;

/* Archive Module */
namespace puyo_tools
{
    public class Archive
    {
        public Archive()
        {
        }

        /* Extract Archive */
        public object[][] extract(byte[] data, bool returnFileNames, string archiveFileName)
        {
            /* Try to find the archive format. */
            switch (FileFormat.getArchiveFormat(data, archiveFileName))
            {
                case ArchiveFormat.ACX: // ACX Archive
                    ACX ACX_archive = new ACX();
                    return ACX_archive.extract(data, returnFileNames);

                case ArchiveFormat.AFS: // AFS Archive
                    AFS AFS_archive = new AFS();
                    return AFS_archive.extract(data, returnFileNames);

                case ArchiveFormat.GNT: // GNT Archive
                    GNT GNT_archive = new GNT();
                    return GNT_archive.extract(data, returnFileNames);

                case ArchiveFormat.MRG: // MRG Archive
                    MRG MRG_archive = new MRG();
                    return MRG_archive.extract(data, returnFileNames);

                case ArchiveFormat.NSIF: // SNT Archive (PS2)
                case ArchiveFormat.NUIF: // SNT Archive (PSP)
                    SNT SNT_archive = new SNT();
                    return SNT_archive.extract(data, returnFileNames);

                case ArchiveFormat.SPK: // SPK Archive
                    SPK SPK_archive = new SPK();
                    return SPK_archive.extract(data, returnFileNames);

                case ArchiveFormat.TEX: // TEX Archive
                    TEX TEX_archive = new TEX();
                    return TEX_archive.extract(data, returnFileNames);

                case ArchiveFormat.VDD: // VDD Archive
                    VDD VDD_archive = new VDD();
                    return VDD_archive.extract(data, returnFileNames);
            }

            /* Not a supported archive. */
            return new object[0][];
        }

        /* Create an Archive */
        public byte[] create(ArchiveFormat format, byte[][] data, string[] fileNames, bool addFileNames)
        {
            /* Attempt to create archive */
            switch (format)
            {
                case ArchiveFormat.ACX: // ACX Archive
                    ACX ACX_archive = new ACX();
                    return ACX_archive.create(data, fileNames, addFileNames);

                case ArchiveFormat.AFS: // AFS Archive
                    AFS AFS_archive = new AFS();
                    return AFS_archive.create(data, fileNames);

                case ArchiveFormat.GNT: // GNT Archive
                    GNT GNT_archive = new GNT();
                    return GNT_archive.create(data, fileNames, addFileNames);

                case ArchiveFormat.MRG: // MRG Archive
                    MRG MRG_archive = new MRG();
                    return MRG_archive.create(data, fileNames);

                case ArchiveFormat.NSIF: // SNT Archive (PS2)
                case ArchiveFormat.NUIF: // SNT Archive (PSP)
                    SNT SNT_archive = new SNT();
                    return SNT_archive.create(data, fileNames, addFileNames, format == ArchiveFormat.NUIF);

                case ArchiveFormat.SPK: // SPK Archive
                    SPK SPK_archive = new SPK();
                    return SPK_archive.create(data, fileNames);

                case ArchiveFormat.TEX: // TEX Archive
                    TEX TEX_archive = new TEX();
                    return TEX_archive.create(data, fileNames);

                case ArchiveFormat.VDD: // VDD Archive
                    VDD VDD_archive = new VDD();
                    return VDD_archive.create(data, fileNames);
            }

            return new byte[0];
        }

        /* Get the output directory. */
        public string getOutputDirectory(byte[] data, string archiveFileName)
        {
            switch (FileFormat.getArchiveFormat(data, archiveFileName))
            {
                case ArchiveFormat.ACX:  return ExtractDir.ACX;
                case ArchiveFormat.AFS:  return ExtractDir.AFS;
                case ArchiveFormat.GNT:  return ExtractDir.GNT;
                case ArchiveFormat.MRG:  return ExtractDir.MRG;
                case ArchiveFormat.NSIF:
                case ArchiveFormat.NUIF: return ExtractDir.SNT;
                case ArchiveFormat.SPK:  return ExtractDir.SPK;
                case ArchiveFormat.TEX:  return ExtractDir.TEX;
                case ArchiveFormat.VDD:  return ExtractDir.VDD;
            }

            return String.Empty;
        }
    }
}