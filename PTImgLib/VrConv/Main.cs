// Main.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.
using System;
using System.IO;
using VrSharp;
using ImgSharp;
using System.Collections;
using System.Timers;
using System.Diagnostics;

namespace VrConv
{
	class MainClass
	{
		private static string OutName;
		private static string InName;
        private static bool ToGvr = false;
        private static bool ToSvr = false;
		
		public static void Main(string[] args)
		{
            Stopwatch sw = Stopwatch.StartNew();

            VrFormat format = VrFormat.Fmt00000004;
			switch(args.Length)
			{
                default:
				case 0:
					Console.WriteLine("VrConv for .NET\nUsage: " + "VrConv" + " <Source> [Output[:Vr Format]]");
                    Console.WriteLine("Source: Required. Input image.");
                    Console.WriteLine("Output: Optional. Output image.");
                    Console.WriteLine("Vr Format: Optional. Describes the VrHeader");
				return;
				
				case 1:
					InName = args[0];
                    if(!File.Exists(InName))
                    {
                        Console.Write("File does not exist...");
                        Console.ReadKey(true);
                        return;
                    }

                    if (VrFile.IsGvr(args[0]))
                    {
                        ToGvr = false;
                        ToSvr = false;
                    }
                    else if (VrFile.IsSvr(args[0]))
                    {
                        ToGvr = false;
                        ToSvr = false;
                    }
                    else
                    {
                        ToGvr = true;
                        ToSvr = false;
                    }

					OutName = args[0];
                    OutName = OutName.Remove(OutName.Length - 4);

                    if (ToGvr)
                        OutName += ".gvr";
                    else if (ToSvr)
                        OutName += ".svr";
                    else
                        OutName += ".png";
				break;
				
				case 2:
                    InName = args[0];
                    if (!File.Exists(InName))
                    {
                        Console.Write("File does not exist...\n" + InName);
                        Console.ReadKey(true);
                        return;
                    }
                    if (VrFile.IsGvr(args[0]))
                    {
                        ToGvr = false;
                        ToSvr = false;
                    }
                    else if (VrFile.IsSvr(args[0]))
                    {
                        ToGvr = false;
                        ToSvr = false;
                    }
                    else
                    {
                        ToGvr = true;
                        ToSvr = false;
                    }
					OutName = args[1];
                    char[] delimit = new char[1];
                    delimit[0] = ':';
                    string[] namestrings = OutName.Split(delimit);
                    if (namestrings.Length > 1)
                    {
                        format = VrCodecs.GetCodec(namestrings[1]).Format;
                        OutName = namestrings[0];
                    }
				break;
			}
            try
            {
                if (ToGvr)
                {
                    ImgFile ImgIn = new ImgFile(InName);
                    VrFile VrOut = new VrFile(ImgIn.GetDecompressedData(), ImgIn.GetWidth(), ImgIn.GetHeight(), format);
                    FileStream FStream = new FileStream(OutName, FileMode.Create);
                    FStream.Write(VrOut.GetCompressedData(), 0, VrOut.CompressedLength());
                    FStream.Close();
                }
                else
                {
                    VrFile VrIn = new VrFile(InName);
                    Console.WriteLine("Conversion Operation\nWidth: " + VrIn.GetWidth() + " x Height: " + VrIn.GetHeight());

                    System.Drawing.Imaging.ImageFormat ImgOutFmt = System.Drawing.Imaging.ImageFormat.Png;
                    ImgOutFmt = ImgFile.ImgFormatFromFilename(OutName);

                    ImgFile ImgOut = new ImgFile(VrIn.GetDecompressedData(), VrIn.GetWidth(), VrIn.GetHeight(), ImgOutFmt);
                    FileStream FStream = new FileStream(OutName, FileMode.Create);
                    FStream.Write(ImgOut.GetCompressedData(), 0, ImgOut.CompressedLength());
                    FStream.Close();
                }
            }
            catch (VrNoSuitableCodecException e)
            {
                Console.WriteLine("VrNoSuitableCodecException recieved:");
                Console.WriteLine(e.Message);
            }
            sw.Stop();

            Console.WriteLine("\nOperation Completed!\nExecution Time: " + sw.ElapsedMilliseconds + " ms");
            Console.ReadKey(true);
		}
	}
}