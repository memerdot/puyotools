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
        SquareTwiddled = 0x01,
        SquareTwiddledMipMaps = 0x02,
        Vq = 0x03,
        VqMipMaps = 0x04,
        Index8 = 0x05,
        Index4 = 0x06,
        Index8ExternPalette = 0x07,
        Index4ExternPalette = 0x08,
        Rectangle = 0x09,
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

            // Add the Data Formats

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