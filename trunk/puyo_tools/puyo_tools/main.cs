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
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Converter
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Converter
                new ToolStripMenuItem("Image Viewer",     null, LaunchProgram), // Viewer
            };
			
	        /* Build the menu (Mono) */
			Type t = Type.GetType ("Mono.Runtime");
			if (t != null)
			{
	            ItemIterator Menu = new ItemArray("Root", new ItemIterator[] {
	                new ItemArray("Compression", new ItemIterator[] {
	                    new ItemArray("Decompress", new ItemIterator[] {
	                        new Item(programItem[0]),
	                        new Item(programItem[1]),
	                    }),
	                    new ItemArray("Compress", new ItemIterator[] {
	                        new Item(programItem[2]),
	                        new Item(programItem[3]),
	                    }),
	                }),
	                new Item(new ToolStripSeparator()),
	                new ItemArray("Archives", new ItemIterator[] {
	                    new ItemArray("Extractor", new ItemIterator[] {
	                        new Item(programItem[4]),
	                        new Item(programItem[5]),
	                    }),
	                    new Item(programItem[6]),
	                    new Item(programItem[7]),
	                }),
	                new Item(new ToolStripSeparator()),
	                new ItemArray("Images", new ItemIterator[] {
	                    new ItemArray("Decoder", new ItemIterator[] {
	                        new Item(programItem[8]),
	                        new Item(programItem[9]),
	                    }),
	                    new Item(programItem[10]),
	                }),
	                new Item(new ToolStripSeparator()),
	                new Item("About", aboutProgram),
	            });
	
	            /* Setup menu */
	            ToolStrip toolStrip = new ToolStrip();
	            toolStrip.Items.AddRange((ToolStripItem[])Menu.buildToolStripItemArray());
	            this.Controls.Add(toolStrip);
			}
			else
			{
	        	/* Build the menu (MS.NET) */
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
	                new ToolStripDropDownButton("Images", null, new ToolStripMenuItem[] {
	                    new ToolStripMenuItem("Decoder", null, new ToolStripItem[] {
	                        programItem[8],
	                        programItem[9],
	                    }),
	                    programItem[10],
	                }),
	                new ToolStripSeparator(),
	                new ToolStripButton("About", null, About),
	            });
	            this.Controls.Add(toolStrip);
			}

            /* Draw logo */
            this.Controls.Add(new PictureBox() {
                Image = new Bitmap((Bitmap)new ComponentResourceManager(typeof(images)).GetObject("logo")),
                Location = new Point(16, 32),
                Size = new Size(316, 47),
            });
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
            else if (sender == programItem[8]) program = new Image_Convert();
            else if (sender == programItem[9]) program = new Image_Convert(true);
            else if (sender == programItem[10]) program = new Image_Viewer();
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new puyo_tools());
        }

        /* About */
        private void About(object sender, EventArgs e)
        {
            Form AboutDialog = new Form() {
                BackColor = Color.White,
            };
            FormContent.Create(AboutDialog, "About Puyo Tools", new Size(256, 400), false);


            Panel bottom = new Panel() {
                BackColor = this.BackColor,
                Location  = new Point(0, AboutDialog.Size.Height - 72),
                Size      = new Size(AboutDialog.Size.Width, 40),
            };
            Button close = new Button() {
                Text     = "Close",
                Location = new Point((AboutDialog.Size.Width / 2) - 32, 8),
                Size     = new Size(64, 24),
            };
            close.Click += delegate(object sender2, EventArgs f) {
                AboutDialog.Close();
            };

            bottom.Controls.Add(close);
            AboutDialog.Controls.Add(bottom);

            AboutDialog.ShowDialog();
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