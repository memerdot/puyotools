using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class archive_create : Form
    {
        /* Options */
        private CheckBox
            autoCompress, // Auto compress archive
            addFileNames; // Add filenames to the files in archives that don't store filenames.

        private ComboBox
            archiveFormat,     // Archive Format
            compressionFormat; // Compression Format

        private ListView
            archiveLayout; // Archive Layout

        private ColumnHeader
            archiveNum,  // Number in list
            archiveFile; // Filename

        private Button
            doWorkButton; // Start the work.

        private StatusMessage
            status; // Status Box.

        private string[]
            files; // Filenames.

        private string
            saveFileName; // Save filename.

        private string[] archiveFormats = { // Archive Formats
            "ACX",
            "AFS",
            "GNT",
            "MRG",
            "SNT (PS2)",
            "SNT (PSP)",
            "SPK",
            "TEX",
            "VDD"
        };

        private string[] compressionFormats = { // Compression Formats
            "CNX",
            "CXLZ",
            "LZ01"
        };

        /* Select files and display options */
        public archive_create()
        {
            /* Set up the window. */
            this.ClientSize = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Archive Creator Options";
            this.MaximizeBox = false;

            /* Select the files. */
            files = Files.selectFiles("Select Files(s)", "All Files (*.*)|*.*");

            /* Don't continue if no files were selected. */
            if (files.Length < 1)
                return;

            /* Display number of files. */
            Label numFiles     = new Label();
            numFiles.Text      = files.Length + " File" + (files.Length > 1 ? "s" : "") + " Selected";
            numFiles.Location  = new Point(8, 8);
            numFiles.Size      = new Size(384, 16);
            numFiles.TextAlign = ContentAlignment.MiddleCenter;
            numFiles.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            this.Controls.Add(numFiles);

            /* Show Options */
            showOptions();
        }

        /* Show Options */
        private void showOptions()
        {
            /* Choose a compression format. */
            Label archiveFormatLabel    = new Label();
            archiveFormatLabel.Text     = "Archive Format:";
            archiveFormatLabel.Location = new Point(8, 40);
            archiveFormatLabel.Size     = new Size(84, 16);

            this.Controls.Add(archiveFormatLabel);

            archiveFormat = new ComboBox();
            archiveFormat.Items.AddRange(archiveFormats);
            archiveFormat.DropDownStyle    = ComboBoxStyle.DropDownList;
            archiveFormat.MaxDropDownItems = archiveFormats.Length;
            archiveFormat.SelectedIndex    = 0;
            archiveFormat.Location         = new Point(96, 36);

            this.Controls.Add(archiveFormat);

            /* Now show the archive layout. */
            archiveLayout      = new ListView();
            archiveLayout.Location      = new Point(8, 64);
            archiveLayout.Size          = new Size(300, 240);
            archiveLayout.FullRowSelect = true;
            archiveLayout.View          = View.Details;
            archiveLayout.GridLines     = true;

            archiveNum  = new ColumnHeader();
            archiveFile = new ColumnHeader();

            archiveNum.Text  = "#";
            archiveFile.Text = "Filename";

            archiveNum.Width  = (int)(archiveLayout.Width * .2) - 12;
            archiveFile.Width = (int)(archiveLayout.Width * .8) - 12;

            archiveLayout.Columns.Add(archiveNum);
            archiveLayout.Columns.Add(archiveFile);

            /* Add the items. */
            for (int i = 0; i < files.Length; i++)
            {
                ListViewItem num = new ListViewItem();
                num.Text        = "" + (i + 1);

                ListViewItem.ListViewSubItem fileName = new ListViewItem.ListViewSubItem();
                fileName.Text = Path.GetFileName(files[i]);

                num.SubItems.Add(fileName);
                archiveLayout.Items.Add(num);
            }

            this.Controls.Add(archiveLayout);

            /* Move Item Buttons */
            Button moveItemUp   = new Button();
            moveItemUp.Text     = "Move Up";
            moveItemUp.Location = new Point(314, 64);
            moveItemUp.Size     = new Size(76, 24);
            moveItemUp.Click   += new EventHandler(moveUp);
            this.Controls.Add(moveItemUp);

            Button moveItemDown   = new Button();
            moveItemDown.Text     = "Move Down";
            moveItemDown.Location = new Point(314, 96);
            moveItemDown.Size     = new Size(76, 24);
            moveItemDown.Click   += new EventHandler(moveDown);
            this.Controls.Add(moveItemDown);


            /* Compress the archive. */
            autoCompress          = new CheckBox();
            autoCompress.Text     = "Compress the archive using the following compression:";
            autoCompress.Location = new Point(8, 310);
            autoCompress.Size     = new Size(300, 20);
            autoCompress.Enabled  = false;

            this.Controls.Add(autoCompress);

            compressionFormat = new ComboBox();
            compressionFormat.Items.AddRange(compressionFormats);
            compressionFormat.DropDownStyle    = ComboBoxStyle.DropDownList;
            compressionFormat.MaxDropDownItems = compressionFormats.Length;
            compressionFormat.SelectedIndex    = 0;
            compressionFormat.Location         = new Point(308, 310);
            compressionFormat.Size             = new Size(64, compressionFormat.Height);
            compressionFormat.Enabled          = false;

            this.Controls.Add(compressionFormat);

            /* Add filenames to archives that don't store then. */
            addFileNames          = new CheckBox();
            addFileNames.Text     = "Add filenames to archives that don't store filenames.";
            addFileNames.Location = new Point(8, 334);
            addFileNames.Size     = new Size(this.Width - 16, 20);

            this.Controls.Add(addFileNames);

            /* Display Extract Button */
            doWorkButton          = new Button();
            doWorkButton.Text     = "Create Archive";
            doWorkButton.Location = new Point(8, 358);
            doWorkButton.Size     = new Size(128, 24);
            doWorkButton.Click   += new EventHandler(startWork);

            this.Controls.Add(doWorkButton);

            this.ShowDialog();
        }

        /* Start & setup the work. */
        private void startWork(object sender, EventArgs e)
        {
            /* Show the save filename. */
            string filter = String.Empty;

            switch (archiveFormat.SelectedIndex)
            {
                case 0: filter = "ACX Archive (*.acx)|*.acx"; break; // ACX
                case 1: filter = "AFS Archive (*.afs)|*.afs"; break; // AFS
                case 2: filter = "GNT Archive (*.gnt)|*.gnt"; break; // GNT
                case 3: filter = "MRG Archive (*.mrg)|*.mrg"; break; // MRG
                case 4:
                case 5: filter = "SNT Archive (*.snt)|*.snt"; break; // SNT
                case 6: filter = "SPK Archive (*.spk)|*.spk"; break; // SPK
                case 7: filter = "TEX Archive (*.tex)|*.tex"; break; // TEX
                case 8: filter = "VDD Archive (*.vdd)|*.vdd"; break; // VDD
            }

            saveFileName = Files.saveFile("Save Output Archive", filter);

            if (saveFileName == String.Empty)
                return;

            /* Disable the start work button and other things. */
            archiveFormat.Enabled     = false;
            compressionFormat.Enabled = false;
            archiveLayout.Enabled     = false;
            doWorkButton.Enabled      = false;

            /* Display status message. */
            status = new StatusMessage("Creating Archive", StatusMessage.createArchive, files.Length);
            status.Show();

            /* Start to decompress the files now */
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Do the work. */
        private void run(object sender, DoWorkEventArgs e)
        {
            /* Create the file array array. */
            byte[][] data = new byte[files.Length][];

            /* Loop through each of the files. */
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    /* Update the status. */
                    status.updateStatus(StatusMessage.addToArchive, Path.GetFileName(files[i]), (i + 1));

                    /* Load the file. */
                    FileStream file = new FileStream(files[i], FileMode.Open);
                    data[i] = new byte[file.Length];

                    file.Read(data[i], 0, (int)file.Length);
                    file.Close();

                    /* Set the new shortened filename. */
                    files[i] = Path.GetFileName(files[i]);
                }

                status.updateStatus(StatusMessage.createArchive, Path.GetFileName(saveFileName), 0);

                /* Set up our archive data. */
                byte[] archiveData = new byte[0];

                /* See which archive we need to create. */
                if (archiveFormat.SelectedIndex == 0) // ACX
                {
                    ACX creator = new ACX();
                    archiveData = creator.create(data, files, addFileNames.Checked);
                }
                else if (archiveFormat.SelectedIndex == 1) // AFS
                {
                    AFS creator = new AFS();
                    archiveData = creator.create(data, files);
                }
                else if (archiveFormat.SelectedIndex == 2) // GNT
                {
                    GNT creator = new GNT();
                    archiveData = creator.create(data, files, addFileNames.Checked);
                }
                else if (archiveFormat.SelectedIndex == 3) // MRG
                {
                    MRG creator = new MRG();
                    archiveData = creator.create(data, files);
                }
                else if (archiveFormat.SelectedIndex == 4) // SNT (PS2)
                {
                    SNT creator = new SNT();
                    archiveData = creator.create(data, files, addFileNames.Checked, false);
                }
                else if (archiveFormat.SelectedIndex == 5) // SNT (PSP)
                {
                    SNT creator = new SNT();
                    archiveData = creator.create(data, files, addFileNames.Checked, true);
                }
                else if (archiveFormat.SelectedIndex == 6) // SPK
                {
                    SPK creator = new SPK();
                    archiveData = creator.create(data, files);
                }
                else if (archiveFormat.SelectedIndex == 7) // TEX
                {
                    TEX creator = new TEX();
                    archiveData = creator.create(data, files);
                }
                else if (archiveFormat.SelectedIndex == 8) // VDD
                {
                    VDD creator = new VDD();
                    archiveData = creator.create(data, files);
                }

                /* Compress the archive? */
                if (autoCompress.Checked)
                {
                    if (compressionFormat.SelectedIndex == 1) // CXLZ
                    {
                        CXLZ compressor = new CXLZ();
                        archiveData = compressor.compress(archiveData);
                    }
                    else if (compressionFormat.SelectedIndex == 2) // LZ01
                    {
                        LZ01 compressor = new LZ01();
                        archiveData = compressor.compress(archiveData);
                    }
                }

                /* Output the archive. */
                FileStream outputFile = new FileStream(saveFileName, FileMode.Create);
                outputFile.Write(archiveData, 0, archiveData.Length);
                outputFile.Close();
            }
            catch (Exception) { }

            /* Now we are done with the work. */
            status.Close();
            this.Close();
        }

        /* Attempt to decompress the file. */
        private byte[] decompressFile(byte[] data)
        {
            /* CNX Compression */
            if (Header.isFile(data, Header.CNX, 0))
            {
                CNX decompressor = new CNX();
                return decompressor.decompress(data);
            }

            /* CXLZ compression */
            else if (Header.isFile(data, Header.CXLZ, 0))
            {
                CXLZ decompressor = new CXLZ();
                return decompressor.decompress(data);
            }

            /* LZ01 compression */
            else if (Header.isFile(data, Header.LZ01, 0))
            {
                LZ01 decompressor = new LZ01();
                return decompressor.decompress(data);
            }

            /* No compression could be found, or no supported compression. */
            else
                return data;
        }

        /* Get file extension for unnamed files. */
        private string getFileExt(byte[] data, string archiveFileName)
        {
            /* See what extention we need to add to the end of the file. */
            /* GVR file */
            if (Header.isFile(data, Header.GVR, 0))
                return ".gvr";

            /* SVR file */
            else if (Header.isFile(data, Header.SVR, 0))
                return ".svr";

            /* GIM file */
            else if (Header.isFile(data, Header.GIM, 0) ||
                     Header.isFile(data, Header.MIG, 0))
                return ".gim";

            /* SVP file */
            else if (Header.isFile(data, Header.SVP, 0))
                return ".svp";

            /* TEX file. */
            else if (Header.isFile(data, Header.TEX, 0))
                return ".tex";

            /* SNT file */
            else if ((Header.isFile(data, Header.SNT_PS2, 0) && Header.isFile(data, Header.SNT_SUB_PS2, 0x20)) ||
                     (Header.isFile(data, Header.SNT_PSP, 0) && Header.isFile(data, Header.SNT_SUB_PSP, 0x20)))
                return ".snt";

            /* GNT file */
            else if (Header.isFile(data, Header.GNT, 0) &&
                     Header.isFile(data, Header.GNT_SUB, 0x20))
                return ".gnt";

            /* ADX file extracted from ACX archive. */
            else if (Path.GetExtension(archiveFileName).ToLower() == ".acx")
                return ".adx";

            /* Other type of file. */
            else
                return ".bin";
        }

        /* Get number of letters in the string */
        private string getDigits(int numOfFiles)
        {
            string str = String.Empty;
            int digits = 1;

            while (digits < numOfFiles)
            {
                digits *= 10;
                str += "0";
            }

            return str;
        }

        /* Move an item in an array. */
        private string[] moveItemInArray(string[] array, int sourceIndex, int destinationIndex)
        {
            string[] outputArray = new string[array.Length];
            string arrayItem = array[sourceIndex];

            for (int i = 0; i < array.Length; i++)
            {
                if (i >= sourceIndex && i < destinationIndex)
                    outputArray[i] = array[i + 1];
                else if (i > destinationIndex && i <= sourceIndex)
                    outputArray[i] = array[i - 1];
                else if (i == destinationIndex)
                    outputArray[i] = arrayItem;
                else
                    outputArray[i] = array[i];
            }

            return outputArray;
        }

        /* Move Items */
        private void moveUp(object sender, EventArgs e)
        {
            if (!doWorkButton.Enabled)
                return;

            if (archiveLayout.SelectedIndices.Count > 0 && archiveLayout.SelectedIndices[0] > 0)
            {
                int selectedIndex    = archiveLayout.SelectedIndices[0];
                int destinationIndex = archiveLayout.SelectedIndices[0] - 1;

                files = moveItemInArray(files, selectedIndex, destinationIndex);

                /* Refresh the list. */
                archiveLayout.Items.Clear();
                for (int i = 0; i < files.Length; i++)
                {
                    ListViewItem num = new ListViewItem();
                    num.Text = "" + (i + 1);

                    ListViewItem.ListViewSubItem fileName = new ListViewItem.ListViewSubItem();
                    fileName.Text = Path.GetFileName(files[i]);

                    num.SubItems.Add(fileName);
                    archiveLayout.Items.Add(num);
                }

                archiveLayout.Items[destinationIndex].Selected = true;
            }
            archiveLayout.Focus();
        }

        private void moveDown(object sender, EventArgs e)
        {
            if (!doWorkButton.Enabled)
                return;

            if (archiveLayout.SelectedIndices.Count > 0 && archiveLayout.SelectedIndices[0] < archiveLayout.Items.Count - 1)
            {
                int selectedIndex    = archiveLayout.SelectedIndices[0];
                int destinationIndex = archiveLayout.SelectedIndices[0] + 1;

                files = moveItemInArray(files, selectedIndex, destinationIndex);

                /* Refresh the list. */
                archiveLayout.Items.Clear();
                for (int i = 0; i < files.Length; i++)
                {
                    ListViewItem num = new ListViewItem();
                    num.Text = "" + (i + 1);

                    ListViewItem.ListViewSubItem fileName = new ListViewItem.ListViewSubItem();
                    fileName.Text = Path.GetFileName(files[i]);

                    num.SubItems.Add(fileName);
                    archiveLayout.Items.Add(num);
                }

                archiveLayout.Items[destinationIndex].Selected = true;
            }
            archiveLayout.Focus();
        }
    }
}