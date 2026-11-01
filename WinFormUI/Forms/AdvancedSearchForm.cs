using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QueryEngine;

namespace WinFormUI.Forms
{
    public class AdvancedSearchForm : Form
    {
        private TextBox txtAllWords;
        private TextBox txtExactPhrase;
        private TextBox txtAnyWords;
        private TextBox txtNoneWords;
        private TextBox txtFolder;
        private Button btnBrowseFolder;
        private ComboBox cmbFilter;
        private CheckBox chkMinSize;
        private NumericUpDown numMinSize;
        private ComboBox cmbMinSizeUnit;
        private CheckBox chkMaxSize;
        private NumericUpDown numMaxSize;
        private ComboBox cmbMaxSizeUnit;
        private CheckBox chkDateFrom;
        private DateTimePicker dtpFrom;
        private CheckBox chkDateTo;
        private DateTimePicker dtpTo;
        private Button btnSearch;
        private Button btnCancel;

        public string GeneratedQuery { get; private set; }
        public FilterCategory SelectedFilter { get; private set; }

        public AdvancedSearchForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Advanced Search";
            this.Size = new Size(540, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int leftCol = 20;
            int inputCol = 160;
            int inputWidth = 330;
            int y = 20;
            int step = 32;

            // All words
            Label lblAll = new Label { Text = "All these words:", Location = new Point(leftCol, y + 3), AutoSize = true };
            txtAllWords = new TextBox { Location = new Point(inputCol, y), Width = inputWidth };
            this.Controls.Add(lblAll);
            this.Controls.Add(txtAllWords);
            y += step;

            // Exact phrase
            Label lblExact = new Label { Text = "This exact phrase:", Location = new Point(leftCol, y + 3), AutoSize = true };
            txtExactPhrase = new TextBox { Location = new Point(inputCol, y), Width = inputWidth };
            this.Controls.Add(lblExact);
            this.Controls.Add(txtExactPhrase);
            y += step;

            // Any words
            Label lblAny = new Label { Text = "Any of these words:", Location = new Point(leftCol, y + 3), AutoSize = true };
            txtAnyWords = new TextBox { Location = new Point(inputCol, y), Width = inputWidth };
            this.Controls.Add(lblAny);
            this.Controls.Add(txtAnyWords);
            y += step;

            // None of these words
            Label lblNone = new Label { Text = "None of these words:", Location = new Point(leftCol, y + 3), AutoSize = true };
            txtNoneWords = new TextBox { Location = new Point(inputCol, y), Width = inputWidth };
            this.Controls.Add(lblNone);
            this.Controls.Add(txtNoneWords);
            y += step + 8;

            // In folder
            Label lblFolder = new Label { Text = "In folder:", Location = new Point(leftCol, y + 3), AutoSize = true };
            txtFolder = new TextBox { Location = new Point(inputCol, y), Width = inputWidth - 75 };
            btnBrowseFolder = new Button { Text = "Browse...", Location = new Point(inputCol + inputWidth - 70, y - 1), Width = 70, Height = 25 };
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            this.Controls.Add(lblFolder);
            this.Controls.Add(txtFolder);
            this.Controls.Add(btnBrowseFolder);
            y += step;

            // Filter Category
            Label lblFilter = new Label { Text = "What to match:", Location = new Point(leftCol, y + 3), AutoSize = true };
            cmbFilter = new ComboBox { Location = new Point(inputCol, y), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var f in FilterCategory.GetDefaultFilters())
            {
                cmbFilter.Items.Add(f.Name);
            }
            cmbFilter.SelectedIndex = 0;
            this.Controls.Add(lblFilter);
            this.Controls.Add(cmbFilter);
            y += step + 8;

            // Size Min
            chkMinSize = new CheckBox { Text = "At least:", Location = new Point(leftCol, y + 3), AutoSize = true };
            numMinSize = new NumericUpDown { Location = new Point(inputCol, y), Width = 100, Maximum = 1000000, Enabled = false };
            cmbMinSizeUnit = new ComboBox { Location = new Point(inputCol + 110, y), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbMinSizeUnit.Items.AddRange(new object[] { "KB", "MB", "GB" });
            cmbMinSizeUnit.SelectedIndex = 1;
            chkMinSize.CheckedChanged += (s, e) => { numMinSize.Enabled = chkMinSize.Checked; cmbMinSizeUnit.Enabled = chkMinSize.Checked; };
            this.Controls.Add(chkMinSize);
            this.Controls.Add(numMinSize);
            this.Controls.Add(cmbMinSizeUnit);
            y += step;

            // Size Max
            chkMaxSize = new CheckBox { Text = "At most:", Location = new Point(leftCol, y + 3), AutoSize = true };
            numMaxSize = new NumericUpDown { Location = new Point(inputCol, y), Width = 100, Maximum = 1000000, Enabled = false };
            cmbMaxSizeUnit = new ComboBox { Location = new Point(inputCol + 110, y), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbMaxSizeUnit.Items.AddRange(new object[] { "KB", "MB", "GB" });
            cmbMaxSizeUnit.SelectedIndex = 1;
            chkMaxSize.CheckedChanged += (s, e) => { numMaxSize.Enabled = chkMaxSize.Checked; cmbMaxSizeUnit.Enabled = chkMaxSize.Checked; };
            this.Controls.Add(chkMaxSize);
            this.Controls.Add(numMaxSize);
            this.Controls.Add(cmbMaxSizeUnit);
            y += step + 8;

            // Date Modified From
            chkDateFrom = new CheckBox { Text = "Date from:", Location = new Point(leftCol, y + 3), AutoSize = true };
            dtpFrom = new DateTimePicker { Location = new Point(inputCol, y), Width = 180, Enabled = false, Format = DateTimePickerFormat.Short };
            chkDateFrom.CheckedChanged += (s, e) => dtpFrom.Enabled = chkDateFrom.Checked;
            this.Controls.Add(chkDateFrom);
            this.Controls.Add(dtpFrom);
            y += step;

            // Date Modified To
            chkDateTo = new CheckBox { Text = "Date to:", Location = new Point(leftCol, y + 3), AutoSize = true };
            dtpTo = new DateTimePicker { Location = new Point(inputCol, y), Width = 180, Enabled = false, Format = DateTimePickerFormat.Short };
            chkDateTo.CheckedChanged += (s, e) => dtpTo.Enabled = chkDateTo.Checked;
            this.Controls.Add(chkDateTo);
            this.Controls.Add(dtpTo);
            y += step + 16;

            // Buttons
            btnSearch = new Button { Text = "Search", Location = new Point(inputCol + inputWidth - 170, y), Width = 80, Height = 28, DialogResult = DialogResult.OK };
            btnSearch.Click += BtnSearch_Click;
            btnCancel = new Button { Text = "Cancel", Location = new Point(inputCol + inputWidth - 80, y), Width = 80, Height = 28, DialogResult = DialogResult.Cancel };
            this.Controls.Add(btnSearch);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSearch;
            this.CancelButton = btnCancel;
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            // All words
            if (!string.IsNullOrWhiteSpace(txtAllWords.Text))
            {
                sb.Append(txtAllWords.Text.Trim()).Append(" ");
            }

            // Exact phrase
            if (!string.IsNullOrWhiteSpace(txtExactPhrase.Text))
            {
                sb.Append("\"").Append(txtExactPhrase.Text.Trim()).Append("\" ");
            }

            // Any words
            if (!string.IsNullOrWhiteSpace(txtAnyWords.Text))
            {
                var anyTokens = txtAnyWords.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (anyTokens.Length > 0)
                {
                    sb.Append("<").Append(string.Join(" | ", anyTokens)).Append("> ");
                }
            }

            // None words
            if (!string.IsNullOrWhiteSpace(txtNoneWords.Text))
            {
                var noneTokens = txtNoneWords.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tok in noneTokens)
                {
                    sb.Append("!").Append(tok).Append(" ");
                }
            }

            // In folder
            if (!string.IsNullOrWhiteSpace(txtFolder.Text))
            {
                string path = txtFolder.Text.Trim();
                if (path.Contains(" "))
                {
                    sb.Append("parent:\"").Append(path).Append("\" ");
                }
                else
                {
                    sb.Append("parent:").Append(path).Append(" ");
                }
            }

            // Min Size
            if (chkMinSize.Checked && numMinSize.Value > 0)
            {
                sb.Append(string.Format("size:>={0}{1} ", (long)numMinSize.Value, cmbMinSizeUnit.SelectedItem));
            }

            // Max Size
            if (chkMaxSize.Checked && numMaxSize.Value > 0)
            {
                sb.Append(string.Format("size:<={0}{1} ", (long)numMaxSize.Value, cmbMaxSizeUnit.SelectedItem));
            }

            // Date Range
            if (chkDateFrom.Checked && chkDateTo.Checked)
            {
                sb.Append(string.Format("dm:{0:yyyy-MM-dd}..{1:yyyy-MM-dd} ", dtpFrom.Value, dtpTo.Value));
            }
            else if (chkDateFrom.Checked)
            {
                sb.Append(string.Format("dm:>={0:yyyy-MM-dd} ", dtpFrom.Value));
            }
            else if (chkDateTo.Checked)
            {
                sb.Append(string.Format("dm:<={0:yyyy-MM-dd} ", dtpTo.Value));
            }

            int selectedFilterIndex = cmbFilter.SelectedIndex;
            var filters = FilterCategory.GetDefaultFilters();
            if (selectedFilterIndex >= 0 && selectedFilterIndex < filters.Count)
            {
                this.SelectedFilter = filters[selectedFilterIndex];
            }

            this.GeneratedQuery = sb.ToString().Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
