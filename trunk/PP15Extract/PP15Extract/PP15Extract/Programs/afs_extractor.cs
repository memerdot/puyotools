using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace pp15_fileTools
{
    public class AFS_Extract : Form
    {
        /* Variables */
        OpenFileDialog selectFiles;        // Select Files Dialog
        StatusMessage status;              // Status Messages
        private byte[] data;               // File Data
        private int[][] extractOffsets;    // Extracted Offsets & File Lengths
        private string[] extractFileNames; // Extracted Filenames.
        private byte[] fileData;           // Extracted File Data
        private int[][] sntExtractOffsets; // SNT / GNT Extracted Offsets
        private string[] sntExtractFileNames; // SNT / GNT Extracted File Names
        Button startButton;                // Decompress Button
        CheckBox autoDecompress;           // Auto Decompress LZ01 / CXLZ
        CheckBox autoExtract;              // Auto Extract SNT / GNT Files
        CheckBox findFileNames;            // Attempt to find filenames
        CheckBox autoConvert;              // Auto Convert to PNG
        CheckBox autoDelete;               // Delete extracted files.
        string outputDir;                  // Output Directory of file
        string sntOutputDir;               // SNT / GNT Output Directory

        BackgroundWorker bw = new BackgroundWorker(); // Background Worker

        public AFS_Extract()
        {
            /* Select our files */
            selectFiles                  = new OpenFileDialog();
            selectFiles.Title            = "Select AFS file(s)";
            selectFiles.Multiselect      = true;
            selectFiles.RestoreDirectory = true;
            selectFiles.CheckFileExists  = true;
            selectFiles.CheckPathExists  = true;
            selectFiles.AddExtension     = true;
            selectFiles.Filter           = "AFS Files|*.*";
            selectFiles.DefaultExt       = "";
            selectFiles.ShowDialog();

            if (selectFiles.FileNames.Length < 1)
                return;


            /* Set up the window */
            this.ClientSize      = new Size(400, 206);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "AFS Extractor Options";

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
            autoDecompress.Text     = "Decompress extracted files if they contain LZ01 / CXLZ compression.";
            autoDecompress.Location = new Point(8, 32);
            autoDecompress.Size     = new Size(384, 20);
            this.Controls.Add(autoDecompress);

            /* Add an option to extract an SNT / GNT file */
            autoExtract          = new CheckBox();
            autoExtract.Text     = "Extract SNT / GNT extracted files.";
            autoExtract.Location = new Point(8, 56);
            autoExtract.Size     = new Size(384, 20);
            this.Controls.Add(autoExtract);

            /* Add an option to decompress compress file */
            findFileNames          = new CheckBox();
            findFileNames.Text     = "Attempt to find filenames for files in SNT archives.";
            findFileNames.Location = new Point(24, 80);
            findFileNames.Size     = new Size(384, 20);
            this.Controls.Add(findFileNames);

            /* Add an option to convert to PNG */
            autoConvert          = new CheckBox();
            autoConvert.Text     = "Attempt to convert AFS / SNT / GNT extracted files to PNG if it is an image.";
            autoConvert.Location = new Point(8, 104);
            autoConvert.Size     = new Size(384, 32);
            this.Controls.Add(autoConvert);

            /* Auto Delete Files */
            autoDelete          = new CheckBox();
            autoDelete.Text     = "Delete files that have been successfully converted to PNG.";
            autoDelete.Location = new Point(24, 140);
            autoDelete.Size     = new Size(368, 20);
            this.Controls.Add(autoDelete);

            /* Add the Extract Button */
            startButton          = new Button();
            startButton.Text     = "Extract";
            startButton.Location = new Point(8, 172);
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
            status = new StatusMessage("Extract AFS File", StatusMessage.extractArchive, selectFiles.FileNames.Length);
            status.Show();

            /* Decompress our files now */
            bw.DoWork += run;
            bw.RunWorkerAsync();
        }

        /* Run the AFS Extractor */
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

                    if (Header.isFile(data, Header.AFS, 0))
                    {
                        AFS afs          = new AFS();
                        extractOffsets   = afs.extract(data);
                        extractFileNames = afs.extractFileNames;

                        /* Set directory for output files */
                        outputDir = Path.GetDirectoryName(selectFiles.FileNames[i]) + Path.DirectorySeparatorChar + "AFS Extracted Files" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(selectFiles.FileNames[i]);
                    }
                    else
                        continue;

                    /* Create directory for decompressed files */
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* We got a list of files */
                    for (int j = 0; j < extractOffsets.Length; j++)
                    {
                        /* Create file data */
                        fileData = new byte[extractOffsets[j][1]];
                        Array.Copy(data, extractOffsets[j][0], fileData, 0, extractOffsets[j][1]);

                        /* Decompress extracted files, if we want to. */
                        if (autoDecompress.Checked)
                        {
                            /* Is this file LZ01 compressed? */
                            if (Header.isFile(fileData, Header.LZ01, 0))
                            {
                                status.updateStatus(StatusMessage.decompress, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                                CNX decompressor = new CNX();
                                fileData = decompressor.decompress(fileData);

                                status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                            }

                            /* Is this file CXLZ compressed? */
                            else if (Header.isFile(fileData, Header.CXLZ, 0))
                            {
                                status.updateStatus(StatusMessage.decompress, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));

                                CXLZ decompressor = new CXLZ();
                                fileData = decompressor.decompress(fileData);

                                status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                            }
                        }

                        /* Attempt to write the file now. */
                        file = new FileStream(outputDir + Path.DirectorySeparatorChar + extractFileNames[j], FileMode.Create);
                        file.Write(fileData, 0, fileData.Length);
                        file.Close();

                        /* Attempt to convert to PNG if possible */
                        if (autoConvert.Checked)
                        {
                            status.updateStatus(StatusMessage.toPng, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                            Conversions.toPNG(fileData, outputDir + Path.DirectorySeparatorChar + extractFileNames[j]);

                            /* Let's see if we can delete the original file now */
                            if (autoDelete.Checked && File.Exists(outputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(extractFileNames[j]) + ".png"))
                                File.Delete(outputDir + Path.DirectorySeparatorChar + extractFileNames[j]);
                        }

                        /* Now we want to extract some SNT / GNT archives */
                        if (autoExtract.Checked)
                        {
                            /* Let's get our list of filenames and offsets */
                            if (Header.isFile(fileData, Header.SNT, 0) && Header.isFile(fileData, Header.SNT2, 0x20))
                            {
                                SNT snt = new SNT();
                                sntExtractOffsets   = snt.extract(fileData, findFileNames.Enabled);
                                sntExtractFileNames = snt.extractFileNames;

                                /* Set directory for output files */
                                sntOutputDir = outputDir + Path.DirectorySeparatorChar + "SNT Extracted Files" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(extractFileNames[j]);
                            }
                            else if (Header.isFile(fileData, Header.GNT, 0) && Header.isFile(fileData, Header.GNT2, 0x20))
                            {
                                GNT gnt = new GNT();
                                sntExtractOffsets   = gnt.extract(fileData, findFileNames.Enabled);
                                sntExtractFileNames = gnt.extractFileNames;

                                /* Set directory for output files */
                                sntOutputDir = outputDir + Path.DirectorySeparatorChar + "GNT Extracted Files" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(extractFileNames[j]);
                            }
                            else
                                continue;

                            /* Create directory for decompressed files */
                            if (!Directory.Exists(sntOutputDir))
                                Directory.CreateDirectory(sntOutputDir);

                            /* We got a list of files */
                            for (int k = 0; k < sntExtractOffsets.Length; k++)
                            {
                                if (sntExtractFileNames[k] == String.Empty)
                                {
                                    sntExtractFileNames[k] = Path.GetFileNameWithoutExtension(extractFileNames[j]) + "_" + k;

                                    /* Find the correct extension */
                                    if (Header.isFile(fileData, Header.GIM, sntExtractOffsets[k][0]) || Header.isFile(fileData, Header.MIG, sntExtractOffsets[k][0]))
                                        sntExtractFileNames[k] += ".gim";
                                    else if (Header.isFile(fileData, Header.SVR, sntExtractOffsets[k][0]))
                                        sntExtractFileNames[k] += ".svr";
                                    else if (Header.isFile(fileData, Header.GVR, sntExtractOffsets[k][0]))
                                        sntExtractFileNames[k] += ".gvr";
                                    else
                                        sntExtractFileNames[k] += Path.GetExtension(extractFileNames[j]);
                                }

                                /* Attempt to write the file now. */
                                file = new FileStream(sntOutputDir + Path.DirectorySeparatorChar + sntExtractFileNames[k], FileMode.Create);
                                file.Write(fileData, sntExtractOffsets[k][0], sntExtractOffsets[k][1]);
                                file.Close();

                                /* Attempt to convert to PNG if possible */
                                if (autoConvert.Checked)
                                {
                                    byte[] extractData = new byte[sntExtractOffsets[k][1]];
                                    Array.Copy(fileData, sntExtractOffsets[k][0], extractData, 0, sntExtractOffsets[k][1]);
                                    status.updateStatus(StatusMessage.toPng, Path.GetFileName(selectFiles.FileNames[i]), (i + 1));
                                    Conversions.toPNG(extractData, sntOutputDir + Path.DirectorySeparatorChar + sntExtractFileNames[k]);

                                    /* Let's see if we can delete the original file now */
                                    if (autoDelete.Checked && File.Exists(sntOutputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(sntExtractFileNames[k]) + ".png"))
                                        File.Delete(sntOutputDir + Path.DirectorySeparatorChar + sntExtractFileNames[k]);
                                }
                            }
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