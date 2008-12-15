using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class Image_Converter : Form
    {
        /* Options */
        private CheckBox
            autoDecompress,      // Auto decompress compressed files
            autoCompress;        // Auto compress files
            //autoDeleteConverted, // Auto delete extract files converted to PNG
            //autoExtractArchive,  // Auto extract archives in the archive
            //getFileNames;        // Return file names if we can find them
        
        private ComboBox
            compressionFormat; // Compression Format

        //private Button
            //doWorkButton; // Start the work.

        //private StatusMessage
            //status; // Status Box.

        private string[]
            files; // Filenames.

        private string[] compressionFormats = { // Compression Formats
            "CNX",
            "CXLZ",
            "LZ01"
        };

        /* Select files and display options */
        public Image_Converter()
        {
            /* Set up the window. */
            this.ClientSize      = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Image Converter Options";
            this.MaximizeBox     = false;

            /* Select the files. */
            files = Files.selectFiles("Select Files(s)", "All Supported Images (*.cnx;*.gim;*.gmp;*.gvr;*.png)|*.cnx;*.gim;*.gmp;*.gvr;*.png|CNX Files (*.cnx)|*.cnx|GIM Images (*.gim)|*.gim|GMP Images (*.gmp)|*.gmp|GVR Images (*.gvr)|*.gvr|PNG Images (*.png)|*.png");

            /* Don't continue if no files were selected. */
            if (files.Length < 1)
                return;

            /* Display number of files. */
            Label numFiles     = new Label();
            numFiles.Text      = files.Length + " File" + (files.Length > 1 ? "s" : "") + " Selected";
            numFiles.Location  = new Point(8, 8);
            numFiles.Size      = new Size(384, 16);
            numFiles.TextAlign = ContentAlignment.MiddleCenter;
            numFiles.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            this.Controls.Add(numFiles);

            /* Show Options */
            showOptions();
        }

        /* Show Options */
        private void showOptions()
        {
            /* Decompress file containing a supported compression format. */
            autoDecompress          = new CheckBox();
            autoDecompress.Text     = "Decompress files and images containing compression.";
            autoDecompress.Location = new Point(8, 32);
            autoDecompress.Size     = new Size(this.Width - 16, 20);
            autoDecompress.Checked  = true;

            this.Controls.Add(autoDecompress);

            /* Compress output image. */
            autoCompress          = new CheckBox();
            autoCompress.Text     = "Compress output image:";
            autoCompress.Location = new Point(8, 56);
            autoCompress.Size     = new Size(150, 20);
            autoCompress.Enabled  = false;

            this.Controls.Add(autoCompress);

            compressionFormat = new ComboBox();
            compressionFormat.Items.AddRange(compressionFormats);
            compressionFormat.DropDownStyle    = ComboBoxStyle.DropDownList;
            compressionFormat.MaxDropDownItems = compressionFormats.Length;
            compressionFormat.SelectedIndex    = 0;
            compressionFormat.Location         = new Point(158, 56);
            compressionFormat.Size             = new Size(64, compressionFormat.Height);
            compressionFormat.Enabled          = false;

            this.Controls.Add(compressionFormat);

            this.ShowDialog();
        }
    }
}