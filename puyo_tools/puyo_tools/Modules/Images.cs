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
        private string ImageName     = null;

        /* Image Object for unpacking */
        public Images(Stream dataStream, string dataFilename)
        {
            /* Set up our image information */
            Data = dataStream;

            ImageInformation(ref Data, out Format, out Converter, out ImageName);
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

        /* Get image information */
        private void ImageInformation(ref Stream data, out GraphicFormat format, out ImageClass converter, out string name)
        {
            try
            {
                /* Let's check for image formats based on the headers first */
                switch ((GraphicHeader)ObjectConverter.StreamToUInt(data, 0x0))
                {
                    case GraphicHeader.GIM: // GIM (Big Endian)
                    case GraphicHeader.MIG: // GIM (Little Endian)
                        format    = GraphicFormat.GIM;
                        converter = new GIM();
                        name      = "GIM";
                        return;
                }

                /* Ok, do special checks now */

                /* PVR file */
                //if ((ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GBIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x19, 1)[0] < 64) ||
                //    (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x9, 1)[0] < 64))
                //{
                //    format    = GraphicFormat.PVR;
                //    converter = new PVR();
                //    name      = "PVR";
                //    return;

                /* GVR File */
                if ((ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GBIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.GVRT) ||
                    (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GCIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.GVRT) ||
                    (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GVRT))
                {
                    format    = GraphicFormat.GVR;
                    converter = new GVR();
                    name      = "GVR";
                    return;
                }

                /* SVR File */
                if ((ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.GBIX && ObjectConverter.StreamToString(data, 0x10, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x19, 1)[0] > 64) ||
                    (ObjectConverter.StreamToString(data, 0x0, 4) == FileHeader.PVRT && ObjectConverter.StreamToBytes(data, 0x9, 1)[0] > 64))
                {
                    format = GraphicFormat.SVR;
                    converter = new SVR();
                    name = "SVR";
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
                return;
            }
            catch
            {
                /* An error occured. */
                format     = GraphicFormat.NULL;
                converter  = null;
                name       = null;
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
    public enum GraphicHeader : uint
    {
        NULL = 0x00000000,
        GBIX = 0x58494247,
        GIM = 0x4D49472E,
        GMP = 0x2D504D47,
        GVR = 0x58494347,
        MIG = 0x2E47494D,
        //PVRT = 0x54525650,
    }

    public abstract class ImageClass
    {
        /* Image Functions */
        public abstract Bitmap Unpack(ref Stream data); // Unpack image
        public abstract Stream Pack(ref Bitmap data);   // Pack Image
    }
}