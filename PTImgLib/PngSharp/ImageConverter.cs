using System;
using System.IO;
using System.Drawing;

namespace ImgSharp
{
    class ImageConverter
    {
        // A little cheating here - I took this code when my own was failing.
        // Turns out it wasn't the problem, but it's convinient to have this
        // in function form ;)
        static public byte[] imageToByteArray(System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat fmt)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
        static public Image byteArrayToImage(byte[] byteArrayIn, ref System.Drawing.Imaging.ImageFormat fmt)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            fmt = returnImage.RawFormat;
            return returnImage;
        }
    }
}