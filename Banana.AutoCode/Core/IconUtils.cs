using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Banana.AutoCode.Core
{
    public static class IconUtils
    {
        private const uint SHGFI_ICON = 0x100;         // Gets the icon 
        private const uint SHGFI_DISPLAYNAME = 0x200;  // Gets the Display name
        private const uint SHGFI_TYPENAME = 0x400;     // Gets the type name
        private const uint SHGFI_LARGEICON = 0x0;      // Large icon
        private const uint SHGFI_SMALLICON = 0x1;      // Small icon

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        private class Win32
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }

        public static Icon GetIcon(string path)
        { 
            SHFILEINFO info = new SHFILEINFO();
            IntPtr iconHandle = Win32.SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_ICON | SHGFI_TYPENAME | SHGFI_SMALLICON);

            return Icon.FromHandle(info.hIcon);
        }
    }
}
