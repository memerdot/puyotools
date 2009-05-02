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
    public class Compression_Decompress : Form
    {
        /* Set up our form variables */
        private GroupBox
            decompressionSettings   = new GroupBox(), // Decompression Settings
            imageConversionSettings = new GroupBox(); // Image Conversion Settings

        private CheckBox
            useStoredFilename = new CheckBox(), // Use stored filename
            deleteSourceFile  = new CheckBox(), // Delete Source file
            decompressSameDir = new CheckBox(), // Output to same directory
            unpackImage       = new CheckBox(), // Unpack image
            deleteSourceImage = new CheckBox(), // Delete Source Image
            convertSameDir    = new CheckBox(); // Output to same directory


        private Button
            startWorkButton = new Button(); // Start Work Button

        private string[]
            files; // Files to Decompress

        private StatusMessage
            status; // Status Message

        public Compression_Decompress()
        {
            /* Select the files */
            files = Files.selectFiles("Select Compressed Files",
                "All Files (*.*)|*.*|" +
                "CNX Compressed Files (*.cnx)|*.cnx|" +
                "NARC Compressed Archives (*.carc)|*.carc|" +
                "ONE Compressed Archives (*.onz)|*.onz");

            /* If no files were selected, don't continue */
            if (files.Length == 0)
                return;

            showOptions();
        }

        public Compression_Decompress(bool selectDirectory)
        {
            /* Select the directories */
            string directory = Files.SelectDirectory("Select a directory");

            /* If no directory was selected, don't continue */
            if (directory == null)
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
            FormContent.Create(this, "Compression - Decompress", new Size(400, 248));

            /* Files Selected */
            FormContent.Add(this, new Label(),
                String.Format("{0} {1} Selected",
                    files.Length.ToString(),
                    (files.Length > 1 ? "Files" : "File")),
                new Point(0, 8),
                new Size(this.Width, 16),
                ContentAlignment.TopCenter,
                new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold));

            /* Decompression Settings */
            FormContent.Add(this, decompressionSettings,
                "Decompression Settings",
                new Point(8, 32),
                new Size(this.Size.Width - 24, 84));

            /* Use stored filename */
            FormContent.Add(decompressionSettings, useStoredFilename, true,
                "Use filename stored in the compressed file.",
                new Point(8, 20),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Output to same directory */
            FormContent.Add(decompressionSettings, decompressSameDir,
                "Output file to same directory as source (and overwrite if necessary).",
                new Point(8, 40),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Delete source file */
            FormContent.Add(decompressionSettings, deleteSourceFile,
                "Delete source file (on successful decompression).",
                new Point(8, 60),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Image Conversion Settings */
            FormContent.Add(this, imageConversionSettings,
                "Image Conversion Settings",
                new Point(8, 124),
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

            /* Convert */
            FormContent.Add(this, startWorkButton,
                "Decompress",
                new Point((this.Width / 2) - 60, 216),
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
            status = new StatusMessage("Compression - Decompress", files);
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
                        /* Set up the decompressor */
                        Compression compression = new Compression(inputStream, Path.GetFileName(fileList[i]));
                        if (compression.Format == CompressionFormat.NULL)
                            continue;

                        /* Set up the output directories and file names */
                        outputDirectory = Path.GetDirectoryName(fileList[i]) + (decompressSameDir.Checked ? String.Empty : Path.DirectorySeparatorChar + compression.OutputDirectory);
                        outputFilename  = (useStoredFilename.Checked ? compression.GetFilename() : Path.GetFileName(fileList[i]));

                        /* Decompress data */
                        MemoryStream decompressedData = (MemoryStream)compression.Decompress();

                        /* Check to make sure the decompression was successful */
                        if (decompressedData == null)
                            continue;
                        else
                            data = decompressedData;
                    }

                    /* Create the output directory if it does not exist */
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);

                    /* Write file data */
                    using (FileStream outputStream = new FileStream(outputDirectory + Path.DirectorySeparatorChar + outputFilename, FileMode.Create, FileAccess.Write))
                        data.WriteTo(outputStream);

                    /* Delete source image? */
                    if (deleteSourceFile.Checked && File.Exists(fileList[i]))
                        File.Delete(fileList[i]);

                    /* Unpack image? */
                    if (unpackImage.Checked)
                    {
                        /* Create Image object and make sure the format is supported */
                        Images images = new Images(data, outputFilename);
                        if (images.Format != GraphicFormat.NULL)
                        {
                            /* Set up our input and output image */
                            string inputImage  = outputDirectory + Path.DirectorySeparatorChar + outputFilename;
                            string outputImage = outputDirectory + Path.DirectorySeparatorChar + (convertSameDir.Checked ? String.Empty : images.OutputDirectory + Path.DirectorySeparatorChar) + Path.GetFileNameWithoutExtension(outputFilename) + ".png";

                            /* Convert image */
                            Bitmap imageData = images.Unpack();

                            /* Make sure an image was written */
                            if (imageData != null)
                            {
                                data = new MemoryStream();
                                imageData.Save(data, ImageFormat.Png);

                                /* Create the output directory if it does not exist */
                                if (!Directory.Exists(Path.GetDirectoryName(outputImage)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(outputImage));

                                /* Output the image */
                                using (FileStream outputStream = new FileStream(outputImage, FileMode.Create, FileAccess.Write))
                                    data.WriteTo(outputStream);

                                /* Delete the source image if we want to */
                                if (deleteSourceImage.Checked && File.Exists(inputImage) && File.Exists(outputImage))
                                    File.Delete(inputImage);
                            }
                        }
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