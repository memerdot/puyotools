using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Extensions;
using VrSharp.SvrTexture;

namespace puyo_tools
{
    // Svr Texture
    class SVR : ImageModule
    {
        public SVR()
        {
            Name      = "SVR";
            Extension = ".svr";
            CanEncode = false;
            CanDecode = true;
        }

        // Convert the texture to a bitmap
        public override Bitmap Unpack(ref Stream data)
        {
            try
            {
                SvrTexture TextureInput = new SvrTexture(data.Copy());
                if (TextureInput.NeedsExternalClut())
                {
                    if (PaletteData != null)
                        TextureInput.SetClut(new SvpClut(PaletteData.Copy())); // Texture has an external clut; set it
                    else
                        throw new GraphicFormatNeedsPalette(); // Texture needs an external clut; throw an exception
                }

                return TextureInput.GetTextureAsBitmap();
            }
            catch (GraphicFormatNeedsPalette)
            {
                throw new GraphicFormatNeedsPalette(); // Throw it again
            }
            catch   { return null; }
            finally { PaletteData = null; }
        }

        public override Stream Pack(ref Stream data)
        {
            return null;
        }

        // External Clut Filename
        public override string PaletteFilename(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename) + ".svp";
        }

        // See if the texture is a Svr
        public override bool Check(ref Stream input, string filename)
        {
            try   { return SvrTexture.IsSvrTexture(input.ToByteArray()); }
            catch { return false; }
        }
    }
}