using WinFormsApp1.Compression;

namespace WinFormsApp1.Forms
{
    public partial class MainForm : Form
    {
        private readonly List<string> selectedFiles = new();
        private RadioButton radioButtonIndividualFiles;
        private System.Windows.Forms.RadioButton radioButtonSingleArchive;

        public MainForm()
        {
            InitializeComponent();
            // comboBoxAlgorithm already filled in InitializeComponent, no need to add again here
            comboBoxAlgorithm.SelectedIndex = 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                labelStatus.Text = "Cancelling...";
                labelStatus.ForeColor = Color.Orange;
                btnCancel.Enabled = false;
                btnPauseResume.Enabled = false;
            }
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                var folderPath = folderDialog.SelectedPath;
                var files = Directory.GetFiles(folderPath);
                selectedFiles.Clear();
                selectedFiles.AddRange(files);
                listBoxFiles.Items.Clear();
                listBoxFiles.Items.AddRange(files);
                labelStatus.Text = $"{files.Length} files selected from folder.";
                labelStatus.ForeColor = Color.LightGreen;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new() { Multiselect = true, Title = "Select Files" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedFiles.Clear();
                selectedFiles.AddRange(dialog.FileNames);
                listBoxFiles.Items.Clear();
                listBoxFiles.Items.AddRange(dialog.FileNames);
                labelStatus.Text = $"{dialog.FileNames.Length} file(s) selected.";
                labelStatus.ForeColor = Color.LightGreen;
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            selectedFiles.Clear();
            selectedFiles.AddRange(files);
            listBoxFiles.Items.Clear();
            listBoxFiles.Items.AddRange(files);
            labelStatus.Text = $"{files.Length} file(s) added by drag & drop.";
            labelStatus.ForeColor = Color.LightGreen;
        }

