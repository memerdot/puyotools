using System;
using System.IO;
using System.Diagnostics;
using VrSharp.GvrTexture;
using VrSharp.PvrTexture;
using VrSharp.SvrTexture;

namespace VrConv
{
    class VrConv
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Vr Conv Usage:");
                Console.WriteLine("Decode: vrconv -d <input> [-o <output>] [-c <clut>] [-ac]");
                //Console.WriteLine("Encode: vrconv -e <input> <vrformat> <pixelfmt> <datafmt> [options]");
                Console.WriteLine("Help:   Decoding: vrconv -d /?");
                //Console.WriteLine("        Encoding: vrconv -e /? <vrformat (gvr/pvr/svr)>");

                Console.WriteLine();
                Console.WriteLine("------------------------");
                Console.WriteLine("Vr Conv Build:");
                Console.WriteLine("Ver {0} ({1})", GetVrConvVersion(), GetVrConvBuildDate());
            }
            else
            {
                if (args[0] == "-d")
                {
                    if (args.Length >= 2 && args[1] == "/?")
                        DecodingHelp();
                    else
                        DecodeVrTexture(args);
                }
                /*else if (args[0] == "-e")
                {
                    if (args.Length >= 3 && args[1] == "/?")
                        EncodingHelp(args[2]);
                    else
                        EncodeVrTexture(args);
                }*/
                else
                    Console.WriteLine("I don't know what you want to do!");
            }
        }

        static void DecodeVrTexture(string[] args)
        {
            // Get the command line arguments
            int OutFileArgIndex  = Array.IndexOf(args, "-o");
            int ClutArgIndex     = Array.IndexOf(args, "-c");
            int AutoClutArgIndex = Array.IndexOf(args, "-ac");

            // Get the strings in the command line arguments
            string InputFile  = args[1];
            string OutputFile = (OutFileArgIndex != -1 && OutFileArgIndex < args.Length ? args[OutFileArgIndex + 1] : Path.GetFileNameWithoutExtension(InputFile) + ".png");
            string ClutFile   = (ClutArgIndex != -1 && AutoClutArgIndex == -1 && ClutArgIndex < args.Length ? args[ClutArgIndex + 1] : null);

            string InputPath  = (Path.GetDirectoryName(InputFile) != String.Empty ? Path.GetDirectoryName(InputFile) + Path.DirectorySeparatorChar : String.Empty);
            string OutputPath = InputPath;

            // Load the data (as a byte array)
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("ERROR: {0} does not exist.", Path.GetFileNameWithoutExtension(args[1]));
                return;
            }
            byte[] VrData = new byte[0];
            using (BufferedStream stream = new BufferedStream(new FileStream(args[1], FileMode.Open, FileAccess.Read)))
            {
                VrData = new byte[stream.Length];
                stream.Read(VrData, 0x00, VrData.Length);
            }

            Console.WriteLine("Vr Conv");
            Console.WriteLine("------------------------");
            Console.WriteLine("Decoding: {0}", Path.GetFileName(InputFile));

            // Start the watch to see how long it takes to decode
            bool DecodeSuccess = false;
            MemoryStream BitmapData = null;
            Stopwatch timer = Stopwatch.StartNew();

            // Decode the data now
            if (GvrTexture.IsGvrTexture(VrData))
            {
                if (AutoClutArgIndex != -1)
                    ClutFile = InputPath + Path.GetFileNameWithoutExtension(InputFile) + ".gvp";

                DecodeSuccess = new VrDecoder.Gvr().DecodeTexture(VrData, ClutFile, out BitmapData);
            }
            else if (PvrTexture.IsPvrTexture(VrData))
            {
                if (AutoClutArgIndex != -1)
                    ClutFile = InputPath + Path.GetFileNameWithoutExtension(InputFile) + ".pvp";

                DecodeSuccess = new VrDecoder.Pvr().DecodeTexture(VrData, ClutFile, out BitmapData);
            }
            else if (SvrTexture.IsSvrTexture(VrData))
            {
                if (AutoClutArgIndex != -1)
                    ClutFile = InputPath + Path.GetFileNameWithoutExtension(InputFile) + ".svp";

                DecodeSuccess = new VrDecoder.Svr().DecodeTexture(VrData, ClutFile, out BitmapData);
            }
            else
                Console.WriteLine("ERROR: Not a Gvr, Pvr, or Svr texture.");

            // Was the data decoded successfully?
            if (DecodeSuccess && BitmapData != null)
            {
                using (BufferedStream stream = new BufferedStream(new FileStream(OutputPath + OutputFile, FileMode.Create, FileAccess.Write)))
                    BitmapData.WriteTo(stream);

                timer.Stop();
                Console.WriteLine("Texture decoded in {0} ms.", timer.ElapsedMilliseconds);
            }
            else if (DecodeSuccess && BitmapData == null)
                Console.WriteLine("ERROR: Unable to decode texture.");

            Console.WriteLine();
        }

        static void EncodeVrTexture(string[] args)
        {
        }

        static void DecodingHelp()
        {
            Console.WriteLine();
            Console.WriteLine(
                "\t-o <output> : Set output filename (default is <input>.png)\n" +
                "\t-c <clut>   : Sets the clut filename\n" +
                "\t-ac         : Auto find clut file using <input> filename.");
        }

        static void EncodingHelp(string format)
        {
            /*if (format.ToLower() == "gvr")
            {
                Console.WriteLine();
                Console.WriteLine(
                    "<pixelfmt> Pixel Formats:\n" +
                    "\ti8     : Intensity 8-bit\n" +
                    "\trgb565 : Rgb565\n" +
                    "\trgb5a3 : Rgb5a3\n" +
                    "\tnone   : Use if data format is not 4/8-bit Clut");
                Console.WriteLine();
                Console.WriteLine(
                    "<datafmt> Data Formats:\n" +
                    "\ti4       : Intensity 4-bit\n" +
                    "\ti8       : Intensity 8-bit\n" +
                    "\tia4      : Intensity 4-bit with Alpha\n" +
                    "\tia8      : Intensity 8-bit with Alpha\n" +
                    "\trgb565   : Rgb565\n" +
                    "\trgb5a3   : Rgb5a3\n" +
                    "\targb8888 : Argb8888\n" +
                    "\tindex4   : 4-bit Clut (set pixel format)\n" +
                    "\tindex8   : 8-bit Clut (set pixel format)\n" +
                    "\tcmp      : S3tc/Dxtn1 Compression");
                Console.WriteLine();
                Console.WriteLine(
                    "[options] Options:\n" +
                    "\t-o <output>  : Set output filename (default is <input>.gvr)\n" +
                    "\t-c <clut>    : Set filename of clut (default is <input>.gvp)\n" +
                    "\t-gi <gindex> : Sets the Global Index (default is 0)\n" +
                    "\t-gbix <type> : Sets the Gbix type\n" +
                    "\t               (gbix (gc), gcix (wii, default), none)");
            }

            else if (format.ToLower() == "svr")
            {
                Console.WriteLine();
                Console.WriteLine(
                    "<pixelfmt> Pixel Formats:\n" +
                    "\trgb5a3   : Rgb5a3\n" +
                    "\targb8888 : Argb8888");
                Console.WriteLine();
                Console.WriteLine(
                    "<datafmt> Data Formats:\n" +
                    "\trect : Rectangle\n" +
                    "\ti4ec : Index 4-bit w/ External Clut\n" +
                    "\ti8ec : Index 8-bit w/ External Clut\n" +
                    "\ti4   : Index 4-bit (will set proper format based on\n" +
                    "\t       pixel format and texture dimensions.\n" +
                    "\ti8   : Index 8-bit (will set proper format based on\n" +
                    "\t       pixel format and texture dimensions.");
                Console.WriteLine();
                Console.WriteLine(
                    "[options] Options:\n" +
                    "\t-o <output>  : Set output filename (default is <input>.gvr)\n" +
                    "\t-c <clut>    : Set filename of clut (default is <input>.gvp)\n" +
                    "\t-gi <gindex> : Sets the Global Index (default is 0)\n" +
                    "\t-gbix <type> : Sets the Gbix type\n" +
                    "\t               (gbix (gc), gcix (wii, default), none)");
            }*/
        }

        static Version GetVrConvVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        static DateTime GetVrConvBuildDate()
        {
            Version VrConvVersion = GetVrConvVersion();
            return new DateTime(2000, 1, 1).AddDays(VrConvVersion.Build).AddSeconds(VrConvVersion.Revision * 2);
        }
    }
}