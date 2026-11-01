using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace QueryEngine
{
    public static class IndexDatabase
    {
        private const uint MAGIC_HEADER = 0x45564558; // "EVEX"
        public const int DB_VERSION = 2; // Version 2: Parallel Block-Compressed Indexed Database
        private const int CHUNK_SIZE = 150000;

        public static string DefaultDatabasePath
        {
            get
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, "EverythingEx", "Everything.db");
            }
        }

        public static bool DatabaseExists(string path = null)
        {
            path = path ?? DefaultDatabasePath;
            return File.Exists(path);
        }

        public static long GetDatabaseSize(string path = null)
        {
            path = path ?? DefaultDatabasePath;
            if (!File.Exists(path)) return 0;
            return new FileInfo(path).Length;
        }

        public static List<FileAndDirectoryEntry> LoadFromFile(string path = null)
        {
            path = path ?? DefaultDatabasePath;
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 131072))
                using (var reader = new BinaryReader(fs))
                {
                    uint magic = reader.ReadUInt32();
                    if (magic != MAGIC_HEADER)
                    {
                        return null;
                    }

                    int version = reader.ReadInt32();
                    if (version == 2)
                    {
                        return LoadVersion2(reader);
                    }
                    else if (version == 1)
                    {
                        return LoadVersion1(reader);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static List<FileAndDirectoryEntry> LoadVersion2(BinaryReader reader)
        {
            int totalFolders = reader.ReadInt32();
            int compFoldersLen = reader.ReadInt32();
            byte[] compFoldersBytes = reader.ReadBytes(compFoldersLen);

            string[] loadedFolders = new string[totalFolders];
            using (var ms = new MemoryStream(compFoldersBytes, false))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var bs = new BufferedStream(ds, 65536))
            using (var fbr = new BinaryReader(bs))
            {
                int fc = fbr.ReadInt32();
                for (int i = 0; i < fc; i++)
                {
                    loadedFolders[i] = fbr.ReadString();
                }
            }

            int totalEntries = reader.ReadInt32();
            int chunkCount = reader.ReadInt32();

            byte[][] readChunks = new byte[chunkCount][];
            int[] itemCounts = new int[chunkCount];
            for (int c = 0; c < chunkCount; c++)
            {
                itemCounts[c] = reader.ReadInt32();
                int clen = reader.ReadInt32();
                readChunks[c] = reader.ReadBytes(clen);
            }

            var chunkEntries = new FileAndDirectoryEntry[chunkCount][];

            Parallel.For(0, chunkCount, c =>
            {
                int count = itemCounts[c];
                var list = new FileAndDirectoryEntry[count];

                using (var ms = new MemoryStream(readChunks[c], false))
                using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                using (var bs = new BufferedStream(ds, 65536))
                using (var dbr = new BinaryReader(bs))
                {
                    for (int i = 0; i < count; i++)
                    {
                        int fIdx = dbr.ReadInt32();
                        string fn = dbr.ReadString();
                        long sz = dbr.ReadInt64();
                        long modTicks = dbr.ReadInt64();
                        long creTicks = dbr.ReadInt64();
                        uint rawAttr = dbr.ReadUInt32();

                        bool isFolder = (rawAttr & 0x80000000) != 0;
                        uint attr = rawAttr & 0x7FFFFFFF;

                        DateTime modified = modTicks > 0 ? DateTime.FromFileTimeUtc(modTicks).ToLocalTime() : DateTime.MinValue;
                        DateTime created = creTicks > 0 ? DateTime.FromFileTimeUtc(creTicks).ToLocalTime() : DateTime.MinValue;

                        list[i] = new FileAndDirectoryEntry
                        {
                            Path = loadedFolders[fIdx],
                            FileName = fn,
                            IsFolder = isFolder,
                            Size = sz,
                            DateModified = modified,
                            DateCreated = created,
                            FileAttributes = attr
                        };
                    }
                }

                chunkEntries[c] = list;
            });

            var result = new List<FileAndDirectoryEntry>(totalEntries);
            for (int c = 0; c < chunkCount; c++)
            {
                if (chunkEntries[c] != null)
                {
                    result.AddRange(chunkEntries[c]);
                }
            }

            return result;
        }

        private static List<FileAndDirectoryEntry> LoadVersion1(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var entries = new List<FileAndDirectoryEntry>(count);
            var pathPool = new Dictionary<string, string>(32768, StringComparer.Ordinal);

            for (int i = 0; i < count; i++)
            {
                string fileName = reader.ReadString();
                string rawDirPath = reader.ReadString();
                string dirPath;
                if (!pathPool.TryGetValue(rawDirPath, out dirPath))
                {
                    pathPool.Add(rawDirPath, rawDirPath);
                    dirPath = rawDirPath;
                }

                bool isFolder = reader.ReadBoolean();
                long size = reader.ReadInt64();
                long modifiedTicks = reader.ReadInt64();
                long createdTicks = reader.ReadInt64();
                uint attributes = reader.ReadUInt32();
                ulong frn = reader.ReadUInt64();
                ulong pfrn = reader.ReadUInt64();

                DateTime modified = modifiedTicks > 0 ? DateTime.FromFileTimeUtc(modifiedTicks).ToLocalTime() : DateTime.MinValue;
                DateTime created = createdTicks > 0 ? DateTime.FromFileTimeUtc(createdTicks).ToLocalTime() : DateTime.MinValue;

                var entry = new FileAndDirectoryEntry
                {
                    FileName = fileName,
                    Path = dirPath,
                    IsFolder = isFolder,
                    Size = size,
                    DateModified = modified,
                    DateCreated = created,
                    FileAttributes = attributes,
                    FileReferenceNumber = frn,
                    ParentFileReferenceNumber = pfrn
                };

                entries.Add(entry);
            }

            return entries;
        }

        public static bool SaveToFile(IList<FileAndDirectoryEntry> entries, string path = null)
        {
            if (entries == null) return false;

            path = path ?? DefaultDatabasePath;

            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string tempPath = path + ".tmp";

                // Filter out any deleted entries
                var activeEntries = new List<FileAndDirectoryEntry>(entries.Count);
                for (int i = 0; i < entries.Count; i++)
                {
                    var item = entries[i];
                    if (item != null && !item.IsDeleted)
                    {
                        activeEntries.Add(item);
                    }
                }
                entries = activeEntries;

                // 1. Build directory map (indexed folder list)
                var folderList = new List<string>(Math.Min(entries.Count, 262144));
                var folderMap = new Dictionary<string, int>(Math.Min(entries.Count, 262144), StringComparer.Ordinal);
                int[] entryFolderIndices = new int[entries.Count];

                for (int i = 0; i < entries.Count; i++)
                {
                    string p = entries[i].Path ?? string.Empty;
                    int idx;
                    if (!folderMap.TryGetValue(p, out idx))
                    {
                        idx = folderList.Count;
                        folderList.Add(p);
                        folderMap.Add(p, idx);
                    }
                    entryFolderIndices[i] = idx;
                }

                // 2. Compress directory table
                byte[] compressedFolders;
                using (var ms = new MemoryStream(Math.Max(1024, folderList.Count * 25)))
                {
                    using (var ds = new DeflateStream(ms, CompressionLevel.Fastest, true))
                    using (var bs = new BufferedStream(ds, 65536))
                    using (var bw = new BinaryWriter(bs))
                    {
                        bw.Write(folderList.Count);
                        for (int i = 0; i < folderList.Count; i++)
                        {
                            bw.Write(folderList[i]);
                        }
                    }
                    compressedFolders = ms.ToArray();
                }

                // 3. Compress entry chunks in parallel
                int chunkSize = CHUNK_SIZE;
                int chunkCount = (entries.Count + chunkSize - 1) / chunkSize;
                if (chunkCount == 0) chunkCount = 1;

                byte[][] compressedChunks = new byte[chunkCount][];
                int[] chunkItemCounts = new int[chunkCount];

                Parallel.For(0, chunkCount, c =>
                {
                    int start = c * chunkSize;
                    int end = Math.Min(start + chunkSize, entries.Count);
                    int count = Math.Max(0, end - start);
                    chunkItemCounts[c] = count;

                    using (var ms = new MemoryStream(Math.Max(1024, count * 30)))
                    {
                        using (var ds = new DeflateStream(ms, CompressionLevel.Fastest, true))
                        using (var bs = new BufferedStream(ds, 65536))
                        using (var bw = new BinaryWriter(bs))
                        {
                            for (int i = start; i < end; i++)
                            {
                                var e = entries[i];
                                bw.Write(entryFolderIndices[i]);
                                bw.Write(e.FileName ?? string.Empty);
                                bw.Write(e.RawSize);

                                long modTicks = e.DateModified != DateTime.MinValue ? e.DateModified.ToFileTimeUtc() : 0;
                                long creTicks = e.DateCreated != DateTime.MinValue ? e.DateCreated.ToFileTimeUtc() : 0;
                                bw.Write(modTicks);
                                bw.Write(creTicks);

                                uint rawAttr = e.FileAttributes & 0x7FFFFFFF;
                                if (e.IsFolder)
                                {
                                    rawAttr |= 0x80000000;
                                }
                                bw.Write(rawAttr);
                            }
                        }
                        compressedChunks[c] = ms.ToArray();
                    }
                });

                // 4. Write database container file
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 131072))
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write(MAGIC_HEADER);
                    writer.Write(DB_VERSION);
                    writer.Write(folderList.Count);
                    writer.Write(compressedFolders.Length);
                    writer.Write(compressedFolders);

                    writer.Write(entries.Count);
                    writer.Write(chunkCount);
                    for (int c = 0; c < chunkCount; c++)
                    {
                        writer.Write(chunkItemCounts[c]);
                        writer.Write(compressedChunks[c].Length);
                        writer.Write(compressedChunks[c]);
                    }
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.Move(tempPath, path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
