// Not implimented at the moment
// Once work on converting to a vr format
// is started then this code will be uncommented.
// Otherwise it will stay commented to save some
// bytes in the build of VrSharp.

/*
using System;
using VrSharp.GvrTexture;
using VrSharp.PvrTexture;

namespace VrSharp.BitmapImage
{
    public class VrBitmap : VrBitmapBase
    {
        public byte[] ConvertToRaw(byte[] data)
        {
            return null;
        }

        #region Gvr Texture Conversion
        public byte[] ConvertToGvrTexture(GvrPixelFormat PixelFormat, GvrDataFormat DataFormat)
        {
            return ConvertToGvrTexture(PixelFormat, DataFormat, new GvrOptions());
        }

        public byte[] ConvertToGvrTexture(GvrPixelFormat PixelFormat, GvrDataFormat DataFormat, GvrOptions GvrOptions)
        {
            // Get the codecs and make sure we can encode
            GvrPixelCodec PixelCodec = GvrCodecList.GetPixelCodec(PixelFormat);
            GvrDataCodec DataCodec   = GvrCodecList.GetDataCodec(DataFormat);
            if ((DataFormat == GvrDataFormat.Index4 || DataFormat == GvrDataFormat.Index8) &&
                (PixelCodec == null || !PixelCodec.CanEncode()))
                return null;
            if (DataCodec == null || !DataCodec.CanEncode())
                return null;

            // Write out the header first
            int GbixOffset = (GvrOptions.IncludeGlobalIndex ? 0x00 : -1);
            int GvrtOffset = (GvrOptions.IncludeGlobalIndex ? 0x10 : 0x00);
            byte[] Header  = new byte[GvrtOffset + 0x10];

            if (GvrOptions.IncludeGlobalIndex) // Write out the Gbix/Gcix header
            {
                CopyToArray(Header, GbixOffset + 0x00, (GvrOptions.GvrType == GvrOptions.ConsoleFormat.Wii ? "GCIX" : "GBIX"));
                CopyToArray(Header, GbixOffset + 0x04, (uint)0x00000008);
                CopyToArray(Header, GbixOffset + 0x08, SwapUInt(GvrOptions.GlobalIndex));
                CopyToArray(Header, GbixOffset + 0x0C, (uint)0x00000000);
            }

            CopyToArray(Header, GvrtOffset + 0x00, "GVRT");
            CopyToArray(Header, GvrtOffset + 0x04, (uint)0x00000000); // Leave blank for now
            CopyToArray(Header, GvrtOffset + 0x08, (ushort)0x0000);
            Header[GvrtOffset + 0x0A] = (byte)(((byte)PixelFormat & 0xF) << 4);
            Header[GvrtOffset + 0x0B] = (byte)DataFormat;

            return null;
        }

        // Gvr Options
        public class GvrOptions : VrOptionsBase
        {
            public ConsoleFormat GvrType = ConsoleFormat.Wii;

            public enum ConsoleFormat
            {
                GameCube,
                Wii,
            }
        }
        #endregion

        #region Pvr Texture Conversion
        // Pvr Options
        public class PvrOptions : VrOptionsBase
        {
            public PvrCompressionFormat CompressionFormat = PvrCompressionFormat.None;
        }
        #endregion

        #region Svr Texture Conversion
        // Svr Options
        public class SvrOptions : VrOptionsBase { }
        #endregion

        // General Vr Options
        public class VrOptionsBase
        {
            public bool IncludeGlobalIndex = true;
            public uint GlobalIndex        = 0x00000000;
        }
    }
}
*/