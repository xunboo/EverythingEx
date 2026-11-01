using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormUI.Core
{
    public static class ListViewSortIndicator
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETHEADER = LVM_FIRST + 31;

        private const int HDM_FIRST = 0x1200;
        private const int HDM_GETITEMW = HDM_FIRST + 11;
        private const int HDM_SETITEMW = HDM_FIRST + 12;

        private const int HDI_FORMAT = 0x0004;
        private const int HDF_SORTUP = 0x0400;
        private const int HDF_SORTDOWN = 0x0200;

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public uint mask;
            public int cxy;
            public IntPtr pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            public uint type;
            public IntPtr pvFilter;
            public uint state;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref HDITEM lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static IntPtr GetHeaderHandle(ListView listView)
        {
            if (listView == null || !listView.IsHandleCreated) return IntPtr.Zero;
            return SendMessage(listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool IsPointInHeader(ListView listView, Point screenPoint)
        {
            IntPtr hHeader = GetHeaderHandle(listView);
            if (hHeader == IntPtr.Zero) return false;

            RECT rc;
            if (GetWindowRect(hHeader, out rc))
            {
                return rc.ToRectangle().Contains(screenPoint);
            }
            return false;
        }

        public static void SetSortArrow(ListView listView, int columnIndex, SortOrder order)
        {
            if (listView == null || !listView.IsHandleCreated) return;

            IntPtr hHeader = GetHeaderHandle(listView);
            if (hHeader == IntPtr.Zero) return;

            for (int i = 0; i < listView.Columns.Count; i++)
            {
                var item = new HDITEM();
                item.mask = HDI_FORMAT;

                if (SendMessage(hHeader, HDM_GETITEMW, (IntPtr)i, ref item) != IntPtr.Zero)
                {
                    if (i == columnIndex && order != SortOrder.None)
                    {
                        item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN);
                        item.fmt |= (order == SortOrder.Ascending) ? HDF_SORTUP : HDF_SORTDOWN;
                    }
                    else
                    {
                        item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN);
                    }

                    SendMessage(hHeader, HDM_SETITEMW, (IntPtr)i, ref item);
                }
            }
        }
    }
}
