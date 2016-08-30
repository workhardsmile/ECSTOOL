import clr, sys
sys.path.append("C:\\Program Files\\IronPython 2.7\\Lib")
import os
clr.AddReference('IronPythonTest')
clr.AddReference('System.Windows.Forms')
from IronPythonTest import *
from System.Windows.Forms import *
from time import sleep

os.popen("C: && C:\\Windows\\System32\\calc.exe","r")
#os.system("C: && C:\\Windows\\System32\\calc.exe")
#win32api.ShellExecute(0, 'open', 'C:\\Windows\\System32\\calc.exe', '','',0)
#s_hwnd = ApiMethod.FindWindow(None, "IronPythonTest (正在运行) - Microsoft Visual Studio");
s_hwnd = ApiMethod.GetProcessMainFormHandle("devenv");
ApiMethod.ShowWindow(s_hwnd, ApiCode.SW_MAXIMIZE);
ApiMethod.SetForegroundWindow(s_hwnd);
sleep(3.0);
ApiMethod.SendStringXY(s_hwnd, "你好", 531, 66);
#from System.Windows.Forms.SendKeys
SendKeys.SendWait("~");
mainWnd = ApiMethod.GetProcessMainFormHandle("calc");
#_mainWnd = ApiMethod.FindWindow(None, "计算器");
#_hwnd = ApiMethod.FindWindow("CalcFrame", None);
sleep(1.0);
ApiMethod.ShowWindow(mainWnd, ApiCode.SW_SHOWDEFAULT);
ApiMethod.SetForegroundWindow(mainWnd);
sleep(1.0);
#ApiMethod.SetWindowCurrent(mainWnd);
#hwnd_button = ApiMethod.FindWindowEx(mainWnd, new IntPtr(0), null, "OK");
hwnd_button = ApiMethod.GetChildByID(mainWnd, 121);
ApiMethod.SendString(mainWnd,"1+2+4");
sleep(2.0);
ApiMethod.SendString(mainWnd, "*2");
sleep(2.0);
ApiMethod.SendMessage(hwnd_button,ApiCode.WM_CLICK , mainWnd, "0");
sleep(1.0);
SendKeys.SendWait("%{2}");
sleep(1.0);
SendKeys.SendWait("^{h}");
sleep(1.0);
flag=ApiMethod.ClickPosXY(mainWnd, 35, 38);
sleep(1.0);
flag=ApiMethod.ClickPosXY(mainWnd, 35, 38);
sleep(1.0);
SendKeys.SendWait("{F1}");
test=Test()
test.Hello()