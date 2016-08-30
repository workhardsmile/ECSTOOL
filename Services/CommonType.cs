using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LuaInterface;

namespace ECSTOOL
{
    public class MessageType
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public const string KEY = "WUGLXECS";
        /// <summary>
        /// 批量命令
        /// </summary>
        public const string StartKey = "{@";
        public const string EndKey = "@}";
        /// <summary>
        /// 插件处理命令(抛出事件）
        /// </summary>
        public const int SUB_COMMAND = 1111;
        /// <summary>
        /// 输出命令1
        /// </summary>
        public const int OUTPUT_COMMAND = 1112;
        /// <summary>
        /// 输出命令2
        /// </summary>
        public const int OUTPUT_MESSAGE = 1113;
        /// <summary>
        /// 输出命令4
        /// </summary>
        public const int OUTPUT_LOG = 1114;
        /// <summary>
        /// 输出命令4
        /// </summary>
        public const int COMMAND_ENGION = 1115;
        /// <summary>
        /// 返回外网IP
        /// </summary>
        public const int PUBLIC_IP = 1116;
        /// <summary>
        /// 发送邮件
        /// </summary>
        public const int SEND_MAIL = 1117;
        /// <summary>
        /// 添加定时任务
        /// </summary>
        public const int CRON_ADD = 1118;
        /// <summary>
        /// 删除定时任务
        /// </summary>
        public const int CRON_DEL = 1119;
        /// <summary>
        /// 查看文本文件内容
        /// </summary>
        public const int VIEW_FILE = 1120;
        /// <summary>
        /// 单线程运行CMD
        /// </summary>
        public const int RUN_CMD = 1121;
        /// <summary>
        /// 系统命令
        /// </summary>
        public const int EXEC_WIN_API = 1000;
        public const int EXEC_CMD = 1001;
        public const int EXEC_SHELL = 1002;
        public const int OPEN_EXE = 1003;
        public const int KILL_PROC = 1004;
        public const int OPEN_FILE = 1005;
        public const int OPEN_WEB = 1006;
        public const int OPEN_SCRIPT = 1007;
        public const int EXEC_SCRIPT = 1008;
        public const int STOP_SCRIPT = 1009;
        /// <summary>
        /// SERVER操作
        /// </summary>       
        public const int START_ALL_SERVER = 2000;
        public const int START_ONE_SERVER = 2001;
        public const int CLOSE_ALL_SERVER = 2002;
        public const int CLOSE_ONE_SERVER = 2003;
        public const int RESTART_ALL_SERVER = 2004;
        public const int RESTART_ONE_SERVER = 2005;
        public const int GET_STATUS_SERVER = 2006;
        public const int ADD_ONE_SERVER = 2007;
        public const int DELETE_ONE_SERVER = 2008;
        ///// <summary>
        ///// SSH操作
        ///// </summary>       
        //public const int START_ALL_SSH = 2000;
        //public const int START_ONE_SSH = 2001;
        //public const int CLOSE_ALL_SSH = 2002;
        //public const int CLOSE_ONE_SSH = 2003;
        //public const int RESTART_ALL_SSH = 2004;
        //public const int RESTART_ONE_SSH = 2005;
        //public const int GET_STATUS_SSH = 2006;
        //public const int ADD_ONE_LINUX = 2007;
        ///// <summary>
        ///// SOCKET操作
        ///// </summary>       
        //public const int START_ALL_SOCKET = 3000;
        //public const int START_ONE_SOCKET = 3001;
        //public const int CLOSE_ALL_SOCKET = 3002;
        //public const int CLOSE_ONE_SOCKET = 3003;
        //public const int RESTART_ALL_SOCKET = 3004;
        //public const int RESTART_ONE_SOCKET = 3005;
        //public const int GET_STATUS_SOCKET = 3006;
        //public const int ADD_ONE_WINDOWS = 3007;
        /// <summary>
        /// 工具命令
        /// </summary>
        public const int GET_SERVER_OSTYPE = 4000;
        public const int GET_ALL_SERVERS = 4001;
        public const int GET_ERROR_SERVERS = 4002;
        public const int GET_CONNECT_SERVERS = 4003;
        public const int GET_NOCONNECT_SERVERS = 4004;
        public const int CHECK_FILE = 4005;
        public const int CHECK_DIRECTORY = 4006;
        public const int C_GET_FILES = 4007;
        public const int S_GET_FILES = 4008;
        public const int C_GET_DIRS = 4009;
        public const int S_GET_DIRS = 4010;
        public const int SEND_FILE = 4011;
        public const int DOWNLOAD_FILE = 4012;
        public const int CLOSE_SEND_FILE = 4013;
        public const int CLOSE_DOWNLOAD_FILE = 4014;

