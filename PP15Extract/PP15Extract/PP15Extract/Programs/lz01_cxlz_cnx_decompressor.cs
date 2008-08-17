using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace pp15_fileTools
{
    public class LZ01_CNX_CXLZ_Decompress : Form
    {
        /* Variables */
        OpenFileDialog selectFiles; // Select Files Dialog
        StatusMessage status;       // Status Messages
        private byte[] data;        // File Data
        Button startButton;         // Decompress Button
        CheckBox autoConvert;       // Auto Convert to PNG
        CheckBox autoDelete;        // Delete extracted files.
        string outputDir;           // Output Directory of file

        BackgroundWorker bw = new BackgroundWorker(); // Background Worker

        public LZ01_CNX_CXLZ_Decompress()
        {
            /* Select our files */
            selectFiles                  = new OpenFileDialog();
            selectFiles.Title            = "Select LZ01 / CNX / CXLZ Compressed file(s)";
            selectFiles.Multiselect      = true;
            selectFiles.RestoreDirectory = true;
            selectFiles.CheckFileExists  = true;
            selectFiles.CheckPathExists  = true;
            selectFiles.AddExtension     = true;
            selectFiles.Filter           = "LZ01 / CNX / CXLZ Compressed Files|*.*";
            selectFiles.DefaultExt       = "";
            selectFiles.ShowDialog();

            if (selectFiles.FileNames.Length < 1)
                return;


            /* Set up the window */
            this.ClientSize      = new Size(400, 124);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "LZ01 / CNX / CXLZ Decompressor Options";

            /* Number of files selected. */
            Label filesSelected     = new Label();
            filesSelected.Text      = selectFiles.FileNames.Length + " File" + (selectFiles.FileNames.Length > 1 ? "s" : "") + " Selected";
            filesSelected.Location  = new Point(8, 8);
            filesSelected.Size      = new Size(384, 16);
            filesSelected.TextAlign = ContentAlignment.MiddleCenter;
            filesSelected.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            this.Controls.Add(filesSelected);

            /* Add an option to convert to PNG */
            autoConvert          = new CheckBox();
            autoConvert.Text     = "Attempt to convert file to PNG if it is an image.";
            autoConvert.Location = new Point(8, 32);
            autoConvert.Size     = new Size(384, 20);
            this.Controls.Add(autoConvert);

            /* Auto Delete Files */
            autoDelete          = new CheckBox();
            autoDelete.Text     = "Delete decompressed file if it was successfully converted to PNG.";
            autoDelete.Location = new Point(24, 56);
            autoDelete.Size     = new Size(368, 20);
            this.Controls.Add(autoDelete);

            /* Add the Convert Button */
            startButton          = new Button();
            startButton.Text     = "Decompress";
            startButton.Location = new Point(8, 88);
            startButton.Size     = new Size(128, 24);
            startButton.Click   += new EventHandler(startWork);
            this.Controls.Add(startButton);

            /* Finally, show the dialog */
            this.ShowDialog();
        }

        /* Decompress Button */
        private void startWork(object sender, EventArgs e)
        {
            /* Display Status Box */
            startButton.Enabled = false;
            status = new StatusMessage("Decompress LZ01 / CNX / CXLZ Compressed File", StatusMessage.decompress, selectFiles.FileNames.Length);
            status.Show();

            /* Decompress our files now */
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Run the LZ01 / CXLZ Decompressor */
        private void run(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < selectFiles.FileNames.Length; i++)
            {
                try
                {
                    status.updateStatus(StatusMessage.decompress, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                    FileStream file = new FileStream(selectFiles.FileNames[i], FileMode.Open);
                    data = new byte[file.Length];
                    file.Read(data, 0, data.Length);
                    file.Close();


                    /* Is this file LZ01 compressed? */
                    if (Header.isFile(data, Header.LZ01, 0))
                    {
                        CNX decompressor = new CNX();
                        data = decompressor.decompress(data);

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "LZ01 Decompressed Files";
                    }

                    /* Is this file CXLZ compressed? */
                    else if (Header.isFile(data, Header.CXLZ, 0))
                    {
                        CXLZ decompressor = new CXLZ();
                        data = decompressor.decompress(data);

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "CXLZ Decompressed Files";
                    }

                    /* Is this file CNX compressed? */
                    else if (Header.isFile(data, Header.CNX, 0))
                    {
                        CNX decompressor = new CNX();
                        data = decompressor.decompress(data);

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "CNX Decompressed Files";
                    }

                    /* Is the file not one of these? */
                    else
                        continue;

                    /* Create directory for decompressed files */
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* Attempt to write the file now. */
                    file = new FileStream(outputDir + Path.DirectorySeparatorChar + Path.GetFileName(selectFiles.FileNames[i]), FileMode.Create);
                    file.Write(data, 0, data.Length);
                    file.Close();

                    /* Attempt to convert to PNG if possible */
                    if (autoConvert.Checked)
                    {
                        status.updateStatus(StatusMessage.toPng, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                        Conversions.toPNG(data, outputDir + Path.DirectorySeparatorChar + Path.GetFileName(selectFiles.FileNames[i]));

                        /* Let's see if we can delete the original file now */
                        if (autoDelete.Checked && File.Exists(outputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(selectFiles.FileNames[i]) + ".png"))
                            File.Delete(outputDir + Path.DirectorySeparatorChar + Path.GetFileName(selectFiles.FileNames[i]));
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            status.Close();
            this.Close();
        }
    }
}