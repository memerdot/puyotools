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
            compressArchive = new CheckBox(), // Compress the Archive
            extractFilenames = new CheckBox(), // Extract Filenames
            decompressSource = new CheckBox(), // Decompress Source Files
            useDefinedFilenamesSource = new CheckBox(), // Use Filenames defined in the file
            decompressExtracted = new CheckBox(), // Decompress Extracted Files
            useDefinedFilenamesExtract = new CheckBox(), // Use Filenames defined in the file
            extractExtracted = new CheckBox(), // Extract Extracted Files
            autoConvertImages = new CheckBox(), // Convert Extracted Images to PNG
            autoDeleteSourceImages = new CheckBox(); // Delete Source Files

        private ComboBox
            archiveFormat     = new ComboBox(), // Archive Format
            compressionFormat = new ComboBox(); // Compression Format

        private Button
            moveFileUp      = new Button(), // Move file up in the archive
            moveFileDown    = new Button(), // Move file down in the archive
            renameFile      = new Button(), // Rename file
            renameAllFiles  = new Button(), // Rename all files
            startWorkButton = new Button(); // Start Work Button

        public static string renameFilename = String.Empty; // Rename filename
        public static bool canRenameFile = false; // Can rename filename

        private ListView
            archiveContents = new ListView(); // Contents of Archive

        private List<string>
            sourceFilenames      = new List<string>(), // List of source filenames
            destinationFilenames = new List<string>(); // List of filenames in the archive

        private string[]
            files; // Files to Decompress

        private StatusMessage
            status; // Status Message

        private string[] archiveFormats = new string[] { // Archive Formats
            "ACX",
            "AFS",
            "GNT",
            "GVM",
            "MRG",
            "NARC",
            "ONE",
            "PVM",
            "SNT (PS2)",
            "SNT (PSP)",
            "SPK",
            "TEX",
            "VDD",
        };

        private string[] compressionFormats = new string[] { // Compression Formats
            "LZSS",
        };

        private int[] filenameLength = new int[] { // Filename Lengths
            63,  // ACX
            31,  // AFS
            63,  // GNT
            27,  // GVM
            31,  // MRG
            255, // NARC
            55,  // ONE
            27,  // PVM
            63,  // SNT (PS2)
            63,  // SNT (PSP)
            22,  // SPK
            22,  // TEX
            15,  // VDD
        };

        //private int[][] blockSizes = new int[] { // Block Sizes
        //    new int[] {4}, // ACX
        //    new int[] {2048, 16}, // AFS
        //};

        public Archive_Create()
        {
            /* Select the files */
            files = Files.selectFiles("Select Files to Add", "All Files (*.*)|*.*");

            /* If no files were selected, don't continue */
            if (files.Length == 0)
                return;

            /* Set up the form */
            FormContent.Create(this, "Archive - Create", new Size(528, 400));

            /* Files Selected */
            FormContent.Add(this, new Label(),
                String.Format("{0} {1} Selected",
                    files.Length.ToString(),
                    (files.Length > 1 ? "Files" : "File")),
                new Point(0, 8),
                new Size(this.Width, 16),
                ContentAlignment.TopCenter,
                new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold));

            /* Archive Format */
            FormContent.Add(this, new Label(),
                "Archive Format:",
                new Point(16, 32),
                new Size(96, 16));

            FormContent.Add(this, archiveFormat,
                archiveFormats,
                new Point(112, 32),
                new Size(64, 16),
                changeArchiveFormat);

            /* Archive Contents */
            FormContent.Add(this, archiveContents,
                new string[] { "#", "Source Filename", "Filename in the Archive" },
                new int[] { 48, 160, 160 },
                new Point(16, 56),
                new Size(400, 192));

            /* Move File Up */
            FormContent.Add(this, moveFileUp,
                "Move\nUp",
                new Point(424, 56),
                new Size(96, 32),
                moveUp);

            /* Move File Down */
            FormContent.Add(this, moveFileDown,
                "Move\nDown",
                new Point(424, 96),
                new Size(96, 32),
                moveDown);

            /* Rename Selected File */
            FormContent.Add(this, renameFile,
                "Rename\nSelected File",
                new Point(424, 176),
                new Size(96, 32),
                renameSelectedFile);

            /* Rename All Files */
            FormContent.Add(this, renameAllFiles,
                "Rename\nAll Files",
                new Point(424, 216),
                new Size(96, 32),
                renameFiles);

            /* Compress Archive */
            FormContent.Add(this, compressArchive,
                "Compress the archive using the following compression:",
                new Point(16, 264),
                new Size(304, 16));

            /* Compress Archive Format */
            FormContent.Add(this, compressionFormat,
                compressionFormats,
                new Point(320, 264),
                new Size(64, 16));

            /* Create Archive */
            FormContent.Add(this, startWorkButton,
                "Create Archive",
                new Point((this.Width / 2) - 60, 368),
                new Size(120, 24),
                startWork);

            /* Add files to the archive contents */
            for (int i = 0; i < files.Length; i++)
            {
                sourceFilenames.Add(files[i]);
                destinationFilenames.Add(Path.GetFileName(files[i]));
            }

            /* Populate the list */
            populateArchiveList();

            this.ShowDialog();
        }

        /* Populate the file list */
        private void populateArchiveList()
        {
            archiveContents.Items.Clear();
            for (int i = 0; i < files.Length; i++)
            {
                /* Set up our variables */
                ListViewItem fileNumber = new ListViewItem(); // File Number

                ListViewItem.ListViewSubItem
                    sourceFilename      = new ListViewItem.ListViewSubItem(), // Source Filename
                    destinationFilename = new ListViewItem.ListViewSubItem(); // Filename in the Archive

                fileNumber.Text          = (i + 1).ToString("#,0");
                sourceFilename.Text      = Path.GetFileName(sourceFilenames[i]);
                destinationFilename.Text = (destinationFilenames[i] == String.Empty ? "(No Filename)" : StringData.LimitLength(destinationFilenames[i], filenameLength[archiveFormat.SelectedIndex]));

                fileNumber.SubItems.Add(sourceFilename);
                fileNumber.SubItems.Add(destinationFilename);
                archiveContents.Items.Add(fileNumber);
            }
        }

        /* Move file up in the list */
        private void moveUp(object sender, EventArgs e)
        {
            /* Get the index that was selected */
            if (archiveContents.SelectedIndices.Count == 0)
                return;

            int index = archiveContents.SelectedIndices[0];

            /* Don't do this if this is the first item in the list */
            if (index == 0)
                return;

            /* Move the file up now */
            sourceFilenames.Insert(index - 1, sourceFilenames[index]);
            destinationFilenames.Insert(index - 1, destinationFilenames[index]);

            sourceFilenames.RemoveAt(index + 1);
            destinationFilenames.RemoveAt(index + 1);

            populateArchiveList();

            archiveContents.Items[index - 1].Selected = true;
            archiveContents.Focus();
        }

        /* Move file down in the list */
        private void moveDown(object sender, EventArgs e)
        {
            /* Get the index that was selected */
            if (archiveContents.SelectedIndices.Count == 0)
                return;

            int index = archiveContents.SelectedIndices[0];

            /* Don't do this if this is the last item in the list */
            if (index == sourceFilenames.Count - 1)
                return;

            /* Move the file down now */
            sourceFilenames.Insert(index + 2, sourceFilenames[index]);
            destinationFilenames.Insert(index + 2, destinationFilenames[index]);

            sourceFilenames.RemoveAt(index);
            destinationFilenames.RemoveAt(index);

            populateArchiveList();

            archiveContents.Items[index + 1].Selected = true;
            archiveContents.Focus();
        }

        /* Rename Selected File */
        private void renameSelectedFile(object sender, EventArgs e)
        {
            /* Make sure we selected something */
            if (archiveContents.SelectedIndices.Count == 0)
                return;

            int index = archiveContents.SelectedIndices[0];

            Archive_Create_RenameFile program = new Archive_Create_RenameFile(destinationFilenames[index], false);

            /* Can we rename the file? */
            if (canRenameFile)
            {
                destinationFilenames[index] = renameFilename;
                canRenameFile = false;

                populateArchiveList();
            }
        }

        /* Rename All Files */
        private void renameFiles(object sender, EventArgs e)
        {
            Archive_Create_RenameFile program = new Archive_Create_RenameFile((archiveContents.SelectedIndices.Count == 0 ? String.Empty : destinationFilenames[archiveContents.SelectedIndices[0]]), false);

            /* Can we rename the file? */
            if (canRenameFile)
            {
                for (int i = 0; i < destinationFilenames.Count; i++)
                    destinationFilenames[i] = renameFilename;

                canRenameFile = false;

                populateArchiveList();
            }
        }

        /* Change Archive Format */
        private void changeArchiveFormat(object sender, EventArgs e)
        {
            /* Re-populate the list */
            populateArchiveList();
        }

        /* Start Work */
        private void startWork(object sender, EventArgs e)
        {
            /* First, disable the buttons */
            startWorkButton.Enabled = false;
            moveFileUp.Enabled      = false;
            moveFileDown.Enabled    = false;
            renameFile.Enabled      = false;
            renameAllFiles.Enabled  = false;

            /* Set up our background worker */
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += run;

            /* Now, show our status */
            status = new StatusMessage("Archive - Create", files);

            bw.RunWorkerAsync();
            status.ShowDialog();
        }

        /* Decompress the files */
        private void run(object sender, DoWorkEventArgs e)
        {
            /* Let's get the output filename and archive format */
            ArchiveFormat format = ArchiveFormat.NULL;
            string filter = String.Empty;
            switch (archiveFormat.SelectedIndex)
            {
                case 0:  format = ArchiveFormat.ACX;  filter = "ACX Archive (*.acx)|*.acx"; break;
                case 1:  format = ArchiveFormat.AFS;  filter = "AFS Archive (*.afs)|*.afs"; break;
                case 2:  format = ArchiveFormat.GNT;  filter = "GNT Archive (*.gnt)|*.gnt"; break;
                case 3:  format = ArchiveFormat.GVM;  filter = "GVM Archive (*.gvm)|*.gvm"; break;
                case 4:  format = ArchiveFormat.MRG;  filter = "MRG Archive (*.mrg)|*.mrg"; break;
                case 5:  format = ArchiveFormat.NARC; filter = "NARC Archive (*.narc)|*.narc"; break;
                case 6:  format = ArchiveFormat.ONE;  filter = "ONE Archive (*.one)|*.one"; break;
                case 7:  format = ArchiveFormat.PVM;  filter = "PVM Archive (*.pvm)|*.pvm"; break;
                case 8:  format = ArchiveFormat.NSIF; filter = "SNT Archive (*.snt)|*.snt"; break;
                case 9:  format = ArchiveFormat.NUIF; filter = "SNT Archive (*.snt)|*.snt"; break;
                case 10: format = ArchiveFormat.SPK;  filter = "SPK Archive (*.spk)|*.spk"; break;
                case 11: format = ArchiveFormat.TEX;  filter = "TEX Archive (*.tex)|*.tex"; break;
                case 12: format = ArchiveFormat.VDD;  filter = "VDD Archive (*.vdd)|*.vdd"; break;
            }

            string outputFilename = Files.saveFile("Select Archive Filename", filter);

            /* Don't continue if we didn't select anything */
            if (outputFilename == String.Empty)
                return;

            /* Create the file list */
            List<string> sourceFileList  = new List<string>(sourceFilenames);
            List<string> archiveFileList = new List<string>(destinationFilenames);

            try
            {
                /* Let's create our archive class */
                //Archive archive = new Archive(format);

                /* Does the archive contain filenames? */
                //if (!archive.ContainsFilenames())
                //{
                //    for (int i = 0; i < archiveFileList.Count; i++)
                //        archiveFileList[i] = String.Empty;
                //}

                /* Start writing the output files. If we are compressing it, use a Memory Stream */
                Stream outputData;
                if (compressArchive.Checked)
                    outputData = new MemoryStream();
                else
                    outputData = new FileStream(outputFilename, FileMode.Create, FileAccess.Write);

                /* Ok, let's write out the header */
                //byte[] header = archive.CreateHeader(sourceFileList.ToArray(), archiveFileList.ToArray());
                //outputData.Write(header, 0x0, header.Length);

                for (int i = 0; i < sourceFileList.Count; i++)
                {
                    /* Set the current file */
                    status.CurrentFile = i;

                    /* Does the archive not contains filenames, and we want to add them? */

                    /* Ok, let's load the file */
                    Stream inputFile;
                    using (inputFile = new FileStream(sourceFileList[i], FileMode.Open, FileAccess.Read))
                    {
                        /* Ok, let's add the file to the archive */
                        //inputFile = archive.FormatFileToAdd(inputFile);
                        outputData.Write(ObjectConverter.StreamToBytes(inputFile, 0x0, (int)inputFile.Length), 0x0, (int)inputFile.Length);

                        /* Add the padding to the file */
                        //if (inputFile.Length % 16 != 0)
                            //outputData.Write(ObjectConverter.StreamToBytes(archive.AddPadding((uint)inputFile.Length, 16), 0x0, 16 - (int)(inputFile.Length % 16)), 0x0, 16 - (int)(inputFile.Length % 16));
                    }
                }

                /* Ok, let's add the footer now */
                //byte[] footer = archive.CreateFooter(sourceFileList.ToArray(), archiveFileList.ToArray(), header);

                //if (footer.Length > 0)
                //    outputData.Write(footer, 0x0, footer.Length);

                /* Do we want to compress the data? */
                if (compressArchive.Checked)
                {
                    Compression compression = new Compression(outputData, outputFilename, CompressionFormat.LZSS, new LZSS());
                    Stream decompressedData = outputData;
                    Stream compressedData = compression.Compress();

                    /* Now, let's create our File Stream */
                    outputData = new FileStream(outputFilename, FileMode.Create, FileAccess.Write);
                    if (compressedData.Length > 0)
                        outputData.Write(ObjectConverter.StreamToBytes(compressedData, 0x0, (int)compressedData.Length), 0x0, (int)compressedData.Length);
                    else
                        outputData.Write(ObjectConverter.StreamToBytes(decompressedData, 0x0, (int)decompressedData.Length), 0x0, (int)decompressedData.Length);
                }

                /* Ok, we are done writing the file */
                outputData.Close();
            }
            catch
            {
                /* Something went wrong. */
            }

            /* Close the status box now */
            status.Close();
            this.Close();
        }
    }

    /* Class to rename selected file or all files */
    class Archive_Create_RenameFile : Form
    {
        /* Filename */
        TextBox newFilename = new TextBox();

        public Archive_Create_RenameFile(string selectedFilename, bool renameAllFiles)
        {
            FormContent.Create(this,
                String.Format("Rename {0}",
                    (renameAllFiles ? "All Files" : (selectedFilename == String.Empty ? "nameless file" : selectedFilename))),
                new Size(320, 100), false);

            /* Rename this to */
            FormContent.Add(this, new Label(),
                String.Format("Rename {0} to:\n(Leave blank for no filename)", (renameAllFiles ? "all files" : (selectedFilename == String.Empty ? "nameless file" : selectedFilename))),
                new Point(8, 8),
                new Size(304, 32));

            /* Display Text Box */
            FormContent.Add(this, newFilename,
                selectedFilename,
                new Point(8, 40),
                new Size(304, 24));

            /* Display Rename Button */
            FormContent.Add(this, new Button(),
                "Rename",
                new Point(94, 68),
                new Size(64, 24),
                renameFile);

            /* Display Cancel Button */
            FormContent.Add(this, new Button(),
                "Cancel",
                new Point(162, 68),
                new Size(64, 24),
                cancelRename);

            this.ShowDialog();
        }

        /* Rename the file */
        private void renameFile(object sender, EventArgs e)
        {
            Archive_Create.canRenameFile = true;
            Archive_Create.renameFilename = newFilename.Text;

            this.Close();
        }

        /* Cancel the rename */
        private void cancelRename(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}