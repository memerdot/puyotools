﻿using System;
using System.IO;

namespace VrSharp.GvrTexture
{
    public class GvpClut : VpClut
    {
        #region Constructors
        /// <summary>
        /// Open a Gvp clut from a file.
        /// </summary>
        /// <param name="file">Filename of the file that contains the clut data.</param>
        public GvpClut(string file)
            : base(file)
        {
            InitSuccess = ReadHeader();
        }

        /// <summary>
        /// Open a Gvp clut from a stream.
        /// </summary>
        /// <param name="stream">Stream that contains the clut data.</param>
        public GvpClut(Stream stream)
            : base(stream)
        {
            InitSuccess = ReadHeader();
        }

        /// <summary>
        /// Open a Gvp clut from a byte array.
        /// </summary>
        /// <param name="array">Byte array that contains the clut data.</param>
        public GvpClut(byte[] array)
            : base(array)
        {
            InitSuccess = ReadHeader();
        }
        #endregion

        #region Header
        // Read the header and sets up the appropiate values.
        // Returns true if successful, otherwise false
        private bool ReadHeader()
        {
            if (!IsGvpClut(ClutData))
                return false;

            NumClutEntries = (ushort)((ClutData[0x0E] << 8) | ClutData[0x0F]);

            // I don't know how gvp's are supposed to be formatted
            PixelFormat = (byte)GvrPixelFormat.Unknown;
            PixelCodec  = null;

            return true;
        }

        // Checks if the input file is a gvp
        private bool IsGvpClut(byte[] data)
        {
            if (Compare(data, "GVPL", 0x00) &&
                BitConverter.ToUInt32(data, 0x04) == data.Length - 8)
                return true;

            return false;
        }
        #endregion
    }
}