using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using VrSharp;
using ImgSharp;

namespace puyo_tools
{
    /* PVR Images */
    class PVR : ImageModule
    {
        public PvrPixelFormat PaletteFormat = PvrPixelFormat.Argb1555;
        public PvrDataFormat DataFormat = PvrDataFormat.SquareTwiddled;
        public bool GbixHeader = true;
        public uint GlobalIndex = 0;

        public PVR()
        {
            Name      = "PVR";
            Extension = ".pvr";
            CanEncode = true;
            CanDecode = true;
        }

        /* Convert the PVR to an image */
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the PVR to an image */
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
            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show(f.ToString());
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
            /* Convert the image to a PVR */
            /* Right now Argb1555 Square Twiddled */
            try
            {
                ImgFile imageInput         = new ImgFile(data.ReadBytes(0, (int)data.Length));
                PvrFileEncoder imageOutput = new PvrFileEncoder(imageInput.GetDecompressedData(), imageInput.GetWidth(), imageInput.GetHeight(), PaletteFormat, DataFormat, GbixHeader, GlobalIndex);

                return new MemoryStream(imageOutput.GetCompressedData());
            }
            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show(f.ToString());
                return null;
            }
        }

        // External Palette Filename
        public override string PaletteFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".pvp";
        }

        /* Check to see if this is a PVR */
        public override bool Check(ref Stream input, string filename)
        {
            try
            {
                return ((input.ReadString(0x0, 4) == GraphicHeader.GBIX && input.ReadString(0x10, 4) == GraphicHeader.PVRT && input.ReadByte(0x19) < 0x60) ||
                    (input.ReadString(0x0, 4) == GraphicHeader.PVRT && input.ReadByte(0x9) < 0x60));
            }
            catch
            {
                return false;
            }
        }
    }
}