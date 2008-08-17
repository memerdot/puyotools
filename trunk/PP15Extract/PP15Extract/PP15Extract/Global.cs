using System;

namespace pp15_fileTools
{
    public class Header
    {
        /* List of headers for files */

        /* Compression headers */
        public static byte[]
            LZ01 = { 0x4C, 0x5A, 0x30, 0x31 }, // LZ01
            CXLZ = { 0x43, 0x58, 0x4C, 0x5A }, // CXLZ
            CNX  = { 0x43, 0x4E, 0x58, 0x02 }; // CNX

        /* Graphic Formats */
        public static byte[]
            SVR = { 0x47, 0x42, 0x49, 0x58 }, // SVR
            GVR = { 0x47, 0x43, 0x49, 0x58 }, // GVR
            GIM = { 0x2E, 0x47, 0x49, 0x4D }, // GIM (Little Endian)
            MIG = { 0x4D, 0x49, 0x47, 0x2E }, // GIM (Big Endian)
            TGA = { 0x2E, 0x74, 0x67, 0x61 }; // TGA (Actually an extension.)

        /* Archive Formats */
        public static byte[]
            SNT = { 0x4E, 0x55, 0x49, 0x46 }, // SNT
            GNT = { 0x4E, 0x47, 0x49, 0x46 }, // GNT
            AFS = { 0x41, 0x46, 0x53, 0x00 }; // AFS

        /* Special */
        public static byte[]
            SNT2 = { 0x4E, 0x55, 0x54, 0x4C }, // SNT file (not SNC)
            GNT2 = { 0x4E, 0x47, 0x54, 0x4C }; // GNT file (not GNCS)


        /* Checks if it is the following file format */
        public static bool isFile(byte[] data, byte[] header, int offset)
        {
            if (offset < 0 || offset + header.Length >= data.Length)
                return false;

            for (int i = 0; i < header.Length; i++)
            {
                if (data[offset + i] != header[i])
                    return false;
            }

            return true;
        }
    }
}