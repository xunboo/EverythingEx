using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QueryEngine
{
    public class SearchEngine
    {
        public static List<FileAndDirectoryEntry> Search(
            List<FileAndDirectoryEntry> entries,
            string queryString,
            SearchOptions options,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entries == null || entries.Count == 0)
            {
                return new List<FileAndDirectoryEntry>();
            }

            options = options ?? new SearchOptions();
            queryString = queryString ?? string.Empty;

            // Combine active filter if present
            string activeFilterQuery = options.ActiveFilter != null ? options.ActiveFilter.FilterQuery : string.Empty;
            Func<FileAndDirectoryEntry, bool> activeFilterPredicate = null;
            if (!string.IsNullOrWhiteSpace(activeFilterQuery))
            {
                activeFilterPredicate = CompileQuery(activeFilterQuery, new SearchOptions { MatchCase = false });
            }

            // Compile main search query
            Func<FileAndDirectoryEntry, bool> queryPredicate = null;
            int maxCount = options.MaxResults;
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                // Check for count: function in query
                Match countMatch = Regex.Match(queryString, @"\bcount:(\d+)\b", RegexOptions.IgnoreCase);
                if (countMatch.Success)
                {
                    int.TryParse(countMatch.Groups[1].Value, out maxCount);
                    queryString = queryString.Remove(countMatch.Index, countMatch.Length).Trim();
                }

                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    queryPredicate = CompileQuery(queryString, options);
                }
            }

            // If both are null, return everything up to maxCount
            if (activeFilterPredicate == null && queryPredicate == null)
            {
                var nonDeleted = new List<FileAndDirectoryEntry>(entries.Count);
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e != null && !e.IsDeleted)
                    {
                        nonDeleted.Add(e);
                        if (maxCount > 0 && nonDeleted.Count >= maxCount) break;
                    }
                }
                return nonDeleted;
            }

            // High-performance multi-threaded search with pre-allocated partition arrays
            int partitionCount = Math.Max(1, Environment.ProcessorCount);
            int chunkSize = (entries.Count + partitionCount - 1) / partitionCount;
            List<FileAndDirectoryEntry>[] partitionResults = new List<FileAndDirectoryEntry>[partitionCount];

            Parallel.For(0, partitionCount, (partitionIndex, state) =>
            {
                int start = partitionIndex * chunkSize;
                int end = Math.Min(start + chunkSize, entries.Count);
                if (start >= end)
                {
                    partitionResults[partitionIndex] = new List<FileAndDirectoryEntry>(0);
                    return;
                }

                List<FileAndDirectoryEntry> localMatches = new List<FileAndDirectoryEntry>(Math.Min(2048, end - start));

                for (int i = start; i < end; i++)
                {
                    if (cancellationToken.IsCancellationRequested || state.ShouldExitCurrentIteration)
                    {
                        break;
                    }

                    FileAndDirectoryEntry item = entries[i];
                    if (item == null || item.IsDeleted) continue;

                    if (activeFilterPredicate != null && !activeFilterPredicate(item))
                    {
                        continue;
                    }

                    if (queryPredicate != null && !queryPredicate(item))
                    {
                        continue;
                    }

                    localMatches.Add(item);

                    if (maxCount > 0 && localMatches.Count >= maxCount)
                    {
                        state.Break();
                        break;
                    }
                }

                partitionResults[partitionIndex] = localMatches;
            });

            int totalMatched = 0;
            for (int p = 0; p < partitionCount; p++)
            {
                if (partitionResults[p] != null)
                {
                    totalMatched += partitionResults[p].Count;
                }
            }

            var results = new List<FileAndDirectoryEntry>(totalMatched);
            for (int p = 0; p < partitionCount; p++)
            {
                if (partitionResults[p] != null && partitionResults[p].Count > 0)
                {
                    results.AddRange(partitionResults[p]);
                }
            }

            if (maxCount > 0 && results.Count > maxCount)
            {
                return results.Take(maxCount).ToList();
            }

            return results;
        }

        public static Func<FileAndDirectoryEntry, bool> CompileQuery(string queryString, SearchOptions globalOptions)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return item => true;
            }

            // Split into OR branches (respecting quotes and brackets)
            List<string> orBranches = SplitByTopLevelDelimiter(queryString, '|');
            if (orBranches.Count == 0)
            {
                return item => true;
            }

            List<Func<FileAndDirectoryEntry, bool>> orPredicates = new List<Func<FileAndDirectoryEntry, bool>>();

            foreach (var branch in orBranches)
            {
                string trimmedBranch = branch.Trim();
                if (string.IsNullOrEmpty(trimmedBranch)) continue;

                // Split into AND tokens
                List<string> andTokens = TokenizeSearchString(trimmedBranch);
                if (andTokens.Count == 0) continue;

                List<Func<FileAndDirectoryEntry, bool>> andPredicates = new List<Func<FileAndDirectoryEntry, bool>>();

                foreach (var token in andTokens)
                {
                    var pred = CompileToken(token, globalOptions);
                    if (pred != null)
                    {
                        andPredicates.Add(pred);
                    }
                }

                if (andPredicates.Count == 1)
                {
                    orPredicates.Add(andPredicates[0]);
                }
                else if (andPredicates.Count > 1)
                {
                    var preds = andPredicates.ToArray();
                    orPredicates.Add(item =>
                    {
                        for (int i = 0; i < preds.Length; i++)
                        {
                            if (!preds[i](item)) return false;
                        }
                        return true;
                    });
                }
            }

            if (orPredicates.Count == 1)
            {
                return orPredicates[0];
            }
            if (orPredicates.Count > 1)
            {
                var orArray = orPredicates.ToArray();
                return item =>
                {
                    for (int i = 0; i < orArray.Length; i++)
                    {
                        if (orArray[i](item)) return true;
                    }
                    return false;
                };
            }

            return item => true;
        }

        private static Func<FileAndDirectoryEntry, bool> CompileToken(string rawToken, SearchOptions globalOptions)
        {
            rawToken = rawToken.Trim();
            if (string.IsNullOrEmpty(rawToken)) return null;

            bool isNegated = false;
            if (rawToken.StartsWith("!"))
            {
                isNegated = true;
                rawToken = rawToken.Substring(1).Trim();
                if (string.IsNullOrEmpty(rawToken)) return null;
            }

            // Check if grouped: <...> or (...)
            if ((rawToken.StartsWith("<") && rawToken.EndsWith(">")) ||
                (rawToken.StartsWith("(") && rawToken.EndsWith(")")))
            {
                string inner = rawToken.Substring(1, rawToken.Length - 2).Trim();
                var innerPred = CompileQuery(inner, globalOptions);
                return isNegated ? (Func<FileAndDirectoryEntry, bool>)(item => !innerPred(item)) : innerPred;
            }

            // Local modifiers
            bool matchCase = globalOptions.MatchCase;
            bool matchWholeWord = globalOptions.MatchWholeWord;
            bool matchPath = globalOptions.MatchPath;
            bool enableRegex = globalOptions.EnableRegex;

            string token = rawToken;

            // Strip quotes if any
            if (token.StartsWith("\"") && token.EndsWith("\"") && token.Length >= 2)
            {
                token = token.Substring(1, token.Length - 2);
            }

            // Parse leading modifiers
            bool modifierFound = true;
            while (modifierFound && !string.IsNullOrEmpty(token))
            {
                modifierFound = false;
                if (token.StartsWith("case:", StringComparison.OrdinalIgnoreCase))
                {
                    matchCase = true;
                    token = token.Substring(5);
                    modifierFound = true;
                }
                else if (token.StartsWith("nocase:", StringComparison.OrdinalIgnoreCase))
                {
                    matchCase = false;
                    token = token.Substring(7);
                    modifierFound = true;
                }
                else if (token.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                {
                    matchPath = true;
                    token = token.Substring(5);
                    modifierFound = true;
                }
                else if (token.StartsWith("nopath:", StringComparison.OrdinalIgnoreCase))
                {
                    matchPath = false;
                    token = token.Substring(7);
                    modifierFound = true;
                }
                else if (token.StartsWith("regex:", StringComparison.OrdinalIgnoreCase))
                {
                    enableRegex = true;
                    token = token.Substring(6);
                    modifierFound = true;
                }
                else if (token.StartsWith("noregex:", StringComparison.OrdinalIgnoreCase))
                {
                    enableRegex = false;
                    token = token.Substring(8);
                    modifierFound = true;
                }
                else if (token.StartsWith("ww:", StringComparison.OrdinalIgnoreCase) ||
                         token.StartsWith("wholeword:", StringComparison.OrdinalIgnoreCase))
                {
                    matchWholeWord = true;
                    token = token.IndexOf(':') >= 0 ? token.Substring(token.IndexOf(':') + 1) : token;
                    modifierFound = true;
                }
                else if (token.StartsWith("nowholeword:", StringComparison.OrdinalIgnoreCase))
                {
                    matchWholeWord = false;
                    token = token.Substring(12);
                    modifierFound = true;
                }
            }

            // If term contains backslash, Everything automatically activates path matching!
            if (token.IndexOf('\\') >= 0)
            {
                matchPath = true;
            }

            // Built-in Macro functions: audio:, video:, doc:, exe:, zip:, pic:, folder:, file:
            if (string.Equals(token, "folder:", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "folders:", StringComparison.OrdinalIgnoreCase))
            {
                return WrapNegation(item => item.IsFolder, isNegated);
            }
            if (string.Equals(token, "file:", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(token, "files:", StringComparison.OrdinalIgnoreCase))
            {
                return WrapNegation(item => !item.IsFolder, isNegated);
            }
            if (string.Equals(token, "root:", StringComparison.OrdinalIgnoreCase))
            {
                return WrapNegation(item => string.IsNullOrEmpty(item.Path) || item.Path.EndsWith(":\\"), isNegated);
            }
            if (string.Equals(token, "empty:", StringComparison.OrdinalIgnoreCase))
            {
                return WrapNegation(item => (!item.IsFolder && item.Size == 0), isNegated);
            }

            // Function parsing: function:value
            int colonIndex = token.IndexOf(':');
            if (colonIndex > 0)
            {
                string funcName = token.Substring(0, colonIndex).ToLowerInvariant();
                string funcVal = token.Substring(colonIndex + 1).Trim();

                if (funcName == "ext")
                {
                    var pred = CompileExtensionFilter(funcVal);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "size")
                {
                    var pred = CompileSizeFilter(funcVal);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "dm" || funcName == "date-modified")
                {
                    var pred = CompileDateFilter(funcVal, item => item.DateModified);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "dc" || funcName == "date-created")
                {
                    var pred = CompileDateFilter(funcVal, item => item.DateCreated);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "attrib" || funcName == "attributes")
                {
                    var pred = CompileAttributeFilter(funcVal);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "parent")
                {
                    var pred = (Func<FileAndDirectoryEntry, bool>)(item =>
                        item.Path != null && item.Path.IndexOf(funcVal, StringComparison.OrdinalIgnoreCase) >= 0);
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "startwith" || funcName == "startswith")
                {
                    var pred = (Func<FileAndDirectoryEntry, bool>)(item =>
                        item.FileName != null && item.FileName.StartsWith(funcVal, StringComparison.OrdinalIgnoreCase));
                    return WrapNegation(pred, isNegated);
                }
                if (funcName == "endwith" || funcName == "endswith")
                {
                    var pred = (Func<FileAndDirectoryEntry, bool>)(item =>
                        item.FileName != null && item.FileName.EndsWith(funcVal, StringComparison.OrdinalIgnoreCase));
                    return WrapNegation(pred, isNegated);
                }

                // Check for filter macros: audio:, video:, doc:, etc.
                var macroFilter = FilterCategory.GetDefaultFilters()
                    .FirstOrDefault(f => !string.IsNullOrEmpty(f.SearchMacro) && f.SearchMacro.TrimEnd(':').Equals(funcName, StringComparison.OrdinalIgnoreCase));
                if (macroFilter != null)
                {
                    var macroPred = CompileQuery(macroFilter.FilterQuery, globalOptions);
                    if (string.IsNullOrEmpty(funcVal))
                    {
                        return WrapNegation(macroPred, isNegated);
                    }
                    else
                    {
                        var innerValPred = CompileToken(funcVal, globalOptions);
                        return WrapNegation(item => macroPred(item) && innerValPred(item), isNegated);
                    }
                }
            }

            // Normal search term evaluation
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            // Regular Expression match
            if (enableRegex)
            {
                try
                {
                    RegexOptions regexOpt = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                    Regex rx = new Regex(token, regexOpt);
                    return WrapNegation(item =>
                    {
                        string target = matchPath ? item.FullFileName : item.FileName;
                        return target != null && rx.IsMatch(target);
                    }, isNegated);
                }
                catch
                {
                    // If regex pattern is invalid, fall through to text match
                }
            }

            // Wildcards * and ?
            if (token.IndexOf('*') >= 0 || token.IndexOf('?') >= 0)
            {
                string regexPattern = "^" + Regex.Escape(token).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                RegexOptions regexOpt = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                Regex rx = new Regex(regexPattern, regexOpt);
                return WrapNegation(item =>
                {
                    string target = matchPath ? item.FullFileName : item.FileName;
                    return target != null && rx.IsMatch(target);
                }, isNegated);
            }

            // Whole word match
            if (matchWholeWord)
            {
                string wordPattern = @"\b" + Regex.Escape(token) + @"\b";
                RegexOptions regexOpt = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                Regex rx = new Regex(wordPattern, regexOpt);
                return WrapNegation(item =>
                {
                    string target = matchPath ? item.FullFileName : item.FileName;
                    return target != null && rx.IsMatch(target);
                }, isNegated);
            }

            // Fast Substring match
            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return WrapNegation(item =>
            {
                string target = matchPath ? item.FullFileName : item.FileName;
                return target != null && target.IndexOf(token, comparison) >= 0;
            }, isNegated);
        }

        private static Func<FileAndDirectoryEntry, bool> WrapNegation(Func<FileAndDirectoryEntry, bool> pred, bool isNegated)
        {
            if (pred == null) return null;
            if (!isNegated) return pred;
            return item => !pred(item);
        }

        private static Func<FileAndDirectoryEntry, bool> CompileExtensionFilter(string extList)
        {
            var rawExts = extList.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> extSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ext in rawExts)
            {
                string clean = ext.Trim();
                if (!clean.StartsWith(".")) clean = "." + clean;
                extSet.Add(clean);
            }

            return item =>
            {
                if (item.IsFolder) return false;
                return extSet.Contains(item.Extension);
            };
        }

        private static Func<FileAndDirectoryEntry, bool> CompileSizeFilter(string sizeExpr)
        {
            sizeExpr = sizeExpr.Trim().ToLowerInvariant();

            // Named sizes
            if (sizeExpr == "empty") return item => !item.IsFolder && item.Size == 0;
            if (sizeExpr == "tiny") return item => !item.IsFolder && item.Size >= 0 && item.Size <= 10 * 1024;
            if (sizeExpr == "small") return item => !item.IsFolder && item.Size > 10 * 1024 && item.Size <= 100 * 1024;
            if (sizeExpr == "medium") return item => !item.IsFolder && item.Size > 100 * 1024 && item.Size <= 1024 * 1024;
            if (sizeExpr == "large") return item => !item.IsFolder && item.Size > 1024 * 1024 && item.Size <= 16 * 1024 * 1024;
            if (sizeExpr == "huge") return item => !item.IsFolder && item.Size > 16 * 1024 * 1024 && item.Size <= 128 * 1024 * 1024;
            if (sizeExpr == "gigantic") return item => !item.IsFolder && item.Size > 128 * 1024 * 1024;

            // Range: min..max
            int rangeIndex = sizeExpr.IndexOf("..");
            if (rangeIndex > 0)
            {
                string minStr = sizeExpr.Substring(0, rangeIndex);
                string maxStr = sizeExpr.Substring(rangeIndex + 2);
                long minBytes = ParseSizeToBytes(minStr);
                long maxBytes = ParseSizeToBytes(maxStr);
                return item => !item.IsFolder && item.Size >= minBytes && item.Size <= maxBytes;
            }

            // Operators: >=, <=, >, <, !=, =
            string op = "=";
            string valStr = sizeExpr;
            if (sizeExpr.StartsWith(">=")) { op = ">="; valStr = sizeExpr.Substring(2); }
            else if (sizeExpr.StartsWith("<=")) { op = "<="; valStr = sizeExpr.Substring(2); }
            else if (sizeExpr.StartsWith("!=")) { op = "!="; valStr = sizeExpr.Substring(2); }
            else if (sizeExpr.StartsWith(">")) { op = ">"; valStr = sizeExpr.Substring(1); }
            else if (sizeExpr.StartsWith("<")) { op = "<"; valStr = sizeExpr.Substring(1); }
            else if (sizeExpr.StartsWith("=")) { op = "="; valStr = sizeExpr.Substring(1); }

            long targetBytes = ParseSizeToBytes(valStr);

            switch (op)
            {
                case ">": return item => !item.IsFolder && item.Size > targetBytes;
                case ">=": return item => !item.IsFolder && item.Size >= targetBytes;
                case "<": return item => !item.IsFolder && item.Size >= 0 && item.Size < targetBytes;
                case "<=": return item => !item.IsFolder && item.Size >= 0 && item.Size <= targetBytes;
                case "!=": return item => !item.IsFolder && item.Size != targetBytes;
                default: return item => !item.IsFolder && item.Size == targetBytes;
            }
        }

        private static long ParseSizeToBytes(string sizeStr)
        {
            sizeStr = sizeStr.Trim().ToLowerInvariant();
            long multiplier = 1;

            if (sizeStr.EndsWith("kb")) { multiplier = 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 2); }
            else if (sizeStr.EndsWith("k")) { multiplier = 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 1); }
            else if (sizeStr.EndsWith("mb")) { multiplier = 1024 * 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 2); }
            else if (sizeStr.EndsWith("m")) { multiplier = 1024 * 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 1); }
            else if (sizeStr.EndsWith("gb")) { multiplier = 1024L * 1024 * 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 2); }
            else if (sizeStr.EndsWith("g")) { multiplier = 1024L * 1024 * 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 1); }
            else if (sizeStr.EndsWith("tb")) { multiplier = 1024L * 1024 * 1024 * 1024; sizeStr = sizeStr.Substring(0, sizeStr.Length - 2); }
            else if (sizeStr.EndsWith("b")) { multiplier = 1; sizeStr = sizeStr.Substring(0, sizeStr.Length - 1); }

            double val;
            if (double.TryParse(sizeStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
            {
                return (long)(val * multiplier);
            }
            return 0;
        }

        private static Func<FileAndDirectoryEntry, bool> CompileDateFilter(string dateExpr, Func<FileAndDirectoryEntry, DateTime> dateSelector)
        {
            dateExpr = dateExpr.Trim().ToLowerInvariant();
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            if (dateExpr == "today")
            {
                DateTime start = today;
                DateTime end = today.AddDays(1);
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "yesterday")
            {
                DateTime start = today.AddDays(-1);
                DateTime end = today;
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "thisweek")
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime start = today.AddDays(-1 * diff);
                DateTime end = start.AddDays(7);
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "lastweek")
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime thisWeekStart = today.AddDays(-1 * diff);
                DateTime start = thisWeekStart.AddDays(-7);
                DateTime end = thisWeekStart;
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "thismonth")
            {
                DateTime start = new DateTime(today.Year, today.Month, 1);
                DateTime end = start.AddMonths(1);
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "lastmonth")
            {
                DateTime thisMonthStart = new DateTime(today.Year, today.Month, 1);
                DateTime start = thisMonthStart.AddMonths(-1);
                DateTime end = thisMonthStart;
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "thisyear")
            {
                DateTime start = new DateTime(today.Year, 1, 1);
                DateTime end = start.AddYears(1);
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }
            if (dateExpr == "lastyear")
            {
                DateTime start = new DateTime(today.Year - 1, 1, 1);
                DateTime end = new DateTime(today.Year, 1, 1);
                return item => { var d = dateSelector(item); return d >= start && d < end; };
            }

            // Range: start..end
            int rangeIndex = dateExpr.IndexOf("..");
            if (rangeIndex > 0)
            {
                string startStr = dateExpr.Substring(0, rangeIndex);
                string endStr = dateExpr.Substring(rangeIndex + 2);
                DateTime startDate, endDate;
                if (DateTime.TryParse(startStr, out startDate) && DateTime.TryParse(endStr, out endDate))
                {
                    return item => { var d = dateSelector(item); return d >= startDate && d <= endDate; };
                }
            }

            // Operators: >=, <=, >, <
            string op = ">=";
            string valStr = dateExpr;
            if (dateExpr.StartsWith(">=")) { op = ">="; valStr = dateExpr.Substring(2); }
            else if (dateExpr.StartsWith("<=")) { op = "<="; valStr = dateExpr.Substring(2); }
            else if (dateExpr.StartsWith(">")) { op = ">"; valStr = dateExpr.Substring(1); }
            else if (dateExpr.StartsWith("<")) { op = "<"; valStr = dateExpr.Substring(1); }

            DateTime targetDate;
            if (DateTime.TryParse(valStr, out targetDate))
            {
                switch (op)
                {
                    case ">": return item => dateSelector(item) > targetDate;
                    case ">=": return item => dateSelector(item) >= targetDate;
                    case "<": return item => dateSelector(item) < targetDate;
                    case "<=": return item => dateSelector(item) <= targetDate;
                }
            }

            return item => true;
        }

        private static Func<FileAndDirectoryEntry, bool> CompileAttributeFilter(string attribStr)
        {
            attribStr = attribStr.Trim().ToLowerInvariant();
            return item =>
            {
                for (int i = 0; i < attribStr.Length; i++)
                {
                    char c = attribStr[i];
                    if (c == 'd' && !item.IsFolder) return false;
                    if (c == 'h' && (item.FileAttributes & 0x02) == 0) return false;
                    if (c == 'r' && (item.FileAttributes & 0x01) == 0) return false;
                    if (c == 's' && (item.FileAttributes & 0x04) == 0) return false;
                    if (c == 'a' && (item.FileAttributes & 0x20) == 0) return false;
                }
                return true;
            };
        }

        private static List<string> SplitByTopLevelDelimiter(string str, char delimiter)
        {
            List<string> result = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            int angleBracketDepth = 0;
            int parenDepth = 0;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (!inQuotes && c == '<')
                {
                    angleBracketDepth++;
                    current.Append(c);
                }
                else if (!inQuotes && c == '>')
                {
                    if (angleBracketDepth > 0) angleBracketDepth--;
                    current.Append(c);
                }
                else if (!inQuotes && c == '(')
                {
                    parenDepth++;
                    current.Append(c);
                }
                else if (!inQuotes && c == ')')
                {
                    if (parenDepth > 0) parenDepth--;
                    current.Append(c);
                }
                else if (c == delimiter && !inQuotes && angleBracketDepth == 0 && parenDepth == 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result;
        }

        private static List<string> TokenizeSearchString(string str)
        {
            List<string> tokens = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            int angleBracketDepth = 0;
            int parenDepth = 0;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (!inQuotes && c == '<')
                {
                    angleBracketDepth++;
                    current.Append(c);
                }
                else if (!inQuotes && c == '>')
                {
                    if (angleBracketDepth > 0) angleBracketDepth--;
                    current.Append(c);
                }
                else if (!inQuotes && c == '(')
                {
                    parenDepth++;
                    current.Append(c);
                }
                else if (!inQuotes && c == ')')
                {
                    if (parenDepth > 0) parenDepth--;
                    current.Append(c);
                }
                else if (char.IsWhiteSpace(c) && !inQuotes && angleBracketDepth == 0 && parenDepth == 0)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens;
        }
    }
}
