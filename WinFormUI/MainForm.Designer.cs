namespace WinFormUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileNewWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpenPath = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileShowInExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpenWith = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileCopyFullName = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditInvertSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuView = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewFilters = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewStatusBar = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.menuViewZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuViewZoomReset = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSearchMatchCase = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSearchMatchWholeWord = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSearchMatchPath = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSearchEnableRegex = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.menuSearchAdvanced = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.menuSearchFiltersMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBookmarks = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBookmarksAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBookmarksOrganize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBookmarksSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsRescan = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsCompactDb = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsRestartAsAdmin = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.menuToolsOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuToolsWebsite = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpSearchSyntax = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.txtFilterString = new System.Windows.Forms.TextBox();
            this.cmbFilter = new System.Windows.Forms.ComboBox();
            this.lvResult = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colExtension = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttributes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.muContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxOpenPath = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxShowInExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxOpenWith = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopyFullName = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopyName = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusItemCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusQueryTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.trayOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.trayExit = new System.Windows.Forms.ToolStripMenuItem();
            this.searchDebounceTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.muContextMenu.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.trayContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuEdit,
            this.menuView,
            this.menuSearch,
            this.menuBookmarks,
            this.menuTools,
            this.menuHelp});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(860, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileNewWindow,
            this.menuFileOpen,
            this.menuFileOpenPath,
            this.menuFileShowInExplorer,
            this.menuFileOpenWith,
            this.toolStripSeparator1,
            this.menuFileCopyFullName,
            this.menuFileCopyPath,
            this.toolStripSeparator2,
            this.menuFileExport,
            this.toolStripSeparator3,
            this.menuFileExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "&File";
            // 
            // menuFileNewWindow
            // 
            this.menuFileNewWindow.Name = "menuFileNewWindow";
            this.menuFileNewWindow.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuFileNewWindow.Size = new System.Drawing.Size(242, 22);
            this.menuFileNewWindow.Text = "&New Search Window";
            this.menuFileNewWindow.Click += new System.EventHandler(this.menuFileNewWindow_Click);
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.Name = "menuFileOpen";
            this.menuFileOpen.ShortcutKeyDisplayString = "Enter";
            this.menuFileOpen.Size = new System.Drawing.Size(242, 22);
            this.menuFileOpen.Text = "&Open";
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileOpenPath
            // 
            this.menuFileOpenPath.Name = "menuFileOpenPath";
            this.menuFileOpenPath.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Return)));
            this.menuFileOpenPath.Size = new System.Drawing.Size(242, 22);
            this.menuFileOpenPath.Text = "Open &Path";
            this.menuFileOpenPath.Click += new System.EventHandler(this.menuFileOpenPath_Click);
            // 
            // menuFileShowInExplorer
            // 
            this.menuFileShowInExplorer.Name = "menuFileShowInExplorer";
            this.menuFileShowInExplorer.Size = new System.Drawing.Size(242, 22);
            this.menuFileShowInExplorer.Text = "Show in &Explorer";
            this.menuFileShowInExplorer.Click += new System.EventHandler(this.menuFileShowInExplorer_Click);
            // 
            // menuFileOpenWith
            // 
            this.menuFileOpenWith.Name = "menuFileOpenWith";
            this.menuFileOpenWith.Size = new System.Drawing.Size(242, 22);
            this.menuFileOpenWith.Text = "Open &With...";
            this.menuFileOpenWith.Click += new System.EventHandler(this.menuFileOpenWith_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(239, 6);
            // 
            // menuFileCopyFullName
            // 
            this.menuFileCopyFullName.Name = "menuFileCopyFullName";
            this.menuFileCopyFullName.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.menuFileCopyFullName.Size = new System.Drawing.Size(242, 22);
            this.menuFileCopyFullName.Text = "Copy &Full Path";
            this.menuFileCopyFullName.Click += new System.EventHandler(this.menuFileCopyFullName_Click);
            // 
            // menuFileCopyPath
            // 
            this.menuFileCopyPath.Name = "menuFileCopyPath";
            this.menuFileCopyPath.Size = new System.Drawing.Size(242, 22);
            this.menuFileCopyPath.Text = "Copy Path";
            this.menuFileCopyPath.Click += new System.EventHandler(this.menuFileCopyPath_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(239, 6);
            // 
            // menuFileExport
            // 
            this.menuFileExport.Name = "menuFileExport";
            this.menuFileExport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuFileExport.Size = new System.Drawing.Size(242, 22);
            this.menuFileExport.Text = "&Export...";
            this.menuFileExport.Click += new System.EventHandler(this.menuFileExport_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(239, 6);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.menuFileExit.Size = new System.Drawing.Size(242, 22);
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuEdit
            // 
            this.menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuEditCut,
            this.menuEditCopy,
            this.toolStripSeparator4,
            this.menuEditSelectAll,
            this.menuEditInvertSelection});
            this.menuEdit.Name = "menuEdit";
            this.menuEdit.Size = new System.Drawing.Size(39, 20);
            this.menuEdit.Text = "&Edit";
            // 
            // menuEditCut
            // 
            this.menuEditCut.Name = "menuEditCut";
            this.menuEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.menuEditCut.Size = new System.Drawing.Size(193, 22);
            this.menuEditCut.Text = "Cu&t";
            this.menuEditCut.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // menuEditCopy
            // 
            this.menuEditCopy.Name = "menuEditCopy";
            this.menuEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.menuEditCopy.Size = new System.Drawing.Size(193, 22);
            this.menuEditCopy.Text = "&Copy";
            this.menuEditCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(190, 6);
            // 
            // menuEditSelectAll
            // 
            this.menuEditSelectAll.Name = "menuEditSelectAll";
            this.menuEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.menuEditSelectAll.Size = new System.Drawing.Size(193, 22);
            this.menuEditSelectAll.Text = "Select &All";
            this.menuEditSelectAll.Click += new System.EventHandler(this.menuEditSelectAll_Click);
            // 
            // menuEditInvertSelection
            // 
            this.menuEditInvertSelection.Name = "menuEditInvertSelection";
            this.menuEditInvertSelection.Size = new System.Drawing.Size(193, 22);
            this.menuEditInvertSelection.Text = "&Invert Selection";
            this.menuEditInvertSelection.Click += new System.EventHandler(this.menuEditInvertSelection_Click);
            // 
            // menuView
            // 
            this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewFilters,
            this.menuViewStatusBar,
            this.toolStripSeparator5,
            this.menuViewRefresh,
            this.toolStripSeparator6,
            this.menuViewZoomIn,
            this.menuViewZoomOut,
            this.menuViewZoomReset});
            this.menuView.Name = "menuView";
            this.menuView.Size = new System.Drawing.Size(44, 20);
            this.menuView.Text = "&View";
            // 
            // menuViewFilters
            // 
            this.menuViewFilters.Checked = true;
            this.menuViewFilters.CheckOnClick = true;
            this.menuViewFilters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuViewFilters.Name = "menuViewFilters";
            this.menuViewFilters.Size = new System.Drawing.Size(182, 22);
            this.menuViewFilters.Text = "&Filters Bar";
            this.menuViewFilters.Click += new System.EventHandler(this.menuViewFilters_Click);
            // 
            // menuViewStatusBar
            // 
            this.menuViewStatusBar.Checked = true;
            this.menuViewStatusBar.CheckOnClick = true;
            this.menuViewStatusBar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuViewStatusBar.Name = "menuViewStatusBar";
            this.menuViewStatusBar.Size = new System.Drawing.Size(182, 22);
            this.menuViewStatusBar.Text = "&Status Bar";
            this.menuViewStatusBar.Click += new System.EventHandler(this.menuViewStatusBar_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(179, 6);
            // 
            // menuViewRefresh
            // 
            this.menuViewRefresh.Name = "menuViewRefresh";
            this.menuViewRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.menuViewRefresh.Size = new System.Drawing.Size(182, 22);
            this.menuViewRefresh.Text = "&Refresh";
            this.menuViewRefresh.Click += new System.EventHandler(this.menuViewRefresh_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(179, 6);
            // 
            // menuViewZoomIn
            // 
            this.menuViewZoomIn.Name = "menuViewZoomIn";
            this.menuViewZoomIn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.menuViewZoomIn.Size = new System.Drawing.Size(182, 22);
            this.menuViewZoomIn.Text = "Zoom &In";
            this.menuViewZoomIn.Click += new System.EventHandler(this.menuViewZoomIn_Click);
            // 
            // menuViewZoomOut
            // 
            this.menuViewZoomOut.Name = "menuViewZoomOut";
            this.menuViewZoomOut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.menuViewZoomOut.Size = new System.Drawing.Size(182, 22);
            this.menuViewZoomOut.Text = "Zoom &Out";
            this.menuViewZoomOut.Click += new System.EventHandler(this.menuViewZoomOut_Click);
            // 
            // menuViewZoomReset
            // 
            this.menuViewZoomReset.Name = "menuViewZoomReset";
            this.menuViewZoomReset.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.menuViewZoomReset.Size = new System.Drawing.Size(182, 22);
            this.menuViewZoomReset.Text = "Reset Zoom";
            this.menuViewZoomReset.Click += new System.EventHandler(this.menuViewZoomReset_Click);
            // 
            // menuSearch
            // 
            this.menuSearch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSearchMatchCase,
            this.menuSearchMatchWholeWord,
            this.menuSearchMatchPath,
            this.menuSearchEnableRegex,
            this.toolStripSeparator7,
            this.menuSearchAdvanced,
            this.toolStripSeparator8,
            this.menuSearchFiltersMenu});
            this.menuSearch.Name = "menuSearch";
            this.menuSearch.Size = new System.Drawing.Size(54, 20);
            this.menuSearch.Text = "&Search";
            // 
            // menuSearchMatchCase
            // 
            this.menuSearchMatchCase.CheckOnClick = true;
            this.menuSearchMatchCase.Name = "menuSearchMatchCase";
            this.menuSearchMatchCase.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.I)));
            this.menuSearchMatchCase.Size = new System.Drawing.Size(217, 22);
            this.menuSearchMatchCase.Text = "Match &Case";
            this.menuSearchMatchCase.Click += new System.EventHandler(this.menuSearchOption_Click);
            // 
            // menuSearchMatchWholeWord
            // 
            this.menuSearchMatchWholeWord.CheckOnClick = true;
            this.menuSearchMatchWholeWord.Name = "menuSearchMatchWholeWord";
            this.menuSearchMatchWholeWord.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.B)));
            this.menuSearchMatchWholeWord.Size = new System.Drawing.Size(217, 22);
            this.menuSearchMatchWholeWord.Text = "Match &Whole Word";
            this.menuSearchMatchWholeWord.Click += new System.EventHandler(this.menuSearchOption_Click);
            // 
            // menuSearchMatchPath
            // 
            this.menuSearchMatchPath.CheckOnClick = true;
            this.menuSearchMatchPath.Name = "menuSearchMatchPath";
            this.menuSearchMatchPath.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U)));
            this.menuSearchMatchPath.Size = new System.Drawing.Size(217, 22);
            this.menuSearchMatchPath.Text = "Match &Path";
            this.menuSearchMatchPath.Click += new System.EventHandler(this.menuSearchOption_Click);
            // 
            // menuSearchEnableRegex
            // 
            this.menuSearchEnableRegex.CheckOnClick = true;
            this.menuSearchEnableRegex.Name = "menuSearchEnableRegex";
            this.menuSearchEnableRegex.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
            this.menuSearchEnableRegex.Size = new System.Drawing.Size(217, 22);
            this.menuSearchEnableRegex.Text = "Enable &Regex";
            this.menuSearchEnableRegex.Click += new System.EventHandler(this.menuSearchOption_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(214, 6);
            // 
            // menuSearchAdvanced
            // 
            this.menuSearchAdvanced.Name = "menuSearchAdvanced";
            this.menuSearchAdvanced.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.menuSearchAdvanced.Size = new System.Drawing.Size(217, 22);
            this.menuSearchAdvanced.Text = "&Advanced Search...";
            this.menuSearchAdvanced.Click += new System.EventHandler(this.menuSearchAdvanced_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(214, 6);
            // 
            // menuSearchFiltersMenu
            // 
            this.menuSearchFiltersMenu.Name = "menuSearchFiltersMenu";
            this.menuSearchFiltersMenu.Size = new System.Drawing.Size(217, 22);
            this.menuSearchFiltersMenu.Text = "&Filters";
            // 
            // menuBookmarks
            // 
            this.menuBookmarks.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuBookmarksAdd,
            this.menuBookmarksOrganize,
            this.menuBookmarksSeparator});
            this.menuBookmarks.Name = "menuBookmarks";
            this.menuBookmarks.Size = new System.Drawing.Size(78, 20);
            this.menuBookmarks.Text = "&Bookmarks";
            // 
            // menuBookmarksAdd
            // 
            this.menuBookmarksAdd.Name = "menuBookmarksAdd";
            this.menuBookmarksAdd.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.menuBookmarksAdd.Size = new System.Drawing.Size(217, 22);
            this.menuBookmarksAdd.Text = "&Add to Bookmarks...";
            this.menuBookmarksAdd.Click += new System.EventHandler(this.menuBookmarksAdd_Click);
            // 
            // menuBookmarksOrganize
            // 
            this.menuBookmarksOrganize.Name = "menuBookmarksOrganize";
            this.menuBookmarksOrganize.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
            this.menuBookmarksOrganize.Size = new System.Drawing.Size(217, 22);
            this.menuBookmarksOrganize.Text = "&Organize Bookmarks...";
            this.menuBookmarksOrganize.Click += new System.EventHandler(this.menuBookmarksOrganize_Click);
            // 
            // menuBookmarksSeparator
            // 
            this.menuBookmarksSeparator.Name = "menuBookmarksSeparator";
            this.menuBookmarksSeparator.Size = new System.Drawing.Size(214, 6);
            // 
            // menuTools
            // 
            this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolsRescan,
            this.menuToolsRestartAsAdmin,
            this.toolStripSeparator9,
            this.menuToolsOptions,
            this.menuToolsWebsite});
            this.menuTools.Name = "menuTools";
            this.menuTools.Size = new System.Drawing.Size(46, 20);
            this.menuTools.Text = "&Tools";
            // 
            // menuToolsRescan
            // 
            this.menuToolsRescan.Name = "menuToolsRescan";
            this.menuToolsRescan.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.menuToolsRescan.Size = new System.Drawing.Size(209, 22);
            this.menuToolsRescan.Text = "&Rescan All Drives";
            this.menuToolsRescan.Click += new System.EventHandler(this.menuToolsRescan_Click);
            // 
            // menuToolsCompactDb
            // 
            this.menuToolsCompactDb.Name = "menuToolsCompactDb";
            this.menuToolsCompactDb.Size = new System.Drawing.Size(209, 22);
            this.menuToolsCompactDb.Text = "&Compact Database";
            this.menuToolsCompactDb.Visible = false;
            this.menuToolsCompactDb.Click += new System.EventHandler(this.menuToolsCompactDb_Click);
            // 
            // menuToolsRestartAsAdmin
            // 
            this.menuToolsRestartAsAdmin.Name = "menuToolsRestartAsAdmin";
            this.menuToolsRestartAsAdmin.Size = new System.Drawing.Size(209, 22);
            this.menuToolsRestartAsAdmin.Text = "Restart as &Administrator...";
            this.menuToolsRestartAsAdmin.Click += new System.EventHandler(this.menuToolsRestartAsAdmin_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(206, 6);
            // 
            // menuToolsOptions
            // 
            this.menuToolsOptions.Name = "menuToolsOptions";
            this.menuToolsOptions.Size = new System.Drawing.Size(209, 22);
            this.menuToolsOptions.Text = "&Options...";
            this.menuToolsOptions.Click += new System.EventHandler(this.menuToolsOptions_Click);
            // 
            // menuToolsWebsite
            // 
            this.menuToolsWebsite.Name = "menuToolsWebsite";
            this.menuToolsWebsite.Size = new System.Drawing.Size(209, 22);
            this.menuToolsWebsite.Text = "Visit Everything &Website";
            this.menuToolsWebsite.Click += new System.EventHandler(this.menuToolsWebsite_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpSearchSyntax,
            this.toolStripSeparator10,
            this.menuHelpAbout});
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(44, 20);
            this.menuHelp.Text = "&Help";
            // 
            // menuHelpSearchSyntax
            // 
            this.menuHelpSearchSyntax.Name = "menuHelpSearchSyntax";
            this.menuHelpSearchSyntax.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.menuHelpSearchSyntax.Size = new System.Drawing.Size(200, 22);
            this.menuHelpSearchSyntax.Text = "&Search Syntax Help";
            this.menuHelpSearchSyntax.Click += new System.EventHandler(this.menuHelpSearchSyntax_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(197, 6);
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Name = "menuHelpAbout";
            this.menuHelpAbout.Size = new System.Drawing.Size(200, 22);
            this.menuHelpAbout.Text = "&About EverythingEx";
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.txtFilterString);
            this.pnlSearch.Controls.Add(this.cmbFilter);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 24);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlSearch.Size = new System.Drawing.Size(860, 28);
            this.pnlSearch.TabIndex = 1;
            // 
            // txtFilterString
            // 
            this.txtFilterString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFilterString.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFilterString.Location = new System.Drawing.Point(4, 3);
            this.txtFilterString.Name = "txtFilterString";
            this.txtFilterString.Size = new System.Drawing.Size(732, 23);
            this.txtFilterString.TabIndex = 0;
            this.txtFilterString.TextChanged += new System.EventHandler(this.txtFilterString_TextChanged);
            this.txtFilterString.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFilterString_KeyDown);
            // 
            // cmbFilter
            // 
            this.cmbFilter.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbFilter.FormattingEnabled = true;
            this.cmbFilter.Location = new System.Drawing.Point(736, 3);
            this.cmbFilter.Name = "cmbFilter";
            this.cmbFilter.Size = new System.Drawing.Size(120, 23);
            this.cmbFilter.TabIndex = 1;
            this.cmbFilter.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_SelectedIndexChanged);
            // 
            // lvResult
            // 
            this.lvResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colPath,
            this.colSize,
            this.colDateModified,
            this.colExtension,
            this.colAttributes});
            this.lvResult.ContextMenuStrip = this.muContextMenu;
            this.lvResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvResult.FullRowSelect = true;
            this.lvResult.HideSelection = false;
            this.lvResult.Location = new System.Drawing.Point(0, 52);
            this.lvResult.Name = "lvResult";
            this.lvResult.Size = new System.Drawing.Size(860, 396);
            this.lvResult.TabIndex = 2;
            this.lvResult.UseCompatibleStateImageBehavior = false;
            this.lvResult.View = System.Windows.Forms.View.Details;
            this.lvResult.VirtualMode = true;
            this.lvResult.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvResult_ColumnClick);
            this.lvResult.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvResult_RetrieveVirtualItem);
            this.lvResult.SelectedIndexChanged += new System.EventHandler(this.lvResult_SelectedIndexChanged);
            this.lvResult.DoubleClick += new System.EventHandler(this.lvResult_DoubleClick);
            this.lvResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvResult_KeyDown);
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 280;
            // 
            // colPath
            // 
            this.colPath.Text = "Path";
            this.colPath.Width = 360;
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            this.colSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colSize.Width = 90;
            // 
            // colDateModified
            // 
            this.colDateModified.Text = "Date Modified";
            this.colDateModified.Width = 130;
            // 
            // colExtension
            // 
            this.colExtension.Text = "Extension";
            this.colExtension.Width = 70;
            // 
            // colAttributes
            // 
            this.colAttributes.Text = "Attributes";
            this.colAttributes.Width = 65;
            // 
            // muContextMenu
            // 
            this.muContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxOpen,
            this.ctxOpenPath,
            this.ctxShowInExplorer,
            this.ctxOpenWith,
            this.toolStripSeparator11,
            this.ctxCopy,
            this.ctxCopyFullName,
            this.ctxCopyPath,
            this.ctxCopyName,
            this.toolStripSeparator12,
            this.ctxDelete,
            this.ctxRename,
            this.toolStripSeparator13,
            this.ctxProperties});
            this.muContextMenu.Name = "muContextMenu";
            this.muContextMenu.Size = new System.Drawing.Size(203, 264);
            // 
            // ctxOpen
            // 
            this.ctxOpen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctxOpen.Name = "ctxOpen";
            this.ctxOpen.Size = new System.Drawing.Size(202, 22);
            this.ctxOpen.Text = "&Open";
            this.ctxOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // ctxOpenPath
            // 
            this.ctxOpenPath.Name = "ctxOpenPath";
            this.ctxOpenPath.Size = new System.Drawing.Size(202, 22);
            this.ctxOpenPath.Text = "Open &Path";
            this.ctxOpenPath.Click += new System.EventHandler(this.menuFileOpenPath_Click);
            // 
            // ctxShowInExplorer
            // 
            this.ctxShowInExplorer.Name = "ctxShowInExplorer";
            this.ctxShowInExplorer.Size = new System.Drawing.Size(202, 22);
            this.ctxShowInExplorer.Text = "Show in &Explorer";
            this.ctxShowInExplorer.Click += new System.EventHandler(this.menuFileShowInExplorer_Click);
            // 
            // ctxOpenWith
            // 
            this.ctxOpenWith.Name = "ctxOpenWith";
            this.ctxOpenWith.Size = new System.Drawing.Size(202, 22);
            this.ctxOpenWith.Text = "Open &With...";
            this.ctxOpenWith.Click += new System.EventHandler(this.menuFileOpenWith_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(199, 6);
            // 
            // ctxCopy
            // 
            this.ctxCopy.Name = "ctxCopy";
            this.ctxCopy.Size = new System.Drawing.Size(202, 22);
            this.ctxCopy.Text = "&Copy";
            this.ctxCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
            // 
            // ctxCopyFullName
            // 
            this.ctxCopyFullName.Name = "ctxCopyFullName";
            this.ctxCopyFullName.Size = new System.Drawing.Size(202, 22);
            this.ctxCopyFullName.Text = "Copy &Full Path";
            this.ctxCopyFullName.Click += new System.EventHandler(this.menuFileCopyFullName_Click);
            // 
            // ctxCopyPath
            // 
            this.ctxCopyPath.Name = "ctxCopyPath";
            this.ctxCopyPath.Size = new System.Drawing.Size(202, 22);
            this.ctxCopyPath.Text = "Copy Path";
            this.ctxCopyPath.Click += new System.EventHandler(this.menuFileCopyPath_Click);
            // 
            // ctxCopyName
            // 
            this.ctxCopyName.Name = "ctxCopyName";
            this.ctxCopyName.Size = new System.Drawing.Size(202, 22);
            this.ctxCopyName.Text = "Copy Name";
            this.ctxCopyName.Click += new System.EventHandler(this.ctxCopyName_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(199, 6);
            // 
            // ctxDelete
            // 
            this.ctxDelete.Name = "ctxDelete";
            this.ctxDelete.Size = new System.Drawing.Size(202, 22);
            this.ctxDelete.Text = "&Delete";
            this.ctxDelete.Click += new System.EventHandler(this.ctxDelete_Click);
            // 
            // ctxRename
            // 
            this.ctxRename.Name = "ctxRename";
            this.ctxRename.Size = new System.Drawing.Size(202, 22);
            this.ctxRename.Text = "&Rename";
            this.ctxRename.Click += new System.EventHandler(this.ctxRename_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(199, 6);
            // 
            // ctxProperties
            // 
            this.ctxProperties.Name = "ctxProperties";
            this.ctxProperties.Size = new System.Drawing.Size(202, 22);
            this.ctxProperties.Text = "P&roperties";
            this.ctxProperties.Click += new System.EventHandler(this.ctxProperties_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusItemCount,
            this.statusSpacer,
            this.statusMessage,
            this.statusQueryTime});
            this.statusBar.Location = new System.Drawing.Point(0, 448);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(860, 22);
            this.statusBar.TabIndex = 3;
            this.statusBar.Text = "statusBar";
            // 
            // statusItemCount
            // 
            this.statusItemCount.Name = "statusItemCount";
            this.statusItemCount.Size = new System.Drawing.Size(53, 17);
            this.statusItemCount.Text = "0 objects";
            // 
            // statusSpacer
            // 
            this.statusSpacer.Name = "statusSpacer";
            this.statusSpacer.Size = new System.Drawing.Size(738, 17);
            this.statusSpacer.Spring = true;
            // 
            // statusMessage
            // 
            this.statusMessage.Name = "statusMessage";
            this.statusMessage.Size = new System.Drawing.Size(40, 17);
            this.statusMessage.Text = "Ready";
            // 
            // statusQueryTime
            // 
            this.statusQueryTime.Name = "statusQueryTime";
            this.statusQueryTime.Size = new System.Drawing.Size(43, 17);
            this.statusQueryTime.Text = "0.000s";
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.trayContextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.notifyIcon.Text = "EverythingEx";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // trayContextMenu
            // 
            this.trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trayOpen,
            this.trayOptions,
            this.toolStripSeparator14,
            this.trayExit});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(161, 76);
            // 
            // trayOpen
            // 
            this.trayOpen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trayOpen.Name = "trayOpen";
            this.trayOpen.Size = new System.Drawing.Size(160, 22);
            this.trayOpen.Text = "Open Everything";
            this.trayOpen.Click += new System.EventHandler(this.trayOpen_Click);
            // 
            // trayOptions
            // 
            this.trayOptions.Name = "trayOptions";
            this.trayOptions.Size = new System.Drawing.Size(160, 22);
            this.trayOptions.Text = "&Options...";
            this.trayOptions.Click += new System.EventHandler(this.menuToolsOptions_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(157, 6);
            // 
            // trayExit
            // 
            this.trayExit.Name = "trayExit";
            this.trayExit.Size = new System.Drawing.Size(160, 22);
            this.trayExit.Text = "E&xit";
            this.trayExit.Click += new System.EventHandler(this.trayExit_Click);
            // 
            // searchDebounceTimer
            // 
            this.searchDebounceTimer.Interval = 40;
            this.searchDebounceTimer.Tick += new System.EventHandler(this.searchDebounceTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 470);
            this.Controls.Add(this.lvResult);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Everything";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.muContextMenu.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.trayContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileNewWindow;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpenPath;
        private System.Windows.Forms.ToolStripMenuItem menuFileShowInExplorer;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpenWith;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuFileCopyFullName;
        private System.Windows.Forms.ToolStripMenuItem menuFileCopyPath;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuFileExport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;

        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuEditCut;
        private System.Windows.Forms.ToolStripMenuItem menuEditCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem menuEditSelectAll;
        private System.Windows.Forms.ToolStripMenuItem menuEditInvertSelection;

        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuViewFilters;
        private System.Windows.Forms.ToolStripMenuItem menuViewStatusBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem menuViewRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomIn;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomOut;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomReset;

        private System.Windows.Forms.ToolStripMenuItem menuSearch;
        private System.Windows.Forms.ToolStripMenuItem menuSearchMatchCase;
        private System.Windows.Forms.ToolStripMenuItem menuSearchMatchWholeWord;
        private System.Windows.Forms.ToolStripMenuItem menuSearchMatchPath;
        private System.Windows.Forms.ToolStripMenuItem menuSearchEnableRegex;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem menuSearchAdvanced;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem menuSearchFiltersMenu;

        private System.Windows.Forms.ToolStripMenuItem menuBookmarks;
        private System.Windows.Forms.ToolStripMenuItem menuBookmarksAdd;
        private System.Windows.Forms.ToolStripMenuItem menuBookmarksOrganize;
        private System.Windows.Forms.ToolStripSeparator menuBookmarksSeparator;

        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuToolsRescan;
        private System.Windows.Forms.ToolStripMenuItem menuToolsCompactDb;
        private System.Windows.Forms.ToolStripMenuItem menuToolsRestartAsAdmin;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem menuToolsOptions;
        private System.Windows.Forms.ToolStripMenuItem menuToolsWebsite;

        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpSearchSyntax;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;

        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.TextBox txtFilterString;
        private System.Windows.Forms.ComboBox cmbFilter;

        private System.Windows.Forms.ListView lvResult;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colPath;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colDateModified;
        private System.Windows.Forms.ColumnHeader colExtension;
        private System.Windows.Forms.ColumnHeader colAttributes;

        private System.Windows.Forms.ContextMenuStrip muContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ctxOpen;
        private System.Windows.Forms.ToolStripMenuItem ctxOpenPath;
        private System.Windows.Forms.ToolStripMenuItem ctxShowInExplorer;
        private System.Windows.Forms.ToolStripMenuItem ctxOpenWith;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem ctxCopy;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyFullName;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyPath;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem ctxDelete;
        private System.Windows.Forms.ToolStripMenuItem ctxRename;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem ctxProperties;

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusItemCount;
        private System.Windows.Forms.ToolStripStatusLabel statusSpacer;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.ToolStripStatusLabel statusQueryTime;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem trayOpen;
        private System.Windows.Forms.ToolStripMenuItem trayOptions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem trayExit;

        private System.Windows.Forms.Timer searchDebounceTimer;
    }
}
