using System;
using System.Collections.Generic;

namespace VrSharp
{
    // SVR Palette Formats
    public enum SvrPaletteFormat : byte
    {
        Format08 = 0x08,
        Format09 = 0x09,
    }

    // SVR Data Formats
    public enum SvrDataFormat : byte
    {
        Format60 = 0x60,
        Format64 = 0x64,
        Format68 = 0x68,
        Format69 = 0x69,
        Format6A = 0x6A,
        Format6C = 0x6C,
    }

    public class VrPaletteCodec
    {
        public VrPaletteDecoder Decode;
        public VrPaletteEncoder Encode;
    }

    public class VrDataCodec
    {
        public VrDataDecoder Decode;
        public VrDataEncoder Encode;
    }

    public abstract class SvrPaletteCodec : VrPaletteCodec
    {
        public SvrPaletteFormat Format;
    }

    public abstract class SvrDataCodec : VrDataCodec
    {
        public SvrDataFormat Format;
    }

    // SVR Palette Format Classes
    public class SvrPaletteCodec_08 : SvrPaletteCodec
    {
        public SvrPaletteCodec_08()
        {
            Decode = new SvrPaletteDecoder_08();
            Encode = null;
            Format = SvrPaletteFormat.Format08;
        }
    }
    public class SvrPaletteCodec_09 : SvrPaletteCodec
    {
        public SvrPaletteCodec_09()
        {
            Decode = new SvrPaletteDecoder_09();
            Encode = null;
            Format = SvrPaletteFormat.Format09;
        }
    }

    // SVR Data Format Classes
    public class SvrDataCodec_60 : SvrDataCodec
    {
        public SvrDataCodec_60()
        {
            Decode = new SvrDataDecoder_60();
            Encode = null;
            Format = SvrDataFormat.Format60;
        }
    }
    public class SvrDataCodec_64 : SvrDataCodec
    {
        public SvrDataCodec_64()
        {
            Decode = new SvrDataDecoder_64();
            Encode = null;
            Format = SvrDataFormat.Format64;
        }
    }
    public class SvrDataCodec_68 : SvrDataCodec
    {
        public SvrDataCodec_68()
        {
            Decode = new SvrDataDecoder_68();
            Encode = null;
            Format = SvrDataFormat.Format68;
        }
    }
    public class SvrDataCodec_69 : SvrDataCodec
    {
        public SvrDataCodec_69()
        {
            Decode = null;
            Encode = null;
            Format = SvrDataFormat.Format69;
        }
    }
    public class SvrDataCodec_6A : SvrDataCodec
    {
        public SvrDataCodec_6A()
        {
            Decode = new SvrDataDecoder_6A();
            Encode = null;
            Format = SvrDataFormat.Format6A;
        }
    }
    public class SvrDataCodec_6C : SvrDataCodec
    {
        public SvrDataCodec_6C()
        {
            Decode = new SvrDataDecoder_6C();
            Encode = null;
            Format = SvrDataFormat.Format6C;
        }
    }

    // VrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public static class SvrCodecs
    {
        private static bool init = false;

        public static Dictionary<byte, SvrPaletteCodec> SvrPaletteCodecs = new Dictionary<byte, SvrPaletteCodec>();
        public static Dictionary<byte, SvrDataCodec> SvrDataCodecs       = new Dictionary<byte, SvrDataCodec>();

        // Initalize
        public static void Initalize()
        {
            // Add the Palette Formats
            SvrPaletteCodecs.Add(0x08, new SvrPaletteCodec_08());
            SvrPaletteCodecs.Add(0x09, new SvrPaletteCodec_09());

            // Add the Data Formats
            SvrDataCodecs.Add(0x60, new SvrDataCodec_60());
            SvrDataCodecs.Add(0x64, new SvrDataCodec_64());
            SvrDataCodecs.Add(0x68, new SvrDataCodec_68());
            SvrDataCodecs.Add(0x69, new SvrDataCodec_69());
            SvrDataCodecs.Add(0x6A, new SvrDataCodec_6A());
            SvrDataCodecs.Add(0x6C, new SvrDataCodec_6C());

            init = true;
        }

        public static SvrPaletteCodec GetPaletteCodec(byte Codec)
        {
            if (!init) Initalize();

            if (SvrPaletteCodecs.ContainsKey(Codec))
                return SvrPaletteCodecs[Codec];

            return null;
        }
        public static SvrDataCodec GetDataCodec(byte Codec)
        {
            if (!init) Initalize();

            if (SvrDataCodecs.ContainsKey(Codec))
                return SvrDataCodecs[Codec];

            return null;
        }
    }
}