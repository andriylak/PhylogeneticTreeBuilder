using System.Windows.Forms;

namespace PhylogeneticTreeBuilder.App
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblNewick;
        private TextBox txtNewick;

        private FlowLayoutPanel flpBottom;
        private Button btnCopy;
        private Button btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblNewick = new Label();
            txtNewick = new TextBox();
            flpBottom = new FlowLayoutPanel();
            btnCopy = new Button();
            btnSave = new Button();
            btnRun = new Button();
            btnBrowse = new Button();
            txtCsvPath = new TextBox();
            lblCsv = new Label();
            cmbAlgorithm = new ComboBox();
            lblAlgorithm = new Label();
            tlpTop = new TableLayoutPanel();
            flpBottom.SuspendLayout();
            SuspendLayout();
            // 
            // lblNewick
            // 
            lblNewick.Location = new Point(-1, 114);
            lblNewick.Name = "lblNewick";
            lblNewick.Padding = new Padding(12, 8, 12, 0);
            lblNewick.Size = new Size(96, 31);
            lblNewick.TabIndex = 1;
            lblNewick.Text = "  Newick:";
            // 
            // txtNewick
            // 
            txtNewick.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtNewick.Font = new Font("Consolas", 10F);
            txtNewick.Location = new Point(12, 148);
            txtNewick.Multiline = true;
            txtNewick.Name = "txtNewick";
            txtNewick.ReadOnly = true;
            txtNewick.ScrollBars = ScrollBars.Both;
            txtNewick.Size = new Size(858, 336);
            txtNewick.TabIndex = 0;
            txtNewick.WordWrap = false;
            // 
            // flpBottom
            // 
            flpBottom.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpBottom.Controls.Add(btnCopy);
            flpBottom.Controls.Add(btnSave);
            flpBottom.Dock = DockStyle.Bottom;
            flpBottom.Location = new Point(0, 493);
            flpBottom.Name = "flpBottom";
            flpBottom.Padding = new Padding(12);
            flpBottom.Size = new Size(882, 60);
            flpBottom.TabIndex = 2;
            // 
            // btnCopy
            // 
            btnCopy.Location = new Point(15, 15);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(75, 30);
            btnCopy.TabIndex = 0;
            btnCopy.Text = "Copy";
            btnCopy.Click += BtnCopy_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(96, 15);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 30);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save…";
            btnSave.Click += BtnSave_Click;
            // 
            // btnRun
            // 
            btnRun.Anchor = AnchorStyles.Left;
            btnRun.Location = new Point(176, 77);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(85, 27);
            btnRun.TabIndex = 8;
            btnRun.Text = "Build Tree";
            btnRun.Click += BtnRun_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Left;
            btnBrowse.Location = new Point(721, 77);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(149, 27);
            btnBrowse.TabIndex = 7;
            btnBrowse.Text = "Browse…";
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // txtCsvPath
            // 
            txtCsvPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtCsvPath.Location = new Point(176, 43);
            txtCsvPath.Name = "txtCsvPath";
            txtCsvPath.ReadOnly = true;
            txtCsvPath.Size = new Size(694, 27);
            txtCsvPath.TabIndex = 6;
            // 
            // lblCsv
            // 
            lblCsv.Anchor = AnchorStyles.Left;
            lblCsv.Location = new Point(16, 50);
            lblCsv.Name = "lblCsv";
            lblCsv.Size = new Size(155, 20);
            lblCsv.TabIndex = 5;
            lblCsv.Text = "Distance matrix (CSV):";
            // 
            // cmbAlgorithm
            // 
            cmbAlgorithm.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAlgorithm.Items.AddRange(new object[] { "UPGMA", "Neighbor Joining" });
            cmbAlgorithm.Location = new Point(176, 9);
            cmbAlgorithm.Name = "cmbAlgorithm";
            cmbAlgorithm.Size = new Size(694, 28);
            cmbAlgorithm.TabIndex = 1;
            // 
            // lblAlgorithm
            // 
            lblAlgorithm.Anchor = AnchorStyles.Left;
            lblAlgorithm.Location = new Point(16, 12);
            lblAlgorithm.Name = "lblAlgorithm";
            lblAlgorithm.Size = new Size(79, 20);
            lblAlgorithm.TabIndex = 0;
            lblAlgorithm.Text = "Algorithm:";
            // 
            // tlpTop
            // 
            tlpTop.Location = new Point(0, 0);
            tlpTop.Name = "tlpTop";
            tlpTop.Size = new Size(200, 100);
            tlpTop.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 553);
            Controls.Add(txtNewick);
            Controls.Add(lblNewick);
            Controls.Add(flpBottom);
            Controls.Add(lblAlgorithm);
            Controls.Add(cmbAlgorithm);
            Controls.Add(lblCsv);
            Controls.Add(txtCsvPath);
            Controls.Add(btnBrowse);
            Controls.Add(btnRun);
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Phylogenetic Tree Builder";
            flpBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
        private Button btnRun;
        private Button btnBrowse;
        private TextBox txtCsvPath;
        private Label lblCsv;
        private ComboBox cmbAlgorithm;
        private Label lblAlgorithm;
        private TableLayoutPanel tlpTop;
    }
}
