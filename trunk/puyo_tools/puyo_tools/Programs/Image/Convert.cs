using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class Image_Convert : Form
    {
        /* Set up our form variables */
        private GroupBox
            imageConversionSettings = new GroupBox(), // Image Conversion Settings
            decompressionSettings   = new GroupBox(); // Decompression Settings

        private CheckBox
            convertSameDir    = new CheckBox(), // Output to same directory
            deleteSourceImage = new CheckBox(), // Delete Source Image
            decompressFile    = new CheckBox(), // Decompress File
            useStoredFilename = new CheckBox(); // Use stored filename

        private Button
            startWorkButton = new Button(); // Start Work Button

        private string[]
            files; // Files to Decompress

        private StatusMessage
            status; // Status Message

        public Image_Convert()
        {
            /* Select the files */
            files = FileSelectionDialog.OpenFiles("Select Image Files",
                "Supported Image Formats (*.cnx;*.gim;*.gvr;*.pvr;*.pvz;*.svr)|*.cnx;*.gim;*.gvr;*.pvr;*.pvz;*.svr|" +
                "CNX Compressed Image (*.cnx)|*.cnx|" +
                "GIM Images (*.gim)|*.gim|" +
                "GVR Images (*.gvr)|*.gvr|" +
                "PVR Images (*.pvr;*.pvz)|*.pvr;*.pvz|" +
                "SVR Images (*.svr)|*.svr|" +
                "All Files (*.*)|*.*");

            /* If no files were selected, don't continue */
            if (files == null || files.Length == 0)
                return;

            /* Show Options */
            showOptions();
        }

        public Image_Convert(bool selectDirectory)
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
            FormContent.Create(this, "Image - Convert", new Size(400, 208));

            /* Files Selected */
            FormContent.Add(this, new Label(),
                String.Format("{0} {1} Selected",
                    files.Length.ToString(),
                    (files.Length > 1 ? "Files" : "File")),
                new Point(0, 8),
                new Size(this.Width, 16),
                ContentAlignment.TopCenter,
                new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold));

            /* Image Conversion Settings */
            FormContent.Add(this, imageConversionSettings,
                "Image Conversion Settings",
                new Point(8, 32),
                new Size(this.Size.Width - 24, 64));

            /* Output to same directory */
            FormContent.Add(imageConversionSettings, convertSameDir,
                "Output file to same directory as source (and overwrite if necessary).",
                new Point(8, 20),
                new Size(imageConversionSettings.Size.Width - 16, 16));

            /* Delete source file */
            FormContent.Add(imageConversionSettings, deleteSourceImage,
                "Delete source image (on successful conversion).",
                new Point(8, 40),
                new Size(imageConversionSettings.Size.Width - 16, 16));

            /* Decompression Settings */
            FormContent.Add(this, decompressionSettings,
                "Decompression Settings",
                new Point(8, 104),
                new Size(this.Size.Width - 24, 64));

            /* Decompress file */
            FormContent.Add(decompressionSettings, decompressFile, true,
                "Decompress source file.",
                new Point(8, 20),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Use stored filename */
            FormContent.Add(decompressionSettings, useStoredFilename, true,
                "Use filename stored in the compressed file.",
                new Point(8, 40),
                new Size(decompressionSettings.Size.Width - 16, 16));

            /* Convert */
            FormContent.Add(this, startWorkButton,
                "Convert",
                new Point((this.Width / 2) - 60, 176),
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
            status = new StatusMessage("Image - Convert", files);
            bw.RunWorkerAsync();
            status.ShowDialog();
        }

        /* Convert the images */
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
                    MemoryStream data = null;
                    string outputFilename  = Path.GetFileName(fileList[i]);
                    string outputDirectory = Path.GetDirectoryName(fileList[i]);
                    using (Stream inputStream = new FileStream(fileList[i], FileMode.Open, FileAccess.Read))
                    {
                        /* Decompress this file? */
                        if (decompressFile.Checked)
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

                        /* Create Image object and make sure the format is supported */
                        Images images = new Images((data == null ? inputStream : data), outputFilename);
                        if (images.Format == GraphicFormat.NULL)
                            continue;

                        /* Set up our input and output image */
                        string inputImage  = outputDirectory + Path.DirectorySeparatorChar + outputFilename;
                        string outputImage = outputDirectory + Path.DirectorySeparatorChar + (convertSameDir.Checked ? String.Empty : images.OutputDirectory + Path.DirectorySeparatorChar) + Path.GetFileNameWithoutExtension(outputFilename) + ".png";
                        outputFilename     = outputImage;

                        /* Convert image */
                        Bitmap imageData = images.Unpack();

                        /* Don't continue if an image wasn't created */
                        if (imageData == null)
                            continue;

                        data = new MemoryStream();
                        imageData.Save(data, ImageFormat.Png);

                        /* Create the output directory if it does not exist */
                        if (!Directory.Exists(Path.GetDirectoryName(outputImage)))
                            Directory.CreateDirectory(Path.GetDirectoryName(outputImage));

                        /* Output the image */
                        using (FileStream outputStream = new FileStream(outputImage, FileMode.Create, FileAccess.Write))
                            outputStream.Write(data);
                    }

                    /* Delete the source image if we want to */
                    if (deleteSourceImage.Checked && File.Exists(fileList[i]) && File.Exists(outputFilename))
                        File.Delete(fileList[i]);
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