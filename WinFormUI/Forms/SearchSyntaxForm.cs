using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormUI.Forms
{
    public class SearchSyntaxForm : Form
    {
        private ListView lvSyntax;
        private TextBox txtSearch;
        private Label lblSearch;

        private static readonly string[][] SyntaxData = new string[][]
        {
            new string[] { "space", "AND operator: file1 file2 matches both terms", "foo bar" },
            new string[] { "|", "OR operator: file1 | file2 matches either term", "*.jpg | *.png" },
            new string[] { "!", "NOT operator: !file excludes terms", "!*.tmp" },
            new string[] { "< >", "Group operators together", "<foo | bar> baz" },
            new string[] { "\" \"", "Match exact phrase including spaces", "\"Program Files\"" },
            new string[] { "*", "Wildcard: matches zero or more characters", "*.docx" },
            new string[] { "?", "Wildcard: matches exactly one character", "file?.txt" },
            new string[] { "case:", "Match case", "case:Makefile" },
            new string[] { "nocase:", "Do not match case", "nocase:readme" },
            new string[] { "path:", "Match against full path and name", "path:system32" },
            new string[] { "nopath:", "Match against filename only", "nopath:cmd.exe" },
            new string[] { "ww:", "Match whole word only", "ww:word" },
            new string[] { "regex:", "Enable regular expressions", "regex:^[0-9]+\\.txt" },
            new string[] { "ext:<list>", "Match specific file extension(s)", "ext:mp4;mkv" },
            new string[] { "size:<range>", "Match file size (kb, mb, gb, or > < = ..)", "size:>100mb" },
            new string[] { "size:empty", "Match empty files (0 bytes)", "size:empty" },
            new string[] { "size:tiny", "Match files between 0 KB and 10 KB", "size:tiny" },
            new string[] { "size:small", "Match files between 10 KB and 100 KB", "size:small" },
            new string[] { "size:medium", "Match files between 100 KB and 1 MB", "size:medium" },
            new string[] { "size:large", "Match files between 1 MB and 16 MB", "size:large" },
            new string[] { "size:huge", "Match files between 16 MB and 128 MB", "size:huge" },
            new string[] { "size:gigantic", "Match files greater than 128 MB", "size:gigantic" },
            new string[] { "dm:<date>", "Date modified (today, yesterday, thisweek, dates)", "dm:today" },
            new string[] { "dc:<date>", "Date created", "dc:thisyear" },
            new string[] { "file:", "Match files only", "file:notes" },
            new string[] { "folder:", "Match folders only", "folder:projects" },
            new string[] { "attrib:<d,h,r,s,a>", "Match file attributes (Directory, Hidden, ReadOnly, System, Archive)", "attrib:h" },
            new string[] { "empty:", "Match empty files or folders", "empty:" },
            new string[] { "root:", "Match items in the drive root", "root:" },
            new string[] { "parent:<path>", "Match parent folder path", "parent:windows" },
            new string[] { "startwith:<text>", "Match filename starting with text", "startwith:log_" },
            new string[] { "endwith:<text>", "Match filename ending with text", "endwith:.bak" },
            new string[] { "count:<number>", "Limit maximum search results count", "count:100" },
            new string[] { "audio:", "Filter macro for audio files", "audio: rock" },
            new string[] { "zip: / compressed:", "Filter macro for compressed archives", "zip: backup" },
            new string[] { "doc:", "Filter macro for documents and text", "doc: report" },
            new string[] { "exe:", "Filter macro for executable programs", "exe: setup" },
            new string[] { "pic:", "Filter macro for pictures and images", "pic: wallpaper" },
            new string[] { "video:", "Filter macro for video files", "video: 1080p" }
        };

        public SearchSyntaxForm()
        {
            InitializeComponent();
            PopulateTable(string.Empty);
        }

        private void InitializeComponent()
        {
            this.Text = "Everything Search Syntax Help";
            this.Size = new Size(720, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;
            this.MinimizeBox = false;

            lblSearch = new Label
            {
                Text = "Filter syntax:",
                Location = new Point(12, 14),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(90, 11),
                Size = new Size(240, 23)
            };
            txtSearch.TextChanged += (s, e) => PopulateTable(txtSearch.Text.Trim());

            lvSyntax = new ListView
            {
                Location = new Point(12, 42),
                Size = new Size(680, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            lvSyntax.Columns.Add("Syntax / Command", 150);
            lvSyntax.Columns.Add("Description", 350);
            lvSyntax.Columns.Add("Example", 160);

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(lvSyntax);
        }

        private void PopulateTable(string filter)
        {
            lvSyntax.Items.Clear();
            foreach (var row in SyntaxData)
            {
                if (string.IsNullOrEmpty(filter) ||
                    row[0].IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    row[1].IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    row[2].IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var item = new ListViewItem(row[0]);
                    item.SubItems.Add(row[1]);
                    item.SubItems.Add(row[2]);
                    lvSyntax.Items.Add(item);
                }
            }
        }
    }
}
