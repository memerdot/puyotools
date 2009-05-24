using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using VrSharp;
using ImgSharp;

namespace puyo_tools
{
    /* SVR Images */
    class SVR : ImageClass
    {
        public SVR()
        {
        }

        /* Convert the SVR to an image */
        public override Bitmap Unpack(ref Stream data, Stream palette)
        {
            /* Convert the SVR to an image */
            try
            {
                VrFile imageInput = new VrFile(StreamConverter.ToByteArray(data, 0, (int)data.Length), (palette == null ? null : StreamConverter.ToByteArray(palette, 0, (int)palette.Length)));
                ImgFile imageOutput = new ImgFile(imageInput.GetDecompressedData(), imageInput.GetWidth(), imageInput.GetHeight(), ImageFormat.Png);

                return new Bitmap(new MemoryStream(imageOutput.GetCompressedData()));
            }
            catch (VrCodecNeedsPaletteException)
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

        public override Stream Pack(ref Bitmap data)
        {
            return null;
        }

        /* Check to see if this is a SVR */
        public override bool Check(ref Stream input)
        {
            try
            {
                return ((input.ReadString(0x0, 4) == GraphicHeader.GBIX && input.ReadString(0x10, 4) == GraphicHeader.PVRT && input.ReadByte(0x19) >= 0x60) ||
                    (input.ReadString(0x0, 4) == GraphicHeader.PVRT && input.ReadByte(0x9) >= 0x60));
            }
            catch
            {
                return false;
            }
        }

        /* Image Information */
        public override Images.Information Information()
        {
            string Name   = "SVR";
            string Ext    = ".svr";
            string Filter = "SVR Image (*.svr)|*.svr";

            bool Unpack = true;
            bool Pack   = false;

            return new Images.Information(Name, Unpack, Pack, Ext, Filter);
        }
    }
}