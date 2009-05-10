using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class puyo_tools : Form
    {
        private ToolStripMenuItem[] programItem;

        public puyo_tools()
        {
            /* Create the form */
            FormContent.Create(this, "Puyo Tools", new Size(344, 96));

            /* Create the tool menu */
            programItem = new ToolStripMenuItem[] { // Prorams
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Decompressor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Decompressor
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Compressor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Compressor
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Extractor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Extractor
                new ToolStripMenuItem("Create",           null, LaunchProgram), // Create
                new ToolStripMenuItem("Archive Explorer", null, LaunchProgram), // Explorer
            };

            ToolStrip toolStrip = new ToolStrip(new ToolStripItem[] {
                new ToolStripDropDownButton("Compression", null, new ToolStripMenuItem[] {
                    new ToolStripMenuItem("Decompress", null, new ToolStripItem[] {
                        programItem[0],
                        programItem[1],
                    }),
                    new ToolStripMenuItem("Compress", null, new ToolStripItem[] {
                        programItem[2],
                        programItem[3],
                    }),
                }),
                new ToolStripSeparator(),
                new ToolStripDropDownButton("Archives", null, new ToolStripMenuItem[] {
                    new ToolStripMenuItem("Extractor", null, new ToolStripItem[] {
                        programItem[4],
                        programItem[5],
                    }),
                    programItem[6],
                    programItem[7],
                }),
                new ToolStripSeparator(),
                new ToolStripButton("About", null, aboutProgram),
            });
            this.Controls.Add(toolStrip);

            /* Draw logo */
            PictureBox logo = new PictureBox();
            logo.Image = new Bitmap((Bitmap)new ComponentResourceManager(typeof(images)).GetObject("logo"));
            logo.Location = new Point(16, 32);
            logo.Size = new Size(316, 47);
            this.Controls.Add(logo);
        }

        private void LaunchProgram(object sender, EventArgs e)
        {
            Form program = null;
                 if (sender == programItem[0]) program = new Compression_Decompress();
            else if (sender == programItem[1]) program = new Compression_Decompress(true);
            else if (sender == programItem[2]) program = new Compression_Compress();
            else if (sender == programItem[3]) program = new Compression_Compress(true);
            else if (sender == programItem[4]) program = new Archive_Extract();
            else if (sender == programItem[5]) program = new Archive_Extract(true);
            else if (sender == programItem[6]) program = new Archive_Create();
            else if (sender == programItem[7]) program = new Archive_Explorer();
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new puyo_tools());
        }

        /* About Puyo Tools */
        private void aboutProgram(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Puyo Tools" + "\n" +
                "Version 0.12 Alpha 4" + "\n\n" +
                "Written by nmn and Nick Woronekin" + "\n\n" +
                "Special Thanks:" + "\n" +
                "Luke Zapart (drx) - CNX Decompressor",
                "About Puyo Tools",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}