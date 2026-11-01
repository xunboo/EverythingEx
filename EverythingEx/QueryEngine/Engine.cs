using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UsnOperation;

namespace QueryEngine
{
    public class Engine
    {
        /// <summary>
        /// When its values is 1407374883553285(0x5000000000005L), it means this file/folder is under drive root
        /// </summary>
        protected const UInt64 ROOT_FILE_REFERENCE_NUMBER = 0x5000000000005L;

        protected static readonly HashSet<string> ExcludeFolderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            "$AttrDef",
            "$BadClus",
            "$BitMap",
            "$Boot",
            "$LogFile",
            "$Mft",
            "$MftMirr",
            "$Secure",
            "$TxfLog",
            "$UpCase",
            "$Volume",
            "$Extend"
        };

        public static IEnumerable<DriveInfo> GetAllAvailableDrives()
        {
            return DriveInfo.GetDrives().Where(d => d.IsReady);
        }

        public static List<FileAndDirectoryEntry> GetAllFilesAndDirectories(
            IProgress<string> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            List<FileAndDirectoryEntry> result = new List<FileAndDirectoryEntry>();

            var drives = GetAllAvailableDrives();

            foreach (var drive in drives)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (progress != null)
                {
                    progress.Report(string.Format("Scanning drive {0}...", drive.Name));
                }

                List<FileAndDirectoryEntry> driveEntries = GetDriveFilesAndDirectories(drive, progress, cancellationToken);
                result.AddRange(driveEntries);
            }

