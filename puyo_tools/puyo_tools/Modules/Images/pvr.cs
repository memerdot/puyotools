using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using VrSharp.PvrTexture;

namespace puyo_tools
{
    // Pvr Texture
    class PVR : ImageModule
    {
        public PvrPixelFormat PixelFormat = PvrPixelFormat.Argb1555;
        public PvrDataFormat DataFormat = PvrDataFormat.SquareTwiddled;
        public PvrCompressionFormat CompressionFormat = PvrCompressionFormat.None;
        public bool GbixHeader = true;
        public uint GlobalIndex = 0;

        public PVR()
        {
            Name      = "PVR";
            Extension = ".pvr";
            CanEncode = true;
            CanDecode = true;
        }

        // Convert the texture to a bitmap
        public override Bitmap Unpack(ref Stream data)
        {
            try
            {
                PvrTexture TextureInput = new PvrTexture(data.Copy());
                if (TextureInput.NeedsExternalClut())
                {
                    if (PaletteData != null)
                        TextureInput.SetClut(new PvpClut(PaletteData.Copy())); // Texture has an external clut; set it
                    else
                        throw new GraphicFormatNeedsPalette(); // Texture needs an external clut; throw an exception
                }

                return TextureInput.GetTextureAsBitmap();
            }
            catch (GraphicFormatNeedsPalette)
            {
                throw new GraphicFormatNeedsPalette(); // Throw it again
            }
            //catch   { return null; }
            finally { PaletteData = null; }
        }

        public override Stream Pack(ref Stream data)
        {
            // Convert the bitmap to a pvr
            try
            {
                PvrTextureEncoder TextureEncoder = new PvrTextureEncoder(data, PixelFormat, DataFormat);
                TextureEncoder.EnableGbix(GbixHeader);
                if (GbixHeader)
                    TextureEncoder.WriteGbix(GlobalIndex);
                if (CompressionFormat != PvrCompressionFormat.None)
                    TextureEncoder.SetCompressionFormat(CompressionFormat);

                return TextureEncoder.GetTextureAsStream();
            }
            catch { return null; }
        }

        // External Clut Filename
        public override string PaletteFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".pvp";
        }

        // See if the texture is a Pvr
        public override bool Check(ref Stream input, string filename)
        {
            try   { return PvrTexture.IsPvrTexture(input.ToByteArray()); }
            catch { return false; }
        }
    }
}