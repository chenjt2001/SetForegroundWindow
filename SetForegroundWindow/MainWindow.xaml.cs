using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SetForegroundWindow
{
    public struct WindowInfo

    {
        public IntPtr hWnd;
        public string szWindowName;
    }


    public partial class MainWindow : Window
    {

        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);
        WindowInfo[] infos = { };


        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);


        // “置顶”按钮被点击
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd;
            int result;

            hWnd = IntPtr.Zero;
            foreach (WindowInfo info in infos)
                if (info.szWindowName == WindowNameComboBox.Text)
                    hWnd = info.hWnd;

            if (hWnd == IntPtr.Zero)
            {
                Failure();
                return;
            }

            try
            {
                SetForegroundWindow(hWnd);
                //ShowWindow(hWnd, 5);

                SetWindowPos(hWnd, 0, 0, 0, 0, 0, 0x001 | 0x002 | 0x040);
                result = SetWindowPos(hWnd, -1, 0, 0, 0, 0, 0x001 | 0x002 | 0x040);

                if (result == 1)
                    Success();
                else
                    Failure();
            }

            catch
            {
                Failure();
            }

        }

        // 设置成功
        private void Success()
        {
            MessageBox.Show("设置成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 设置失败
        private void Failure()
        {
            MessageBox.Show("设置失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public WindowInfo[] GetAllDesktopWindows()
        {

            //用来保存窗口对象 列表
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows 
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);

                //get hwnd 
                wnd.hWnd = hWnd;

                //get window name  
                GetWindowText(hWnd, sb, sb.Capacity);

                wnd.szWindowName = sb.ToString();

                //add it into list 

                wndList.Add(wnd);
                return true;
            }, 0);

            return wndList.ToArray();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> titles;

            infos = GetAllDesktopWindows();
            titles = new List<string>();


            foreach (WindowInfo info in infos)
            {
                if (info.szWindowName != "" && IsWindowVisible(info.hWnd))
                    titles.Add(info.szWindowName);
            }

            WindowNameComboBox.ItemsSource = titles;

        }
    }
}
