using System;
using System.IO;
using System.Collections.Generic;
using Extensions;
using VrSharp;

namespace puyo_tools
{
    public class PVZ : CompressionModule
    {
        public PVZ()
        {
            Name          = "PVZ";
            CanCompress   = false;
            CanDecompress = false;
        }

        // Decompress
        public override MemoryStream Decompress(ref Stream data)
        {
            return null; // Now handled by the Pvr module
        }

        /* Compress */
        public override MemoryStream Compress(ref Stream data, string filename)
        {
            return null; // Will be handled by the Pvr Module later
        }

        // Get the filename
        public override string DecompressFilename(ref Stream data, string filename)
        {
            // Only return a different extension if the current one is pvz
            if (Path.GetExtension(filename).ToLower() == ".pvz")
                return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".PVR" : ".pvr");

            return filename;
        }
        public override string CompressFilename(ref Stream data, string filename)
        {
            // Since we can only compress PVR files, add a pvz extension
            return Path.GetFileNameWithoutExtension(filename) + (Path.GetExtension(filename).IsAllUpperCase() ? ".PVZ" : ".pvz");
        }

        // Check
        public override bool Check(ref Stream data, string filename)
        {
            /*try
            {
                return ((data.ReadString(0x4, 4) == "GBIX" && data.ReadString(0x14, 4) == "PVRT") ||
                    data.ReadString(0x4, 4) == "PVRT");
            }
            catch
            {
                return false;
            }*/
            
            return false; // VrSharp has Pvz compression/decompression built in now.
        }
    }
}