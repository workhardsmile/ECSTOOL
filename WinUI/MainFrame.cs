using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using PlugsRoot;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using LuaInterface;

namespace ECSTOOL
{
    public delegate void DelegateOpenForm(string param);
    public delegate void ApplicationExit(int second);
    public partial class MainFrame : Form, IApplication
    {
        public CommandForm cmdForm;
        public PlugsForm plugsForm;
        public ServerList serverForm;
        public ServerOperator operForm;

        public FileSendP2P FSP;
        public FileReceiveP2P FRP;
        public SshServer LinuxServer;
        public ServerReceiver WinServer;
        public ClientReceiver WinClient;
        public SendSocket FileSender;
        public ReceiveSocket FileReceiver;
        public Queue<MessagePackage> MessageQueue;  //待处理数据包队列（主线程）

        public event DelegateOutputMessage EventOutputMessage;
        public DelegateOpenForm EventOpenForm;
        public DelegateLoadComboList DelegateLoadComboListF;
        public ApplicationExit ApplicationExitF;

        private Cron ctask = new Cron();
        private Thread ThreadFSP;
        private Thread ThreadFRP;
        private Thread SendThread;
        private Thread ListenThread;
        private Thread DownloadThread;
        private object SendContext;
        private object DownloadContext;
        public LuaFramework LuaPlus;
        private WinCMD CMD;
        public int FileType;

        public ArrayList hReturn = new ArrayList();
        public LogManager LogWriter = new LogManager(Path.Combine(Program.startPath, "Log\\ScriptLog.txt"));

        #region cron定时任务
        public void CronTask(object state)
        {
            ctask.OnStart();
            while (true)
            {
                while (Cron.MessageQueue.Count > 0)
                {
                    lock (Cron.MessageQueue)
                    {
                        MessagePackage datagram = Cron.MessageQueue.Dequeue();  // 取队列数据
                        lock (this.MessageQueue)
                        {
                            this.MessageQueue.Enqueue(datagram);
                        }
                    }
                }
                Thread.Sleep(60000);
                ctask.RefreshTask();
            }
        }

        #endregion

