using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class Archive_Extractor : Form
    {
        /* Options */
        private CheckBox
            autoConvertImages,   // Auto convert images to PNG
            autoDecompress,      // Auto decompress compressed files
            autoDeleteConverted, // Auto delete extract files converted to PNG
            autoExtractArchive,  // Auto extract archives in the archive
            getFileNames;        // Return file names if we can find them

        private Button
            doWorkButton; // Start the work.

        private StatusMessage
            status; // Status Box.

        private string[]
            files; // Filenames.

        /* Select files and display options */
        public Archive_Extractor()
        {
            /* Set up the window. */
            this.ClientSize      = new Size(400, 190);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Archive Extractor Options";
            this.MaximizeBox     = false;

            /* Select the files. */
            files = Files.selectFiles("Select Archive(s)", "All Supported Archives (*.acx;*.afs;*.gnt;*.mrg;*.one;*.snt;*.spk;*.tex;*.vdd)|*.acx;*.afs;*.gnt;*.mrg;*.one;*.snt;*.spk;*.tex;*.vdd|ACX Archives (*.acx)|*.acx|AFS Archives (*.afs)|*.afs|GNT Archives (*.gnt)|*.gnt|MRG Archives (*.mrg)|*.mrg|ONE Archives (*.one)|*.one|SNT Archives (*.snt)|*.snt|SPK Archives (*.spk)|*.spk|TEX Archives (*.tex)|*.tex|VDD Archives (*.vdd)|*.vdd|All Files & Archives (*.*)|*.*");

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
            /* Decompress file containing CXLZ or LZ01 compression. */
            autoDecompress          = new CheckBox();
            autoDecompress.Text     = "Decompress source and extracted files containing compression.";
            autoDecompress.Location = new Point(8, 32);
            autoDecompress.Size     = new Size(this.Width - 16, 30);
            autoDecompress.Checked  = true;

            this.Controls.Add(autoDecompress);

            /* Get file names from the files in the archive. */
            getFileNames          = new CheckBox();
            getFileNames.Text     = "Extract file names, if the archive contains file names.";
            getFileNames.Location = new Point(8, 66);
            getFileNames.Size     = new Size(this.Width - 16, 20);
            getFileNames.Checked  = true;

            this.Controls.Add(getFileNames);

            /* Extract archives inside the source archive. */
            autoExtractArchive          = new CheckBox();
            autoExtractArchive.Text     = "Extract archives located inside the source archive.";
            autoExtractArchive.Location = new Point(8, 90);
            autoExtractArchive.Size     = new Size(this.Width - 16, 20);

            this.Controls.Add(autoExtractArchive);

            /* Auto convert extracted images to PNG. */
            autoConvertImages          = new CheckBox();
            autoConvertImages.Text     = "Convert images to PNG, if possible.";
            autoConvertImages.Location = new Point(8, 114);
            autoConvertImages.Size     = new Size(this.Width - 16, 20);

            this.Controls.Add(autoConvertImages);

            /* Delete images that have been converted to PNG. */
            autoDeleteConverted          = new CheckBox();
            autoDeleteConverted.Text     = "Delete source image if conversion was sucessful.";
            autoDeleteConverted.Location = new Point(32, 138);
            autoDeleteConverted.Size     = new Size(this.Width - 16 - 24, 20);

            this.Controls.Add(autoDeleteConverted);

            /* Display Extract Button */
            doWorkButton   = new Button();
            doWorkButton.Text     = "Extract Archives";
            doWorkButton.Location = new Point(8, 162);
            doWorkButton.Size     = new Size(128, 24);
            doWorkButton.Click   += new EventHandler(startWork);

            this.Controls.Add(doWorkButton);

            this.ShowDialog();
        }

        /* Start & setup the work. */
        private void startWork(object sender, EventArgs e)
        {
            /* Disable the start work button. */
            doWorkButton.Enabled = false;

            /* Display status message. */
            status = new StatusMessage("Extract Archive", StatusMessage.extractArchive, files.Length);
            status.Show();

            /* Start to decompress the files now */
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Do the work. */
        private void run(object sender, DoWorkEventArgs e)
        {
            /* Loop through each of the files. */
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    /* Update the status. */
                    status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));

                    /* Load the file. */
                    FileStream file = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                    byte[] data     = new byte[file.Length];

                    file.Read(data, 0, (int)file.Length);
                    file.Close();

                    /* Check to see if the file is compressed. */
                    if (autoDecompress.Checked)
                    {
                        status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));
                        Compression compression = new Compression();
                        data = compression.decompress(data);
                    }

                    /* See if we can extract this file */
                    object[][] extractData = new object[0][];
                    string outputDir;

                    status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));

                    /* Now we can extract the archive. */
                    Archive archive = new Archive();
                    extractData = archive.extract(data, getFileNames.Checked, Path.GetFileName(files[i]));

                    /* Check if this is a valid archive. */
                    if (extractData[0].Length == 0)
                        continue;

                    /* Get the output dir. */
                    outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + archive.getOutputDirectory(data, Path.GetFileName(files[i])) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);

                    /* Create the directory if neccessary. */
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* Write the files. */
                    for (int j = 0; j < extractData[0].Length; j++)
                    {
                        byte[] outputData     = (byte[])extractData[0][j];
                        string outputFileName = (string)extractData[1][j];

                        /* Check to see if we have a filename and we don't want then. */
                        if (!getFileNames.Checked && outputFileName != String.Empty && outputFileName != null)
                            outputFileName = Path.GetFileNameWithoutExtension(files[i]) + "_" + j.ToString(getDigits(extractData[0].Length)) + Path.GetExtension(outputFileName);

                        /* Check to see if the filename is empty. */
                        else if (outputFileName == String.Empty || outputFileName == null)
                            outputFileName = Path.GetFileNameWithoutExtension(files[i]) + "_" + j.ToString(getDigits(extractData[0].Length)) + FileFormat.getFileExtension(outputData, files[i]);

                        /* Check to see if the file is compressed. */
                        if (autoDecompress.Checked)
                        {
                            status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));
                            Compression compression = new Compression();
                            outputData = compression.decompress(outputData);
                        }
                        status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));
                        
                        /* Now output the file */
                        FileStream outputFile = new FileStream(outputDir + Path.DirectorySeparatorChar + outputFileName, FileMode.Create, FileAccess.Write);
                        outputFile.Write(outputData, 0, outputData.Length);
                        outputFile.Close();

                        /* Convert the files to PNG. */
                        if (autoConvertImages.Checked)
                        {
                            status.updateStatus(StatusMessage.toPng, Path.GetFileName(files[i]), (i + 1));
                            Conversions.toPNG(outputData, outputDir + Path.DirectorySeparatorChar + outputFileName);

                            /* See if the conversion was successful and we want to delete source images. */
                            if (autoDeleteConverted.Checked && File.Exists(outputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(outputFileName) + ".png"))
                                File.Delete(outputDir + Path.DirectorySeparatorChar + outputFileName);
                        }

                        /* See if we need to extract this archive. */
                        if (autoExtractArchive.Checked)
                        {
                            object[][] newExtractData = new object[0][];
                            string newOutputDir       = String.Empty;

                            status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));

                            /* Now we can extract the archive. */
                            Archive subArchive = new Archive();
                            newExtractData = subArchive.extract(outputData, getFileNames.Checked, outputFileName);

                            /* Was it a valid archive? */
                            if (newExtractData.Length > 0)
                            {
                                /* Add it to the files list */
                                Array.Resize<string>(ref files, files.Length + 1);
                                files[files.Length - 1] = outputDir + Path.DirectorySeparatorChar + outputFileName;

                                status.updateTotalFiles(files.Length);
                            }
                        }
                    }

                }
                catch
                {
                    continue;
                }
            }

            /* Now we are done with the work. */
            status.Close();
            this.Close();
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
    }
}