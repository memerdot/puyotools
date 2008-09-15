using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;

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
            new Button(), // GIM <-> PNG
            new Button()  // GVR <-> PNG
        };

        private string[] labelPrograms = // Labels for each program
        {
            "Decompress",            // File Decompressor
            "Compress",              // File Compressor
            "Extract",               // Archive Extractor
            "Create",                // Archive Creator
            "GIM <-> PNG Converter", // GIM <-> PNG
            "GVR <-> PNG Converter"  // GVR <-> PNG  
        };


        public puyo_tools()
        {
            /* Set up form options */
            this.ClientSize      = new Size(344, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Puyo Tools";

            /* Compression */
            Label compression = new Label();
            compression.Text      = "Compression\n(CXLZ, LZ01)";
            compression.Location  = new Point(8, 8);
            compression.Size      = new Size(this.Width - (compression.Location.X * 2), 32);
            compression.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            compression.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(compression);

            /* Archives */
            Label archives     = new Label();
            archives.Text      = "Archives\n(ACX, AFS, GNT, MRG, SNT, SPK, TEX, VDD)";
            archives.Location  = new Point(8, 96);
            archives.Size      = new Size(this.Width - (archives.Location.X * 2), 32);
            archives.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            archives.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(archives);

            /* Display program buttons */
            buttonPrograms[0].Text     = labelPrograms[0];
            buttonPrograms[0].Location = new Point(8, 40);
            buttonPrograms[0].Size     = new Size(160, 32);
            buttonPrograms[0].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[0]);

            //buttonPrograms[3].Text     = labelPrograms[3];
            //buttonPrograms[3].Location = new Point(8, 48);
            //buttonPrograms[3].Size     = new Size(160, 32);
            //buttonPrograms[3].Click   += new EventHandler(startProgram);
            //this.Controls.Add(buttonPrograms[3]);

            buttonPrograms[2].Text     = labelPrograms[2];
            //buttonPrograms[6].Location = new Point(8, 88);
            buttonPrograms[2].Location = new Point(8, 128);
            buttonPrograms[2].Size     = new Size(160, 32);
            buttonPrograms[2].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[2]);

            buttonPrograms[3].Text     = labelPrograms[3];
            buttonPrograms[3].Location = new Point(176, 128);
            buttonPrograms[3].Size     = new Size(160, 32);
            buttonPrograms[3].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[3]);

            //buttonPrograms[10].Text = labelPrograms[10];
            //buttonPrograms[10].Location = new Point(8, 168);
            //buttonPrograms[10].Size = new Size(160, 32);
            //buttonPrograms[10].Click += new EventHandler(startProgram);
            //this.Controls.Add(buttonPrograms[10]);
        }

        private void startProgram(object sender, EventArgs e)
        {
            if (sender == buttonPrograms[0]) // File Decompressor
            {
                LZ01_CXLZ_Decompress decompressor = new LZ01_CXLZ_Decompress();
            }

            else if (sender == buttonPrograms[2]) // Archive Extractor
            {
                archive_extract decompressor = new archive_extract();
            }

            else if (sender == buttonPrograms[3]) // Archive Creator
            {
                archive_create decompressor = new archive_create();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new puyo_tools());
        }
    }
}