        #region 文件分发下载
        /// <summary>
        /// Linux文件上传
        /// </summary>
        /// <param name="etcinfo"></param>
        /// <param name="file"></param>
        /// <param name="dirs"></param>
        private void LinuxUploadFile(string[] etcinfo, string file, string[] dirs)
        {
            if (etcinfo.Length >= 4 && dirs.Length >= 1)
            {
                int port = 22;
                FileInfo fi = new FileInfo(file.Trim());
                try
                {
                    port = Convert.ToInt32(etcinfo[1]);
                }
                catch { }
                ScpClient_Ex sce = new ScpClient_Ex(etcinfo[0], port, etcinfo[2], etcinfo[3]);
                sce.File_Upload = file.Trim();
                sce.Upload_Name = fi.Name;
                //Thread temp = new Thread(sce.UploadFile);
                //temp.Start();
                sce.UploadFile();
                if (this.LinuxServer != null)
                {
                    this.Invoke(this.LinuxServer.DelegateMoveFileF, etcinfo[0], fi.Name, fi.Length, dirs, 10000);
                }
            }
            else
            {
                this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "LINUX_UPLOAD: 服务器配置或目录错误!", null);
            }
        }
        /// <summary>
        /// Linux文件下载
        /// </summary>
        /// <param name="etcinfo"></param>
        /// <param name="file"></param>
        /// <param name="dirs"></param>
        private void LinuxDownloadFile(string[] etcinfo, string file, string[] dirs)
        {
            if (etcinfo.Length >= 4 && dirs.Length >= 1)
            {
                int port = 22;
                try
                {
                    port = Convert.ToInt32(etcinfo[1]);
                }
                catch { }
                ScpClient_Ex sce = new ScpClient_Ex(etcinfo[0], port, etcinfo[2], etcinfo[3]);
                sce.Download_Name = file;
                int index = file.LastIndexOf("/");
                string name = file.Substring(index + 1);
                file = Path.Combine(dirs[0].Trim(), name);
                if (Directory.Exists(dirs[0]))
                {
                    sce.File_Download = file;
                    sce.DownloadFile();
                }
                WinCMD WC = new WinCMD();
                for (int i = 1; i < dirs.Length; i++)
                {
                    if (!Directory.Exists(dirs[i].Trim()))
                    {
                        continue;
                    }
                    if (File.Exists(file) && Directory.Exists(dirs[i]))
                    {
                        WC.Execute("copy /y \"" + file + "\" \"" + dirs[i] + "\"");
                    }
                }
                ScpClient_Ex.IP = etcinfo[0];
                ScpClient_Ex.Type = 2;
                ScpClient_Ex.IsFinished = true;
            }
            else
            {
                this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "LINUX_UPLOAD: 服务器配置或目录错误!", null);
            }
        }
        public void SendFiles()
        {
            if (this.SendContext != null)
            {
                SendFile(this.SendContext);
            }
        }
        /// <summary>
        /// Windows文件发送
        /// </summary>
        /// <param name="CommandContent"></param>
        /// <returns></returns>
        public void SendFile(object CommandContent)
        {
            //CommandContent = "F:\\python核心编程.pdf>10.34.130.62?C:\\firefox?C:\\firefox\\firefox fire|127.0.0.1?C:\\firefox|";
            string[] tasks = ((string)CommandContent).Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (tasks.Length >= 2)
            {
                string[] files = tasks[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < files.Length; k++)
                {
                    if (File.Exists(Path.Combine(Program.startPath, files[k])))
                    {
                        files[k] = Path.Combine(Program.startPath, files[k]);
                    }
                }
                if (files.Length > 0 && File.Exists(files[0]))
                {
                    if (this.FileSender != null)
                    {
                        this.FileSender.Close();
                        Thread.Sleep(1000);
                        this.FileSender = null;
                    }
                    string[] objs = tasks[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (objs.Length > 0)
                    {
                        this.FileSender = new SendSocket(Program.SenderPort, files);
                        this.FileSender.PkgCount = objs.Length;
                        this.ListenThread = new Thread(this.FileSender.BeginListen);
                        this.ListenThread.IsBackground = true;
                        this.ListenThread.Start();

                        for (int x = 0; x < objs.Length; x++)
                        {
                            string sendStr = Program.SenderPort + "|" + files.Length + "|";
                            string[] sub = objs[x].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                            if (sub.Length > 1)
                            {
                                sendStr += objs[x].Replace(sub[0] + "?", "");
                                if (Program.ListNetIP != null && Program.ListNetIP[sub[0]] != null)
                                {
                                    this.FileSender.PkgCount--;
                                    MessageType.AddMessageQueue(ref this.MessageQueue, Program.NetIP + "|" + sendStr, MessageType.DOWNLOAD_FILE, sub[0], Program.NetIP);
                                }
                                else
                                {
                                    MessageType.AddMessageQueue(ref this.MessageQueue, Program.IP + "|" + sendStr, MessageType.DOWNLOAD_FILE, sub[0], Program.IP);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //output
                    //MessageType.AddMessageQueue(ref this.MessageQueue, "SEND_FILE: 发送文件错误：格式错误或文件不存在。", MessageType.OUTPUT_COMMAND, Program.IP);
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "SEND_FILE: 发送文件错误：格式错误或文件不存在。", null);
                    return;
                }
            }
        }
        public void DownloadFiles()
        {
            if (this.DownloadContext != null)
            {
                DownloadFile(this.DownloadContext);
            }
        }
        /// <summary>
        /// Windows文件下载
        /// </summary>
        /// <param name="CommandContent"></param>
        /// <returns></returns>
        public void DownloadFile(object CommandContent)
        {
            //CommandContent = "127.0.0.1|18889|1|C:\\firefox?C:\\firefox\\firefox fire";
            string[] tasks = ((string)CommandContent).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (tasks.Length > 3)
            {
                try
                {
                    int port = Convert.ToInt32(tasks[1]);
                    int fileCount = Convert.ToInt32(tasks[2]);
                    string[] dirs = tasks[3].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        if (Directory.Exists(Path.Combine(Program.startPath, dirs[i])))
                        {
                            dirs[i] = Path.Combine(Program.startPath, dirs[i]);
                        }
                    }
                    if (dirs.Length > 0 && Directory.Exists(dirs[0]))
                    {
                        if (this.FileReceiver != null)
                        {
                            this.FileReceiver.Close();
                            Thread.Sleep(1000);
                            this.FileReceiver = null;
                        }
                        this.FileReceiver = new ReceiveSocket(tasks[0], port, dirs, fileCount);
                        this.FileReceiver.BeginReceive();
                    }
                }
                catch (Exception ex)
                {
                    //output
                    //MessageType.AddMessageQueue(ref this.MessageQueue, "DOWNLOAD_FILE: 下载文件错误：格式错误或目录不存在。", MessageType.OUTPUT_COMMAND, Program.IP);
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "DOWNLOAD_FILE: 下载文件错误：格式错误或目录不存在。", null);
                    return;
                }
            }
        }
        private bool CloseSendFile()
        {
            if (this.SendThread != null && this.SendThread.IsAlive)
            {
                this.SendThread.Abort();
                this.SendThread = null;
            }
            if (this.FileSender != null)
            {
                this.FileSender.Close();
                this.FileSender = null;
            }
            if (this.ThreadFRP != null && this.ThreadFRP.IsAlive)
            {
                this.ThreadFRP.Abort();
                this.ThreadFRP = null;
            }
            if (this.FRP != null)
            {
                this.FRP.IsStop = true;
                this.FileSender = null;
            }
            Thread.Sleep(10);
            return true;
        }
        private bool CloseDownloadFile()
        {
            if (this.DownloadThread != null && this.DownloadThread.IsAlive)
            {
                this.DownloadThread.Abort();
                this.DownloadThread = null;
            }
            if (this.FileReceiver != null)
            {
                this.FileReceiver.Close();
                this.FileReceiver = null;
            }
            if (this.ThreadFSP != null && this.ThreadFSP.IsAlive)
            {
                this.ThreadFSP.Abort();
                this.ThreadFSP = null;
            }
            if (FSP != null)
            {
                FSP = null;
            }
            Thread.Sleep(10);
            return true;
        }
        #endregion

        #region 委托事件
        public void AppExit(int second)
        {
            Thread.Sleep(1000 * second);
            Application.Exit();
        }
        public void ClosePlugForm(object sender, EventCloseForm e)
        {
            this.toolPlugToolStripMenuItem1.Checked = false; ;
        }
        public void CloseServerForm(object sender, EventCloseForm e)
        {
            this.serverListToolStripMenuItem.Checked = false;
        }
        public void CloseFileForm(object sender, EventCloseForm e)
        {
            this.fileTransToolStripMenuItem.Checked = false;
        }
        public void FileTransCommand(bool isSend, string paramIP, Hashtable ListIP)
        {
            if (operForm != null && !operForm.IsDisposed)
            {
                operForm.Close();
            }
            this.operForm = new ServerOperator(isSend, paramIP, ListIP);
            this.operForm.parent = this;
            operForm.Show(dockPanelMain, DockState.Document);
            this.operForm.DelagateCloseFileF += new DelegateCloseFile(CloseFileForm);
            this.fileTransToolStripMenuItem.Checked = true;
        }
        public void OpenCommand(string ListIP)
        {
            if (cmdForm != null && !cmdForm.IsDisposed)
            {
                cmdForm.Close();
            }
            this.cmdForm = new CommandForm(ListIP);
            this.cmdForm.parent = this;
            cmdForm.Show(dockPanelMain, DockState.Document);
            this.ironPythonToolStripMenuItem1.Checked = true;
        }
        #endregion

        #region LUA虚拟机
        public string scriptPath = "";
        public string scriptText = "";
        public Thread LuaExecThread;
        public void LuaExecFile()
        {
            if (this.LuaExecThread == null || !this.LuaExecThread.IsAlive)
            {
                if (scriptPath != "" && File.Exists(scriptPath))
                {
                    this.LuaExecThread = new Thread(LuaExec);
                    this.LuaExecThread.Start();
                    this.Invoke(this.cmdForm.DelegateSetEnableF, true);
                }
            }
        }
        public void LuaExec()
        {
            if (this.LuaPlus == null)
            {
                this.LuaPlus = new LuaFramework();
            }
            //MessageType MT = new MessageType();
            //this.LuaPlus.BindLuaApiClass(MT);
            this.LuaPlus.pLuaVM.RegisterFunction("ExecCommand", this, this.GetType().GetMethod("ExecCommand"));
            this.LuaPlus.pLuaVM["MyForm"] = this;

            try
            {
                object[] objRet = this.LuaPlus.pLuaVM.DoFile(scriptPath);
                LuaFunction EnterFunc = this.LuaPlus.pLuaVM.GetFunction("EnterBegin");
                if (EnterFunc != null)
                {
                    objRet = EnterFunc.Call();
                }
                if (objRet != null)
                {
                    string result = "LUA_RETURN: \n";
                    foreach (object obj in objRet)
                    {
                        result += obj + "\n";
                    }
                    MessageType.AddMessageQueue(ref this.MessageQueue, result, MessageType.OUTPUT_COMMAND, Program.IP);
                }
            }
            catch (Exception ex)
            {
                MessageType.AddMessageQueue(ref this.MessageQueue, ex.Message, MessageType.OUTPUT_COMMAND, Program.IP);
            }
            //执行完毕
            //this.Invoke(this.DelegateSetEnableF, true);
            this.Invoke(this.cmdForm.DelegateSetEnableF, false);
        }
        //ExecCommand("localhost,127.0.0.1:win_cmd{@C: && cd windows && dir@}")
        public void LuaExecText()
        {
            string result = "LUA_RETURN: \n";
            if (!scriptText.Equals(""))
            {
                if (this.LuaPlus == null)
                {
                    this.LuaPlus = new LuaFramework();
                }
                //MessageType MT = new MessageType();
                //this.LuaPlus.BindLuaApiClass(MT);
                this.LuaPlus.pLuaVM.RegisterFunction("ExecCommand", this, this.GetType().GetMethod("ExecCommand"));
                this.LuaPlus.pLuaVM["MyForm"] = this;

                try
                {
                    object[] objRet = this.LuaPlus.pLuaVM.DoString(scriptText);
                    LuaFunction EnterFunc = this.LuaPlus.pLuaVM.GetFunction("EnterBegin");
                    if (EnterFunc != null)
                    {
                        objRet = EnterFunc.Call();
                    }
                    if (objRet != null)
                    {
                        foreach (object obj in objRet)
                        {
                            result += obj + "\n";
                        }
                        MessageType.AddMessageQueue(ref this.MessageQueue, result, MessageType.OUTPUT_COMMAND, Program.IP);
                    }
                }
                catch (Exception ex)
                {
                    result += ex.Message;
                    MessageType.AddMessageQueue(ref this.MessageQueue, result, MessageType.OUTPUT_COMMAND, Program.IP);
                }
            }
            this.Invoke(this.cmdForm.DelegateSetEnableF, false);
        }
        #endregion

        #region 自定义命令方法
        /// <summary>
        /// 命令行支持
        /// </summary>
        /// <param name="Program.Args">-log "D:\\log_wugang.txt" -py "C:\\Users\\NHN\\Documents\\Visual Studio 2010\\Projects\\ECSTOOL\\ECSTOOL\\bin\Debug\\Scripts\\baidu1.py"</param>
        private void CommandExec()
        {
            if (Program.Args != null && Program.Args.Length > 0)
            {
                bool flag = true;
                string result;
                for (int i = 0; i < Program.Args.Length; i++)
                {
                    switch (Program.Args[i].ToLower())
                    {
                        case "-e":
                            if (File.Exists(Program.Args[i + 1]))
                            {
                                if (this.MessageQueue == null)
                                {
                                    this.MessageQueue = new Queue<MessagePackage>();
                                }
                                //this.scriptPath = Program.Args[i + 1];
                                MessageType.AddMessageQueue(ref this.MessageQueue, Program.Args[i + 1], MessageType.EXEC_SCRIPT, "localhost");
                                i++;
                            }
                            break;
                        case "-t":
                            int second = 0;
                            try
                            {
                                second = Convert.ToInt32(Program.Args[i + 1]);
                            }
                            catch { }
                            Thread.Sleep(second * 1000);
                            i++;
                            break;
                        case "-s":
                            flag = false;
                            break;
                        default:
                            result = "ECSTOOL Help:\r\nIronPythonTest.exe\t -log logPath ;执行命令后输出日志全路径,默认Log/";
                            //result += "\r\n\t\t -py filePath ;执行python脚本全路径";
                            result += "\r\n\t\t -e filePath ;执行lua脚本全路径";
                            result += "\r\n\t\t -t second ;等待时间S(秒)";
                            //result += "\r\n\t\t -o ;打开主窗体";
                            result += "\r\n\t\t -s ;即时退出系统";
                            MessageBox.Show(result, "命令行格式提示", MessageBoxButtons.OK);
                            flag = false;
                            break;
                    }
                    if (flag == false)
                    {
                        //Application.Exit();
                        this.Invoke(this.ApplicationExitF, 0);
                        break;
                    }
                }
            }
            Program.Args = null;
        }
        public void ExecCommand(string CommandContent)
        {
            //测试用
            //MessageType.AddMessageQueue(ref this.MessageQueue, CommandContent, MessageType.OUTPUT_COMMAND, Program.IP);
            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + CommandContent, null);
            MessageType.AddMessageQueue(ref this.MessageQueue, CommandContent, MessageType.COMMAND_ENGION, Program.IP);
        }
        public bool OpenWeb(string CommandContent)
        {
            bool flag = true;
            Process.Start(CommandContent);
            return flag;
        }
        public void StartOneSsh(object _CommandContent)
        {
            string CommandContent = (string)_CommandContent;
            if (this.LinuxServer != null && !this.LinuxServer._stopReceiver)
            {
                this.LinuxServer.ConnectOneServer(CommandContent);
            }
            else
            {
                this.LinuxServer.StartWork();
                this.LinuxServer.ConnectOneServer(CommandContent);
            }
        }
        public void CloseAllSsh(object _CommandContent)
        {
            this.LinuxServer.CloseAllSession();
        }
        public void CloseOneSsh(object _CommandContent)
        {
            string CommandContent = (string)_CommandContent;
            this.LinuxServer.CloseOneSession(CommandContent);
        }
        public void StartOneSocket(object _CommandContent)
        {
            string CommandContent = (string)_CommandContent;
            if (this.WinClient.IsRun)
            {
                this.WinClient.ConnectOneServer(CommandContent);
            }
            else
            {
                this.WinClient.StartReceiver();
                this.WinClient.ConnectOneServer(CommandContent);
            }
        }
        public void CloseAllSocket(object CommandContent)
        {
            this.WinClient.CloseAllSession();
        }
        public void CloseOneSocket(object _CommandContent)
        {
            string CommandContent = (string)_CommandContent;
            this.WinClient.CloseOneSession(CommandContent);
        }
        //public void RestartOneSocket(object _CommandContent)
        //{
        //    string CommandContent = (string)_CommandContent;
        //    if (this.WinClient.IsRun)
        //    {
        //        this.WinClient.ConnectOneServer(CommandContent);
        //    }
        //    else
        //    {
        //        this.WinClient.StartReceiver();
        //        this.WinClient.ConnectOneServer(CommandContent);
        //    }
        //}
        //public void RestartOneSsh(object _CommandContent)
        //{
        //    string CommandContent = (string)_CommandContent;
        //    if (!this.LinuxServer._stopReceiver)
        //    {
        //        this.LinuxServer.ConnectOneServer(CommandContent);
        //    }
        //    else
        //    {
        //        this.LinuxServer.StartWork();
        //        this.LinuxServer.ConnectOneServer(CommandContent);
        //    }
        //}
        public bool GetStatusSocket(string CommandContent)
        {
            bool flag = true;
            try
            {
                flag = this.WinClient.ClientSessionTable[CommandContent] == null ? false : true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        public bool GetStatusSsh(string CommandContent)
        {
            bool flag = true;
            try
            {
                flag = this.LinuxServer.SessionTable[CommandContent] == null ? false : true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        public bool CheckDirectory(string CommandContent)
        {
            bool flag = false;
            if (Directory.Exists(CommandContent))
            {
                flag = true;
            }
            return flag;
        }
        public long CheckFile(string CommandContent, long Size, int TimeOut)
        {
            long flag = 0;
            while (TimeOut > 0)
            {
                if (File.Exists(CommandContent))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(CommandContent);
                        flag = fi.Length;
                        if (flag >= Size)
                        {
                            break;
                        }
                    }
                    catch { }
                }
                Thread.Sleep(10);
            }
            return flag;
        }
        public bool DeleteOneServer(string CommandContent)
        {
            bool flag = false;
            try
            {
                StreamReader sr = new StreamReader(Program.EtcFile, Encoding.Default);
                string temp, result = "";
                while ((temp = sr.ReadLine()) != null)
                {
                    if (temp.Trim().Contains(CommandContent + ";"))
                    {
                        flag = true;
                        continue;
                    }
                    result += temp + "\n";
                }
                sr.Close();
                Thread.Sleep(100);
                StreamWriter sw = new StreamWriter(Program.EtcFile, false, Encoding.Default);
                sw.Write(result);
                sw.Close();
                result = null;
            }
            catch
            {
            }
            return flag;
        }
        public bool DeleteCron(string CommandContent)
        {
            bool flag = false;
            CommandContent = CommandContent.Trim();
            try
            {
                StreamReader sr = new StreamReader(this.ctask.crontabPath, Encoding.Default);
                string temp, result = "";
                while ((temp = sr.ReadLine()) != null)
                {
                    if (temp.Trim().Equals(CommandContent))
                    {
                        flag = true;
                        continue;
                    }
                    result += temp + "\n";
                }
                sr.Close();
                Thread.Sleep(100);
                StreamWriter sw = new StreamWriter(this.ctask.crontabPath, false, Encoding.Default);
                sw.Write(result);
                sw.Close();
                result = null;
            }
            catch
            {
            }
            return flag;
        }
        public bool AddCron(string CommandContent)
        {
            bool flag = false;
            try
            {
                StreamWriter sw = new StreamWriter(this.ctask.crontabPath, true, Encoding.Default);
                //StreamWriter sw = File.AppendText(this.ctask.crontabPath);
                sw.Write(CommandContent + "\n");
                sw.Close();
                flag = true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        public bool ReplaceRight(string FilePath, string Source, string Target)
        {
            bool flag = false;
            try
            {
                if (Source != null && !Source.Trim().Equals("") && File.Exists(FilePath.Trim()))
                {
                    string x = File.ReadAllText(FilePath);
                    bool z = x.Contains("\r\n") ? true : false;
                    x = null;
                    StreamReader sr = new StreamReader(FilePath, Encoding.Default);
                    string temp, result = "";
                    while ((temp = sr.ReadLine()) != null)
                    {
                        int index = temp.IndexOf(Source);
                        if (index >= 0)
                        {
                            temp = temp.Remove(index + Source.Length) + Target;
                            flag = true;
                        }
                        if (temp != "")
                        {
                            if (z)
                            {
                                result += temp + "\r\n";
                            }
                            else
                            {
                                result += temp + "\n";
                            }
                        }
                    }
                    sr.Close();
                    Thread.Sleep(100);
                    StreamWriter sw = new StreamWriter(FilePath, false, Encoding.Default);
                    sw.Write(result);
                    sw.Close();
                    result = null;
                }
            }
            catch
            {
            }
            return flag;
        }
        public bool LineReplace(string FilePath, string Source, string Target)
        {
            bool flag = false;
            try
            {
                if (Source != null && File.Exists(FilePath))
                {
                    string x = File.ReadAllText(FilePath);
                    bool z = x.Contains("\r\n") ? true : false;
                    x = null;
                    StreamReader sr = new StreamReader(FilePath, Encoding.Default);
                    string temp, result = "";
                    while ((temp = sr.ReadLine()) != null)
                    {
                        temp = temp.Replace(Source, Target);
                        if (!temp.Trim().Equals(""))
                        {
                            if (z)
                            {
                                result += temp + "\r\n";
                            }
                            else
                            {
                                result += temp + "\n";
                            }
                            flag = true;
                        }
                    }
                    sr.Close();
                    Thread.Sleep(100);
                    StreamWriter sw = new StreamWriter(FilePath, false, Encoding.Default);
                    sw.Write(result);
                    sw.Close();
                    result = null;
                }
            }
            catch
            {
            }
            return flag;
        }
        #endregion

        #region 任务队列处理
        /// <summary>
        /// 包处理线程
        /// </summary>
        /// <param name="state"></param>
        private void HandleMessage(object state)
        {
            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] SYS_START: 系统启动成功！", null);
            //this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] SYS_START: 系统启动成功！"+Program.IP+":"+Program.NetIP+":"+Program.IsServer, null);
            while (true)
            {
                if (this.MessageQueue.Count > 0)
                {
                    MessagePackage mp = null;
                    lock (this.MessageQueue)
                    {
                        mp = this.MessageQueue.Dequeue();
                        if (mp.ExecIP == null || mp.ExecIP.Equals(Program.IP) || mp.ExecIP.Equals(Program.NetIP) || mp.ExecIP.Equals("127.0.0.1") || mp.ExecIP.ToLower().Equals("localhost") || mp.ExecIP.Trim().Equals("") || (mp.ReturnIP != mp.ExecIP && mp.ReturnIP != Program.IP && mp.ReturnIP != Program.NetIP))
                        {
                            //服务器调试模式输出
                            if (Program.IsServer)
                            {
                                if (mp.MessageType != MessageType.OUTPUT_COMMAND && mp.MessageType != MessageType.OUTPUT_LOG && mp.MessageType != MessageType.OUTPUT_MESSAGE && mp.MessageType != MessageType.L_SEND_FILE && mp.MessageType != MessageType.L_SEND_FILES && mp.MessageType != MessageType.L_DOWNLOAD_FILE && mp.MessageType != MessageType.L_DOWNLOAD_FILES)
                                {
                                    //MessageType.AddMessageQueue(ref this.MessageQueue, mp.ReturnIP + ":" + mp.CommandContent, MessageType.OUTPUT_COMMAND, Program.IP);
                                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + mp.ReturnIP + ":" + mp.CommandContent, null);
                                }
                            }
                            SysIO fi = null;
                            string temp = null;
                            string[] path = null;
                            string[] command = null;
                            WinCMD wc;
                            string[] param;
                            string[] _wcheck;
                            string[] _lcheck;
                            switch (mp.MessageType)
                            {
                                //输出屏幕
                                case MessageType.OUTPUT_COMMAND:
                                    #region
                                    //假如输出窗口没打开，则打开
                                    this.Invoke(EventOpenForm, "cmd");
                                    if (this.hReturn.Count > 10)
                                    {
                                        string _ts1 = (string)this.hReturn[this.hReturn.Count - 2];
                                        string _ts2 = (string)this.hReturn[this.hReturn.Count - 1];
                                        this.hReturn.Clear();
                                        this.hReturn.Add(_ts1);
                                        this.hReturn.Add(_ts2);
                                    }
                                    this.hReturn.Add(mp.CommandContent);
                                    //事件委托方式输出
                                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + mp.CommandContent, null);
                                    break;
                                    #endregion
                                //输出消息框
                                case MessageType.OUTPUT_MESSAGE:
                                    #region
                                    MessageType.MessageBoxShow(mp.CommandContent, "info");
                                    break;
                                    #endregion
                                //输出日志
                                case MessageType.OUTPUT_LOG:
                                    #region
                                    // logName|content
                                    int index = mp.CommandContent.IndexOf("|");
                                    string logName = mp.CommandContent.Substring(0, index);
                                    if (logName != null && !logName.Equals(""))
                                    {
                                        LogManager lm = new LogManager(Path.Combine(Program.startPath, "Log\\" + logName + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
                                        lm.WriteLog(mp.CommandContent.Remove(0, index + 1));
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:command_type{@command1@}{@command2@}…
                                case MessageType.COMMAND_ENGION:
                                    #region
                                    int style = mp.CommandContent.IndexOf(":");
                                    string ipStr = mp.CommandContent.Substring(0, style);
                                    string cmdType = null;
                                    try
                                    {
                                        string _con = mp.CommandContent.ToLower();
                                        if (_con.Contains(":cron_add[[") || _con.Contains(":cron_del[["))
                                        {
                                            cmdType = mp.CommandContent.Substring(style + 1, mp.CommandContent.IndexOf("[[") - style - 1);
                                        }
                                        else
                                        {
                                            cmdType = mp.CommandContent.Substring(style + 1, mp.CommandContent.IndexOf(MessageType.StartKey) - style - 1);
                                        }
                                    }
                                    catch
                                    {
                                        //MessageType.AddMessageQueue(ref this.MessageQueue, "COMMAND_ERROR: 命令格式错误！", MessageType.OUTPUT_COMMAND, Program.IP);
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "COMMAND_ERROR: 命令格式错误！", null);
                                        break;
                                    }
                                    string[] commands = null;
                                    string _class = cmdType.Trim().ToLower();
                                    if (_class.Equals("cron_add") || _class.Equals("cron_del"))
                                    {
                                        commands = MessageType.GetCommandArray(mp.CommandContent, "[[", "]]");
                                    }
                                    else
                                    {
                                        commands = MessageType.GetCommandArray(mp.CommandContent);
                                    }
                                    switch (_class)
                                    {
                                        case "win_cmd":
                                            style = MessageType.EXEC_CMD;
                                            break;
                                        case "linux_shell":
                                            style = MessageType.EXEC_SHELL;
                                            break;
                                        case "win_api":
                                            style = MessageType.EXEC_WIN_API;
                                            break;
                                        case "open_exe":
                                            style = MessageType.OPEN_EXE;
                                            break;
                                        case "open_file":
                                            style = MessageType.OPEN_FILE;
                                            break;
                                        case "kill_proc":
                                            style = MessageType.KILL_PROC;
                                            break;
                                        case "open_web":
                                            style = MessageType.OPEN_WEB;
                                            break;
                                        case "exec_lua":
                                            style = MessageType.EXEC_SCRIPT;
                                            break;
                                        case "open_lua":
                                            style = MessageType.OPEN_SCRIPT;
                                            break;
                                        case "stop_lua":
                                            style = MessageType.STOP_SCRIPT;
                                            break;
                                        case "net_send_file":
                                            style = MessageType.N_SEND_FILE;
                                            break;
                                        case "send_net_file":
                                            style = MessageType.N_SEND_FILE;
                                            break;
                                        case "send_windows_file":
                                            style = MessageType.SEND_FILE;
                                            break;
                                        case "download_windows_file":
                                            style = MessageType.DOWNLOAD_FILES;
                                            break;
                                        case "send_linux_file":
                                            style = MessageType.L_SEND_FILES;
                                            break;
                                        case "download_linux_file":
                                            style = MessageType.L_DOWNLOAD_FILES;
                                            break;
                                        case "close_send_file":
                                            style = MessageType.CLOSE_SEND_FILE;
                                            break;
                                        case "close_download_file":
                                            style = MessageType.CLOSE_DOWNLOAD_FILE;
                                            break;
                                        case "get_windows_directories":
                                            style = MessageType.GET_DIRS;
                                            break;
                                        case "get_windows_files":
                                            style = MessageType.GET_FILES;
                                            break;
                                        case "get_connect_servers":
                                            style = MessageType.GET_CONNECT_SERVERS;
                                            break;
                                        case "connect_all_servers":
                                            style = MessageType.START_ALL_SERVER;
                                            break;
                                        case "connect_one_server":
                                            style = MessageType.START_ONE_SERVER;
                                            break;
                                        case "close_all_servers":
                                            style = MessageType.CLOSE_ALL_SERVER;
                                            break;
                                        case "close_one_server":
                                            style = MessageType.CLOSE_ONE_SERVER;
                                            break;
                                        case "get_status_server":
                                            style = MessageType.GET_STATUS_SERVER;
                                            break;
                                        case "delete_one_server":
                                            style = MessageType.DELETE_ONE_SERVER;
                                            break;
                                        case "get_server_type":
                                            style = MessageType.GET_SERVER_OSTYPE;
                                            break;
                                        case "get_all_servers":
                                            style = MessageType.GET_ALL_SERVERS;
                                            break;
                                        case "get_noconnect_servers":
                                            style = MessageType.GET_NOCONNECT_SERVERS;
                                            break;
                                        case "check_windows_file":
                                            style = MessageType.CHECK_FILE;
                                            break;
                                        case "check_windows_directory":
                                            style = MessageType.CHECK_DIRECTORY;
                                            break;
                                        case "check_linux_file":
                                            style = MessageType.L_CHECK_FILE;
                                            break;
                                        case "check_linux_directory":
                                            style = MessageType.L_CHECK_DIRECTORY;
                                            break;
                                        case "replace_right":
                                            style = MessageType.REPLACE_RIGHT;
                                            break;
                                        case "replace_text":
                                            style = MessageType.REPLACE_TEXT;
                                            break;
                                        case "send_mail":
                                            style = MessageType.SEND_MAIL;
                                            break;
                                        case "cron_add":
                                            style = MessageType.CRON_ADD;
                                            break;
                                        case "cron_del":
                                            style = MessageType.CRON_DEL;
                                            break;
                                        case "view_file":
                                            style = MessageType.VIEW_FILE;
                                            break;
                                        case "run_cmd":
                                            style = MessageType.RUN_CMD;
                                            break;
                                        case "out_put":
                                            style = MessageType.OUTPUT_COMMAND;
                                            break;
                                        default:
                                            int _style = cmdType.IndexOf(":");
                                            if (_style > 0)
                                            {
                                                //ipStr = cmdType.Trim().Substring(0, _style);
                                                commands = new string[] { mp.CommandContent.Remove(0, style + 1) };
                                                style = MessageType.COMMAND_ENGION;
                                            }
                                            else
                                            {
                                                style = MessageType.OUTPUT_COMMAND;
                                            }
                                            break;
                                        //case "reconnect_all_server":
                                        //    style = MessageType.RESTART_ALL_SERVER;
                                        //    break;
                                        //case "reconnect_one_server":
                                        //    style = MessageType.RESTART_ONE_SERVER;
                                        //    break;
                                        //case "add_one_server":
                                        //    style = MessageType.ADD_ONE_SERVER;
                                        //    break;
                                        //case "get_error_servers":
                                        //    style = MessageType.GET_ERROR_SERVERS;
                                        //    break;
                                    }
                                    string[] IpList = ipStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    if ((style == MessageType.EXEC_SHELL || style == MessageType.L_CHECK_DIRECTORY || style == MessageType.L_CHECK_FILE || style == MessageType.L_DOWNLOAD_FILE || style == MessageType.L_SEND_FILE) && commands != null)
                                    {
                                        for (int i = 0; i < IpList.Length; i++)
                                        {
                                            foreach (string str in commands)
                                            {
                                                MessageType.AddMessageQueue(ref this.MessageQueue, str, style, Program.IP, IpList[i]);
                                            }
                                        }
                                    }
                                    else if (style != MessageType.OUTPUT_COMMAND && commands != null)
                                    {
                                        for (int i = 0; i < IpList.Length; i++)
                                        {
                                            foreach (string str in commands)
                                            {
                                                if (Program.ListNetIP != null && Program.ListNetIP[IpList[i]] != null)
                                                {
                                                    MessageType.AddMessageQueue(ref this.MessageQueue, str, style, IpList[i], Program.NetIP);
                                                }
                                                else
                                                {
                                                    MessageType.AddMessageQueue(ref this.MessageQueue, str, style, IpList[i], Program.IP);
                                                }
                                            }
                                        }
                                    }
                                    else if (style == MessageType.OUTPUT_COMMAND)
                                    {
                                        for (int i = 0; i < IpList.Length; i++)
                                        {
                                            foreach (string str in commands)
                                            {
                                                MessageType.AddMessageQueue(ref this.MessageQueue, str, style, Program.IP, IpList[i]);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //MessageType.AddMessageQueue(ref this.MessageQueue, "COMMAND_ERROR: 无此命令类型或命令格式错误！", MessageType.OUTPUT_COMMAND, Program.IP);
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "COMMAND_ERROR: 无此命令类型或命令格式错误！", null);

                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:linux_shell{@shell1@}{@shell2@}…//保持连接执行
                                case MessageType.EXEC_SHELL:
                                    #region
                                    if (this.LinuxServer != null)
                                    {
                                        ShellPkg sp = new ShellPkg();
                                        sp.IP = mp.ReturnIP;
                                        sp.Command = mp.CommandContent;
                                        sp.DTime = DateTime.Now.ToLongTimeString();
                                        sp.MessageType = MessageType.EXEC_SHELL;
                                        lock (this.LinuxServer._datagramQueueRequest)
                                        {
                                            this.LinuxServer._datagramQueueRequest.Enqueue(sp);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:win_cmd{@dos1@}{@dos2@}…//每条初始化执行
                                case MessageType.EXEC_CMD:
                                    #region
                                    if (CMD == null)
                                    {
                                        CMD = new WinCMD();
                                    }
                                    CMD.Command = mp.CommandContent;
                                    CMD.Second = 0;
                                    Thread _tcmd = new Thread(CMD.Execute);
                                    _tcmd.Start();
                                    Thread.Sleep(500);
                                    for (int l = 0; l < 10; l++)
                                    {
                                        if (!CMD.Return.Equals(""))
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_CMD:  <" + mp.ExecIP + "> Output.\n" + CMD.Return, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            break;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                    #endregion
                                case MessageType.RUN_CMD:
                                    #region
                                    if (CMD == null)
                                    {
                                        CMD = new WinCMD();
                                    }
                                    string _out = CMD.RunCmd(mp.CommandContent);
                                    MessageType.AddMessageQueue(ref MessageQueue, "EXEC_CMD:  <" + mp.ExecIP + "> Output.\n" + _out, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:open_file{@path1@}{@path2@}…
                                case MessageType.OPEN_FILE:
                                    #region
                                    if (File.Exists(mp.CommandContent) || Directory.Exists(mp.CommandContent))
                                    {
                                            Process.Start(mp.CommandContent);
                                            MessageType.AddMessageQueue(ref MessageQueue, "OPEN_FILE:  <" + mp.ExecIP + ">" + mp.CommandContent + " 已打开。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);                                        
                                    }
                                    else
                                    {
                                        try
                                        {
                                            WinCMD _wc = new WinCMD();
                                            _wc.cmd_str = mp.CommandContent;
                                            Thread _trun = new Thread(_wc.RunCmd);
                                            _trun.Start();
                                            Thread.Sleep(500);
                                            MessageType.AddMessageQueue(ref MessageQueue, "OPEN_FILE:  <" + mp.ExecIP + ">" + mp.CommandContent + " 已执行。", MessageType.OUTPUT_COMMAND, mp.ReturnIP); 
                                            for (int l = 0; l < 10; l++)
                                            {
                                                if (!_wc.cmd_outstr.Equals(""))
                                                {
                                                    MessageType.AddMessageQueue(ref MessageQueue, "EXEC_CMD:  <" + mp.ExecIP + "> Output.\n" + _wc.cmd_outstr, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                    break;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "OPEN_FILE:  <" + mp.ExecIP + "> 发生异常:"+ex.StackTrace, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:open_web{@url1@}{@url2@}…
                                case MessageType.OPEN_WEB:
                                    #region
                                    this.OpenWeb(mp.CommandContent);
                                    MessageType.AddMessageQueue(ref MessageQueue, "OPEN_WEB:  <" + mp.ExecIP + ">" + mp.CommandContent + " 已打开。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:open_exe{@path1|param2@}{@path1|param2@}…
                                case MessageType.OPEN_EXE:
                                    #region
                                    wc = new WinCMD();
                                    param = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (param.Length > 0)
                                    {
                                        try
                                        {
                                            wc.ExePath = param[0];
                                            if (param.Length > 1)
                                            {
                                                wc.Argument = param[1];
                                                Thread tempT = new Thread(wc.StartProgram);
                                                tempT.Start();
                                            }
                                            else
                                            {
                                                param = param[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (param.Length > 1)
                                                {
                                                    try
                                                    {
                                                        Process.Start(param[0], param[1]);
                                                    }
                                                    catch
                                                    { }
                                                }
                                                else
                                                {
                                                    Thread tempT = new Thread(wc.StartProgram);
                                                    tempT.Start();
                                                }
                                            }
                                            MessageType.AddMessageQueue(ref MessageQueue, "OPEN_EXE:  <" + mp.ExecIP + "> " + param[0] + "已打开。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        catch
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "OPEN_EXE:  <" + mp.ExecIP + "> 路径错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:kill_proc{@proc1@}{@proc2@}…
                                case MessageType.KILL_PROC:
                                    #region
                                    //this.OpenWeb(mp.CommandContent);
                                    ApiMethod.KillProc(mp.CommandContent);
                                    MessageType.AddMessageQueue(ref MessageQueue, "KILL_PROC:  <" + mp.ExecIP + ">" + mp.CommandContent + " 已全部停止。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:win_api{@proc1|send_text_xy|x|y|string@}{@proc2|send_text|id|string@}
                                //{@proc3|click|id@}{@proc4|click_xy|x|y@}{@proc5|send_key|key@}{@proc6|get_byid|id@}
                                //{@proc7|set_max@}{@proc8|set_min@}{@proc9|set_normal@}{@proc9|set_front@}{@proc10|get_text|id@}…
                                case MessageType.EXEC_WIN_API:
                                    #region
                                    string[] pars = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (pars.Length > 2)
                                    {
                                        IntPtr main_hand = IntPtr.Zero;
                                        try
                                        {
                                            main_hand = ApiMethod.GetProcessMainFormHandle(pars[0]);
                                        }
                                        catch { }
                                        if (main_hand != IntPtr.Zero)
                                        {
                                            switch (pars[1].ToLower())
                                            {
                                                case "send_text_xy":
                                                    if (pars.Length > 4)
                                                    {
                                                        int x = 0, y = 0;
                                                        try
                                                        {
                                                            x = Convert.ToInt32(pars[2]);
                                                            y = Convert.ToInt32(pars[3]);
                                                            ApiMethod.SendStringXY(main_hand, pars[4], x, y);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送文本[" + pars[4] + "]到[" + pars[0] + "](" + x + "," + y + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送文本[" + pars[4] + "]到[" + pars[0] + "](" + pars[2] + "," + pars[3] + ")失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "send_text":
                                                    if (pars.Length > 3)
                                                    {
                                                        int id = 0;
                                                        try
                                                        {
                                                            id = Convert.ToInt32(pars[2]);
                                                            IntPtr sub_hand = ApiMethod.GetChildByID(main_hand, id);
                                                            ApiMethod.SendString(sub_hand, pars[3]);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送文本[" + pars[3] + "]到[" + pars[0] + "](ID=" + id + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            ApiMethod.SendString(main_hand, pars[3]);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送文本[" + pars[3] + "]到[" + pars[0] + "](ID=" + id + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "click_xy":
                                                    if (pars.Length > 3)
                                                    {
                                                        int x = 0, y = 0;
                                                        try
                                                        {
                                                            x = Convert.ToInt32(pars[2]);
                                                            y = Convert.ToInt32(pars[3]);
                                                            ApiMethod.ClickPosXY(main_hand, x, y);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 点击[" + pars[0] + "](" + x + "," + y + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 点击[" + pars[0] + "](" + pars[2] + "," + pars[3] + ")失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "click":
                                                    if (pars.Length > 2)
                                                    {
                                                        int id = 0;
                                                        try
                                                        {
                                                            id = Convert.ToInt32(pars[2]);
                                                            IntPtr sub_hand = ApiMethod.GetChildByID(main_hand, id);
                                                            ApiMethod.SendMessage(sub_hand, ApiCode.WM_CLICK, main_hand, "0");
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 点击[" + pars[0] + "](ID=" + id + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 点击[" + pars[0] + "](ID=" + pars[2] + ")失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "get_text":
                                                    if (pars.Length > 2)
                                                    {
                                                        int id = 0;
                                                        try
                                                        {
                                                            id = Convert.ToInt32(pars[2]);
                                                            IntPtr sub_hand = ApiMethod.GetChildByID(main_hand, id);
                                                            string text = ApiMethod.GetStringText(sub_hand);
                                                            //ApiMethod.SendMessage(sub_hand, ApiCode.WM_CLICK, main_hand, "0");
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 得到[" + pars[0] + "](ID=" + id + ")文本为" + text, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 得到[" + pars[0] + "](ID=" + pars[2] + ")文本失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "send_key":
                                                    if (pars.Length > 2)
                                                    {
                                                        try
                                                        {
                                                            SendKeys.SendWait(pars[2]);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送按键" + pars[2] + "到[" + pars[0] + "]。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 发送按键" + pars[2] + "到[" + pars[0] + "]失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "get_byid":
                                                    if (pars.Length > 2)
                                                    {
                                                        int id = 0;
                                                        try
                                                        {
                                                            id = Convert.ToInt32(pars[2]);
                                                            IntPtr sub_hand = ApiMethod.GetChildByID(main_hand, id);
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 得到句柄" + sub_hand + "来自[" + pars[0] + "]（ID=" + id + ")。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                        catch
                                                        {
                                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 获得句柄来自[" + pars[0] + "]（ID=" + pars[2] + ")失败。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                        }
                                                    }
                                                    break;
                                                case "set_max":
                                                    ApiMethod.ShowWindow(main_hand, ApiCode.SW_MAXIMIZE);
                                                    break;
                                                case "set_min":
                                                    ApiMethod.ShowWindow(main_hand, ApiCode.SW_MINIMIZE);
                                                    break;
                                                case "set_normal":
                                                    ApiMethod.ShowWindow(main_hand, ApiCode.SW_SHOWNORMAL);
                                                    break;
                                                case "set_front":
                                                    ApiMethod.SetForegroundWindow(main_hand);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "EXEC_WIN_API:  <" + mp.ExecIP + "> 进程[" + pars[0] + "]不存在。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:open_lua{@path1@}{@path2@}…
                                case MessageType.OPEN_SCRIPT:
                                    #region
                                    wc = new WinCMD();
                                    param = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (param.Length > 0 && File.Exists(param[0]))
                                    {
                                        wc.ExePath = Program.startPath + @"\Plugs\应用脚本编辑器.exe";
                                        wc.Argument = param[0];
                                        Thread tempT = new Thread(wc.StartProgram);
                                        tempT.Start();
                                        MessageType.AddMessageQueue(ref MessageQueue, "OPEN_SCRIPT:  <" + mp.ExecIP + "> " + param[0] + "已打开。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "OPEN_SCRIPT:  <" + mp.ExecIP + "> 路径错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:exec_lua{@path1@}{@path2@}…
                                case MessageType.EXEC_SCRIPT:
                                    #region
                                    if (File.Exists(mp.CommandContent))
                                    {
                                        this.scriptPath = mp.CommandContent;
                                        if (this.LuaPlus != null)
                                        {
                                            this.LuaPlus.pLuaVM.Close();
                                            this.LuaPlus.pLuaVM = null;
                                            this.LuaPlus = null;
                                        }
                                        if (this.LuaExecThread != null)
                                        {
                                            this.LuaExecThread.Abort();
                                            this.LuaExecThread = null;
                                        }
                                        this.LuaExecThread = new Thread(this.LuaExec);
                                        this.LuaExecThread.Start();
                                    }
                                    else if (File.Exists(Path.Combine(Program.startPath, mp.CommandContent)))
                                    {
                                        this.scriptPath = Path.Combine(Program.startPath, mp.CommandContent);
                                        if (this.LuaPlus != null)
                                        {
                                            this.LuaPlus.pLuaVM.Close();
                                            this.LuaPlus.pLuaVM = null;
                                            this.LuaPlus = null;
                                        }
                                        if (this.LuaExecThread != null)
                                        {
                                            this.LuaExecThread.Abort();
                                            this.LuaExecThread = null;
                                        }
                                        this.LuaExecThread = new Thread(this.LuaExec);
                                        this.LuaExecThread.Start();
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 脚本文件不存在。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:stop_lua{@@}
                                case MessageType.STOP_SCRIPT:
                                    #region
                                    if (this.LuaPlus != null)
                                    {
                                        this.LuaPlus.pLuaVM.Close();
                                        this.LuaPlus.pLuaVM = null;
                                        this.LuaPlus = null;
                                    }
                                    if (this.LuaExecThread != null)
                                    {
                                        this.LuaExecThread.Abort();
                                        this.LuaExecThread = null;
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_connect_servers{@allx@}{@server@}{@client@}{@linux@}…
                                case MessageType.GET_CONNECT_SERVERS:
                                    #region
                                    string servers = "在线列表：";
                                    //    new string[this.WinClient.ClientSessionTable.Count + this.WinServer.ClientSessionTable.Count];
                                    switch (mp.CommandContent)
                                    {
                                        case "client":
                                            if (this.WinServer != null && this.WinServer.ClientSessionTable != null)
                                            {
                                                foreach (object obj in this.WinServer.ClientSessionTable.Keys)
                                                {
                                                    servers += "\n" + ((TSession)this.WinServer.ClientSessionTable[obj]).ClientSocket.RemoteEndPoint;
                                                }
                                            }
                                            break;
                                        case "server":
                                            if (this.WinClient != null && this.WinClient.ClientSessionTable != null)
                                            {
                                                foreach (object obj in this.WinClient.ClientSessionTable.Keys)
                                                {
                                                    servers += "\n" + ((TSession)this.WinClient.ClientSessionTable[obj]).ClientSocket.RemoteEndPoint;
                                                }
                                            }
                                            break;
                                        case "linux":
                                            if (this.LinuxServer != null && this.LinuxServer.SessionTable != null)
                                            {
                                                foreach (object obj in this.LinuxServer.SessionTable.Keys)
                                                {
                                                    servers += "\n" + ((SshReader)this.LinuxServer.SessionTable[obj]).IP + ":22";
                                                }
                                            }
                                            break;
                                        default:
                                            if (this.WinServer != null && this.WinServer.ClientSessionTable != null)
                                            {
                                                foreach (object obj in this.WinServer.ClientSessionTable.Keys)
                                                {
                                                    servers += "\n" + ((TSession)this.WinServer.ClientSessionTable[obj]).ClientSocket.RemoteEndPoint;
                                                }
                                            }
                                            if (this.WinClient != null && this.WinClient.ClientSessionTable != null)
                                            {
                                                foreach (object obj in this.WinClient.ClientSessionTable.Keys)
                                                {
                                                    servers += "\n" + ((TSession)this.WinClient.ClientSessionTable[obj]).ClientSocket.RemoteEndPoint;
                                                }
                                            }
                                            if (this.LinuxServer != null && this.LinuxServer.SessionTable != null)
                                            {
                                                foreach (object obj in this.LinuxServer.SessionTable.Keys)
                                                {
                                                    servers += "\n" + ((SshReader)this.LinuxServer.SessionTable[obj]).IP + ":22";
                                                }
                                            }
                                            break;
                                    }
                                    if (servers != "在线列表")
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, servers, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, servers + "为空！", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_noconnect_servers{@@}
                                case MessageType.GET_NOCONNECT_SERVERS:
                                    #region
                                    string n_servers = "离线列表";
                                    if (this.WinClient != null)
                                    {
                                        foreach (object obj in this.WinClient._hostTable.Keys)
                                        {
                                            if (this.WinClient.ClientSessionTable[obj] == null)
                                            {
                                                n_servers += "\n" + obj;
                                            }
                                        }
                                    }
                                    if (this.LinuxServer != null)
                                    {
                                        foreach (object obj in this.LinuxServer.ServerTable.Keys)
                                        {
                                            if (this.LinuxServer.SessionTable[obj] == null)
                                            {
                                                n_servers += "\n" + obj;
                                            }
                                        }
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, n_servers, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_all_servers{@@}
                                case MessageType.GET_ALL_SERVERS:
                                    #region
                                    string a_servers = "所有服务器";
                                    if (this.WinClient != null)
                                    {
                                        foreach (object obj in this.WinClient._hostTable.Keys)
                                        {
                                            a_servers += "\n" + obj;
                                        }
                                    }
                                    if (this.LinuxServer != null)
                                    {
                                        foreach (object obj in this.LinuxServer.ServerTable.Keys)
                                        {

                                            a_servers += "\n" + obj;
                                        }
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, a_servers, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:connect_all_servers{@@}
                                case MessageType.START_ALL_SERVER:
                                    #region
                                    if (this.WinClient != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.WinClient.ConnectAllServer);
                                    }
                                    if (this.LinuxServer != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.LinuxServer.ConnectAllServer);
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, "连接所有服务器！", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:connect_one_server{@IP_1@}{@IP_2@}…
                                case MessageType.START_ONE_SERVER:
                                    #region
                                    if (this.WinClient != null && this.WinClient._hostTable[mp.CommandContent] != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.StartOneSocket, mp.CommandContent);
                                    }
                                    if (this.LinuxServer != null && this.LinuxServer.ServerTable[mp.CommandContent] != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.StartOneSsh, mp.CommandContent);
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, "连接服务器<" + mp.CommandContent + ">", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:close_all_servers{@@}
                                case MessageType.CLOSE_ALL_SERVER:
                                    #region
                                    if (this.WinClient != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.CloseAllSocket);
                                    }
                                    if (this.LinuxServer != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.CloseAllSsh);
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, "断开所有服务器！", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:close_one_server{@IP_1@}{@IP_2@}…
                                case MessageType.CLOSE_ONE_SERVER:
                                    #region
                                    if (this.WinClient != null && this.WinClient._hostTable[mp.CommandContent] != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.CloseOneSocket, mp.CommandContent);
                                    }
                                    if (this.LinuxServer != null && this.LinuxServer.SessionTable[mp.CommandContent] != null)
                                    {
                                        ThreadPool.QueueUserWorkItem(this.CloseOneSsh, mp.CommandContent);
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, "断开服务器<" + mp.CommandContent + ">", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_status_server{@IP_1@}{@IP_2@}…
                                case MessageType.GET_STATUS_SERVER:
                                    #region
                                    bool s_flag = false;
                                    if (this.WinClient != null && this.WinClient._hostTable[mp.CommandContent] != null)
                                    {
                                        s_flag = this.GetStatusSocket(mp.CommandContent);
                                    }
                                    if (this.LinuxServer != null && this.LinuxServer.SessionTable[mp.CommandContent] != null)
                                    {
                                        s_flag = this.GetStatusSsh(mp.CommandContent);
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + ">: " + s_flag, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:delete_one_server{@IP_1@}{@IP_2@}…
                                case MessageType.DELETE_ONE_SERVER:
                                    #region
                                    if (this.DeleteOneServer(mp.CommandContent))
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 配置已删除，重启生效。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 配置不存在或其他错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2:con_add[[min|hour|day|month|week|command]]
                                //127.0.0.1:cron_add[[*|*|*|*|*|get_all_servers{@@}]]
                                case MessageType.CRON_ADD:
                                    #region
                                    string[] _cas = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_cas.Length >= 6)
                                    {
                                        if (this.AddCron(mp.CommandContent))
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 定时任务已添加。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 配置不存在或其他错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 定时任务格式错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2:con_add[[min|hour|day|month|week|command]]
                                //127.0.0.1:cron_del[[*|*|*|*|*|get_all_servers{@@}]]
                                case MessageType.CRON_DEL:
                                    #region
                                    string[] _cds = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_cds.Length >= 6)
                                    {
                                        if (this.DeleteCron(mp.CommandContent))
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 定时任务已删除。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 配置不存在或其他错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 定时任务格式错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2:send_mail{@to1;to2;to3|cc1;cc2|subject|body|true@}
                                //127.0.0.1:send_mail{@workhard_smile@163.com;wugang@nhn.com|wugang@nhn.com|测试|<html>测试<br />test</html>|true@}
                                case MessageType.SEND_MAIL:
                                    #region
                                    string[] _sms = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_sms.Length >= 4)
                                    {
                                        MailHelper mh = new MailHelper();
                                        if (_sms.Length == 4)
                                        {
                                            if (mh.SendEmail(_sms[0], _sms[1], _sms[2], _sms[3]))
                                            {
                                                MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 全部邮件已发送。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            }
                                            else
                                            {
                                                MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 部分邮件发送失败。详见:" + mh.lm.LogPath, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            }
                                        }
                                        else if (_sms.Length == 5)
                                        {
                                            if (_sms[4].ToLower().Equals("true"))
                                            {
                                                if (mh.SendEmail(_sms[0], _sms[1], _sms[2], _sms[3], true))
                                                {
                                                    MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 全部邮件已发送。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                }
                                                else
                                                {
                                                    MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 部分邮件发送失败。详见:" + mh.lm.LogPath, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                }
                                            }
                                            else
                                            {
                                                if (mh.SendEmail(_sms[0], _sms[1], _sms[2], _sms[3], false))
                                                {
                                                    MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 全部邮件已发送。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                }
                                                else
                                                {
                                                    MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 部分邮件发送失败。详见:" + mh.lm.LogPath, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 发送邮件命令格式错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //127.0.0.1:view_file{@crontab.lst@}
                                case MessageType.VIEW_FILE:
                                    #region
                                    string _c = "";
                                    if (File.Exists(mp.CommandContent))
                                    {
                                        FileInfo _fi = new FileInfo(mp.CommandContent);
                                        if (_fi.Length > 102400)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 文件大小超过最大限制102400 bytes。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            _c = File.ReadAllText(mp.CommandContent, Encoding.Default);
                                            MessageType.AddMessageQueue(ref MessageQueue, _c, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }

                                    }
                                    else if (File.Exists(Path.Combine(Program.startPath, mp.CommandContent)))
                                    {
                                        _c = Path.Combine(Program.startPath, mp.CommandContent);
                                        FileInfo _fi = new FileInfo(_c);
                                        if (_fi.Length > 102400)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 文件大小超过最大限制102400 bytes。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            _c = File.ReadAllText(_c, Encoding.Default);
                                            MessageType.AddMessageQueue(ref MessageQueue, _c, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.CommandContent + "> 文件不存在。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:check_windows_file{@path1|minSize1|timeout1@}{@path2|minSize2|timeout2@}…
                                case MessageType.CHECK_FILE:
                                    #region
                                    _wcheck = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_wcheck.Length > 2)
                                    {
                                        long size = 0;
                                        int time = 0;
                                        try
                                        {
                                            size = Convert.ToInt64(_wcheck[1]);
                                            time = Convert.ToInt32(_wcheck[2]);
                                        }
                                        catch { }
                                        size = this.CheckFile(_wcheck[0], size, time);
                                        if (size > 0)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.ExecIP + ">文件[" + _wcheck[0] + "]大小为" + size, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.ExecIP + ">文件[" + _wcheck[0] + "]文件不存在或大小为" + size, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:check_windows_directory{@path1@}{@path2@}…
                                case MessageType.CHECK_DIRECTORY:
                                    #region
                                    if (this.CheckDirectory(mp.CommandContent))
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.ExecIP + ">文件[" + mp.CommandContent + "]存在。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "<" + mp.ExecIP + ">文件[" + mp.CommandContent + "]不存在。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:check_linux_file{@path1|minSize1|timeout1@}{@path2|minSize2|timeout2@}…
                                case MessageType.L_CHECK_FILE:
                                    #region
                                    _lcheck = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_lcheck.Length > 2)
                                    {
                                        long size = 0;
                                        int time = 0;
                                        try
                                        {
                                            size = Convert.ToInt64(_lcheck[1]);
                                            time = Convert.ToInt32(_lcheck[2]);
                                        }
                                        catch { }
                                        size = this.LinuxServer.LCheckFile(mp.ReturnIP, _lcheck[0], size, time);
                                        if (size > 0)
                                        {
                                            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "<" + mp.ReturnIP + ">文件" + _lcheck[0] + "大小为" + size, null);
                                        }
                                        else
                                        {
                                            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "<" + mp.ReturnIP + ">文件" + _lcheck[0] + "不存在或大小为0", null);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:check_linux_directory{@path1@}{@path2@}…
                                case MessageType.L_CHECK_DIRECTORY:
                                    #region
                                    if (this.LinuxServer.LCheckDirectory(mp.ReturnIP, mp.CommandContent))
                                    {
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "<" + mp.ReturnIP + ">文件夹[" + mp.CommandContent + "]存在。", null);
                                    }
                                    else
                                    {
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "<" + mp.ReturnIP + ">文件夹[" + mp.CommandContent + "]不存在。", null);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_windows_directories{@path1|n@}{@path2|n@}…
                                case MessageType.GET_DIRS:
                                    #region
                                    command = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (command.Length >= 2)
                                    {
                                        fi = new SysIO();
                                        int count = 2;
                                        try
                                        {
                                            count = Convert.ToInt16(command[0]);
                                        }
                                        catch
                                        { }
                                        //if (count != 2 || fi.dirs.Count <= 0)
                                        //{
                                        fi.GetDirs(command[1], count);
                                        if (fi.dirs.Count > 0)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_DIRS: 获取服务器<" + mp.ExecIP + ">目录列表", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            foreach (string dir in fi.dirs)
                                            {
                                                temp += dir + "|";
                                            }
                                            MessageType.AddMessageQueue(ref MessageQueue, temp, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_DIRS: 获取服务器<" + mp.ExecIP + ">目录列表失败", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:get_windows_files{@path1|n@}{@path2|n@}…
                                case MessageType.GET_FILES:
                                    #region
                                    command = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (command.Length >= 2)
                                    {
                                        fi = new SysIO();
                                        int count = 2;
                                        try
                                        {
                                            count = Convert.ToInt16(command[0]);
                                        }
                                        catch
                                        { }
                                        //if (count != 2 || fi.files.Count <= 0)
                                        //{
                                        fi.GetFiles(command[1], count);
                                        if (fi.files.Count > 0)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_FILES: 获取服务器<" + mp.ExecIP + ">文件列表", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            foreach (string file in fi.files)
                                            {
                                                temp += file + "|";
                                            }
                                            MessageType.AddMessageQueue(ref MessageQueue, temp, MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_FILES: 获取服务器<" + mp.ExecIP + ">文件列表失败", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:send_windows_file{@filePath1>IP1?IP1_dir1?IP1_dir2?…|IP2?IP2_dir1?IP2_dir2?…|…@}{@filePath2>IP1?IP1_dir1?IP1_dir2?…|IP2?IP2_dir1?IP2_dir2?…|…@}…
                                case MessageType.SEND_FILE:
                                    #region
                                    if (ComputerInfo.IsBusyPort(Program.SenderPort))
                                    {
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "SenderPort端口<" + Program.SenderPort + ">被占用!", null);
                                        MessageType.AddMessageQueue(ref MessageQueue, "SenderPort端口<" + Program.SenderPort + ">被占用!", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        break;
                                    }
                                    if (this.SendThread != null && this.SendThread.IsAlive)
                                    {
                                        this.SendThread.Abort();
                                        this.SendThread = null;
                                        Thread.Sleep(10);
                                    }
                                    //this.CloseSendFile();
                                    this.SendContext = mp.CommandContent;
                                    this.SendThread = new Thread(this.SendFiles);
                                    this.SendThread.IsBackground = true;
                                    this.SendThread.Start();
                                    //ThreadPool.QueueUserWorkItem(this.serverForm.SendFile, mp.CommandContent);
                                    //this.serverForm.SendFile(mp.CommandContent);
                                    break;
                                    #endregion
                                //IP1,IP2,…:download_windows_file{@IP_1|port1|count|IP1_dir1?IP1_dir2?…@}{@IP_2|port2|count|IP1_dir1?IP1_dir2?…@}…
                                case MessageType.DOWNLOAD_FILE:
                                    #region
                                    if (this.DownloadThread != null && this.DownloadThread.IsAlive)
                                    {
                                        this.DownloadThread.Abort();
                                        this.SendThread = null;
                                        Thread.Sleep(10);
                                    }
                                    //this.CloseDownloadFile();
                                    this.DownloadContext = mp.CommandContent;
                                    this.DownloadThread = new Thread(this.DownloadFiles);
                                    this.DownloadThread.IsBackground = true;
                                    this.DownloadThread.Start();
                                    //ThreadPool.QueueUserWorkItem(this.serverForm.DownloadFile, mp.CommandContent);
                                    //this.serverForm.DownloadFile(mp.CommandContent);
                                    break;
                                    #endregion
                                //IP1,IP2,…:download_windows_file{@IP_1|port1|count|IP1_dir1?IP1_dir2?…@}{@IP_2|port2|count|IP1_dir1?IP1_dir2?…@}…
                                case MessageType.DOWNLOAD_FILES:
                                    #region
                                    string[] downs = mp.CommandContent.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (Program.ListNetIP != null && Program.ListNetIP[downs[0]] != null)
                                    {
                                        MessageType.AddMessageQueue(ref this.MessageQueue, downs[1] + ">" + Program.NetIP + "?" + downs[2], MessageType.SEND_FILE, downs[0], Program.NetIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref this.MessageQueue, downs[1] + ">" + Program.IP + "?" + downs[2], MessageType.SEND_FILE, downs[0], Program.IP);
                                    }
                                    //ThreadPool.QueueUserWorkItem(this.serverForm.DownloadFile, mp.CommandContent);
                                    //this.serverForm.DownloadFile(mp.CommandContent);
                                    break;
                                    #endregion
                                //linux界面发送文件
                                case MessageType.L_SEND_FILE:
                                    #region
                                    string[] lip = mp.CommandContent.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (lip.Length > 1)
                                    {
                                        if (File.Exists(lip[0]))
                                        {
                                            string[] _servers = lip[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (_servers.Length > 0)
                                            {
                                                foreach (string str in _servers)
                                                {
                                                    string[] sys = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                    if (sys.Length > 1)
                                                    {
                                                        string[] etcinfo = sys[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                        string[] dirs = sys[1].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                                        this.LinuxUploadFile(etcinfo, lip[0], dirs);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "LINUX_UPLOAD: 发送文件不存在!", null);
                                            }
                                        }
                                    }
                                    break;
                                    #endregion
                                //linux界面下载文件
                                case MessageType.L_DOWNLOAD_FILE:
                                    #region
                                    string[] _down = mp.CommandContent.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_down.Length >= 3)
                                    {
                                        string[] etcinfo = _down[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                        string[] dirs = _down[2].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                        this.LinuxDownloadFile(etcinfo, _down[1].Trim(), dirs);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:send_linux_file{@filePath1>IP1;IP1_dir1?IP1_dir2?…|IP2;IP2_dir1?IP2_dir2?…|…@}{@filePath2>IP1;IP1_dir1?IP1_dir2?…|IP2;IP2_dir1?IP2_dir2?…|…@}…
                                case MessageType.L_SEND_FILES:
                                    #region
                                    string[] lips = mp.CommandContent.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (lips.Length > 1)
                                    {
                                        if (File.Exists(lips[0].Trim()))
                                        {
                                            string[] _servers = lips[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (_servers.Length > 0)
                                            {
                                                foreach (string str in _servers)
                                                {
                                                    string[] sys = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                    if (sys.Length > 1)
                                                    {
                                                        string[] etcinfo = new string[4];
                                                        ServerInfo si = (ServerInfo)this.LinuxServer.ServerTable[sys[0]];
                                                        if (si != null)
                                                        {
                                                            etcinfo[0] = si.IP;
                                                            etcinfo[1] = si.SshPort.ToString();
                                                            etcinfo[2] = si.User;
                                                            etcinfo[3] = si.Password;
                                                            string[] dirs = sys[1].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                                            this.LinuxUploadFile(etcinfo, lips[0].Trim(), dirs);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "LINUX_UPLOAD: 发送文件不存在!", null);
                                            }
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:download_linux_file{@IP_1|file1|IP1_dir1?IP1_dir2?…@}{@IP_2|file2|IP1_dir1?IP1_dir2?…@}…
                                case MessageType.L_DOWNLOAD_FILES:
                                    #region
                                    string[] _downs = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (_downs.Length >= 3)
                                    {
                                        string[] etcinfo = new string[4];
                                        ServerInfo si = (ServerInfo)this.LinuxServer.ServerTable[_downs[0]];
                                        if (si != null)
                                        {
                                            etcinfo[0] = si.IP;
                                            etcinfo[1] = si.SshPort.ToString();
                                            etcinfo[2] = si.User;
                                            etcinfo[3] = si.Password;
                                            string[] dirs = _downs[2].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                            this.LinuxDownloadFile(etcinfo, _downs[1].Trim(), dirs);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:send_net_file{@filePath1>IP1?IP1_dir1?IP1_dir2?…|IP2?IP2_dir1?IP2_dir2?…|…@}{@filePath2>IP1?IP1_dir1?IP1_dir2?…|IP2?IP2_dir1?IP2_dir2?…|…@}…
                                case MessageType.N_SEND_FILE:
                                    #region
                                    int _port = Program.SenderPort + 1;
                                    if (ComputerInfo.IsBusyPort(_port))
                                    {
                                        this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "SenderPort端口<" + _port + ">被占用!", null);

                                        MessageType.AddMessageQueue(ref MessageQueue, "SenderPort端口<" + _port + ">被占用!", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        break;
                                    }
                                    string[] ips = mp.CommandContent.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (ips.Length > 1)
                                    {
                                        string[] dirs = ips[1].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (this.ThreadFRP != null && this.ThreadFRP.IsAlive)
                                        {
                                            this.ThreadFRP.Abort();
                                        }
                                        if (this.FRP != null)
                                        {
                                            this.FRP.IsStop = true;
                                            this.FileSender = null;
                                            Thread.Sleep(10);
                                        }
                                        //this.CloseSendFile();
                                        this.FRP = new FileReceiveP2P(_port, dirs);
                                        //frp.StartReceive();
                                        this.ThreadFRP = new Thread(FRP.StartReceive);
                                        this.ThreadFRP.IsBackground = true;
                                        this.ThreadFRP.Start();

                                        MessageType.AddMessageQueue(ref this.MessageQueue, Program.NetIP + "|" + _port + "|" + dirs.Length + "|" + ips[0], MessageType.N_DOWNLOAD_FILE, mp.ReturnIP, Program.NetIP);
                                        //MessageType.AddMessageQueue(ref this.MessageQueue, Program.IP + "|" + Program.SenderPort + "|" + dirs.Length + "|" + ips[0], MessageType.N_DOWNLOAD_FILE, mp.ReturnIP, Program.NetIP);
                                    }
                                    break;
                                    #endregion
                                case MessageType.N_DOWNLOAD_FILE:
                                    #region
                                    string[] ps = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (ps.Length >= 4)
                                    {
                                        int port = 18889, dirCount = 1;
                                        try
                                        {
                                            port = Convert.ToInt32(ps[1]);
                                            dirCount = Convert.ToInt32(ps[2]);
                                        }
                                        catch (Exception ex)
                                        {
                                            //MessageType.AddMessageQueue(ref MessageQueue, "配置参数异常!\r\n" + ex.Message, MessageType.OUTPUT_MESSAGE, Program.IP);
                                            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "配置参数异常!\r\n" + ex.Message, null);
                                            MessageType.MessageBoxShow("配置参数异常!\r\n" + ex.Message, "info");
                                            break;
                                        }
                                        if (File.Exists(ps[3]))
                                        {
                                            if (this.ThreadFSP != null && this.ThreadFSP.IsAlive)
                                            {
                                                this.ThreadFSP.Abort();
                                            }
                                            if (FSP != null)
                                            {
                                                FSP = null;
                                                Thread.Sleep(10);
                                            }
                                            //this.CloseDownloadFile();
                                            FSP = new FileSendP2P(ps[0], port, ps[3], 50000, dirCount);
                                            //fsp.StartSend();
                                            ThreadFSP = new Thread(FSP.StartSend);
                                            ThreadFSP.IsBackground = true;
                                            ThreadFSP.Start();
                                        }
                                        else
                                        {
                                            this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "发送文件不存在!", null);
                                            MessageType.MessageBoxShow("发送文件不存在!", "info");
                                            //MessageType.AddMessageQueue(ref MessageQueue, "发送文件不存在!", MessageType.OUTPUT_MESSAGE, Program.IP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:close_send_file{@@}
                                case MessageType.CLOSE_SEND_FILE:
                                    #region
                                    this.CloseSendFile();
                                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "关闭发送进程！", null);
                                    //MessageType.AddMessageQueue(ref MessageQueue, "关闭发送进程！", MessageType.OUTPUT_COMMAND, Program.IP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:close_download_file{@@}
                                case MessageType.CLOSE_DOWNLOAD_FILE:
                                    #region
                                    this.CloseDownloadFile();
                                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "关闭接收进程!", null);
                                    //MessageType.AddMessageQueue(ref MessageQueue, "关闭接收进程!", MessageType.OUTPUT_COMMAND, Program.IP);
                                    break;
                                    #endregion
                                //IP1,IP2,…:replace_right{@file1|left1|right1@}{@file1|left1|right1@}…
                                case MessageType.REPLACE_RIGHT:
                                    #region
                                    param = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (param.Length > 2 && File.Exists(param[0]))
                                    {
                                        this.ReplaceRight(param[0], param[1], param[2]);
                                        MessageType.AddMessageQueue(ref MessageQueue, "REPLACE_RIGHT:  <" + mp.ExecIP + "> " + param[0] + "中字串" + param[1] + "右边替换为" + param[2], MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "REPLACE_RIGHT:  <" + mp.ExecIP + "> 路径错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //IP1,IP2,…:replace_text{@file1|old1|new1@}{@file2|old2|new2@}…
                                case MessageType.REPLACE_TEXT:
                                    #region
                                    param = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (param.Length > 0 && File.Exists(param[0]))
                                    {
                                        this.LineReplace(param[0], param[1], param[2]);
                                        MessageType.AddMessageQueue(ref MessageQueue, "REPLACE_TEXT:  <" + mp.ExecIP + "> " + param[0] + "中字串" + param[1] + "替换为" + param[2], MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    else
                                    {
                                        MessageType.AddMessageQueue(ref MessageQueue, "REPLACE_TEXT:  <" + mp.ExecIP + "> 路径错误。", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                    }
                                    break;
                                    #endregion
                                //公网IP
                                case MessageType.PUBLIC_IP:
                                    #region
                                    if (Program.IP != mp.CommandContent && mp.CommandContent != "127.0.0.1")
                                    {
                                        if (Program.ListNetIP == null)
                                        {
                                            Program.ListNetIP = new Hashtable();
                                        }
                                        if (Program.ListNetIP[mp.ReturnIP] == null)
                                        {
                                            Program.ListNetIP.Add(mp.ReturnIP, "");
                                        }
                                        if (Program.NetIP != mp.CommandContent)
                                        {
                                            Program.NetIP = mp.CommandContent;
                                            MessageType.AddMessageQueue(ref MessageQueue, mp.CommandContent, MessageType.PUBLIC_IP, mp.CommandContent, Program.NetIP);
                                        }
                                    }
                                    //if (Program.NetIP != null && !Program.NetIP.Equals("") && !Program.NetIP.Equals(Program.IP))
                                    //{
                                    //    Program.IP = Program.NetIP;
                                    //}
                                    break;
                                    #endregion
                                //获取目录列表
                                case MessageType.C_GET_DIRS:
                                    #region
                                    command = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (command.Length >= 2)
                                    {
                                        fi = new SysIO();
                                        int count = 2;
                                        try
                                        {
                                            count = Convert.ToInt16(command[0]);
                                        }
                                        catch
                                        { }
                                        //if (count != 2 || fi.dirs.Count <= 0)
                                        //{
                                        fi.GetDirs(command[1], count);
                                        if (fi.dirs.Count > 0)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_DIRS: 获取服务器<" + mp.ExecIP + ">目录列表", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            foreach (string dir in fi.dirs)
                                            {
                                                temp += dir + "|";
                                            }
                                            MessageType.AddMessageQueue(ref MessageQueue, temp, MessageType.S_GET_DIRS, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_DIRS: 获取服务器<" + mp.ExecIP + ">目录列表失败", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //加载目录列表
                                case MessageType.S_GET_DIRS:
                                    #region
                                    path = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (path.Length > 0)
                                    {
                                        this.Invoke(DelegateLoadComboListF, path, false);
                                    }
                                    break;
                                    #endregion
                                //获取文件列表
                                case MessageType.C_GET_FILES:
                                    #region
                                    command = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (command.Length >= 2)
                                    {
                                        fi = new SysIO();
                                        int count = 2;
                                        try
                                        {
                                            count = Convert.ToInt16(command[0]);
                                        }
                                        catch
                                        { }
                                        //if (count != 2 || fi.files.Count <= 0)
                                        //{
                                        fi.GetFiles(command[1], count);
                                        if (fi.files.Count > 0)
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_FILES: 获取服务器<" + mp.ExecIP + ">文件列表", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                            foreach (string file in fi.files)
                                            {
                                                temp += file + "|";
                                            }
                                            MessageType.AddMessageQueue(ref MessageQueue, temp, MessageType.S_GET_FILES, mp.ReturnIP);
                                        }
                                        else
                                        {
                                            MessageType.AddMessageQueue(ref MessageQueue, "GET_FILES: 获取服务器<" + mp.ExecIP + ">文件列表失败", MessageType.OUTPUT_COMMAND, mp.ReturnIP);
                                        }
                                    }
                                    break;
                                    #endregion
                                //加载文件列表
                                case MessageType.S_GET_FILES:
                                    #region
                                    path = mp.CommandContent.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (path.Length > 0)
                                    {
                                        this.Invoke(DelegateLoadComboListF, path, true);
                                    }
                                    break;
                                    #endregion
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            //序列化反序列化TDatagram发送
                            string temp = "MessagePackage|" + SerializeObj.Serialize<MessagePackage>(mp);
                            TDatagram datagram = new TDatagram(temp);
                            datagram.IP = mp.ExecIP;
                            if (this.WinServer != null && this.WinServer._datagramQueueResponse != null)
                            {
                                lock (this.WinServer._datagramQueueResponse)
                                {
                                    this.WinServer._datagramQueueResponse.Enqueue(datagram);
                                }
                            }
                            if (this.WinClient != null && this.WinClient._datagramQueueResponse != null)
                            {
                                lock (this.WinClient._datagramQueueResponse)
                                {
                                    this.WinClient._datagramQueueResponse.Enqueue(datagram);
                                }
                            }

                        }
                    }
                }
                if (this.FileSender != null && SendSocket.IsFinished)
                {
                    MessageType.AddMessageQueue(ref MessageQueue, "SEND_FILE: Sender Exit.", MessageType.OUTPUT_COMMAND, Program.IP);
                    if (this.FileType == 1)
                    {
                        //MessageType.AddMessageQueue(ref MessageQueue, "私网Windows文件分发完成!\r\n传输至" + this.FileSender.PkgCount + "服务器平均速度：" + this.FileSender.Speed, MessageType.OUTPUT_MESSAGE, Program.IP);
                        MessageType.MessageBoxShow("私网Windows文件分发完成!\r\n传输至" + this.FileSender.PkgCount + "服务器平均速度：" + this.FileSender.Speed, "info");
                        this.FileType = 0;
                    }
                    if (this.ListenThread != null && this.ListenThread.IsAlive)
                    {
                        this.ListenThread.Abort();
                        this.ListenThread = null;
                    }
                    this.FileSender.Close();
                    this.FileSender = null;
                    if (this.SendThread != null && this.SendThread.IsAlive)
                    {
                        this.SendThread.Abort();
                        this.SendThread = null;
                    }
                    //this.CloseSendFile();
                }
                if (this.FSP != null && FileSendP2P.IsFinished)
                {
                    //MessageType.AddMessageQueue(ref MessageQueue, "SEND_FILE: Sender Exit.", MessageType.OUTPUT_COMMAND, Program.IP);
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "SEND_FILE: Sender Exit.", null);
                    if (this.FileType == 1)
                    {
                        //MessageType.AddMessageQueue(ref MessageQueue, "公网Windows文件分发完成!", MessageType.OUTPUT_MESSAGE, Program.IP);
                        MessageType.MessageBoxShow("公网Windows文件分发完成!", "info");
                        this.FileType = 0;
                    }
                    if (this.ThreadFSP != null && this.ThreadFSP.IsAlive)
                    {
                        this.ThreadFSP.Abort();
                    }
                    this.FSP = null;
                    //this.CloseDownloadFile();
                }
                if (this.FileReceiver != null && ReceiveSocket.IsFinished)
                {
                    if (Program.ListNetIP != null && Program.ListNetIP[this.FileReceiver.IP] != null)
                    {
                        MessageType.AddMessageQueue(ref MessageQueue, "SEND_FILE: From <" + Program.NetIP + "> Exit.", MessageType.OUTPUT_COMMAND, this.FileReceiver.IP);
                    }
                    else
                    {
                        MessageType.AddMessageQueue(ref MessageQueue, "SEND_FILE: From <" + Program.IP + "> Exit.", MessageType.OUTPUT_COMMAND, this.FileReceiver.IP);
                    }
                    //MessageType.AddMessageQueue(ref MessageQueue, "DOWNLOAD_FILE: From <" + this.FileReceiver.IP + "> Exit.\r\n" + this.FileReceiver.Result, MessageType.OUTPUT_COMMAND, Program.IP);
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "DOWNLOAD_FILE: From <" + this.FileReceiver.IP + "> Exit.\r\n" + this.FileReceiver.Result, null);

                    if (this.FileType == 2)
                    {
                        //MessageType.AddMessageQueue(ref MessageQueue, "Windows文件下载完成!\r\n" + this.FileReceiver.Result, MessageType.OUTPUT_MESSAGE, Program.IP);
                        MessageType.MessageBoxShow("从Windows<" + this.FileReceiver.IP + ">下载文件完成!\r\n" + this.FileReceiver.Result, "info");
                        this.FileType = 0;
                    }
                    this.FileReceiver.Close();
                    this.FileReceiver = null;
                    if (this.DownloadThread != null && this.DownloadThread.IsAlive)
                    {
                        this.DownloadThread.Abort();
                        this.DownloadThread = null;
                    }
                    //this.CloseDownloadFile();
                }
                if (this.FRP != null && FileReceiveP2P.IsFinished)
                {
                    MessageType.AddMessageQueue(ref MessageQueue, "SEND_FILE: From <" + Program.NetIP + "> Exit.", MessageType.OUTPUT_COMMAND, this.FRP.ClientIP);
                    //MessageType.AddMessageQueue(ref MessageQueue, "DOWNLOAD_FILE: From <" + this.FRP.ClientIP + "> Exit.", MessageType.OUTPUT_COMMAND, Program.IP);
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "DOWNLOAD_FILE: From <" + this.FRP.ClientIP + "> Exit.", null);
                    if (this.ThreadFRP != null && this.ThreadFRP.IsAlive)
                    {
                        this.ThreadFRP.Abort();
                    }
                    this.FRP.IsStop = true;
                    this.FRP = null;
                    //this.CloseSendFile();
                }
                if (ScpClient_Ex.IsFinished && ScpClient_Ex.Type == 1)
                {
                    MessageType.MessageBoxShow("发送文件至Linux<" + ScpClient_Ex.IP + ">完成!", "info");
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "SEND_FILE: TO <" + ScpClient_Ex.IP + "> Completed.", null);
                    ScpClient_Ex.IsFinished = false;
                    ScpClient_Ex.Type = 0;
                    ScpClient_Ex.IP = "";
                }
                if (ScpClient_Ex.IsFinished && ScpClient_Ex.Type == 2)
                {
                    MessageType.MessageBoxShow("从Linux<" + ScpClient_Ex.IP + ">下载文件完成!", "info");
                    this.Invoke(EventOutputMessage, "[" + DateTime.Now.ToLongTimeString() + "] " + "DOWNLOAD_FILE: FROM <" + ScpClient_Ex.IP + "> Completed.", null);
                    ScpClient_Ex.IsFinished = false;
                    ScpClient_Ex.Type = 0;
                    ScpClient_Ex.IP = "";
                }
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// 包同步线程
        /// </summary>
        /// <param name="state"></param>
        private void GetMessage(object state)
        {
            while (true)
            {
                if (this.LinuxServer != null && this.LinuxServer.MessageQueue != null)
                {
                    while (this.LinuxServer.MessageQueue.Count > 0)
                    {
                        lock (this.LinuxServer.MessageQueue)
                        {
                            MessagePackage datagram = this.LinuxServer.MessageQueue.Dequeue();  // 取队列数据
                            lock (this.MessageQueue)
                            {
                                this.MessageQueue.Enqueue(datagram);
                            }
                        }
                    }
                }
                if (this.WinServer != null && this.WinServer.MessageQueue != null)
                {
                    while (this.WinServer.MessageQueue.Count > 0)
                    {
                        lock (this.WinServer.MessageQueue)
                        {
                            MessagePackage datagram = this.WinServer.MessageQueue.Dequeue();  // 取队列数据
                            lock (this.MessageQueue)
                            {
                                this.MessageQueue.Enqueue(datagram);
                            }
                        }
                    }
                }
                if (this.WinClient != null && this.WinClient.MessageQueue != null)
                {
                    while (this.WinClient.MessageQueue.Count > 0)
                    {
                        lock (this.WinClient.MessageQueue)
                        {
                            MessagePackage datagram = this.WinClient.MessageQueue.Dequeue();  // 取队列数据
                            lock (this.MessageQueue)
                            {
                                this.MessageQueue.Enqueue(datagram);
                            }
                        }
                    }
                }
                if (this.FileSender != null && this.FileSender.MessageQueue != null)
                {
                    while (this.FileSender.MessageQueue.Count > 0)
                    {
                        lock (this.FileSender.MessageQueue)
                        {
                            MessagePackage datagram = this.FileSender.MessageQueue.Dequeue();  // 取队列数据
                            lock (this.MessageQueue)
                            {
                                this.MessageQueue.Enqueue(datagram);
                            }
                        }
                    }
                }
                if (this.FileReceiver != null && this.FileReceiver.MessageQueue != null)
                {
                    while (this.FileReceiver.MessageQueue.Count > 0)
                    {
                        lock (this.FileReceiver.MessageQueue)
                        {
                            MessagePackage datagram = this.FileReceiver.MessageQueue.Dequeue();  // 取队列数据
                            lock (this.MessageQueue)
                            {
                                this.MessageQueue.Enqueue(datagram);
                            }
                        }
                    }
                }
                GC.Collect();
                Thread.Sleep(1000);
            }
        }
        #endregion

        #region IApplication Members

        public StatusStrip MyStatusStrip { get { return this.statusStrip; } }
        public DockPanel MyDockPanel { get { return this.dockPanelMain; } }
        public ArrayList aReturn { get { return this.hReturn; } }
        public string HomePath { get { return Program.startPath; } }
        public string CronPath { get { return this.ctask.crontabPath; } }
        public void AddCommand(string cmdStr)
        {
            MessageType.AddMessageQueue(ref this.MessageQueue, "127.0.0.1:" + cmdStr, MessageType.COMMAND_ENGION, "127.0.0.1");
        }

        #endregion

        #region IServiceContainer Members

        private ServiceContainer serviceContainer = new ServiceContainer();

        public void AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback, bool promote)
        {
            serviceContainer.AddService(serviceType, callback, promote);
        }

        public void AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback)
        {
            serviceContainer.AddService(serviceType, callback);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            serviceContainer.AddService(serviceType, serviceInstance, promote);
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            serviceContainer.AddService(serviceType, serviceInstance);
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            serviceContainer.RemoveService(serviceType, promote);
        }

        public void RemoveService(Type serviceType)
        {
            serviceContainer.RemoveService(serviceType);
        }
        #endregion

        #region IServiceProvider Members

        //由于Form类型本身间接的继承了IServiceProvider接口，所以我们要覆盖掉Form本身的实现
        //所以我们使用了new关键字
        public new object GetService(Type serviceType)
        {
            return serviceContainer.GetService(serviceType);
        }

        #endregion

        #region 加载布局
        string uiFile = Path.Combine(Application.StartupPath, "CustomUI.xml");
        public void MainTestForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(uiFile))
            {
                DeserializeDockContent ddContent = new DeserializeDockContent(GetContentFromPersistString);
                dockPanelMain.LoadFromXml(uiFile, ddContent);
            }
            try
            {
                if (MessageQueue == null)
                {
                    MessageQueue = new Queue<MessagePackage>();
                }
                this.EventOpenForm += new DelegateOpenForm(OpenForms);
                if (Program.IsServer)
                {
                    WinServer = new ServerReceiver();
                    WinServer.TcpSocketPort = Program.ServerPort;
                    WinServer.StartReceiver();
                }
                WinClient = new ClientReceiver();
                //开启监听处理线程
                this.WinClient.StartReceiver();
                LinuxServer = new SshServer();
                this.LinuxServer.StartWork();
                ThreadPool.QueueUserWorkItem(GetMessage);
                ThreadPool.QueueUserWorkItem(HandleMessage);
                ThreadPool.QueueUserWorkItem(CronTask);

                this.ApplicationExitF += new ApplicationExit(AppExit);
                Thread _temp = new Thread(CommandExec);
                _temp.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Socket或系统错误：" + ex.Message, "提示");
            }
        }
        private IDockContent GetContentFromPersistString(string persistString)
        {
            try
            {
                string s = typeof(PlugsForm).ToString();
                switch (persistString)
                {
                    case "ECSTOOL.CommandForm":
                        //this.ironPythonToolStripMenuItem1.Checked = true;
                        this.ironPythonToolStripMenuItem1.Checked = true;
                        this.cmdForm = new CommandForm();
                        this.cmdForm.parent = this;
                        this.serverForm.DelegateCommandF += new DelegateCommand(OpenCommand);
                        return this.cmdForm;
                    case "ECSTOOL.PlugsForm":
                        this.toolPlugToolStripMenuItem1.Checked = true;
                        this.plugsForm = new PlugsForm(this);
                        this.plugsForm.DelagateClosePlugF += new DelegateClosePlug(ClosePlugForm);
                        return this.plugsForm;
                    case "ECSTOOL.ServerList":
                        this.serverListToolStripMenuItem.Checked = true;
                        this.serverForm = new ServerList();
                        this.serverForm.parent = this;
                        this.serverForm.DelagateCloseServerF += new DelegateCloseServer(CloseServerForm);
                        this.serverForm.DelegateFileTransF += new DelegateFileTrans(FileTransCommand);
                        return this.serverForm;
                    case "ECSTOOL.ServerOperator":
                        this.fileTransToolStripMenuItem.Checked = true;
                        this.operForm = new ServerOperator();
                        this.operForm.parent = this;
                        this.operForm.DelagateCloseFileF += new DelegateCloseFile(CloseFileForm);
                        return this.operForm;
                    default:
                        Assembly assembly = Assembly.LoadFile(Path.Combine(Application.StartupPath, "Plugs\\" + persistString.Remove(persistString.LastIndexOf(".")) + ".dll"));
                        IPlugin instance = (IPlugin)assembly.CreateInstance(persistString);
                        instance.Application = this;
                        return (DockContent)instance;
                }
            }
            catch { }
            return null;
        }
        #endregion

        #region 窗体事件
        private void OpenForms(string param)
        {
            if (param.ToLower().Equals("cmd"))
            {
                if (this.cmdForm == null || this.cmdForm.IsHidden)
                {
                    ironPythonToolStripMenuItem1_Click(null, null);
                }
            }
            else if (param.ToLower().Equals("server"))
            {
                if (this.serverForm == null || this.serverForm.IsHidden)
                {
                    serverListToolStripMenuItem_Click(null, null);
                }
            }
        }
        private void withAPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Program.startPath, @"Docs\使用帮助.CHM");
            if (File.Exists(path))
            {
                Process.Start(path);
            }
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm hf = new HelpForm();
            hf.ShowDialog();
        }

        public MainFrame()
        {
            InitializeComponent();
            this.Text = "服务器集群管理平台V" + Program.version.Substring(0, 3);
            this.notifyECS.Text = this.Text;
        }

        private void ironPythonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.ironPythonToolStripMenuItem1.Checked == false)
            {
                if (cmdForm != null && !cmdForm.IsDisposed)
                {
                    cmdForm.Activate();
                    cmdForm.Focus();
                }
                else
                {
                    cmdForm = new CommandForm();
                    cmdForm.parent = this;
                    cmdForm.Show(dockPanelMain, DockState.Document);
                }
                this.ironPythonToolStripMenuItem1.Checked = true;
                //this.ironPythonToolStripMenuItem1.Image = ECSTOOL.Properties.Resources.correct;
            }
            else
            {
                if (cmdForm != null && !cmdForm.IsDisposed)
                {
                    //cmdForm.Close();
                    cmdForm.Hide();
                }
                this.ironPythonToolStripMenuItem1.Checked = false;
                //this.ironPythonToolStripMenuItem1.Image = ECSTOOL.Properties.Resources.correct1;
            }
        }

        private void toolPlugToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.toolPlugToolStripMenuItem1.Checked == false)
            {
                if (plugsForm != null && !plugsForm.IsDisposed)
                {
                    plugsForm.Activate();
                    plugsForm.Show();
                }
                else
                {
                    plugsForm = new PlugsForm(this);
                    plugsForm.Show(dockPanelMain, DockState.DockLeft);
                    this.plugsForm.DelagateClosePlugF += new DelegateClosePlug(ClosePlugForm);
                }
                this.toolPlugToolStripMenuItem1.Checked = true;
                //this.toolPlugToolStripMenuItem1.Image = ECSTOOL.Properties.Resources.correct;                
            }
            else
            {
                if (plugsForm != null && !plugsForm.IsDisposed)
                {
                    plugsForm.Close();
                }
                this.toolPlugToolStripMenuItem1.Checked = false; ;
                //this.toolPlugToolStripMenuItem1.Image = ECSTOOL.Properties.Resources.correct1;                
            }
        }
        private void serverListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.serverListToolStripMenuItem.Checked == false)
            {
                if (serverForm != null && !serverForm.IsDisposed)
                {
                    serverForm.Activate();
                }
                else
                {
                    serverForm = new ServerList();
                    this.serverForm.parent = this;
                    serverForm.Show(dockPanelMain, DockState.DockRight);
                    this.serverForm.DelagateCloseServerF += new DelegateCloseServer(CloseServerForm);
                    this.serverForm.DelegateFileTransF += new DelegateFileTrans(FileTransCommand);
                    this.serverForm.DelegateCommandF += new DelegateCommand(OpenCommand);
                }
                this.serverListToolStripMenuItem.Checked = true;
            }
            else
            {
                if (serverForm != null && !serverForm.IsDisposed)
                {
                    serverForm.Hide();
                }
                this.serverListToolStripMenuItem.Checked = false;
            }
        }
        private void fileTransToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.fileTransToolStripMenuItem.Checked == false)
            {
                if (operForm != null && !operForm.IsDisposed)
                {
                    operForm.Activate();
                }
                else
                {
                    this.operForm = new ServerOperator();
                    this.operForm.parent = this;
                    operForm.Show(dockPanelMain, DockState.Document);
                    this.operForm.DelagateCloseFileF += new DelegateCloseFile(CloseFileForm);
                }
                this.fileTransToolStripMenuItem.Checked = true;
            }
            else
            {
                if (operForm != null && !operForm.IsDisposed)
                {
                    operForm.Close();
                }
                this.fileTransToolStripMenuItem.Checked = false;
            }
        }
        private void MainFrame_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MessageQueue != null)  // 清空队列
            {
                lock (MessageQueue)
                {
                    while (MessageQueue.Count > 0)
                    {
                        MessagePackage datagram = MessageQueue.Dequeue();
                        datagram.Clear();
                    }
                }
            }
            if (LinuxServer != null)
            {
                LinuxServer.Close();
                LinuxServer = null;
            }
            if (WinServer != null)
            {
                WinServer.Close();
                WinServer = null;
            }
            if (WinClient != null)
            {
                WinClient.Close();
                WinClient = null;
            }
            if (FileSender != null)
            {
                FileSender.Close();
                FileSender = null;
            }
            if (FileReceiver != null)
            {
                FileReceiver.Close();
                FileReceiver = null;
            }
        }
        private void MainFrame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                this.notifyECS.Visible = true;
                //任务栏区显示图标 
                this.ShowInTaskbar = false;
                return;
            }
        }
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //保存布局
            dockPanelMain.SaveAsXml(Path.Combine(Application.StartupPath, "CustomUI.xml"));
            Application.ExitThread();
            //this.Close();
            Application.Exit();
            //this.Close();
        }
        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示 
                WindowState = FormWindowState.Maximized;
                //激活窗体并给予它焦点 
                this.Activate();
                //任务栏区显示图标 
                this.ShowInTaskbar = true;
                this.notifyECS.Visible = false;
            }
        }
        private void notifyECS_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            显示ToolStripMenuItem_Click(null, null);
        }

        private void 退出系统ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //保存布局
            dockPanelMain.SaveAsXml(Path.Combine(Application.StartupPath, "CustomUI.xml"));
            Application.ExitThread();
            Application.Exit();
        }
        private void welcomeNginxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://127.0.0.1:80/");
            }
            catch { }
        }
        private void 停止NginxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageType.AddMessageQueue(ref this.MessageQueue, "nginx", MessageType.KILL_PROC, "127.0.0.1");
        }

        private void 启动NginxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                //if (CMD == null)
                //{
                //    CMD = new WinCMD();
                //}
                //string output = CMD.Execute("cd nginx && nginx -s stop >nul", 10);
                //MessageType.AddMessageQueue(ref MessageQueue, "EXEC_CMD:  <127.0.0.1> Output.\n" + output, MessageType.OUTPUT_COMMAND, "127.0.0.1");
                //output = CMD.Execute("cd nginx && start nginx >nul", 60);
                //MessageType.AddMessageQueue(ref MessageQueue, "EXEC_CMD:  <127.0.0.1> Output.\n" + output, MessageType.OUTPUT_COMMAND, "127.0.0.1");
                MessageType.AddMessageQueue(ref this.MessageQueue, "cd nginx && nginx -s stop >nul", MessageType.EXEC_CMD, "127.0.0.1");
                MessageType.AddMessageQueue(ref this.MessageQueue, "127.0.0.1:win_cmd{@cd nginx && start nginx >nul@}", MessageType.COMMAND_ENGION, "127.0.0.1");
            }
            catch { }
        }
        #endregion
    }
}
