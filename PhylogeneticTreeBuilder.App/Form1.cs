// TODO: point this to your core library namespace:
namespace PhylogeneticTreeBuilder.App
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent(); // generated in the Designer file
            cmbAlgorithm.SelectedIndex = 0;    // default to UPGMA
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
            }
        }

        private void BtnRun_Click(object? sender, EventArgs e)
        {
            txtNewick.Clear();

            var algo = cmbAlgorithm.SelectedItem?.ToString();
            var path = txtCsvPath.Text;

            if (string.IsNullOrWhiteSpace(algo))
            {
                MessageBox.Show(this, "Please select an algorithm.", "Wrong Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show(this, "Please choose a valid CSV file.", "Wrong Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var matrix = new DistanceMatrix(path);

                TreeNode root;
                if (string.Equals(algo, "UPGMA", StringComparison.Ordinal))
                {
                    var upgma = new UPGMA();
                    root = upgma.BuildTree(matrix);
                }
                else
                {
                    var nj = new NeighbourJoining();
                    root = nj.BuildTree(matrix);
                }

                // Your TreeNode should already format using InvariantCulture internally.
                var newick = root.ToNewick();
                if (!newick.EndsWith(";")) newick += ";";

                txtNewick.Text = newick;
                MessageBox.Show(this, "Tree built.", "Succeed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidDataException ex) // your validation errors
            {
                MessageBox.Show(this,
                    ex.Message,
                    "Invalid data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                     ex.ToString(),
                     "Unexpected error",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
            }
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewick.Text))
            {
                Clipboard.SetText(txtNewick.Text);
                MessageBox.Show(this, "Newick copied to clipboard.", "Succeed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "Nothing to copy. Build a tree first.", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewick.Text))
            {
                MessageBox.Show(this,
                    "Newick is empty. Build a tree first.",
                     "Fail",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
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
            }
        }

    }
}
