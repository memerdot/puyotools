﻿// Main.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.
using System;
using System.IO;
using GvrSharp;
using ImgSharp;
using System.Collections;
using System.Timers;
using System.Diagnostics;

namespace GvrConv
{
	class MainClass
	{
		private static string OutName;
		private static string InName;
        private static bool ToGvr = false;
		
		public static void Main(string[] args)
		{
            Stopwatch sw = Stopwatch.StartNew();

            GvrFormat format = GvrFormat.Fmt1808;
			switch(args.Length)
			{
				case 0:
					Console.WriteLine("GvrConv for .NET\nUsage: " + "GvrConv" + " <source> [output]");
				return;
				
				case 1:
					InName = args[0];
                    if(!File.Exists(InName))
                    {
                        Console.Write("File does not exist...");
                        Console.ReadKey(true);
                        return;
                    }
                    if (GvrFile.IsGvr(args[0]))
                    {
                        ToGvr = false;
                    }
                    else
                    {
                        ToGvr = true;
                    }
					OutName = args[0];
                    OutName = OutName.Remove(OutName.Length - 4);
                    if(ToGvr)
                        OutName += ".gvr";
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
                    if (GvrFile.IsGvr(args[0]))
                    {
                        ToGvr = false;
                    }
                    else
                    {
                        ToGvr = true;
                    }
					OutName = args[1];
                    char[] delimit = new char[1];
                    delimit[0] = ':';
                    string[] namestrings = OutName.Split(delimit);
				break;
			}
            try
            {
                if (ToGvr)
                {
                    ImgFile ImgIn = new ImgFile(InName);
                    GvrFile GvrOut = new GvrFile(ImgIn.GetDecompressedData(), ImgIn.GetWidth(), ImgIn.GetHeight(), format);
                    FileStream FStream = new FileStream(OutName, FileMode.Create);
                    FStream.Write(GvrOut.GetCompressedData(), 0, GvrOut.CompressedLength());
                    FStream.Close();
                }
                else
                {
                    GvrFile GvrIn = new GvrFile(InName);
                    Console.WriteLine("Conversion Operation\nWidth: " + GvrIn.GetWidth() + " x Height: " + GvrIn.GetHeight());

                    System.Drawing.Imaging.ImageFormat ImgOutFmt = System.Drawing.Imaging.ImageFormat.Png;
                    ImgOutFmt = ImgFile.ImgFormatFromFilename(OutName);

                    ImgFile ImgOut = new ImgFile(GvrIn.GetDecompressedData(), GvrIn.GetWidth(), GvrIn.GetHeight(), ImgOutFmt);
                    FileStream FStream = new FileStream(OutName, FileMode.Create);
                    FStream.Write(ImgOut.GetCompressedData(), 0, ImgOut.CompressedLength());
                    FStream.Close();
                }
            }
            catch (GvrNoSuitableCodecException e)
            {
                Console.WriteLine("GvrNoSuitableCodecException recieved:");
                Console.WriteLine(e.Message);
            }
            sw.Stop();

            Console.WriteLine("\nOperation Completed!\nExecution Time: " + sw.ElapsedMilliseconds + " ms");
            Console.ReadKey(true);
		}
	}
}