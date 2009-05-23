using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace puyo_tools
{
    public class Compression_Compress : Form
    {
        /* Set up our form variables */
        private GroupBox
            compressionSettings   = new GroupBox(); // Decompression Settings

        private CheckBox
            useStoredFilename = new CheckBox(), // Use stored filename
            deleteSourceFile = new CheckBox(), // Delete Source file
            compressSameDir = new CheckBox(), // Output to same directory
            unpackImage = new CheckBox(), // Unpack image
            deleteSourceImage = new CheckBox(), // Delete Source Image
            convertSameDir = new CheckBox(); // Output to same directory

        private ComboBox
            compressionFormat = new ComboBox(); // Compression Format


        private Button
            startWorkButton = new Button(); // Start Work Button

        private string[]
            files; // Files to Decompress

        private StatusMessage
            status; // Status Message

        public Compression_Compress()
        {
            /* Select the files */
            files = FileSelectionDialog.OpenFiles("Select Files to Compress", "All Files|*.*");

            /* If no files were selected, don't continue */
            if (files == null || files.Length == 0)
                return;

            showOptions();
        }

        public Compression_Compress(bool selectDirectory)
        {
            /* Select the directories */
            string directory = FileSelectionDialog.SaveDirectory("Select a directory");

            /* If no directory was selected, don't continue */
            if (directory == null || directory == String.Empty)
                return;

            /* Ask the user if they want to search sub directories */
            DialogResult result = MessageBox.Show(this, "Do you want to add the files from sub directories?", "Add Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                files = Files.FindFilesInDirectory(directory, true);
            else
                files = Files.FindFilesInDirectory(directory, false);

            /* Show Options */
            showOptions();
        }

        private void showOptions()
        {
            /* Set up the form */
            FormContent.Create(this, "Compression - Compress", new Size(400, 156));

            /* Files Selected */
            FormContent.Add(this, new Label(),
                String.Format("{0} {1} Selected",
                    files.Length.ToString(),
                    (files.Length > 1 ? "Files" : "File")),
                new Point(0, 8),
                new Size(this.Width, 16),
                ContentAlignment.TopCenter,
                new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold));

            /* Compression Settings */
            FormContent.Add(this, compressionSettings,
                "Compression Settings",
                new Point(8, 32),
                new Size(this.Size.Width - 24, 84));

            /* Compression Format */
            FormContent.Add(compressionSettings, new Label(),
                "Compression Format:",
                new Point(8, 20),
                new Size(compressionSettings.Size.Width - 16, 16));

            /* Compression Format */
            FormContent.Add(compressionSettings, compressionFormat,
                //new string[] {"CNX", "CXLZ", "LZ01", "LZSS"},
                new string[] {"CXLZ", "LZSS"},
                new Point(8, 36),
                new Size(120, 16));

            /* Output to same directory */
            FormContent.Add(compressionSettings, compressSameDir,
                "Output file to same directory as source (and overwrite if necessary).",
                new Point(8, 60),
                new Size(compressionSettings.Size.Width - 16, 16));

            /* Convert */
            FormContent.Add(this, startWorkButton,
                "Compress",
                new Point((this.Width / 2) - 60, 124),
                new Size(120, 24),
                startWork);

            this.ShowDialog();
        }

        /* Start Work */
        private void startWork(object sender, EventArgs e)
        {
            /* First, disable the button */
            startWorkButton.Enabled = false;

            /* Set up our background worker */
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += run;

            /* Add the Status Message */
            status = new StatusMessage("Compression - Compress", files);
            bw.RunWorkerAsync();
            status.ShowDialog();
        }

        /* Decompress the files */
        private void run(object sender, DoWorkEventArgs e)
        {
            /* Create the file list */
            List<string> fileList = new List<string>();
            foreach (string i in files)
                fileList.Add(i);

            for (int i = 0; i < files.Length; i++)
            {
                /* Set the current file */
                status.CurrentFile = i;

                try
                {
                    /* Open up the file */
                    MemoryStream data;
                    string outputDirectory, outputFilename;
                    using (FileStream inputStream = new FileStream(fileList[i], FileMode.Open, FileAccess.Read))
                    {
                        /* Set up the compressor to use */
                        CompressionClass compressor = null;
                        CompressionFormat format    = CompressionFormat.NULL;
                        switch (compressionFormat.SelectedIndex)
                        {
                            case 0: compressor = new CXLZ(); format = CompressionFormat.CXLZ; break;
                            case 1: compressor = new LZSS(); format = CompressionFormat.LZSS; break;
                            //case 0: compressor = new CNX();  format = CompressionFormat.CNX;  break;
                            //case 1: compressor = new CXLZ(); format = CompressionFormat.CXLZ; break;
                            //case 2: compressor = new LZ01(); format = CompressionFormat.LZ01; break;
                            //case 3: compressor = new LZSS(); format = CompressionFormat.LZSS; break;
                        }

                        /* Set up the decompressor */
                        Compression compression = new Compression(inputStream, Path.GetFileName(fileList[i]), format, compressor);

                        /* Set up the output directories and file names */
                        outputDirectory = Path.GetDirectoryName(fileList[i]) + (compressSameDir.Checked ? String.Empty : Path.DirectorySeparatorChar + "Compressed");
                        outputFilename  = Path.GetFileName(fileList[i]);

                        /* Decompress data */
                        MemoryStream compressedData = (MemoryStream)compression.Compress();

                        /* Check to make sure the decompression was successful */
                        if (compressedData == null)
                            continue;
                        else
                            data = compressedData;
                    }

                    /* Create the output directory if it does not exist */
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);

                    /* Write file data */
                    using (FileStream outputStream = new FileStream(outputDirectory + Path.DirectorySeparatorChar + outputFilename, FileMode.Create, FileAccess.Write))
                        data.WriteTo(outputStream);
                }
                catch
                {
                    /* Something went wrong. Continue please. */
                    continue;
                }
            }

            /* Close the status box now */
            status.Close();
            this.Close();
        }
    }
}