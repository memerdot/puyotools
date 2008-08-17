using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;

namespace pp15_fileTools
{
    public class pp15_fileTools : Form
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
            new Button(), // AFS Extractor
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
            "AFS Extractor",            // AFS Exractor
            "AFS Creator",              // AFS Creator
            "GIM <-> PNG Converter",    // GIM <-> PNG
            "GVR <-> PNG Converter"     // GVR <-> PNG
        };


        public pp15_fileTools()
        {
            /* Set up form options */
            this.ClientSize      = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Puyo Puyo! 15th Anniversary File Tools";

            /* Display program buttons */
            buttonPrograms[0].Text     = labelPrograms[0];
            buttonPrograms[0].Location = new Point(8, 8);
            buttonPrograms[0].Size     = new Size(160, 32);
            buttonPrograms[0].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[0]);

            buttonPrograms[3].Text     = labelPrograms[3];
            buttonPrograms[3].Location = new Point(8, 48);
            buttonPrograms[3].Size     = new Size(160, 32);
            buttonPrograms[3].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[3]);

            buttonPrograms[6].Text     = labelPrograms[6];
            buttonPrograms[6].Location = new Point(8, 88);
            buttonPrograms[6].Size     = new Size(160, 32);
            buttonPrograms[6].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[6]);

            buttonPrograms[7].Text     = labelPrograms[7];
            buttonPrograms[7].Location = new Point(8, 128);
            buttonPrograms[7].Size     = new Size(160, 32);
            buttonPrograms[7].Click   += new EventHandler(startProgram);
            this.Controls.Add(buttonPrograms[7]);
        }

        private void startProgram(object sender, EventArgs e)
        {
            if (sender == buttonPrograms[0]) // LZ01/CXLZ Decompressor
            {
                LZ01_CNX_CXLZ_Decompress decompressor = new LZ01_CNX_CXLZ_Decompress();
            }

            else if (sender == buttonPrograms[3]) // SNT/GNT Extractor
            {
                SNT_GNT_Extract extractor = new SNT_GNT_Extract();
            }

            else if (sender == buttonPrograms[6]) // AFS Exractor
            {
                AFS_Extract decompressor = new AFS_Extract();
            }

            else if (sender == buttonPrograms[7]) // AFS Creator
            {
                AFS_Create decompressor = new AFS_Create();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new pp15_fileTools());
        }
    }
}