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
    class SVR : ImageModule
    {
        public SVR()
        {
            Name      = "SVR";
            Extension = ".svr";
            CanEncode = false;
            CanDecode = true;
        }

        /* Convert the SVR to an image */
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the SVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(data.ToByteArray(), (PaletteData == null ? null : PaletteData.ToByteArray()));
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
            finally
            {
                // Reset palette data
                PaletteData = null;
            }
        }

        public override Stream Pack(ref Stream data)
        {
            return null;
        }

        // External Palette Filename
        public override string PaletteFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".svp";
        }

        /* Check to see if this is a SVR */
        public override bool Check(ref Stream input, string filename)
        {
            try
            {
                return ((input.ReadString(0x0, 4) == GraphicHeader.GBIX && input.ReadString(0x10, 4) == GraphicHeader.PVRT && input.ReadByte(0x19) >= 0x60 && input.ReadByte(0x19) < 0x70) ||
                    (input.ReadString(0x0, 4) == GraphicHeader.PVRT && input.ReadByte(0x9) >= 0x60 && input.ReadByte(0x9) < 0x70));
            }
            catch
            {
                return false;
            }
        }
    }
}