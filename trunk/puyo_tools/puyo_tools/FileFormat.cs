using System;
using System.IO;

namespace puyo_tools
{
    /* Archive format ID */
    public enum ArchiveFormat : byte
    {
        NULL, // Unknown Archive Format
        ACX,  // ACX
        AFS,  // AFS
        GNT,  // GNT
        GVM,  // GVM
        MRG,  // MRG
        NARC, // NARC
        NSIF, // SNT (PS2)
        NUIF, // SNT (PSP)
        ONE,  // ONE
        PVM,  // PVM
        SNT,  // SNT
        SPK,  // SPK
        TEX,  // TEX
        VDD,  // VDD
    }

    /* 4 byte header for each archive format */
    public enum ArchiveHeader : uint
    {
        ACX  = 0x00000000, // (ACX) 0x00, 0x00, 0x00, 0x00
        AFS  = 0x00534641, // (AFS) 0x41, 0x46, 0x53, 0x00
        MRG  = 0x3047524D, // (MRG) 0x4D, 0x52, 0x47, 0x30
        GNT  = 0x4649474E, // (GNT) 0x4E, 0x47, 0x49, 0x46
        GVM  = 0x484D5647, // (GVM) 0x47, 0x56, 0x4D, 0x48
        NARC = 0x4352414E, // (NARC) 0x4E, 0x41, 0x52, 0x43
        NGTL = 0x4C54474E, // (GNT) 0x4E, 0x47, 0x54, 0x4C (not GNCS)
        NSIF = 0x4649534E, // (SNT) 0x4E, 0x53, 0x49, 0x46 (PS2)
        NSTL = 0x4C54534E, // (SNT) 0x4E, 0x53, 0x54, 0x4C (PS2, not SNC)
        NUIF = 0x4649554E, // (SNT) 0x4E, 0x55, 0x49, 0x46 (PSP)
        NUTL = 0x4C54554E, // (SNT) 0x4E, 0x55, 0x54, 0x4C (PSP, not SNC)
        ONE  = 0x2E656E6F, // (ONE) 0x6F, 0x6E, 0x65, 0x2E
        PVM  = 0x484D5650, // (PVM) 0x50, 0x56, 0x4D, 0x48
        SPK  = 0x30444E53, // (SPK) 0x53, 0x4E, 0x44, 0x30
        TEX  = 0x30584554, // (TEX) 0x54, 0x45, 0x58, 0x30
    }

    /* Image Format ID */
    /*public enum GraphicFormat : byte
    {
        NULL, // Unknown Image Format
        GIM,  // GIM/MIG
        GMP,  // GMP
        GVP,  // GVP
        GVR,  // GVR
        PVP,  // PVP
        PVR,  // PVR
        SVP,  // SVP
        SVR,  // SVR
        
    }*/

    /* 4 bytes for each image format */
    /*public enum GraphicHeader : uint
    {
        GBIX = 0x58494247, // (GBIX) 0x47, 0x42, 0x49, 0x58
        GIM  = 0x4D49472E, // (GIM) 0x2E, 0x47, 0x49, 0x4D
        GMP  = 0x2D504D47, // (GMP) 0x47, 0x4D, 0x50, 0x2D
        GMP2 = 0x00303032, // (GMP2) 0x32, 0x30, 0x30, 0x00
        GVP  = 0x4C505650, // (GVP) 0x47, 0x56, 0x50, 0x58
        GVR  = 0x58494347, // (GVR) 0x47, 0x43, 0x49, 0x58
        MIG  = 0x2E47494D, // (GIM) 0x4D, 0x49, 0x47, 0x2E (GIM, Big Endian)
        PVP  = 0x4C505650, // (PVP) 0x50, 0x56, 0x50, 0x4C (Pallete data for PVR. Also SVP)
        PVR  = 0x58494247, // (PVR) 0x47, 0x42, 0x49, 0x58 (Also SVR)
        PVRT = 0x54525650, // (PVR) 0x50, 0x56, 0x52, 0x54 (PVRT)
    }*/

