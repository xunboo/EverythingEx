using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QueryEngine;

namespace WinFormUI.Core
{
    public static class FileOperations
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 0x0000000C;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private const int FO_DELETE = 0x0003;
        private const short FOF_ALLOWUNDO = 0x0040;
        private const short FOF_NOCONFIRMATION = 0x0010;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        public static void Open(FileAndDirectoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.FullFileName)) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = entry.FullFileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to open '{0}':\n{1}", entry.FullFileName, ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void OpenPath(FileAndDirectoryEntry entry)
        {
            if (entry == null) return;
            string targetPath = entry.IsFolder ? entry.FullFileName : entry.Path;
            if (string.IsNullOrEmpty(targetPath)) return;

            try
            {
                if (Directory.Exists(targetPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = targetPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to open path '{0}':\n{1}", targetPath, ex.Message),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ShowInExplorer(FileAndDirectoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.FullFileName)) return;

            try
            {
                if (File.Exists(entry.FullFileName) || Directory.Exists(entry.FullFileName))
                {
                    ShowSelectedInExplorer.FileOrFolder(entry.FullFileName, false);
                }
                else if (Directory.Exists(entry.Path))
                {
                    Process.Start("explorer.exe", entry.Path);
                }
            }
            catch
            {
                try
                {
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", entry.FullFileName));
                }
                catch { }
            }
        }

        public static void OpenWith(FileAndDirectoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.FullFileName)) return;

            try
            {
                Process.Start("rundll32.exe", string.Format("shell32.dll,OpenAs_RunDLL \"{0}\"", entry.FullFileName));
            }
            catch { }
        }

        public static void ShowProperties(FileAndDirectoryEntry entry, IntPtr hwnd)
        {
            if (entry == null || string.IsNullOrEmpty(entry.FullFileName)) return;

            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = entry.FullFileName;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            info.hwnd = hwnd;

            ShellExecuteEx(ref info);
        }

        public static void CopyFilesToClipboard(IList<FileAndDirectoryEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            StringCollection fileCollection = new StringCollection();
            foreach (var item in entries)
            {
                if (!string.IsNullOrEmpty(item.FullFileName))
                {
                    fileCollection.Add(item.FullFileName);
                }
            }

            if (fileCollection.Count > 0)
            {
                try
                {
                    Clipboard.SetFileDropList(fileCollection);
                }
                catch { }
            }
        }

        public static void CopyFullPaths(IList<FileAndDirectoryEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            List<string> paths = new List<string>();
            foreach (var item in entries)
            {
                if (!string.IsNullOrEmpty(item.FullFileName))
                {
                    paths.Add(item.FullFileName);
                }
            }

            if (paths.Count > 0)
            {
                try
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, paths));
                }
                catch { }
            }
        }

        public static void CopyNames(IList<FileAndDirectoryEntry> entries)
        {
            if (entries == null || entries.Count == 0) return;

            List<string> names = new List<string>();
            foreach (var item in entries)
            {
                if (!string.IsNullOrEmpty(item.FileName))
                {
                    names.Add(item.FileName);
                }
            }

            if (names.Count > 0)
            {
                try
                {
                    Clipboard.SetText(string.Join(Environment.NewLine, names));
                }
                catch { }
            }
        }

        public static bool DeleteToRecycleBin(IList<FileAndDirectoryEntry> entries, IntPtr hwnd)
        {
            if (entries == null || entries.Count == 0) return false;

            var result = MessageBox.Show(
                string.Format("Are you sure you want to send {0} item(s) to the Recycle Bin?", entries.Count),
                "Delete Multiple Items", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return false;

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.FullFileName)) continue;

                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();
                shf.wFunc = FO_DELETE;
                shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
                shf.pFrom = entry.FullFileName + '\0' + '\0';
                shf.hwnd = hwnd;

                SHFileOperation(ref shf);
            }

            return true;
        }
    }
}
