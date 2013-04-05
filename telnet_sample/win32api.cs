using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace telnet_sample
{
    class win32api
    {
        const int SWP_NOSIZE = 0x0001;
        private static IntPtr MyConsole = GetConsoleWindow();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        // form http://stackoverflow.com/questions/1548838/setting-position-of-a-console-window-opened-in-a-winforms-app/1548881#1548881
        public static void set_console_desktop_xy( int x, int y )
        {
            SetWindowPos(MyConsole, 0, x , y , 0, 0, SWP_NOSIZE);
        }
    }
}
