using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class Compression_Decompressor : Form
    {
        /* Options */
        private CheckBox
            autoConvertImages,   // Auto convert images to PNG
            autoDeleteConverted; // Auto delete extract files converted to PNG

        private Button
            doWorkButton; // Start the work.

        private StatusMessage
            status; // Status Box.

        private string[]
            files; // Filenames.

        /* Select files and display options */
        public Compression_Decompressor()
        {
            /* Set up the window. */
            this.ClientSize      = new Size(400, 112);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Compression Decompressor Options";
            this.MaximizeBox     = false;

            /* Select the files. */
            files = Files.selectFiles("Select Compressed Files(s)", "CNX, CXLZ, LZ01 Compressed Files (*.*)|*.*");

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
            /* Auto convert extracted images to PNG. */
            autoConvertImages          = new CheckBox();
            autoConvertImages.Text     = "Convert images to PNG, if possible.";
            autoConvertImages.Location = new Point(8, 32);
            autoConvertImages.Size     = new Size(this.Width - 16, 20);

            this.Controls.Add(autoConvertImages);

            /* Delete images that have been converted to PNG. */
            autoDeleteConverted          = new CheckBox();
            autoDeleteConverted.Text     = "Delete source image if conversion was sucessful.";
            autoDeleteConverted.Location = new Point(32, 56);
            autoDeleteConverted.Size     = new Size(this.Width - 16 - 24, 20);

            this.Controls.Add(autoDeleteConverted);

            /* Display Extract Button */
            doWorkButton          = new Button();
            doWorkButton.Text     = "Decompress Files";
            doWorkButton.Location = new Point(8, 80);
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
            status = new StatusMessage("Decompress Files", StatusMessage.decompress, files.Length);
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
                    status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));

                    /* Load the file. */
                    FileStream file = new FileStream(files[i], FileMode.Open);
                    byte[] data = new byte[file.Length];
                    byte[] decompressedData;
                    string outputDir;

                    file.Read(data, 0, (int)file.Length);
                    file.Close();

                    Compression compression = new Compression();
                    decompressedData = compression.decompress(data);

                    /* The data wasn't compressed, or it wasn't a supported compression format. */
                    if (data == decompressedData || decompressedData.Length == 0)
                        continue;

                    /* Get the output dir. */
                    outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + compression.getOutputDirectory(data);

                    /* Create the directory if neccessary. */
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* Now output the file */
                    FileStream outputFile = new FileStream(outputDir + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), FileMode.Create);
                    outputFile.Write(decompressedData, 0, decompressedData.Length);
                    outputFile.Close();

                    /* Convert the files to PNG. */
                    if (autoConvertImages.Checked)
                    {
                        status.updateStatus(StatusMessage.toPng, Path.GetFileName(files[i]), (i + 1));
                        Conversions.toPNG(decompressedData, outputDir + Path.DirectorySeparatorChar + Path.GetFileName(files[i]));

                        /* See if the conversion was successful and we want to delete source images. */
                        if (autoDeleteConverted.Checked && File.Exists(outputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]) + ".png"))
                            File.Delete(outputDir + Path.DirectorySeparatorChar + Path.GetFileName(files[i]));
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
    }
}