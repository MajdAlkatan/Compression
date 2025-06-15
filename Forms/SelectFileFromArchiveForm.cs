using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsApp1.Forms
{
    public partial class SelectFileFromArchiveForm : Form
    {
        public string SelectedFileName { get; private set; } = string.Empty;

        public SelectFileFromArchiveForm(List<string> fileNames)
        {
            InitializeComponent();

            listBoxFiles.Items.AddRange(fileNames.ToArray());

            btnOK.Enabled = false;

            listBoxFiles.SelectedIndexChanged += (s, e) =>
            {
                btnOK.Enabled = listBoxFiles.SelectedIndex != -1;
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedItem != null)
            {
                SelectedFileName = listBoxFiles.SelectedItem.ToString()!;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}