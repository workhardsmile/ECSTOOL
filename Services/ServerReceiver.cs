using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;

namespace ECSTOOL
{
    #region  数据包接收器类 DatagramReceiver

    /// <summary>
    /// 类 名 称：Socket 数据包接收器类 DatagramReceiver
    /// 主要功能：公路交通流量调查Socket数据接收接收器类
    /// 编 写 者：HuLihui
    /// 创建日期：2005-11-23
    /// 修改日期：2006-04-13, 2006-05-05
    /// 框架版本：2008-10-05（用于技术探讨，便于了解框架）
    /// 说    明：1) 代码编译没有错误，但省略了许多内容
    ///           2) 原代码超过3K，可能有些地方删减欠妥，请留言探讨
    ///</summary>

    public class ServerReceiver
    {

        #region 事件（可按自己要求定制参数类型）

        public event EventHandler ReceiverException;  // 接收器异常错误
        public event EventHandler ReceiverWork;       // 接收器工作事件（启动、暂停、关闭、改参）

        public event EventHandler ClientException;  // 客户端异常事件（读/写异常）
        public event EventHandler ClientRequest;    // 客户端请求事件（连接、关闭）

        public event EventHandler DatagramError;   // 错误数据包
        public event EventHandler DatagramHandle;  // 处理数据包

        #endregion

        #region 字段

        private int _loopWaitTime = 25;                      // 默认循环等待时间（毫秒）
        private int _maxAllowDatagramQueueCount = 8 * 1024;  // 默认的最大队列数据
        private int _maxAllowClientCount = 256;              // 接收器程序允许的最大客户端连接数
        private int _maxAllowListenQueueLength = 5;          // 侦听队列的最大长度
        private int _maxSocketDataTimeout = 180;             // 最大无数据接收时间（120秒）
        private int _maxDatagramSize = 65535;

        private int _clientCount;           // 连接客户端会话计数
        private int _datagramCount;         // 总计收到客户端数据包（含错误包）
        private int _errorDatagramCount;    // 总计收到客户端错误数据包
        private int _exceptionCount;        // 接收器端总计发生的异常错误
        public int _datagramQueueRequestCount;    // 待处理包计数（请求）
        //public int _datagramQueueResponseCount;    // 待处理包计数（响应发送）
        //public int _datagramQueueBroadcastCount;    // 待处理包计数（广播）

        public Queue<TDatagram> _datagramQueueBroadcast;  // 待处理数据包队列（广播发送）
        public Queue<TDatagram> _datagramQueueRequest;  // 待处理数据包队列（请求）
        public Queue<TDatagram> _datagramQueueResponse;  // 待处理数据包队列（响应发送）

        public Queue<MessagePackage> MessageQueue;  //待处理数据包队列（主线程）

        private int _tcpSocketPort = 18888;  // 接收器端口号
        private Socket _receiverSocket;     // 接收器 Socket 对象

        private bool _stopReceiver = true;          // 停止 DatagramReceiver 接收器
        private bool _stopConnectRequest = false;   // 停止客户端连接请求

        private Hashtable _sessionTable;          // 客户端会话哈希表

        private string _dbConnectionStr;       // 数据库连接字串（时段）
        private SqlConnection _sqlConnection;  // 数据库连接对象（时段）

        private ArrayList BlackIPS = new ArrayList();

        //private Thread ListenThread;
        //private Thread RequestThread;
        //private Thread ResponseThread;
        //private Thread CheckThread;

        #endregion

        #region 属性

        public bool IsRun  // 接收器运行状态
        {
            get { return !_stopReceiver; }
        }

        public bool StopConnectRequst
        {
            get { return _stopConnectRequest; }
            set { _stopConnectRequest = value; }
        }

        public int TcpSocketPort  // Socket 端口
        {
            get { return _tcpSocketPort; }
            set { _tcpSocketPort = value; }
        }

        public int DatagramCount  // 总计接收数据包
        {
            get { return _datagramCount; }
        }

        public int ErrorDatagramCount  // 总计错误数据包
        {
            get { return _errorDatagramCount; }
        }

        public int ExceptionCount  // 接收器异常错误数
        {
            get { return _exceptionCount; }
        }

        public int ClientCount  // 当前的客户端连接数
        {
            get { return _clientCount; }
        }

        public int DatagramQueueCount  // 待处理的数据包总数
        {
            get { return _datagramQueueRequestCount; }
        }

        public int MaxAllowClientCount  // 允许连接的最大数
        {
            get { return _maxAllowClientCount; }
            set { _maxAllowClientCount = value; }
        }

        public int MaxListenQueueLength  // 侦听队列的长度
        {
            get { return _maxAllowListenQueueLength; }
            set { _maxAllowListenQueueLength = value; }
        }

        public int MaxAllowDatagramQueueCount  // 最大数据包队列数
        {
            set { _maxAllowDatagramQueueCount = value; }
        }

        public int MaxSocketDataTimeout  // 最大无数据传输时间（即超时时间）
        {
            get { return _maxSocketDataTimeout; }
            set { _maxSocketDataTimeout = value; }
        }

