using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GvrSharp
{
    // An enumeration of the known Gvr formats
    public enum GvrFormat : ushort
    {
        Rgb_5a3_8x4 = 0x0005,
        Unknown1 = 0x000E,
        Pal_565_8x4 = 0x1809,
        P16_5a3_8x4 = 0x2809,
        P16_5a3_8x8 = 0x2808
    };

    public abstract class GvrCodec
    {
        GvrDecoder Decode;
        GvrEncoder Encode;
        GvrFormat Format;
    };

    public class GvrCodecs
    {
        static private Hashtable hshTable = new Hashtable();
        static void Initialize()
        {
            hshTable.Add("0x0005", GvrFormat.Rgb_5a3_8x4);
            hshTable.Add("0x1809", GvrFormat.Pal_565_8x4);
            hshTable.Add("0x2809", GvrFormat.P16_5a3_8x4);
            hshTable.Add("0x2808", GvrFormat.P16_5a3_8x8);
        }
    }
}
