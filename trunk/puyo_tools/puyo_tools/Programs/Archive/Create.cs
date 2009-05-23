using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class Archive_Create : Form
    {
        /* Set up our form variables */
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

        private Form renameDialog;
        private TextBox renameTextBox;
        private bool renameAll;

        private ToolStripButton renameAllFiles;

        private TabControl settingsBox;
        private TabPage[] settingsPages;

        /* Archive & Compression Information */
        private List<ArchiveInformation> archiveInformation = new List<ArchiveInformation>();
        private List<CompressionInformation> compressionInformation = new List<CompressionInformation>();

        /* Archive & Compression Formats */
        ToolStripComboBox archiveFormatList;
        ToolStripComboBox compressionFormatList;

        public Archive_Create()
        {

            /* Set up the form */
            FormContent.Create(this, "Archive - Create", new Size(640, 400));

            /* Fill the archive & compression formats */
            initalizeArchiveInformation();
            initalizeCompressionInformation();

            /* Create the combobox that contains the archive formats */
            archiveFormatList               = new ToolStripComboBox();
            archiveFormatList.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (ArchiveInformation archiveInfo in archiveInformation)
                archiveFormatList.Items.Add(archiveInfo.Name);

            archiveFormatList.SelectedIndex         = 0;
            archiveFormatList.MaxDropDownItems      = archiveFormatList.Items.Count;
            archiveFormatList.SelectedIndexChanged += new EventHandler(changeArchiveFormat);

            /* Create the combobox that contains the compression formats */
            compressionFormatList               = new ToolStripComboBox();
            compressionFormatList.DropDownStyle = ComboBoxStyle.DropDownList;
            compressionFormatList.Items.Add("Do Not Compress");
            foreach (CompressionInformation compressionInfo in compressionInformation)
                compressionFormatList.Items.Add(compressionInfo.Name);

            compressionFormatList.SelectedIndex    = 0;
            compressionFormatList.MaxDropDownItems = compressionFormatList.Items.Count;

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
            archiveFileList.SmallImageList = new ImageList() {
                ImageSize = new Size(1, 16),
            };

            /* Settings Box */
            settingsBox = new TabControl() {
                Location = new Point(436, 32),
                Size     = new Size(196, 328),
            };

            /* Set up the settings */
            settingsPages = new TabPage[archiveInformation.Count];
            blockSizes    = new ComboBox[archiveInformation.Count];
            for (int i = 0; i < archiveInformation.Count; i++)
            {
                settingsPages[i] = new TabPage(archiveInformation[i].Name + " Settings") {
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

                for (int j = 0; j < archiveInformation[i].BlockSize.Length; j++)
                {
                    /* If the last element is -1, we aren't allowed to edit it */
                    if (archiveInformation[i].BlockSize[j] == -1)
                    {
                        blockSizes[i].Enabled = false;
                        break;
                    }
                    blockSizes[i].Items.Add(archiveInformation[i].BlockSize[j].ToString());
                }

                blockSizes[i].SelectedIndex    = 0;
                blockSizes[i].MaxDropDownItems = blockSizes[i].Items.Count;
                settingsPages[i].Controls.Add(blockSizes[i]);


                for (int j = 0; j < archiveInformation[i].Settings.Length; j++)
                {
                    archiveInformation[i].Settings[j].Location = new Point(8, 32 + (j * 24));
                    archiveInformation[i].Settings[j].Size     = new Size(settingsBox.Size.Width - 16, 16);
                    settingsPages[i].Controls.Add(archiveInformation[i].Settings[j]);
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
            /* Make sure we have files */
            if (fileList.Count == 0)
                return;

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
                string output_filename = FileSelectionDialog.SaveFile("Create Archive", String.Empty, archiveInformation[archiveFormatList.SelectedIndex].Filter);

                if (output_filename == null || output_filename == String.Empty)
                    return;

                /* First, disable the buttons */
                startWorkButton.Enabled = false;
                renameAllFiles.Enabled  = false;

                /* Start creating the archive */
                Archive archive = new Archive(archiveInformation[archiveFormatList.SelectedIndex].Format, Path.GetFileName(output_filename), archiveInformation[archiveFormatList.SelectedIndex].Archiver);

                using (FileStream outputStream = new FileStream(output_filename, FileMode.Create, FileAccess.ReadWrite))
                {
                    /* Make sure block size is a number */
                    int blockSize = 0;
                    if (!int.TryParse(blockSizes[archiveFormatList.SelectedIndex].Text, out blockSize) || blockSize < 1)
                        blockSize = archiveInformation[archiveFormatList.SelectedIndex].BlockSize[0];

                    /* Create and write the header */
                    bool[] settings = getSettings(archiveFormatList.SelectedIndex);
                    uint[] offsetList;
                    MemoryStream header = archive.CreateHeader(fileList.ToArray(), archiveFilenames.ToArray(), blockSize, settings, out offsetList);
                    outputStream.Write(header);

                    /* Add the files */
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        /* Pad file so we can have the correct block offset */
                        while (outputStream.Position < offsetList[i])
                            outputStream.WriteByte(archive.PaddingByte);

                        using (Stream inputStream = new FileStream(fileList[i], FileMode.Open, FileAccess.Read))
                        {
                            /* Format the file so we can add it */
                            Stream inputFile = inputStream;
                            inputFile = archive.FormatFileToAdd(ref inputFile);
                            if (inputFile == null)
                                throw new Exception();

                            /* Write the data to the file */
                            outputStream.Write(inputFile);
                        }
                    }

                    /* Pad file so we can have the correct block offset */
                    while (outputStream.Position % blockSize != 0)
                        outputStream.WriteByte(archive.PaddingByte);

                    /* Write the footer */
                    MemoryStream footer = archive.CreateFooter(fileList.ToArray(), archiveFilenames.ToArray(), blockSize, settings, ref header);
                    if (footer != null)
                    {
                        outputStream.Write(footer);

                        /* Pad file so we can have the correct block offset */
                        while (outputStream.Position % blockSize != 0)
                            outputStream.WriteByte(archive.PaddingByte);
                    }

                    /* Compress the data if we want to */
                    if (compressionFormatList.SelectedIndex != 0)
                    {
                        Compression compression     = new Compression(outputStream, output_filename, compressionInformation[compressionFormatList.SelectedIndex - 1].Format, compressionInformation[compressionFormatList.SelectedIndex - 1].Compressor);
                        MemoryStream compressedData = compression.Compress();
                        if (compressedData != null)
                        {
                            /* Clear the output stream and write the compressed data */
                            outputStream.Position = 0;
                            outputStream.SetLength(compressedData.Length);
                            outputStream.Write(compressedData);
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
                new Size(320, 100), false);

            /* Rename this to */
            FormContent.Add(renameDialog, new Label(),
                "Rename selected files to:",
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

        /* Initalize Settings */
        private void initalizeArchiveInformation()
        {
            /* Get archive compression formats */
            Archive archive = new Archive();
            foreach (ArchiveFormat format in Enum.GetValues(typeof(ArchiveFormat)))
            {
                ArchiveClass archiver;
                string name, filter;
                int[] blockSize;
                CheckBox[] settings;

                archive.CreationInformation(format, out archiver, out name, out filter, out blockSize, out settings);

                /* If the format doesn't exist, move onto the next format */
                if (archiver == null)
                    continue;

                /* Add all the stuff */
                archiveInformation.Add(new ArchiveInformation(format, archiver, name, filter, blockSize, settings));
            }
        }
        private void initalizeCompressionInformation()
        {
            /* Get compression formats */
            Compression compression = new Compression();
            foreach (CompressionFormat format in Enum.GetValues(typeof(CompressionFormat)))
            {
                CompressionClass compressor;
                string name;

                compression.CompressorInformation(format, out compressor, out name);

                /* If the format doesn't exist, move onto the next format */
                if (compressor == null)
                    continue;

                /* Add all the stuff */
                compressionInformation.Add(new CompressionInformation(format, compressor, name));
            }
        }

        /* Get the settings for each archive format */
        private bool[] getSettings(int formatIndex)
        {
            bool[] settings = new bool[archiveInformation[formatIndex].Settings.Length];

            for (int i = 0; i < settings.Length; i++)
                settings[i] = archiveInformation[formatIndex].Settings[i].Checked;

            return settings;
        }

        /* Archive Creation Settings */
        public class ArchiveInformation
        {
            /* Archive Information */
            private ArchiveFormat _Format;
            private ArchiveClass _Archiver;
            private string _Name, _Filter;
            private int[] _BlockSize;
            private CheckBox[] _Settings;

            public ArchiveInformation(ArchiveFormat format, ArchiveClass archiver, string name, string filter, int[] blockSize, CheckBox[] settings)
            {
                _Format    = format;
                _Archiver  = archiver;
                _Name      = name;
                _Filter    = filter;
                _BlockSize = blockSize;
                _Settings  = settings ?? new CheckBox[0];
            }

            /* Return values */
            public ArchiveFormat Format
            {
                get { return _Format; }
            }
            public ArchiveClass Archiver
            {
                get { return _Archiver; }
            }
            public string Name
            {
                get { return _Name; }
            }
            public string Filter
            {
                get { return _Filter; }
            }
            public int[] BlockSize
            {
                get { return _BlockSize; }
            }
            public CheckBox[] Settings
            {
                get { return _Settings; }
            }
        }

        /* Archive Creation Settings */
        public class CompressionInformation
        {
            /* Archive Information */
            private CompressionFormat _Format;
            private CompressionClass _Compressor;
            private string _Name;

            public CompressionInformation(CompressionFormat format, CompressionClass compressor, string name)
            {
                _Format     = format;
                _Compressor = compressor;
                _Name       = name;
            }

            /* Return values */
            public CompressionFormat Format
            {
                get { return _Format; }
            }
            public CompressionClass Compressor
            {
                get { return _Compressor; }
            }
            public string Name
            {
                get { return _Name; }
            }
        }
    }
}