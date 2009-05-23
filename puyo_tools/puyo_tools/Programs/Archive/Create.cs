using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace puyo_tools
{
    public class Archive_Create : Form
    {
        /* Set up our form variables */
        private CheckBox
            compressArchive = new CheckBox(); // Compress the Archive

        private ComboBox
            archiveFormat     = new ComboBox(), // Archive Format
            compressionFormat = new ComboBox(); // Compression Format

        private ComboBox[]
            blockSizes; // Block Sizes

        private Button
            startWorkButton = new Button(); // Start Work Button

        private ListView
            archiveFileList = new ListView(); // Contents of Archive

        private List<string>
            sourceFilenames  = new List<string>(), // List of source filenames
            archiveFilenames = new List<string>(); // List of filenames in the archive

        private List<string>
            fileList = new List<string>(); // Files

        private StatusMessage
            status; // Status Message

        private Archive
            archive = new Archive();

        private Form renameDialog;
        private TextBox renameTextBox;
        private bool renameAll;

        private ToolStripButton renameAllFiles;

        private TabControl settingsBox;
        private TabPage[] settingsPages;

        /* Archive Formats */
        ToolStripComboBox archiveFormatList;
        private List<ArchiveClass> archiver = new List<ArchiveClass>();
        private List<string>
            archiverName   = new List<string>(),
            archiverFilter = new List<string>();
        private List<int[]> archiverBlockSize = new List<int[]>();
        private List<int> archiverFilenameLength = new List<int>();

        private ArchiveFormat[] archiveFormats = new ArchiveFormat[] { // Archive Formats
            ArchiveFormat.ACX,
            ArchiveFormat.AFS,
            ArchiveFormat.GNT,
            ArchiveFormat.GVM,
            ArchiveFormat.MDL,
            ArchiveFormat.MRG,
            ArchiveFormat.NARC,
            ArchiveFormat.ONE,
            ArchiveFormat.PVM,
            ArchiveFormat.SNT,
            ArchiveFormat.SPK,
            ArchiveFormat.TEX,
            ArchiveFormat.TXAG,
            ArchiveFormat.VDD,
        };

        /* Hashtable settings */
        private Dictionary<ArchiveFormat, CheckBox[]> table_settings = new Dictionary<ArchiveFormat, CheckBox[]>();

        /* Compression Formats */
        ToolStripComboBox compressionFormatList;
        private List<CompressionClass> compressor = new List<CompressionClass>();
        private List<string> compressorName = new List<string>();

        private CompressionFormat[] compressionFormats = new CompressionFormat[] { // Compression Formats
            CompressionFormat.CXLZ,
            CompressionFormat.LZSS,
        };

        public Archive_Create()
        {

            /* Set up the form */
            FormContent.Create(this, "Archive - Create", new Size(640, 400));

            /* Fill the archive formats */
            foreach (ArchiveFormat format in archiveFormats)
            {
                ArchiveClass archive;
                string name, filter;
                int[] blockSize;
                int filenameLength;

                new Archive().ArchiveInformation(format, out archive, out name, out filter, out blockSize, out filenameLength);

                archiver.Add(archive);
                archiverName.Add(name);
                archiverFilter.Add(filter);
                archiverBlockSize.Add(blockSize);
                archiverFilenameLength.Add(filenameLength);
            }

            /* Fill the compression formats */
            foreach (CompressionFormat format in compressionFormats)
            {
                CompressionClass compression;
                string name;

                new Compression().CompressionInformation(format, out compression, out name);

                compressor.Add(compression);
                compressorName.Add(name);
            }



            /* Create the combobox containing the archive formats */
            archiveFormatList               = new ToolStripComboBox();
            archiveFormatList.DropDownStyle = ComboBoxStyle.DropDownList;
            archiveFormatList.Items.AddRange(archiverName.ToArray());
            archiveFormatList.SelectedIndex = 0;
            archiveFormatList.MaxDropDownItems = archiveFormatList.Items.Count;
            archiveFormatList.SelectedIndexChanged += new EventHandler(changeArchiveFormat);

            compressionFormatList               = new ToolStripComboBox();
            compressionFormatList.DropDownStyle = ComboBoxStyle.DropDownList;
            compressionFormatList.Items.Add("Do Not Compress");
            compressionFormatList.Items.AddRange(compressorName.ToArray());
            compressionFormatList.SelectedIndex = 0;

            renameAllFiles = new ToolStripButton("Rename All", null, rename);

            /* Create the toolstrip */
            ToolStrip toolStrip = new ToolStrip(new ToolStripItem[] {
                new ToolStripButton("Add", (Bitmap)new ComponentResourceManager(typeof(icons)).GetObject("new_file"), addFiles),
                new ToolStripSeparator(),
                new ToolStripLabel("Archive:"),
                archiveFormatList,
                new ToolStripSeparator(),
                new ToolStripLabel("Compression:"),
                compressionFormatList,
                new ToolStripSeparator(),
                renameAllFiles,
            });
            this.Controls.Add(toolStrip);

            /* Create the right click menu */
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Rename", null, rename),
                new ToolStripMenuItem("Delete", null, delete),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Move Up", null, moveUp),
                new ToolStripMenuItem("Move Down", null, moveDown),
            });
            archiveFileList.ContextMenuStrip = contextMenu;

            /* Archive Contents */
            FormContent.Add(this, archiveFileList,
                new string[] { "#", "Source Filename", "Filename in the Archive" },
                new int[] { 48, 170, 170 },
                new Point(8, 32),
                new Size(420, 328));

            /* Quick hack for setting height of list view items */
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(1, 16);
            archiveFileList.SmallImageList = imageList;

            /* Move File Up */
            //FormContent.Add(this, moveFileUp,
            //    "Move\nUp",
            //    new Point(424, 56),
            //    new Size(96, 32),
            //    null);

            /* Move File Down */
            //FormContent.Add(this, moveFileDown,
            //    "Move\nDown",
            //    new Point(424, 96),
            //    new Size(96, 32),
            //    null);

            /* Settings Box */
            settingsBox = new TabControl() {
                Location = new Point(436, 32),
                Size = new Size(196, 328),
            };

            /* Initalize the settings table and all the settings */
            initializeSettingsTable();
            settingsPages = new TabPage[archiveFormats.Length];
            blockSizes    = new ComboBox[archiveFormats.Length];
            for (int i = 0; i < archiveFormats.Length; i++)
            {
                settingsPages[i] = new TabPage(archiverName[i] + " Settings") {
                    UseVisualStyleBackColor = true,
                };

                /* Add block size */
                settingsPages[i].Controls.Add(new Label() {
                    Text     = "Block Size",
                    Location = new Point(8, 8),
                    Size     = new Size(64, 16),
                });

                /* Now add the entries */
                blockSizes[i] = new ComboBox() {
                    Location = new Point(72, 4),
                    Size     = new Size(64, 16),
                    DropDownStyle = ComboBoxStyle.DropDown,
                };

                for (int j = 0; j < archiverBlockSize[i].Length; j++)
                {
                    /* If the last element is -1, we aren't allowed to edit it */
                    if (archiverBlockSize[i][j] == -1)
                    {
                        blockSizes[i].Enabled = false;
                        break;
                    }
                    blockSizes[i].Items.Add(archiverBlockSize[i][j].ToString());
                }

                blockSizes[i].SelectedIndex    = 0;
                blockSizes[i].MaxDropDownItems = blockSizes[i].Items.Count;
                settingsPages[i].Controls.Add(blockSizes[i]);


                for (int j = 0; j < table_settings[archiveFormats[i]].Length; j++)
                {
                    table_settings[archiveFormats[i]][j].Location = new Point(8, 32 + (j * 24));
                    table_settings[archiveFormats[i]][j].Size     = new Size(settingsBox.Size.Width - 16, 16);
                    settingsPages[i].Controls.Add(table_settings[archiveFormats[i]][j]);
                }
            }

            this.Controls.Add(settingsBox);
            settingsBox.TabPages.Add(settingsPages[0]);

            /* Create Archive */
            FormContent.Add(this, startWorkButton,
                "Create Archive",
                new Point((this.Width / 2) - 60, 368),
                new Size(120, 24),
                startWork);

            this.ShowDialog();
        }

        /* Start Work */
        private void startWork(object sender, EventArgs e)
        {
            /* Set up our background worker */
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += run;

            /* Now, show our status */
            status = new StatusMessage("Archive - Create", fileList.ToArray());

            bw.RunWorkerAsync();
            status.ShowDialog();
        }

        /* Create the archive */
        private void run(object sender, DoWorkEventArgs e)
        {
            try
            {
                /* Get output filename */
                string output_filename = FileSelectionDialog.SaveFile("Create Archive", String.Empty, archiverFilter[archiveFormatList.SelectedIndex]);

                if (output_filename == null || output_filename == String.Empty)
                    return;

                /* First, disable the buttons */
                startWorkButton.Enabled = false;
                renameAllFiles.Enabled  = false;

                /* Start creating the archive */
                Archive archive = new Archive(archiveFormats[archiveFormatList.SelectedIndex], Path.GetFileName(output_filename), archiver[archiveFormatList.SelectedIndex]);

                using (FileStream outputStream = new FileStream(output_filename, FileMode.Create, FileAccess.ReadWrite))
                {
                    /* Make sure block size is a number */
                    int blockSize = 0;
                    if (!int.TryParse(blockSizes[archiveFormatList.SelectedIndex].Text, out blockSize) || blockSize < 1)
                        blockSize = archiverBlockSize[archiveFormatList.SelectedIndex][0];

                    /* Create and write the header */
                    bool[] settings = getSettings(archiveFormats[archiveFormatList.SelectedIndex]);
                    List<uint> offsetList;
                    List<byte> header = archive.CreateHeader(fileList.ToArray(), archiveFilenames.ToArray(), blockSize, settings, out offsetList);
                    (new MemoryStream(header.ToArray())).WriteTo(outputStream);

                    /* Add the files */
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        /* Pad file so we can have the correct block offset */
                        while (outputStream.Position < offsetList[i])
                            outputStream.WriteByte(archive.PaddingByte);

                        using (Stream inputStream = new FileStream(fileList[i], FileMode.Open, FileAccess.Read))
                        {
                            /* Format file to add */
                            Stream inputFileData = inputStream;
                            MemoryStream fileData = (MemoryStream)archive.FormatFileToAdd(ref inputFileData);
                            if (fileData == null)
                                fileData = (MemoryStream)StreamConverter.Copy(inputStream);

                            /* Write the data now */
                            fileData.WriteTo(outputStream);
                        }
                    }

                    /* Pad file so we can have the correct block offset */
                    while (outputStream.Position % blockSize != 0)
                        outputStream.WriteByte(archive.PaddingByte);

                    /* Write the footer */
                    List<byte> footer = archive.CreateFooter(fileList.ToArray(), archiveFilenames.ToArray(), blockSize, settings, ref header);
                    if (footer != null)
                    {
                        (new MemoryStream(footer.ToArray())).WriteTo(outputStream);

                        /* Pad file so we can have the correct block offset */
                        while (outputStream.Position % blockSize != 0)
                            outputStream.WriteByte(archive.PaddingByte);
                    }

                    /* Do we want to compress the data? */
                    if (compressionFormatList.SelectedIndex != 0)
                    {
                        Compression compression     = new Compression(outputStream, output_filename, compressionFormats[compressionFormatList.SelectedIndex - 1], compressor[compressionFormatList.SelectedIndex - 1]);
                        MemoryStream compressedData = (MemoryStream)compression.Compress();
                        if (compressedData != null)
                        {
                            outputStream.Position = 0;
                            compressedData.WriteTo(outputStream);
                            outputStream.SetLength(compressedData.Length);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                status.Close();
                this.Close();
            }
        }

        /* Change Archive Format */
        private void changeArchiveFormat(object sender, EventArgs e)
        {
            settingsBox.TabPages.Clear();
            settingsBox.TabPages.Add(settingsPages[archiveFormatList.SelectedIndex]);
        }

        /* Add files */
        private void addFiles(object sender, EventArgs e)
        {
            string[] files = FileSelectionDialog.OpenFiles("Select Files", "All Files (*.*)|*.*");

            if (files == null || files.Length == 0)
                return;

            /* Add the files now */
            foreach (string file in files)
            {
                fileList.Add(file);
                sourceFilenames.Add(Path.GetFileName(file));
                archiveFilenames.Add(Path.GetFileName(file));

                archiveFileList.Items.Add(new ListViewItem(new string[] {
                    fileList.Count.ToString("#,0"),
                    sourceFilenames[sourceFilenames.Count - 1],
                    archiveFilenames[archiveFilenames.Count - 1],
                }));
            }
        }

        /* Remove files */
        private void delete(object sender, EventArgs e)
        {
            /* Make sure we selected something */
            if (archiveFileList.SelectedIndices.Count == 0)
                return;

            /* Remove items */
            foreach (int item in archiveFileList.SelectedIndices)
            {
                fileList.RemoveAt(item);
                sourceFilenames.RemoveAt(item);
                archiveFilenames.RemoveAt(item);
            }

            /* Populate the list again */
            populateList();
        }

        /* Rename files */
        private void rename(object sender, EventArgs e)
        {
            /* Make sure we have items first */
            if (archiveFileList.Items.Count == 0)
                return;

            /* If we want to rename all, select all the items */
            if (sender == renameAllFiles)
                renameAll = true;
            else if (archiveFileList.SelectedIndices.Count == 0)
                return;

            renameDialog  = new Form();
            renameTextBox = new TextBox();
            string selectedFilename = String.Empty;

            if (!renameAll)
                selectedFilename = archiveFilenames[archiveFileList.SelectedIndices[0]];

            FormContent.Create(renameDialog,
                "Rename Files",
                //String.Format("Rename {0}",
                    //(renameAllFiles ? "All Files" : (selectedFilename == String.Empty ? "nameless file" : selectedFilename))),
                new Size(320, 100), false);

            /* Rename this to */
            FormContent.Add(renameDialog, new Label(),
                "Rename selected files to:",
                //String.Format("Rename {0} to:\n(Leave blank for no filename)", (renameAllFiles ? "all files" : (selectedFilename == String.Empty ? "nameless file" : selectedFilename))),
                new Point(8, 8),
                new Size(304, 32));

            /* Display Text Box */
            FormContent.Add(renameDialog, renameTextBox,
                selectedFilename,
                new Point(8, 40),
                new Size(304, 24));

            /* Display Rename Button */
            FormContent.Add(renameDialog, new Button(),
                "Rename",
                new Point(94, 68),
                new Size(64, 24),
                doRename);

            /* Display Cancel Button */
            FormContent.Add(renameDialog, new Button(),
                "Cancel",
                new Point(162, 68),
                new Size(64, 24),
                cancelRename);

            renameDialog.ShowDialog();
        }

        /* Do rename */
        private void doRename(object sender, EventArgs e)
        {
            /* Rename selected files, or all files */
            if (renameAll)
            {
                for (int i = 0; i < archiveFileList.Items.Count; i++)
                    archiveFilenames[i] = renameTextBox.Text;

                renameAll = false;
            }
            else
            {
                foreach (int item in archiveFileList.SelectedIndices)
                    archiveFilenames[item] = renameTextBox.Text;
            }

            renameDialog.Close();

            /* Populate the list */
            populateList();
        }

        /* Cancel rename */
        private void cancelRename(object sender, EventArgs e)
        {
            renameDialog.Close();
        }

        /* Move item up in the list */
        private void moveUp(object sender, EventArgs e)
        {
            /* Make sure we selected stuff */
            if (archiveFileList.SelectedIndices.Count == 0)
                return;

            /* Make sure we didn't select the first one */
            if (archiveFileList.SelectedIndices.Contains(0))
                return;

            /* Move files up in the list now */
            for (int i = 0; i < archiveFileList.SelectedIndices.Count; i++)
            {
                int item = archiveFileList.SelectedIndices[i];

                string file        = fileList[item];
                string sourceFile  = sourceFilenames[item];
                string archiveFile = archiveFilenames[item];
                fileList.RemoveAt(item);
                sourceFilenames.RemoveAt(item);
                archiveFilenames.RemoveAt(item);
                fileList.Insert(item - 1, file);
                sourceFilenames.Insert(item - 1, sourceFile);
                archiveFilenames.Insert(item - 1, archiveFile);
            }

            /* Populate the list */
            populateList();
        }

        /* Move items down in the list */
        private void moveDown(object sender, EventArgs e)
        {
            /* Make sure we selected stuff */
            if (archiveFileList.SelectedIndices.Count == 0)
                return;

            /* Make sure we didn't select the last one */
            if (archiveFileList.SelectedIndices.Contains(archiveFileList.Items.Count - 1))
                return;

            /* Move files down in the list now */
            for (int i = 0; i < archiveFileList.SelectedIndices.Count; i++)
            {
                int item = archiveFileList.SelectedIndices[i];

                string file        = fileList[item];
                string sourceFile  = sourceFilenames[item];
                string archiveFile = archiveFilenames[item];
                fileList.RemoveAt(item);
                sourceFilenames.RemoveAt(item);
                archiveFilenames.RemoveAt(item);
                fileList.Insert(item + 1, file);
                sourceFilenames.Insert(item + 1, sourceFile);
                archiveFilenames.Insert(item + 1, archiveFile);
            }

            /* Populate the list */
            populateList();
        }

        /* Populate the list */
        private void populateList()
        {
            /* Clear the file list first */
            archiveFileList.Items.Clear();

            /* Populate it now! */
            for (int i = 0; i < fileList.Count; i++)
            {
                archiveFileList.Items.Add(new ListViewItem(new string[] {
                    (i + 1).ToString("#,0"),
                    sourceFilenames[i],
                    archiveFilenames[i],
                }));
            }
        }

        /* Initalize settings table */
        private void initializeSettingsTable()
        {
            /* Add the archive formats */
            foreach (ArchiveFormat format in archiveFormats)
                table_settings.Add(format, null);

            /* ACX Settings */
            table_settings[ArchiveFormat.ACX] = new CheckBox[] {
                new CheckBox() {
                    Text     = "Add filenames",
                    Checked  = false,
                }};

            /* AFS Settings */
            table_settings[ArchiveFormat.AFS] = new CheckBox[] {
                new CheckBox() {
                    Text     = "Use AFS v1",
                    Checked  = false,
                },
                new CheckBox() {
                    Text     = "Store Creation Time",
                    Checked  = true,
                }};

            /* GNT Settings */
            table_settings[ArchiveFormat.GNT] = new CheckBox[] {
                new CheckBox() {
                    Text     = "Add filenames",
                    Checked  = false,
                }};

            /* GVM Settings */
            table_settings[ArchiveFormat.GVM] = new CheckBox[] {
                new CheckBox() {
                    Text = "Add Filenames",
                    Checked = true,
                },
                new CheckBox() {
                    Text = "Add GVR Pixel Format",
                    Checked = true,
                },
                new CheckBox() {
                    Text = "Add GVR Dimensions",
                    Checked = true,
                },
                new CheckBox() {
                    Text = "Add GVR Global Index",
                    Checked = true,
                }};

            /* MDL Settings */
            table_settings[ArchiveFormat.MDL] = new CheckBox[] {
                new CheckBox() {
                    Text     = "Add filenames",
                    Checked  = false,
                }};

            /* MRG Settings */
            table_settings[ArchiveFormat.MRG] = new CheckBox[0];

            /* NARC Settings */
            table_settings[ArchiveFormat.NARC] = new CheckBox[] {
                new CheckBox() {
                    Text     = "Add filenames",
                    Checked  = true,
                }};

            /* ONE Settings */
            table_settings[ArchiveFormat.ONE] = new CheckBox[0];

            /* PVM Settings */
            table_settings[ArchiveFormat.PVM] = new CheckBox[] {
                new CheckBox() {
                    Text    = "Add Filenames",
                    Checked = true,
                },
                new CheckBox() {
                    Text    = "Add PVR Pixel Format",
                    Checked = true,
                },
                new CheckBox() {
                    Text    = "Add PVR Dimensions",
                    Checked = true,
                },
                new CheckBox() {
                    Text    = "Add PVR Global Index",
                    Checked = true,
                }};

            /* SNT Settings */
            table_settings[ArchiveFormat.SNT] = new CheckBox[] {
                new CheckBox() {
                    Text    = "PSP SNT Archive",
                    Checked = false,
                },
                new CheckBox() {
                    Text     = "Add filenames",
                    Checked  = false,
                }};

            /* SPK Settings */
            table_settings[ArchiveFormat.SPK] = new CheckBox[0];

            /* TEX Settings */
            table_settings[ArchiveFormat.TEX] = new CheckBox[0];

            /* TXAG Settings */
            table_settings[ArchiveFormat.TXAG] = new CheckBox[0];

            /* VDD Settings */
            table_settings[ArchiveFormat.VDD] = new CheckBox[0];
        }

        /* Get the settings for each archive format */
        private bool[] getSettings(ArchiveFormat format)
        {
            bool[] settings = new bool[table_settings[format].Length];

            for (int i = 0; i < settings.Length; i++)
                settings[i] = table_settings[format][i].Checked;

            return settings;
        }
    }
}