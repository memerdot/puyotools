using System;
using System.Collections.Generic;

namespace VrSharp
{
    // PVR Palette Formats
    public enum PvrPaletteFormat : byte
    {
        Argb1555 = 0x00,
        Rgb565   = 0x01,
        Argb4444 = 0x02,
        Yuv442   = 0x03,
        Bump     = 0x04,
    }

    // PVR Data Formats
    public enum PvrDataFormat : byte
    {
        Format01 = 0x01,
        SquareTwiddledMipMaps = 0x02,
        Vq = 0x03,
        VqMipMaps = 0x04,
        Format05 = 0x05,
        Index4 = 0x06,
        Format07 = 0x07,
        Index4ExternPalette = 0x08,
        Format09 = 0x09,
        RectangleStride = 0x0B,
        RectangleTwiddled = 0x0C,
        SmallVq = 0x10,
        SmallVqMipMaps = 0x11,
    }

    public abstract class PvrPaletteCodec : VrPaletteCodec
    {
        public PvrPaletteFormat Format;
    }

    public abstract class PvrDataCodec : VrDataCodec
    {
        public PvrDataFormat Format;
    }

    // PVR Palette Format Classes
    public class PvrPaletteCodec_Argb1555 : PvrPaletteCodec
    {
        public PvrPaletteCodec_Argb1555()
        {
            Decode = new PvrPaletteDecoder_Argb1555();
            Encode = null;
            Format = PvrPaletteFormat.Argb1555;
        }
    }
    public class PvrPaletteCodec_Rgb565 : PvrPaletteCodec
    {
        public PvrPaletteCodec_Rgb565()
        {
            Decode = new PvrPaletteDecoder_Rgb565();
            Encode = null;
            Format = PvrPaletteFormat.Rgb565;
        }
    }
    public class PvrPaletteCodec_Argb4444 : PvrPaletteCodec
    {
        public PvrPaletteCodec_Argb4444()
        {
            Decode = new PvrPaletteDecoder_Argb4444();
            Encode = null;
            Format = PvrPaletteFormat.Argb4444;
        }
    }

    // PVR Data Format Classes
    public class PvrDataCodec_01 : PvrDataCodec
    {
        public PvrDataCodec_01()
        {
            Decode = new PvrDataDecoder_01();
            Encode = null;
            Format = PvrDataFormat.Format01;
        }
    }
    public class PvrDataCodec_05 : PvrDataCodec
    {
        public PvrDataCodec_05()
        {
            Decode = new PvrDataDecoder_05();
            Encode = null;
            Format = PvrDataFormat.Format05;
        }
    }
    public class PvrDataCodec_07 : PvrDataCodec
    {
        public PvrDataCodec_07()
        {
            Decode = new PvrDataDecoder_07();
            Encode = null;
            Format = PvrDataFormat.Format07;
        }
    }
    public class PvrDataCodec_09 : PvrDataCodec
    {
        public PvrDataCodec_09()
        {
            Decode = new PvrDataDecoder_09();
            Encode = null;
            Format = PvrDataFormat.Format09;
        }
    }

    // VrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public static class PvrCodecs
    {
        private static bool init = false;

        public static Dictionary<byte, PvrPaletteCodec> PvrPaletteCodecs = new Dictionary<byte, PvrPaletteCodec>();
        public static Dictionary<byte, PvrDataCodec> PvrDataCodecs = new Dictionary<byte, PvrDataCodec>();

        // Initalize
        public static void Initalize()
        {
            // Add the Palette Formats
            PvrPaletteCodecs.Add(0x00, new PvrPaletteCodec_Argb1555());
            PvrPaletteCodecs.Add(0x01, new PvrPaletteCodec_Rgb565());
            PvrPaletteCodecs.Add(0x02, new PvrPaletteCodec_Argb4444());

            // Add the Data Formats
            PvrDataCodecs.Add(0x01, new PvrDataCodec_01());
            PvrDataCodecs.Add(0x05, new PvrDataCodec_05());
            PvrDataCodecs.Add(0x07, new PvrDataCodec_07());
            PvrDataCodecs.Add(0x09, new PvrDataCodec_09());

            init = true;
        }

        public static PvrPaletteCodec GetPaletteCodec(byte Codec)
        {
            if (!init) Initalize();

            if (PvrPaletteCodecs.ContainsKey(Codec))
                return PvrPaletteCodecs[Codec];

            return null;
        }
        public static PvrDataCodec GetDataCodec(byte Codec)
        {
            if (!init) Initalize();

            if (PvrDataCodecs.ContainsKey(Codec))
                return PvrDataCodecs[Codec];

            return null;
        }
    }
}