using System;

namespace puyo_tools
{
    public class CNX
    {
        /* CNX cracked by drx (Luke Zapart)
         * <thedrx@gmail.com> */

        private uint 
            compressedSize   = 0, // Compressed Size
            decompressedSize = 0; // Decompressed Sizes.

        private int
            Cpointer = 0x10, // Compressed Pointer
            Dpointer = 0;    // Decompressed Pointer

        private byte
            Ccontrol = 0, // Control Byte
            Cmode    = 0; // Mode

        private byte[]
            decompressedData, // Decompressed Data
            compressedData;   // Compressed Data


        public CNX()
        {
        }

        /* Decompress */
        public byte[] decompress(byte[] compressedData)
        {
            try
            {
                /* Get the sizes of the compressed & decompressed files */
                compressedSize   = Endian.swapInt(BitConverter.ToUInt32(compressedData, 0x8)) + 16;
                decompressedSize = Endian.swapInt(BitConverter.ToUInt32(compressedData, 0xC));

                decompressedData = new byte[decompressedSize];

                /* Ok, let's attempt to decompress the data */
                while (Cpointer < compressedSize)
                {
                    /* Check for out of bounds */
                    if (Dpointer >= decompressedSize)
                        break;

                    Ccontrol = compressedData[Cpointer];
                    Cpointer++;

                    for (int i = 0; i < 4; i++)
                    {
                        /* Get control mode and shift bytes. */
                        Cmode      = (byte)(Ccontrol & 3);
                        Ccontrol >>= 2;

                        /* Check for the mode */
                        switch (Cmode)
                        {
                            /* Padding Mode
					         * All CNX archives seem to be packed in 0x800 chunks. when nearing
					         * a 0x800 cutoff, there usually is a padding command at the end to skip
					         * a few bytes (to the next 0x800 chunk, i.e. 0x4800, 0x7000, etc.) */
                            case 0:
                                byte temp_byte;

                                temp_byte = compressedData[Cpointer];
                                Cpointer += (temp_byte & 0xFF) + 1;

                                i = 3;
                                break;

                            /* Single Byte Copy Mode */
                            case 1:
                                decompressedData[Dpointer] = compressedData[Cpointer];
                                Cpointer++;
                                Dpointer++;
                                break;

                            /* Copy from destination buffer to current position */
                            case 2:
                                uint off, len, temp_word;

                                temp_word = Endian.swapShort(BitConverter.ToUInt16(compressedData, Cpointer));

                                off = (temp_word >> 5) + 1;
                                len = (temp_word & 0x1F) + 4;

                                Cpointer += 2;

                                for (int j = 0; j < len; j++)
                                {
                                    decompressedData[Dpointer] = decompressedData[Dpointer - off];
                                    Dpointer++;
                                }

                                break;

                            /* Direct Block Copy (first byte signifies length of copy) */
                            case 3:
                                byte blockLength;

                                blockLength = compressedData[Cpointer];
                                Cpointer++;

                                for (int j = 0; j < blockLength; j++)
                                {
                                    decompressedData[Dpointer] = compressedData[Cpointer];
                                    Cpointer++;
                                    Dpointer++;
                                }

                                break;
                        }
                    }
                }

                return decompressedData;
            }
            catch
            {
                return new byte[0];
            }
        }

        /* Compress */
        public byte[] compress(byte[] decompressedData)
        {
            try
            {
                /* Set the sizes of the compressed & decompressed files */
                //decompressedSize = (uint)decompressedData.Length;
                //compressedSize   = Cpointer + decompressedSize + (int)Math.Ceiling((double) decompressedSize / 8);

                compressedData = new byte[compressedSize];

                /* Can we compress this? (Filesize is too big?) */
                if (decompressedSize >= Int32.MaxValue || compressedSize >= Int32.MaxValue)
                    return new byte[0];

                /* Set CNX header. */
                Array.Copy(Header.LZ01, 0, compressedData, 0, Header.LZ01.Length);

                /* Set filesizes */
                Array.Copy(BitConverter.GetBytes(compressedSize),   0, compressedData, 0x4, 4); // Compressed Size
                Array.Copy(BitConverter.GetBytes(decompressedSize), 0, compressedData, 0x8, 4); // Decompressed Size

                /* Write compressed data, the easy way! */
                while (Cpointer < compressedSize)
                {
                    compressedData[Cpointer] = 0xFF;
                    Cpointer++;

                    for (int i = 0; i < 8; i++)
                    {
                        compressedData[Cpointer] = decompressedData[Dpointer];
                        Cpointer++;
                        Dpointer++;
                    }
                }

                return compressedData;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
    }
}
