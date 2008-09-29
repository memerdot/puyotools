// VrCodec.cs
// By Nmn / For PuyoNexus.net
// --
// This file is released under the New BSD license. See license.txt for details.
// This code comes with absolutely no warrenty.

// -- How to create a new VrCodec --
// ----
// Quite simple really.
// 1. Add a new entry in "VrFormat"
// 2. Create your derivitive of "VrDecoder" in VrDecoder.cs (See comments and existing implementatins for hints)
// 3. Create your derivitive of "VrEncoder" in VrEncoder.cs (See comments and existing implementatins for hints)
// 3. Create your derivitive of "VrCodec" in VrCodec.cs (Before you go ravaging through your tabs, you're already here ;))
// 4. Setup code to register your "VrCodec" in VrCodec.cs
// And now you have a brand new Vr VrCodec capable of decoding.
// ----

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace VrSharp
{
    // An enumeration of the known Vr formats
    public enum VrFormat : uint
    {
        Fmt00000004 = 0x00000004,
        Fmt00000005 = 0x00000005,
        Fmt00000006 = 0x00000006,
        Fmt0000000E = 0x0000000E,
        Fmt00001808 = 0x00001808,
        Fmt00001809 = 0x00001809,
        Fmt00002809 = 0x00002809,
        Fmt00002808 = 0x00002808,
        Fmt096C0000 = 0x096C0000
    };

    public enum VrType
    {
        SvrFile,
        GvrFile,
        PvrFile
    };

    public abstract class VrCodec
    {
        public VrDecoder Decode;
        public VrEncoder Encode;
        public VrFormat Format;
    };

    public class VrCodec_00000004 : VrCodec
    {
        public VrCodec_00000004()
        {
            Decode = new VrDecoder_00000004();
            Encode = null;
            Format = VrFormat.Fmt00000004;
        }
    }
    public class VrCodec_00000005 : VrCodec
    {
        public VrCodec_00000005()
        {
            Decode = new VrDecoder_00000005();
            Encode = null;
            Format = VrFormat.Fmt00000005;
        }
    }
    public class VrCodec_00000006 : VrCodec
    {
        public VrCodec_00000006()
        {
            Decode = new VrDecoder_00000006();
            Encode = null;
            Format = VrFormat.Fmt00000006;
        }
    }
    public class VrCodec_00001808 : VrCodec
    {
        public VrCodec_00001808()
        {
            Decode = new VrDecoder_00001808();
            Encode = null;
            Format = VrFormat.Fmt00001808;
        }
    }
    public class VrCodec_00001809 : VrCodec
    {
        public VrCodec_00001809()
        {
            Decode = new VrDecoder_00001809();
            Encode = new VrEncoder_00001809();
            Format = VrFormat.Fmt00001809;
        }
    }
    public class VrCodec_00002808 : VrCodec
    {
        public VrCodec_00002808()
        {
            Decode = new VrDecoder_00002808();
            Encode = null;
            Format = VrFormat.Fmt00002808;
        }
    }
    public class VrCodec_00002809 : VrCodec
    {
        public VrCodec_00002809()
        {
            Decode = new VrDecoder_00002809();
            Encode = null;
            Format = VrFormat.Fmt00002809;
        }
    }
    public class VrCodec_096C0000 : VrCodec
    {
        public VrCodec_096C0000()
        {
            Decode = new VrDecoder_096C0000();
            Encode = null;
            Format = VrFormat.Fmt096C0000;
        }
    }

    // VrCodecs is a static class for containing codecs
    // Register your codecs here in the initialize function.
    public class VrCodecs
    {
        private static Hashtable hshTable = new Hashtable();
        private static bool inited = false;
        public static void Initialize()
        {
            Register("00000004", new VrCodec_00000004());
            Register("00000005", new VrCodec_00000005());
            Register("00000006", new VrCodec_00000006());
            Register("00001808", new VrCodec_00001808());
            Register("00001809", new VrCodec_00001809());
            Register("00002808", new VrCodec_00002808());
            Register("00002809", new VrCodec_00002809());
            Register("096C0000", new VrCodec_096C0000());
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
        public static bool Register(string CodecID, VrCodec Codec)
        {
            if (hshTable.ContainsKey(CodecID))
            {
                hshTable.Remove(CodecID);
            }
            hshTable.Add(CodecID, Codec);
            return true;
        }
        public static VrCodec GetCodec(string Codec)
        {
            if (!inited) Initialize();
            if (hshTable.ContainsKey(Codec))
            {
                return (VrCodec)hshTable[Codec];
            }
            return null;
        }
    }
}