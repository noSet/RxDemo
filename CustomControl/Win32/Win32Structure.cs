using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CustomControl.Win32
{
    /// <summary>
    /// 钩子类型
    /// </summary>
    /// <remarks>https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwindowshookexa</remarks>
    [SuppressMessage("Naming", "CA1707:标识符不应包含下划线", Justification = "<挂起>")]
    public enum WH_Code
    {
        /// <summary>
        /// Installs a hook procedure that monitors mouse messages. For more information, see the MouseProc hook procedure.
        /// </summary>
        WH_MOUSE = 7,

        /// <summary>
        /// Installs a hook procedure that monitors low-level mouse input events. For more information, see the LowLevelMouseProc hook procedure.
        /// </summary>
        WH_MOUSE_LL = 14,
    }

    [SuppressMessage("Naming", "CA1707:标识符不应包含下划线", Justification = "<挂起>")]
    public enum WM_MOUSE : int
    {
        /// <summary>
        /// 鼠标开始
        /// </summary>
        WM_MOUSEFIRST = 0x200,

        /// <summary>
        /// 鼠标移动
        /// </summary>
        WM_MOUSEMOVE = 0x200,

        /// <summary>
        /// 左键按下
        /// </summary>
        WM_LBUTTONDOWN = 0x201,

        /// <summary>
        /// 左键释放
        /// </summary>
        WM_LBUTTONUP = 0x202,

        /// <summary>
        /// 左键双击
        /// </summary>
        WM_LBUTTONDBLCLK = 0x203,

        /// <summary>
        /// 右键按下
        /// </summary>
        WM_RBUTTONDOWN = 0x204,

        /// <summary>
        /// 右键释放
        /// </summary>
        WM_RBUTTONUP = 0x205,

        /// <summary>
        /// 右键双击
        /// </summary>
        WM_RBUTTONDBLCLK = 0x206,

        /// <summary>
        /// 中键按下
        /// </summary>
        WM_MBUTTONDOWN = 0x207,

        /// <summary>
        /// 中键释放
        /// </summary>
        WM_MBUTTONUP = 0x208,

        /// <summary>
        /// 中键双击
        /// </summary>
        WM_MBUTTONDBLCLK = 0x209,

        /// <summary>
        /// 滚轮滚动
        /// </summary>
        /// <remarks>WINNT4.0以上才支持此消息</remarks>
        WM_MOUSEWHEEL = 0x020A,

        /// <summary>
        /// 鼠标结束
        /// </summary>
        WM_MOUSELAST = 0x020A
    }

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Performance", "CA1815:重写值类型上的 Equals 和相等运算符", Justification = "<挂起>")]
    [SuppressMessage("Design", "CA1051:不要声明可见实例字段", Justification = "<挂起>")]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mousehookstruct?redirectedfrom=MSDN</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Performance", "CA1815:重写值类型上的 Equals 和相等运算符", Justification = "<挂起>")]
    [SuppressMessage("Design", "CA1051:不要声明可见实例字段", Justification = "<挂起>")]
    public struct MOUSEHOOKSTRUCT
    {
        public POINT pt;
        public IntPtr hwnd;
        public uint wHitTestCode;
        public uint dwExtraInfo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msllhookstruct?redirectedfrom=MSDN</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Performance", "CA1815:重写值类型上的 Equals 和相等运算符", Justification = "<挂起>")]
    [SuppressMessage("Design", "CA1051:不要声明可见实例字段", Justification = "<挂起>")]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public uint ExtraInfo;

        //POINT pt;
        //DWORD mouseData;
        //DWORD flags;
        //DWORD time;
        //ULONG_PTR dwExtraInfo;
    }


    /// <summary>
    /// 钩子委托
    /// </summary>
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
}
