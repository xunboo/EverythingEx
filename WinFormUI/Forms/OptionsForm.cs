using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinFormUI.Core;

namespace WinFormUI.Forms
{
    public class OptionsForm : Form
    {
        private TabControl tabControl;
        private CheckBox chkMinimizeToTray;
        private CheckBox chkShowTrayIcon;
        private CheckBox chkRealTimeMonitoring;
        private CheckBox chkShowFullPathInTitle;
        private NumericUpDown numFontSize;
        private CheckedListBox clbDrives;
        private Button btnOk;
        private Button btnCancel;

        public OptionsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Everything Options";
            this.Size = new Size(460, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            tabControl = new TabControl
            {
                Location = new Point(12, 12),
                Size = new Size(420, 275)
            };

            // General tab
            TabPage tabGeneral = new TabPage("General");
            chkShowTrayIcon = new CheckBox { Text = "Show Everything tray icon", Location = new Point(20, 20), AutoSize = true };
            chkMinimizeToTray = new CheckBox { Text = "Minimize to system tray when minimized", Location = new Point(20, 50), AutoSize = true };
            chkShowFullPathInTitle = new CheckBox { Text = "Show selected item full path in window title", Location = new Point(20, 80), AutoSize = true };

            tabGeneral.Controls.Add(chkShowTrayIcon);
            tabGeneral.Controls.Add(chkMinimizeToTray);
            tabGeneral.Controls.Add(chkShowFullPathInTitle);

            // Indexes tab
            TabPage tabIndexes = new TabPage("Indexes");
            chkRealTimeMonitoring = new CheckBox { Text = "Enable real-time file system monitoring (auto-update)", Location = new Point(20, 15), AutoSize = true };
            Label lblDrives = new Label { Text = "Available Drives to Index:", Location = new Point(20, 42), AutoSize = true };
            clbDrives = new CheckedListBox { Location = new Point(20, 60), Size = new Size(370, 110) };

            Label lblDb = new Label { Text = "Index Database file (Everything.db):", Location = new Point(20, 180), AutoSize = true };
            TextBox txtDbPath = new TextBox { Text = QueryEngine.IndexDatabase.DefaultDatabasePath, Location = new Point(20, 202), Width = 280, ReadOnly = true };
            Button btnOpenDb = new Button { Text = "Open Folder", Location = new Point(305, 200), Width = 85, Height = 25 };
            btnOpenDb.Click += (s, e) =>
            {
                string dir = Path.GetDirectoryName(QueryEngine.IndexDatabase.DefaultDatabasePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", dir);
            };

            tabIndexes.Controls.Add(chkRealTimeMonitoring);
            tabIndexes.Controls.Add(lblDrives);
            tabIndexes.Controls.Add(clbDrives);
            tabIndexes.Controls.Add(lblDb);
            tabIndexes.Controls.Add(txtDbPath);
            tabIndexes.Controls.Add(btnOpenDb);

            // View tab
            TabPage tabView = new TabPage("View");
            Label lblFont = new Label { Text = "Result List Font Size (pt):", Location = new Point(20, 25), AutoSize = true };
            numFontSize = new NumericUpDown { Location = new Point(180, 22), Width = 70, Minimum = 7, Maximum = 24, DecimalPlaces = 1 };

            tabView.Controls.Add(lblFont);
            tabView.Controls.Add(numFontSize);

            tabControl.TabPages.Add(tabGeneral);
            tabControl.TabPages.Add(tabIndexes);
            tabControl.TabPages.Add(tabView);

            btnOk = new Button { Text = "OK", Location = new Point(265, 300), Width = 80, Height = 28, DialogResult = DialogResult.OK };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button { Text = "Cancel", Location = new Point(352, 300), Width = 80, Height = 28, DialogResult = DialogResult.Cancel };

            this.Controls.Add(tabControl);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void LoadCurrentSettings()
        {
            var s = AppSettings.Instance;
            chkShowTrayIcon.Checked = s.ShowTrayIcon;
            chkMinimizeToTray.Checked = s.MinimizeToTray;
            chkShowFullPathInTitle.Checked = s.ShowFullPathInTitle;
            chkRealTimeMonitoring.Checked = s.RealTimeMonitoring;
            numFontSize.Value = (decimal)s.FontSize;

            clbDrives.Items.Clear();
            foreach (var d in DriveInfo.GetDrives())
            {
                if (d.IsReady)
                {
                    string info = string.Format("{0} ({1}, {2})", d.Name, d.DriveFormat, d.DriveType);
                    clbDrives.Items.Add(info, true);
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var s = AppSettings.Instance;
            s.ShowTrayIcon = chkShowTrayIcon.Checked;
            s.MinimizeToTray = chkMinimizeToTray.Checked;
            s.ShowFullPathInTitle = chkShowFullPathInTitle.Checked;
            s.RealTimeMonitoring = chkRealTimeMonitoring.Checked;
            s.FontSize = (float)numFontSize.Value;
            AppSettings.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
