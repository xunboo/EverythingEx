using System;
using System.Collections.Generic;
using System.IO;

namespace WinFormUI.Core
{
    [Serializable]
    public class BookmarkItem
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public string Filter { get; set; }

        public BookmarkItem() { }

        public BookmarkItem(string name, string query, string filter)
        {
            this.Name = name;
            this.Query = query;
            this.Filter = filter;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class BookmarkManager
    {
        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EverythingEx", "bookmarks.txt");

        public static List<BookmarkItem> Bookmarks { get; } = new List<BookmarkItem>();

        static BookmarkManager()
        {
            Load();
        }

        public static void Load()
        {
            try
            {
                Bookmarks.Clear();
                if (File.Exists(SettingsFile))
                {
                    string[] lines = File.ReadAllLines(SettingsFile);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split('\t');
                        if (parts.Length >= 2)
                        {
                            string name = parts[0];
                            string query = parts[1];
                            string filter = parts.Length > 2 ? parts[2] : "Everything";
                            Bookmarks.Add(new BookmarkItem(name, query, filter));
                        }
                    }
                }
                else
                {
                    // Add some useful default bookmarks
                    Bookmarks.Add(new BookmarkItem("Recent Downloads", "parent:Downloads dm:today", "Everything"));
                    Bookmarks.Add(new BookmarkItem("Large Files (>100MB)", "size:>100mb", "Everything"));
                    Bookmarks.Add(new BookmarkItem("Temporary Files", "*.tmp | *.bak | *.log", "Everything"));
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                List<string> lines = new List<string>();
                foreach (var b in Bookmarks)
                {
                    lines.Add(string.Format("{0}\t{1}\t{2}", b.Name, b.Query, b.Filter));
                }

                File.WriteAllLines(SettingsFile, lines);
            }
            catch { }
        }
    }
}
