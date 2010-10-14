using System;
using System.IO;
using Extensions;
using System.Drawing;
using System.Collections.Generic;

/* Archive Module */
namespace PuyoTools
{
    public class Images
    {
        /* Image format */
        public ImageModule Encoder { get; private set; }
        public ImageModule Decoder { get; private set; }

        public GraphicFormat Format = GraphicFormat.NULL;
        private Stream Data         = null;
        private string Filename     = null;
        public string ImageName     = null;
        private string FileExt      = null;

        // Image Dictionary
        public static Dictionary<GraphicFormat, ImageModule> Dictionary { get; private set; }

        // Set up image object for decoding
        public Images(Stream data, string filename)
        {
            // Set up information and initalize decompressor
            Decoder  = null;
            Data     = data;
            Filename = filename;

            InitalizeDecoder();
        }

        /* Unpack image */
        public Bitmap Unpack()
        {
            return Decoder.Unpack(ref Data);
        }

        /* Pack image */
        public Stream Pack()
        {
            //return Encoder.Pack(ref imageData);
            return null;
        }

        // External Palette Filename
        public string PaletteFilename
        {
            get
            {
                return Decoder.PaletteFilename(Filename);
            }
        }

        /* Output Directory */
        public string OutputDirectory
        {
            get
            {
                return (ImageName == null ? null : ImageName + " Converted");
            }
        }

        /* File Extension */
        public string FileExtension
        {
            get
            {
                return (FileExt == null ? String.Empty : FileExt);
            }
        }

        // Initalize Decoder
        private void InitalizeDecoder()
        {
            foreach (KeyValuePair<GraphicFormat, ImageModule> value in Dictionary)
            {
                if (Dictionary[value.Key].Check(ref Data, Filename))
                {
                    // This is the compression format
                    if (value.Value.CanDecode)
                    {
                        Format    = value.Key;
                        Decoder   = value.Value;
                        ImageName = Decoder.Name;
                        FileExt   = Decoder.Extension;
                    }

                    break;
                }
            }
        }

        // Initalize Compressor
        private void InitalizeEncoder()
        {
            // Get compressor based on compression format
            if (Dictionary.ContainsKey(Format) && Dictionary[Format].CanEncode)
            {
                Encoder   = Dictionary[Format];
                ImageName = Encoder.Name;
            }
        }

        // Initalize Image Dictionary
        public static void InitalizeDictionary()
        {
            Dictionary = new Dictionary<GraphicFormat, ImageModule>();

            // Add all the entries to the dictionary
            Dictionary.Add(GraphicFormat.GIM, new GIM());
            Dictionary.Add(GraphicFormat.GMP, new GMP());
            Dictionary.Add(GraphicFormat.GVR, new GVR());
            Dictionary.Add(GraphicFormat.PVR, new PVR());
            Dictionary.Add(GraphicFormat.SVR, new SVR());
        }
    }

    // Image Format
    public enum GraphicFormat
    {
        NULL,
        GIM,
        GMP,
        GVR,
        PVR,
        SVR,
    }

    // Image Header
    public static class GraphicHeader
    {
        public const string
            GBIX = "GBIX",
            GCIX = "GCIX",
            GIM  = ".GIM1.00\x00PSP",
            GMP  = "GMP-200\x00",
            GVRT = "GVRT",
            MIG  = "MIG.00.1PSP\x00",
            PVRT = "PVRT";
    }

    public abstract class ImageModule
    {
        // Variables
        public string Name      { get; protected set; }
        public string Extension { get; protected set; }
        public bool CanEncode   { get; protected set; }
        public bool CanDecode   { get; protected set; }

        // Other variables
        public Stream PaletteData   = null;
        public int PixelFormatIndex = -1;
        public int DataFormatIndex  = -1;

        // Image Functions
        public abstract Bitmap Unpack(ref Stream data); // Unpack image
        public abstract Stream Pack(ref Stream data);   // Pack Image
        public abstract bool Check(ref Stream data, string filename); // Check Image
        public virtual string PaletteFilename(string filename) // External Palette Filename
        {
            return filename;
        }
    }
}