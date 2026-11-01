using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormUI.Core;

namespace WinFormUI.Forms
{
    public class BookmarksForm : Form
    {
        private ListView lvBookmarks;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnClose;

        public BookmarkItem SelectedBookmark { get; private set; }

        public BookmarksForm()
        {
            InitializeComponent();
            LoadBookmarks();
        }

        private void InitializeComponent()
        {
            this.Text = "Organize Bookmarks";
            this.Size = new Size(540, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lvBookmarks = new ListView
            {
                Location = new Point(12, 12),
                Size = new Size(390, 290),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvBookmarks.Columns.Add("Name", 130);
            lvBookmarks.Columns.Add("Search Query", 160);
            lvBookmarks.Columns.Add("Filter", 90);
            lvBookmarks.DoubleClick += LvBookmarks_DoubleClick;

            btnAdd = new Button { Text = "New...", Location = new Point(415, 12), Width = 95, Height = 28 };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "Edit...", Location = new Point(415, 48), Width = 95, Height = 28 };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(415, 84), Width = 95, Height = 28 };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button { Text = "Close", Location = new Point(415, 274), Width = 95, Height = 28, DialogResult = DialogResult.OK };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(lvBookmarks);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnClose);
            this.AcceptButton = btnClose;
        }

        private void LoadBookmarks()
        {
            lvBookmarks.Items.Clear();
            foreach (var b in BookmarkManager.Bookmarks)
            {
                var lvi = new ListViewItem(b.Name);
                lvi.SubItems.Add(b.Query);
                lvi.SubItems.Add(b.Filter);
                lvi.Tag = b;
                lvBookmarks.Items.Add(lvi);
            }
        }

        private void LvBookmarks_DoubleClick(object sender, EventArgs e)
        {
            if (lvBookmarks.SelectedItems.Count > 0)
            {
                this.SelectedBookmark = lvBookmarks.SelectedItems[0].Tag as BookmarkItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dlg = new BookmarkEditForm())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    BookmarkManager.Bookmarks.Add(dlg.Bookmark);
                    BookmarkManager.Save();
                    LoadBookmarks();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lvBookmarks.SelectedItems.Count > 0)
            {
                var item = lvBookmarks.SelectedItems[0].Tag as BookmarkItem;
                using (var dlg = new BookmarkEditForm(item))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        BookmarkManager.Save();
                        LoadBookmarks();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvBookmarks.SelectedItems.Count > 0)
            {
                var item = lvBookmarks.SelectedItems[0].Tag as BookmarkItem;
                if (MessageBox.Show("Delete bookmark '" + item.Name + "'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    BookmarkManager.Bookmarks.Remove(item);
                    BookmarkManager.Save();
                    LoadBookmarks();
                }
            }
        }
    }

    public class BookmarkEditForm : Form
    {
        private TextBox txtName;
        private TextBox txtQuery;
        private TextBox txtFilter;
        private Button btnOk;
        private Button btnCancel;

        public BookmarkItem Bookmark { get; private set; }

        public BookmarkEditForm(BookmarkItem existing = null)
        {
            this.Bookmark = existing ?? new BookmarkItem();

            this.Text = existing == null ? "Add Bookmark" : "Edit Bookmark";
            this.Size = new Size(380, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblName = new Label { Text = "Name:", Location = new Point(15, 18), AutoSize = true };
            txtName = new TextBox { Location = new Point(80, 15), Width = 265, Text = this.Bookmark.Name };

            Label lblQuery = new Label { Text = "Query:", Location = new Point(15, 53), AutoSize = true };
            txtQuery = new TextBox { Location = new Point(80, 50), Width = 265, Text = this.Bookmark.Query };

            Label lblFilter = new Label { Text = "Filter:", Location = new Point(15, 88), AutoSize = true };
            txtFilter = new TextBox { Location = new Point(80, 85), Width = 265, Text = string.IsNullOrEmpty(this.Bookmark.Filter) ? "Everything" : this.Bookmark.Filter };

            btnOk = new Button { Text = "OK", Location = new Point(185, 125), Width = 75, Height = 25, DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) =>
            {
                this.Bookmark.Name = txtName.Text.Trim();
                this.Bookmark.Query = txtQuery.Text.Trim();
                this.Bookmark.Filter = txtFilter.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel = new Button { Text = "Cancel", Location = new Point(270, 125), Width = 75, Height = 25, DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblQuery);
            this.Controls.Add(txtQuery);
            this.Controls.Add(lblFilter);
            this.Controls.Add(txtFilter);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
