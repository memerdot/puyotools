using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Collections.Generic;
using Extensions;

namespace puyo_tools
{
    public class Archive_Explorer : Form
    {
        /* Tool Strip */
        private ToolStripMenuItem
            addFileNumberExtracted,
            decompressExtracted,
            extractImageAsPng;
        private ToolStripButton
            extractSelectedFiles,
            extractAllFiles;

        /* List View stuff */
        private ListView fileListView = new ListView();
        private ColumnHeader
            fileList_number = new ColumnHeader(),
            fileList_name   = new ColumnHeader(),
            fileList_size   = new ColumnHeader();

        /* Archive Information */
        private GroupBox archiveInformation = new GroupBox();
        private Label
            archiveInformation_name  = new Label(),
            archiveInformation_files = new Label(),
            archiveInformation_type  = new Label();

        /* Archive Details */
        private int level = 0;
        private List<ArchiveFileList> FileList = new List<ArchiveFileList>();
        private List<Stream> ArchiveData  = new List<Stream>();
        private List<string> ArchiveName  = new List<string>();
        private List<string> ArchiveType  = new List<string>();

        public Archive_Explorer()
        {
            /* Create the window */
            FormContent.Create(this, "Puyo Tools Archive Explorer", new Size(600, 392));

            /* Tool Strip */
            extractSelectedFiles = new ToolStripButton("Extract Selected Files", (Bitmap)new ComponentResourceManager(typeof(icons)).GetObject("save"), new EventHandler(extract));
            extractAllFiles      = new ToolStripButton("Extract All Files", (Bitmap)new ComponentResourceManager(typeof(icons)).GetObject("save_all"), new EventHandler(extract));

            addFileNumberExtracted = new ToolStripMenuItem("Add file number to extracted files", null, new EventHandler(selectOption));
            decompressExtracted    = new ToolStripMenuItem("Decompress extracted files", null, new EventHandler(selectOption));
            extractImageAsPng      = new ToolStripMenuItem("Extract images as PNG", null, new EventHandler(selectOption));

            ToolStrip toolStrip = new ToolStrip(new ToolStripItem[] {
                new ToolStripButton("Open Archive", (Bitmap)new ComponentResourceManager(typeof(icons)).GetObject("open"), new EventHandler(loadArchive)),
                new ToolStripSeparator(),
                extractSelectedFiles,
                extractAllFiles,
                new ToolStripSeparator(),
                new ToolStripDropDownButton("Extraction Options", null, new ToolStripMenuItem[] {
                    addFileNumberExtracted,
                    decompressExtracted,
                    extractImageAsPng,
                }),
            });

            this.Controls.Add(toolStrip);

            /* Display the file list */
            /* Add the File Details */
            fileListView.Location = new Point(8, 32);
            fileListView.Size = new Size(584, 280);
            fileListView.View = View.Details;
            fileListView.GridLines = true;
            fileListView.FullRowSelect = true;
            fileListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            fileListView.DoubleClick += new EventHandler(loadEmbeddedArchive);

            /* Quick hack for setting height of list view items */
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(1, 16);
            fileListView.SmallImageList = imageList;

            /* Add Column Headers */
            fileList_number.Text = "#";
            fileList_number.Width = 48;
            fileListView.Columns.Add(fileList_number);

            fileList_name.Text = "Filename";
            fileList_name.Width = 280;
            fileListView.Columns.Add(fileList_name);

            fileList_size.Text = "Output Filesize";
            fileList_size.Width = 200;
            fileListView.Columns.Add(fileList_size);

            this.Controls.Add(fileListView);

            /* Add the Archive Information */
            FormContent.Add(this, archiveInformation,
                "Archive Information",
                new Point(8, 320),
                new Size(584, 65));

            FormContent.Add(archiveInformation, archiveInformation_name,
                String.Empty,
                new Point(8, 16),
                new Size(archiveInformation.Size.Width - 16, 16));

            FormContent.Add(archiveInformation, archiveInformation_files,
                String.Empty,
                new Point(8, 32),
                new Size(archiveInformation.Size.Width - 16, 16));

            FormContent.Add(archiveInformation, archiveInformation_type,
                String.Empty,
                new Point(8, 48),
                new Size(archiveInformation.Size.Width - 16, 16));

            this.ShowDialog();
        }

