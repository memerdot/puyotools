using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;

namespace pp_tools
{
    public class pp_tools : Form
    {
        /* Buttons & Labels */
        private Button[] buttonPrograms = // Buttons for each program
        {
            new Button(), // LZ01/CXLZ Decompressor
            new Button(), // LZ01 Compressor
            new Button(), // CXLZ Compressor
            new Button(), // SNT/GNT Extractor
            new Button(), // SNT Creator
            new Button(), // GNT Creator
            new Button(), // Archive Extractor
            new Button(), // AFS Creator
            new Button(), // GIM <-> PNG
            new Button()  // GVR <-> PNG
        };

        private string[] labelPrograms = // Labels for each program
        {
            "LZ01 / CXLZ Decompressor", // LZ01/CXLZ Decompressor
            "LZ01 Compressor",          // LZ01 Compressor
            "CXLZ Compressor",          // CXLZ Compressor
            "SNT / GNT Extractor",      // SNT/GNT Extractor
            "SNT Creator",              // SNT Creator
            "GNT Creator",              // GNT Creator
            "Archive Extractor",        // Archive Extractor
            "AFS Creator",              // AFS Creator
            "GIM <-> PNG Converter",    // GIM <-> PNG
            "GVR <-> PNG Converter"     // GVR <-> PNG  
        };


        public pp_tools()
        {
            /* Set up form options */
            this.ClientSize      = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Puyo Puyo! Tools";

            /* Display program buttons */
            buttonPrograms[0].Text     = labelPrograms[0];
            buttonPrograms[0].Location = new Point(8, 8);
            buttonPrograms[0].Size     = new Size(160, 32);
            buttonPrograms[0].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[0]);

            //buttonPrograms[3].Text     = labelPrograms[3];
            //buttonPrograms[3].Location = new Point(8, 48);
            //buttonPrograms[3].Size     = new Size(160, 32);
            //buttonPrograms[3].Click   += new EventHandler(startProgram);
            //this.Controls.Add(buttonPrograms[3]);

            buttonPrograms[6].Text     = labelPrograms[6];
            //buttonPrograms[6].Location = new Point(8, 88);
            buttonPrograms[6].Location = new Point(8, 48);
            buttonPrograms[6].Size     = new Size(160, 32);
            buttonPrograms[6].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[6]);

            //buttonPrograms[7].Text     = labelPrograms[7];
            //buttonPrograms[7].Location = new Point(8, 128);
            //buttonPrograms[7].Size     = new Size(160, 32);
            //buttonPrograms[7].Click   += new EventHandler(startProgram);
            //this.Controls.Add(buttonPrograms[7]);

            //buttonPrograms[10].Text = labelPrograms[10];
            //buttonPrograms[10].Location = new Point(8, 168);
            //buttonPrograms[10].Size = new Size(160, 32);
            //buttonPrograms[10].Click += new EventHandler(startProgram);
            //this.Controls.Add(buttonPrograms[10]);
        }

        private void startProgram(object sender, EventArgs e)
        {
            if (sender == buttonPrograms[0]) // LZ01/CXLZ Decompressor
            {
                LZ01_CXLZ_Decompress decompressor = new LZ01_CXLZ_Decompress();
            }

            else if (sender == buttonPrograms[6]) // Archive Extractor
            {
                archive_extract decompressor = new archive_extract();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new pp_tools());
        }
    }
}