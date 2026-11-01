using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace QueryEngine
{
    public static class ParallelSorter
    {
        private static readonly FieldInfo ItemsField = typeof(List<FileAndDirectoryEntry>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Sort(
            List<FileAndDirectoryEntry> list,
            int column,
            bool ascending,
            CancellationToken token = default(CancellationToken))
        {
            if (list == null || list.Count <= 1) return;

            var comparison = GetComparison(column, ascending);
            FileAndDirectoryEntry[] internalArray = null;

            if (ItemsField != null)
            {
                try
                {
                    internalArray = ItemsField.GetValue(list) as FileAndDirectoryEntry[];
                }
                catch { }
            }

            if (internalArray != null)
            {
                // In-place parallel merge sort directly on List<T>'s internal buffer
                ParallelMergeSort(internalArray, list.Count, comparison, token);
            }
            else
            {
                var array = list.ToArray();
                ParallelMergeSort(array, array.Length, comparison, token);

                if (!token.IsCancellationRequested)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        list[i] = array[i];
                    }
                }
            }
        }

        private static void ParallelMergeSort<T>(T[] array, int n, Comparison<T> comparison, CancellationToken token)
        {
            int p = Math.Max(1, Environment.ProcessorCount);
            if (n <= 16384 || p <= 1)
            {
                Array.Sort(array, 0, n, Comparer<T>.Create(comparison));
                return;
            }

            int chunkSize = (n + p - 1) / p;

            // Phase 1: Sort each chunk in parallel using native Array.Sort
            Parallel.For(0, p, i =>
            {
                if (token.IsCancellationRequested) return;
                int start = i * chunkSize;
                int length = Math.Min(chunkSize, n - start);
                if (length > 1)
                {
                    Array.Sort(array, start, length, Comparer<T>.Create(comparison));
                }
            });

            if (token.IsCancellationRequested) return;

            // Phase 2: Pairwise parallel merge
            T[] temp = new T[n];
            T[] src = array;
            T[] dst = temp;

            for (int width = chunkSize; width < n; width *= 2)
            {
                if (token.IsCancellationRequested) return;
                int numMerges = (n + (width * 2) - 1) / (width * 2);

                Parallel.For(0, numMerges, m =>
                {
                    if (token.IsCancellationRequested) return;
                    int left = m * width * 2;
                    int mid = Math.Min(left + width, n);
                    int right = Math.Min(left + (width * 2), n);

                    int i = left;
                    int j = mid;
                    int k = left;

                    while (i < mid && j < right)
                    {
                        if (comparison(src[i], src[j]) <= 0)
                        {
                            dst[k++] = src[i++];
                        }
                        else
                        {
                            dst[k++] = src[j++];
                        }
                    }

                    while (i < mid) dst[k++] = src[i++];
                    while (j < right) dst[k++] = src[j++];
                });

                var swap = src;
                src = dst;
                dst = swap;
            }

            if (!object.ReferenceEquals(src, array) && !token.IsCancellationRequested)
            {
                Array.Copy(src, 0, array, 0, n);
            }
            temp = null;
        }

        public static Comparison<FileAndDirectoryEntry> GetComparison(int column, bool ascending)
        {
            if (ascending)
            {
                switch (column)
                {
                    case 1: // Path
                        return (a, b) =>
                        {
                            if (object.ReferenceEquals(a.Path, b.Path))
                            {
                                return string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                            }
                            if (a.Path == null) return -1;
                            if (b.Path == null) return 1;
                            int cmp = string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 2: // Size (Ascending: smallest first, unknown sizes/folders at bottom)
                        return (a, b) =>
                        {
                            long sa = a.RawSize;
                            long sb = b.RawSize;

                            if (sa < 0 && sb < 0) return string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                            if (sa < 0) return 1;  // Unknown/folder at bottom
                            if (sb < 0) return -1; // Unknown/folder at bottom

                            int cmp = sa.CompareTo(sb);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 3: // Date Modified (Ascending: oldest first)
                        return (a, b) =>
                        {
                            int cmp = a.DateModified.CompareTo(b.DateModified);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 4: // Extension (Ascending)
                        return (a, b) =>
                        {
                            int cmp = CompareExtensions(a.FileName, b.FileName);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 5: // Attributes
                        return (a, b) =>
                        {
                            int cmp = a.FileAttributes.CompareTo(b.FileAttributes);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 6: // Date Created
                        return (a, b) =>
                        {
                            int cmp = a.DateCreated.CompareTo(b.DateCreated);
                            return cmp != 0 ? cmp : string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    default: // Name (Ascending: A to Z)
                        return (a, b) =>
                        {
                            int cmp = string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                            return cmp != 0 ? cmp : string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
                        };
                }
            }
            else // Descending
            {
                switch (column)
                {
                    case 1: // Path (Descending)
                        return (a, b) =>
                        {
                            if (object.ReferenceEquals(a.Path, b.Path))
                            {
                                return string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                            }
                            if (a.Path == null) return 1;
                            if (b.Path == null) return -1;
                            int cmp = string.Compare(b.Path, a.Path, StringComparison.OrdinalIgnoreCase);
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 2: // Size (Descending: largest first, unknown sizes/folders at bottom)
                        return (a, b) =>
                        {
                            long sa = a.RawSize;
                            long sb = b.RawSize;

                            if (sa < 0 && sb < 0) return string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                            if (sa < 0) return 1;  // Unknown/folder at bottom
                            if (sb < 0) return -1; // Unknown/folder at bottom

                            int cmp = sb.CompareTo(sa); // Largest first!
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 3: // Date Modified (Descending: newest first)
                        return (a, b) =>
                        {
                            int cmp = b.DateModified.CompareTo(a.DateModified);
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 4: // Extension (Descending)
                        return (a, b) =>
                        {
                            int cmp = CompareExtensions(b.FileName, a.FileName);
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 5: // Attributes (Descending)
                        return (a, b) =>
                        {
                            int cmp = b.FileAttributes.CompareTo(a.FileAttributes);
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    case 6: // Date Created (Descending: newest first)
                        return (a, b) =>
                        {
                            int cmp = b.DateCreated.CompareTo(a.DateCreated);
                            return cmp != 0 ? cmp : string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                        };

                    default: // Name (Descending: Z to A)
                        return (a, b) =>
                        {
                            int cmp = string.Compare(b.FileName, a.FileName, StringComparison.OrdinalIgnoreCase);
                            return cmp != 0 ? cmp : string.Compare(b.Path, a.Path, StringComparison.OrdinalIgnoreCase);
                        };
                }
            }
        }

        public static int CompareExtensions(string nameA, string nameB)
        {
            int dotA = nameA != null ? nameA.LastIndexOf('.') : -1;
            int dotB = nameB != null ? nameB.LastIndexOf('.') : -1;

            if (dotA < 0 && dotB < 0) return 0;
            if (dotA < 0) return -1;
            if (dotB < 0) return 1;

            int lenA = nameA.Length - dotA;
            int lenB = nameB.Length - dotB;
            return string.Compare(nameA, dotA, nameB, dotB, Math.Max(lenA, lenB), StringComparison.OrdinalIgnoreCase);
        }
    }
}