            return result;
        }

        public static List<FileAndDirectoryEntry> GetDriveFilesAndDirectories(
            DriveInfo drive,
            IProgress<string> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            List<FileAndDirectoryEntry> result = new List<FileAndDirectoryEntry>();

            bool usnSucceeded = false;

            if (drive.DriveType == DriveType.Fixed && string.Equals(drive.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var usnOperator = new UsnOperator(drive))
                    {
                        var usnEntries = usnOperator.GetEntries()
                            .Where(e => !ExcludeFolderNames.Contains(e.FileName))
                            .ToList();

                        if (usnEntries.Count > 0)
                        {
                            var folders = usnEntries.Where(e => e.IsFolder).ToArray();
                            List<FrnFilePath> paths = GetFolderPath(folders, drive);

                            var joined = usnEntries.Join(
                                paths,
                                usn => usn.ParentFileReferenceNumber,
                                path => path.FileReferenceNumber,
                                (usn, path) => new FileAndDirectoryEntry(usn, path.Path));

                            result.AddRange(joined);
                            usnSucceeded = true;
                        }
                    }
                }
                catch (Exception)
                {
                    usnSucceeded = false;
                }
            }

            // Fallback for non-NTFS or if USN access is denied (non-admin)
            if (!usnSucceeded)
            {
                result.AddRange(EnumerateDriveFallback(drive, progress, cancellationToken));
            }

            return result;
        }

        private static List<FileAndDirectoryEntry> EnumerateDriveFallback(
            DriveInfo drive,
            IProgress<string> progress,
            CancellationToken cancellationToken)
        {
            List<FileAndDirectoryEntry> entries = new List<FileAndDirectoryEntry>();
            Queue<string> pendingDirs = new Queue<string>();
            pendingDirs.Enqueue(drive.RootDirectory.FullName);

            int count = 0;

            while (pendingDirs.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string currentDir = pendingDirs.Dequeue();

                try
                {
                    var dirInfo = new DirectoryInfo(currentDir);
                    if (ExcludeFolderNames.Contains(dirInfo.Name))
                    {
                        continue;
                    }

                    // Enumerate directories
                    DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                    foreach (var subDir in subDirs)
                    {
                        if ((subDir.Attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            continue; // avoid symlink loops
                        }

                        if (!ExcludeFolderNames.Contains(subDir.Name))
                        {
                            entries.Add(new FileAndDirectoryEntry
                            {
                                FileName = subDir.Name,
                                Path = currentDir,
                                IsFolder = true,
                                Size = -1,
                                DateModified = subDir.LastWriteTime,
                                DateCreated = subDir.CreationTime,
                                FileAttributes = (uint)subDir.Attributes
                            });

                            pendingDirs.Enqueue(subDir.FullName);

                            count++;
                            if (count % 10000 == 0 && progress != null)
                            {
                                progress.Report(string.Format("Indexed {0:N0} items on {1}...", count, drive.Name));
                            }
                        }
                    }

                    // Enumerate files
                    FileInfo[] files = dirInfo.GetFiles();
                    foreach (var file in files)
                    {
                        entries.Add(new FileAndDirectoryEntry
                        {
                            FileName = file.Name,
                            Path = currentDir,
                            IsFolder = false,
                            Size = file.Length,
                            DateModified = file.LastWriteTime,
                            DateCreated = file.CreationTime,
                            FileAttributes = (uint)file.Attributes
                        });

                        count++;
                        if (count % 10000 == 0 && progress != null)
                        {
                            progress.Report(string.Format("Indexed {0:N0} items on {1}...", count, drive.Name));
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip restricted folders
                }
                catch (PathTooLongException)
                {
                    // Skip paths exceeding max length
                }
                catch (Exception)
                {
                    // Ignore other I/O errors gracefully
                }
            }

            return entries;
        }

        private static List<FrnFilePath> GetFolderPath(UsnEntry[] folders, DriveInfo drive)
        {
            Dictionary<UInt64, FrnFilePath> pathDic = new Dictionary<ulong, FrnFilePath>(folders.Length + 1);
            pathDic.Add(ROOT_FILE_REFERENCE_NUMBER,
                new FrnFilePath(ROOT_FILE_REFERENCE_NUMBER, null, string.Empty, drive.Name.TrimEnd('\\')));

            foreach (var folder in folders)
            {
                if (!pathDic.ContainsKey(folder.FileReferenceNumber))
                {
                    pathDic.Add(folder.FileReferenceNumber,
                        new FrnFilePath(folder.FileReferenceNumber, folder.ParentFileReferenceNumber, folder.FileName));
                }
            }

            Stack<UInt64> treeWalkStack = new Stack<ulong>();

            foreach (var key in pathDic.Keys.ToList())
            {
                treeWalkStack.Clear();

                FrnFilePath currentValue;
                if (!pathDic.TryGetValue(key, out currentValue))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentValue.Path)
                    && currentValue.ParentFileReferenceNumber.HasValue
                    && pathDic.ContainsKey(currentValue.ParentFileReferenceNumber.Value))
                {
                    FrnFilePath parentValue = pathDic[currentValue.ParentFileReferenceNumber.Value];

                    while (string.IsNullOrWhiteSpace(parentValue.Path)
                        && parentValue.ParentFileReferenceNumber.HasValue
                        && pathDic.ContainsKey(parentValue.ParentFileReferenceNumber.Value))
                    {
                        currentValue = parentValue;

                        if (currentValue.ParentFileReferenceNumber.HasValue
                            && pathDic.ContainsKey(currentValue.ParentFileReferenceNumber.Value))
                        {
                            treeWalkStack.Push(key);
                            parentValue = pathDic[currentValue.ParentFileReferenceNumber.Value];
                        }
                        else
                        {
                            parentValue = null;
                            break;
                        }
                    }

                    if (parentValue != null)
                    {
                        currentValue.Path = BuildPath(currentValue, parentValue);

                        while (treeWalkStack.Count > 0)
                        {
                            UInt64 walkedKey = treeWalkStack.Pop();

                            FrnFilePath walkedNode = pathDic[walkedKey];
                            FrnFilePath parentNode = pathDic[walkedNode.ParentFileReferenceNumber.Value];

                            walkedNode.Path = BuildPath(walkedNode, parentNode);
                        }
                    }
                }
            }

            var result = pathDic.Values.Where(p => !string.IsNullOrWhiteSpace(p.Path) && p.Path.StartsWith(drive.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            return result;
        }

        private static string BuildPath(FrnFilePath currentNode, FrnFilePath parentNode)
        {
            return string.Concat(parentNode.Path, "\\", currentNode.FileName);
        }
    }
}
