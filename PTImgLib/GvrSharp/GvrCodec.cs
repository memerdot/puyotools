// GvrCodec.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

// -- How to create a new GvrCodec --
// ----
// Quite simple really.
// 1. Add a new entry in "GvrFormat"
// 2. Create your derivitive of "GvrDecoder" in GvrDecoder.cs (See comments and existing implementatins for hints)
// 3. Create your derivitive of "GvrEncoder" in GvrEncoder.cs (See comments and existing implementatins for hints)
// 3. Create your derivitive of "GvrCodec" in GvrCodec.cs (Before you go ravaging through your tabs, you're already here ;))
// 4. Setup code to register your "GvrCodec" in GvrCodec.cs
// And now you have a brand new Gvr GvrCodec capable of decoding.
// ----

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GvrSharp
{
    // An enumeration of the known Gvr formats
    public enum GvrFormat : ushort
    {
        Rgb_565_4x4 = 0x0004,
        Rgb_5a3_4x4 = 0x0005,
        Unknown1 = 0x000E,
        Pal_565_8x4 = 0x1808,
        Pal_5a3_8x4 = 0x1809,
        P16_5a3_8x4 = 0x2809,
        P16_5a3_8x8 = 0x2808
    };

    public abstract class GvrCodec
    {
        public GvrDecoder Decode;
        public GvrEncoder Encode;
        public GvrFormat Format;
    };

    public class GvrCodec_0004 : GvrCodec
    {
        public GvrCodec_0004()
        {
            Decode = new GvrDecoder_0004();
            Encode = null;
            Format = GvrFormat.Rgb_565_4x4;
        }
    }
    public class GvrCodec_0005 : GvrCodec
    {
        public GvrCodec_0005()
        {
            Decode = new GvrDecoder_0005();
            Encode = null;
            Format = GvrFormat.Rgb_5a3_4x4;
        }
    }
    public class GvrCodec_1808 : GvrCodec
    {
        public GvrCodec_1808()
        {
            Decode = new GvrDecoder_1808();
            Encode = null;
            Format = GvrFormat.Rgb_565_4x4;
        }
    }
    public class GvrCodec_1809 : GvrCodec
    {
        public GvrCodec_1809()
        {
            Decode = new GvrDecoder_1809();
            Encode = new GvrEncoder_1809();
            Format = GvrFormat.Pal_565_8x4;
        }
    }
    public class GvrCodec_2808 : GvrCodec
    {
        public GvrCodec_2808()
        {
            Decode = new GvrDecoder_2808();
            Encode = null;
            Format = GvrFormat.Pal_565_8x4;
        }
    }
    public class GvrCodec_2809 : GvrCodec
    {
        public GvrCodec_2809()
        {
            Decode = new GvrDecoder_2809();
            Encode = null;
            Format = GvrFormat.Pal_565_8x4;
        }
    }

    // GvrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public class GvrCodecs
    {
        private static Hashtable hshTable = new Hashtable();
        private static bool inited = false;
        public static void Initialize()
        {
            Register("0004", new GvrCodec_0004());
            Register("0005", new GvrCodec_0005());
            Register("1808", new GvrCodec_1808());
            Register("1809", new GvrCodec_1809());
            Register("2808", new GvrCodec_2808());
            Register("2809", new GvrCodec_2809());
            inited = true;
        }
        public static bool Unregister(string CodecID)
        {
            if (hshTable.ContainsKey(CodecID))
            {
                hshTable.Remove(CodecID);
                return true;
            }
            return false;
        }
        public static bool Register(string CodecID, GvrCodec Codec)
        {
            if (hshTable.ContainsKey(CodecID))
            {
                hshTable.Remove(CodecID);
            }
            hshTable.Add(CodecID, Codec);
            return true;
        }
        public static GvrCodec GetCodec(string Codec)
        {
            if (!inited) Initialize();
            if (hshTable.ContainsKey(Codec))
            {
                return (GvrCodec)hshTable[Codec];
            }
            return null;
        }
    }
}