        private ArchiveFileList GetFileList(ref Stream data, string filename, out string type)
        {
            type = null;

            try
            {
                /* Check to see if the archive is compressed */
                Compression compression = new Compression(data, filename);
                if (compression.Format != CompressionFormat.NULL)
                {
                    /* Decompress */
                    MemoryStream decompressedData = compression.Decompress();
                    if (decompressedData != null)
                        data = decompressedData;
                }

                /* Check to see if this is an archive */
                Archive archive = new Archive(data, filename);
                if (archive.Format == ArchiveFormat.NULL)
                    throw new ArchiveFormatNotSupported();

                /* Check to see if the data was translated */
                if (data != archive.Data)
                    data = archive.Data;

                /* Get the file list and archive type */
                type = archive.ArchiveName;
                return archive.GetFileList();
            }
            catch
            {
                return null;
            }
        }

        private void populateList(ArchiveFileList fileList)
        {
            /* Erase the current list */
            fileListView.Items.Clear();

            /* Make sure the file list contains entries */
            if (fileList == null || fileList.Entries == 0)
                return;

            /* If we are not at the base level, we need to add some extra crap */
            if (level > 0)
            {
                /* Add the Parent Archive link */
                ListViewItem item = new ListViewItem(new string[] {
                    "..",
                    "Parent Archive",
                    null,
                });
                item.Font = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);

                fileListView.Items.Add(item);
            }

            for (int i = 0; i < fileList.Entries; i++)
            {
                /* Add the file information */
                ListViewItem item = new ListViewItem(new string[] {
                    (i + 1).ToString("#,0"),
                    (fileList.Entry[i].FileName == String.Empty ? " -- No Filename -- " : fileList.Entry[i].FileName),
                    String.Format("{0} ({1} bytes)", FormatFileSize(fileList.Entry[i].Length), fileList.Entry[i].Length.ToString("#,0")),
                });

                fileListView.Items.Add(item);
            }
        }

        /* Select Option */
        private void selectOption(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
        }

        /* Load Archive */
        private void loadArchive(object sender, EventArgs e)
        {
            /* Select the file to open */
            string file = FileSelectionDialog.OpenFile("Select Archive",
                "Supported Archives (*.acx;*.afs;*.carc;*.gnt;*.gvm;*.mdl;*.mrg;*.narc;*.one;*.onz;*.pvm;*.snt;*.spk;*.tex;*.txd;*.vdd)|*.acx;*.afs;*.carc;*.gnt;*.gvm;*.mdl;*.mrg;*.narc;*.one;*.onz;*.pvm;*.snt;*.spk;*.tex;*.txd;*.vdd|" +
                "ACX Archive (*.acx)|*.acx|" +
                "AFS Archive (*.afs)|*.afs|" +
                "GNT Archive (*.gnt)|*.gnt|" +
                "GVM Archive (*.gvm)|*.gvm|" +
                "MDL Archive (*.mdl)|*.mdl|" + 
                "MRG Archive (*.mrg)|*.mrg|" +
                "NARC Archive (*.narc;*.carc)|*.narc;*.carc|" +
                "ONE Archive (*.one;*.onz)|*.one;*.onz|" +
                "PVM Archive (*.pvm)|*.pvm|" +
                "SNT Archive (*.snt)|*.snt|" +
                "SPK Archive (*.spk)|*.spk|" +
                "TEX Archive (*.tex)|*.tex|" +
                "TXAG Archive (*.txd)|*.txd|" +
                "VDD Archive (*.vdd)|*.vdd|" +
                "All Files (*.*)|*.*");

            if (file == null || file == String.Empty)
                return;

            /* Ok, load the file and get the file list */
            string archiveType   = String.Empty;
            Stream archiveStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            ArchiveFileList fileList = GetFileList(ref archiveStream, file, out archiveType);

            /* If we were able to open it up and get the contents, populate the list */
            if (fileList != null && fileList.Entries > 0)
            {
                /* Clear the archive data list if it contains entries */
                if (ArchiveData.Count > 0)
                {
                    level = 0;
                    FileList.Clear();
                    ArchiveData.Clear();
                    ArchiveName.Clear();
                    ArchiveType.Clear();
                }

                /* Add the entries */
                FileList.Add(fileList);
                ArchiveData.Add(archiveStream);
                ArchiveName.Add(Path.GetFileName(file));
                ArchiveType.Add(archiveType);

                /* Populate the list and update the archive information */
                populateList(fileList);
                updateArchiveInformation();
            }
        }

