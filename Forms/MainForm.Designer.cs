namespace WinFormsApp1.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnCompress;
        private System.Windows.Forms.Button btnDecompress;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.ComboBox comboBoxAlgorithm;
        private System.Windows.Forms.Label labelStatus;
        private ProgressBar progressBar;
        private Button btnSelectFolder;
        private CancellationTokenSource _cts;
        private Button btnCancel;          // زر الإلغاء
        private Button btnPauseResume;
        private bool isPaused = false;
        private ManualResetEventSlim pauseEvent = new(true);

           private void InitializeComponent()
            {
                this.SuspendLayout();
                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(400, 600),
                    Size = new Size(120, 50),
                    BackColor = Color.FromArgb(219, 84, 97),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = false
                };
                // btnCancel.Click += btnCancel_Click;
                Controls.Add(btnCancel);
                
                btnPauseResume = new Button
                {
                    Text = "Pause",
                    Location = new Point(550, 600),
                    Size = new Size(120, 50),
                    BackColor = Color.FromArgb(100, 149, 237),  // CornflowerBlue
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = false
                };
                btnPauseResume.Click += BtnPauseResume_Click;
                Controls.Add(btnPauseResume);
                // Form settings
                this.ClientSize = new System.Drawing.Size(1080, 780);
                this.Text = "File Compression Tool";
                this.BackColor = System.Drawing.Color.FromArgb(34, 40, 49);
                this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.AllowDrop = true;
                this.DragEnter += MainForm_DragEnter;
                this.DragDrop += MainForm_DragDrop;

                // Header Label
                var lblHeader = new Label();
                lblHeader.Text = "File Compression Tool";
                lblHeader.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
                lblHeader.ForeColor = System.Drawing.Color.FromArgb(0, 173, 181);
                lblHeader.Location = new System.Drawing.Point(30, 20);
                lblHeader.Size = new System.Drawing.Size(400, 50);
                this.Controls.Add(lblHeader);

                // Browse Button
                btnBrowse = new Button();
                btnBrowse.Text = "Browse Files...";
                btnBrowse.Location = new System.Drawing.Point(30, 90);
                btnBrowse.Size = new System.Drawing.Size(140, 45);
                btnBrowse.FlatStyle = FlatStyle.Flat;
                btnBrowse.ForeColor = Color.White;
                btnBrowse.BackColor = Color.FromArgb(0, 173, 181);
                btnBrowse.FlatAppearance.BorderSize = 0;
                btnBrowse.Cursor = Cursors.Hand;
                btnBrowse.Click += btnBrowse_Click;
                this.Controls.Add(btnBrowse);

                // Select Folder Button (زر اختيار المجلد)
                btnSelectFolder = new Button();
                btnSelectFolder.Text = "Select Folder...";
                btnSelectFolder.Location = new System.Drawing.Point(190, 90);
                btnSelectFolder.Size = new System.Drawing.Size(140, 45);
                btnSelectFolder.FlatStyle = FlatStyle.Flat;
                btnSelectFolder.ForeColor = Color.White;
                btnSelectFolder.BackColor = Color.FromArgb(0, 173, 181);
                btnSelectFolder.FlatAppearance.BorderSize = 0;
                btnSelectFolder.Cursor = Cursors.Hand;
                // btnSelectFolder.Click += btnSelectFolder_Click;
                this.Controls.Add(btnSelectFolder);

                // Algorithm ComboBox with Label
                var lblAlgo = new Label();
                lblAlgo.Text = "Algorithm:";
                lblAlgo.ForeColor = Color.White;
                lblAlgo.Location = new System.Drawing.Point(350, 100);
                lblAlgo.Size = new System.Drawing.Size(100, 25);
                this.Controls.Add(lblAlgo);

                comboBoxAlgorithm = new ComboBox();
                comboBoxAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBoxAlgorithm.Items.AddRange(new string[] { "Huffman", "Shannon-Fano" });
                comboBoxAlgorithm.SelectedIndex = 0;
                comboBoxAlgorithm.Location = new System.Drawing.Point(450, 95);
                comboBoxAlgorithm.Size = new System.Drawing.Size(220, 30);
                comboBoxAlgorithm.BackColor = Color.FromArgb(57, 62, 70);
                comboBoxAlgorithm.ForeColor = Color.White;
                comboBoxAlgorithm.FlatStyle = FlatStyle.Flat;
                this.Controls.Add(comboBoxAlgorithm);

                // Files ListBox with Label
                var lblFiles = new Label();
                lblFiles.Text = "Selected Files:";
                lblFiles.ForeColor = Color.White;
                lblFiles.Location = new System.Drawing.Point(30, 150);
                lblFiles.Size = new System.Drawing.Size(150, 25);
                this.Controls.Add(lblFiles);

                listBoxFiles = new ListBox();
                listBoxFiles.Location = new System.Drawing.Point(30, 180);
                listBoxFiles.Size = new System.Drawing.Size(1020, 400);
                listBoxFiles.SelectionMode = SelectionMode.MultiExtended;
                listBoxFiles.BackColor = Color.FromArgb(57, 62, 70);
                listBoxFiles.ForeColor = Color.White;
                listBoxFiles.HorizontalScrollbar = true;
                this.Controls.Add(listBoxFiles);

                // Compress Button
                btnCompress = new Button();
                btnCompress.Text = "Compress";
                btnCompress.Location = new System.Drawing.Point(30, 600);
                btnCompress.Size = new System.Drawing.Size(160, 50);
                btnCompress.FlatStyle = FlatStyle.Flat;
                btnCompress.ForeColor = Color.White;
                btnCompress.BackColor = Color.FromArgb(0, 173, 181);
                btnCompress.FlatAppearance.BorderSize = 0;
                btnCompress.Cursor = Cursors.Hand;
                btnCompress.Click += btnCompress_Click;
                this.Controls.Add(btnCompress);

                // Decompress Button
                btnDecompress = new Button();
                btnDecompress.Text = "Decompress";
                btnDecompress.Location = new System.Drawing.Point(220, 600);
                btnDecompress.Size = new System.Drawing.Size(160, 50);
                btnDecompress.FlatStyle = FlatStyle.Flat;
                btnDecompress.ForeColor = Color.White;
                btnDecompress.BackColor = Color.FromArgb(0, 173, 181);
                btnDecompress.FlatAppearance.BorderSize = 0;
                btnDecompress.Cursor = Cursors.Hand;
                btnDecompress.Click += btnDecompress_Click;
                this.Controls.Add(btnDecompress);

                // Radio Buttons for compress mode
                radioButtonIndividualFiles = new RadioButton();
                radioButtonIndividualFiles.Text = "Compress files individually";
                radioButtonIndividualFiles.Location = new System.Drawing.Point(700, 45);
                radioButtonIndividualFiles.ForeColor = Color.White;
                radioButtonIndividualFiles.AutoSize = true;
                radioButtonIndividualFiles.TabIndex = 0;
                radioButtonIndividualFiles.Checked = true;
                this.Controls.Add(radioButtonIndividualFiles);

        radioButtonSingleArchive = new RadioButton();
        radioButtonSingleArchive.Text = "Compress into a single archive";
        radioButtonSingleArchive.Location = new System.Drawing.Point(700, 90);
        radioButtonSingleArchive.ForeColor = Color.White;
        radioButtonSingleArchive.AutoSize = true;
        radioButtonSingleArchive.TabIndex = 1;
        this.Controls.Add(radioButtonSingleArchive);

        // ProgressBar
        progressBar = new ProgressBar();
        progressBar.Location = new System.Drawing.Point(30, 670);
        progressBar.Size = new System.Drawing.Size(1020, 25);
        progressBar.Style = ProgressBarStyle.Marquee;
        this.Controls.Add(progressBar);
        progressBar.Visible = false;


    // Status Label
     labelStatus = new Label();
     labelStatus.Text = "Ready";
     labelStatus.ForeColor = Color.LightGray;
     labelStatus.Location = new System.Drawing.Point(30, 700);
     labelStatus.Size = new System.Drawing.Size(1020, 25);
     this.Controls.Add(labelStatus);

     this.ResumeLayout(false);
     this.PerformLayout();
        }


    
    }
}





