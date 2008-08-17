// Main.cs
// Created by nmn at 03:44 08/14/2008
using System;
using System.IO;
using GvrSharp;
using ImgSharp;
using System.Collections;

namespace GvrConv
{
	class MainClass
	{
		private static string OutName;
		private static string InName;
        private static bool ToGvr = false;
		
		public static void Main(string[] args)
		{
            GvrFormat format = GvrFormat.Rgb_5a3_8x4;
			switch(args.Length)
			{
				case 0:
					Console.WriteLine("GvrConv for .NET\nUsage: " + System.AppDomain.CurrentDomain.ToString() + " <source> [output]");
				return;
				
				case 1:
					InName = args[0];
                    if(!File.Exists(InName))
                    {
                        Console.Write("File does not exist...");
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
                        Console.Write("File does not exist...");
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
            if (ToGvr)
            {
                ImgFile ImgIn = new ImgFile(InName);
                GvrFile GvrOut = new GvrFile(ImgIn.GetDecompressedData(), ImgIn.GetWidth(), ImgIn.GetHeight());
                FileStream FStream = new FileStream(OutName, FileMode.Create);
                FStream.Write(ImgOut.GetCompressedData(), 0, ImgOut.Length());
                FStream.Close();
            }
            else
            {
                string extension = OutName.Substring(OutName.Length - 4, 3);
                Console.WriteLine(extension);

                GvrFile GvrIn = new GvrFile(InName);
                ImgFile ImgOut = new ImgFile(GvrIn.GetDecompressedData(), GvrIn.GetWidth(), GvrIn.GetHeight());
                FileStream FStream = new FileStream(OutName, FileMode.Create);
                FStream.Write(ImgOut.GetCompressedData(), 0, ImgOut.Length());
                FStream.Close();
            }
		}
	}
}