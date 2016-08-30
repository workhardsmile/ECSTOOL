using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace ECSTOOL
{
    /// <summary>
    /// WindowsAPI常见操作封装
    /// </summary>
    public class ApiMethod
    {
        #region Dll Import
        /// <summary>
        /// 如果一个弹出式窗口存在，返回值为非零，即使该窗口被其他窗口完全覆盖。如果弹出式窗口不存在，返回值为零。 
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int AnyPopup();
        /// <summary>
        /// 得到窗体文本
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpString"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        /// <summary>
        /// 窗体子控件查找委托
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool CallBack(IntPtr hwnd, int lParam);
        /// <summary>
        /// 枚举一个父窗口的所有线程
        /// </summary>
        /// <param name="dwThreadId"></param>
        /// <param name="lpfn"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int EnumThreadWindows(IntPtr dwThreadId, CallBack lpfn, int lParam);
        /// <summary>
        /// 枚举一个父窗口的所有子窗口
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="lpfn"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="wMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="wMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessageA(IntPtr hwnd, int wMsg, int wParam, int lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// 获得窗体文本长度
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        /// <summary>
        /// 获得父窗体句柄
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        /// <summary>
        /// 根据类名或窗体名称查找窗体句柄
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName,
    string lpWindowName);
        /// <summary>
        /// 查找窗体子控件句柄
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpszClass"></param>
        /// <param name="lpszWindow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent,
    IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        /// <summary>
        /// 设置窗体显示位置
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hWndInsertAfter"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd,
        int hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        int uFlags
        );
        /// <summary>
        /// 获得窗体大小 得到的矩形 为窗体起始 到结束的 坐标。
        /// </summary>
        /// <param name="hwnd">窗体句柄</param>
        /// <param name="lpRect">装数据的矩形对象</param>
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hwnd, ref System.Drawing.Rectangle lpRect);
        /// <summary>
        /// 获得子窗体句柄
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindow")]
        public static extern IntPtr GetWindow(IntPtr hwnd, int cmd);
        /// <summary>
        /// 获得窗体 用户区域大小 其他同 GetWindowRect
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern int GetClientRect(IntPtr hwnd, ref System.Drawing.Rectangle lpRect);
        /// <summary>
        /// 获得子对话框句柄
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetDlgItem")]
        public static extern IntPtr GetDlgItem(IntPtr hwnd, int cmd);
        /// <summary>
        /// 通过全屏幕坐标获取控件句柄
        /// </summary>
        /// <param name="w">坐标</param>
        /// <returns>坐标位置的控件句柄</returns>
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(Point w);
        /// <summary>
        /// 移动窗体的Windows函数
        /// </summary>
        /// <param name="hWnd">窗体句柄</param>
        /// <param name="x">新坐标起点x坐标</param>
        /// <param name="y">先坐标末点y坐标</param>
        /// <param name="nWidth">新宽度</param>
        /// <param name="nHeight">新高度</param>
        /// <param name="BRePaint">是否移动后重绘制窗体（如果选false会导致桌面窗体图像滞留）</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
        /// <summary>
        /// 设置窗体显示方式，失败返回0
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, int code);
        /// <summary>
        /// 设置窗体显示最前端
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        public static extern long SetForegroundWindow(IntPtr hwnd);
        /// <summary>
        /// 获取当前活跃窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
        public static extern IntPtr GetActiveWindow();
        /// <summary>
        /// 获得最前端窗口句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();
        /// <summary>
        /// 使指定窗体获得键盘输入焦点
        /// </summary>
        /// <param name="hWnd">窗体句柄</param>
        /// <returns>若函数调用成功，则返回原先拥有键盘焦点的窗口句柄。若hWnd参数无效或窗口未与调用线程的消息队列相关，则返回值为NULL</returns>
        [DllImport("user32.dll", EntryPoint = "SetFocus")]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        /// <summary>
        /// 获得焦点句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetFocus")]
        public static extern IntPtr GetFocus();
        /// <summary>
        /// 取得捕获了鼠标的窗口(如果存在)的句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetCapture")]
        public static extern IntPtr GetCapture();
        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        /// <param name="x">指定位置x坐标</param>
        /// <param name="y">指定位置y坐标</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern bool SetCursorPos(int x, int y);
        /// <summary>
        /// 鼠标事件
        /// </summary>
        /// <param name="dwFlags">事件名称</param>
        /// <param name="dx">x位移</param>
        /// <param name="dy">y位移</param>
        /// <param name="dwData"></param>
        /// <param name="dwExtraInfo"></param>
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public extern static void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);
        /// <summary>
        /// 获取当前窗体最近一次活跃的窗口 获得messagebox
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetLastActivePopup")]
        public static extern IntPtr GetLastActivePopup(IntPtr hWnd);
        /// <summary>
        /// 获得窗口句柄的编号
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nlndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, int nlndex);
        /// <summary>
        /// 得到此控件的类名
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="classname">接收数据的要首先给出空间</param>
        /// <param name="nlndex">所要取得的最大字符数，如果设置为0 则什么都没有</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder classname, int nlndex);
        /// <summary>
        /// BitBlt dwRop parameter
        /// </summary>
        public const int SRCCOPY = 0x00CC0020; 
        /// <summary>
        /// 对指定的源设备环境区域中的像素进行位块（bit_block）转换，以传送到目标设备环境
        /// </summary>
        /// <param name="hObject"></param>
        /// <param name="nXDest"></param>
        /// <param name="nYDest"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="hObjectSource"></param>
        /// <param name="nXSrc"></param>
        /// <param name="nYSrc"></param>
        /// <param name="dwRop"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
         int nWidth, int nHeight, IntPtr hObjectSource,
         int nXSrc, int nYSrc, int dwRop);
        /// <summary>
        /// GDI图形用户界面相关程序，包含的函数用来绘制图像和显示文字
        /// </summary>
        /// <param name="hDC"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
         int nHeight);
        /// <summary>
        /// GDI图形用户界面相关程序，包含的函数用来绘制图像和显示文字
        /// </summary>
        /// <param name="hDC"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        /// <summary>
        /// GDI图形用户界面相关程序，包含的函数用来绘制图像和显示文字
        /// </summary>
        /// <param name="hDC"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        /// <summary>
        /// GDI图形用户界面相关程序，包含的函数用来绘制图像和显示文字
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        /// <summary>
        /// GDI图形用户界面相关程序，包含的函数用来绘制图像和显示文字
        /// </summary>
        /// <param name="hDC"></param>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        /// <summary>
        /// 键盘事件
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwExtralnfo"></param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtralnfo);
        /// <summary>
        /// 获取操作系统分辨率
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        /// <summary>
        /// 获取桌面上窗体句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        /// <summary>
        /// 返回hWnd参数所指定的窗口的设备环境
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        /// <summary>
        /// 取得窗体进程ID
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);
        /// <summary>
        /// 函数释放设备上下文环境(DC)供其他应用程序使用
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hDC"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        /// <summary>
        /// 获取ico图标
        /// </summary>
        /// <param name="lpszFile"></param>
        /// <param name="niconIndex"></param>
        /// <param name="phiconLarge"></param>
        /// <param name="phiconSmall"></param>
        /// <param name="nIcons"></param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern int ExtractIconEx(string lpszFile, int niconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);     
        #endregion


        #region 封装方法
        /// <summary>
        /// 杀掉指定进程
        /// </summary>
        /// <param name="proc_name"></param>
        public static void KillProc(string proc_name)
        {
            Process[] process = Process.GetProcesses();
            foreach (Process prc in process)
            {
                if (prc.ProcessName.ToLower().Equals(proc_name.ToLower()))
                {
                    try
                    {
                        prc.Kill();
                    }
                    catch { }
                }
            }

        }
        ///   <summary>    
        ///   窗口置前    
        ///   </summary>    
        ///   <param   name="hWnd">窗口句柄</param>    
        public static void SetWindowCurrent(IntPtr hWnd)
        {
            SetWindowPos(hWnd, -1, 0, 0, 0, 0, 0x4000 | 0x0001 | 0x0002);
        }
        /// <summary>
        /// 获取子控件的句柄
        /// </summary>
        /// <param name="parent">父窗体句柄</param>
        /// <param name="nIndex">索引</param>
        /// <returns></returns>
        /// 获取窗体上按钮的句柄，按钮上的文字为OK 
        /// IntPtr hwnd_button = ApiMethod.FindWindowEx(mainWnd, new IntPtr(0), null, "OK");
        public static IntPtr GetChildByID(IntPtr parent, int nIndex)
        {
            IntPtr result = IntPtr.Zero;
            //获取窗体上全部的子控件句柄
            ApiMethod.EnumChildWindows(parent, new ApiMethod.CallBack(delegate(IntPtr hwnd, int lParam)
            {
                int id = GetWindowLong(hwnd, ApiCode.GWL_ID);
                if (nIndex == id)
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }), 0);
            return result;
        }
        /// <summary>
        /// 得到控件的文本内容
        /// </summary>
        /// <param name="txtHand"></param>
        /// <returns></returns>
        public static string GetStringText(IntPtr txtHand)
        {
            String txt = new string(' ', 255);
            IntPtr txtPtr = Marshal.StringToHGlobalAnsi(txt);
            int result = SendMessage(txtHand, ApiCode.WM_GETTEXT, (IntPtr)255, txtPtr);
            if (result == 0)
            {
                return "";
            }
            else
                return Marshal.PtrToStringAnsi(txtPtr);
        }
        /// <summary>
        /// 向控件输入字符串
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool SendString(IntPtr hand, string text)
        {
            bool flag = true;
            char[] textChar = text.ToCharArray();
            foreach (char ch in textChar)
            {
                try
                {
                    PostMessage(hand, ApiCode.WM_CHAR, ch, 0);
                    //System.Threading.Thread.Sleep(10);
                }
                catch
                {
                    flag = false;
                    continue;
                }
            }
            return flag;
        }
        /// <summary>
        /// 模拟键盘向指定位置输入字符串
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool SendStringXY(IntPtr hand, string text, int x, int y)
        {
            bool flag = true;

            Rectangle windowRect = new Rectangle();
            GetWindowRect(hand, ref windowRect);
            SetCursorPos(windowRect.Left + x, windowRect.Top + y);
            MouseDown("left");
            MouseUp("left");
            try
            {
                SendKeys.SendWait(text);
            }
            catch
            {
                flag = false;
            }
            return flag;

        }
        /// <summary>
        /// 指定位置单击事件
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ClickPosXY(IntPtr parent, int x, int y)
        {
            bool flag = true;
            try
            {
                Rectangle windowRect = new Rectangle();
                GetWindowRect(parent, ref windowRect);
                SetCursorPos(windowRect.Left + x, windowRect.Top + y);
                MouseDown("left");
                MouseUp("left");
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        /// <summary>
        /// 通过进程名获得主窗口句柄
        /// </summary>
        /// <param name="processName">进程名</param>
        /// <returns>窗口句柄对象</returns>
        public static IntPtr GetProcessMainFormHandle(string processName)
        {
            Process pro = null;
            Process[] pros = Process.GetProcessesByName(processName);
            if (pros.Length > 0)
            {
                pro = pros[0];
            }
            IntPtr m_WindowHandle = IntPtr.Zero;
            m_WindowHandle = pro.MainWindowHandle;
            return m_WindowHandle;
        }
        /// <summary>
        /// 获得桌面的图像
        /// </summary>
        /// <returns></returns>
        public static Bitmap CaptureScreen()
        {
            return CaptureWindow(GetDesktopWindow());
        }
        /// <summary>
        /// 获得桌面的图像
        /// </summary>
        /// <returns></returns>
        public static Bitmap windowFullScreen()
        {
            //建立屏幕Graphics
            Graphics grpScreen = Graphics.FromHwnd(IntPtr.Zero);
            //根据屏幕大小建立位图
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, grpScreen);
            //建立位图相关Graphics
            Graphics grpBitmap = Graphics.FromImage(bitmap);
            //建立屏幕上下文
            IntPtr hdcScreen = grpScreen.GetHdc();
            //建立位图上下文
            IntPtr hdcBitmap = grpBitmap.GetHdc();
            //将屏幕捕获保存在图位中
            BitBlt(hdcBitmap, 0, 0, bitmap.Width, bitmap.Height, hdcScreen, 0, 0, 0x00CC0020);
            //关闭位图句柄
            grpBitmap.ReleaseHdc(hdcBitmap);
            //关闭屏幕句柄
            grpScreen.ReleaseHdc(hdcScreen);
            //释放位图对像
            grpBitmap.Dispose();
            //释放屏幕对像
            grpScreen.Dispose();

            //返回捕获位图
            return bitmap;
        }
        /// <summary>
        /// 通过窗体句柄获取此窗体的图像
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = GetWindowDC(handle);
            // get the size
            Rectangle windowRect = new Rectangle();
            GetWindowRect(handle, ref windowRect);
            int width = windowRect.Width - windowRect.X;
            int height = windowRect.Height - windowRect.Y;
            // create a device context we can copy to
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            // bitblt over
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
            // restore selection
            SelectObject(hdcDest, hOld);
            // clean up 
            DeleteDC(hdcDest);
            ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            DeleteObject(hBitmap);
            return (Bitmap)img;
        }
        /// <summary>
        /// 键盘弹起一个键
        /// </summary>
        /// <param name="key"></param>
        public static void SendKeyup(string key)
        {
            
            byte bkey = byte.Parse(key);
            keybd_event(bkey, 0, ApiCode.KEYEVENTF_KEYUP, 0);
        }
        /// <summary>
        /// 键盘按下一个键System.Windows.Forms.SendKeys.Send/SendWait
        /// </summary>
        /// <param name="key"></param>
        public static void SendKeydown(string key)
        {            
            byte bkey = byte.Parse(key);
            keybd_event(bkey, 0, ApiCode.KEYEVENTF_KEYDOWN, 0);
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="button"></param>
        public static void MouseDown(string button)
        {
            switch (button)
            {
                case "left":
                    mouse_event(ApiCode.Mouse_LeftDown, 0, 0, 0, IntPtr.Zero);
                    break;
                case "right":
                    mouse_event(ApiCode.Mouse_RightDown, 0, 0, 0, IntPtr.Zero);
                    break;
                case "middle":
                    mouse_event(ApiCode.Mouse_MiddleDown, 0, 0, 0, IntPtr.Zero);
                    break;

            }

        }
        /// <summary>
        /// 鼠标弹起
        /// </summary>
        /// <param name="button"></param>
        public static void MouseUp(string button)
        {
            switch (button)
            {
                case "left":
                    mouse_event(ApiCode.Mouse_LeftUp, 0, 0, 0, IntPtr.Zero);
                    break;
                case "right":
                    mouse_event(ApiCode.Mouse_RightUp, 0, 0, 0, IntPtr.Zero);
                    break;
                case "middle":
                    mouse_event(ApiCode.Mouse_MiddleUp, 0, 0, 0, IntPtr.Zero);
                    break;

            }
        }
        /// <summary>
        /// 鼠标移动到指定坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseMove(int x, int y)
        {
            SetCursorPos(x, y);
        }
        /// <summary>
        /// 获取系统宽度度
        /// </summary>
        /// <returns></returns>
        public static int GetSystemX()
        {
            return GetSystemMetrics(ApiCode.SM_CXSCREEN);
        }
        /// <summary>
        /// 获取系统高度
        /// </summary>
        /// <returns></returns>
        public static int GetSystemY()
        {
            return GetSystemMetrics(ApiCode.SM_CYSCREEN);
        }
        #endregion
    }
}
