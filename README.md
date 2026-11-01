# <img src="WinFormUI/Resources/app.ico" height="32px"> EverythingEx

**EverythingEx** is a high-performance, lightweight instant desktop search utility built with C# and .NET Framework 4.8 (x64). It faithfully replicates and implements the core technology, search syntax, and responsive user experience of [voidtools Everything](https://www.voidtools.com/).

---

## 🌟 Key Features

### 1. ⚡ Instant Indexing Engine
- **Ultra-Fast NTFS USN Journal Scanning**: Directly parses the Master File Table (MFT) and Update Sequence Number (USN) change journal on NTFS volumes, indexing millions of files and folders in just 1–2 seconds.
- **Universal Multi-Drive Support & Fallback**: Automatically discovers and indexes all active drive letters (`C:`, `D:`, etc.). If administrator privileges are unavailable or non-NTFS drives (FAT32, exFAT, network shares) are encountered, it gracefully falls back to optimized directory enumeration.
- **Real-Time Filesystem Monitoring**: Integrates a non-blocking `FileSystemWatcher` engine with on-demand lookup caching. File and directory creations, deletions, renames, and modifications are reflected in memory instantly without upfront path string bloat.
- **Stale Entry Detection**: On-the-fly Win32 existence verification filters out ghost entries and purges deleted paths automatically.

### 2. 🚀 Compact Database & Optimized Memory Footprint
- **Compressed Binary Database (`Everything.db`)**: High-speed chunked Deflate/LZ4 compression with hierarchical folder-string deduplication reduces multi-gigabyte raw indexes to just ~40–80 MB on disk.
- **Sub-Second Cold Start**: Loads millions of items into memory in under a second.
- **Lean Memory Architecture**: Actively manages the Large Object Heap (LOH) with in-place operations and automatic working set trimming (`TrimProcessMemory`). A dataset of 3+ million items remains stable at ~350–450 MB of RAM with zero memory ballooning.
- **No Background Thread Drag**: Intelligent size and attribute population avoids wasteful disk I/O, ensuring CPU usage drops to 0.0% immediately after indexing and sorting.

