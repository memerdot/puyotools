﻿using System;
using System.IO;

namespace VrSharp
{
    public abstract class VpClut
    {
        #region Fields
        protected bool InitSuccess = false; // Initalization

        protected byte[] ClutData; // Vp Clut Data

        protected ushort NumClutEntries; // Number of Clut Entries
        #endregion

        #region Constructors
        /// <summary>
        /// Open a Vp clut from a file.
        /// </summary>
        /// <param name="file">Filename of the file that contains the clut data.</param>
        public VpClut(string file)
        {
            byte[] data;
            try
            {
                using (BufferedStream stream = new BufferedStream(new FileStream(file, FileMode.Open, FileAccess.Read), 0x1000))
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }
            catch { data = new byte[0]; }

            ClutData = data;
        }

        /// <summary>
        /// Open a Vp clut from a stream.
        /// </summary>
        /// <param name="stream">Stream that contains the clut data.</param>
        public VpClut(Stream stream)
        {
            if (stream is MemoryStream) // We can use ToArray() for memory streams
            {
                try   { ClutData = (stream as MemoryStream).ToArray(); }
                catch { ClutData = new byte[0]; }
            }
            else
            {
                byte[] data;
                try
                {
                    using (BufferedStream bufStream = new BufferedStream(stream, 0x1000))
                    {
                        data = new byte[bufStream.Length];
                        bufStream.Read(data, 0, data.Length);
                    }
                }
                catch { data = new byte[0]; }

                ClutData = data;
            }
        }

        /// <summary>
        /// Open a Vp clut from a byte array.
        /// </summary>
        /// <param name="array">Byte array that contains the clut data.</param>
        public VpClut(byte[] array)
        {
            if (array == null)
                ClutData = new byte[0];
            else
                ClutData = array;
        }
        #endregion

        #region Clut
        /// <summary>
        /// Get the clut data.
        /// </summary>
        /// <param name="PixelCodec">Pixel Codec used for the clut.</param>
        /// <returns></returns>
        public byte[] GetClut(VrPixelCodec PixelCodec)
        {
            if (!InitSuccess) return new byte[0];

            byte[] clut = new byte[NumClutEntries * (PixelCodec.GetBpp() / 8)];
            Array.Copy(ClutData, 0x10, clut, 0x00, clut.Length);

            return clut;
        }

        /// <summary>
        /// Get the number of entries in the clut file.
        /// </summary>
        /// <returns></returns>
        public ushort GetNumClutEntries()
        {
            if (!InitSuccess) return 0;
            return NumClutEntries;
        }
        #endregion

        #region Misc
        /// <summary>
        /// Returns if the clut was loaded successfully.
        /// </summary>
        /// <returns></returns>
        public bool LoadSuccess()
        {
            return InitSuccess;
        }
        #endregion

        #region Private Properties
        // Function for checking headers
        // Checks to see if the string matches the byte data at the specific offset
        protected static bool Compare(byte[] array, string str, int offset)
        {
            if (offset < 0 || offset + str.Length > array.Length)
                return false; // Out of bounds

            for (int i = 0; i < str.Length; i++)
            {
                if (array[offset + i] != str[i])
                    return false;
            }

            return true;
        }
        #endregion
    }
}