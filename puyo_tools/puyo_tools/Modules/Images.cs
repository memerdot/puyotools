using System;
using System.IO;
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
                //switch ((GraphicHeader)ObjectConverter.StreamToUInt(data, 0x0))
                switch (StreamConverter.ToString(data, 0x0, 12))
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

                /* PVR file */
                if ((StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.GBIX && StreamConverter.ToString(data, 0x10, 4) == GraphicHeader.PVRT && StreamConverter.ToByte(data, 0x19) < 64) ||
                    (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.PVRT && StreamConverter.ToByte(data, 0x9) < 64))
                {
                    format    = GraphicFormat.PVR;
                    //converter = new PVR();
                    converter = null;
                    name      = "PVR";
                    ext       = ".pvr";
                    return;
                }

                /* GVR File */
                if ((StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.GBIX && StreamConverter.ToString(data, 0x10, 4) == GraphicHeader.GVRT) ||
                    (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.GCIX && StreamConverter.ToString(data, 0x10, 4) == GraphicHeader.GVRT) ||
                    (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.GVRT))
                {
                    format    = GraphicFormat.GVR;
                    converter = new GVR();
                    name      = "GVR";
                    ext       = ".gvr";
                    return;
                }

                /* SVR File */
                if ((StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.GBIX && StreamConverter.ToString(data, 0x10, 4) == GraphicHeader.PVRT && StreamConverter.ToByte(data, 0x19) > 64) ||
                    (StreamConverter.ToString(data, 0x0, 4) == GraphicHeader.PVRT && StreamConverter.ToByte(data, 0x9) > 64))
                {
                    format    = GraphicFormat.SVR;
                    converter = new SVR();
                    name      = "SVR";
                    ext       = ".svr";
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
            GVRT = "GVRT",
            MIG  = "MIG.00.1PSP\x00",
            PVRT = "PVRT";
    }

    public abstract class ImageClass
    {
        /* Image Functions */
        public abstract Bitmap Unpack(ref Stream data); // Unpack image
        public abstract Stream Pack(ref Bitmap data);   // Pack Image
    }
}