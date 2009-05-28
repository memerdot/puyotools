using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
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
            catch (Exception f)
            {
                System.Windows.Forms.MessageBox.Show(f.ToString());
                return null;
            }
        }

        public Bitmap Unpack2(ref Stream data)
        {
            try
            {
                /* Check to see if GimConv exists. */
                string gimConvFileName = "tools" + Path.DirectorySeparatorChar + "GimConv" + Path.DirectorySeparatorChar + "GimConv.exe";
                if (!File.Exists(gimConvFileName))
                    return null;

                /* Save the data to a temporary file. */
                string tempFileName = Path.GetTempFileName();
                tempFileName = Path.ChangeExtension(tempFileName, ".gim");

                /* Create the temporary output file. */
                string tempOutputFileName = Path.GetTempFileName();
                tempOutputFileName = Path.ChangeExtension(tempOutputFileName, ".png");

                FileStream tempFile = new FileStream(tempFileName, FileMode.Append, FileAccess.Write);
                tempFile.Write(StreamConverter.ToByteArray(data, 0, (int)data.Length), 0, (int)data.Length);
                tempFile.Close();

                /* Now, run GimConv to convert the file to a PNG. */

                /* Set up the process information. */
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle      = ProcessWindowStyle.Hidden;
                startInfo.FileName         = gimConvFileName;
                startInfo.Arguments        = String.Format("\"{0}\" -o \"{1}\"", tempFileName, tempOutputFileName);

                /* Run GimConv. */
                Process gimConv;
                gimConv = Process.Start(startInfo);
                gimConv.WaitForExit();
                gimConv.Close();

                /* Load the new output image, if it exists. */
                if (File.Exists(tempOutputFileName))
                {
                    /* Create a new Bitmap and return it. */
                    FileStream file = new FileStream(tempOutputFileName, FileMode.Open, FileAccess.Read);
                    //Bitmap image = new Bitmap(tempOutputFileName);
                    Bitmap image = new Bitmap(file);
                    file.Close();

                    /* Delete our temporary images. */
                    File.Delete(tempFileName);
                    File.Delete(tempOutputFileName);

                    return image;
                }

                /* There was an error converting to PNG. */
                File.Delete(tempFileName);

                return null;
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

        /* Pack a Bitmap into a GIM */
        public byte[] pack(Bitmap image, int imageFormatOption, int paletteFormatOption, int endianFormatOption)
        {
            try
            {
                /* Set Image Format */
                string imageFormat = String.Empty;

                switch (imageFormatOption)
                {
                    case 0: imageFormat = "-default";  break;
                    case 1: imageFormat = "-rgba8888"; break;
                    case 2: imageFormat = "-rgba4444"; break;
                    case 3: imageFormat = "-rgba5551"; break;
                    case 4: imageFormat = "-rgba5650"; break;
                    case 5: imageFormat = "-index4";   break;
                    case 6: imageFormat = "-index8";   break;
                    case 7: imageFormat = "-index16";  break;
                    case 8: imageFormat = "-index32";  break;
                }

                /* Set Palette Format */
                string paletteFormat = String.Empty;

                switch (paletteFormatOption)
                {
                    case 0: imageFormat = "-default";  break;
                    case 1: imageFormat = "-rgba8888"; break;
                    case 2: imageFormat = "-rgba4444"; break;
                    case 3: imageFormat = "-rgba5551"; break;
                    case 4: imageFormat = "-rgba5650"; break;
                }

                /* Set Endian */
                string endianFormat = (endianFormatOption == 1 ? "-B" : "");

                /* Save the data to a temporary file. */
                string tempFileName = Path.GetTempFileName();
                Path.ChangeExtension(tempFileName, ".png");

                /* Create the temporary output file. */
                string tempOutputFileName = Path.GetTempFileName();
                Path.ChangeExtension(tempOutputFileName, ".gim");

                /* Save the Bitmap to the temporary file. */
                image.Save(tempFileName, ImageFormat.Png);

                return new byte[0];
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}