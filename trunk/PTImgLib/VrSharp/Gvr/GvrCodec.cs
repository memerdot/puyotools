using System;
using System.Collections.Generic;

namespace VrSharp
{
    // GVR Palette Formats
    public enum GvrPaletteFormat : byte
    {
        Format00 = 0x00,
        Format01 = 0x01,
        Format02 = 0x02,
        Format18 = 0x18,
        Format28 = 0x28,
    }

    // GVR Data Formats
    public enum GvrDataFormat : byte
    {
        Format02 = 0x02,
        Format03 = 0x03,
        Format04 = 0x04,
        Format05 = 0x05,
        Format06 = 0x06,
        Format08 = 0x08,
        Format09 = 0x09,
        Format0E = 0x0E,
    }

    public abstract class GvrPaletteCodec : VrPaletteCodec
    {
        public GvrPaletteFormat Format;
    }

    public abstract class GvrDataCodec : VrDataCodec
    {
        public GvrDataFormat Format;
    }

    // GVR Palette Format Classes
    public class GvrPaletteCodec_00 : GvrPaletteCodec
    {
        public GvrPaletteCodec_00()
        {
            Decode = null;
            Encode = null;
            Format = GvrPaletteFormat.Format00;
        }
    }
    public class GvrPaletteCodec_01 : GvrPaletteCodec
    {
        public GvrPaletteCodec_01()
        {
            Decode = null;
            Encode = null;
            Format = GvrPaletteFormat.Format01;
        }
    }
    public class GvrPaletteCodec_02 : GvrPaletteCodec
    {
        public GvrPaletteCodec_02()
        {
            Decode = null;
            Encode = null;
            Format = GvrPaletteFormat.Format02;
        }
    }
   public class GvrPaletteCodec_18 : GvrPaletteCodec
    {
        public GvrPaletteCodec_18()
        {
            Decode = new GvrPaletteDecoder_18();
            Encode = null;
            Format = GvrPaletteFormat.Format18;
        }
    }
    public class GvrPaletteCodec_28 : GvrPaletteCodec
    {
        public GvrPaletteCodec_28()
        {
            Decode = new GvrPaletteDecoder_28();
            Encode = null;
            Format = GvrPaletteFormat.Format28;
        }
    }

    // GVR Data Format Classes
    public class GvrDataCodec_02 : GvrDataCodec
    {
        public GvrDataCodec_02()
        {
            Decode = new GvrDataDecoder_02();
            Encode = null;
            Format = GvrDataFormat.Format02;
        }
    }
    public class GvrDataCodec_03 : GvrDataCodec
    {
        public GvrDataCodec_03()
        {
            Decode = new GvrDataDecoder_03();
            Encode = null;
            Format = GvrDataFormat.Format03;
        }
    }
    public class GvrDataCodec_04 : GvrDataCodec
    {
        public GvrDataCodec_04()
        {
            Decode = new GvrDataDecoder_04();
            Encode = null;
            Format = GvrDataFormat.Format04;
        }
    }
    public class GvrDataCodec_05 : GvrDataCodec
    {
        public GvrDataCodec_05()
        {
            Decode = new GvrDataDecoder_05();
            Encode = null;
            Format = GvrDataFormat.Format05;
        }
    }
    public class GvrDataCodec_06 : GvrDataCodec
    {
        public GvrDataCodec_06()
        {
            Decode = new GvrDataDecoder_06();
            Encode = null;
            Format = GvrDataFormat.Format06;
        }
    }
    public class GvrDataCodec_08 : GvrDataCodec
    {
        public GvrDataCodec_08()
        {
            Decode = new GvrDataDecoder_08();
            Encode = null;
            Format = GvrDataFormat.Format08;
        }
    }
    public class GvrDataCodec_09 : GvrDataCodec
    {
        public GvrDataCodec_09()
        {
            Decode = new GvrDataDecoder_09();
            Encode = null;
            Format = GvrDataFormat.Format09;
        }
    }
    public class GvrDataCodec_0E : GvrDataCodec
    {
        public GvrDataCodec_0E()
        {
            Decode = new GvrDataDecoder_0E();
            Encode = null;
            Format = GvrDataFormat.Format0E;
        }
    }

    // VrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public static class GvrCodecs
    {
        private static bool init = false;

        public static Dictionary<byte, GvrPaletteCodec> GvrPaletteCodecs = new Dictionary<byte, GvrPaletteCodec>();
        public static Dictionary<byte, GvrDataCodec> GvrDataCodecs       = new Dictionary<byte, GvrDataCodec>();

        // Initalize
        public static void Initalize()
        {
            // Add the Palette Formats
            GvrPaletteCodecs.Add(0x00, new GvrPaletteCodec_00());
            GvrPaletteCodecs.Add(0x01, new GvrPaletteCodec_01());
            GvrPaletteCodecs.Add(0x02, new GvrPaletteCodec_02());
            GvrPaletteCodecs.Add(0x08, new GvrPaletteCodec_01());
            GvrPaletteCodecs.Add(0x09, new GvrPaletteCodec_01());
            GvrPaletteCodecs.Add(0x18, new GvrPaletteCodec_18());
            GvrPaletteCodecs.Add(0x19, new GvrPaletteCodec_18());
            GvrPaletteCodecs.Add(0x28, new GvrPaletteCodec_28());

            // Add the Data Formats
            GvrDataCodecs.Add(0x02, new GvrDataCodec_02());
            GvrDataCodecs.Add(0x03, new GvrDataCodec_03());
            GvrDataCodecs.Add(0x04, new GvrDataCodec_04());
            GvrDataCodecs.Add(0x05, new GvrDataCodec_05());
            GvrDataCodecs.Add(0x06, new GvrDataCodec_06());
            GvrDataCodecs.Add(0x08, new GvrDataCodec_08());
            GvrDataCodecs.Add(0x09, new GvrDataCodec_09());
            GvrDataCodecs.Add(0x0E, new GvrDataCodec_0E());

            init = true;
        }

        public static GvrPaletteCodec GetPaletteCodec(byte Codec)
        {
            if (!init) Initalize();

            if (GvrPaletteCodecs.ContainsKey(Codec))
                return GvrPaletteCodecs[Codec];

            return null;
        }
        public static GvrDataCodec GetDataCodec(byte Codec)
        {
            if (!init) Initalize();

            if (GvrDataCodecs.ContainsKey(Codec))
                return GvrDataCodecs[Codec];

            return null;
        }
    }
}