        private void loadEmbeddedArchive(object sender, EventArgs e)
        {
            if (fileListView.SelectedIndices.Count == 1)
            {
                /* Let's make a copy of the selected file data and see if it is an archive */
                int selectedItem = fileListView.SelectedIndices[0] - (level == 0 ? 0 : 1);

                /* Did we select the first item and we are not at the base? */
                if (selectedItem < 0 && level > 0)
                {
                    /* Go back to the previous entry */
                    level--;
                    FileList.RemoveAt(FileList.Count - 1);
                    ArchiveData.RemoveAt(ArchiveData.Count - 1);
                    ArchiveName.RemoveAt(ArchiveName.Count - 1);
                    ArchiveType.RemoveAt(ArchiveType.Count - 1);

                    /* Populate the list and update the archive information */
                    populateList(FileList[level]);
                    updateArchiveInformation();
                }
                else
                {
                    /* Open up the new archive if we can */
                    Stream archiveStream = ArchiveData[level].Copy(FileList[level].Entry[selectedItem].Offset, FileList[level].Entry[selectedItem].Length);

                    /* Try to see if we can open this */
                    string archiveType  = String.Empty;
                    ArchiveFileList fileList = GetFileList(ref archiveStream, FileList[level].Entry[selectedItem].FileName, out archiveType);

                    /* Was it an archive that we can open? */
                    if (fileList != null && fileList.Entries > 0)
                    {
                        /* Yes it is! */
                        FileList.Add(fileList);
                        ArchiveData.Add(archiveStream);
                        ArchiveName.Add(FileList[level].Entry[selectedItem].FileName);
                        ArchiveType.Add(archiveType);
                        level++;

                        /* Populate the list and update the archive information */
                        populateList(fileList);
                        updateArchiveInformation();
                    }
                    else
                    {
                        /* Maybe this is an image? */
                        loadEmbeddedImage();
                    }
                }
            }
        }

        private void loadEmbeddedImage()
        {
            try
            {
                /* get the selected item and the data and filename */
                int item = fileListView.SelectedIndices[0] - (level == 0 ? 0 : 1);
                MemoryStream imageData = ArchiveData[level].Copy(FileList[level].Entry[item].Offset, FileList[level].Entry[item].Length);
                string filename = FileList[level].Entry[item].FileName;

                /* Check to see if the archive is compressed */
                Compression compression = new Compression(imageData, filename);
                if (compression.Format != CompressionFormat.NULL)
                {
                    /* Decompress */
                    MemoryStream decompressedData = compression.Decompress();
                    if (decompressedData != null)
                        imageData = decompressedData;
                }

                /* Check to see if this is an image */
                Images image = new Images(imageData, filename);
                if (image.Format == GraphicFormat.NULL)
                    throw new GraphicFormatNotSupported();

                /* Check to see if a palette file exists */
                if (image.Format == GraphicFormat.SVR && indexOfFile(Path.GetFileNameWithoutExtension(filename) + ".svp") != -1)
                {
                }

                /* Try to open this image if we can */
                try
                {
                    new Image_Viewer(imageData, filename);
                }
                catch (GraphicFormatNeedsPalette)
                {
                    string paletteFile = Path.GetFileNameWithoutExtension(filename);
                    if (image.Format == GraphicFormat.GVR)
                        paletteFile += ".gvp";
                    else if (image.Format == GraphicFormat.SVR)
                        paletteFile += ".svp";

                    int index = indexOfFile(paletteFile);
                    if (index == -1)
                        throw new Exception();
                    else
                        new Image_Viewer(imageData, filename, ArchiveData[level].Copy(FileList[level].Entry[index].Offset, FileList[level].Entry[index].Length));
                }
            }
            catch
            {
            }
        }

        /* Find file */
        private int indexOfFile(string filename)
        {
            /* Let's see if an entry exists with that filename */
            for (int i = 0; i < FileList[level].Entries; i++)
            {
                if (filename.ToLower() == FileList[level].Entry[i].FileName.ToLower())
                    return i;
            }

            return -1;
        }

