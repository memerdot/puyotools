using System;
using System.Collections.Generic;

namespace VrSharp
{
    // PVR Palette Formats
    public enum PvrPixelFormat : byte
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
        SquareTwiddled        = 0x01,
        SquareTwiddledMipMaps = 0x02,
        Vq                    = 0x03,
        VqMipMaps             = 0x04,
        Index4                = 0x05,
        Index4MipMaps         = 0x06,
        Index8                = 0x07,
        Index8MipMaps         = 0x08,
        Rectangle             = 0x09,
        RectangleTwiddled     = 0x0D,
        SmallVq               = 0x10,
        SmallVqMipMaps        = 0x11,
    }

    public abstract class PvrPixelCodec : VrPaletteCodec
    {
        public PvrPixelFormat Format;
    }

    public abstract class PvrDataCodec : VrDataCodec
    {
        public PvrDataFormat Format;
    }

    // PVR Palette Format Classes
    public class PvrPaletteCodec_Argb1555 : PvrPixelCodec
    {
        public PvrPaletteCodec_Argb1555()
        {
            Decode = new PvrPaletteDecoder_Argb1555();
            Encode = new PvrPaletteEncoder_Argb1555();
            Format = PvrPixelFormat.Argb1555;
        }
    }
    public class PvrPaletteCodec_Rgb565 : PvrPixelCodec
    {
        public PvrPaletteCodec_Rgb565()
        {
            Decode = new PvrPaletteDecoder_Rgb565();
            Encode = new PvrPaletteEncoder_Rgb565();
            Format = PvrPixelFormat.Rgb565;
        }
    }
    public class PvrPaletteCodec_Argb4444 : PvrPixelCodec
    {
        public PvrPaletteCodec_Argb4444()
        {
            Decode = new PvrPaletteDecoder_Argb4444();
            Encode = new PvrPaletteEncoder_Argb4444();
            Format = PvrPixelFormat.Argb4444;
        }
    }

    // PVR Data Format Classes
    public class PvrDataCodec_SquareTwiddled : PvrDataCodec
    {
        public PvrDataCodec_SquareTwiddled()
        {
            Decode = new PvrDataDecoder_SquareTwiddled();
            Encode = new PvrDataEncoder_SquareTwiddled();
            Format = PvrDataFormat.SquareTwiddled;
        }
    }
    public class PvrDataCodec_Vq : PvrDataCodec
    {
        public PvrDataCodec_Vq()
        {
            Decode = new PvrDataDecoder_Vq();
            Encode = null;
            Format = PvrDataFormat.Vq;
        }
    }
    public class PvrDataCodec_VqMipMaps : PvrDataCodec
    {
        public PvrDataCodec_VqMipMaps()
        {
            Decode = new PvrDataDecoder_VqMipMaps();
            Encode = null;
            Format = PvrDataFormat.VqMipMaps;
        }
    }
    public class PvrDataCodec_Index4 : PvrDataCodec
    {
        public PvrDataCodec_Index4()
        {
            Decode = new PvrDataDecoder_Index4();
            Encode = null;
            Format = PvrDataFormat.Index4;
        }
    }
    public class PvrDataCodec_Index8 : PvrDataCodec
    {
        public PvrDataCodec_Index8()
        {
            Decode = new PvrDataDecoder_Index8();
            Encode = null;
            Format = PvrDataFormat.Index8;
        }
    }
    public class PvrDataCodec_Rectangle : PvrDataCodec
    {
        public PvrDataCodec_Rectangle()
        {
            Decode = new PvrDataDecoder_Rectangle();
            Encode = new PvrDataEncoder_Rectangle();
            Format = PvrDataFormat.Rectangle;
        }
    }
    public class PvrDataCodec_RectangleTwiddled : PvrDataCodec
    {
        public PvrDataCodec_RectangleTwiddled()
        {
            Decode = new PvrDataDecoder_RectangleTwiddled();
            Encode = null;
            Format = PvrDataFormat.RectangleTwiddled;
        }
    }

    // VrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public static class PvrCodecs
    {
        private static bool init = false;

        public static Dictionary<byte, PvrPixelCodec> PvrPixelCodecs = new Dictionary<byte, PvrPixelCodec>();
        public static Dictionary<byte, PvrDataCodec> PvrDataCodecs   = new Dictionary<byte, PvrDataCodec>();

        // Initalize
        public static void Initalize()
        {
            // Add the Palette Formats
            PvrPixelCodecs.Add(0x00, new PvrPaletteCodec_Argb1555());
            PvrPixelCodecs.Add(0x01, new PvrPaletteCodec_Rgb565());
            PvrPixelCodecs.Add(0x02, new PvrPaletteCodec_Argb4444());

            // Add the Data Formats
            PvrDataCodecs.Add(0x01, new PvrDataCodec_SquareTwiddled());
            PvrDataCodecs.Add(0x03, new PvrDataCodec_Vq());
            PvrDataCodecs.Add(0x04, new PvrDataCodec_VqMipMaps());
            PvrDataCodecs.Add(0x05, new PvrDataCodec_Index4());
            PvrDataCodecs.Add(0x07, new PvrDataCodec_Index8());
            PvrDataCodecs.Add(0x09, new PvrDataCodec_Rectangle());
            PvrDataCodecs.Add(0x0D, new PvrDataCodec_RectangleTwiddled());

            init = true;
        }

        public static PvrPixelCodec GetPixelCodec(byte Codec)
        {
            if (!init) Initalize();

            if (PvrPixelCodecs.ContainsKey(Codec))
                return PvrPixelCodecs[Codec];

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