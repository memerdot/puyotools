using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;

namespace puyo_tools
{
    public class puyo_tools : Form
    {
        /* Buttons & Labels */
        private Button[] buttonPrograms = // Buttons for each program
        {
            new Button(), // File Decompressor
            new Button(), // File Compressor
            new Button(), // Archive Extractor
            new Button(), // Archive Creator
            new Button(), // GimConv
            new Button(), // VrConv
            new Button(),
        };

        private string[] labelPrograms = // Labels for each program
        {
            //"Decompress",                  // File Decompressor
            //"Compress",                    // File Compressor
            //"Extract",                     // Archive Extractor
            //"Create",                      // Archive Creator
            //"Image Converter",             // Image Converter
            //"VrConv\n(GVR, PVR, PVZ, SVR)" // VrConv
            "Select Files",
            "Select Directory",
            "Select Files",
            "Select Directory",
            "Select Files",
            "Select Directory",
        };

        public void puyo_tools2()
        {
            /* Create the form */
            FormContent.Create(this, "Puyo Tools", new Size(344, 258));

            /* Draw logo */
            PictureBox logo = new PictureBox();
            logo.Image = new Bitmap((Bitmap)new ComponentResourceManager(typeof(images)).GetObject("logo"));
            logo.Location = new Point(0, 0);
            logo.Size = new Size(316, 47);
            this.Controls.Add(logo);
        }

        public puyo_tools()
        {
            /* Set up form options */
            this.ClientSize      = new Size(344, 258 + 64);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Puyo Tools";

            /* Compression */
            Label compression = new Label();
            //compression.Text      = "Compression\n(CNX, CXLZ, LZ01)";
            compression.Text      = "Compression Decompressor";
            compression.Location  = new Point(8, 8);
            compression.Size      = new Size(this.Width - (compression.Location.X * 2), 32);
            compression.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            compression.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(compression);

            /* Archives */
            Label archives     = new Label();
            //archives.Text      = "Archives\n(ACX, AFS, GNT, GVM, MRG, ONE, PVM, SNT, SPK, TEX, VDD)";
            archives.Text      = "Archive Extractor";
            archives.Location  = new Point(8, 96);
            archives.Size      = new Size(this.Width - (archives.Location.X * 2), 32);
            archives.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            archives.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(archives);

            /* Image Conversion */
            Label imgConversion     = new Label();
            //imgConversion.Text      = "Image Conversion\n(GIM, GMP, GVR, PNG)";
            imgConversion.Text      = "Image Unpacker\n(Convert to PNG)";
            imgConversion.Location  = new Point(8, 184);
            imgConversion.Size      = new Size(this.Width - (imgConversion.Location.X * 2), 32);
            imgConversion.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            imgConversion.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(imgConversion);

            /* Display program buttons */
            buttonPrograms[0].Text     = labelPrograms[0];
            buttonPrograms[0].Location = new Point(8, 40);
            buttonPrograms[0].Size     = new Size(160, 32);
            buttonPrograms[0].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[0]);

            buttonPrograms[1].Text = labelPrograms[3];
            buttonPrograms[1].Location = new Point(176, 40);
            buttonPrograms[1].Size = new Size(160, 32);
            buttonPrograms[1].Click += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[1]);

            buttonPrograms[2].Text     = labelPrograms[2];
            buttonPrograms[2].Location = new Point(8, 128);
            buttonPrograms[2].Size     = new Size(160, 32);
            buttonPrograms[2].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[2]);

            buttonPrograms[3].Text     = labelPrograms[3];
            buttonPrograms[3].Location = new Point(176, 128);
            buttonPrograms[3].Size     = new Size(160, 32);
            buttonPrograms[3].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[3]);

            buttonPrograms[4].Text     = labelPrograms[4];
            buttonPrograms[4].Location = new Point(8, 216);
            buttonPrograms[4].Size     = new Size(160, 32);
            buttonPrograms[4].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[4]);

            //aboutProgram();

            buttonPrograms[5].Text     = labelPrograms[5];
            buttonPrograms[5].Location = new Point(176, 216);
            buttonPrograms[5].Size     = new Size(160, 32);
            buttonPrograms[5].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[5]);

            buttonPrograms[6].Text = "Archive Explorer";
            buttonPrograms[6].Location = new Point(8, 280);
            buttonPrograms[6].Size = new Size(160, 32);
            buttonPrograms[6].Click += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[6]);
        }

        private void startProgram(object sender, EventArgs e)
        {
            if (sender == buttonPrograms[0]) // File Decompressor
            {
                //Compression_Decompressor decompressor = new Compression_Decompressor();
                Compression_Decompress program = new Compression_Decompress();
            }

            if (sender == buttonPrograms[1]) // File Decompressor
            {
                //Compression_Decompressor decompressor = new Compression_Decompressor();
                Compression_Decompress program = new Compression_Decompress(true);
                //Compression_Compress program = new Compression_Compress();
            }

            else if (sender == buttonPrograms[2]) // Archive Extractor
            {
                //Archive_Extractor decompressor = new Archive_Extractor();
                Archive_Extract program = new Archive_Extract();
            }

            else if (sender == buttonPrograms[3]) // Archive Creator
            {
                //Archive_Creator decompressor = new Archive_Creator();
                //Archive_Create program = new Archive_Create();
                Archive_Extract program = new Archive_Extract(true);
                //Archive_Explorer program = new Archive_Explorer();
            }

            else if (sender == buttonPrograms[4]) // Image Converter
            {
                //Image_Converter program = new Image_Converter();
                Image_Convert program = new Image_Convert();
            }

            else if (sender == buttonPrograms[5]) // Image Converter
            {
                //Image_Converter program = new Image_Converter();
                Image_Convert program = new Image_Convert(true);
            }

            else if (sender == buttonPrograms[6]) // Image Converter
            {
                Archive_Explorer program = new Archive_Explorer();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new puyo_tools());
        }

        /* About Puyo Tools */
        private void aboutProgram()
        {
            MessageBox.Show(this,
                "Puyo Tools" + "\n" +
                "Version 0.12" + "\n\n" +
                "Written by nmn and Nick Woronekin" + "\n\n" +
                "Special Thanks:" + "\n" +
                "Luke Zapart (drx) - CNX Decompressor",
                "About Puyo Tools",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}