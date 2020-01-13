using System;
using System.Runtime.InteropServices;

namespace CustomControl.Win32
{
    internal static class NativeMethods
    {
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetLastError();

        /// <summary>
        /// 用于设置窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hWndInsertAfter"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        /// <summary>
        /// 安装钩子
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hmod"></param>
        /// <param name="dwThreadId"></param>
        /// <returns></returns>
        /// <remarks>https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwindowshookexa</remarks>
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SetWindowsHookEx(WH_Code idHook, HookProc lpfn, IntPtr hmod, int dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function succeeds, the return value is nonzero.If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
        /// <remarks>https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-unhookwindowshookex</remarks>
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hhk"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        /// <remarks>https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-callnexthookex</remarks>
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <param name="lpPoint">A pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns nonzero if successful or zero otherwise. To get extended error information, call GetLastError.</returns>
        /// <remarks>https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos</remarks>
        [DllImport("user32.dll")]
        public extern static bool GetCursorPos(ref POINT lpPoint);
    }
}