        public const int N_SEND_FILE = 4015;
        public const int N_DOWNLOAD_FILE = 4016;


        public const int L_SEND_FILE = 4017;
        public const int L_DOWNLOAD_FILE = 4018;
        public const int L_CHECK_FILE = 4019;
        public const int L_CHECK_DIRECTORY = 4020;
        public const int GET_FILES = 4021;
        public const int GET_DIRS = 4022;
        public const int L_SEND_FILES = 4023;
        public const int L_DOWNLOAD_FILES = 4024;
        public const int REPLACE_RIGHT = 4025;
        public const int REPLACE_TEXT = 4026;
        public const int DOWNLOAD_FILES = 4027;
        /// <summary>
        /// 分解批量命令 
        /// </summary>
        public static string[] GetCommandArray(string commands)
        {
            return GetCommandArray(commands, StartKey, EndKey);
        }
        /// <summary>
        /// 分解批量命令 
        /// </summary>
        public static string[] GetCommandArray(string commands, string start, string end)
        {
            int packPos = 0;
            int packLen = 0;
            int subIndex = 0;   // 缓冲区下标
            bool hasBeginChar = false;
            ArrayList temp = new ArrayList();
            string strStart, strEnd;

            while (subIndex < commands.Length)
            {
                if (subIndex > commands.Length - start.Length || subIndex > commands.Length - end.Length)
                {
                    break;
                }
                strStart = commands.Substring(subIndex, start.Length);
                strEnd = commands.Substring(subIndex, end.Length);
                if (strStart == start)
                {
                    subIndex = packPos = subIndex + start.Length;
                    packLen = 0;
                    hasBeginChar = true;
                }
                else if (strEnd == end)
                {
                    if (hasBeginChar)
                    {
                        temp.Add(commands.Substring(packPos, packLen));
                        subIndex = packPos = subIndex + end.Length;
                        hasBeginChar = false;
                        packLen = 0;
                    }
                    else
                    {
                        ++subIndex;
                    }
                }
                else
                {
                    if (hasBeginChar)
                    {
                        ++packLen;
                    }
                    ++subIndex;
                }
            }
            string[] values;
            if (temp.Count == 0)
            {
                //values = new string[] { commands };
                values = null;
            }
            else
            {
                values = (string[])temp.ToArray(typeof(string));
            }

            return values;
        }
        [LuaFunc("MessageBoxShow")] 
        public static void MessageBoxShow(string content, string type)
        {
            ShowMsg sm = new ShowMsg();
            sm.content = content;
            sm.type = type;
            System.Threading.Thread temp = new System.Threading.Thread(sm.Show);
            temp.Start();
        }
        class ShowMsg
        {
            public string content;
            public string type;