    /* File Headers */
    public class FileHeader
    {
        /* Compression Formats */
        public const string
            CNX  = "CNX\x02",
            LZ00 = "LZ00",
            LZ01 = "LZ01",
            ONZ  = "\x11";

        /* Archive Formats */
        public const string
            AFS  = "AFS",
            GVM  = "GVMH",
            MRG  = "MRG0",
            NARC = "NARC",
            NGTL = "NGTL",
            NSTL = "NSTL",
            NUTL = "NUTL",
            ONE  = "one.",
            PVM  = "PVMH",
            SPK  = "SPK0",
            TEX  = "TEX0";

        /* Image Formats */
        public const string
            GBIX = "GBIX",
            GCIX = "GCIX",
            GIM  = ".GIM1.00",
            GMP  = "GMP-200",
            GVPL = "GVPL",
            GVRT = "GVRT",
            MIG  = "MIG.00.1",
            PVPL = "PVPL",
            PVRT = "PVRT";
    }

    /* Output Directories */
    public class OutputDirectory
    {
        public const string
            ACX  = "ACX Extracted",
            AFS  = "AFS Extracted",
            GNT  = "GNT Extracted",
            GVM  = "GVM Extracted",
            MRG  = "MRG Extracted",
            NARC = "NARC Extracted",
            ONE  = "ONE Extracted",
            PVM  = "PVM Extracted",
            SNT  = "SNT Extracted",
            SPK  = "SPK Extracted",
            TEX  = "TEX Extracted",
            VDD  = "VDD Extracted";

        public const string
            GIM = "GIM Converted",
            GVR = "GVR Converted";
    }

    /* File Formats */
    public class FileFormat
    {
        /* Return the archive format */
        public static ArchiveFormat getArchiveFormat(byte[] data, string archiveExt)
        {
            /* Get the header of the archive. */
            ArchiveHeader header = (ArchiveHeader)BitConverter.ToUInt32(data, 0x0);

            /* Get the extension of the filename. */
            archiveExt = Path.GetExtension(archiveExt).ToLower();

            if (header == ArchiveHeader.ACX && archiveExt == ".acx")
                return ArchiveFormat.ACX; // ACX Archive

            else if (header == ArchiveHeader.AFS)
                return ArchiveFormat.AFS; // AFS Archive

            else if (header == ArchiveHeader.GNT && (ArchiveHeader)BitConverter.ToUInt32(data, 0x20) == ArchiveHeader.NGTL)
                return ArchiveFormat.GNT; // GNT Archive

            else if (header == ArchiveHeader.MRG)
                return ArchiveFormat.MRG; // MRG Archive

            else if (header == ArchiveHeader.ONE)
                return ArchiveFormat.ONE; // ONE Archive

            else if (header == ArchiveHeader.NSIF && (ArchiveHeader)BitConverter.ToUInt32(data, 0x20) == ArchiveHeader.NSTL)
                return ArchiveFormat.NSIF; // SNT Archive (PS2)

            else if (header == ArchiveHeader.NUIF && (ArchiveHeader)BitConverter.ToUInt32(data, 0x20) == ArchiveHeader.NUTL)
                return ArchiveFormat.NUIF; // SNT Archive (PSP)

            else if (header == ArchiveHeader.SPK)
                return ArchiveFormat.SPK; // SPK Archive

            else if (header == ArchiveHeader.TEX)
                return ArchiveFormat.TEX; // TEX Archive

            else if (archiveExt == ".vdd")
                return ArchiveFormat.VDD; // VDD Archive

            return ArchiveFormat.NULL;
        }


        /* Get the file format. */
        public static string getFileExtension(byte[] data, string archiveExt)
        {
            /* Get the extension of the filename. */
            archiveExt = Path.GetExtension(archiveExt).ToLower();

            /* Archives */
            switch (getArchiveFormat(data, archiveExt))
            {
                case ArchiveFormat.ACX:  return ".acx";
                case ArchiveFormat.AFS:  return ".afs";
                case ArchiveFormat.GNT:  return ".gnt";
                case ArchiveFormat.MRG:  return ".mrg";
                case ArchiveFormat.NSIF:
                case ArchiveFormat.NUIF: return ".snt";
                case ArchiveFormat.ONE:  return ".one";
                case ArchiveFormat.SPK:  return ".spk";
                case ArchiveFormat.TEX:  return ".tex";
                case ArchiveFormat.VDD:  return ".vdd";
            }

            /* ADX file */
            if (BitConverter.ToUInt16(data, 0x0) == 0x80 && archiveExt == ".acx")
                return ".adx";

            return ".bin";
        }