        /* Extract files */
        private void extract(object sender, EventArgs e)
        {
            /* Set the files to extract */
            List<int> files = new List<int>();
            if (sender == extractSelectedFiles) // Select only the files chosen
            {
                for (int i = 0; i < fileListView.SelectedIndices.Count; i++)
                {
                    if (level == 0 || fileListView.SelectedIndices[i] > 0)
                        files.Add(fileListView.SelectedIndices[i]);
                }
            }
            else // Select all the files
            {
                for (int i = (level == 0 ? 0 : 1); i < fileListView.Items.Count; i++)
                    files.Add(i);
            }

            /* Don't continue if no files were selected */
            if (files.Count == 0)
                return;

            /* Display the save file dialog or folder dialog */
            string output_directory = String.Empty;
            string output_filename  = String.Empty;

            /* Directory selection if there are more than one file */
            if (files.Count > 1)
            {
                output_directory = FileSelectionDialog.SaveDirectory("Select a directory to output the files to");

                /* Make sure a directory was selected */
                if (output_directory == null || output_directory == String.Empty)
                    return;
            }

            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    string filename = FileList[level].Entry[files[i] - (level > 0 ? 1 : 0)].FileName;
                    int num         = files[i] - (level > 0 ? 1 : 0);

                    output_filename = (filename == String.Empty ? num.ToString().PadLeft(FileList.Count.Digits(), '0') :
                        (addFileNumberExtracted.Checked ? Path.GetFileNameWithoutExtension(filename) + '_' + num.ToString().PadLeft(FileList.Count.Digits(), '0') + Path.GetExtension(filename) :
                        filename));

                    /* Which selection dialog do we want? */
                    if (files.Count == 1)
                    {
                        output_filename = FileSelectionDialog.SaveFile("Extract File", output_filename,
                            (Path.GetExtension(filename) == String.Empty ? "All Files (*.*)|*.*" : Path.GetExtension(filename).Substring(1).ToUpper() + " File (*" + Path.GetExtension(filename) + ")|*" + Path.GetExtension(filename)));

                        /* Make sure we selected a file */
                        if (output_filename == null || output_filename == String.Empty)
                            return;

                        output_directory = Path.GetDirectoryName(output_filename);
                        output_filename  = Path.GetFileName(output_filename);
                    }

                    /* If for some reason the output directory doesn't exist, create it */
                    if (!Directory.Exists(output_directory))
                        Directory.CreateDirectory(output_directory);

                    /* Copy the data to a new stream */
                    MemoryStream outputData = ArchiveData[level].Copy(FileList[level].Entry[num].Offset, FileList[level].Entry[num].Length);

                    /* Do we want to decompress the file? */
                    if (decompressExtracted.Checked)
                    {
                        Compression compression = new Compression(outputData, output_filename);
                        if (compression.Format != CompressionFormat.NULL)
                        {
                            MemoryStream decompressedData = compression.Decompress();
                            if (decompressedData != null)
                            {
                                outputData      = decompressedData;
                                output_filename = compression.GetFilename();
                            }
                        }
                    }

                    /* Convert the file to a PNG? */
                    if (extractImageAsPng.Checked)
                    {
                        Images images = new Images(outputData, output_filename);
                        if (images.Format != GraphicFormat.NULL)
                        {
                            Bitmap imageData = images.Unpack();
                            if (imageData != null)
                            {
                                outputData = new MemoryStream();
                                imageData.Save(outputData, ImageFormat.Png);
                                output_filename = Path.GetFileNameWithoutExtension(output_filename) + ".png";
                            }
                        }
                    }

                    /* Write the file */
                    using (FileStream outputStream = new FileStream(output_directory + Path.DirectorySeparatorChar + output_filename, FileMode.Create, FileAccess.Write))
                        outputStream.Write(outputData);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void updateArchiveInformation()
        {
            /* Update the archive information */
            archiveInformation_name.Text  = String.Empty;
            archiveInformation_files.Text = "Files: " + FileList[level].Entries.ToString("#,0");
            archiveInformation_type.Text  = "Format: " + ArchiveType[level];

            for (int i = 0; i <= level; i++)
                archiveInformation_name.Text += (i == 0 ? String.Empty : " / ") + ArchiveName[i];
        }

        private string FormatFileSize(long bytes)
        {
            /* Set byte values */
            long
                kilobyte = 1024,
                megabyte = 1024 * kilobyte,
                gigabyte = 1024 * megabyte,
                terabyte = 1024 * gigabyte;

            /* Ok, let's format our filesize now */
            if (bytes > terabyte)      return Decimal.Divide(bytes, terabyte).ToString("#,###.00") + " TB";
            else if (bytes > gigabyte) return Decimal.Divide(bytes, gigabyte).ToString("#,###.00") + " GB";
            else if (bytes > megabyte) return Decimal.Divide(bytes, megabyte).ToString("#,###.00") + " MB";
            else if (bytes > kilobyte) return Decimal.Divide(bytes, kilobyte).ToString("#,###.00") + " KB";

            return bytes.ToString("#,0") + " bytes";
        }
    }
}