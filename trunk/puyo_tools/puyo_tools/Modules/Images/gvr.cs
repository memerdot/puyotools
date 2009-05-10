using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
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
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the GVR to an image */
            try
            {
                VrFile imageInput   = new VrFile(StreamConverter.ToByteArray(data, 0, (int)data.Length));
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