        /* Get the archive format */
        public static ArchiveFormat Archive(Stream data, string fileExt)
        {
            try
            {
                /* Let's check for archive formats based on the headers first */
                switch ((ArchiveHeader)ObjectConverter.StreamToUInt(data, 0x0))
                {
                    case ArchiveHeader.AFS:  return ArchiveFormat.AFS;  // AFS
                    case ArchiveHeader.GVM:  return ArchiveFormat.GVM;  // GVM
                    case ArchiveHeader.MRG:  return ArchiveFormat.MRG;  // MRG
                    case ArchiveHeader.NARC: return ArchiveFormat.NARC; // NARC
                    case ArchiveHeader.ONE:  return ArchiveFormat.ONE;  // ONE
                    case ArchiveHeader.PVM:  return ArchiveFormat.PVM;  // PVM
                    case ArchiveHeader.SPK:  return ArchiveFormat.SPK;  // SPK
                    case ArchiveHeader.TEX:  return ArchiveFormat.TEX;  // TEX

                    /* GNT and SNT check */
                    case ArchiveHeader.GNT:
                    case ArchiveHeader.NSIF:
                    case ArchiveHeader.NUIF:
                        switch ((ArchiveHeader)ObjectConverter.StreamToUInt(data, 0x20))
                        {
                            case ArchiveHeader.NGTL: return ArchiveFormat.GNT;  // GNT
                            case ArchiveHeader.NSTL: return ArchiveFormat.NSIF; // SNT (PS2)
                            case ArchiveHeader.NUTL: return ArchiveFormat.NUIF; // SNT (PSP)
                        } break;
                }

                /* Now let's check based on file extension */
                switch (Path.GetExtension(fileExt).Substring(1).ToLower())
                {
                    case "acx": return ArchiveFormat.ACX; // ACX
                    case "vdd": return ArchiveFormat.VDD; // VDD
                }

                /* Ok, we couldn't find the archive format */
                return ArchiveFormat.NULL;
            }
            catch
            {
                /* Ok, something went wrong */
                return ArchiveFormat.NULL;
            }
        }

        /* Get the image format */
        public static GraphicFormat Image(Stream data, string fileExt)
        {
            try
            {
                /* Let's check for image formats based on the headers first */
                switch ((GraphicHeader)ObjectConverter.StreamToUInt(data, 0x0))
                {
                    case GraphicHeader.GIM: return GraphicFormat.GIM; // GIM (Big Endian)
                    case GraphicHeader.MIG: return GraphicFormat.GIM; // GIM (Little Endian)
                }

                /* Ok, do special checks now */

                /* PVR file */
                if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GBIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x19, 1)[0] < 64)
                    return GraphicFormat.PVR;
                else if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x9, 1)[0] < 64)
                    return GraphicFormat.PVR;

                /* GVR File */
                if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GBIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.GVRT)
                    return GraphicFormat.GVR;
                else if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GCIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.GVRT)
                    return GraphicFormat.GVR;
                else if (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GVRT)
                    return GraphicFormat.GVR;

                return GraphicFormat.NULL;
            }
            catch
            {
                /* Ok, something went wrong */
                return GraphicFormat.NULL;
            }
        }

        /* Get the file extension */
        public static string GetExtension(Stream data)
        {
            /* Image Format */
            switch (Image(data, null))
            {
                case GraphicFormat.GIM: return ".gim";
                case GraphicFormat.GVR: return ".gvr";
                case GraphicFormat.PVR: return ".pvr";
                //case GraphicFormat.SVR: return ".svr";
            }

            return String.Empty;
        }
    }
}