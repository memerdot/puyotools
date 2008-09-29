using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace puyo_tools
{
    public class Conversions
    {
        public static void toPNG(byte[] data, string fileName)
        {
            /* is this a GIM? */
            if (Header.isFile(data, Header.GIM, 0) || Header.isFile(data, Header.MIG, 0) && File.Exists("tools" + Path.DirectorySeparatorChar + "GimConv" + Path.DirectorySeparatorChar + "GimConv.exe"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName         = "tools" + Path.DirectorySeparatorChar + "GimConv" + Path.DirectorySeparatorChar + "GimConv.exe";
                startInfo.Arguments        = "\"" + fileName + "\" -o \"" + Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".png\"";
                startInfo.WindowStyle      = ProcessWindowStyle.Hidden;

                Process gimConv;
                gimConv = Process.Start(startInfo);
                gimConv.WaitForExit();
                gimConv.Close();
            }

            /* Is this a GVR? */
            else if (Header.isFile(data, Header.GVR, 0) && File.Exists("tools" + Path.DirectorySeparatorChar + "GvrConv" + Path.DirectorySeparatorChar + "GvrConv.exe"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName         = "tools" + Path.DirectorySeparatorChar + "GvrConv" + Path.DirectorySeparatorChar + "GvrConv.exe";
                startInfo.Arguments        = "\"" + fileName + "\" \"" + Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".png\"";
                startInfo.WindowStyle      = ProcessWindowStyle.Hidden;

                Process GvrConv;
                GvrConv = Process.Start(startInfo);
                GvrConv.WaitForExit();
                GvrConv.Close();
            }

            /* GMP? */
            else if (FileFormat.getImageFormat(data, fileName) == GraphicFormat.GMP)
            {
                GMP gmp = new GMP();
                Bitmap image = gmp.unpack(data);
                image.Save(Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".png", ImageFormat.Png);
            }
        }

        /* It's time to make an ISO */
        public static string toISO(string dir)
        {
            if (File.Exists("tools" + Path.DirectorySeparatorChar + "mkisofs" + Path.DirectorySeparatorChar + "mkisofs.exe"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName         = "tools" + Path.DirectorySeparatorChar + "mkisofs" + Path.DirectorySeparatorChar + "mkisofs.exe";
                startInfo.Arguments        = "-l -o \"output_rofs.tmp\" \"" + dir + "\"";
                startInfo.WindowStyle      = ProcessWindowStyle.Hidden;

                Process mkisofs;
                mkisofs = Process.Start(startInfo);
                mkisofs.WaitForExit();
                mkisofs.Close();

                return "output_rofs.tmp";
            }
            else
                return "";
        }

        /* Hopefully we can convert it to a CVM */
        public static void toCVM(string fileName)
        {
            if (File.Exists("tools" + Path.DirectorySeparatorChar + "mkisofs" + Path.DirectorySeparatorChar + "rofs_header.bin"))
            {
                /* Create the new file */
                FileStream file = new FileStream("output_rofs.cvm", FileMode.Create);
                FileStream tmpFile = new FileStream("output_rofs.tmp", FileMode.Open);

                /* Write the header */
                FileStream header = new FileStream("tools" + Path.DirectorySeparatorChar + "mkisofs" + Path.DirectorySeparatorChar + "rofs_header.bin", FileMode.Open);
                byte[] headerData = new byte[(int)header.Length];
                header.Read(headerData, 0, headerData.Length);
                header.Close();
                file.Write(headerData, 0, headerData.Length);

                /* Now we write the file data, in chunks of 20MB */
                int incAmount = 1024 * 1024 * 20; // 20MB
                for (int i = 0; i < file.Length; i += incAmount)
                {
                    int readAmount;
                    if (i + incAmount >= file.Length)
                        readAmount = (int)file.Length - i;
                    else
                        readAmount = incAmount;

                    byte[] readData = new byte[readAmount];

                    tmpFile.Read(readData, 0, readAmount);
                    file.Write(readData, 0, readAmount);
                }

                /* Finish writing the files */
                file.Close();
                tmpFile.Close();

                /* Now delete the temporary file */
                File.Delete("output_rofs.tmp");
            }
        }
    }
}