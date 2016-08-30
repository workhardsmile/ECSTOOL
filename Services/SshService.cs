using System;
using Renci.SshNet;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Routrek.Crypto;
using Routrek.SSHC;
using Routrek.SSHCV1;
using Routrek.SSHCV2;
using Routrek.Toolkit;
using Routrek.PKI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ECSTOOL
{
    public delegate void DelegateMoveFile(string ip, string name, long size, string[] dirs, int timeout);
    public class SshServer
    {
        private int _loopWaitTime = 20;                      // 默认循环等待时间（毫秒）

        public Queue<ShellPkg> _datagramQueueBroadcast;  // 待处理数据包队列（广播发送）
        public Queue<ShellPkg> _datagramQueueRequest;  // 待处理数据包队列（请求）

        public Hashtable ServerTable;     //IP,port|user|pwd
        public Hashtable SessionTable;    //连接会话：IP,SshReader

        public bool _stopReceiver = true;          // 停止 DatagramReceiver 接收器

        public Queue<MessagePackage> MessageQueue;  //待处理数据包队列（主线程）
        public event DelegateCheckSession DelegateCheckSessionF; //声明事件
        public DelegateMoveFile DelegateMoveFileF;

        public SshServer()
        {
            ServerTable = new Hashtable();
            SessionTable = new Hashtable();
            this.DelegateMoveFileF += new DelegateMoveFile(this.MoveSendFile);
        }
        public void Close()  // 关闭接收器（要求在断开全部连接后才能停止）
        {
            // 先设置该值, 否则在循环 AccecptClientConnect 时可能出错
            _stopReceiver = true;
            CloseAllSession();
            if (_datagramQueueRequest != null)  // 清空队列
            {
                lock (_datagramQueueRequest)
                {
                    while (_datagramQueueRequest.Count > 0)
                    {
                        //TDatagram datagram = _datagramQueueRequest.Dequeue();
                        //datagram.Clear();
                    }
                }
            }
            if (_datagramQueueBroadcast != null)  // 清空队列
            {
                lock (_datagramQueueBroadcast)
                {
                    while (_datagramQueueBroadcast.Count > 0)
                    {
                        //TDatagram datagram = _datagramQueueRequest.Dequeue();
                        //datagram.Clear();
                    }
                }
            }
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
            if (SessionTable != null)  // 关闭各个会话
            {
                lock (SessionTable)
                {
                    foreach (SshReader session in SessionTable.Values)
                    {
                        try
                        {
                            session.CloseConnect();
                        }
                        catch { }
                    }
                }
            }
            if (SessionTable != null)  // 最后清空会话列表
            {
                lock (SessionTable)
                {
                    SessionTable.Clear();
                }
            }
        }
        /// <summary>
        ///  启动接收器
        /// </summary>
        public bool StartWork()
        {
            try
            {
                _stopReceiver = true;
                this.Close();

                _datagramQueueRequest = new Queue<ShellPkg>();
                _datagramQueueBroadcast = new Queue<ShellPkg>();
                MessageQueue = new Queue<MessagePackage>();

                _stopReceiver = false;  // 循环中均要该标志

                //侦听客户端连接请求线程, 使用委托推断, 不建 CallBack 对象
                //if (!ThreadPool.QueueUserWorkItem(ConnectAllServer))
                //{
                //    return false;
                //}

                // 处理数据包队列线程
                if (!ThreadPool.QueueUserWorkItem(RequestDatagrams))
                {
                    return false;
                }

                // 响应数据包队列线程
                if (!ThreadPool.QueueUserWorkItem(BroadcastDatagrams))
                {
                    return false;
                }

                // 检查客户会话状态, 长时间未通信则清除该对象
                if (!ThreadPool.QueueUserWorkItem(CheckClientState))
                {
                    return false;
                }
            }
            catch
            {
                _stopReceiver = true;
            }
            return !_stopReceiver;
        }
        /// <summary>
        /// 连接所有服务器，由于要用线程池，故带一个参数
        /// </summary>
        public void ConnectAllServer(object state)
        {
            CloseAllSession();
            foreach (string _ip in ServerTable.Keys)
            {
                bool flag = false;
                ServerInfo si = (ServerInfo)ServerTable[_ip];
                if (si != null && si.OsType == 0)
                {
                    SshReader sr = new SshReader(si.IP, si.SshPort, si.User, si.Password);
                    try
                    {
                        sr.OpenConnect();
                        flag = true;
                    }
                    catch
                    {
                        flag = false;
                    }
                    if (SessionTable[si.IP] == null && flag)
                    {
                        SessionTable.Add(si.IP, sr);
                    }
                }
            }
        }
        /// <summary>
        /// 连接一个服务器
        /// </summary>
        public bool ConnectOneServer(string _ip)
        {
            CloseOneSession(_ip);

            bool flag = false;
            ServerInfo si = (ServerInfo)ServerTable[_ip];
            if (si != null && si.OsType == 0)
            {
                SshReader sr = new SshReader(si.IP, si.SshPort, si.User, si.Password);
                try
                {
                    sr.OpenConnect();
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
                if (SessionTable[si.IP] == null && flag)
                {
                    SessionTable.Add(si.IP, sr);
                }
            }
            return flag;
        }
        /// <summary>
        /// 检查客户端状态（扫描方式，若长时间无数据，则断开）
        /// </summary>
        private void CheckClientState(object state)
        {
            while (!_stopReceiver)
            {
                try
                {
                    foreach (string _ip in ServerTable.Keys)
                    {
                        SshReader sr = (SshReader)SessionTable[_ip];
                        if (sr == null || sr._conn.IsClosed)
                        {
                            sr = null;
                            SessionTable.Remove(_ip);
                            DelegateCheckSessionF(_ip + ":closed", null);
                        }
                        else
                        {
                            DelegateCheckSessionF(_ip + ":normal", null);
                        }
                    }
                }
                catch { }
                Thread.Sleep(_loopWaitTime);
            }
        }
        /// <summary>
        /// 处理数据包队列，由于要用线程池，故带一个参数
        /// </summary>
        private void RequestDatagrams(object state)
        {
            int mill = 5 * _loopWaitTime;
            while (!_stopReceiver)
            {
                if (_datagramQueueRequest.Count > 0)
                {
                    lock (_datagramQueueRequest)
                    {
                        ShellPkg sp = _datagramQueueRequest.Dequeue();
                        switch (sp.MessageType)
                        {
                            case MessageType.EXEC_SHELL:
                                SshReader sr = (SshReader)SessionTable[sp.IP];
                                ServerInfo si = (ServerInfo)ServerTable[sp.IP];
                                if (sr != null && si != null)
                                {
                                    if (si.User.Equals("root"))
                                    {
                                        sr.WaitString("]#", mill);
                                    }
                                    else
                                    {
                                        sr.WaitString("]$", mill);
                                    }
                                    sr.InputCommand(sp.Command);
                                    if (si.User.Equals("root"))
                                    {
                                        sr.WaitString("]#", mill);
                                    }
                                    else
                                    {
                                        sr.WaitString("]$", mill);
                                    }
                                    sr.ClearMsg();
                                    MessageType.AddMessageQueue(ref this.MessageQueue, "SHELL_RETURN: [" + DateTime.Now.ToLongTimeString() + "]<" + sp.IP + ">\n" + sr.msg, MessageType.OUTPUT_COMMAND, Program.IP);
                                    sr.msg = "";
                                }
                                break;
                            case MessageType.OUTPUT_COMMAND:
                                MessageType.AddMessageQueue(ref this.MessageQueue, "SHELL_RETURN: [" + DateTime.Now.ToLongTimeString() + "]<" + sp.IP + ">\n" + sp.Command, MessageType.OUTPUT_COMMAND, Program.IP);
                                break;
                            default:
                                break;
                        }
                    }
                }
                Thread.Sleep(_loopWaitTime);
            }
        }
        /// <summary>
        /// 处理数据包队列，由于要用线程池，故带一个参数
        /// </summary>
        private void BroadcastDatagrams(object state)
        {
            int mill = 5 * _loopWaitTime;
            while (!_stopReceiver)
            {
                if (_datagramQueueBroadcast.Count > 0)
                {
                    lock (_datagramQueueBroadcast)
                    {
                        ShellPkg sp = _datagramQueueBroadcast.Dequeue();
                        foreach (string _ip in SessionTable.Keys)
                        {
                            SshReader sr = (SshReader)SessionTable[_ip];
                            ServerInfo si = (ServerInfo)ServerTable[_ip];
                            switch (sp.MessageType)
                            {
                                case MessageType.EXEC_SHELL:
                                    if (sr != null && si != null)
                                    {
                                        if (si.User.Equals("root"))
                                        {
                                            sr.WaitString("]#", mill);
                                        }
                                        else
                                        {
                                            sr.WaitString("]$", mill);
                                        }
                                        sr.InputCommand(sp.Command);
                                        if (si.User.Equals("root"))
                                        {
                                            sr.WaitString("]#", mill);
                                        }
                                        else
                                        {
                                            sr.WaitString("]$", mill);
                                        }
                                        sr.ClearMsg();
                                        MessageType.AddMessageQueue(ref this.MessageQueue, "SHELL_RETURN: [" + DateTime.Now.ToLongTimeString() + "]<" + _ip + ">\n" + sr.msg, MessageType.OUTPUT_COMMAND, Program.IP);
                                        sr.msg = "";
                                    }
                                    break;
                                case MessageType.OUTPUT_COMMAND:
                                    MessageType.AddMessageQueue(ref this.MessageQueue, "SHELL_RETURN: [" + DateTime.Now.ToLongTimeString() + "]<" + _ip + ">\n" + sp.Command, MessageType.OUTPUT_COMMAND, Program.IP);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                Thread.Sleep(mill);
            }
        }
        /// <summary>
        ///  关闭全部客户端会话，并做关闭标记
        /// </summary>
        public void CloseAllSession()
        {
            try
            {
                if (SessionTable.Keys != null)
                {
                    foreach (string _ip in SessionTable.Keys)
                    {
                        CloseOneSession(_ip);
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 直接关闭一个客户端会话
        /// </summary>
        /// <param name="sessionIP"></param>
        public bool CloseOneSession(string sessionIP)
        {
            try
            {
                SshReader sr = (SshReader)SessionTable[sessionIP];
                if (sr != null)
                {
                    if (!sr._conn.IsClosed)
                    {
                        sr.CloseConnect();
                    }
                    sr = null;
                }
                SessionTable.Remove(sessionIP);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 文件夹是否存在
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="DirPath"></param>
        /// <returns></returns>
        public bool LCheckDirectory(string IP, string DirPath)
        {
            bool flag = false;
            SshReader sr = (SshReader)SessionTable[IP];
            ServerInfo si = (ServerInfo)ServerTable[IP];
            int n = 500;
            if (sr == null && si != null)
            {
                ConnectOneServer(IP);
                while (sr == null && n > 0)
                {
                    Thread.Sleep(10);
                    sr = (SshReader)SessionTable[IP];
                    if (sr != null)
                    {
                        break;
                    }
                    n--;
                }
            }
            if (sr == null || si == null)
            {
                return flag;
            }
            string command = "if [ -d \"" + DirPath + "\" ]; then echo \"True\"; else echo \"False\"; fi;";
            if (si.User.Equals("root"))
            {
                sr.WaitString("]#", _loopWaitTime * 5);
                sr.InputCommand(command);
                sr.WaitString("]#", _loopWaitTime * 5);
            }
            else
            {
                sr.WaitString("]$", _loopWaitTime * 5);
                sr.InputCommand(command);
                sr.WaitString("]$", _loopWaitTime * 5);
            }
            sr.ClearMsg();
            if (sr.msg.Trim().Equals("True"))
            {
                flag = true;
            }
            return flag;
        }
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="FilePath"></param>
        /// <param name="Size">最小大小</param>
        /// <param name="TimeOut"></param>
        /// <returns>文件大小</returns>
        public long LCheckFile(string IP, string FilePath, long Size, int TimeOut)
        {
            long length = 0;
            SshReader sr = (SshReader)SessionTable[IP];
            ServerInfo si = (ServerInfo)ServerTable[IP];
            bool flag = false;
            int n = 500;
            if (sr == null && si != null)
            {
                ConnectOneServer(IP);
                while (sr == null && n > 0)
                {
                    Thread.Sleep(10);
                    sr = (SshReader)SessionTable[IP];
                    if (sr != null)
                    {
                        break;
                    }
                    n--;
                }
            }
            if (sr == null || si == null)
            {
                return length;
            }
            string command = "du -b \"" + FilePath.Trim() + "\"|awk '{print $1}'";
            while (TimeOut > 0)
            {
                if (si.User.Equals("root"))
                {
                    sr.WaitString("]#", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]#", _loopWaitTime * 5);
                }
                else
                {
                    sr.WaitString("]$", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]$", _loopWaitTime * 5);
                }
                sr.ClearMsg();
                if (sr.msg != "")
                {
                    try
                    {
                        length = Convert.ToInt64(sr.msg.Trim());
                    }
                    catch { }
                }
                if (length >= Size)
                {
                    break;
                }
                sr.msg = "";
                Thread.Sleep(10);
                TimeOut--;
            }
            return length;
        }
        /// <summary>
        /// 发送文件后续
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="dirs"></param>
        /// <param name="timeout"></param>
        public void MoveSendFile(string ip, string name, long size, string[] dirs, int timeout)
        {
            SshReader sr = (SshReader)SessionTable[ip];
            ServerInfo si = (ServerInfo)ServerTable[ip];
            bool flag = false;
            int n = 500;
            if (sr == null && si != null)
            {
                ConnectOneServer(ip);
                while (sr == null && n > 0)
                {
                    Thread.Sleep(10);
                    sr = (SshReader)SessionTable[ip];
                    if (sr != null)
                    {
                        break;
                    }
                    n--;
                }
            }
            if (sr == null || si == null)
            {
                return;
            }
            string command = "cd ~&&du -b " + name + "|awk '{print $1}'";
            while (timeout > 0)
            {
                if (si.User.Equals("root"))
                {
                    sr.WaitString("]#", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]#", _loopWaitTime * 5);
                }
                else
                {
                    sr.WaitString("]$", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]$", _loopWaitTime * 5);
                }
                sr.ClearMsg();
                long _size = 0;
                if (sr.msg != "")
                {
                    try
                    {
                        _size = Convert.ToInt64(sr.msg.Trim());
                    }
                    catch { }
                }
                if (_size >= size)
                {
                    flag = true;
                    break;
                }
                sr.msg = "";
                Thread.Sleep(100);
                timeout--;
            }
            if (flag)
            {
                n = 0;
                command = "pwd";
                if (si.User.Equals("root"))
                {
                    sr.WaitString("]#", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]#", _loopWaitTime * 5);
                }
                else
                {
                    sr.WaitString("]$", _loopWaitTime * 5);
                    sr.InputCommand(command);
                    sr.WaitString("]#", _loopWaitTime * 5);
                }
                string home = sr.msg;
                foreach (string dir in dirs)
                {                    
                    if (dir.Equals(home) || dir.Trim().Equals("~") || dir.Equals(home + "/") || dir.Trim().Equals("~/"))
                    {
                        n = 1;
                        continue;
                    }
                    command = "cp ~/" + name + " " + dir.Trim();
                    if (si.User.Equals("root"))
                    {
                        sr.WaitString("]#", _loopWaitTime * 5);
                        sr.InputCommand(command);
                    }
                    else
                    {
                        sr.WaitString("]$", _loopWaitTime * 5);
                        sr.InputCommand(command);
                    }

                }
                if (n != 1)
                {
                    command = "rm -f ~/" + name;
                    if (si.User.Equals("root"))
                    {
                        sr.WaitString("]#", _loopWaitTime * 5);
                        sr.InputCommand(command);
                    }
                    else
                    {
                        sr.WaitString("]$", _loopWaitTime * 5);
                        sr.InputCommand(command);
                    }
                }
                ScpClient_Ex.IsFinished = true;
                ScpClient_Ex.Type = 1;
                ScpClient_Ex.IP = ip;
            }
        }
    }
    /// <summary>
    /// ssh协议
    /// </summary>
    public class SshReader : ISSHConnectionEventReceiver, ISSHChannelEventReceiver
    {
        public SSHConnection _conn;
        public SSHChannel _pf;
        public bool _ready;
        public string msg = "";

        private SSHConnectionParameter f = new SSHConnectionParameter();
        public string IP;
        private string username, password;
        private int port;
        private Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //private System.Timers.Timer timer;
        public SshReader(string host, int port, string username, string password)
        {
            this.IP = host;
            this.port = port;
            this.username = username;
            this.password = password;
        }
        public SshReader()
        {
            // TODO: Complete member initialization
        }
        public void OpenConnect()
        {
            f.UserName = username;
            f.Password = password;
            f.Protocol = SSHProtocol.SSH2;
            f.AuthenticationType = AuthenticationType.Password;
            f.WindowSize = 0x1000;
            s.Connect(new IPEndPoint(IPAddress.Parse(IP), port));
            _conn = SSHConnection.Connect(f, this, s);
            this._pf = _conn.OpenShell(this);
            SSHConnectionInfo ci = _conn.ConnectionInfo;
        }
        public void CloseConnect()
        {
            this.msg = "";
            this.s.Close();
            this._conn.Close();
            this._pf.Close();
        }
        public void WaitString(string s, int time)
        {
            int count = 0;
            Thread.Sleep(100);
            while (true)
            {
                if (this.msg.IndexOf(s) > 0)
                {
                    return;
                }
                if (count > 99999)
                {
                    Thread.Sleep(time);
                    return;
                }
                count++;
                //timer = new System.Timers.Timer(10);
                //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                //timer.Enabled = true;
            }

        }
        //private void timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Thread.CurrentThread.IsBackground = false;
        //    timer.Enabled = false;
        //}
        /// <summary>
        /// 格式化
        /// </summary>
        public void ClearMsg()
        {
            int start = this.msg.IndexOf("\n");
            int end = this.msg.LastIndexOf("\r");
            try
            {
                if (start >= 0 && start <= end)
                {
                    this.msg = this.msg.Remove(end).Remove(0, start + 1);
                    this.msg = Regex.Replace(this.msg, @".{1}\[[0-9].{0,4}m", "");
                    //MatchCollection mc = Regex.Matches(this.msg, @".{1}\[0.{0,7}m", RegexOptions.CultureInvariant);
                    //if (mc != null && mc.Count>0)
                    //{
                    //    foreach (Match m in mc)
                    //    {
                    //        this.msg = this.msg.Replace(m.Value, "");
                    //    }
                    //}
                }
            }
            catch { }

        }
        public void InputCommand(string command)
        {
            this.msg = "";
            byte[] data = (new UnicodeEncoding()).GetBytes(command + "\n");
            this._pf.Transmit(data);
        }
        public void OnData(byte[] data, int offset, int length)
        {
            //System.Console.Write(Encoding.ASCII.GetString(data, offset, length));
            msg += Encoding.ASCII.GetString(data, offset, length);
        }
        public void OnDebugMessage(bool always_display, byte[] data)
        {
            Debug.WriteLine("DEBUG: " + Encoding.ASCII.GetString(data));
        }
        public void OnIgnoreMessage(byte[] data)
        {
            Debug.WriteLine("Ignore: " + Encoding.ASCII.GetString(data));
        }
        public void OnAuthenticationPrompt(string[] msg)
        {
            Debug.WriteLine("Auth Prompt " + msg[0]);
        }

        public void OnError(Exception error, string msg)
        {
            Debug.WriteLine("ERROR: " + msg);
        }
        public void OnChannelClosed()
        {
            Debug.WriteLine("Channel closed");
            _conn.Disconnect("");
            //_conn.AsyncReceive(this);
        }
        public void OnChannelEOF()
        {
            _pf.Close();
            Debug.WriteLine("Channel EOF");
        }
        public void OnExtendedData(int type, byte[] data)
        {
            Debug.WriteLine("EXTENDED DATA");
        }
        public void OnConnectionClosed()
        {
            Debug.WriteLine("Connection closed");
        }
        public void OnUnknownMessage(byte type, byte[] data)
        {
            Debug.WriteLine("Unknown Message " + type);
        }
        public void OnChannelReady()
        {
            _ready = true;
        }
        public void OnChannelError(Exception error, string msg)
        {
            Debug.WriteLine("Channel ERROR: " + msg);
        }
        public void OnMiscPacket(byte type, byte[] data, int offset, int length)
        {
        }
        public PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator_host, int originator_port)
        {
            PortForwardingCheckResult r = new PortForwardingCheckResult();
            r.allowed = true;
            r.channel = this;
            return r;
        }
        public void EstablishPortforwarding(ISSHChannelEventReceiver rec, SSHChannel channel)
        {
            _pf = channel;
        }
    }
    /// <summary>
    /// 文件传输
    /// </summary>
    public class ScpClient_Ex
    {
        private string dir_upload;
        private string file_upload;
        private string upload_name;
        private string dir_download;
        private string file_download;
        private string download_name;
        private ScpClient scpClient;
        public string msg;
        public static bool IsFinished = false;
        public static int Type = 0;
        public static string IP = "";

        public string Dir_Upload
        {
            set
            {
                this.dir_upload = value;
            }
            get
            {
                return this.dir_upload;
            }
        }
        public string File_Upload
        {
            set
            {
                this.file_upload = value;
            }
            get
            {
                return this.file_upload;
            }
        }
        public string Dir_Download
        {
            set
            {
                this.dir_download = value;
            }
            get
            {
                return this.dir_download;
            }
        }
        public string File_Download
        {
            set
            {
                this.file_download = value;
            }
            get
            {
                return this.file_download;
            }
        }
        public string Upload_Name
        {
            set
            {
                this.upload_name = value;
            }
            get
            {
                return this.upload_name;
            }
        }
        public string Download_Name
        {
            set
            {
                this.download_name = value;
            }
            get
            {
                return this.download_name;
            }
        }
        public ScpClient_Ex(string host, int port, string username, string password)
        {
            dir_upload = "";
            file_upload = "";
            dir_download = "";
            file_download = "";
            upload_name = "";
            download_name = "";
            scpClient = new ScpClient(host, port, username, password);
            scpClient.BufferSize = 1024;
            IsFinished = false;
            Type = 0;
        }
        public void DownloadFile()
        {
            if (!download_name.Equals(""))
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file_download);
                    this.scpClient.Connect();
                    this.scpClient.Download(download_name, fi);
                    this.scpClient.Disconnect();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    this.msg = "DownloadFile Error: " + ex.Message;
                }
            }
            else
            {
                this.msg = "下载文件路径错误或者目标路径名不存在！";
            }
        }
        //public void DownloadToDir()
        //{
        //    if (!download_name.Equals("") && System.IO.Directory.Exists(dir_download))
        //    {
        //        try
        //        {
        //            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dir_download);
        //            this.scpClient.Connect();
        //            this.scpClient.Download(download_name, dir);
        //            this.scpClient.Disconnect();
        //            Thread.Sleep(100);
        //        }
        //        catch (Exception ex)
        //        {
        //            this.msg = "DownloadDirectory Error: " + ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        this.msg = "下载目录路径错误或者目标路径名不存在！";
        //    }
        //}
        public void UploadFile()
        {
            if (!upload_name.Equals("") && System.IO.File.Exists(file_upload))
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file_upload);
                    this.scpClient.Connect();
                    this.scpClient.Upload(fi, upload_name);
                    this.scpClient.Disconnect();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    this.msg = "UploadFile Error: " + ex.Message;
                }
            }
            else
            {
                this.msg = "上传文件路径不存在或者目标文件名错误！";
            }
        }
        public void UploadFromDir()
        {
            if (!upload_name.Equals("") && System.IO.Directory.Exists(dir_upload))
            {
                try
                {
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dir_upload);
                    this.scpClient.Connect();
                    this.scpClient.Upload(dir, upload_name);
                    this.scpClient.Disconnect();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    this.msg = "UploadDirectory Error: " + ex.Message;
                }
            }
            else
            {
                this.msg = "上传目录路径不存在或者目标目录名错误！";
            }
        }
        ~ScpClient_Ex()
        {
            this.scpClient.Dispose();
        }
    }
}
