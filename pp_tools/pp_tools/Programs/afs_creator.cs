using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace pp_tools
{
    public class AFS_Create : Form
    {
        /* Variables */
        OpenFileDialog selectFiles;        // Select Files Dialog
        StatusMessage status;              // Status Messages
        private byte[] data;               // File Data
        Button startButton;                // Decompress Button
        //CheckBox autoDecompress;           // Auto Decompress LZ01 / CXLZ
        //CheckBox findFileNames;            // Attempt to find filenames
        //CheckBox autoConvert;              // Auto Convert to PNG
        //CheckBox autoDelete;               // Delete extracted files.
        ListView listFiles;                  // List of files
        string outputDir;                  // Output Directory of file

        BackgroundWorker bw = new BackgroundWorker(); // Background Worker

        public AFS_Create()
        {
            /* Select our files */
            selectFiles                  = new OpenFileDialog();
            selectFiles.Title            = "Select files(s)";
            selectFiles.Multiselect      = true;
            selectFiles.RestoreDirectory = true;
            selectFiles.CheckFileExists  = true;
            selectFiles.CheckPathExists  = true;
            selectFiles.AddExtension     = true;
            selectFiles.Filter           = "All Files|*.*";
            selectFiles.DefaultExt       = "";
            selectFiles.ShowDialog();

            if (selectFiles.FileNames.Length < 1)
                return;

            /* Set up the window */
            this.ClientSize      = new Size(400, 330);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "AFS Creator Options";

            /* Number of files selected. */
            Label filesSelected     = new Label();
            filesSelected.Text      = selectFiles.FileNames.Length + " File" + (selectFiles.FileNames.Length > 1 ? "s" : "") + " Selected";
            filesSelected.Location  = new Point(8, 8);
            filesSelected.Size      = new Size(384, 16);
            filesSelected.TextAlign = ContentAlignment.MiddleCenter;
            filesSelected.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            this.Controls.Add(filesSelected);

            /* Listview of files */
            listFiles               = new ListView();
            listFiles.View          = View.Details;
            listFiles.FullRowSelect = true;
            listFiles.MultiSelect   = false;
            listFiles.Location      = new Point(8, 32);
            listFiles.Size          = new Size(344, 200);

            ColumnHeader header = listFiles.Columns.Add("Files");
            header.Width = 320;
            for (int i = 0; i < selectFiles.FileNames.Length; i++)
            {
                ListViewItem item = listFiles.Items.Add(Path.GetFileName(selectFiles.FileNames[i]));
            }
            this.Controls.Add(listFiles);

            /* Add an option to decompress compress file */
            //autoDecompress          = new CheckBox();
            //autoDecompress.Text     = "Decompress file if it contains LZ01 / CXLZ compression.";
            //autoDecompress.Location = new Point(8, 32);
            //autoDecompress.Size     = new Size(384, 20);
            //autoDecompress.Checked  = true;
            //this.Controls.Add(autoDecompress);

            /* Add an option to decompress compress file */
            //findFileNames          = new CheckBox();
            //findFileNames.Text     = "Attempt to find filenames for files in the archive.";
            //findFileNames.Location = new Point(8, 56);
            //findFileNames.Size     = new Size(384, 20);
            //this.Controls.Add(findFileNames);

            /* Add an option to convert to PNG */
            //autoConvert          = new CheckBox();
            //autoConvert.Text     = "Attempt to convert file to PNG if it is an image.";
            //autoConvert.Location = new Point(8, 80);
            //autoConvert.Size     = new Size(384, 20);
            //this.Controls.Add(autoConvert);

            /* Auto Delete Files */
            //autoDelete          = new CheckBox();
            //autoDelete.Text     = "Delete extracted files if they were successfully converted to PNG.";
            //autoDelete.Location = new Point(24, 104);
            //autoDelete.Size     = new Size(368, 20);
            //this.Controls.Add(autoDelete);

            /* Add the Extract Button */
            startButton          = new Button();
            startButton.Text     = "Create";
            startButton.Location = new Point(8, 300);
            startButton.Size     = new Size(128, 24);
            startButton.Click   += new EventHandler(startWork);
            this.Controls.Add(startButton);

            /* Finally, show the dialog */
            this.ShowDialog();
        }

        /* Extract Button */
        private void startWork(object sender, EventArgs e)
        {
            /* Display Status Box */
            startButton.Enabled = false;
            status = new StatusMessage("Create AFS File", StatusMessage.createArchive, selectFiles.FileNames.Length);
            status.Show();

            /* Decompress our files now */
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Run the AFS Creator */
        private void run(object sender, DoWorkEventArgs e)
        {
            try
            {
                status.updateStatus(StatusMessage.createArchive, String.Empty, 1);

                AFS afs = new AFS();
                //data    = afs.create(selectFiles.FileNames);

                /* Set directory for output files */
                outputDir = Path.GetDirectoryName(selectFiles.FileNames[0]);

                /* Create directory for decompressed files */
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                /* Attempt to write the file now. */
                FileStream file;
                file = new FileStream(outputDir + Path.DirectorySeparatorChar + "output.afs", FileMode.Create);
                file.Write(data, 0, data.Length);
                file.Close();
            }
            catch (Exception)
            {
            }

            status.Close();
            this.Close();
        }
    }
}