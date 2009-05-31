using System;
using System.IO;
using Extensions;
using System.Drawing;

/* Archive Module */
namespace puyo_tools
{
    public class Images
    {
        /* Image format */
        private ImageClass Converter = null;
        public GraphicFormat Format  = GraphicFormat.NULL;
        private Stream Data          = null;
        private Bitmap imageData     = null;
        public string ImageName      = null;
        private string FileExt       = null;

        /* Image Object for unpacking */
        public Images(Stream dataStream, string dataFilename)
        {
            /* Set up our image information */
            Data = dataStream;

            ImageInformation(ref Data, out Format, out Converter, out ImageName, out FileExt);
        }

        /* Unpack image */
        public Bitmap Unpack()
        {
            return Converter.Unpack(ref Data);
        }
        public Bitmap Unpack(Stream palette)
        {
            return Converter.Unpack(ref Data, palette);
        }

        /* Pack image */
        public Stream Pack()
        {
            return Converter.Pack(ref imageData);
        }

        /* Output Directory */
        public string OutputDirectory
        {
            get
            {
                return (ImageName == null ? null : ImageName + " Converted");
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

        /* Get image information */
        private void ImageInformation(ref Stream data, out GraphicFormat format, out ImageClass converter, out string name, out string ext)
        {
            try
            {
                /* Let's check for image formats based on the 12 byte headers first */
                switch (data.ReadString(0x0, 12, false))
                {
                    case GraphicHeader.GIM: // GIM (Big Endian)
                    case GraphicHeader.MIG: // GIM (Little Endian)
                        format    = GraphicFormat.GIM;
                        converter = new GIM();
                        name      = "GIM";
                        ext       = ".gim";
                        return;
                }

                /* Ok, do special checks now */

                /* GVR File */
                if ((data.ReadString(0x0, 4) == GraphicHeader.GBIX && data.ReadString(0x10, 4) == GraphicHeader.GVRT) ||
                    (data.ReadString(0x0, 4) == GraphicHeader.GCIX && data.ReadString(0x10, 4) == GraphicHeader.GVRT) ||
                    (data.ReadString(0x0, 4) == GraphicHeader.GVRT))
                {
                    format    = GraphicFormat.GVR;
                    converter = new GVR();
                    name      = "GVR";
                    ext       = ".gvr";
                    return;
                }

                /* PVR file */
                if ((data.ReadString(0x0, 4) == GraphicHeader.GBIX && data.ReadString(0x10, 4) == GraphicHeader.PVRT && data.ReadByte(0x19) < 0x60) ||
                    (data.ReadString(0x0, 4) == GraphicHeader.PVRT && data.ReadByte(0x9) < 0x60))
                {
                    format    = GraphicFormat.PVR;
                    converter = new PVR();
                    name      = "PVR";
                    ext       = ".pvr";
                    return;
                }

                /* SVR File */
                if ((data.ReadString(0x0, 4) == GraphicHeader.GBIX && data.ReadString(0x10, 4) == GraphicHeader.PVRT && data.ReadByte(0x19) >= 0x60) ||
                    (data.ReadString(0x0, 4) == GraphicHeader.PVRT && data.ReadByte(0x9) >= 0x60))
                {
                    format    = GraphicFormat.SVR;
                    converter = new SVR();
                    name      = "SVR";
                    ext       = ".svr";
                    return;
                }

                /* GMP File */
                if (data.ReadString(0x0, 8, false) == "GMP-200\x00")
                {
                    format    = GraphicFormat.GMP;
                    converter = new GMP();
                    name      = "GMP";
                    ext       = ".gmp";
                    return;
                }

                /* Unknown or unsupported compression */
                throw new GraphicFormatNotSupported();
            }
            catch (GraphicFormatNotSupported)
            {
                /* Unknown or unsupported image format */
                format     = GraphicFormat.NULL;
                converter  = null;
                name       = null;
                ext        = null;
                return;
            }
            catch
            {
                /* An error occured. */
                format     = GraphicFormat.NULL;
                converter  = null;
                name       = null;
                ext        = null;
                return;
            }
        }

        /* Image Information */
        public class Information
        {
            public string Name = null;
            public string Ext = null;
            public string Filter = null;

            public bool Unpack = false;
            public bool Pack  = false;

            public Information(string name, bool unpack, bool pack, string ext, string filter)
            {
                Name   = name;
                Ext    = ext;
                Filter = filter;

                Unpack = unpack;
                Pack   = pack;
            }
        }
    }

    /* Image Format */
    public enum GraphicFormat : byte
    {
        NULL,
        GIM,
        GMP,
        GVR,
        PVR,
        SVR,
    }

    /* Image Header */
    public static class GraphicHeader
    {
        public const string
            GBIX = "GBIX",
            GCIX = "GCIX",
            GIM  = ".GIM1.00\x00PSP",
            GMP  = "GMP-200\x00",
            GVRT = "GVRT",
            MIG  = "MIG.00.1PSP\x00",
            PVRT = "PVRT";
    }

    public abstract class ImageClass
    {
        /* Image Functions */
        public abstract Bitmap Unpack(ref Stream data);   // Unpack image
        public abstract Stream Pack(ref Bitmap data);     // Pack Image
        public abstract bool Check(ref Stream data);      // Check Image
        public abstract Images.Information Information(); // Image Information
        public virtual Bitmap Unpack(ref Stream data, Stream palette) // Unpack image (with external palette file)
        {
            return null;
        }
    }
}