            public void Show()
            {
                switch (type)
                {
                    case "error":
                        MessageBox.Show(content, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case "warning":
                        MessageBox.Show(content, "警告提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case "info":
                        MessageBox.Show(content, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "question":
                        MessageBox.Show(content, "问题提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                        break;
                    default:
                        MessageBox.Show(content, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
        }
        [LuaFunc("AddMessageQueueER")] 
        public static void AddMessageQueue(ref Queue<MessagePackage> MessageQueue, string MessageContent, int MessageType, string ExecIP, string ReturnIP)
        {
            MessagePackage mp = new MessagePackage();
            mp.MessageType = MessageType;
            mp.CommandContent = MessageContent;
            mp.ExecIP = ExecIP;
            mp.ReturnIP = ReturnIP;
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(mp);
            }
        }
        [LuaFunc("AddMessageQueueE")] 
        public static void AddMessageQueue(ref Queue<MessagePackage> MessageQueue, string MessageContent, int MessageType, string ExecIP)
        {
            MessagePackage mp = new MessagePackage();
            mp.MessageType = MessageType;
            mp.CommandContent = MessageContent;
            mp.ExecIP = ExecIP;
            mp.ReturnIP = ExecIP;
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(mp);
            }
        }
        [LuaFunc("GetMyResult")]  
        public static object[] GetMyResult(LuaTable luaTbl)
        {
            object[] _array;
            // pairs  
            //foreach (object oKey in luaTbl.Keys)  
            //{  
            //    if (oKey.ToString() == "n") // “参数个数”索引  
            //        continue;  
            //    Console.Write("{0}\t", luaTbl[oKey]);  
            //}
            int length = int.Parse(luaTbl["n"].ToString());
            _array = new object[length];
            for (int i = 1; i <= length; i++)
            {
                //Console.Write("{0}\t", luaTbl[i]); 
                _array[i - 1] = luaTbl[i];
            }
            return _array;
        }
    }
    public class ShellPkg
    {
        private string _IP;
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }
        private string _Command;
        public string Command
        {
            get { return _Command; }
            set { _Command = value; }
        }
        private string _dTime;
        public string DTime
        {
            get { return _dTime; }
            set { _dTime = value; }
        }
        private int _MessageType;
        public int MessageType
        {
            get { return _MessageType; }
            set { _MessageType = value; }
        }
    }
    [Serializable()]
    public class MessagePackage
    {
        private int messageType;
        public int MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        private string _ExecIP;
        public string ExecIP
        {
            get { return _ExecIP; }
            set { _ExecIP = value; }
        }
        private string _ReturnIP;
        public string ReturnIP
        {
            get { return _ReturnIP; }
            set { _ReturnIP = value; }
        }

        private string commandContent;
        public string CommandContent
        {
            get { return commandContent; }
            set { commandContent = value; }
        }
        public void Clear()
        {
            this.commandContent = null;
        }
    }
    [Serializable]
    public class SysIO
    {
        public ArrayList files = new ArrayList();
        public ArrayList dirs = new ArrayList();
        public bool GetFiles(string path, int depth)
        {
            bool flag = false;
            if (Directory.Exists(path) && depth > 0)
            {
                try
                {
                    DirectoryInfo dis = new DirectoryInfo(path);
                    FileInfo[] fis = dis.GetFiles();
                    foreach (FileInfo fi in fis)
                    {
                        files.Add(fi.FullName);
                    }
                    DirectoryInfo[] sub = dis.GetDirectories();
                    foreach (DirectoryInfo di in sub)
                    {
                        GetFiles(di.FullName, depth - 1);
                    }
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }
        public bool GetDirs(string path, int depth)
        {
            bool flag = false;
            if (Directory.Exists(path) && depth > 0)
            {
                try
                {
                    DirectoryInfo dis = new DirectoryInfo(path);
                    DirectoryInfo[] sub = dis.GetDirectories();
                    foreach (DirectoryInfo di in sub)
                    {
                        dirs.Add(di.FullName);
                        GetDirs(di.FullName, depth - 1);
                    }
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }
    }
    /// <summary>
    /// 接收数据包类（框架）
    /// </summary>
    public class TDatagram
    {
        public static byte BeginChar = Convert.ToByte('{');
        public static byte EndChar = Convert.ToByte('}');
        private EncryDecryUtil edu;

        private string _ip;                         // 会话 ID
        private string _datagram;       //加密之前解密之后的明文数据
        //会话ID，即 Socket 的句柄属性
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }
        public string Datagram
        {
            get { return _datagram; }
            set { _datagram = value; }
        }
        /// <summary>
        /// 接收报文
        /// </summary>
        /// <param name="datagramBuffer"></param>
        public TDatagram(byte[] datagramBuffer)
        {
            //this._datagram = System.Text.Encoding.Default.GetString(datagramBuffer);
            this.DecryptDatagram(datagramBuffer); //解密
        }
        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="datagram"></param>
        public TDatagram(string datagram)
        {
            this._datagram = datagram;
        }
        /// <summary>
        /// 清除包缓冲区
        /// </summary>
        public void Clear()
        {
            _datagram = null;
        }
        /// <summary>
        /// 发送加密，_datagram --> DatagramBuffer
        /// </summary>
        public byte[] EncryptDatagram()
        {
            edu = new EncryDecryUtil();
            byte[] _data;
            if (Program.IsEncry)
            {
                _data = Encoding.Default.GetBytes(edu.EncryptString(this._datagram, MessageType.KEY));
            }
            else
            {
                _data = Encoding.Default.GetBytes(this._datagram);
            }
            return _data;
        }
        /// <summary>
        /// 接收解密，DatagramBuffer --> _datagram
        /// </summary>
        private void DecryptDatagram(byte[] datagramBuffer)
        {
            try
            {
                edu = new EncryDecryUtil();
                string temp = Encoding.Default.GetString(datagramBuffer);
                if (Program.IsEncry)
                {
                    this.Datagram = edu.DecryptString(temp, MessageType.KEY);
                }
                else
                {
                    this.Datagram = temp;
                }
            
            }
            catch
            {
            }
        }
        /// <summary>
        /// 判断数据包类型，包括判错
        /// </summary>
        public bool CheckDatagramKind()
        {
            return true;
            //throw new System.NotImplementedException();
        }
        /// <summary>
        /// 解析数据包
        /// </summary>
        public bool ResolveDatagram()
        {
            return true;
        }
    }
    /// <summary>
    /// Socket会话类 Session
    /// </summary>
    public class TSession
    {
        // 接收数据缓冲区大小 （8车道、1个时段数据）
        private const int DefaultBufferSize = 16 * 1024;

        // 最大无数据接收时间（默认为 600 秒）
        private const int MaxNoDataReceivedTime = 600;

        private int _id;                         // 会话 ID
        private Socket _clientSocket;            // 客户端 Socket
        private string _ip = string.Empty;       // 客户端 IP 地址

        private TSessionState _state;             // 会话状态 
        private TDisconnectType _disconnectType;  // 客户端的退出类型

        private DateTime _loginTime;             // 绘画开始时间
        private DateTime _lastDataReceivedTime;  // 最近接收数据的时间

        public byte[] ReceiveBuffer;   // 数据接收缓冲区
        public byte[] DatagramBuffer;  // 数据包文缓冲区，防止接收空间不够

        public int ID  //会话ID，即 Socket 的句柄属性
        {
            get { return _id; }
            set { _id = value; }
        }

        public string IP  //会话客户端 IP
        {
            get { return _ip; }
        }

        public Socket ClientSocket  // 获得与客户端会话关联的Socket对象
        {
            get { return _clientSocket; }
        }

        public TDisconnectType DisconnectType  // 存取客户端的退出方式
        {
            get { return _disconnectType; }
            set { _disconnectType = value; }
        }

        public int ReceiveBufferLength  // 接收缓冲区长度
        {
            get { return ReceiveBuffer.Length; }
        }

        public int DatagramBufferLength  // 会话的数据包长度
        {
            get
            {
                if (DatagramBuffer == null)
                {
                    return 0;
                }
                else
                {
                    return DatagramBuffer.Length;
                }
            }
        }

        public TSessionState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public DateTime LastDataReceivedTime
        {
            set { _lastDataReceivedTime = value; }
            get { return _lastDataReceivedTime; }
        }

        public DateTime LoginTime
        {
            set { _loginTime = value; }
            get { return _loginTime; }
        }

        public TSession(Socket _cliSocket)  // _cliSocket会话使用的Socket连接
        {
            this._clientSocket = _cliSocket;
            this._id = (int)this._clientSocket.Handle;

            IPEndPoint iep = (IPEndPoint)_cliSocket.RemoteEndPoint;
            _ip = iep.Address.ToString();

            ReceiveBuffer = new byte[DefaultBufferSize];  // 数据接收缓冲区
            DatagramBuffer = null;  // 数据包存储区

            _lastDataReceivedTime = DateTime.Now;  // 会话开始时间
            _state = TSessionState.Normal;
        }

        public void Clear()  //  清空缓冲区
        {
            ReceiveBuffer = null;
            DatagramBuffer = null;
        }

        public void ClearDatagramBuffer()  // 清除包文缓冲区
        {
            if (DatagramBuffer != null) DatagramBuffer = null;
        }

        /// <summary>
        /// 拷贝接收缓冲区的数据到数据缓冲区（即多次读一个包文）
        /// </summary>
        public void CopyToDatagramBuffer(int startPos, int packLen)
        {
            int datagramLen = 0;
            if (DatagramBuffer != null) datagramLen = DatagramBuffer.Length;

            // 调整长度（DataBuffer 为 null 不会出错）
            Array.Resize(ref DatagramBuffer, datagramLen + packLen);

            // 拷贝到数据就缓冲区
            Array.Copy(ReceiveBuffer, startPos, DatagramBuffer, datagramLen, packLen);
        }

    }
    /// <summary>
    /// 连接断开类型枚举 DisconectType
    /// </summary>
    public enum TDisconnectType
    {
        Normal,     // 正常断开
        Timeout,    // 超时断开
        Exception   // 异常断开
    }
    /// <summary>
    /// 通信会话状态枚举 SessionState
    /// </summary>
    public enum TSessionState
    {
        Normal,   // 正常
        NoReply,  // 无应答, 即将关闭
        Closing,  // 正在关闭
        Closed    // 已经关闭
    }
    /// <summary>
    /// 序列化和反序列化
    /// </summary>
    public class SerializeObj
    {
        public SerializeObj()
        { }

        /// <summary>
        /// 序列化 对象到字符串
        /// </summary>
        /// <param name="obj">泛型对象</param>
        /// <returns>序列化后的字符串</returns>
        public static string Serialize<T>(T obj)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                return Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception("序列化失败,原因:" + ex.Message);
            }
        }

        /// <summary>
        /// 反序列化 字符串到对象
        /// </summary>
        /// <param name="obj">泛型对象</param>
        /// <param name="str">要转换为对象的字符串</param>
        /// <returns>反序列化出来的对象</returns>
        public static T Desrialize<T>(T obj, string str)
        {
            try
            {
                obj = default(T);
                IFormatter formatter = new BinaryFormatter();
                byte[] buffer = Convert.FromBase64String(str);
                MemoryStream stream = new MemoryStream(buffer);
                obj = (T)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("反序列化失败,原因:" + ex.Message);
            }
            return obj;
        }
    }
}
