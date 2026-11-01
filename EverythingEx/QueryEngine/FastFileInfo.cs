using System;
using System.Runtime.InteropServices;

namespace QueryEngine
{
    public static class FastFileInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public uint dwFileAttributes;
            public uint ftCreationTimeLow;
            public uint ftCreationTimeHigh;
            public uint ftLastAccessTimeLow;
            public uint ftLastAccessTimeHigh;
            public uint ftLastWriteTimeLow;
            public uint ftLastWriteTimeHigh;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetFileAttributesEx(string lpFileName, int fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        /// <summary>
        /// Quickly gets file size using native Win32 GetFileAttributesEx without opening a file handle.
        /// Returns -1 for directories, -2 if inaccessible or missing.
        /// </summary>
        public static long GetFileSize(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return -2;

            WIN32_FILE_ATTRIBUTE_DATA data;
            if (GetFileAttributesEx(fullPath, 0, out data))
            {
                if ((data.dwFileAttributes & 0x00000010) != 0)
                {
                    return -1; // Directory
                }
                return ((long)data.nFileSizeHigh << 32) | (long)data.nFileSizeLow;
            }
            return -2; // Inaccessible or missing
        }

        /// <summary>
        /// Checks whether the file or directory actually exists on disk.
        /// If deleted (ERROR_FILE_NOT_FOUND or ERROR_PATH_NOT_FOUND), marks entry.IsDeleted = true.
        /// </summary>
        public static bool Exists(FileAndDirectoryEntry entry)
        {
            if (entry == null || entry.IsDeleted) return false;

            string fullPath = entry.FullFileName;
            if (string.IsNullOrEmpty(fullPath)) return false;

            WIN32_FILE_ATTRIBUTE_DATA data;
            if (GetFileAttributesEx(fullPath, 0, out data))
            {
                return true;
            }

            int err = Marshal.GetLastWin32Error();
            if (err == 2 || err == 3) // ERROR_FILE_NOT_FOUND (2) or ERROR_PATH_NOT_FOUND (3)
            {
                entry.IsDeleted = true;
                return false;
            }

            return true; // Access denied or drive busy, treat as existing
        }

        /// <summary>
        /// Populates file size, date modified, and attributes in one fast Win32 call.
        /// If the file or directory has been deleted, marks entry.IsDeleted = true.
        /// </summary>
        public static bool PopulateFileInfo(FileAndDirectoryEntry entry)
        {
            if (entry == null || entry.IsDeleted) return false;

            string fullPath = entry.FullFileName;
            if (string.IsNullOrEmpty(fullPath)) return false;

            WIN32_FILE_ATTRIBUTE_DATA data;
            if (GetFileAttributesEx(fullPath, 0, out data))
            {
                if ((data.dwFileAttributes & 0x00000010) == 0)
                {
                    entry.Size = ((long)data.nFileSizeHigh << 32) | (long)data.nFileSizeLow;
                }
                else
                {
                    entry.Size = -1;
                    entry.IsFolder = true;
                }

                if (entry.DateModified == DateTime.MinValue || entry.DateModified.Year < 1980)
                {
                    long writeTicks = ((long)data.ftLastWriteTimeHigh << 32) | (long)data.ftLastWriteTimeLow;
                    if (writeTicks > 0)
                    {
                        entry.DateModified = DateTime.FromFileTimeUtc(writeTicks).ToLocalTime();
                    }
                }

                return true;
            }

            int err = Marshal.GetLastWin32Error();
            if (err == 2 || err == 3) // File or directory no longer exists on disk
            {
                entry.IsDeleted = true;
                return false;
            }

            entry.Size = -2; // Mark as checked and inaccessible so we don't query repeatedly
            return false;
        }
    }
}
