﻿using System;
using System.IO;

namespace puyo_tools
{
    public class StreamConverter
    {
        /* Copy a stream to another stream */
        public static Stream Copy(Stream stream)
        {
            return Copy(stream, 0x0, (int)stream.Length);
        }

        /* Copy part of stream */
        public static Stream Copy(Stream stream, int offset, int length)
        {
            MemoryStream outputStream = new MemoryStream(length);

            stream.Position = offset;
            for (int i = 0; i < length; i++)
                outputStream.WriteByte((byte)stream.ReadByte());

            return outputStream;
        }
        public static Stream Copy(Stream stream, uint offset, uint length)
        {
            return Copy(stream, (int)offset, (int)length);
        }

        /* Convert stream to string */
        public static string ToString(Stream stream, long offset, uint maxLength, bool nullBytes)
        {
            string str = String.Empty;

            stream.Position = offset;
            for (int i = 0; i < maxLength; i++)
            {
                char strChar = (char)stream.ReadByte();
                if (strChar == '\0' && !nullBytes)
                    break;
                else
                    str += strChar;
            }

            return str;
        }
        public static string ToString(Stream stream, long offset, uint maxLength)
        {
            return ToString(stream, offset, maxLength, false);
        }
        public static string ToString(Stream stream, int offset, int maxLength)
        {
            return ToString(stream, offset, (uint)maxLength, false);
        }

        /* Convert Stream to Byte Array */
        public static byte[] ToByteArray(Stream stream, int offset, int length)
        {
            byte[] byteArray = new byte[length];
            stream.Position  = offset;
            stream.Read(byteArray, 0, length);
            return byteArray;
        }
        public static byte[] ToByteArray(Stream stream, uint offset, int length)
        {
            return ToByteArray(stream, (int)offset, length);
        }
        public static byte[] ToByteArray(Stream stream, uint offset, uint length)
        {
            return ToByteArray(stream, (int)offset, (int)length);
        }

        /* Convert Stream to unsigned integer */
        public static uint ToUInt(Stream stream, int offset)
        {
            return BitConverter.ToUInt32(ToByteArray(stream, offset, 4), 0);
        }
        public static uint ToUInt(Stream stream, uint offset)
        {
            return BitConverter.ToUInt32(ToByteArray(stream, (int)offset, 4), 0);
        }

        /* Convert Stream to unsigned short */
        public static ushort ToUShort(Stream stream, int offset)
        {
            return BitConverter.ToUInt16(ToByteArray(stream, offset, 2), 0);
        }

        /* Convert Stream to Byte */
        public static byte ToByte(Stream stream, long offset)
        {
            stream.Position = offset;
            return (byte)stream.ReadByte();
        }
    }

    public class StringConverter
    {
        /* Convert string to byte array */
        public static byte[] ToByteArray(string str, int length)
        {
            byte[] byteArray = new byte[length];

            for (int i = 0; i < length; i++)
            {
                if (str.Length < i)
                    byteArray[i] = (byte)str[i];
                else
                    byteArray[i] = 0;
            }

            return byteArray;
        }
    }

    public class ByteConverter
    {
        /* Convert byte array to string */
        public static string ToString(byte[] byteArray, int maxLength)
        {
            string str = String.Empty;

            for (int i = 0; i < byteArray.Length && i < maxLength; i++)
                str += (char)byteArray[i];

            return str;
        }
    }
}