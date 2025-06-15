using WinFormsApp1.Compression;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                // نوقف العملية مؤقتاً
                pauseEvent.Reset();
                btnPauseResume.Text = "Resume";
                isPaused = true;
                labelStatus.Text = "Paused";
                labelStatus.ForeColor = Color.Orange;
            }
            else
            {
                // نستأنف العملية
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

            // ⇢⇢⇢  تفعيل أزرار الإيقاف المؤقت والإلغاء  ⇠⇠⇠
            btnPauseResume.Enabled = true;
            btnPauseResume.Text = "Pause";
            isPaused = false;
            pauseEvent.Set(); // ابدأ بحالة “تشغيل”
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

                    // ⇢ تعطيل الأزرار لأن العملية ألغيت قبل البدء
                    btnPauseResume.Enabled = false;
                    btnCancel.Enabled = false;
                    return;
                }

                string outputFolder = folderDialog.SelectedPath;

                var ratios = await Task.Run(() =>
                    CompressionManager.CompressFiles(selectedFiles,
                        algo,
                        outputFolder,
                        _cts.Token,
                        pauseEvent)); // لاحِظ تمرير pauseEvent

                progressBar.Visible = false;
                btnPauseResume.Enabled = false; // ⇢ إيقاف الزرَّين بعد الانتهاء
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
                        CompressionManager.CreateArchive(selectedFiles,
                            archivePath,
                            algo,
                            _cts.Token,
                            pauseEvent)); // تمرير pauseEvent

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
                    btnPauseResume.Enabled = false; // ⇢ إيقاف الأزرار بعد نهاية الفرع
                    btnCancel.Enabled = false;
                }
            }
        }


        private async void btnDecompress_Click(object sender, EventArgs e)
        {
            string algo = comboBoxAlgorithm.SelectedItem?.ToString() ?? "Huffman";

            if (radioButtonIndividualFiles.Checked)
            {
                // فك الضغط القديم: ملفات .cmp منفردة
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

                await Task.Run(() => CompressionManager.DecompressFiles(cmpFiles, algo, _cts.Token));

                progressBar.Visible = false;

                labelStatus.Text = "Decompression completed successfully.";
                labelStatus.ForeColor = Color.Green;
            }
            else if (radioButtonSingleArchive.Checked)
            {
                // فك ضغط أرشيف واحد: نطلب من المستخدم اختيار ملف الأرشيف أولاً
                if (selectedFiles.Count != 1 || !selectedFiles[0].EndsWith(".cmp"))
                {
                    MessageBox.Show("Please select a single .cmp archive file to decompress.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string archivePath = selectedFiles[0];

                // عرض قائمة الملفات داخل الأرشيف للمستخدم ليختار أي ملف يستخرج
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
                                CompressionManager.ExtractSingleFile(archivePath, selectedFile, algo, outputDir: "",
                                    token: _cts.Token),
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