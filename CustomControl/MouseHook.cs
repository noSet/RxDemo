using CustomControl.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CustomControl
{
    public sealed class MouseHook : IDisposable
    {
        private readonly HookProc _hookProc;

        private IntPtr _mouseHookId;

        public MouseHook(GridFlowLayoutPanel owner)
        {
            _hookProc = new HookProc(MouseHookProc);

            int currentThreadId = NativeMethods.GetCurrentThreadId();
            _mouseHookId = NativeMethods.SetWindowsHookEx(WH_Code.WH_MOUSE, _hookProc, IntPtr.Zero, currentThreadId);

            if (_mouseHookId == IntPtr.Zero)
            {
                var errorCode = NativeMethods.GetLastError();
                Debug.WriteLine(errorCode);
            }

            Owner = owner;
        }

        public GridFlowLayoutPanel Owner { get; }

        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(_mouseHookId);
        }

        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode <= 0)
            {
                return NativeMethods.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
            }

            // 鼠标移动事件
            switch (wParam)
            {
                case (int)WM_MOUSE.WM_MOUSEMOVE:
                case (int)WM_MOUSE.WM_MOUSEWHEEL:
                    {
                        MOUSEHOOKSTRUCT mouseHookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));

                        if (!Owner.ControlHandles.Contains(mouseHookStruct.hwnd))
                        {
                            break;
                        }

                        var a = Form.FromHandle(mouseHookStruct.hwnd);
                        Console.WriteLine($"X ={mouseHookStruct.pt.X}, Y = { mouseHookStruct.pt.Y}");
                        break;
                    }

                case (int)WM_MOUSE.WM_LBUTTONDOWN:
                    {
                        MOUSEHOOKSTRUCT mouseHookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));

                        if (!Owner.ControlHandles.Contains(mouseHookStruct.hwnd))
                        {
                            break;
                        }

                        MouseButtons button = MouseButtons.None;
                        int clickCount = 0;

                        switch (wParam)
                        {
                            case (int)WM_MOUSE.WM_MBUTTONDOWN:
                                button = MouseButtons.Middle;
                                clickCount = 1;
                                break;
                            case (int)WM_MOUSE.WM_MBUTTONUP:
                                button = MouseButtons.Middle;
                                clickCount = 0;
                                break;
                        }

                        MouseEventArgs mouseEvent = new MouseEventArgs(button, clickCount, mouseHookStruct.pt.X, mouseHookStruct.pt.Y, 0);
                        Console.WriteLine($"X ={mouseHookStruct.pt.X}, Y = { mouseHookStruct.pt.Y}");
                        break;
                    }
            }

            return NativeMethods.CallNextHookEx(this._mouseHookId, nCode, wParam, lParam);
        }
    }
}
