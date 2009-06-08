using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using VrSharp;
using ImgSharp;

namespace puyo_tools
{
    /* GVR Images */
    class GVR : ImageClass
    {
        public GVR()
        {
        }

        /* Convert the GVR to an image */
        public override Bitmap Unpack(ref Stream data, Stream palette)
        {
            /* Convert the GVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(StreamConverter.ToByteArray(data, 0, (int)data.Length), (palette == null ? null : StreamConverter.ToByteArray(palette, 0, (int)palette.Length)));
                ImgFile imageOutput = new ImgFile(imageInput.GetDecompressedData(), imageInput.GetWidth(), imageInput.GetHeight(), ImageFormat.Png);

                return new Bitmap(new MemoryStream(imageOutput.GetCompressedData()));
            }
            catch (VrCodecHeaderException)
            {
                throw new GraphicFormatNeedsPalette();
            }
            catch
            {
                return null;
            }
        }
        public override Bitmap Unpack(ref Stream data)
        {
            return Unpack(ref data, null);
        }

        public override Stream Pack(ref Stream data)
        {
            return null;
        }

        /* Check to see if this is a GVR */
        public override bool Check(ref Stream input)
        {
            try
            {
                return ((input.ReadString(0x0, 4) == GraphicHeader.GBIX && input.ReadString(0x10, 4) == GraphicHeader.GVRT) ||
                    (input.ReadString(0x0, 4) == GraphicHeader.GCIX && input.ReadString(0x10, 4) == GraphicHeader.GVRT) ||
                    (input.ReadString(0x0, 4) == GraphicHeader.GVRT));
            }
            catch
            {
                return false;
            }
        }

        /* Image Information */
        public override Images.Information Information()
        {
            string Name   = "GVR";
            string Ext    = ".gvr";
            string Filter = "GVR Image (*.gvr)|*.gvr";

            bool Unpack = true;
            bool Pack   = false;

            return new Images.Information(Name, Unpack, Pack, Ext, Filter);
        }
    }
}