using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

// https://app.box.com/s/o04ju2fa3a6w4lfpsdciitxqbainq2hy

namespace WinUI3_Transparent
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                left = Left;
                top = Top;
                right = Right;
                bottom = Bottom;
            }
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public const int WM_SIZE = 0x0005;
        public const int WM_PAINT = 0x000F;
        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        public const int HTCAPTION = 2;

        public delegate int SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FillRect(IntPtr hdc, [In] ref RECT rect, IntPtr hbrush);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int ExcludeClipRect(IntPtr hdc, int left, int top, int right, int bottom);

        public const int WS_DLGFRAME = 0x00400000;

        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_ACCEPTFILES = 0x00000010;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_MDICHILD = 0x00000040;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_WINDOWEDGE = 0x00000100;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_CONTEXTHELP = 0x00000400;
        public const int WS_EX_RIGHT = 0x00001000;
        public const int WS_EX_LEFT = 0x00000000;
        public const int WS_EX_RTLREADING = 0x00002000;
        public const int WS_EX_LTRREADING = 0x00000000;
        public const int WS_EX_LEFTSCROLLBAR = 0x00004000;
        public const int WS_EX_RIGHTSCROLLBAR = 0x00000000;
        public const int WS_EX_CONTROLPARENT = 0x00010000;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
        public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
        public const int WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
        public const int WS_EX_COMPOSITED = 0x02000000;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

        public const uint LWA_COLORKEY = 0x00000001;
        public const uint LWA_ALPHA = 0x00000002;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
        {
            return (long)(((ulong)((((red << 0x10) | (green << 8)) | blue) | (alpha << 0x18))) & 0xffffffffL);
        }

        const int GWL_STYLE = (-16);
        const int GWL_EXSTYLE = (-20);
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static long GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern long GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern long GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }

        public const int GCLP_MENUNAME = (-8);
        public const int GCLP_HBRBACKGROUND = (-10);
        public const int GCLP_HCURSOR = (-12);
        public const int GCLP_HICON = (-14);
        public const int GCLP_HMODULE = (-16);
        public const int GCLP_WNDPROC = (-24);
        public const int GCLP_HICONSM = (-34);

        [DllImport("user32.dll", EntryPoint = "SetClassLong")]
        public static extern IntPtr SetClassLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
        public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);      

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nShowCmd);

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int PostMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetCursorPos(out Windows.Graphics.PointInt32 lpPoint);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        public const int SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("Gdi32.dll", SetLastError = true)]
        public static extern IntPtr GetStockObject(int stockObject);

        public const int WHITE_BRUSH = 0;
        public const int LTGRAY_BRUSH = 1;
        public const int GRAY_BRUSH = 2;
        public const int DKGRAY_BRUSH = 3;
        public const int BLACK_BRUSH = 4;
        public const int NULL_BRUSH = 5;
        public const int HOLLOW_BRUSH = NULL_BRUSH;

        public enum HRESULT : int
        {
            S_OK = 0,
            S_FALSE = 1,
            E_NOTIMPL = unchecked((int)0x80004001),
            E_NOINTERFACE = unchecked((int)0x80004002),
            E_POINTER = unchecked((int)0x80004003),
            E_FAIL = unchecked((int)0x80004005),
            E_UNEXPECTED = unchecked((int)0x8000FFFF),
            E_OUTOFMEMORY = unchecked((int)0x8007000E),
        }

        [DllImport("Dwmapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        public const int DWM_BB_ENABLE = 0x00000001;
        public const int DWM_BB_BLURREGION = 0x00000002;
        public const int DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_PASSIVE_UPDATE_MODE,
            DWMWA_USE_HOSTBACKDROPBRUSH,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR,
            DWMWA_CAPTION_COLOR,
            DWMWA_TEXT_COLOR,
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
            DWMWA_SYSTEMBACKDROP_TYPE,
            DWMWA_LAST
        };

        public enum DWMNCRENDERINGPOLICY
        {
            DWMNCRP_USEWINDOWSTYLE, // Enable/disable non-client rendering based on window style
            DWMNCRP_DISABLED,       // Disabled non-client rendering; window style is ignored
            DWMNCRP_ENABLED,        // Enabled non-client rendering; window style is ignored
            DWMNCRP_LAST
        };

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        [DllImport("Dwmapi.dll", SetLastError = true, CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern HRESULT DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute, uint cbAttribute);

        [DllImport("Dwmapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);


        [DllImport("CoreMessaging.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT CreateDispatcherQueueController(DispatcherQueueOptions options, /*PDISPATCHERQUEUECONTROLLER**/ [MarshalAs(UnmanagedType.IUnknown)] out object dispatcherQueueController);

        public enum DISPATCHERQUEUE_THREAD_TYPE
        {
            DQTYPE_THREAD_DEDICATED = 1,
            DQTYPE_THREAD_CURRENT = 2,
        };

        public enum DISPATCHERQUEUE_THREAD_APARTMENTTYPE
        {
            DQTAT_COM_NONE = 0,
            DQTAT_COM_ASTA = 1,
            DQTAT_COM_STA = 2
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct DispatcherQueueOptions
        {
            public uint dwSize;
            public DISPATCHERQUEUE_THREAD_TYPE threadType;
            public DISPATCHERQUEUE_THREAD_APARTMENTTYPE apartmentType;
        }


        IntPtr hWndMain = IntPtr.Zero;
        IntPtr hWndChild = IntPtr.Zero;
        private Microsoft.UI.Windowing.AppWindow _apw;
        private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

        private SUBCLASSPROC SubClassDelegate;

        private Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop? m_pSupportsSystemBackdrop = null;
        private static Windows.UI.Composition.Compositor? m_WindowsCompositor;
        private static Windows.System.DispatcherQueue? m_DispatcherQueue;

        public MainWindow()
        {
            this.InitializeComponent();

            hWndMain = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWndMain);

            //long nExStyle = GetWindowLong(hWndMain, GWL_EXSTYLE);
            //long nRet = SetWindowLong(hWndMain, GWL_EXSTYLE, (nint)(nExStyle & ~WS_EX_WINDOWEDGE));
            //SetWindowPos(hWndMain, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            //nExStyle = GetWindowLong(hWndMain, GWL_EXSTYLE);

            long nStyle = GetWindowLong(hWndMain, GWL_STYLE);
            long nRet = SetWindowLong(hWndMain, GWL_STYLE, (nint)(nStyle & ~WS_DLGFRAME));
            nStyle = GetWindowLong(hWndMain, GWL_STYLE);

            _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
            _apw.Resize(new Windows.Graphics.SizeInt32(360, 360));
            _apw.Move(new Windows.Graphics.PointInt32(600, 400));

            _presenter = _apw.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            _presenter.IsResizable = false;
            //_presenter.IsMinimizable = false;
            _presenter.SetBorderAndTitleBar(false, false);          

            //DWM_BLURBEHIND bb = new DWM_BLURBEHIND()
            //{
            //    dwFlags = DWM_BB_ENABLE | DWM_BB_BLURREGION,
            //    fEnable = true,
            //    //hRgnBlur = IntPtr.Zero,                
            //    //hRgnBlur = CreateRectRgn(-2, -1, 0, 0)
            //    //hRgnBlur = CreateRectRgn(0, 0, 90, 90),
            //};
            //HRESULT hr = DwmEnableBlurBehindWindow(hWndMain, ref bb);

            // Windows 11
            int nValue = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DEFAULT;
            HRESULT hr = DwmSetWindowAttribute(hWndMain, (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref nValue, Marshal.SizeOf(typeof(int)));

            //ExtendsContentIntoTitleBar = true;
            //SetTitleBar(AppTitleBar);

            // WS_EX_NOREDIRECTIONBITMAP;
            //hWndChild = FindWindowEx(hWndMain, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);
            hWndChild = FindWindowEx(hWndMain, IntPtr.Zero, "Microsoft.UI.Content.DesktopChildSiteBridge", null);

            //ShowWindow(hWndChild, SW_HIDE);

            //SetClassLong(hWndMain, GCLP_HBRBACKGROUND, GetStockObject(NULL_BRUSH));
            //SetClassLong(hWndChild, GCLP_HBRBACKGROUND, GetStockObject(NULL_BRUSH));

            SubClassDelegate = new SUBCLASSPROC(WindowSubClass);
            //bool bRet = SetWindowSubclass(hWndMain, SubClassDelegate, 0, 0);
            //bool bRet2 = SetWindowSubclass(hWndChild, SubClassDelegate, 0, 0);

            //SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(GetWindowLong(hWndMain, GWL_EXSTYLE) & ~WS_EX_WINDOWEDGE));

            // Can be commented...
            long nExStyle = GetWindowLong(hWndMain, GWL_EXSTYLE);
            if ((nExStyle & WS_EX_LAYERED) == 0)
            {
                SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_LAYERED));
                //nExStyle = GetWindowLong(hWndMain, GWL_EXSTYLE);
                //SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_TRANSPARENT));

                //bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)MakeArgb(255, 255, 0, 0), 128, LWA_ALPHA | LWA_COLORKEY);
                bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta), 255, LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black), 255, LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black), 128, LWA_ALPHA | LWA_COLORKEY);
                ////bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)0, 128, LWA_ALPHA | LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)MakeArgb(255, 136, 23, 152), 255, LWA_COLORKEY);

                bool bRet = SetWindowSubclass(hWndMain, SubClassDelegate, 0, 0);

            }

            UIElement root = (UIElement)this.Content;
            root.PointerMoved += Root_PointerMoved;
            root.PointerPressed += Root_PointerPressed;
            root.PointerReleased += Root_PointerReleased;

            InitializeWindowsComposition(hWndMain);

            m_pSupportsSystemBackdrop = this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>();
            if (m_WindowsCompositor != null && m_pSupportsSystemBackdrop != null)
            {
                //var pBackdropBrush = m_WindowsCompositor.CreateBackdropBrush();
                //var p = pBackdropBrush.Properties;
                //p.InsertColor("TintColor", Microsoft.UI.Colors.Aqua);               
                //m_pSupportsSystemBackdrop.SystemBackdrop = pBackdropBrush;

                //var pTransparentBrush = m_WindowsCompositor.CreateColorBrush(Windows.UI.Color.FromArgb(100, 255, 0, 0));
                var pTransparentBrush = m_WindowsCompositor.CreateColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                m_pSupportsSystemBackdrop.SystemBackdrop = pTransparentBrush;

            }
            var margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hWndMain, ref margins);

        }

        private void InitializeWindowsComposition(IntPtr hWnd)
        {
            HRESULT hr = HRESULT.S_OK;

            // -2147417842 0x8001010e RPC_E_WRONG_THREAD if DesktopAcrylicBackdrop in XAML
            //hr = CreateDispatcherQueueController(options, out object dqc);

            m_DispatcherQueue = Windows.System.DispatcherQueue.GetForCurrentThread();
            if (m_DispatcherQueue == null)
            {
                DispatcherQueueOptions options = new DispatcherQueueOptions
                {
                    threadType = DISPATCHERQUEUE_THREAD_TYPE.DQTYPE_THREAD_CURRENT,
                    apartmentType = DISPATCHERQUEUE_THREAD_APARTMENTTYPE.DQTAT_COM_STA,
                    dwSize = (uint)Marshal.SizeOf(typeof(DispatcherQueueOptions))
                };
                hr = CreateDispatcherQueueController(options, out object dqc);
            }

            if (hr == HRESULT.S_OK && m_WindowsCompositor == null)
            {
                m_WindowsCompositor = new Windows.UI.Composition.Compositor();              
            }
        }

        private int nX = 0, nY = 0, nXWindow = 0, nYWindow = 0;
        private bool bMoving = false;

        private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //nXWindow = _apw.Position.X;
            //nYWindow = _apw.Position.Y;
            ((UIElement)sender).ReleasePointerCaptures();
            bMoving = false;
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                ((UIElement)sender).CapturePointer(e.Pointer);
                nXWindow = _apw.Position.X;
                nYWindow = _apw.Position.Y;
                Windows.Graphics.PointInt32 pt;
                GetCursorPos(out pt);
                nX = pt.X;
                nY = pt.Y;
                //Console.Beep(1000, 10);
                bMoving = true;
            }
            else if (properties.IsRightButtonPressed)
            {
                System.Threading.Thread.Sleep(200);
                Application.Current.Exit();
            }
        }

        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Microsoft.UI.Input.PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
            //Point ptElement = new Point(pp.Position.X, pp.Position.Y);
            //IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(ptElement, (UIElement)sender);
            //foreach (UIElement item in elementStack)
            //{
            //    FrameworkElement feItem = item as FrameworkElement;
            //    //cast to FrameworkElement, need the Name property
            //    if (feItem != null)
            //    {
            //        if (feItem.Name.Equals("myButton"))
            //        {
            //            return;
            //        }
            //    }
            //}

            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            { 
                //Console.Beep(8000, 10);

                Windows.Graphics.PointInt32 pt;
                GetCursorPos(out pt);

                //if (((UIElement)sender).GetType() == typeof(StackPanel))
                if (bMoving)
                    _apw.Move(new Windows.Graphics.PointInt32(nXWindow + (pt.X- nX), nYWindow + (pt.Y - nY)));

                //Microsoft.UI.Input.PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
                //pt.X -= (int)pp.Position.X;
                //pt.Y -= (int)pp.Position.Y;
                //pt.X -=8;
                //pt.Y -= 31;
                //Windows.Graphics.PointInt32 pt = new Windows.Graphics.PointInt32((int)pp.Position.X, (int)pp.Position.Y);               
                //IntPtr pPoint = Marshal.AllocHGlobal(Marshal.SizeOf(pt));
                //Marshal.StructureToPtr(pt, pPoint, false);
                //PostMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, pPoint);
                e.Handled = true;
            }
        }

        private async void Click()
        {
            StackPanel sp = new StackPanel();
            FontIcon fi = new FontIcon()
            {
                FontFamily = new FontFamily("Segoe UI Emoji"),
                Glyph = "\U0001F439",
                FontSize = 50
            };
            sp.Children.Add(fi);
            TextBlock tb = new TextBlock();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Text = "You clicked on the Button !";
            sp.Children.Add(tb);
            ContentDialog cd = new ContentDialog()
            {
                Title = "Information",
                Content = sp,
                CloseButtonText = "Ok"
            };
            cd.XamlRoot = this.Content.XamlRoot;
            var res = await cd.ShowAsync();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            //myButton.Content = "Clicked";           
            //Opacity = 50;
            Click();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private ushort? _Opacity = 100;

        private ushort? Opacity
        {
            get => _Opacity;
            set
            {
                _Opacity = value;
                bool bReturn = SetLayeredWindowAttributes(hWndMain, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta), (byte)(255 * _Opacity / 100), LWA_ALPHA | LWA_COLORKEY);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Opacity)));
            }
        }
        public double SetOpacity(ushort? x) => (double)_Opacity;
        public ushort? GetOpacity(double x) => Opacity = (ushort)x;

        private int WindowSubClass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData)
        {
            switch (uMsg)
            {
                case WM_ERASEBKGND:
                    { 
                        RECT rect;
                        GetClientRect(hWnd, out rect);
                        //int nRet = ExcludeClipRect(wParam, 0, 0, rect.right, 35);
                        IntPtr hBrush = CreateSolidBrush(System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta));                    
                        //IntPtr hBrush = CreateSolidBrush((int)MakeArgb(255, 255, 0, 0));                       
                        //IntPtr hBrush = CreateSolidBrush(System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(255, 32, 32, 32)));
                        FillRect(wParam, ref rect, hBrush);
                        DeleteObject(hBrush);
                        return 1;
                    }
                    break;
            }
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
    }
}