### 3. 🔍 Complete Everything Search Syntax
Fully compatible with the official [voidtools Everything search syntax](https://www.voidtools.com/support/everything/):

- **Boolean Operators**:
  - `Space` : `AND` operation, e.g., `foo bar`
  - `|` : `OR` operation, e.g., `*.jpg | *.png`
  - `!` : `NOT` operation, e.g., `!*.tmp`
  - `< >` or `( )` : Grouping, e.g., `<foo | bar> size:>10mb`
  - `" "` : Matches exact text containing spaces, e.g., `"Program Files"`
  - Wildcards `*` and `?` : e.g., `report_*.docx`
- **Search Modifiers**:
  - `case:` / `nocase:` : Match or ignore case
  - `path:` / `nopath:` : Match against full path
  - `ww:` / `wholeword:` : Whole word match
  - `regex:` / `noregex:` : Regular expression search
- **Search Functions**:
  - `ext:<list>` : Filter by extension, e.g., `ext:jpg;png;mp4`
  - `size:<expr>` : Filter by file size. Supports operators (`>`, `<`, `>=`, `<=`, `=`), units (`kb`, `mb`, `gb`), ranges (`1mb..50mb`), and size keywords (`size:empty`, `size:tiny`, `size:small`, `size:medium`, `size:large`, `size:huge`, `size:gigantic`)
  - `dm:<date>` : Date modified filter. Supports relative dates (`today`, `yesterday`, `thisweek`, `lastweek`, `thismonth`, `thisyear`), date comparison (`dm:>2026-01-01`), and date ranges (`dm:2026-01-01..2026-10-31`)
  - `dc:<date>` : Date created filter
  - `file:` / `folder:` : Search exclusively for files or folders
  - `attrib:<d,h,r,s,a>` : Filter by attributes (Directory, Hidden, Read-only, System, Archive)
  - `empty:` : Find empty files or empty directories
  - `root:` : Limit to root directory items
  - `parent:<path>` : Match by direct parent path
  - `startwith:<text>` / `endwith:<text>` : Prefix or suffix match
  - `count:<n>` : Limit result set to specified count
- **Macro Filters**:
  - `audio:` : Audio files (MP3, WAV, FLAC, AAC, etc.)
  - `zip:` / `compressed:` : Archive files (ZIP, RAR, 7Z, TAR, GZ, etc.)
  - `doc:` / `document:` : Office documents and plain text (DOCX, PDF, XLSX, TXT, etc.)
  - `exe:` : Executable files and scripts (EXE, BAT, CMD, PS1, MSI, etc.)
  - `pic:` / `picture:` : Image and graphic files (PNG, JPG, SVG, GIF, etc.)
  - `video:` : Video files (MP4, MKV, AVI, MOV, etc.)

### 4. 🖥️ Native Virtual ListView UI
- **Virtual Mode Display**: Built on Windows Forms `ListView` virtual mode with native double-buffering. Delivers 60 FPS scrolling and instantaneous rendering of millions of results with minimal UI thread overhead.
- **Windows Shell Icons**: Caches genuine Windows Explorer system icons for files and folders.
- **Multi-Core Parallel In-Place Sorting**: Custom parallel merge sort sorts millions of rows across any column (Name, Path, Size, Date Modified, Date Created, Extension, Attributes) in milliseconds.
- **Native Header Sort Direction Arrows**: Renders native Windows header sort indicators (▲ / ▼) using `HDF_SORTUP` and `HDF_SORTDOWN` via Win32 `SysHeader32` interop.
- **Customizable Columns**: Right-click the column header to toggle visible columns (`Name`, `Path`, `Size`, `Date Modified`, `Date Created`, `Extension`, `Attributes`) or select **Size All Columns to Fit**. Preferences persist automatically.
- **Filter Bar & Category Dropdown**: Quickly switch between presets (`Everything`, `Audio`, `Compressed`, `Document`, `Executable`, `Folder`, `Picture`, `Video`).

### 5. 📋 Shell Integration & Explorer Menus
- **Comprehensive Context Menu**: Open, Open Path, Show in Explorer (with file selected), Open With, Copy File, Copy Full Path, Copy Name, Rename, Delete to Recycle Bin, and native Properties dialog.
- **Advanced Search Dialog**: Graphical builder for complex multi-condition queries.
- **Bookmarks Management**: Save, edit, and organize frequently used queries and filters with instant recall.
- **Export Capabilities**: Export search results to CSV (spreadsheet), TXT (file paths), or official Everything File List 1.0 (`.efu`) formats.
- **System Tray Integration**: Minimize to tray with quick hotkey restore or exit.
- **Search Syntax Reference (`F1`)**: Comprehensive built-in syntax dictionary with examples.

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Description |
| :--- | :--- |
| `Ctrl + E` / `Alt + D` | Focus search box |
| `Enter` | Open selected file / folder |
| `Ctrl + Enter` | Open path of selected file |
| `Alt + Enter` | Show native Windows file properties |
| `Ctrl + Shift + C` | Copy full path(s) to clipboard |
| `Ctrl + C` | Copy selected file(s) (can be pasted in Explorer) |
| `Ctrl + A` | Select all items |
| `Ctrl + S` | Export search results |
| `Ctrl + N` | Open new search window |
| `Ctrl + R` / `F5` | Refresh / Rescan all drives |
| `F1` | Open Search Syntax reference guide |
| `F2` | Rename selected file or folder |
| `Delete` | Delete selected item(s) to Recycle Bin |
| `Esc` | Clear search box or dismiss active menu |

---

## 🛠️ Build & Run

### Prerequisites
- **Operating System**: Windows 7 SP1 / 8 / 10 / 11 or Windows Server (x64 recommended)
- **Runtime**: [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- **Build Tools**: Visual Studio 2022 / 2026 or .NET SDK (with MSBuild)

### Compilation
Open a terminal in the solution directory and run:

```powershell
# Restore dependencies and build Release x64
dotnet build EverythingEx.sln -c Release

# Run application
.\WinFormUI\bin\Release\net48\EverythingEx.UI.exe
```

---

## 📄 License

- **WinFormUI**: MIT License
- **EverythingEx**: Microsoft Public License (Ms-PL)
