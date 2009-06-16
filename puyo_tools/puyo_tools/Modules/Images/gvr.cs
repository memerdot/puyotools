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
    class GVR : ImageModule
    {
        public GVR()
        {
            Name      = "GVR";
            Extension = ".gvr";
            CanEncode = false;
            CanDecode = true;
        }

        /* Convert the GVR to an image */
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the GVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(data.ToByteArray(), (PaletteData == null ? null : PaletteData.ToByteArray()));
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

        public override Stream Pack(ref Stream data)
        {
            return null;
        }

        // External Palette Filename
        public override string PaletteFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".gvp";
        }

        /* Check to see if this is a GVR */
        public override bool Check(ref Stream input, string filename)
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
    }
}