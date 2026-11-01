using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QueryEngine;

namespace WinFormUI.Core
{
    public static class ExportHelper
    {
        public static void Export(IList<FileAndDirectoryEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                MessageBox.Show("No items to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Export";
                sfd.Filter = "CSV (Comma delimited) (*.csv)|*.csv|Text Files (*.txt)|*.txt|Everything File List (*.efu)|*.efu";
                sfd.DefaultExt = "csv";
                sfd.FileName = "Everything.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                        if (ext == ".efu")
                        {
                            ExportEfu(sfd.FileName, entries);
                        }
                        else if (ext == ".txt")
                        {
                            ExportTxt(sfd.FileName, entries);
                        }
                        else
                        {
                            ExportCsv(sfd.FileName, entries);
                        }

                        MessageBox.Show(string.Format("Successfully exported {0:N0} items to:\n{1}", entries.Count, sfd.FileName),
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting file:\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static void ExportCsv(string filePath, IList<FileAndDirectoryEntry> entries)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("\"Name\",\"Path\",\"Size\",\"Date Modified\",\"Attributes\"");
                foreach (var item in entries)
                {
                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                        EscapeCsv(item.FileName),
                        EscapeCsv(item.Path),
                        item.FormattedSize,
                        item.FormattedDateModified,
                        item.FormattedAttributes));
                }
            }
        }

        private static void ExportTxt(string filePath, IList<FileAndDirectoryEntry> entries)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var item in entries)
                {
                    writer.WriteLine(item.FullFileName);
                }
            }
        }

        private static void ExportEfu(string filePath, IList<FileAndDirectoryEntry> entries)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("Filename,Size,Date Modified,Date Created,Attributes");
                foreach (var item in entries)
                {
                    long modifiedTicks = item.DateModified != DateTime.MinValue ? item.DateModified.ToFileTimeUtc() : 0;
                    long createdTicks = item.DateCreated != DateTime.MinValue ? item.DateCreated.ToFileTimeUtc() : 0;
                    writer.WriteLine(string.Format("\"{0}\",{1},{2},{3},{4}",
                        EscapeCsv(item.FullFileName),
                        item.IsFolder ? 0 : Math.Max(0, item.Size),
                        modifiedTicks,
                        createdTicks,
                        item.FileAttributes));
                }
            }
        }

        private static string EscapeCsv(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.Replace("\"", "\"\"");
        }
    }
}
