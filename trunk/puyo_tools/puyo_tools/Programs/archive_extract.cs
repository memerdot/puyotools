using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace puyo_tools
{
    public class archive_extract : Form
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
        public archive_extract()
        {
            /* Set up the window. */
            this.ClientSize      = new Size(400, 190);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = "Archive Extractor Options";
            this.MaximizeBox     = false;

            /* Select the files. */
            files = Files.selectFiles("Select Archive(s)", "All Supported Archives (*.acx;*.afs;*.gnt;*.mrg;*.snt;*.spk;*.tex;*.vdd)|*.acx;*.afs;*.gnt;*.mrg;*.snt;*.spk;*.tex;*.vdd|ACX Archives (*.acx)|*.acx|AFS Archives (*.afs)|*.afs|GNT Archives (*.gnt)|*.gnt|MRG Archives (*.mrg)|*.mrg|SNT Archives (*.snt)|*.snt|SPK Archives (*.spk)|*.spk|TEX Archives (*.tex)|*.tex|VDD Archives (*.vdd)|*.vdd|All Files & Archives (*.*)|*.*");

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
            autoDecompress.Text     = "Decompress source and extracted files containing CXLZ or LZ01 compression.";
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

            /* Extract GNT, SNT, and TEX archives inside other archives. */
            autoExtractArchive          = new CheckBox();
            autoExtractArchive.Text     = "Extract GNT, SNT, and TEX archives located inside the source archive.";
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
                    FileStream file = new FileStream(files[i], FileMode.Open);
                    byte[] data     = new byte[file.Length];

                    file.Read(data, 0, (int)file.Length);
                    file.Close();

                    /* Check to see if the file is compressed. */
                    if (autoDecompress.Checked)
                    {
                        status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));
                        data = decompressFile(data);
                    }

                    /* See if we can extract this file */
                    object[][] extractData = new object[0][];
                    string outputDir;

                    status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));

                    /* ACX Archive */
                    if (Header.isFile(data, Header.ACX, 0) && Path.GetExtension(files[i]).ToLower() == ".acx")
                    {
                        ACX extractor = new ACX();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.ACX + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* AFS Archive */
                    else if (Header.isFile(data, Header.AFS, 0))
                    {
                        AFS extractor = new AFS();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.AFS + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* GNT Archive */
                    else if (Header.isFile(data, Header.GNT, 0) && Header.isFile(data, Header.GNT_SUB, 0x20))
                    {
                        GNT extractor = new GNT();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.GNT + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* MRG Archive */
                    else if (Header.isFile(data, Header.MRG, 0))
                    {
                        MRG extractor = new MRG();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.MRG + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* SNT Archive */
                    else if ((Header.isFile(data, Header.SNT_PS2, 0) && Header.isFile(data, Header.SNT_SUB_PS2, 0x20)) ||
                             (Header.isFile(data, Header.SNT_PSP, 0) && Header.isFile(data, Header.SNT_SUB_PSP, 0x20)))
                    {
                        SNT extractor = new SNT();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.SNT + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* SPK Archive */
                    else if (Header.isFile(data, Header.SPK, 0))
                    {
                        SPK extractor = new SPK();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.SPK + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* TEX Archive */
                    else if (Header.isFile(data, Header.TEX, 0))
                    {
                        TEX extractor = new TEX();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.TEX + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* VDD Archive */
                    else if (Path.GetExtension(files[i]).ToLower() == ".vdd")
                    {
                        VDD extractor = new VDD();
                        extractData = extractor.extract(data, getFileNames.Checked);

                        /* Set the output directory. */
                        outputDir = Path.GetDirectoryName(files[i]) + Path.DirectorySeparatorChar + ExtractDir.VDD + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(files[i]);
                    }

                    /* Not a valid archive. */
                    else
                        continue;

                    /* Create the directory if neccessary. */
                    if (extractData[0].Length > 0 && !Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    /* Write the files. */
                    for (int j = 0; j < extractData[0].Length; j++)
                    {
                        byte[] outputData     = (byte[])extractData[0][j];
                        string outputFileName = (string)extractData[1][j];

                        /* Check to see if the filename is empty. */
                        if (outputFileName == String.Empty || outputFileName == null)
                            outputFileName = Path.GetFileNameWithoutExtension(files[i]) + "_" + j.ToString(getDigits(extractData[0].Length)) + getFileExt(outputData, files[i]);

                        /* Check to see if the file is compressed. */
                        if (autoDecompress.Checked)
                        {
                            status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));
                            outputData = decompressFile(outputData);
                        }
                        status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));
                        
                        /* Now output the file */
                        FileStream outputFile = new FileStream(outputDir + Path.DirectorySeparatorChar + outputFileName, FileMode.Create);
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

                            /* GNT Archive */
                            if (Header.isFile(outputData, Header.GNT, 0) && Header.isFile(outputData, Header.GNT_SUB, 0x20))
                            {
                                GNT extractor = new GNT();
                                newExtractData = extractor.extract(outputData, getFileNames.Checked);

                                /* Set the output directory. */
                                newOutputDir = outputDir + Path.DirectorySeparatorChar + ExtractDir.GNT + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(outputFileName);
                            }

                            /* SNT Archive */
                            else if ((Header.isFile(outputData, Header.SNT_PS2, 0) && Header.isFile(outputData, Header.SNT_SUB_PS2, 0x20)) ||
                                (Header.isFile(outputData, Header.SNT_PSP, 0) && Header.isFile(outputData, Header.SNT_SUB_PSP, 0x20)))
                            {
                                SNT extractor = new SNT();
                                newExtractData = extractor.extract(outputData, getFileNames.Checked);

                                /* Set the output directory. */
                                newOutputDir = outputDir + Path.DirectorySeparatorChar + ExtractDir.SNT + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(outputFileName);
                            }

                            /* TEX Archive */
                            else if (Header.isFile(outputData, Header.TEX, 0))
                            {
                                TEX extractor = new TEX();
                                newExtractData = extractor.extract(outputData, getFileNames.Checked);

                                /* Set the output directory. */
                                newOutputDir = outputDir + Path.DirectorySeparatorChar + ExtractDir.TEX + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(outputFileName);
                            }

                            /* Was it an archive? */
                            if (newOutputDir != String.Empty)
                            {
                                /* Create the directory if neccessary. */
                                if (newExtractData[0].Length > 0 && !Directory.Exists(newOutputDir))
                                    Directory.CreateDirectory(newOutputDir);

                                /* Write the files. */
                                for (int k = 0; k < newExtractData[0].Length; k++)
                                {
                                    byte[] newOutputData     = (byte[])newExtractData[0][k];
                                    string newOutputFileName = (string)newExtractData[1][k];

                                    /* Check to see if the filename is empty. */
                                    if (outputFileName == String.Empty || outputFileName == null)
                                        outputFileName = Path.GetFileNameWithoutExtension(outputFileName) + "_" + j.ToString(getDigits(newExtractData[0].Length)) + getFileExt(newOutputData, outputFileName);

                                    /* Check to see if the file is compressed. */
                                    if (autoDecompress.Checked)
                                    {
                                        status.updateStatus(StatusMessage.decompress, Path.GetFileName(files[i]), (i + 1));
                                        outputData = decompressFile(outputData);
                                    }
                                    status.updateStatus(StatusMessage.extractArchive, Path.GetFileName(files[i]), (i + 1));

                                    /* Now output the file */
                                    outputFile = new FileStream(newOutputDir + Path.DirectorySeparatorChar + newOutputFileName, FileMode.Create);
                                    outputFile.Write(newOutputData, 0, newOutputData.Length);
                                    outputFile.Close();

                                    /* Convert the files to PNG. */
                                    if (autoConvertImages.Checked)
                                    {
                                        status.updateStatus(StatusMessage.toPng, Path.GetFileName(files[i]), (i + 1));
                                        Conversions.toPNG(newOutputData, newOutputDir + Path.DirectorySeparatorChar + newOutputFileName);

                                        /* See if the conversion was successful and we want to delete source images. */
                                        if (autoDeleteConverted.Checked && File.Exists(newOutputDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(newOutputFileName) + ".png"))
                                            File.Delete(newOutputDir + Path.DirectorySeparatorChar + newOutputFileName);
                                    }
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
    }
}