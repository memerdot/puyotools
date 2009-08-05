using System;
using System.IO;
using Extensions;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace puyo_tools
{
    public class Archive_Extract : Form
    {
        /* Set up our form variables */
        private GroupBox
            extractionSettings      = new GroupBox(), // Extraction Settings
            decompressionSettings   = new GroupBox(), // Decompression Settings
            imageConversionSettings = new GroupBox(); // Image Conversion Settings

        private CheckBox
            extractFilenames        = new CheckBox(), // Extract Filenames
            extractSameDir          = new CheckBox(), // Extract to the same directory
            extractExtracted        = new CheckBox(), // Extract extracted archives
            extractDirSameFilename  = new CheckBox(), // Extract to directory with same filename
            deleteSourceArchive     = new CheckBox(), // Delete source archive
            decompressSourceFile    = new CheckBox(), // Decompress Source File
            decompressExtractedFile = new CheckBox(), // Decompress Extracted File
            decompressExtractedDir  = new CheckBox(), // Decompress Extracted File to different directory
            useStoredFilename       = new CheckBox(), // Use stored filename
            unpackImage             = new CheckBox(), // Unpack image
            convertSameDir          = new CheckBox(), // Output to same directory
            deleteSourceImage       = new CheckBox(); // Delete Source Image

        private Button
            startWorkButton = new Button(); // Start Work Button

        private string[]
            files; // Files to Decompress

        private StatusMessage
            status; // Status Message

        public Archive_Extract()
        {
            /* Select the files */
            files = FileSelectionDialog.OpenFiles("Select Archive",
                "Supported Archives (*.acx;*.afs;*.carc;*.gnt;*.gvm;*.mdl;*.mrg;*.mrz;*.narc;*.one;*.onz;*.pvm;*.snt;*.spk;*.tex;*.tez;*.txd;*.vdd)|*.acx;*.afs;*.carc;*.gnt;*.gvm;*.mdl;*.mrg;*.mrz;*.narc;*.one;*.onz;*.pvm;*.snt;*.spk;*.tex;*.tez;*.txd;*.vdd|" +
                "ACX Archive (*.acx)|*.acx|" +
                "AFS Archive (*.afs)|*.afs|" +
                "GNT Archive (*.gnt)|*.gnt|" +
                "GVM Archive (*.gvm)|*.gvm|" +
                "MDL Archive (*.mdl)|*.mdl|" +
                "MRG Archive (*.mrg;*.mrz)|*.mrg;*.mrz|" +
                "NARC Archive (*.narc;*.carc)|*.narc;*.carc|" +
                "ONE Archive (*.one;*.onz)|*.one;*.onz|" +
                "PVM Archive (*.pvm)|*.pvm|" +
                "SNT Archive (*.snt)|*.snt|" +
                "SPK Archive (*.spk)|*.spk|" +
                "TEX Archive (*.tex;*.tez)|*.tex;*.tez|" +
                "TXAG Archive (*.txd)|*.txd|" +
                "VDD Archive (*.vdd)|*.vdd|" +
                "All Files (*.*)|*.*");

            /* If no files were selected, don't continue */
            if (files == null || files.Length == 0)
                return;

            /* Show Options */
            showOptions();
        }

        public Archive_Extract(bool selectDirectory)
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
            FormContent.Create(this, "Archive - Extract", new Size(428, 420));

            /* Files Selected */
            FormContent.Add(this, new Label(),
                String.Format("{0} {1} Selected",
                    files.Length.ToString(),
                    (files.Length > 1 ? "Files" : "File")),
                new Point(0, 8),
                new Size(this.Width, 16),
                ContentAlignment.TopCenter,
                new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold));

            /* Extraction Settings */
            FormContent.Add(this, extractionSettings,
                "Extraction Settings",
                new Point(8, 32),
                new Size(this.Size.Width - 24, 144));

            /* Extract filenames */
            FormContent.Add(extractionSettings, extractFilenames, true,
                "Extract filenames from archive.",
                new Point(8, 20),
                new Size(extractionSettings.Size.Width - 16, 16));

            /* Extract to same directory */
            FormContent.Add(extractionSettings, extractSameDir,
                "Extract files to the same directory as source (and overwrite if necessary).",
                new Point(8, 40),
                new Size(extractionSettings.Size.Width - 16, 16));

            /* Extract to directory with same filename */
            FormContent.Add(extractionSettings, extractDirSameFilename,
                "Extract to directory with the same filename as source archive.\n(You must check the option to delete the source archive.)",
                new Point(8, 60),
                new Size(extractionSettings.Size.Width - 16, 36));

            /* Extract extracted archives */
            FormContent.Add(extractionSettings, extractExtracted,
                "Extract archives extracted from the source archive.",
                new Point(8, 100),
                new Size(extractionSettings.Size.Width - 16, 16));

            /* Delete archive */
            FormContent.Add(extractionSettings, deleteSourceArchive,
                "Delete source archive (on successful extraction).",
                new Point(8, 120),
                new Size(extractionSettings.Size.Width - 16, 16));

            /* Decompression Settings */
            FormContent.Add(this, decompressionSettings,
                "Decompression Settings",
                new Point(8, 184),
                new Size(this.Size.Width - 24, 104));

            /* Decompress file */
            FormContent.Add(decompressionSettings, decompressSourceFile, true,
                "Decompress source file.",
                new Point(8, 20),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Decompress extracted files */
            FormContent.Add(decompressionSettings, decompressExtractedFile,
                "Decompress extracted files.",
                new Point(8, 40),
                new Size(decompressionSettings.Size.Width - 16, 16));

            // Decompress to different directory
            FormContent.Add(decompressionSettings, decompressExtractedDir,
                "Place decompressed files in different directory.",
                new Point(24, 60),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Use stored filename */
            FormContent.Add(decompressionSettings, useStoredFilename, true,
                "Use filename stored in the compressed file.",
                new Point(8, 80),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Image Conversion Settings */
            FormContent.Add(this, imageConversionSettings,
                "Image Conversion Settings",
                new Point(8, 296),
                new Size(this.Size.Width - 24, 84));

            /* Unpack image */
            FormContent.Add(imageConversionSettings, unpackImage,
                "Convert decompressed file to PNG (if it is a supported image).",
                new Point(8, 20),
                new Size(imageConversionSettings.Size.Width - 16, 16));

            /* Output to same directory */
            FormContent.Add(imageConversionSettings, convertSameDir,
                "Output file to same directory as source (and overwrite if necessary).",
                new Point(8, 40),
                new Size(imageConversionSettings.Size.Width - 16, 16));

            /* Delete source file */
            FormContent.Add(imageConversionSettings, deleteSourceImage,
                "Delete source image (on successful conversion).",
                new Point(8, 60),
                new Size(imageConversionSettings.Size.Width - 16, 16));

            /* Extract */
            FormContent.Add(this, startWorkButton,
                "Extract",
                new Point((this.Width / 2) - 60, 388),
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

            /* Now, show our status */
            status = new StatusMessage("Archive - Extract", files);
            status.addProgressBarLocal();

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

            for (int i = 0; i < fileList.Count; i++)
            {
                /* Set the current file */
                status.CurrentFile      = i;
                status.CurrentFileLocal = 0;

                //Set the image file list
                List<string> imageFileList = new List<string>();

                try
                {
                    /* Open up the file */
                    MemoryStream data = null;
                    string outputFilename  = Path.GetFileName(fileList[i]);
                    string outputDirectory = Path.GetDirectoryName(fileList[i]);
                    using (Stream inputStream = new FileStream(fileList[i], FileMode.Open, FileAccess.Read))
                    {
                        /* Decompress this file? */
                        if (decompressSourceFile.Checked)
                        {
                            /* Set up the decompressor */
                            Compression compression = new Compression(inputStream, Path.GetFileName(fileList[i]));
                            if (compression.Format != CompressionFormat.NULL)
                            {
                                /* Decompress data */
                                MemoryStream decompressedData = compression.Decompress();

                                /* Check to make sure the decompression was successful */
                                if (decompressedData != null)
                                {
                                    data = decompressedData;
                                    if (useStoredFilename.Checked)
                                        outputFilename = compression.DecompressFilename;
                                }
                            }
                        }

                        /* Let's get the file list now */
                        Archive archive = new Archive((data == null ? inputStream : data), outputFilename);
                        if (archive.Format == ArchiveFormat.NULL)
                            continue;

                        ArchiveFileList archiveFileList = archive.GetFileList();
                        if (archiveFileList == null || archiveFileList.Entries.Length == 0)
                            continue;

                        /* Set the total files in the archive */
                        status.TotalFilesLocal = archiveFileList.Entries.Length;

                        /* Create the extraction directory */
                        outputDirectory = Path.GetDirectoryName(fileList[i]);
                        if (extractDirSameFilename.Checked && deleteSourceArchive.Checked)
                            outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        else if (!extractSameDir.Checked)
                            outputDirectory += Path.DirectorySeparatorChar + archive.OutputDirectory + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(outputFilename);

                        if (!Directory.Exists(outputDirectory))
                            Directory.CreateDirectory(outputDirectory);

                        /* Extract the data */
                        for (int j = 0; j < archiveFileList.Entries.Length; j++)
                        {
                            /* Set the file number in the archive */
                            status.CurrentFileLocal = j;

                            /* Load the file into a MemoryStream */
                            MemoryStream outputData = archive.GetData().Copy(archiveFileList.Entries[j].Offset, archiveFileList.Entries[j].Length);

                            /* Get the filename we will extract the data to */
                            string extractFilename;
                            if (extractFilenames.Checked && archiveFileList.Entries[j].Filename != String.Empty)
                                extractFilename = archiveFileList.Entries[j].Filename;
                            else
                                extractFilename = j.ToString().PadLeft(archiveFileList.Entries.Length.Digits(), '0') + FileData.GetFileExtension(ref outputData);

                            /* Decompress this data before we write it? */
                            if (decompressExtractedFile.Checked && !decompressExtractedDir.Checked)
                            {
                                /* Set up the decompressor */
                                Compression compression = new Compression(outputData, Path.GetFileName(extractFilename));
                                if (compression.Format != CompressionFormat.NULL)
                                {
                                    /* Decompress data */
                                    MemoryStream decompressedData = compression.Decompress();

                                    /* Check to make sure the decompression was successful */
                                    if (decompressedData != null)
                                    {
                                        outputData = decompressedData;
                                        if (useStoredFilename.Checked)
                                            extractFilename = compression.DecompressFilename;
                                    }
                                }
                            }

                            /* Write the data */
                            using (FileStream outputStream = new FileStream(outputDirectory + Path.DirectorySeparatorChar + extractFilename, FileMode.Create, FileAccess.Write))
                                outputStream.Write(outputData);

                            // Do we want to decompress the file to a different directory?
                            if (decompressExtractedFile.Checked && decompressExtractedDir.Checked)
                            {
                                // Set up the decompressor
                                Compression compression = new Compression(outputData, Path.GetFileName(extractFilename));
                                if (compression.Format != CompressionFormat.NULL)
                                {
                                    // Decompress data
                                    MemoryStream decompressedData = compression.Decompress();

                                    // Check to make sure the decompression was successful */
                                    if (decompressedData != null)
                                    {
                                        outputData = decompressedData;
                                        if (useStoredFilename.Checked)
                                            extractFilename = compression.DecompressFilename;

                                        // Now write the file to the decompressed directory
                                        if (!Directory.Exists(outputDirectory + Path.DirectorySeparatorChar + compression.OutputDirectory))
                                            Directory.CreateDirectory(outputDirectory + Path.DirectorySeparatorChar + compression.OutputDirectory);
                                        using (FileStream outputStream = new FileStream(outputDirectory + Path.DirectorySeparatorChar + compression.OutputDirectory + Path.DirectorySeparatorChar + extractFilename, FileMode.Create, FileAccess.Write))
                                            outputStream.Write(outputData);
                                    }
                                }
                            }


                            /* Convert the file to an image? */
                            if (unpackImage.Checked)
                            {
                                Images images = new Images(outputData, extractFilename);
                                if (images.Format != GraphicFormat.NULL)
                                {
                                    // Add it to the list so we can process it later
                                    imageFileList.Add(outputDirectory + Path.DirectorySeparatorChar + extractFilename);
                                }
                            }

                            /* Extract the extracted file? */
                            if (extractExtracted.Checked)
                            {
                                Archive testArchive = new Archive(outputData, extractFilename);
                                if (testArchive.Format != ArchiveFormat.NULL)
                                {
                                    /* Set the directory appropiately */
                                    string addFile;
                                    if (deleteSourceArchive.Checked && extractDirSameFilename.Checked)
                                        addFile = fileList[i] + Path.DirectorySeparatorChar + extractFilename;
                                    else
                                        addFile = outputDirectory + Path.DirectorySeparatorChar + extractFilename;

                                    fileList.Add(addFile);
                                    status.AddFile(addFile);
                                }
                            }
                        }
                    }

                    // Convert images now
                    if (unpackImage.Checked && imageFileList.Count > 0)
                    {
                        // Reset the local file count
                        status.CurrentFileLocal = 0;
                        status.TotalFilesLocal  = imageFileList.Count;

                        for (int j = 0; j < imageFileList.Count; j++)
                        {
                            // Set the local file count
                            status.CurrentFileLocal = j;

                            using (FileStream inputData = new FileStream(imageFileList[j], FileMode.Open, FileAccess.Read))
                            {
                                Images images = new Images(inputData, imageFileList[j]);
                                if (images.Format != GraphicFormat.NULL)
                                {
                                    // Set up our input and output image
                                    string inputImage  = imageFileList[j];
                                    string outputImage = Path.GetDirectoryName(inputImage) + Path.DirectorySeparatorChar + (convertSameDir.Checked ? String.Empty : images.OutputDirectory + Path.DirectorySeparatorChar) + Path.GetFileNameWithoutExtension(inputImage) + ".png";

                                    // Convert image
                                    Bitmap imageData = null;
                                    try
                                    {
                                        imageData = images.Unpack();
                                    }
                                    catch (GraphicFormatNeedsPalette)
                                    {
                                        // See if the palette file exists
                                        if (File.Exists(Path.GetDirectoryName(inputImage) + Path.DirectorySeparatorChar + images.PaletteFilename))
                                        {
                                            using (FileStream paletteData = new FileStream(Path.GetDirectoryName(inputImage) + Path.DirectorySeparatorChar + images.PaletteFilename, FileMode.Open, FileAccess.Read))
                                            {
                                                images.Decoder.PaletteData = paletteData;
                                                imageData = images.Unpack();
                                            }
                                        }
                                    }

                                    // Make sure an image was written
                                    if (imageData != null)
                                    {
                                        MemoryStream outputData = new MemoryStream();
                                        imageData.Save(outputData, ImageFormat.Png);

                                        // Create the output directory if it does not exist
                                        if (!Directory.Exists(Path.GetDirectoryName(outputImage)))
                                            Directory.CreateDirectory(Path.GetDirectoryName(outputImage));

                                        // Write the image
                                        using (FileStream outputStream = new FileStream(outputImage, FileMode.Create, FileAccess.Write))
                                            outputStream.Write(outputData);

                                        // Delete the source image if we want to
                                        if (deleteSourceImage.Checked && File.Exists(inputImage) && File.Exists(outputImage))
                                            File.Delete(inputImage);
                                    }
                                }
                            }
                        }
                    }

                    // Delete the source archive now?
                    if (deleteSourceArchive.Checked && File.Exists(fileList[i]))
                    {
                        File.Delete(fileList[i]);

                        // We have to rename that directory, remember? */
                        if (extractDirSameFilename.Checked)
                            Directory.Move(outputDirectory, fileList[i]);
                    }
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