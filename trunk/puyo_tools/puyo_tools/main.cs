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
            // Create the window
            FormContent.Create(this, "Puyo Tools", new Size(344, 96));

            // Build the menu
            BuildMenu();

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
                new ToolStripMenuItem("Select Files",     null, LaunchProgram), // Image Encoder
                new ToolStripMenuItem("Select Directory", null, LaunchProgram), // Image Encoder
                new ToolStripMenuItem("Image Viewer",     null, LaunchProgram), // Viewer
            };

            if (Type.GetType("Mono.Runtime") != null)
            {
                /* Build the menu (Mono) */
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
                        new ItemArray("Encoder", new ItemIterator[] {
                            new Item(programItem[10]),
                            new Item(programItem[11]),
                        }),
                        new Item(programItem[12]),
                    }),
                    new Item(new ToolStripSeparator()),
                    new Item("About", About),
                });

                /* Setup menu */
                ToolStrip toolStrip = new ToolStrip();
                toolStrip.Items.AddRange((ToolStripItem[])Menu.buildToolStripItemArray());
                this.Controls.Add(toolStrip);
            }
            else
            {
                /* Build the menu (Visual C#) */
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
                        new ToolStripMenuItem("Encoder", null, new ToolStripItem[] {
                            programItem[10],
                            programItem[11],
                        }),
                        programItem[12],
                    }),
                    new ToolStripSeparator(),
                    new ToolStripButton("About", null, About),
                });
                this.Controls.Add(toolStrip);

                /* Draw logo */
                this.Controls.Add(new PictureBox() {
                    Image = new Bitmap((Bitmap)new ComponentResourceManager(typeof(images)).GetObject("logo")),
                    Location = new Point(16, 32),
                    Size = new Size(316, 47),
                });
            }
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
            else if (sender == programItem[10]) program = new Image_Encoder();
            else if (sender == programItem[11]) program = new Image_Encoder(true);
            else if (sender == programItem[12]) program = new Image_Viewer();
        }

        [STAThread]
        static void Main(string[] args)
        {
            Initalize();
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

            /* Credits */
            /*AboutDialog.Controls.Add(new Label() {
                Text = 
                    "Lightning - ssr-one-util, which was used to add support for Storybook Archives\n\n" +
                    "drx - CNX and LZ01 decompression\n",
                Location = new Point(8, 8),
                Size = new Size(AboutDialog.Size.Width - 16, bottom.Location.Y - 8),
            });*/

            bottom.Controls.Add(close);
            AboutDialog.Controls.Add(bottom);

            AboutDialog.ShowDialog();
        }

        // Initalize
        // THIS MUST BE CALLED OTHERWISE NOTHING WILL WORK
        // AND YOU WILL GET ERRORS
        private static void Initalize()
        {
            // Initalize the dictionaries
            Compression.InitalizeDictionary();
            Archive.InitalizeDictionary();
            Images.InitalizeDictionary();
        }

        // Build Menu
        private static void BuildMenu()
        {
        }
    }
}