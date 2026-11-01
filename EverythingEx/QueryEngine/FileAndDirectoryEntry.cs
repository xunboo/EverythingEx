using System;
using System.IO;
using UsnOperation;

namespace QueryEngine
{
    public class FileAndDirectoryEntry
    {
        public UInt64 FileReferenceNumber { get; set; }

        public UInt64 ParentFileReferenceNumber { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public bool IsFolder { get; set; }

        public uint FileAttributes { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateCreated { get; set; }

        public volatile bool IsDeleted;

        private long size = -1;

        public long RawSize => size;

        public long Size
        {
            get
            {
                if (size == -1 && !IsFolder)
                {
                    FastFileInfo.PopulateFileInfo(this);
                }
                return size;
            }
            set => size = value;
        }

        public string FullFileName
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                {
                    return FileName ?? string.Empty;
                }
                if (Path.EndsWith("\\"))
                {
                    return string.Concat(Path, FileName);
                }
                return string.Concat(Path, "\\", FileName);
            }
        }

        public string Extension
        {
            get
            {
                if (IsFolder)
                {
                    return string.Empty;
                }
                int dot = FileName != null ? FileName.LastIndexOf('.') : -1;
                return (dot >= 0 && dot < FileName.Length - 1) ? FileName.Substring(dot).ToLowerInvariant() : string.Empty;
            }
        }

        public string FormattedSize
        {
            get
            {
                if (IsFolder)
                {
                    return string.Empty;
                }

                if (size == -1)
                {
                    FastFileInfo.PopulateFileInfo(this);
                }

                if (size < 0)
                {
                    return string.Empty;
                }
                if (size == 0)
                {
                    return "0 KB";
                }
                if (size < 1024)
                {
                    return "1 KB";
                }
                if (size < 1024 * 1024)
                {
                    return string.Format("{0:N0} KB", Math.Ceiling(size / 1024.0));
                }
                if (size < 1024 * 1024 * 1024)
                {
                    return string.Format("{0:N1} MB", size / (1024.0 * 1024.0));
                }
                return string.Format("{0:N2} GB", size / (1024.0 * 1024.0 * 1024.0));
            }
        }

        public string FormattedDateModified
        {
            get
            {
                if (DateModified == DateTime.MinValue || DateModified.Year < 1980)
                {
                    return string.Empty;
                }
                return DateModified.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public string FormattedDateCreated
        {
            get
            {
                if (DateCreated == DateTime.MinValue || DateCreated.Year < 1980)
                {
                    return string.Empty;
                }
                return DateCreated.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public string FormattedAttributes
        {
            get
            {
                char[] attr = new char[5];
                int idx = 0;
                if (IsFolder) attr[idx++] = 'D';
                if ((FileAttributes & 0x01) != 0) attr[idx++] = 'R';
                if ((FileAttributes & 0x02) != 0) attr[idx++] = 'H';
                if ((FileAttributes & 0x04) != 0) attr[idx++] = 'S';
                if ((FileAttributes & 0x20) != 0) attr[idx++] = 'A';
                return new string(attr, 0, idx);
            }
        }

        public FileAndDirectoryEntry()
        {
        }

        public FileAndDirectoryEntry(UsnEntry usnEntry, string path)
        {
            this.FileReferenceNumber = usnEntry.FileReferenceNumber;
            this.ParentFileReferenceNumber = usnEntry.ParentFileReferenceNumber;
            this.FileName = usnEntry.FileName;
            this.IsFolder = usnEntry.IsFolder;
            this.FileAttributes = usnEntry.FileAttributes;
            this.DateModified = usnEntry.TimeStamp != DateTime.MinValue ? usnEntry.TimeStamp : DateTime.Now;
            this.DateCreated = usnEntry.TimeStamp;
            this.Path = path;
            this.Size = -1;
        }

        public FileAndDirectoryEntry(string fullPath, bool isFolder, long size = 0, DateTime? modified = null, DateTime? created = null, uint attributes = 0)
        {
            this.IsFolder = isFolder;
            this.Size = isFolder ? -1 : size;
            this.DateModified = modified ?? DateTime.Now;
            this.DateCreated = created ?? DateTime.Now;
            this.FileAttributes = attributes;

            try
            {
                this.FileName = System.IO.Path.GetFileName(fullPath);
                this.Path = System.IO.Path.GetDirectoryName(fullPath) ?? string.Empty;
                if (string.IsNullOrEmpty(this.FileName) && fullPath.EndsWith(":\\"))
                {
                    this.FileName = fullPath;
                    this.Path = string.Empty;
                }
            }
            catch
            {
                this.FileName = fullPath;
                this.Path = string.Empty;
            }
        }
    }
}
