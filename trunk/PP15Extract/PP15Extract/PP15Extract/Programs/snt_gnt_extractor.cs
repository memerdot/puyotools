using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace pp15_fileTools
{
    public class SNT_GNT_Extract : Form
    {
        /* Variables */
        OpenFileDialog selectFiles;        // Select Files Dialog
        StatusMessage status;              // Status Messages
        private byte[] data;               // File Data
        private int[][] extractOffsets;    // Extracted Offsets & File Lengths
        private string[] extractFileNames; // Extracted Filenames.
        Button startButton;                // Decompress Button
        CheckBox autoDecompress;           // Auto Decompress LZ01 / CXLZ
        CheckBox findFileNames;            // Attempt to find filenames
        CheckBox autoConvert;              // Auto Convert to PNG
        CheckBox autoDelete;               // Delete extracted files.
        string outputDir;                  // Output Directory of file

        BackgroundWorker bw = new BackgroundWorker(); // Background Worker

        public SNT_GNT_Extract()
        {
            /* Select our files */
            selectFiles                  = new OpenFileDialog();
            selectFiles.Title            = "Select SNT / GNT file(s)";
            selectFiles.Multiselect      = true;
            selectFiles.RestoreDirectory = true;
            selectFiles.CheckFileExists  = true;
            selectFiles.CheckPathExists  = true;
            selectFiles.AddExtension     = true;
            selectFiles.Filter           = "SNT / GNT Files|*.*";
            selectFiles.DefaultExt       = "";
            selectFiles.ShowDialog();

            if (selectFiles.FileNames.Length < 1)
                return;


            /* Set up the window */
            this.ClientSize      = new Size(400, 170);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "SNT / GNT Extractor Options";

            /* Number of files selected. */
            Label filesSelected     = new Label();
            filesSelected.Text      = selectFiles.FileNames.Length + " File" + (selectFiles.FileNames.Length > 1 ? "s" : "") + " Selected";
            filesSelected.Location  = new Point(8, 8);
            filesSelected.Size      = new Size(384, 16);
            filesSelected.TextAlign = ContentAlignment.MiddleCenter;
            filesSelected.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            this.Controls.Add(filesSelected);

            /* Add an option to decompress compress file */
            autoDecompress          = new CheckBox();
            autoDecompress.Text     = "Decompress file if it contains LZ01 / CXLZ compression.";
            autoDecompress.Location = new Point(8, 32);
            autoDecompress.Size     = new Size(384, 20);
            autoDecompress.Checked  = true;
            this.Controls.Add(autoDecompress);

            /* Add an option to decompress compress file */
            findFileNames          = new CheckBox();
            findFileNames.Text     = "Attempt to find filenames for files in the archive.";
            findFileNames.Location = new Point(8, 56);
            findFileNames.Size     = new Size(384, 20);
            this.Controls.Add(findFileNames);

            /* Add an option to convert to PNG */
            autoConvert          = new CheckBox();
            autoConvert.Text     = "Attempt to convert file to PNG if it is an image.";
            autoConvert.Location = new Point(8, 80);
            autoConvert.Size     = new Size(384, 20);
            this.Controls.Add(autoConvert);

            /* Auto Delete Files */
            autoDelete          = new CheckBox();
            autoDelete.Text     = "Delete extracted files if they were successfully converted to PNG.";
            autoDelete.Location = new Point(24, 104);
            autoDelete.Size     = new Size(368, 20);
            this.Controls.Add(autoDelete);

            /* Add the Convert Button */
            startButton          = new Button();
            startButton.Text     = "Extract";
            startButton.Location = new Point(8, 136);
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
            status = new StatusMessage("Extract SNT / GNT File", StatusMessage.extractArchive, selectFiles.FileNames.Length);
            status.Show();

            /* Decompress our files now */
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Run the SNT / GNT Extractor */
        private void run(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < selectFiles.FileNames.Length; i++)
            {
                try
                {
                    status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                    FileStream file = new FileStream(selectFiles.FileNames[i], FileMode.Open);
                    data = new byte[file.Length];
                    file.Read(data, 0, data.Length);
                    file.Close();

                    /* Attempt to decompress? */
                    if (autoDecompress.Enabled)
                    {
                        /* Is this file LZ01 compressed? */
                        if (Header.isFile(data, Header.LZ01, 0))
                        {
                            status.updateStatus(StatusMessage.decompress, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                            CNX decompressor = new CNX();
                            data = decompressor.decompress(data);

                            status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                        }

                        /* Is this file CXLZ compressed? */
                        else if (Header.isFile(data, Header.CXLZ, 0))
                        {
                            status.updateStatus(StatusMessage.decompress, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                            CXLZ decompressor = new CXLZ();
                            data = decompressor.decompress(data);

                            status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                        }
                    }

                    /* Let's get our list of filenames and offsets */
                    if (Header.isFile(data, Header.SNT, 0) && Header.isFile(data, Header.SNT2, 0x20))
                    {
                        SNT snt = new SNT();
                        extractOffsets = snt.extract(data, findFileNames.Enabled);
                        extractFileNames = snt.extractFileNames;

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "SNT Extracted Files" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(selectFiles.FileNames[i]);
                    }
                    else if (Header.isFile(data, Header.GNT, 0) && Header.isFile(data, Header.GNT2, 0x20))
                    {
                        GNT gnt = new GNT();
                        extractOffsets = gnt.extract(data, findFileNames.Enabled);
                        extractFileNames = gnt.extractFileNames;

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "GNT Extracted Files" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(selectFiles.FileNames[i]);
                    }
                    else
                        continue;

                    /* Create directory for decompressed files */
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* We got a list of files */
                    for (int j = 0; j < extractOffsets.Length; j++)
                    {
                        if (extractFileNames[j] == String.Empty)
                        {
                            extractFileNames[j] = Path.GetFileNameWithoutExtension(selectFiles.FileNames[i]) + "_" + j;

                            /* Find the correct extension */
                            if (Header.isFile(data, Header.GIM, extractOffsets[j][0]) || Header.isFile(data, Header.MIG, extractOffsets[j][0]))
                                extractFileNames[j] += ".gim";
                            else if (Header.isFile(data, Header.SVR, extractOffsets[j][0]))
                                extractFileNames[j] += ".svr";
                            else if (Header.isFile(data, Header.GVR, extractOffsets[j][0]))
                                extractFileNames[j] += ".gvr";
                            else
                                extractFileNames[j] += Path.GetExtension(selectFiles.FileNames[i]);
                        }

                        /* Attempt to write the file now. */
                        file = new FileStream(outputDir + Path.DirectorySeparatorChar + extractFileNames[j], FileMode.Create);
                        file.Write(data, extractOffsets[j][0], extractOffsets[j][1]);
                        file.Close();

                        /* Attempt to convert to PNG if possible */
                        if (autoConvert.Checked)
                        {
                            byte[] extractData = new byte[extractOffsets[j][1]];
                            Array.Copy(data, extractOffsets[j][0], extractData, 0, extractOffsets[j][1]);
                            status.updateStatus(StatusMessage.toPng, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                            Conversions.toPNG(extractData, outputDir + Path.DirectorySeparatorChar + extractFileNames[j]);

                            /* Let's see if we can delete the original file now */
                            if (autoDelete.Checked && File.Exists(outputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(extractFileNames[j]) + ".png"))
                                File.Delete(outputDir + Path.DirectorySeparatorChar + extractFileNames[j]);
                        }
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