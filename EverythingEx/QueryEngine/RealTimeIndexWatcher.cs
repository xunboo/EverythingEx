using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QueryEngine
{
    public class RealTimeIndexWatcher : IDisposable
    {
        private readonly List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private readonly List<FileAndDirectoryEntry> entries;
        private readonly object entriesLock;
        private readonly Dictionary<string, FileAndDirectoryEntry> pathLookup;

        public event EventHandler IndexChanged;

        public RealTimeIndexWatcher(List<FileAndDirectoryEntry> sharedEntries, object syncLock)
        {
            this.entries = sharedEntries;
            this.entriesLock = syncLock;
            // On-demand lookup table for dynamically created / modified items during the session.
            // Does NOT pre-allocate 2.9 million full path strings, saving nearly 1 GB of RAM!
            this.pathLookup = new Dictionary<string, FileAndDirectoryEntry>(1024, StringComparer.OrdinalIgnoreCase);
        }

        public void StartWatchingDrives(IEnumerable<DriveInfo> drives)
        {
            StopWatching();

            foreach (var drive in drives)
            {
                if (!drive.IsReady) continue;

                try
                {
                    var watcher = new FileSystemWatcher(drive.RootDirectory.FullName)
                    {
                        IncludeSubdirectories = true,
                        InternalBufferSize = 65536, // 64 KB (Windows max) to avoid buffer overflow
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                                       NotifyFilters.LastWrite | NotifyFilters.Size
                    };

                    watcher.Created += OnFileCreated;
                    watcher.Deleted += OnFileDeleted;
                    watcher.Renamed += OnFileRenamed;
                    watcher.Changed += OnFileChanged;
                    watcher.Error += (s, e) =>
                    {
                        try
                        {
                            var w = (FileSystemWatcher)s;
                            w.EnableRaisingEvents = false;
                            w.EnableRaisingEvents = true;
                        }
                        catch { }
                    };

                    watcher.EnableRaisingEvents = true;
                    this.watchers.Add(watcher);
                }
                catch
                {
                    // Some drives might not support FileSystemWatcher
                }
            }
        }

        private static bool ShouldIgnore(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return true;
            if (fullPath.IndexOf("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (fullPath.IndexOf("System Volume Information", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private FileAndDirectoryEntry FindEntry(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            lock (entriesLock)
            {
                if (pathLookup.TryGetValue(fullPath, out var found) && !found.IsDeleted)
                {
                    return found;
                }

                string fileName = Path.GetFileName(fullPath);
                string dir = Path.GetDirectoryName(fullPath) ?? string.Empty;

                // Scan backwards: newly created or recently added files are near the end
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    var item = entries[i];
                    if (item != null && !item.IsDeleted &&
                        string.Equals(item.FileName, fileName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(item.Path, dir, StringComparison.OrdinalIgnoreCase))
                    {
                        pathLookup[fullPath] = item;
                        return item;
                    }
                }
            }
            return null;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnore(e.FullPath)) return;

            try
            {
                bool isDir = Directory.Exists(e.FullPath);
                long size = 0;
                if (!isDir && File.Exists(e.FullPath))
                {
                    size = FastFileInfo.GetFileSize(e.FullPath);
                }

                lock (entriesLock)
                {
                    var existing = FindEntry(e.FullPath);
                    if (existing != null)
                    {
                        existing.IsDeleted = false;
                        if (!isDir)
                        {
                            FastFileInfo.PopulateFileInfo(existing);
                        }
                    }
                    else
                    {
                        var newEntry = new FileAndDirectoryEntry(e.FullPath, isDir, size);
                        entries.Add(newEntry);
                        pathLookup[e.FullPath] = newEntry;
                    }
                }

                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnore(e.FullPath)) return;

            try
            {
                lock (entriesLock)
                {
                    var found = FindEntry(e.FullPath);
                    if (found != null)
                    {
                        found.IsDeleted = true;
                        pathLookup.Remove(e.FullPath);
                        entries.Remove(found);

                        // If it's a directory, also delete all indexed children
                        if (found.IsFolder)
                        {
                            string folderPrefix = e.FullPath.TrimEnd('\\') + "\\";
                            for (int i = entries.Count - 1; i >= 0; i--)
                            {
                                var child = entries[i];
                                if (child != null && !child.IsDeleted && child.Path != null &&
                                    (child.Path.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase) ||
                                     child.Path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase)))
                                {
                                    child.IsDeleted = true;
                                    pathLookup.Remove(child.FullFileName);
                                    entries.RemoveAt(i);
                                }
                            }
                        }
                    }
                }

                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (ShouldIgnore(e.FullPath) && ShouldIgnore(e.OldFullPath)) return;

            try
            {
                lock (entriesLock)
                {
                    var found = FindEntry(e.OldFullPath);
                    if (found != null)
                    {
                        pathLookup.Remove(e.OldFullPath);
                        found.FileName = Path.GetFileName(e.FullPath);
                        found.Path = Path.GetDirectoryName(e.FullPath) ?? string.Empty;
                        pathLookup[e.FullPath] = found;

                        // If it's a directory, also update paths of all children
                        if (found.IsFolder)
                        {
                            string oldPrefix = e.OldFullPath.TrimEnd('\\') + "\\";
                            string newPrefix = e.FullPath.TrimEnd('\\') + "\\";
                            for (int i = 0; i < entries.Count; i++)
                            {
                                var child = entries[i];
                                if (child != null && !child.IsDeleted && child.Path != null &&
                                    child.Path.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    pathLookup.Remove(child.FullFileName);
                                    child.Path = newPrefix + child.Path.Substring(oldPrefix.Length);
                                    pathLookup[child.FullFileName] = child;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isDir = Directory.Exists(e.FullPath);
                        long size = isDir ? -1 : FastFileInfo.GetFileSize(e.FullPath);
                        var newEntry = new FileAndDirectoryEntry(e.FullPath, isDir, size);
                        entries.Add(newEntry);
                        pathLookup[e.FullPath] = newEntry;
                    }
                }

                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnore(e.FullPath)) return;

            try
            {
                lock (entriesLock)
                {
                    var found = FindEntry(e.FullPath);
                    if (found != null)
                    {
                        if (!found.IsFolder)
                        {
                            FastFileInfo.PopulateFileInfo(found);
                        }
                    }
                    else
                    {
                        bool isDir = Directory.Exists(e.FullPath);
                        long size = isDir ? -1 : FastFileInfo.GetFileSize(e.FullPath);
                        var newEntry = new FileAndDirectoryEntry(e.FullPath, isDir, size);
                        entries.Add(newEntry);
                        pathLookup[e.FullPath] = newEntry;
                    }
                }

                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        public void StopWatching()
        {
            foreach (var watcher in watchers)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                catch { }
            }
            watchers.Clear();
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}
