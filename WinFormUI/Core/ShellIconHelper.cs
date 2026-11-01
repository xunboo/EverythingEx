using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormUI.Core
{
    public static class ShellIconHelper
    {
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static readonly ConcurrentDictionary<string, int> iconIndexCache = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public static ImageList SmallImageList { get; } = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };

        public const int FolderIconIndex = 0;
        public const int DefaultFileIconIndex = 1;

        static ShellIconHelper()
        {
            // Initialize default folder and file icons
            Icon folderIcon = GetFolderIcon();
            SmallImageList.Images.Add("folder", folderIcon ?? SystemIcons.Application);

            Icon fileIcon = GetFileIcon(".txt");
            SmallImageList.Images.Add("file", fileIcon ?? SystemIcons.Application);
        }

        public static int GetIconIndexForEntry(string extension, bool isFolder)
        {
            if (isFolder)
            {
                return FolderIconIndex;
            }

            if (string.IsNullOrEmpty(extension))
            {
                return DefaultFileIconIndex;
            }

            int index;
            if (iconIndexCache.TryGetValue(extension, out index))
            {
                return index;
            }

            try
            {
                Icon icon = GetFileIcon(extension);
                if (icon != null)
                {
                    lock (SmallImageList)
                    {
                        SmallImageList.Images.Add(extension, icon);
                        index = SmallImageList.Images.Count - 1;
                        iconIndexCache[extension] = index;
                        return index;
                    }
                }
            }
            catch { }

            return DefaultFileIconIndex;
        }

        private static Icon GetFolderIcon()
        {
            var shinfo = new SHFILEINFO();
            IntPtr ptr = SHGetFileInfo("dummy", FILE_ATTRIBUTE_DIRECTORY, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

            if (ptr != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }
            return null;
        }

        private static Icon GetFileIcon(string extension)
        {
            var shinfo = new SHFILEINFO();
            IntPtr ptr = SHGetFileInfo("dummy" + extension, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);

            if (ptr != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }
            return null;
        }
    }
}