        private void BtnPauseResume_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                pauseEvent.Reset();
                btnPauseResume.Text = "Resume";
                isPaused = true;
                labelStatus.Text = "Paused";
                labelStatus.ForeColor = Color.Orange;
            }
            else
            {
                pauseEvent.Set();
                btnPauseResume.Text = "Pause";
                isPaused = false;
                labelStatus.Text = "Resuming...";
                labelStatus.ForeColor = Color.DarkBlue;
            }
        }

        private async void btnCompress_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Please select files to compress.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            labelStatus.Text = "Compressing...";
            labelStatus.ForeColor = Color.DarkBlue;
            progressBar.Visible = true;

            string algo = comboBoxAlgorithm.SelectedItem?.ToString() ?? "Huffman";

            btnPauseResume.Enabled = true;
            btnPauseResume.Text = "Pause";
            isPaused = false;
            pauseEvent.Set();
            _cts = new CancellationTokenSource();
            btnCancel.Enabled = true;
            // -------------------------------------------------

            if (radioButtonIndividualFiles.Checked)
            {
                using var folderDialog = new FolderBrowserDialog
                { Description = "Select output folder for compressed files" };

                if (folderDialog.ShowDialog() != DialogResult.OK)
                {
                    labelStatus.Text = "Compression canceled.";
                    labelStatus.ForeColor = Color.Orange;
                    progressBar.Visible = false;

                    btnPauseResume.Enabled = false;
                    btnCancel.Enabled = false;
                    return;
                }

                string outputFolder = folderDialog.SelectedPath;
                Dictionary<string, double> ratios;
                try
                {
                    ratios = await Task.Run(async () =>
                    {
                        return await CompressionManager.CompressFilesAsync(selectedFiles,
                            algo,
                            outputFolder,
                            _cts.Token,
                            pauseEvent);
                    }
                    );
                }
                catch (OperationCanceledException)
                {
                    labelStatus.Text = "Cancelled";
                    labelStatus.ForeColor = Color.Red;
                    btnCancel.Enabled = false;
                    btnPauseResume.Enabled = false;
                    progressBar.Visible = false;
                    return;
                }

                progressBar.Visible = false;
                btnPauseResume.Enabled = false;
                btnCancel.Enabled = false;

                var msg = string.Join(Environment.NewLine,
                    ratios.Select(kv => $"{Path.GetFileName(kv.Key)}: {kv.Value:F2}%"));

                labelStatus.Text = $"Compression completed:\n{msg}";
                labelStatus.ForeColor = Color.Green;
            }
            else if (radioButtonSingleArchive.Checked)
            {
                using var saveDialog = new SaveFileDialog
                {
                    Title = "Save Archive As",
                    Filter = "Compressed Archive (*.cmp)|*.cmp",
                    DefaultExt = "cmp",
                    FileName = "archive.cmp"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                {
                    labelStatus.Text = "Compression canceled.";
                    labelStatus.ForeColor = Color.Orange;
                    progressBar.Visible = false;

                    btnPauseResume.Enabled = false;
                    btnCancel.Enabled = false;
                    return;
                }

                string archivePath = saveDialog.FileName;

                try
                {
                    await Task.Run(() =>
                    {
                         CompressionManager.CreateArchive(selectedFiles,
                          archivePath,
                          algo,
                          _cts.Token,
                          pauseEvent);
                    });

                    labelStatus.Text = $"Compression completed successfully.\nArchive saved at:\n{archivePath}";
                    labelStatus.ForeColor = Color.Green;
                }
                catch (OperationCanceledException)
                {
                    labelStatus.Text = "Compression canceled by user.";
                    labelStatus.ForeColor = Color.Orange;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during compression:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    labelStatus.Text = "Compression failed.";
                    labelStatus.ForeColor = Color.Red;
                }
                finally
                {
                    progressBar.Visible = false;
                    btnPauseResume.Enabled = false;
                    btnCancel.Enabled = false;
                }
            }
        }


        private async void btnDecompress_Click(object sender, EventArgs e)
        {
            string algo = comboBoxAlgorithm.SelectedItem?.ToString() ?? "Huffman";

            if (radioButtonIndividualFiles.Checked)
            {
                var cmpFiles = selectedFiles.FindAll(f => f.EndsWith(".cmp"));
                if (cmpFiles.Count == 0)
                {
                    MessageBox.Show("Please select .cmp files to decompress.", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                labelStatus.Text = "Decompressing...";
                labelStatus.ForeColor = Color.DarkBlue;
                progressBar.Visible = true;
                _cts = new CancellationTokenSource();
                btnCancel.Enabled = true;

                string archivePath = selectedFiles[0];

                string outputFolderPath;
                using var folderDialog = new FolderBrowserDialog();
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    var folderPath = folderDialog.SelectedPath;
                    outputFolderPath = folderPath;
                }
                else
                {
                    return;
                }

                using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
                using var br = new BinaryReader(fs);

                var type = (StorageType)br.ReadInt32();

                if (type == StorageType.ARCHIVE)
                {
                    await Task.Run(async () => await CompressionManager.ExtractAllFilesAsync(archivePath, outputFolderPath, _cts.Token, pauseEvent));
                }
                else
                {
                    await Task.Run(async () => await CompressionManager.DecompressFilesAsync(selectedFiles, _cts.Token));
                }
                progressBar.Visible = false;

                labelStatus.Text = "Decompression completed successfully.";
                labelStatus.ForeColor = Color.Green;
            }
            else if (radioButtonSingleArchive.Checked)
            {
                if (selectedFiles.Count != 1 || !selectedFiles[0].EndsWith(".cmp"))
                {
                    MessageBox.Show("Please select a single .cmp archive file to decompress.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string archivePath = selectedFiles[0];

                var filesInArchive = CompressionManager.ListArchive(archivePath);

                using (var selectFileForm = new SelectFileFromArchiveForm(filesInArchive))
                {
                    if (selectFileForm.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = selectFileForm.SelectedFileName;

                        labelStatus.Text = "Extracting file...";
                        labelStatus.ForeColor = Color.DarkBlue;
                        progressBar.Visible = true;

                        await Task.Run(() =>
                                CompressionManager.ExtractSingleFile(archivePath, selectedFile, outputDir: "",
                                    token: _cts.Token, pauseEvent),
                            _cts.Token);
                        progressBar.Visible = false;
                        labelStatus.Text = $"File '{selectedFile}' extracted successfully.";
                        labelStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        labelStatus.Text = "Decompression canceled.";
                        labelStatus.ForeColor = Color.Orange;
                    }
                }
            }
        }
    }
}