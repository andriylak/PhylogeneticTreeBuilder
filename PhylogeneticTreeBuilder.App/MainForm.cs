
namespace PhylogeneticTreeBuilder.App
{
    public class MainForm : Form
    {
        private ComboBox cmbAlgorithm;
        private TextBox txtCsvPath;
        private Button btnBrowse;
        private Button btnRun;
        private TextBox txtNewick;
        private Button btnCopy;
        private Button btnSave;
        private Label lblStatus;
        private CheckBox chkUnrooted; // Optional (for NJ display choices later)

        public MainForm()
        {
            Text = "Phylogenetic Tree Builder";
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // Controls
            var lblAlgorithm = new Label { Text = "Algorithm:", AutoSize = true, Top = 16, Left = 16 };
            cmbAlgorithm = new ComboBox
            {
                Top = lblAlgorithm.Top - 4,
                Left = 100,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAlgorithm.Items.AddRange(new object[] { "UPGMA", "Neighbor Joining" });
            cmbAlgorithm.SelectedIndex = 0;

            var lblCsv = new Label { Text = "Distance matrix (CSV):", AutoSize = true, Top = 52, Left = 16 };
            txtCsvPath = new TextBox
            {
                Top = lblCsv.Top - 4,
                Left = 160,
                Width = 450,
                ReadOnly = true
            };
            btnBrowse = new Button
            {
                Text = "Browse…",
                Top = txtCsvPath.Top - 2,
                Left = txtCsvPath.Right + 8,
                Width = 90
            };
            btnBrowse.Click += BtnBrowse_Click;

            btnRun = new Button
            {
                Text = "Build Tree",
                Top = 90,
                Left = 16,
                Width = 120,
                Height = 32
            };
            btnRun.Click += BtnRun_Click;

            chkUnrooted = new CheckBox
            {
                Text = "Show unrooted (NJ only)",
                Top = 96,
                Left = btnRun.Right + 16,
                AutoSize = true,
                Enabled = true // purely a placeholder – hook into your visualization later
            };
            cmbAlgorithm.SelectedIndexChanged += (_, __) =>
            {
                // Optional UI behavior: only relevant for NJ
                chkUnrooted.Enabled = (cmbAlgorithm.SelectedItem?.ToString() == "Neighbor Joining");
            };

            var lblNewick = new Label { Text = "Newick:", AutoSize = true, Top = 136, Left = 16 };
            txtNewick = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Top = lblNewick.Bottom + 4,
                Left = 16,
                Width = ClientSize.Width - 32,
                Height = ClientSize.Height - 220,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                Font = new Font(FontFamily.GenericMonospace, 10f)
            };

            btnCopy = new Button
            {
                Text = "Copy",
                Top = txtNewick.Bottom + 8,
                Left = 16,
                Width = 80,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnCopy.Click += (_, __) =>
            {
                if (!string.IsNullOrWhiteSpace(txtNewick.Text))
                    Clipboard.SetText(txtNewick.Text);
            };

            btnSave = new Button
            {
                Text = "Save…",
                Top = txtNewick.Bottom + 8,
                Left = btnCopy.Right + 8,
                Width = 80,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            btnSave.Click += BtnSave_Click;

            lblStatus = new Label
            {
                Text = "Ready.",
                Top = txtNewick.Bottom + 12,
                Left = btnSave.Right + 16,
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };

            Controls.AddRange(new Control[]
            {
                lblAlgorithm, cmbAlgorithm, lblCsv, txtCsvPath, btnBrowse, btnRun, chkUnrooted,
                lblNewick, txtNewick, btnCopy, btnSave, lblStatus
            });
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select distance matrix CSV",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtCsvPath.Text = ofd.FileName;
                lblStatus.Text = "File selected.";
                lblStatus.ForeColor = Color.DimGray;
            }
        }

        private void BtnRun_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";
            txtNewick.Clear();

            var algo = cmbAlgorithm.SelectedItem?.ToString();
            var path = txtCsvPath.Text;

            if (string.IsNullOrWhiteSpace(algo))
            {
                Fail("Please select an algorithm.");
                return;
            }
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Fail("Please choose a valid CSV file.");
                return;
            }

            try
            {
                // Construct your matrix (this runs validation and can throw InvalidDataException)
                DistanceMatrix matrix = new DistanceMatrix(path);

                // Pick engine
                TreeNode root;
                if (algo == "UPGMA")
                {
                    var upgma = new UPGMA();          // <-- change namespace if needed
                    root = upgma.BuildTree(matrix);
                }
                else
                {
                    var nj = new NeighbourJoining();
                    root = nj.BuildTree(matrix);
                }

                // Newick output — assumes your TreeNode has ToNewick() that omits root length
                var newick = root.ToNewick();
                if (!newick.EndsWith(";")) newick += ";";

                // Force dot decimals if you format inside ToNewick yourself; otherwise this is fine.
                txtNewick.Text = newick;
                Succeed("Tree built.");
            }
            catch (InvalidDataException ex)
            {
                Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Fail("Unexpected error: " + ex.Message);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewick.Text))
            {
                Fail("Newick is empty. Build a tree first.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Save Newick",
                Filter = "Newick (*.nwk;*.newick)|*.nwk;*.newick|All files (*.*)|*.*",
                FileName = "tree.nwk",
                AddExtension = true
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, txtNewick.Text);
                Succeed("Saved.");
            }
        }

        private void Succeed(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.DarkGreen;
        }

        private void Fail(string message)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.Firebrick;
        }
    }


}
