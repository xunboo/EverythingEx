using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QueryEngine;
using WinFormUI.Core;
using WinFormUI.Forms;

namespace WinFormUI
{
    public partial class MainForm : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true)]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

        /// <summary>
        /// Compacts Large Object Heap (LOH) and releases unreferenced memory pages back to the Windows OS kernel.
        /// </summary>
        public static void TrimProcessMemory()
        {
            try
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
            }
            catch { }
        }

        private List<FileAndDirectoryEntry> allEntries = new List<FileAndDirectoryEntry>();
        private List<FileAndDirectoryEntry> filteredEntries = new List<FileAndDirectoryEntry>();
        private readonly object entriesLock = new object();

        private SearchOptions currentOptions = new SearchOptions();
        private RealTimeIndexWatcher indexWatcher;
        private CancellationTokenSource searchCts;
        private CancellationTokenSource loadCts;
        private CancellationTokenSource sortCts;

        private FileColumn activeSortColumn = FileColumn.Name;
        private SortOrder sortOrder = SortOrder.Ascending;
        private bool isUserSorted = false;
        private string previousQuery = null;
        private volatile bool isExiting = false;

        private ContextMenuStrip columnContextMenu;
        private HeaderSubclass headerSubclass;
        private readonly HashSet<FileColumn> visibleColumns = new HashSet<FileColumn>();

        private System.Windows.Forms.Timer realTimeRefreshTimer;
        private System.Windows.Forms.Timer dbAutoSaveTimer;
        private volatile bool isDatabaseDirty = false;

        public MainForm()
        {
            InitializeComponent();
            EnableDoubleBuffer(lvResult);
            muContextMenu.Opening += muContextMenu_Opening;

            realTimeRefreshTimer = new System.Windows.Forms.Timer();
            realTimeRefreshTimer.Interval = 1000;
            realTimeRefreshTimer.Tick += (s, e) =>
            {
                realTimeRefreshTimer.Stop();
                TriggerSearch(true);
            };

            dbAutoSaveTimer = new System.Windows.Forms.Timer();
            dbAutoSaveTimer.Interval = 5000;
            dbAutoSaveTimer.Tick += (s, e) =>
            {
                dbAutoSaveTimer.Stop();
                if (isDatabaseDirty)
                {
                    isDatabaseDirty = false;
                    Task.Run(() =>
                    {
                        List<FileAndDirectoryEntry> snapshot;
                        lock (entriesLock)
                        {
                            if (allEntries == null || allEntries.Count == 0) return;
                            snapshot = new List<FileAndDirectoryEntry>(allEntries);
                        }
                        IndexDatabase.SaveToFile(snapshot);
                    });
                }
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AttachHeaderSubclass();
        }

        private static void EnableDoubleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AppSettings.Load();
            ApplyAppSettings();
            InitializeColumns();
            AttachHeaderSubclass();

            // Set SmallImageList
            lvResult.SmallImageList = ShellIconHelper.SmallImageList;

            // Populate Filter dropdown
            cmbFilter.Items.Clear();
            var filters = FilterCategory.GetDefaultFilters();
            foreach (var f in filters)
            {
                cmbFilter.Items.Add(f.Name);
            }
            cmbFilter.SelectedIndex = 0;

            // Populate Filter Submenu in Search menu
            PopulateFiltersMenu();

            // Populate Bookmarks menu
            RefreshBookmarksMenu();

            // Apply fonts
            ApplyListFont();

            // Admin status setup
            if (UserSecurityHelper.IsRunningAsAdministrator())
            {
                menuToolsRestartAsAdmin.Text = "Running as Administrator (Elevated)";
                menuToolsRestartAsAdmin.Enabled = false;
            }
            else
            {
                menuToolsRestartAsAdmin.Text = "Restart as &Administrator...";
                menuToolsRestartAsAdmin.Enabled = true;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            txtFilterString.Focus();
            LoadAllDrivesAsync();
        }

        private void ApplyAppSettings()
        {
            var s = AppSettings.Instance;
            notifyIcon.Visible = s.ShowTrayIcon;
            menuViewFilters.Checked = pnlSearch.Visible;
            menuViewStatusBar.Checked = statusBar.Visible;
        }

        private void ApplyListFont()
        {
            float size = AppSettings.Instance.FontSize;
            if (size >= 7 && size <= 24)
            {
                lvResult.Font = new Font("Segoe UI", size, FontStyle.Regular);
            }
        }

        private void PopulateFiltersMenu()
        {
            menuSearchFiltersMenu.DropDownItems.Clear();
            var filters = FilterCategory.GetDefaultFilters();
            for (int i = 0; i < filters.Count; i++)
            {
                var f = filters[i];
                var mi = new ToolStripMenuItem(f.Name);
                int index = i;
                mi.Click += (s, e) =>
                {
                    if (index >= 0 && index < cmbFilter.Items.Count)
                    {
                        cmbFilter.SelectedIndex = index;
                    }
                };
                menuSearchFiltersMenu.DropDownItems.Add(mi);
            }

            menuSearchFiltersMenu.DropDownItems.Add(new ToolStripSeparator());
            var miOrganize = new ToolStripMenuItem("&Organize Filters...");
            miOrganize.Click += (s, e) =>
            {
                MessageBox.Show("All standard Voidtools Everything filters (Audio, Compressed, Document, Executable, Folder, Picture, Video) are available in the filter dropdown and search bar macros.",
                    "Filters", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            menuSearchFiltersMenu.DropDownItems.Add(miOrganize);
        }

        private void RefreshBookmarksMenu()
        {
            // Remove existing bookmark items after separator
            int sepIndex = menuBookmarks.DropDownItems.IndexOf(menuBookmarksSeparator);
            while (menuBookmarks.DropDownItems.Count > sepIndex + 1)
            {
                menuBookmarks.DropDownItems.RemoveAt(menuBookmarks.DropDownItems.Count - 1);
            }

            foreach (var b in BookmarkManager.Bookmarks)
            {
                var mi = new ToolStripMenuItem(b.Name);
                var captured = b;
                mi.Click += (s, e) =>
                {
                    txtFilterString.Text = captured.Query ?? string.Empty;
                    for (int i = 0; i < cmbFilter.Items.Count; i++)
                    {
                        if (string.Equals(cmbFilter.Items[i].ToString(), captured.Filter, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbFilter.SelectedIndex = i;
                            break;
                        }
                    }
                    txtFilterString.Focus();
                };
                menuBookmarks.DropDownItems.Add(mi);
            }
        }

        private void LoadAllDrivesAsync(bool forceRescan = false)
        {
            loadCts?.Cancel();
            loadCts = new CancellationTokenSource();
            var token = loadCts.Token;

            statusMessage.Text = "Loading index database...";

            Task.Run(() =>
            {
                try
                {
                    // Check if on-disk Everything.db database exists and we are not forcing a rescan
                    if (!forceRescan && IndexDatabase.DatabaseExists())
                    {
                        var cached = IndexDatabase.LoadFromFile();
                        if (cached != null && cached.Count > 0)
                        {
                            lock (entriesLock)
                            {
                                this.allEntries = cached;
                            }

                            // Start Real-Time Watcher
                            if (AppSettings.Instance.RealTimeMonitoring)
                            {
                                indexWatcher?.Dispose();
                                indexWatcher = new RealTimeIndexWatcher(allEntries, entriesLock);
                                indexWatcher.StartWatchingDrives(Engine.GetAllAvailableDrives());
                                indexWatcher.IndexChanged += (s, e) =>
                                {
                                    this.InvokeIfRequired(m =>
                                    {
                                        m.isDatabaseDirty = true;
                                        m.realTimeRefreshTimer.Stop();
                                        m.realTimeRefreshTimer.Start();
                                        m.dbAutoSaveTimer.Stop();
                                        m.dbAutoSaveTimer.Start();
                                    });
                                };
                            }

                            this.InvokeIfRequired(m =>
                            {
                                m.statusMessage.Text = "Ready (Database loaded: " + cached.Count.ToString("N0") + " items)";
                                m.TriggerSearch(true);
                                m.StartBackgroundSizePopulator();
                            });

                            Task.Run(() =>
                            {
                                Thread.Sleep(300);
                                TrimProcessMemory();
                            });

                            // If legacy database format (>80MB), auto-compact in background
                            if (IndexDatabase.GetDatabaseSize() > 80 * 1024 * 1024)
                            {
                                Task.Run(() => IndexDatabase.SaveToFile(cached));
                            }
                            return;
                        }
                    }

                    // Scan from drives
                    var progress = new Progress<string>(msg =>
                    {
                        this.InvokeIfRequired(m => m.statusMessage.Text = msg);
                    });

                    var entries = Engine.GetAllFilesAndDirectories(progress, token);

                    lock (entriesLock)
                    {
                        this.allEntries = entries;
                    }

                    // Save to Everything.db
                    IndexDatabase.SaveToFile(allEntries);

                    // Start Real-Time Watcher
                    if (AppSettings.Instance.RealTimeMonitoring)
                    {
                        indexWatcher?.Dispose();
                        indexWatcher = new RealTimeIndexWatcher(allEntries, entriesLock);
                        indexWatcher.StartWatchingDrives(Engine.GetAllAvailableDrives());
                        indexWatcher.IndexChanged += (s, e) =>
                        {
                            this.InvokeIfRequired(m =>
                            {
                                m.isDatabaseDirty = true;
                                m.realTimeRefreshTimer.Stop();
                                m.realTimeRefreshTimer.Start();
                                m.dbAutoSaveTimer.Stop();
                                m.dbAutoSaveTimer.Start();
                            });
                        };
                    }

                    this.InvokeIfRequired(m =>
                    {
                        m.statusMessage.Text = "Ready";
                        m.TriggerSearch(true);
                        m.StartBackgroundSizePopulator();
                    });

                    Task.Run(() =>
                    {
                        Thread.Sleep(300);
                        TrimProcessMemory();
                    });
                }
                catch (Exception ex)
                {
                    this.InvokeIfRequired(m =>
                    {
                        m.statusMessage.Text = "Error: " + ex.Message;
                    });
                }
            }, token);
        }

        private void txtFilterString_TextChanged(object sender, EventArgs e)
        {
            searchDebounceTimer.Stop();
            searchDebounceTimer.Start();
        }

        private void searchDebounceTimer_Tick(object sender, EventArgs e)
        {
            searchDebounceTimer.Stop();
            TriggerSearch(false);
        }

        private void TriggerSearch(bool force)
        {
            string query = txtFilterString.Text;
            if (!force && string.Equals(query, previousQuery, StringComparison.Ordinal) &&
                currentOptions.ActiveFilter.Name == cmbFilter.SelectedItem?.ToString())
            {
                return;
            }

            previousQuery = query;

            searchCts?.Cancel();
            sortCts?.Cancel();
            searchCts = new CancellationTokenSource();
            var token = searchCts.Token;

            var optionsCopy = currentOptions.Clone();
            int selectedFilterIdx = cmbFilter.SelectedIndex;
            var filters = FilterCategory.GetDefaultFilters();
            if (selectedFilterIdx >= 0 && selectedFilterIdx < filters.Count)
            {
                optionsCopy.ActiveFilter = filters[selectedFilterIdx];
            }

            var sourceEntries = allEntries;
            if (sourceEntries == null || sourceEntries.Count == 0)
            {
                filteredEntries = new List<FileAndDirectoryEntry>(0);
                lvResult.VirtualListSize = 0;
                statusItemCount.Text = "0 objects";
                statusQueryTime.Text = "0.000s";
                return;
            }

            bool hasFilter = optionsCopy.ActiveFilter != null && !string.IsNullOrEmpty(optionsCopy.ActiveFilter.FilterQuery);
            bool hasQuery = !string.IsNullOrWhiteSpace(query);

            Stopwatch sw = Stopwatch.StartNew();

            Task.Run(() =>
            {
                List<FileAndDirectoryEntry> results;

                if (!hasQuery && !hasFilter)
                {
                    if (!isUserSorted)
                    {
                        results = sourceEntries;
                    }
                    else
                    {
                        List<FileAndDirectoryEntry> activeList;
                        lock (entriesLock)
                        {
                            activeList = new List<FileAndDirectoryEntry>(sourceEntries.Count);
                            for (int i = 0; i < sourceEntries.Count; i++)
                            {
                                var item = sourceEntries[i];
                                if (item != null && !item.IsDeleted)
                                {
                                    activeList.Add(item);
                                }
                            }
                        }

                        if (activeList.Count > 0)
                        {
                            ParallelSorter.Sort(activeList, (int)activeSortColumn, sortOrder == SortOrder.Ascending, token);
                        }
                        results = activeList;
                    }
                }
                else
                {
                    results = SearchEngine.Search(sourceEntries, query, optionsCopy, token);
                    if (isUserSorted && results.Count > 0)
                    {
                        ParallelSorter.Sort(results, (int)activeSortColumn, sortOrder == SortOrder.Ascending, token);
                    }
                }

                // Verify matching results actually exist on disk to filter out any stale or deleted items
                if (results != null && results.Count > 0 && results.Count <= 5000)
                {
                    bool anyDeleted = false;
                    for (int i = results.Count - 1; i >= 0; i--)
                    {
                        var item = results[i];
                        if (item.IsDeleted || !FastFileInfo.Exists(item))
                        {
                            results.RemoveAt(i);
                            anyDeleted = true;
                        }
                    }

                    if (anyDeleted)
                    {
                        this.InvokeIfRequired(m =>
                        {
                            m.isDatabaseDirty = true;
                            m.dbAutoSaveTimer.Stop();
                            m.dbAutoSaveTimer.Start();
                        });
                    }
                }

                sw.Stop();

                if (!token.IsCancellationRequested)
                {
                    this.InvokeIfRequired(m =>
                    {
                        m.filteredEntries = results;
                        m.lvResult.VirtualListSize = results.Count;
                        m.lvResult.Invalidate();

                        m.statusItemCount.Text = string.Format("{0:N0} objects", results.Count);
                        m.statusQueryTime.Text = string.Format("{0:F3}s", sw.Elapsed.TotalSeconds);
                        m.UpdateTitleWithSelection();
                        m.UpdateSortArrow();
                    });
                }
            }, token);
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            TriggerSearch(true);
        }

        private void lvResult_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < filteredEntries.Count)
            {
                var entry = filteredEntries[e.ItemIndex];
                var item = new ListViewItem();
                item.ImageIndex = ShellIconHelper.GetIconIndexForEntry(entry.Extension, entry.IsFolder);

                for (int i = 0; i < lvResult.Columns.Count; i++)
                {
                    FileColumn colType = (lvResult.Columns[i].Tag is FileColumn)
                        ? (FileColumn)lvResult.Columns[i].Tag
                        : FileColumn.Name;

                    string text = GetEntryColumnText(entry, colType);
                    if (i == 0)
                    {
                        item.Text = text;
                    }
                    else
                    {
                        item.SubItems.Add(text);
                    }
                }

                e.Item = item;
            }
            else
            {
                e.Item = new ListViewItem();
            }
        }

        private static string GetEntryColumnText(FileAndDirectoryEntry entry, FileColumn colType)
        {
            switch (colType)
            {
                case FileColumn.Name: return entry.FileName ?? string.Empty;
                case FileColumn.Path: return entry.Path ?? string.Empty;
                case FileColumn.Size: return entry.FormattedSize;
                case FileColumn.DateModified: return entry.FormattedDateModified;
                case FileColumn.DateCreated: return entry.FormattedDateCreated;
                case FileColumn.Extension: return entry.Extension ?? string.Empty;
                case FileColumn.Attributes: return entry.FormattedAttributes;
                default: return string.Empty;
            }
        }

        private void lvResult_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column >= 0 && e.Column < lvResult.Columns.Count)
            {
                var col = lvResult.Columns[e.Column];
                FileColumn clickedType = (col.Tag is FileColumn) ? (FileColumn)col.Tag : FileColumn.Name;

                isUserSorted = true;
                if (clickedType == activeSortColumn)
                {
                    sortOrder = (sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    activeSortColumn = clickedType;
                    sortOrder = SortOrder.Ascending;
                }

                UpdateSortArrow();
                ApplySortAsync();
            }
        }

        private void UpdateSortArrow()
        {
            if (!lvResult.IsHandleCreated) return;

            int visualColIdx = -1;
            if (isUserSorted)
            {
                for (int i = 0; i < lvResult.Columns.Count; i++)
                {
                    if (lvResult.Columns[i].Tag is FileColumn colType && colType == activeSortColumn)
                    {
                        visualColIdx = i;
                        break;
                    }
                }
            }

            ListViewSortIndicator.SetSortArrow(lvResult, visualColIdx, isUserSorted ? sortOrder : SortOrder.None);
        }

        private void ApplySortAsync()
        {
            if (filteredEntries == null || filteredEntries.Count <= 1) return;

            sortCts?.Cancel();
            sortCts = new CancellationTokenSource();
            var token = sortCts.Token;

            List<FileAndDirectoryEntry> targetList;
            lock (entriesLock)
            {
                if (object.ReferenceEquals(filteredEntries, allEntries))
                {
                    targetList = new List<FileAndDirectoryEntry>(allEntries);
                }
                else
                {
                    targetList = filteredEntries;
                }
            }

            FileColumn colType = activeSortColumn;
            int col = (int)colType;
            bool asc = (sortOrder == SortOrder.Ascending);
            string colName = colType.ToString();
            string dir = asc ? "▲" : "▼";

            statusMessage.Text = string.Format("Sorting {0:N0} objects by {1} {2}...", targetList.Count, colName, dir);
            this.Cursor = Cursors.AppStarting;

            Stopwatch sw = Stopwatch.StartNew();

            Task.Run(() =>
            {
                if (colType == FileColumn.Size)
                {
                    // For size sort, pre-populate sizes in parallel so sorting is 100% accurate!
                    int unindexed = 0;
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        var e = targetList[i];
                        if (e != null && !e.IsFolder && e.RawSize == -1) unindexed++;
                    }

                    if (unindexed > 0)
                    {
                        this.InvokeIfRequired(m => m.statusMessage.Text = string.Format("Indexing {0:N0} sizes...", unindexed));

                        Parallel.For(0, targetList.Count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, i =>
                        {
                            if (token.IsCancellationRequested) return;
                            var e = targetList[i];
                            if (e != null && !e.IsFolder && e.RawSize == -1)
                            {
                                FastFileInfo.PopulateFileInfo(e);
                            }
                        });
                    }

                    if (token.IsCancellationRequested) return;
                    this.InvokeIfRequired(m => m.statusMessage.Text = string.Format("Sorting {0:N0} objects by {1} {2}...", targetList.Count, colName, dir));
                }

                ParallelSorter.Sort(targetList, col, asc, token);
                sw.Stop();

                if (!token.IsCancellationRequested)
                {
                    this.InvokeIfRequired(m =>
                    {
                        m.filteredEntries = targetList;
                        m.lvResult.VirtualListSize = targetList.Count;
                        m.lvResult.Invalidate();
                        m.statusItemCount.Text = string.Format("{0:N0} objects", targetList.Count);
                        m.statusMessage.Text = "Ready";
                        m.statusQueryTime.Text = string.Format("{0:F3}s", sw.Elapsed.TotalSeconds);
                        m.Cursor = Cursors.Default;
                        m.UpdateSortArrow();
                    });

                    Task.Run(() =>
                    {
                        Thread.Sleep(150);
                        TrimProcessMemory();
                    });
                }
            }, token);
        }

        #region Column Customization & Header Context Menu

        private class HeaderSubclass : NativeWindow
        {
            private readonly MainForm form;

            public HeaderSubclass(MainForm form, IntPtr hWnd)
            {
                this.form = form;
                AssignHandle(hWnd);
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_CONTEXTMENU = 0x007B;
                const int WM_RBUTTONUP = 0x0205;

                if (m.Msg == WM_CONTEXTMENU || m.Msg == WM_RBUTTONUP)
                {
                    form.ShowColumnContextMenu(Cursor.Position);
                    return;
                }

                base.WndProc(ref m);
            }
        }

        private void AttachHeaderSubclass()
        {
            if (!lvResult.IsHandleCreated) return;
            IntPtr hHeader = ListViewSortIndicator.GetHeaderHandle(lvResult);
            if (hHeader != IntPtr.Zero)
            {
                if (headerSubclass == null || headerSubclass.Handle != hHeader)
                {
                    headerSubclass?.ReleaseHandle();
                    headerSubclass = new HeaderSubclass(this, hHeader);
                }
            }
        }

        private void muContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (ListViewSortIndicator.IsPointInHeader(lvResult, Cursor.Position))
            {
                e.Cancel = true;
                ShowColumnContextMenu(Cursor.Position);
                return;
            }

            if (lvResult.SelectedIndices.Count == 0)
            {
                e.Cancel = true;
                return;
            }
        }

        private readonly struct ColumnInfo
        {
            public readonly FileColumn Type;
            public readonly string Title;
            public readonly int DefaultWidth;
            public readonly HorizontalAlignment Alignment;

            public ColumnInfo(FileColumn type, string title, int defaultWidth, HorizontalAlignment alignment)
            {
                Type = type;
                Title = title;
                DefaultWidth = defaultWidth;
                Alignment = alignment;
            }
        }

        private static readonly ColumnInfo[] AllColumnInfos = new ColumnInfo[]
        {
            new ColumnInfo(FileColumn.Name, "Name", 240, HorizontalAlignment.Left),
            new ColumnInfo(FileColumn.Path, "Path", 360, HorizontalAlignment.Left),
            new ColumnInfo(FileColumn.Size, "Size", 90, HorizontalAlignment.Right),
            new ColumnInfo(FileColumn.DateModified, "Date Modified", 130, HorizontalAlignment.Left),
            new ColumnInfo(FileColumn.DateCreated, "Date Created", 130, HorizontalAlignment.Left),
            new ColumnInfo(FileColumn.Extension, "Extension", 75, HorizontalAlignment.Left),
            new ColumnInfo(FileColumn.Attributes, "Attributes", 75, HorizontalAlignment.Left),
        };

        private void InitializeColumns()
        {
            visibleColumns.Clear();

            string savedCols = AppSettings.Instance.VisibleColumns;
            if (string.IsNullOrWhiteSpace(savedCols))
            {
                savedCols = "Name,Path,Size,DateModified,Extension,Attributes";
            }

            string[] tokens = savedCols.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                FileColumn fc;
                if (Enum.TryParse<FileColumn>(token.Trim(), true, out fc))
                {
                    visibleColumns.Add(fc);
                }
            }

            visibleColumns.Add(FileColumn.Name);

            RebuildListViewColumns();
        }

        private void ToggleColumn(FileColumn colType)
        {
            if (colType == FileColumn.Name) return;

            if (visibleColumns.Contains(colType))
            {
                visibleColumns.Remove(colType);
            }
            else
            {
                visibleColumns.Add(colType);
            }

            RebuildListViewColumns();

            AppSettings.Instance.VisibleColumns = string.Join(",", visibleColumns);
            AppSettings.Save();
        }

        private void RebuildListViewColumns()
        {
            Dictionary<FileColumn, int> widths = new Dictionary<FileColumn, int>();
            foreach (ColumnHeader ch in lvResult.Columns)
            {
                if (ch.Tag is FileColumn fc)
                {
                    widths[fc] = ch.Width;
                }
            }

            lvResult.BeginUpdate();
            lvResult.Columns.Clear();

            foreach (var info in AllColumnInfos)
            {
                if (visibleColumns.Contains(info.Type))
                {
                    int width = widths.ContainsKey(info.Type) ? widths[info.Type] : info.DefaultWidth;
                    var header = new ColumnHeader
                    {
                        Text = info.Title,
                        Width = width,
                        TextAlign = info.Alignment,
                        Tag = info.Type
                    };
                    lvResult.Columns.Add(header);
                }
            }

            lvResult.EndUpdate();
            AttachHeaderSubclass();
            UpdateSortArrow();
            lvResult.Invalidate();
        }

        private void SizeAllColumnsToFit()
        {
            lvResult.BeginUpdate();
            foreach (ColumnHeader col in lvResult.Columns)
            {
                col.Width = -2;
                if (col.Width < 60) col.Width = 60;
            }
            lvResult.EndUpdate();
        }

        private void ShowColumnContextMenu(Point screenPoint)
        {
            if (columnContextMenu == null)
            {
                columnContextMenu = new ContextMenuStrip();
            }
            columnContextMenu.Items.Clear();

            AddColumnMenuItem(FileColumn.Name, "Name", true);
            AddColumnMenuItem(FileColumn.Path, "Path", false);
            AddColumnMenuItem(FileColumn.Size, "Size", false);
            AddColumnMenuItem(FileColumn.DateModified, "Date Modified", false);
            AddColumnMenuItem(FileColumn.DateCreated, "Date Created", false);
            AddColumnMenuItem(FileColumn.Extension, "Extension", false);
            AddColumnMenuItem(FileColumn.Attributes, "Attributes", false);

            columnContextMenu.Items.Add(new ToolStripSeparator());

            var itemSizeToFit = new ToolStripMenuItem("Size All Columns to Fit");
            itemSizeToFit.Click += (s, e) => SizeAllColumnsToFit();
            columnContextMenu.Items.Add(itemSizeToFit);

            columnContextMenu.Show(screenPoint);
        }

        private void AddColumnMenuItem(FileColumn colType, string title, bool isRequired)
        {
            var item = new ToolStripMenuItem(title)
            {
                Checked = visibleColumns.Contains(colType),
                Enabled = !isRequired
            };

            if (!isRequired)
            {
                item.Click += (s, e) =>
                {
                    ToggleColumn(colType);
                };
            }

            columnContextMenu.Items.Add(item);
        }

        #endregion

        private List<FileAndDirectoryEntry> GetSelectedEntries()
        {
            List<FileAndDirectoryEntry> selected = new List<FileAndDirectoryEntry>();
            foreach (int index in lvResult.SelectedIndices)
            {
                if (index >= 0 && index < filteredEntries.Count)
                {
                    selected.Add(filteredEntries[index]);
                }
            }
            return selected;
        }

        private void lvResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedCount = lvResult.SelectedIndices.Count;
            if (selectedCount > 1)
            {
                statusItemCount.Text = string.Format("{0:N0} objects ({1:N0} selected)", filteredEntries.Count, selectedCount);
            }
            else
            {
                statusItemCount.Text = string.Format("{0:N0} objects", filteredEntries.Count);
            }

            UpdateTitleWithSelection();
        }

        private void UpdateTitleWithSelection()
        {
            string adminTag = UserSecurityHelper.IsRunningAsAdministrator() ? " (Administrator)" : "";
            if (AppSettings.Instance.ShowFullPathInTitle && lvResult.SelectedIndices.Count == 1)
            {
                int idx = lvResult.SelectedIndices[0];
                if (idx >= 0 && idx < filteredEntries.Count)
                {
                    this.Text = filteredEntries[idx].FullFileName + " - EverythingEx" + adminTag;
                    return;
                }
            }
            this.Text = "EverythingEx" + adminTag;
        }

        private void menuToolsRestartAsAdmin_Click(object sender, EventArgs e)
        {
            UserSecurityHelper.RestartAsAdministrator();
        }

        private void lvResult_DoubleClick(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count > 0)
            {
                FileOperations.Open(selected[0]);
            }
        }

        private void lvResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (e.Control)
                {
                    menuFileOpenPath_Click(sender, e);
                }
                else
                {
                    menuFileOpen_Click(sender, e);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                ctxDelete_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                ctxRename_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Alt && e.KeyCode == Keys.Return)
            {
                ctxProperties_Click(sender, e);
                e.Handled = true;
            }
        }

        private void txtFilterString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lvResult.Items.Count > 0)
                {
                    lvResult.Focus();
                    if (lvResult.SelectedIndices.Count == 0)
                    {
                        lvResult.SelectedIndices.Add(0);
                    }
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                txtFilterString.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.E) || (e.Alt && e.KeyCode == Keys.D))
            {
                txtFilterString.Focus();
                txtFilterString.SelectAll();
                e.Handled = true;
            }
        }

        // --- Menu Event Handlers ---

        private void menuFileNewWindow_Click(object sender, EventArgs e)
        {
            var newWindow = new MainForm();
            newWindow.Show();
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            foreach (var item in selected)
            {
                FileOperations.Open(item);
            }
        }

        private void menuFileOpenPath_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count > 0)
            {
                FileOperations.OpenPath(selected[0]);
            }
        }

        private void menuFileShowInExplorer_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count > 0)
            {
                FileOperations.ShowInExplorer(selected[0]);
            }
        }

        private void menuFileOpenWith_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count > 0)
            {
                FileOperations.OpenWith(selected[0]);
            }
        }

        private void menuFileCopyFullName_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            FileOperations.CopyFullPaths(selected);
        }

        private void menuFileCopyPath_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            List<string> paths = selected.Select(s => s.Path).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
            if (paths.Count > 0)
            {
                Clipboard.SetText(string.Join(Environment.NewLine, paths));
            }
        }

        private void ctxCopyName_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            FileOperations.CopyNames(selected);
        }

        private void menuEditCopy_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            FileOperations.CopyFilesToClipboard(selected);
        }

        private void menuFileExport_Click(object sender, EventArgs e)
        {
            ExportHelper.Export(filteredEntries);
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            isExiting = true;
            this.Close();
        }

        private void menuEditSelectAll_Click(object sender, EventArgs e)
        {
            lvResult.SelectedIndices.Clear();
            for (int i = 0; i < filteredEntries.Count; i++)
            {
                lvResult.SelectedIndices.Add(i);
            }
        }

        private void menuEditInvertSelection_Click(object sender, EventArgs e)
        {
            var current = new HashSet<int>(lvResult.SelectedIndices.Cast<int>());
            lvResult.SelectedIndices.Clear();
            for (int i = 0; i < filteredEntries.Count; i++)
            {
                if (!current.Contains(i))
                {
                    lvResult.SelectedIndices.Add(i);
                }
            }
        }

        private void menuViewFilters_Click(object sender, EventArgs e)
        {
            pnlSearch.Visible = menuViewFilters.Checked;
        }

        private void menuViewStatusBar_Click(object sender, EventArgs e)
        {
            statusBar.Visible = menuViewStatusBar.Checked;
        }

        private void menuViewRefresh_Click(object sender, EventArgs e)
        {
            TriggerSearch(true);
        }

        private void menuViewZoomIn_Click(object sender, EventArgs e)
        {
            float newSize = lvResult.Font.Size + 1.0f;
            if (newSize <= 24.0f)
            {
                AppSettings.Instance.FontSize = newSize;
                AppSettings.Save();
                ApplyListFont();
            }
        }

        private void menuViewZoomOut_Click(object sender, EventArgs e)
        {
            float newSize = lvResult.Font.Size - 1.0f;
            if (newSize >= 7.0f)
            {
                AppSettings.Instance.FontSize = newSize;
                AppSettings.Save();
                ApplyListFont();
            }
        }

        private void menuViewZoomReset_Click(object sender, EventArgs e)
        {
            AppSettings.Instance.FontSize = 9.0f;
            AppSettings.Save();
            ApplyListFont();
        }

        private void menuSearchOption_Click(object sender, EventArgs e)
        {
            currentOptions.MatchCase = menuSearchMatchCase.Checked;
            currentOptions.MatchWholeWord = menuSearchMatchWholeWord.Checked;
            currentOptions.MatchPath = menuSearchMatchPath.Checked;
            currentOptions.EnableRegex = menuSearchEnableRegex.Checked;
            TriggerSearch(true);
        }

        private void menuSearchAdvanced_Click(object sender, EventArgs e)
        {
            using (var advForm = new AdvancedSearchForm())
            {
                if (advForm.ShowDialog() == DialogResult.OK)
                {
                    txtFilterString.Text = advForm.GeneratedQuery;
                    if (advForm.SelectedFilter != null)
                    {
                        for (int i = 0; i < cmbFilter.Items.Count; i++)
                        {
                            if (cmbFilter.Items[i].ToString() == advForm.SelectedFilter.Name)
                            {
                                cmbFilter.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    txtFilterString.Focus();
                }
            }
        }

        private void menuBookmarksAdd_Click(object sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(txtFilterString.Text) ? "All Files" : txtFilterString.Text;
            var item = new BookmarkItem(name, txtFilterString.Text, cmbFilter.SelectedItem?.ToString());
            using (var dlg = new BookmarkEditForm(item))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    BookmarkManager.Bookmarks.Add(dlg.Bookmark);
                    BookmarkManager.Save();
                    RefreshBookmarksMenu();
                }
            }
        }

        private void menuBookmarksOrganize_Click(object sender, EventArgs e)
        {
            using (var dlg = new BookmarksForm())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedBookmark != null)
                {
                    txtFilterString.Text = dlg.SelectedBookmark.Query ?? string.Empty;
                    for (int i = 0; i < cmbFilter.Items.Count; i++)
                    {
                        if (string.Equals(cmbFilter.Items[i].ToString(), dlg.SelectedBookmark.Filter, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbFilter.SelectedIndex = i;
                            break;
                        }
                    }
                }
                RefreshBookmarksMenu();
            }
        }

        private void menuToolsRescan_Click(object sender, EventArgs e)
        {
            LoadAllDrivesAsync(true);
        }

        private void menuToolsCompactDb_Click(object sender, EventArgs e)
        {
            if (allEntries == null || allEntries.Count == 0)
            {
                MessageBox.Show(this, "No index database loaded to compact.", "Compact Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            statusMessage.Text = "Compacting database...";
            Task.Run(() =>
            {
                long oldSize = IndexDatabase.GetDatabaseSize();
                Stopwatch sw = Stopwatch.StartNew();
                bool success = IndexDatabase.SaveToFile(allEntries);
                sw.Stop();
                long newSize = IndexDatabase.GetDatabaseSize();

                this.InvokeIfRequired(m =>
                {
                    if (success)
                    {
                        double oldMb = oldSize / (1024.0 * 1024.0);
                        double newMb = newSize / (1024.0 * 1024.0);
                        double reduction = (1.0 - (double)newSize / Math.Max(1, oldSize)) * 100.0;
                        string msg = string.Format("Database compacted in {0}ms: {1:F1} MB -> {2:F1} MB ({3:F1}% smaller)",
                            sw.ElapsedMilliseconds, oldMb, newMb, reduction);
                        m.statusMessage.Text = msg;
                        MessageBox.Show(this, msg, "Compact Database Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        m.statusMessage.Text = "Failed to compact database.";
                    }
                });
            });
        }

        private void menuToolsOptions_Click(object sender, EventArgs e)
        {
            using (var opt = new OptionsForm())
            {
                if (opt.ShowDialog() == DialogResult.OK)
                {
                    ApplyAppSettings();
                    ApplyListFont();
                }
            }
        }

        private void menuToolsWebsite_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://www.voidtools.com/");
            }
            catch { }
        }

        private void menuHelpSearchSyntax_Click(object sender, EventArgs e)
        {
            var syntaxForm = new SearchSyntaxForm();
            syntaxForm.Show(this);
        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "EverythingEx v1.5\n\n" +
                "High-performance Windows Desktop Search tool imitating voidtools Everything.\n\n" +
                "Features:\n" +
                "- Instant NTFS USN Journal and recursive drive indexing\n" +
                "- Virtual mode 60fps ListView supporting 1,000,000+ files\n" +
                "- Full Everything search syntax (operators, wildcards, regex, functions, macros)\n" +
                "- Real-time file system monitoring and indexing\n" +
                "- Windows Explorer integration and native context menu\n" +
                "- Export to CSV, TXT, EFU",
                "About EverythingEx", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (FileOperations.DeleteToRecycleBin(selected, this.Handle))
            {
                lock (entriesLock)
                {
                    var selectedSet = new HashSet<FileAndDirectoryEntry>(selected);
                    allEntries.RemoveAll(item => selectedSet.Contains(item));
                }
                TriggerSearch(true);
            }
        }

        private void ctxRename_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count == 1)
            {
                var entry = selected[0];
                string currentName = entry.FileName;
                string newName = InputBoxDialog.Show("Enter new name for the item:", "Rename", currentName);
                if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
                {
                    try
                    {
                        string newFullPath = Path.Combine(entry.Path, newName);
                        if (entry.IsFolder)
                        {
                            Directory.Move(entry.FullFileName, newFullPath);
                        }
                        else
                        {
                            File.Move(entry.FullFileName, newFullPath);
                        }

                        entry.FileName = newName;
                        lvResult.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to rename:\n" + ex.Message, "Rename Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ctxProperties_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedEntries();
            if (selected.Count > 0)
            {
                FileOperations.ShowProperties(selected[0], this.Handle);
            }
        }

        // --- System Tray ---

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            trayOpen_Click(sender, e);
        }

        private void trayOpen_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void StartBackgroundSizePopulator()
        {
            Task.Run(() =>
            {
                var list = allEntries;
                if (list == null || list.Count == 0) return;

                // Quick check: does any file actually need size population?
                // When loaded from Everything.db or NTFS scan, all file sizes and attributes are already indexed.
                bool hasUnpopulated = false;
                for (int i = 0; i < Math.Min(list.Count, 5000); i++)
                {
                    var e = list[i];
                    if (e != null && !e.IsFolder && !e.IsDeleted && e.RawSize == -1)
                    {
                        hasUnpopulated = true;
                        break;
                    }
                }

                if (!hasUnpopulated) return;

                int batchSize = 10000;
                int total = list.Count;
                int batches = (total + batchSize - 1) / batchSize;
                bool anyPopulated = false;

                for (int b = 0; b < batches; b++)
                {
                    if (isExiting) break;
                    int start = b * batchSize;
                    int end = Math.Min(start + batchSize, total);

                    Parallel.For(start, end, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                    {
                        if (isExiting) return;
                        var entry = list[i];
                        if (entry != null && !entry.IsFolder && !entry.IsDeleted && entry.RawSize == -1)
                        {
                            FastFileInfo.PopulateFileInfo(entry);
                            anyPopulated = true;
                        }
                    });

                    Thread.Sleep(15);
                }

                if (!isExiting && anyPopulated)
                {
                    IndexDatabase.SaveToFile(list);
                }
            });
        }

        private void trayExit_Click(object sender, EventArgs e)
        {
            isExiting = true;
            notifyIcon.Visible = false;
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isExiting && e.CloseReason == CloseReason.UserClosing && AppSettings.Instance.MinimizeToTray)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            isExiting = true;
            searchCts?.Cancel();
            sortCts?.Cancel();
            loadCts?.Cancel();
            realTimeRefreshTimer?.Stop();
            dbAutoSaveTimer?.Stop();

            try { indexWatcher?.Dispose(); } catch { }
            try { notifyIcon.Visible = false; notifyIcon.Dispose(); } catch { }

            lock (entriesLock)
            {
                if (allEntries != null && allEntries.Count > 0)
                {
                    try
                    {
                        IndexDatabase.SaveToFile(allEntries);
                    }
                    catch { }
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
