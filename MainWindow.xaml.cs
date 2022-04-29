﻿using Microsoft.UI.Xaml;
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



        IntPtr hWnd = IntPtr.Zero;
        IntPtr hWndChild = IntPtr.Zero;
        private Microsoft.UI.Windowing.AppWindow _apw;
        private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

        private SUBCLASSPROC SubClassDelegate;

        public MainWindow()
        {
            this.InitializeComponent();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);

            _apw.Resize(new Windows.Graphics.SizeInt32(360, 360));
            _apw.Move(new Windows.Graphics.PointInt32(600, 400));

            _presenter = _apw.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            _presenter.IsResizable = false;
            //_presenter.IsMinimizable = false;
            _presenter.SetBorderAndTitleBar(false, false);           

            //hWndChild = FindWindowEx(hWnd, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);
            //ShowWindow(hWndChild, SW_HIDE);

            SubClassDelegate = new SUBCLASSPROC(WindowSubClass);
            bool bRet = SetWindowSubclass(hWnd, SubClassDelegate, 0, 0);

            long nExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((nExStyle & WS_EX_LAYERED) == 0)
            {
                SetWindowLong(hWnd, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_LAYERED));
                //nExStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                //SetWindowLong(hWnd, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_TRANSPARENT));

                //bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)MakeArgb(255, 255, 0, 0), 128, LWA_ALPHA | LWA_COLORKEY);
                bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta), 255, LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black), 255, LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black), 128, LWA_ALPHA | LWA_COLORKEY);
                ////bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)0, 128, LWA_ALPHA | LWA_COLORKEY);

                //bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)MakeArgb(255, 136, 23, 152), 255, LWA_COLORKEY);
            }

            UIElement root = (UIElement)this.Content;
            root.PointerMoved += Root_PointerMoved;
            root.PointerPressed += Root_PointerPressed;
            root.PointerReleased += Root_PointerReleased;
        }

        private int nX = 0, nY = 0, nXWindow = 0, nYWindow = 0;
        private bool bMoving = false;

        private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //nXWindow = _apw.Position.X;
            //nYWindow = _apw.Position.Y;
            bMoving = false;
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
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
                bool bReturn = SetLayeredWindowAttributes(hWnd, (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta), (byte)(255 * _Opacity / 100), LWA_ALPHA | LWA_COLORKEY);
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
