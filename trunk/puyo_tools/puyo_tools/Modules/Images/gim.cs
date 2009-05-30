using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using GimSharp;
using ImgSharp;

namespace puyo_tools
{
    public class GIM : ImageClass
    {
        /* GIM Images */
        public GIM()
        {
        }

        /* Unpack a GIM into a Bitmap */
        public override Bitmap Unpack(ref Stream data)
        {
            /* Convert the GIM to an image */
            try
            {
                GimFile imageInput  = new GimFile(data.ReadBytes(0, (int)data.Length));
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

        /* Check to see if this is a GIM */
        public override bool Check(ref Stream input)
        {
            try
            {
                return (input.ReadString(0x0, 12, false) == GraphicHeader.MIG ||
                    input.ReadString(0x0, 12, false) == GraphicHeader.GIM);
            }
            catch
            {
                return false;
            }
        }

        /* Image Information */
        public override Images.Information Information()
        {
            string Name   = "GIM";
            string Ext    = ".gim";
            string Filter = "GIM Image (*.gim)|*.gim";

            bool Unpack = true;
            bool Pack   = false;

            return new Images.Information(Name, Unpack, Pack, Ext, Filter);
        }
    }
}