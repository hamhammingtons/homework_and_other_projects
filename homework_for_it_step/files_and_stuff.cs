using System;
using System.IO;
using System.Windows.Forms;

namespace FileManagerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateDir_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtDirName.Text))
            {
                Directory.CreateDirectory(txtDirName.Text);
            }
        }

        private void btnGetDirInfo_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtDirName.Text))
            {
                DirectoryInfo di = new DirectoryInfo(txtDirName.Text);
                MessageBox.Show($"Name: {di.Name}\nCreated: {di.CreationTime}");
            }
        }

        private void btnMoveDir_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtDirName.Text))
            {
                Directory.Move(txtDirName.Text, txtMoveDirDest.Text);
            }
        }

        private void btnDeleteDir_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtDirName.Text))
            {
                Directory.Delete(txtDirName.Text, true);
            }
        }

        private void btnCreateFile_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text;
            if (!fileName.EndsWith(".txt")) fileName += ".txt";
            File.WriteAllText(fileName, "Hello World");
        }

        private void btnGetFileInfo_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text;
            if (!fileName.EndsWith(".txt")) fileName += ".txt";
            if (File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                MessageBox.Show($"Size: {fi.Length} bytes\nExtension: {fi.Extension}");
            }
        }

        private void btnCopyFile_Click(object sender, EventArgs e)
        {
            string source = txtFileName.Text;
            if (!source.EndsWith(".txt")) source += ".txt";
            if (File.Exists(source))
            {
                File.Copy(source, "copy_" + source, true);
            }
        }

        private void btnDeleteFile_Click(object sender, EventArgs e)
        {
            string fileName = txtFileName.Text;
            if (!fileName.EndsWith(".txt")) fileName += ".txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        private void btnMoveFile_Click(object sender, EventArgs e)
        {
            string source = txtFileName.Text;
            if (!source.EndsWith(".txt")) source += ".txt";
            if (File.Exists(source))
            {
                File.Move(source, txtMoveFileDest.Text);
            }
        }
    }
}
