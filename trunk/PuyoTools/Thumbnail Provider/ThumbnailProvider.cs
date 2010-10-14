using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;
using System.Diagnostics;
using System.Drawing;
using VrSharp;
using VrSharp.PvrTexture;
using PuyoTools;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Drawing.Imaging;

namespace PuyoTools
{
    #region COM definitions
    [ComVisible(true)]
    [Guid("e357fccd-a995-4576-b01f-234630154e96")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IThumbnailProvider
    {
        void GetThumbnail(uint squareLength, out IntPtr hBitmap, out UInt32 bitmapType);
    }

    [ComVisible(true)]
    [Guid("b824b49d-22ac-4161-ac8a-9916e8fa3f7f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithStream
    {
        void Initialize(IStream stream, UInt32 grfMode);
    }
    #endregion

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("PuyoTools.ThumbnailProvider")]
    [Guid("de05bc1b-88c5-487d-844c-68b656bded75")]
    public class ThumbnailProvider : IThumbnailProvider, IInitializeWithStream
    {
        #region Variables
        private IStream stream;
        protected IStream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        }
        private string filename;
        protected string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }
        private string palettePath;
        private static string defaultPalettePath = Environment.GetEnvironmentVariable("programfiles", EnvironmentVariableTarget.Machine) + "\\Puyo Tools\\Palettes";
        private static string userPalettePath = Environment.GetEnvironmentVariable("appdata", EnvironmentVariableTarget.User) + "\\Puyo Tools\\Palettes";
        private static Guid guid = new Guid("de05bc1b-88c5-487d-844c-68b656bded75");
        #endregion

        #region IInitializeWithStream implementation
        public void Initialize(IStream stream, uint grfMode)
        {
            this.stream = stream;
        }
        #endregion

        #region IThumbnailProvider implementation
        public void GetThumbnail(uint squareLength, out IntPtr hBitmap, out uint bitmapType)
        {
            hBitmap = IntPtr.Zero;
            bitmapType = 0; // Let the shell try and work it out.
            try
            {
                byte[] datas = GetFileContents();
                // Are we dealing with a palette, not an image?
                if (filename.EndsWith(".pvp"))
                {
                    SavePalette(filename, datas);
                    return;
                }
                Bitmap image = LoadImage(datas);
                if (image == null)
                {
                    return;
                }

                if (squareLength > image.Width && squareLength > image.Height)
                {
                    if (image.Width > image.Height) squareLength = (uint)image.Width;
                    if (image.Height > image.Width) squareLength = (uint)image.Height;
                }

                double ratio = ((double)image.Width) / ((double)image.Height);
                int finalHeight = (int)squareLength;
                int finalWidth = (int)squareLength;
                if (ratio > 1.0) { finalWidth = (int)squareLength; finalHeight = (int)(((double)squareLength) / ratio); }
                if (ratio < 1.0) { finalHeight = (int)squareLength; finalWidth = (int)(((double)squareLength) * ratio); }
                Bitmap bitmap = new Bitmap((int)finalWidth, (int)finalHeight);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(image, new Rectangle(0, 0, finalWidth, finalHeight), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                }
                hBitmap = bitmap.GetHbitmap();
            }
            catch(Exception ex)
            {
                // Silently don't generate thumbnail, or show exception
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        protected byte[] GetFileContents()
        {
            if (stream == null) return null;

            STATSTG stat;
            stream.Stat(out stat, 0);
            filename = stat.pwcsName;

            byte[] bytes = new byte[stat.cbSize];

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(UInt64)));
            stream.Read(bytes, bytes.Length, ptr);

            return bytes;
        }
        protected Bitmap LoadImage(byte[] datas)
        {
            try
            {
                Images.InitalizeDictionary();
            }
            catch
            {
                // continue
            }
            Images imageClass = new Images(new MemoryStream(datas), filename);
            try
            {
                /* Get and return the image */
                return imageClass.Unpack();
            }
            catch (GraphicFormatNeedsPalette)
            {
                // We can't actually get the absolute path of the stream (for now - fucking Vista)
                // So instead we pick from a special folder.
                if (filename == null || filename == String.Empty) return null;
                try
                {
                    imageClass = new Images(new MemoryStream(datas), filename);
                    imageClass.Decoder.PaletteData = new FileStream(palettePath + "\\" + imageClass.Decoder.PaletteFilename(filename), FileMode.Open);
                    return imageClass.Unpack();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region Palette Handling

        /// <summary>
        /// Initializes the palette path information, ensuring user paths exist, setting the registry keys when needed.
        /// </summary>
        private void InitializePalettePaths()
        {
            // Attempt to get the system-wide path.
            try
            {
                RegistryKey root = Registry.LocalMachine;
                RegistryKey rk = root.CreateSubKey("Software\\Puyo Tools");
                palettePath = (string)rk.GetValue("FallbackPalettePath", defaultPalettePath);
            }
            catch
            {
                // No real need to handle this exception in a thumb handler.
                palettePath = defaultPalettePath;

                // If possible, try to set the registry key anyways.
                try
                {
                    RegistryKey root = Registry.LocalMachine;
                    RegistryKey rk = root.CreateSubKey("Software\\Puyo Tools");
                    rk.SetValue("FallbackPalettePath", defaultPalettePath);
                }
                catch
                {

                }
            }
            // If the palettePath can't be found, we can't use it.
            // As a thumb handler, we don't have permission to create it.
            if (!Directory.Exists(palettePath))
            {
                palettePath = null;
            }

            // Attempt to create user directories as needed.
            try
            {
                if (!Directory.Exists(userPalettePath))
                {
                    string userPuyoToolsPath = userPalettePath.Remove(userPalettePath.LastIndexOf("\\"));
                    if (!Directory.Exists(userPuyoToolsPath))
                    {
                        Directory.CreateDirectory(userPuyoToolsPath);
                    }
                    Directory.CreateDirectory(userPalettePath);
                }
            }
            catch
            {

            }
        }

        private void SavePalette(string filename, byte[] datas)
        {
            
        }

        #endregion

        #region Registration
        [System.Runtime.InteropServices.ComRegisterFunctionAttribute()]
		static void RegisterServer(String str1)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.SetValue(guid.ToString(), "Puyo Tools Thumbnail Provider");
				rk.Close();

				root = Registry.ClassesRoot;
                rk = root.CreateSubKey(".pvz\\shellex\\{e357fccd-a995-4576-b01f-234630154e96}");
                rk.SetValue("", "{" + guid.ToString() + "}");
                rk = root.CreateSubKey(".pvr\\shellex\\{e357fccd-a995-4576-b01f-234630154e96}");
				rk.SetValue("", "{" + guid.ToString() + "}");
				rk.Close();

                Type[] types =
                {
                    typeof(GimSharp.GimFile),
                    typeof(VrSharp.VrCodec),
                    typeof(ImgSharp.ImgFile)
                };
                foreach (Type type in types)
                {
                    Assembly asm = Assembly.GetAssembly(type);
                    RegistrationServices reg = new RegistrationServices();
                    reg.RegisterAssembly(asm, 0);
                }
                
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}

		[System.Runtime.InteropServices.ComUnregisterFunctionAttribute()]
		static void UnregisterServer(String str1)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;

				// Remove ShellExtenstions registration
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.DeleteValue(guid.ToString());
				rk.Close();

				// Delete regkey
				root = Registry.ClassesRoot;
                root.DeleteSubKey(".pvz\\shellex\\{e357fccd-a995-4576-b01f-234630154e96}");
                root.DeleteSubKey(".pvr\\shellex\\{e357fccd-a995-4576-b01f-234630154e96}");
                root.DeleteSubKey(".pvp\\shellex\\{e357fccd-a995-4576-b01f-234630154e96}");
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}
		#endregion
    }
}