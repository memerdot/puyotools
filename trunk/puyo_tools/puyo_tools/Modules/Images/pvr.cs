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
    class PVR : ImageClass
    {
        public PvrPaletteFormat PaletteFormat = PvrPaletteFormat.Argb1555;
        public PvrDataFormat DataFormat = PvrDataFormat.Format01;
        public bool GbixHeader = true;
        public uint GlobalIndex = 0;

        public PVR()
        {
        }

        /* Convert the PVR to an image */
        public override Bitmap Unpack(ref Stream data, Stream palette)
        {
            /* Convert the PVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(StreamConverter.ToByteArray(data, 0, (int)data.Length), (palette == null ? null : StreamConverter.ToByteArray(palette, 0, (int)palette.Length)));
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
        }
        public override Bitmap Unpack(ref Stream data)
        {
            return Unpack(ref data, null);
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

        /* Check to see if this is a PVR */
        public override bool Check(ref Stream input)
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

        /* Image Information */
        public override Images.Information Information()
        {
            string Name   = "PVR";
            string Ext    = ".pvr";
            string Filter = "PVR Image (*.pvr)|*.pvr";

            bool Unpack = true;
            bool Pack   = false;

            return new Images.Information(Name, Unpack, Pack, Ext, Filter);
        }
    }
}