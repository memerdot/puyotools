using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
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
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the SVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(ObjectConverter.StreamToBytes(data, 0, (int)data.Length));
                ImgFile imageOutput = new ImgFile(imageInput.GetDecompressedData(), imageInput.GetWidth(), imageInput.GetHeight(), ImageFormat.Png);

                return new Bitmap(new MemoryStream(imageOutput.GetCompressedData()));
            }
            catch
            {
                return null;
            }
        }

        public override Stream Pack(ref Bitmap data)
        {
            return null;
        }
    }
}