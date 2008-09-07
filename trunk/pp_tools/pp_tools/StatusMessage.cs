using System;
using System.Drawing;
using System.Windows.Forms;

namespace pp_tools
{
    public class StatusMessage : Form
    {
        Label mainText; // Main Text
        Label subText;  // Sub Text
        int totalFiles; // Number of files

        /* Status Messages */
        /* Compression */
        public static string
            decompress = "Decompressing File", // Decompressing
            compress   = "Compressing File";   // Compressing

        /* Archives */
        public static string
            extractArchive = "Extracting Archive", // Extract Archive
            createArchive  = "Creating Archive";   // Create Archive

        /* Images */
        public static string
            toPng = "Converting image to PNG", // Convert to PNG
            toGim = "Converting image to GIM", // Convert to GIM
            toGvr = "Converting image to GVR"; // Convert to GVR


        public StatusMessage(string title, string header, int files)
        {
            /* Set up the window */
            this.ClientSize      = new Size(300, 110);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.Text            = title;
            this.ControlBox      = false;

            /* Display "Decompressing" */
            mainText           = new Label();
            mainText.Text      = header;
            mainText.Location  = new Point(8, 30);
            mainText.Size      = new Size(284, 16);
            mainText.Font      = new Font(SystemFonts.DialogFont.FontFamily.Name, SystemFonts.DialogFont.Size, FontStyle.Bold);
            mainText.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(mainText);

            subText           = new Label();
            subText.Text      = "File 1 of " + files;
            subText.Location  = new Point(8, 50);
            subText.Size      = new Size(284, 64);
            subText.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(subText);

            /* Set total files */
            totalFiles = files;
        }

        public void updateStatus(string header, string fileName, int file)
        {
            /* Updates the status */
            mainText.Text = header;
            subText.Text = "File " + file + " of " + totalFiles + "\n\n\n" + fileName;
        }
    }
}