using CustomControl.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CustomControl
{
    public sealed class MouseHook : IDisposable
    {
        private readonly GridFlowLayoutPanel _gridFlowLayoutPanel;
        private readonly GridFlowLayoutEngine _gridFlowLayoutEngine;
        private readonly HookProc _hookProc;
        private readonly IntPtr _mouseHookId;

        private Control _currentControl;
        private Point _startDropLocation;

        private int _action;

        public MouseHook(GridFlowLayoutEngine gridFlowLayoutEngine)
        {
            _gridFlowLayoutEngine = gridFlowLayoutEngine;

            _hookProc = new HookProc(MouseHookProc);

            int currentThreadId = NativeMethods.GetCurrentThreadId();
            _mouseHookId = NativeMethods.SetWindowsHookEx(WH_Code.WH_MOUSE, _hookProc, IntPtr.Zero, currentThreadId);

            if (_mouseHookId == IntPtr.Zero)
            {
                var errorCode = NativeMethods.GetLastError();
                Debug.WriteLine(errorCode);
            }
        }

        public MouseHook(GridFlowLayoutPanel gridFlowLayoutPanel)
        {
            _gridFlowLayoutPanel = gridFlowLayoutPanel;

            _hookProc = new HookProc(MouseHookProc);

            int currentThreadId = NativeMethods.GetCurrentThreadId();
            _mouseHookId = NativeMethods.SetWindowsHookEx(WH_Code.WH_MOUSE, _hookProc, IntPtr.Zero, currentThreadId);

            if (_mouseHookId == IntPtr.Zero)
            {
                var errorCode = NativeMethods.GetLastError();
                Debug.WriteLine(errorCode);
            }
        }

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

            switch (wParam)
            {
                case (int)WM_MOUSE.WM_LBUTTONDOWN:
                    {
                        Debug.WriteLine(nameof(WM_MOUSE.WM_LBUTTONDOWN));

                        MOUSEHOOKSTRUCT mouseHookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));

                        foreach (Control control in _gridFlowLayoutPanel.Controls)
                        {
                            Point point = control.PointToClient(new Point(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));

                            // 调整坐标
                            if (point.Y >= 0
                                && point.Y <= 20
                                && point.X >= 0
                                && point.X <= control.ClientSize.Width
                                && Interlocked.CompareExchange(ref _currentControl, control, null) == null
                                && Interlocked.CompareExchange(ref _action, 1, 0) == 0)
                            {
                                _gridFlowLayoutPanel.OnDragStart(_currentControl);
                                _startDropLocation = _gridFlowLayoutPanel.PointToClient(new Point(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));
                                break;
                            }

                            // 调整大小
                            if (control.ClientSize.Height - point.Y >= 0
                                && control.ClientSize.Height - point.Y <= 10
                                && control.ClientSize.Width - point.X >= 0
                                && control.ClientSize.Width - point.X <= 10
                                && Interlocked.CompareExchange(ref _currentControl, control, null) == null
                                && Interlocked.CompareExchange(ref _action, 2, 0) == 0)
                            {
                                _gridFlowLayoutPanel.OnResizeStart(_currentControl);
                                break;
                            }
                        }

                        break;
                    }

                case (int)WM_MOUSE.WM_MOUSEMOVE:
                case (int)WM_MOUSE.WM_MOUSEWHEEL:
                    {
                        Debug.WriteLine(nameof(WM_MOUSE.WM_MOUSEMOVE));

                        if (_currentControl is null)
                        {
                            break;
                        }

                        MOUSEHOOKSTRUCT mouseHookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
                        Point point = _gridFlowLayoutPanel.PointToClient(new Point(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));

                        switch (_action)
                        {
                            case 1:
                                var offset = new Point(point.X - _startDropLocation.X, point.Y - _startDropLocation.Y);

                                // 将偏移后的值重新赋值给初始量
                                _startDropLocation.Offset(offset);

                                _gridFlowLayoutPanel.OnDrag(_currentControl, _currentControl.Location.X + offset.X, _currentControl.Location.Y + offset.Y);
                                break;
                            case 2:
                                _gridFlowLayoutPanel.OnResize(_currentControl, point.X - _currentControl.Location.X, point.Y - _currentControl.Location.Y);
                                break;
                            default:
                                break;
                        }

                        break;
                    }

                case (int)WM_MOUSE.WM_LBUTTONUP:
                    {
                        Debug.WriteLine(nameof(WM_MOUSE.WM_LBUTTONUP));

                        if (_currentControl is null)
                        {
                            break;
                        }

                        switch (_action)
                        {
                            case 1:
                                _gridFlowLayoutPanel.OnDragStop(_currentControl);
                                _currentControl = null;
                                _action = 0;
                                break;
                            case 2:
                                _gridFlowLayoutPanel.OnResizeStop(_currentControl);
                                _currentControl = null;
                                _action = 0; break;
                            default:
                                break;
                        }

                        break;
                    }
            }

            return NativeMethods.CallNextHookEx(this._mouseHookId, nCode, wParam, lParam);
        }
    }
}