        public int LoopWaitTime  // 循环等待时间
        {
            get { return _loopWaitTime; }
            set { _loopWaitTime = value; }
        }

        public string DBConnectionStr
        {
            set { _dbConnectionStr = value; }
        }

        public Hashtable ClientSessionTable  // 当前在线会话列表副本
        {
            get
            {
                Hashtable sessionOnline = new Hashtable();
                lock (_sessionTable)
                {
                    foreach (TSession session in _sessionTable.Values)
                    {
                        sessionOnline.Add(session.ID, session);
                    }
                }
                return sessionOnline;
            }
        }

        #endregion

        public ServerReceiver() { }  // 使用默认参数

        #region 公有方法

        public void Close()  // 关闭接收器（要求在断开全部连接后才能停止）
        {
            // 先设置该值, 否则在循环 AccecptClientConnect 时可能出错
            _stopReceiver = true;
            this.BlackIPS.Clear();
            //if (_sqlConnection != null)
            //{
            //    this.CloseDatabase();
            //}

            if (_datagramQueueRequest != null)  // 清空队列
            {
                lock (_datagramQueueRequest)
                {
                    while (_datagramQueueRequest.Count > 0)
                    {
                        TDatagram datagram = _datagramQueueRequest.Dequeue();
                        datagram.Clear();
                    }
                }
            }
            if (_datagramQueueResponse != null)  // 清空队列
            {
                lock (_datagramQueueResponse)
                {
                    while (_datagramQueueResponse.Count > 0)
                    {
                        TDatagram datagram = _datagramQueueResponse.Dequeue();
                        datagram.Clear();
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
            if (_sessionTable != null)  // 关闭各个会话
            {
                lock (_sessionTable)
                {
                    foreach (TSession session in _sessionTable.Values)
                    {
                        try
                        {
                            session.Clear();
                            session.ClientSocket.Close();
                        }
                        catch { }
                    }
                }
            }

            if (_receiverSocket != null)  // 关闭接收器 Socket
            {
                lock (_receiverSocket)
                {
                    try
                    {
                        if (_sessionTable != null && _sessionTable.Count > 0)
                        {
                            // 可能引起 AcceptClientConnect 的 Poll 结束
                            _receiverSocket.Shutdown(SocketShutdown.Both);
                        }
                        _receiverSocket.Close();
                    }
                    catch
                    {
                        this.OnReceiverException();
                    }
                }
            }

            if (_sessionTable != null)  // 最后清空会话列表
            {
                lock (_sessionTable)
                {
                    _sessionTable.Clear();
                }
            }
        }

        /// <summary>
        ///  启动接收器
        /// </summary>
        public bool StartReceiver()
        {
            try
            {
                _stopReceiver = true;
                this.Close();

                //if (!this.ConnectDatabase()) return false;

                _clientCount = 0;
                _datagramQueueRequestCount = 0;
                _datagramCount = 0;
                _errorDatagramCount = 0;
                _exceptionCount = 0;

                _sessionTable = new Hashtable(_maxAllowClientCount);
                _datagramQueueRequest = new Queue<TDatagram>(_maxAllowDatagramQueueCount);
                _datagramQueueResponse = new Queue<TDatagram>(_maxAllowDatagramQueueCount);
                _datagramQueueBroadcast = new Queue<TDatagram>(_maxAllowDatagramQueueCount);
                MessageQueue = new Queue<MessagePackage>(_maxAllowDatagramQueueCount);

                _stopReceiver = false;  // 循环中均要该标志

                if (!this.CreateReceiverSocket())  //建立服务器端 Socket 对象
                {
                    return false;
                }
                //ListenClientRequest(null);
                //侦听客户端连接请求线程, 使用委托推断, 不建 CallBack 对象
                if (!ThreadPool.QueueUserWorkItem(ListenClientRequest))
                {
                    return false;
                }
                //if (this.ListenThread == null)
                //{
                //    this.ListenThread = new Thread(ListenClientRequest);
                //    this.ListenThread.IsBackground = true;
                //}
                //this.ListenThread.Start();

                // 处理数据包队列线程
                if (!ThreadPool.QueueUserWorkItem(HandleDatagrams))
                {
                    return false;
                }
                //if (this.RequestThread == null)
                //{
                //    this.RequestThread = new Thread(HandleDatagrams);
                //    this.RequestThread.IsBackground = true;
                //}
                //this.RequestThread.Start();

                // 响应数据包队列线程
                if (!ThreadPool.QueueUserWorkItem(ResponseDatagrams))
                {
                    return false;
                }
                //if (this.ResponseThread == null)
                //{
                //    this.ResponseThread = new Thread(ResponseDatagrams);
                //    this.ResponseThread.IsBackground = true;
                //}
                //this.ResponseThread.Start();

                // 检查客户会话状态, 长时间未通信则清除该对象
                if (!ThreadPool.QueueUserWorkItem(CheckClientState))
                {
                    return false;
                }
                //if (this.CheckThread == null)
                //{
                //    this.CheckThread = new Thread(CheckClientState);
                //    this.ResponseThread.IsBackground = true;
                //}
                //this.CheckThread.Start();

                _stopConnectRequest = false;  // 启动接收器，则自动允许连接
            }
            catch
            {
                this.OnReceiverException();
                _stopReceiver = true;
            }
            return !_stopReceiver;
        }

        /// <summary>
        ///  关闭全部客户端会话，并做关闭标记
        /// </summary>
        public void CloseAllSession()
        {
            lock (_sessionTable)
            {
                foreach (TSession session in _sessionTable.Values)
                {
                    lock (session)
                    {
                        if (session.State == TSessionState.Normal)
                        {
                            session.DisconnectType = TDisconnectType.Normal;

                            // 做标记，在另外的进程中关闭
                            session.State = TSessionState.NoReply;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 直接关闭一个客户端会话
        /// </summary>
        /// <param name="sessionIP"></param>
        public bool CloseOneSession(string sessionIP)
        {
            bool closeSuccess = false;

            lock (_sessionTable)
            {
                if (_sessionTable.ContainsKey(sessionIP))  // 包含该会话 ID
                {
                    closeSuccess = true;

                    TSession session = (TSession)_sessionTable[sessionIP];

                    lock (session)
                    {
                        if (session.State == TSessionState.Normal)
                        {
                            session.DisconnectType = TDisconnectType.Normal;

                            // 做标记，在 CheckClientState 中关闭
                            session.State = TSessionState.NoReply;
                        }
                    }
                }
            }
            return closeSuccess;
        }

        #endregion

        #region 发送数据和文件传输
        /// <summary>
        ///  对 ID 号的 session 发送包信息
        /// </summary>
        /// <param name="sessionIP"></param>
        public bool SendData(string sessionIP, byte[] data)
        {
            bool sendSuccess = false;

            TSession session = null;
            lock (_sessionTable)
            {
                session = (TSession)_sessionTable[sessionIP];
            }

            if (session != null && session.State == TSessionState.Normal)
            {
                lock (session)
                {
                    try
                    {
                        int m = DateTime.Now.Minute;
                        int s = DateTime.Now.Second;
                        //byte[] data = Encoding.ASCII.GetBytes(datagram);  // 获得数据字节数组
                        byte[] _data = new byte[data.Length + 6];
                        for (int i = 3; i < data.Length + 3; i++)
                        {
                            _data[i] = data[i - 3];
                        }
                        _data[0] = Convert.ToByte('[');
                        _data[1] = Convert.ToByte(m);
                        _data[2] = TDatagram.BeginChar;
                        _data[data.Length + 3] = TDatagram.EndChar;
                        _data[data.Length + 4] = Convert.ToByte(s);
                        _data[data.Length + 5] = Convert.ToByte(']');
                        session.ClientSocket.BeginSend(_data, 0, _data.Length, SocketFlags.None, EndSendData, session);
                        sendSuccess = true;
                    }
                    catch
                    {
                        session.DisconnectType = TDisconnectType.Exception;

                        // 写 socket 发生错误，则准备关闭该会话，系统不认为是错误
                        session.State = TSessionState.NoReply;

                        this.OnClientException();
                    }
                }
            }
            return sendSuccess;
        }
        /// <summary>
        ///  对 ID 号的 session 发送包信息
        /// </summary>
        /// <param name="sessionID"></param>
        public bool SendBroadcastData(byte[] data)
        {
            bool sendSuccess = false;
            lock (_sessionTable)
            {
                foreach (TSession session in _sessionTable)
                {
                    if (session != null && session.State == TSessionState.Normal)
                    {
                        lock (session)
                        {
                            try
                            {
                                int m = DateTime.Now.Minute;
                                int s = DateTime.Now.Second;
                                //byte[] data = Encoding.ASCII.GetBytes(datagram);  // 获得数据字节数组
                                byte[] _data = new byte[data.Length + 6];
                                for (int i = 3; i < data.Length + 3; i++)
                                {
                                    _data[i] = data[i - 3];
                                }
                                _data[0] = Convert.ToByte('[');
                                _data[1] = Convert.ToByte(m);
                                _data[2] = TDatagram.BeginChar;
                                _data[data.Length + 3] = TDatagram.EndChar;
                                _data[data.Length + 4] = Convert.ToByte(s);
                                _data[data.Length + 5] = Convert.ToByte(']');
                                session.ClientSocket.BeginSend(_data, 0, _data.Length, SocketFlags.None, EndSendData, session);
                                sendSuccess = true;
                            }
                            catch
                            {
                                session.DisconnectType = TDisconnectType.Exception;

                                // 写 socket 发生错误，则准备关闭该会话，系统不认为是错误
                                session.State = TSessionState.NoReply;

                                this.OnClientException();
                            }
                        }
                    }
                }
            }
            return sendSuccess;
        }
        /// <summary>
        ///  对 ID 号的 session 接收文件
        /// </summary>
        /// <param name="sessionIP"></param>
        public bool ReceiveFile(string sessionIP, string _sendFilePath, string _receivePath, long packetSize, int packetCount, int lastDataPacket)
        {
            bool sendSuccess = false;

            TSession session = null;
            lock (_sessionTable)
            {
                session = (TSession)_sessionTable[sessionIP];
            }

            if (session != null && session.State == TSessionState.Normal)
            {
                lock (session)
                {
                    try
                    {
                        //DownUploadFile _duf = new DownUploadFile( _sendFilePath, _receivePath, packetSize, packetCount, lastDataPacket);
                        //_duf.TransferSocket = session.ClientSocket;

                        //Thread _revThead = new Thread(_duf.ReceiveFile);
                        //_revThead.Start();
                        //Thread.Sleep(100);

                        sendSuccess = true;
                    }
                    catch
                    {
                        session.DisconnectType = TDisconnectType.Exception;
                        // 写 socket 发生错误，则准备关闭该会话，系统不认为是错误
                        session.State = TSessionState.NoReply;

                        this.OnClientException();
                    }
                }
            }
            return sendSuccess;
        }
        /// <summary>
        ///  对 ID 号的 session 发送文件
        /// </summary>
        /// <param name="sessionIP"></param>
        public bool SendFile(string sessionIP, string _sendFilePath, string _receivePath, long packetSize, int packetCount, int lastDataPacket)
        {
            bool sendSuccess = false;

            TSession session = null;
            lock (_sessionTable)
            {
                session = (TSession)_sessionTable[sessionIP];
            }

            if (session != null && session.State == TSessionState.Normal)
            {
                //lock (session)
                //{
                try
                {
                    //DownUploadFile _duf = new DownUploadFile( _sendFilePath, _receivePath, packetSize, packetCount, lastDataPacket);
                    //_duf.TransferSocket = session.ClientSocket;

                    //Thread _sendThead = new Thread(_duf.SendFile);
                    //_sendThead.Start();
                    //Thread.Sleep(100);

                    sendSuccess = true;
                }
                catch
                {
                    session.DisconnectType = TDisconnectType.Exception;

                    // 写 socket 发生错误，则准备关闭该会话，系统不认为是错误
                    session.State = TSessionState.NoReply;

                    this.OnClientException();
                }
                //}
            }
            return sendSuccess;
        }
        #endregion

        #region 保护方法(事件方法)

        protected void OnReceiverException()
        {
            if (ReceiverException != null)
            {
                //抛出接收异常事件
                ReceiverException(this, new EventArgs());
            }
        }

        protected void OnReceiverWork()
        {
            if (ReceiverWork != null)
            {
                ReceiverWork(this, new EventArgs());
            }
        }

        protected void OnClientException()
        {
            if (ClientException != null)
            {
                ClientException(this, new EventArgs());
            }
        }

        protected void OnClientRequest()
        {
            if (ClientRequest != null)
            {
                //抛出客户端在线请求事件
                ClientRequest(this, new EventArgs());
            }
        }

        protected void OnDatagramError()
        {
            if (DatagramError != null)
            {
                DatagramError(this, new EventArgs());
            }
        }

        protected void OnDatagramHandle()
        {
            if (DatagramHandle != null)
            {
                DatagramHandle(this, new EventArgs());
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建接收服务器的 Socket, 并侦听客户端连接请求
        /// </summary>
        private bool CreateReceiverSocket()
        {
            try
            {
                _receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _receiverSocket.Bind(new IPEndPoint(IPAddress.Any, _tcpSocketPort));  // 绑定端口
                _receiverSocket.Listen(_maxAllowListenQueueLength);  // 开始监听

                return true;
            }
            catch
            {
                this.OnReceiverException();
                return false;
            }
        }

        /// <summary>
        /// 清理数据库资源
        /// </summary>
        private bool CloseDatabase()
        {
            try
            {
                if (_sqlConnection != null)
                {
                    _sqlConnection.Close();
                }
                return true;
            }
            catch
            {
                this.OnReceiverException();
                return false;
            }
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        private bool ConnectDatabase()
        {
            bool connectSuccess = false;
            _sqlConnection = new SqlConnection();

            try
            {
                _sqlConnection.ConnectionString = _dbConnectionStr;
                _sqlConnection.Open();

                connectSuccess = true;
            }
            catch
            {
                this.OnReceiverException();
            }
            finally
            {
                if (!connectSuccess)
                {
                    this.CloseDatabase();
                }
            }
            return connectSuccess;
        }


        /// <summary>
        /// 判断重复IP地址
        /// </summary>
        private bool CheckSameClientIP(Socket clientSocket)  // 
        {
            IPEndPoint iep = (IPEndPoint)clientSocket.RemoteEndPoint;
            string ip = iep.Address.ToString();

            if (ip.Substring(0, 7) == "127.0.0")
            {
                return false;  //本机器测试特别设定（本机可以多个client套接字）
            }

            lock (_sessionTable)
            {
                foreach (TSession session in _sessionTable.Values)
                {
                    if (session.IP == ip)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 循环侦听客户端请求，由于要用线程池，故带一个参数
        /// </summary>
        private void ListenClientRequest(object state)
        {
            Socket client = null;
            while (!_stopReceiver)
            {
                //已经停止客户端连接请求
                if (_stopConnectRequest)
                {
                    //保证 _receiverSocket == null
                    if (_receiverSocket != null)
                    {
                        try
                        {
                            // 强制关闭接收器
                            _receiverSocket.Close();
                        }
                        catch
                        {
                            this.OnReceiverException();
                        }
                        finally
                        {
                            // 必须为 null，否则 disposed 对象仍然存在，将引发下面的错误
                            _receiverSocket = null;
                        }
                    }
                    continue;
                }
                //尚未停止客户端连接请求，保证_receiverSocket != null;
                else
                {
                    //假如_receiverSocket == null，再次创建_receiverSocket
                    if (_receiverSocket == null)
                    {
                        if (!this.CreateReceiverSocket())
                        {
                            //创建失败
                            continue;
                        }
                    }
                }

                try
                {
                    if (_receiverSocket.Poll(_loopWaitTime, SelectMode.SelectRead))
                    {
                        // 频繁关闭、启动时，这里容易产生错误（提示套接字只能有一个）
                        client = _receiverSocket.Accept();

                        string cip = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
                        foreach (string ips in BlackIPS)
                        {
                            if (ips == cip)
                            {
                                cip = "black";
                                break;
                            }
                        }
                        if (client != null && client.Connected)
                        {
                            //如果超出最大连接数，则关闭该socket
                            if (this._clientCount >= this._maxAllowClientCount || cip.Equals("black"))
                            {
                                this.OnReceiverException();

                                try
                                {
                                    client.Shutdown(SocketShutdown.Both);
                                    client.Close();
                                }
                                catch { }
                            }
                            // 如果已存在该IP地址，则关闭该socket（每个客户端只保留一个套接字）
                            else if (CheckSameClientIP(client))
                            {
                                try
                                {
                                    client.Shutdown(SocketShutdown.Both);
                                    client.Close();
                                }
                                catch { }
                            }
                            // 如果符合要求的新套接字，则保存该会话
                            else
                            {
                                TSession session = new TSession(client);
                                session.LoginTime = DateTime.Now;

                                lock (_sessionTable)
                                {
                                    string preSessionID = session.IP;

                                    // 有可能重复该编号
                                    if (_sessionTable.ContainsKey(session.IP))
                                    {
                                        continue;
                                    }
                                    // 登记该会话客户端
                                    _sessionTable.Add(session.IP, session);
                                    Interlocked.Increment(ref _clientCount);
                                    //if (Program.ListNetIP != null && Program.ListNetIP[session.IP] != null)
                                    //{
                                    MessageType.AddMessageQueue(ref MessageQueue, session.IP, MessageType.PUBLIC_IP, session.IP, Program.NetIP);
                                    //}
                                    //else
                                    //{
                                    //    MessageType.AddMessageQueue(ref MessageQueue, session.IP, MessageType.PUBLIC_IP, session.IP, Program.IP);
                                    //}
                                }

                                this.OnClientRequest();

                                // 客户端连续连接或连接后立即断开，易在该处产生错误，系统忽略之
                                try
                                {
                                    //开始接受来自该客户端的数据
                                    //继续循环接收客户端连接,（递归调用委托方法 EndReceiveData，直到异常断开，客户端心跳保持连接）
                                    session.ClientSocket.BeginReceive(session.ReceiveBuffer, 0,
                                        session.ReceiveBufferLength, SocketFlags.None, EndReceiveData, session);
                                }
                                catch
                                {
                                    session.DisconnectType = TDisconnectType.Exception;
                                    session.State = TSessionState.NoReply;
                                }
                            }
                        }
                        else if (client != null)  // 非空，但没有连接（connected is false），关闭该socket
                        {
                            try
                            {
                                client.Shutdown(SocketShutdown.Both);
                                client.Close();
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    this.OnReceiverException();

                    if (client != null)
                    {
                        try
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        catch { }
                    }
                }
                // 该处可以适当暂停若干毫秒
                Thread.Sleep(50);
            }
            // 该处可以适当暂停若干毫秒
        }

        private void EndSendData(IAsyncResult iar)  //  发送数据完成处理函数, iar 为目标客户端 Session
        {
            TSession session = (TSession)iar.AsyncState;
            lock (_sessionTable)
            {
                // 再次判断是否在表中，Shutdown 时，可能激发本过程
                session = (TSession)_sessionTable[session.IP];
            }

            if (session != null && session.State == TSessionState.Normal)
            {
                lock (session)
                {
                    try
                    {
                        Socket client = session.ClientSocket;
                        int sent = client.EndSend(iar);
                    }
                    catch
                    {
                        session.DisconnectType = TDisconnectType.Exception;

                        // 写 Socket 发生错误，则准备关闭该会话，系统不认为是错误
                        session.State = TSessionState.NoReply;
                        this.OnClientException();
                    }
                }
            }
        }

        private void EndReceiveData(IAsyncResult iar)  // iar 目标客户端 Session
        {
            TSession session = (TSession)iar.AsyncState;
            lock (_sessionTable)
            {
                // 再次判断是否在表中，Shutdown 时，可能激发本过程
                session = (TSession)_sessionTable[session.IP];
            }

            if (session == null || session.State != TSessionState.Normal) return;

            lock (session)
            {
                try
                {
                    Socket client = session.ClientSocket;

                    // 注意：Shutdown 时将调用 ReceiveData，此时也可能收到 0 长数据包
                    int recv = client.EndReceive(iar);
                    if (recv == 0)
                    {
                        session.DisconnectType = TDisconnectType.Normal;
                        session.State = TSessionState.NoReply;
                    }
                    else  // 正常数据包
                    {
                        session.LastDataReceivedTime = DateTime.Now;

                        // 合并报文，按报文头、尾字符标志抽取报文，将包交给数据处理器
                        ResolveBuffer(session, recv);

                        // 继续接收来自来客户端的数据（异步调用）
                        session.ClientSocket.BeginReceive(session.ReceiveBuffer, 0,
                            session.ReceiveBufferLength, SocketFlags.None, EndReceiveData, session);
                    }
                }
                catch  // 读 socket 发生异常，则准备关闭该会话，系统不认为是错误（这种错误可能太多）
                {
                    session.DisconnectType = TDisconnectType.Exception;
                    session.State = TSessionState.NoReply;
                }
            }
        }

        /// <summary>
        /// 检查客户端状态（扫描方式，若长时间无数据，则断开）
        /// </summary>
        private void CheckClientState(object state)
        {
            while (!_stopReceiver)
            {
                DateTime thisTime = DateTime.Now;

                // 建立一个副本 ，然后对副本进行操作
                Hashtable sessionTable2 = new Hashtable();
                lock (_sessionTable)
                {
                    foreach (TSession session in _sessionTable.Values)
                    {
                        if (session != null)
                        {
                            sessionTable2.Add(session.IP, session);
                        }
                    }
                }

                foreach (TSession session in sessionTable2.Values)  // 对副本进行操作
                {
                    Monitor.Enter(session);
                    try
                    {
                        if (session.State == TSessionState.NoReply)  // 分三步清除一个 Session
                        {
                            session.State = TSessionState.Closing;
                            if (session.ClientSocket != null)
                            {
                                try
                                {
                                    // 第一步：shutdown
                                    session.ClientSocket.Shutdown(SocketShutdown.Both);
                                }
                                catch { }
                            }
                        }
                        else if (session.State == TSessionState.Closing)
                        {
                            session.State = TSessionState.Closed;
                            if (session.ClientSocket != null)
                            {
                                try
                                {
                                    // 第二步： Close
                                    session.ClientSocket.Close();
                                }
                                catch { }
                            }
                        }
                        else if (session.State == TSessionState.Closed)
                        {

                            lock (_sessionTable)
                            {
                                // 第三步：remove from table
                                _sessionTable.Remove(session.IP);
                                Interlocked.Decrement(ref _clientCount);
                            }

                            this.OnClientRequest();
                            session.Clear();  // 清空缓冲区
                        }
                        else if (session.State == TSessionState.Normal)  // 正常的会话 
                        {
                            TimeSpan ts = thisTime.Subtract(session.LastDataReceivedTime);
                            if (Math.Abs(ts.TotalSeconds) > _maxSocketDataTimeout)  // 超时，则准备断开连接
                            {
                                session.DisconnectType = TDisconnectType.Timeout;
                                session.State = TSessionState.NoReply;  // 标记为将关闭、准备断开
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(session);
                    }
                }  // end foreach

                sessionTable2.Clear();
                Thread.Sleep(100);
            }  // end while
        }

        /// <summary>
        /// 1) 报文界限字符为<>，其它为合法字符, 
        /// 2) 按报文头、界限标志抽取报文，可能合并包文
        /// 3) 如果一次收完数据，此时 DatagramBuffer 为空
        /// 4) 否则转存到包文缓冲区 session.DatagramBuffer
        /// </summary>
        private void ResolveBuffer(TSession session, int receivedSize)
        {
            // 上次留下的报文缓冲区非空（注意：必然含有开始字符 <，空时不含 <）
            bool hasBeginChar = (session.DatagramBufferLength > 0);

            int packPos = 0;  // ReceiveBuffer 缓冲区中包的开始位置
            int packLen = 0;  // 已经解析的接收缓冲区大小

            byte dataByte = 0;  // 缓冲区字节
            int subIndex = 0;   // 缓冲区下标

            while (subIndex < receivedSize)
            {
                // 接收缓冲区数据，要与报文缓冲区 session.DatagramBuffer 同时考虑
                dataByte = session.ReceiveBuffer[subIndex];

                if (dataByte == TDatagram.BeginChar) // 是数据包的开始字符<，则前面的包文均要放弃
                {
                    // <前面有非空串（包括报文缓冲区），则前面是错包文，防止 AAA<A,1,A> 两个报文一次读现象
                    if (packLen > 0)
                    {
                        Interlocked.Increment(ref _datagramCount);       // 前面有非空字符
                        Interlocked.Increment(ref _errorDatagramCount);  // 一个错误包
                        this.OnDatagramError();
                    }
                    session.ClearDatagramBuffer();  // 清空会话缓冲区，开始一个新包

                    packPos = subIndex + 1;   // 新包起点，即<所在位置
                    packLen = 0;          // 新包的长度（即<）
                    hasBeginChar = true;  // 新包有开始字符
                }
                else if (dataByte == TDatagram.EndChar)  // 数据包的结束字符 >
                {
                    if (hasBeginChar)  // 两个缓冲区中有开始字符<
                    {
                        ++packLen;  // 长度包括结束字符>

                        // >前面的为正确格式的包，则分析该包，并准备加入包队列
                        AnalyzeOneDatagram(session, packPos, packLen - 1);

                        packPos = subIndex + 1;  // 新包起点。注意：subIndex 在循环最后处 + 1
                        packLen = 0;             // 新包长度
                    }
                    else  // >前面没有开始字符，则认为结束字符>为一般字符，待后续的错误包处理
                    {
                        ++packLen;  //   hasBeginChar = false;
                    }
                }
                else  // 非界限字符<>，就是是一般字符，长度 + 1，待解析包处理
                {
                    ++packLen;
                }
                ++subIndex;  // 增加下标号
            }  // end while

            if (packLen > 0)  // 剩下的待处理串，分两种情况
            {
                // 剩下包文，已经包含首字符且不超长，转存到包文缓冲区中，待下次处理
                if (hasBeginChar && packLen + session.DatagramBufferLength <= _maxDatagramSize)
                {
                    session.CopyToDatagramBuffer(packPos, packLen);
                }
                else  // 不含首字符，或超长
                {
                    Interlocked.Increment(ref _datagramCount);
                    Interlocked.Increment(ref _errorDatagramCount);

                    this.OnDatagramError();
                    session.ClearDatagramBuffer();  // 丢弃全部数据
                }
            }
        }

        /// <summary>
        /// 具有<>格式的数据包加入到队列中
        /// </summary>
        private void AnalyzeOneDatagram(TSession session, int packPos, int packLen)
        {
            if (packLen + session.DatagramBufferLength > _maxDatagramSize)  // 超过长度限制
            {
                Interlocked.Increment(ref _datagramCount);
                Interlocked.Increment(ref _errorDatagramCount);
                this.OnDatagramError();
            }
            else // 一个首尾字符相符的包，此时需要判断其类型
            {
                Interlocked.Increment(ref _datagramCount);
                byte[] _datagram = new byte[packLen];
                for (int i = 0; i < packLen; i++)
                {
                    _datagram[i] = session.ReceiveBuffer[i + packPos];
                }

                TDatagram datagram = new TDatagram(_datagram);
                datagram.IP = session.IP;

                if (!datagram.CheckDatagramKind())  // 包格式错误（只能是短期BG、或长期SG包）
                {
                    Interlocked.Increment(ref _datagramCount);
                    Interlocked.Increment(ref _errorDatagramCount);
                    this.OnDatagramError();
                    datagram = null;  // 丢弃当前包
                }
                else  // 实时包、定期包，先解析数据，判断正误，并发回确认包
                {
                    if (datagram.ResolveDatagram())  // 正确的包才入包队列
                    {
                        Interlocked.Increment(ref _datagramQueueRequestCount);
                        lock (_datagramQueueRequest)
                        {
                            _datagramQueueRequest.Enqueue(datagram);  // 数据包入队列
                        }
                    }
                    else
                    {
                        Interlocked.Increment(ref _errorDatagramCount);
                        this.OnDatagramError();
                    }
                }
            }
            session.ClearDatagramBuffer();  // 清包文缓冲区
        }

        /// <summary>
        /// 处理数据包队列，由于要用线程池，故带一个参数
        /// </summary>
        private void HandleDatagrams(object state)
        {
            while (!_stopReceiver)
            {
                this.HandleOneDatagram();  // 处理一个数据包

                if (!_stopReceiver)
                {
                    //// 如果连接关闭，则重新建立，可容许几个连接错误出现
                    //if (_sqlConnection.State == ConnectionState.Closed)
                    //{
                    //    this.OnReceiverWork();
                    //    try
                    //    {
                    //        _sqlConnection.Open();
                    //    }
                    //    catch
                    //    {
                    //        this.OnReceiverException();
                    //    }
                    //}
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 处理一个包数据，包括：验证、存储
        /// 分解加入命令行队列
        /// </summary>
        private void HandleOneDatagram()
        {
            TDatagram datagram = null;

            lock (_datagramQueueRequest)
            {
                if (_datagramQueueRequest.Count > 0)
                {
                    datagram = _datagramQueueRequest.Dequeue();  // 取队列数据
                    Interlocked.Decrement(ref _datagramQueueRequestCount);

                    if (!datagram.CheckDatagramKind())  // 包格式错误（只能是短期BG、或长期SG包）
                    {
                        Interlocked.Increment(ref _datagramCount);
                        Interlocked.Increment(ref _errorDatagramCount);
                        this.OnDatagramError();
                        datagram = null;  // 丢弃当前包
                    }
                    else  // 实时包、定期包，先解析数据，判断正误，并发回确认包
                    {
                        if (datagram.Datagram != null && datagram.ResolveDatagram())  // 正确的包才入包队列
                        {
                            if (datagram.Datagram.IndexOf("Token:") >= 0)
                            {
                                string message = LogFile.Connect + "|[" + DateTime.Now.ToLongTimeString() + "] Request From <" + datagram.IP + ">";
                                MessageType.AddMessageQueue(ref MessageQueue, message, MessageType.OUTPUT_LOG, Program.IP);
                                string token = datagram.Datagram.Replace("Token: ", "");
                                if (token != Program.Token)
                                {
                                    if (Program.ListNetIP != null && Program.ListNetIP[datagram.IP] != null)
                                    {
                                        message = LogFile.Connect + "|[" + DateTime.Now.ToLongTimeString() + "] Authentication failure! <" + Program.NetIP + "> " + datagram.Datagram;
                                    }
                                    else
                                    {
                                        message = LogFile.Connect + "|[" + DateTime.Now.ToLongTimeString() + "] Authentication failure! <" + Program.IP + "> " + datagram.Datagram;
                                    }
                                    MessageType.AddMessageQueue(ref MessageQueue, message, MessageType.OUTPUT_MESSAGE, datagram.IP);
                                    message = LogFile.Connect + "|[" + DateTime.Now.ToLongTimeString() + "] Authentication failure! <" + datagram.IP + "> " + datagram.Datagram;
                                    MessageType.AddMessageQueue(ref MessageQueue, message, MessageType.OUTPUT_COMMAND, Program.IP);
                                    this.BlackIPS.Add(datagram.IP);
                                    CloseOneSession(datagram.IP);
                                    return;
                                }
                                TDatagram _datagram = new TDatagram("Token: Heartbeat Success.");
                                _datagram.IP = datagram.IP;
                                lock (_datagramQueueResponse)
                                {
                                    _datagramQueueResponse.Enqueue(_datagram);  // 数据包入队列
                                }
                            }
                            if (datagram.Datagram.IndexOf("MessagePackage|") >= 0)
                            {
                                string temp = datagram.Datagram.Replace("MessagePackage|", "");
                                MessagePackage mp = new MessagePackage();
                                mp = SerializeObj.Desrialize<MessagePackage>(mp, temp);
                                if (MessageQueue != null)  // 清空队列
                                {
                                    lock (MessageQueue)
                                    {
                                        MessageQueue.Enqueue(mp);
                                    }
                                }
                            }
                            if (datagram.Datagram.IndexOf("SendFile") >= 0)
                            {
                                string[] tasks = datagram.Datagram.Split('|');
                                if (tasks.Length > 3)
                                {

                                }
                            }

                        }
                        else
                        {
                            Interlocked.Increment(ref _errorDatagramCount);
                            this.OnDatagramError();
                        }
                    }
                }
            }

            if (datagram == null) return;

            //datagram.Clear();
            //datagram = null;  // 释放对象
        }

        /// <summary>
        /// 处理数据包队列，由于要用线程池，故带一个参数
        /// </summary>
        private void ResponseDatagrams(object state)
        {
            while (!_stopReceiver)
            {
                TDatagram datagram = null;

                lock (_datagramQueueResponse)
                {
                    if (_datagramQueueResponse.Count > 0)
                    {
                        datagram = _datagramQueueResponse.Dequeue();  // 取队列数据
                        if (datagram.Datagram.IndexOf("Token:") >= 0)
                        {
                            string message = LogFile.Connect + "|[" + DateTime.Now.ToLongTimeString() + "] Response To <" + datagram.IP + ">! " + datagram.Datagram;
                            MessageType.AddMessageQueue(ref MessageQueue, message, MessageType.OUTPUT_LOG, Program.IP);
                        }
                        SendData(datagram.IP, datagram.EncryptDatagram());
                        //SendData(datagram.ID, Encoding.Default.GetBytes(datagram.Datagram));
                    }
                }
                lock (_datagramQueueBroadcast)
                {
                    if (_datagramQueueBroadcast.Count > 0)
                    {
                        datagram = _datagramQueueBroadcast.Dequeue();  // 取队列数据
                        SendBroadcastData(Encoding.Default.GetBytes(datagram.Datagram));
                    }
                }
                Thread.Sleep(10);
            }
        }
        #endregion
    }

    #endregion
}
