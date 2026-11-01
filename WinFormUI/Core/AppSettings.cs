using System;
using System.IO;

namespace WinFormUI.Core
{
    public class AppSettings
    {
        public bool MinimizeToTray { get; set; } = false;
        public bool ShowTrayIcon { get; set; } = true;
        public bool RealTimeMonitoring { get; set; } = true;
        public bool ShowFullPathInTitle { get; set; } = true;
        public float FontSize { get; set; } = 9.0f;
        public string VisibleColumns { get; set; } = "Name,Path,Size,DateModified,Extension,Attributes";

        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EverythingEx", "settings.ini");

        public static AppSettings Instance { get; set; } = new AppSettings();

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string[] lines = File.ReadAllLines(SettingsFile);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();
                            if (key == "MinimizeToTray") Instance.MinimizeToTray = bool.Parse(val);
                            if (key == "ShowTrayIcon") Instance.ShowTrayIcon = bool.Parse(val);
                            if (key == "RealTimeMonitoring") Instance.RealTimeMonitoring = bool.Parse(val);
                            if (key == "ShowFullPathInTitle") Instance.ShowFullPathInTitle = bool.Parse(val);
                            if (key == "FontSize") Instance.FontSize = float.Parse(val);
                            if (key == "VisibleColumns") Instance.VisibleColumns = val;
                        }
                    }
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

                string[] lines = new string[]
                {
                    "MinimizeToTray=" + Instance.MinimizeToTray,
                    "ShowTrayIcon=" + Instance.ShowTrayIcon,
                    "RealTimeMonitoring=" + Instance.RealTimeMonitoring,
                    "ShowFullPathInTitle=" + Instance.ShowFullPathInTitle,
                    "FontSize=" + Instance.FontSize,
                    "VisibleColumns=" + Instance.VisibleColumns
                };

                File.WriteAllLines(SettingsFile, lines);
            }
            catch { }
        }
    }
}
