﻿using System;
using System.IO;
using System.Text;
using System.Drawing;

namespace VrSharp.SvrTexture
{
    public class SvrTextureEncoder : VrTextureEncoder
    {
        #region Constructors
        /// <summary>
        /// Open a bitmap from a file.
        /// </summary>
        /// <param name="file">Filename of the file that contains the bitmap data.</param>
        /// <param name="PixelFormat">Pixel Format</param>
        /// <param name="DataFormat">Data Format</param>
        public SvrTextureEncoder(string file, SvrPixelFormat PixelFormat, SvrDataFormat DataFormat)
            : base(file)
        {
            this.PixelFormat = (byte)PixelFormat;
            this.DataFormat  = (byte)DataFormat;

            InitSuccess = Initalize();
        }

        /// <summary>
        /// Open a bitmap from a stream.
        /// </summary>
        /// <param name="stream">Stream that contains the bitmap data.</param>
        /// <param name="PixelFormat">Pixel Format</param>
        /// <param name="DataFormat">Data Format</param>
        public SvrTextureEncoder(Stream stream, SvrPixelFormat PixelFormat, SvrDataFormat DataFormat)
            : base(stream)
        {
            this.PixelFormat = (byte)PixelFormat;
            this.DataFormat  = (byte)DataFormat;

            InitSuccess = Initalize();
        }

        /// <summary>
        /// Open a bitmap from a byte array.
        /// </summary>
        /// <param name="array">Byte array that contains the bitmap data.</param>
        /// <param name="PixelFormat">Pixel Format</param>
        /// <param name="DataFormat">Data Format</param>
        public SvrTextureEncoder(byte[] array, SvrPixelFormat PixelFormat, SvrDataFormat DataFormat)
            : base(array)
        {
            this.PixelFormat = (byte)PixelFormat;
            this.DataFormat  = (byte)DataFormat;

            InitSuccess = Initalize();
        }

        /// <summary>
        /// Open a bitmap from a System.Drawing.Bitmap.
        /// </summary>
        /// <param name="bitmap">A System.Drawing.Bitmap instance.</param>
        /// <param name="PixelFormat">Pixel Format</param>
        /// <param name="DataFormat">Data Format</param>
        public SvrTextureEncoder(Bitmap bitmap, SvrPixelFormat PixelFormat, SvrDataFormat DataFormat)
            : base(bitmap)
        {
            this.PixelFormat = (byte)PixelFormat;
            this.DataFormat  = (byte)DataFormat;

            InitSuccess = Initalize();
        }
        #endregion

        // Initalize the bitmap
        private bool Initalize()
        {
            // Make sure the width and height are correct
            if (TextureWidth < 8 || TextureHeight < 8) return false;
            if ((TextureWidth & (TextureWidth - 1)) != 0 || (TextureHeight & (TextureHeight - 1)) != 0)
                return false;

            PixelCodec = SvrCodecList.GetPixelCodec((SvrPixelFormat)PixelFormat);
            DataCodec  = SvrCodecList.GetDataCodec((SvrDataFormat)DataFormat);

            if (PixelCodec == null || DataCodec == null)           return false;
            if (!PixelCodec.CanEncode() || !DataCodec.CanEncode()) return false;
            if (!CanEncode((SvrPixelFormat)PixelFormat, (SvrDataFormat)DataFormat, TextureWidth, TextureHeight)) return false;

            GbixOffset = 0x00;
            PvrtOffset = 0x10;

            return true;
        }

        // Checks to see if we can encode the texture based on data format specific things
        private bool CanEncode(SvrPixelFormat PixelFormat, SvrDataFormat DataFormat, int width, int height)
        {
            // The converter should check to see that a pixel codec and data codec exists,
            // along with checking that width >= 8 and height >= 8.
            switch (DataFormat)
            {
                case SvrDataFormat.Index4RectRgb5a3:
                case SvrDataFormat.Index8RectRgb5a3:
                    return (PixelFormat == SvrPixelFormat.Rgb5a3);
                case SvrDataFormat.Index4SqrRgb5a3:
                case SvrDataFormat.Index8SqrRgb5a3:
                    return (width == height && PixelFormat == SvrPixelFormat.Rgb5a3);
                case SvrDataFormat.Index4RectArgb8:
                case SvrDataFormat.Index8RectArgb8:
                    return (PixelFormat == SvrPixelFormat.Argb8888);
                case SvrDataFormat.Index4SqrArgb8:
                case SvrDataFormat.Index8SqrArgb8:
                    return (width == height && PixelFormat == SvrPixelFormat.Argb8888);
            }

            return true;
        }

        // Write the Gbix header
        protected override byte[] WriteGbixHeader()
        {
            MemoryStream GbixHeader = new MemoryStream();
            using (BinaryWriter Writer = new BinaryWriter(GbixHeader))
            {
                Writer.Write(Encoding.UTF8.GetBytes("GBIX"));
                Writer.Write(0x00000008);
                Writer.Write(GlobalIndex);
                Writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
                Writer.Flush();
            }

            return GbixHeader.ToArray();
        }

        // Write the Pvrt header
        protected override byte[] WritePvrtHeader(int TextureSize)
        {
            MemoryStream PvrtHeader = new MemoryStream();
            using (BinaryWriter Writer = new BinaryWriter(PvrtHeader))
            {
                Writer.Write(Encoding.UTF8.GetBytes("PVRT"));
                Writer.Write((DataOffset + TextureSize) - 24);
                Writer.Write(PixelFormat);
                Writer.Write(DataFormat);
                Writer.Write(new byte[] { 0x00, 0x00 });
                Writer.Write(TextureWidth);
                Writer.Write(TextureHeight);
                Writer.Flush();
            }

            return PvrtHeader.ToArray();
        }
    }
}