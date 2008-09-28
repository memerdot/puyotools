using System;
using System.IO;

namespace puyo_tools
{
    /* Compression Format ID */
    public enum CompressionFormat : byte
    {
        CNX  = 0x1, // CNX
        CXLZ = 0x2, // CXLZ
        LZ00 = 0x3, // LZ00
        LZ01 = 0x4, // LZ01
        NULL = 0x0  // Unknown Compression
    }

    /* 4 byte header for each compression format */
    public enum CompressionHeader : uint
    {
        CNX  = 0x02584E43, // (CNX)  0x43, 0x4E, 0x58, 0x02
        CXLZ = 0x5A4C5843, // (CXLZ) 0x43, 0x58, 0x4C, 0x5A
        LZ00 = 0x30305A4C, // (LZ00) 0x4C, 0x5A, 0x30, 0x30
        LZ01 = 0x31305A4C  // (LZ01) 0x4C, 0x5A, 0x30, 0x31
    }

    /* Archive format ID */
    public enum ArchiveFormat : byte
    {
        ACX  = 0x1, // ACX
        AFS  = 0x2, // AFS
        GNT  = 0x3, // GNT
        MRG  = 0x4, // MRG
        NSIF = 0x5, // SNT (PS2)
        NUIF = 0x6, // SNT (PSP)
        SPK  = 0x7, // SPK
        TEX  = 0x8, // TEX
        VDD  = 0x9, // VDD
        NULL = 0x0  // Unknown Archive Format
    }

    /* 4 byte header for each archive format */
    public enum ArchiveHeader : uint
    {
        ACX  = 0x00000000, // (ACX) 0x00, 0x00, 0x00, 0x00
        AFS  = 0x00534641, // (AFS) 0x41, 0x46, 0x53, 0x00
        MRG  = 0x3047524D, // (MRG) 0x4D, 0x52, 0x47, 0x30
        GNT  = 0x4649474E, // (GNT) 0x4E, 0x47, 0x49, 0x46
        NGTL = 0x4C54474E, // (GNT) 0x4E, 0x47, 0x54, 0x4C (not GNCS)
        NSIF = 0x4649434E, // (SNT) 0x4E, 0x43, 0x49, 0x46 (PS2)
        NSTL = 0x3C54534E, // (SNT) 0x4E, 0x53, 0x54, 0x3C (PS2, not SNC)
        NUIF = 0x4649554E, // (SNT) 0x4E, 0x55, 0x49, 0x46 (PSP)
        NUTL = 0x4C54554E, // (SNT) 0x4E, 0x55, 0x54, 0x4C (PSP, not SNC)
        SPK  = 0x30444E53, // (SPK) 0x53, 0x4E, 0x44, 0x30
        TEX  = 0x30584553  // (TEX) 0x53, 0x45, 0x58, 0x30
    }

    /* Image Format ID */
    public enum GraphicFormat : byte
    {
        GIM  = 0x1, // GIM/MIG
        GVR  = 0x2, // GVR
        PVP  = 0x3, // PVP
        PVR  = 0x4, // PVR
        SVP  = 0x5, // SVP
        SVR  = 0x6, // SVR
        NULL = 0x0  // Unknown Image Format
    }

    /* 4 bytes for each image format */
    public enum GraphicHeader : uint
    {
        GIM  = 0x4D49472E, // (GIM) 0x2E, 0x47, 0x49, 0x4D
        GVR  = 0x58494347, // (GVR) 0x47, 0x43, 0x49, 0x58
        MIG  = 0x2E47494D, // (GIM) 0x4D, 0x49, 0x47, 0x2E (GIM, Big Endian)
        PVP  = 0x4C505650, // (PVP) 0x50, 0x56, 0x50, 0x4C (Pallete data for PVR. Also SVP)
        PVR  = 0x58494247, // (PVR) 0x47, 0x42, 0x49, 0x58 (Also SVR)
        PVRT = 0x54525650  // (PVR) 0x50, 0x56, 0x52, 0x54 (PVRT)
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

        /* Return the compression format */
        public static CompressionFormat getCompressionFormat(byte[] data)
        {
            /* Get the header of the compression. */
            CompressionHeader header = (CompressionHeader)BitConverter.ToUInt32(data, 0x0);

            switch (header)
            {
                case CompressionHeader.CNX:  return CompressionFormat.CNX;
                case CompressionHeader.CXLZ: return CompressionFormat.CXLZ;
                case CompressionHeader.LZ00: return CompressionFormat.LZ00;
                case CompressionHeader.LZ01: return CompressionFormat.LZ01;
            }

            return CompressionFormat.NULL;
        }

        /* Return the image format */
        public static GraphicFormat getImageFormat(byte[] data, string archiveExt)
        {
            /* Get the header of the archive. */
            GraphicHeader header = (GraphicHeader)BitConverter.ToUInt32(data, 0x0);

            /* Get the extension of the filename. */
            archiveExt = Path.GetExtension(archiveExt).ToLower();

            if (header == GraphicHeader.GIM || header == GraphicHeader.MIG)
                return GraphicFormat.GIM; // GIM (MIG)

            else if (header == GraphicHeader.GVR)
                return GraphicFormat.GVR; // GVR

            else if (archiveExt == ".pvp")
                return GraphicFormat.PVP; // PVP

            else if (archiveExt == ".svp")
                return GraphicFormat.SVP; // SVP

            else if (header == GraphicHeader.PVRT)
                return GraphicFormat.PVR; // PVR

            else if (header == GraphicHeader.PVR && archiveExt == ".pvr")
                return GraphicFormat.PVR; // PVR

            else if (header == GraphicHeader.PVR)
                return GraphicFormat.SVR; // SVR

            return GraphicFormat.NULL;
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
                case ArchiveFormat.SPK:  return ".spk";
                case ArchiveFormat.TEX:  return ".tex";
                case ArchiveFormat.VDD:  return ".vdd";
            }

            /* Image Format */
            switch (getImageFormat(data, archiveExt))
            {
                case GraphicFormat.GIM: return ".gim";
                case GraphicFormat.GVR: return ".gvr";
                case GraphicFormat.PVP: return ".pvp";
                case GraphicFormat.PVR: return ".pvr";
                case GraphicFormat.SVP: return ".svp";
                case GraphicFormat.SVR: return ".svr";
            }

            /* ADX file */
            if (BitConverter.ToUInt16(data, 0x0) == 0x80 && archiveExt == ".acx")
                return ".adx";

            return ".bin";
        }
    }
}