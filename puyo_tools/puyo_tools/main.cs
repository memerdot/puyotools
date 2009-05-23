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

			/* Create the toolstrip */
            ToolStrip toolStrip = new ToolStrip();
            this.Controls.Add(toolStrip);
			
            /* Create the tool menu */
            programItem = new ToolStripMenuItem[] // Programs
			{
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Decompressor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Decompressor
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Compressor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Compressor
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Extractor
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Extractor
                new ToolStripMenuItem("Create",           null, LaunchProgram), // Create
                new ToolStripMenuItem("Archive Explorer", null, LaunchProgram), // Explorer
            };
			
			/* Build the menu */
			ItemIterator Menu = new ItemArray("Root",new ItemIterator[]
			{
				new ItemArray("Compression", new ItemIterator[]
				{
					new ItemArray("Decompress", new ItemIterator[]
					{
						new Item(programItem[0]),
						new Item(programItem[1]),
					}),
					new ItemArray("Compress", new ItemIterator[]
					{
						new Item(programItem[2]),
						new Item(programItem[3]),
					}),
				}),
				new Item(new ToolStripSeparator()),
				new ItemArray("Archives", new ItemIterator[]
				{
					new ItemArray("Extractor", new ItemIterator[]
					{
						new Item(programItem[4]),
						new Item(programItem[5]),
					}),
					new Item(programItem[6]),
					new Item(programItem[7]),
				}),
				new Item(new ToolStripSeparator()),
				new Item("About", aboutProgram),
			});
			
			/* Setup menu */
			toolStrip.Items.AddRange((ToolStripItem[])Menu.buildToolStripItemArray());
			
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