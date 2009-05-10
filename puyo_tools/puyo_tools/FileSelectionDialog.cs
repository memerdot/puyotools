using System;
using System.Windows.Forms;

namespace puyo_tools
{
    public class FileSelectionDialog
    {
        /* Open File Selection Dialog */
        public static string OpenFile(string title, string filter)
        {
            OpenFileDialog ofd   = new OpenFileDialog();
            ofd.Multiselect      = false;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists  = true;
            ofd.CheckPathExists  = true;
            ofd.AddExtension     = true;
            ofd.Filter           = filter;
            ofd.DefaultExt       = String.Empty;
            ofd.Title            = title;
            ofd.ShowDialog();

            return ofd.FileName;
        }

        public static string[] OpenFiles(string title, string filter)
        {
            OpenFileDialog ofd   = new OpenFileDialog();
            ofd.Multiselect      = true;
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists  = true;
            ofd.CheckPathExists  = true;
            ofd.AddExtension     = true;
            ofd.Filter           = filter;
            ofd.DefaultExt       = String.Empty;
            ofd.Title            = title;
            ofd.ShowDialog();

            return ofd.FileNames;
        }

        /* Save File Selection Dialog */
        public static string SaveFile(string title, string filename, string filter)
        {
            SaveFileDialog sfd   = new SaveFileDialog();
            sfd.AddExtension     = true;
            sfd.DefaultExt       = String.Empty;
            sfd.FileName         = filename;
            sfd.Filter           = filter;
            sfd.OverwritePrompt  = true;
            sfd.RestoreDirectory = true;
            sfd.Title            = title;
            sfd.ValidateNames    = true;
            sfd.ShowDialog();

            return sfd.FileName;
        }

        /* Save Directory Selection Dialog */
        public static string SaveDirectory(string description)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description         = description;
            fbd.SelectedPath        = Application.StartupPath;
            fbd.ShowNewFolderButton = true;
            fbd.ShowDialog();

            return fbd.SelectedPath;
        }
    }
}