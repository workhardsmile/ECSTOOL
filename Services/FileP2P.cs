using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ECSTOOL
{
    public class FileSendP2P
    {
        public string IP;
        private int Port;
        public string FullFileName;
        public int PkgSize=50000;
        public int DirCount;
        public static bool IsFinished = true;
        public string error;

        public FileSendP2P(string _ip, int _port, string _fullFileName,int _pkgSize,int _dirCount)
        {
            this.IP = _ip;
            this.Port = _port;
            this.FullFileName = _fullFileName;
            this.PkgSize = _pkgSize;
            this.DirCount = _dirCount;
            IsFinished = false;
        }
        public void StartSend()
        {
            IsFinished = false;
            try
            {
                //创建一个文件对象   
                FileInfo EzoneFile = new FileInfo(FullFileName);
                //打开文件流   
                FileStream EzoneStream = EzoneFile.OpenRead();
                //包的大小   
                int PacketSize = PkgSize;
                //包的数量   
                int PacketCount = (int)(EzoneStream.Length / ((long)PacketSize));
                //this.textBox8.Text = PacketCount.ToString();
                //this.progressBar1.Maximum = PacketCount;
                //最后一个包的大小   
                int LastDataPacket = (int)(EzoneStream.Length - ((long)(PacketSize * PacketCount)));
                //this.textBox9.Text = LastDataPacket.ToString();
                long StreamLength = EzoneStream.Length;

                ////创建一个网络端点   
                //IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.1.136"), int.Parse(this.textBox5.Text));   

                ////MessageBox.Show(IPAddress.Any);   

                ////创建一个套接字   
                //Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   

                //MessageBox.Show(server.ToString());


                //指向远程服务端节点   
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
                //创建套接字   
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //连接到发送端   
                client.Connect(ipep);


                ////绑定套接字到端口   
                //client.Bind(ipep);   

                //MessageBox.Show(ipep.ToString());   

                ////开始侦听(并堵塞该线程)   
                //server.Listen(10);   
                //确认连接   
                //Socket client = server.Accept();   

                //MessageBox.Show(client.ToString());   


                //获得客户端节点对象   
                IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
                //获得客户端的IP地址   
                //this.textBox7.Text=clientep.Address.ToString();   
                //发送[文件名]到客户端   
                TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(EzoneFile.Name));
                //发送[包的大小]到客户端   
                TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(PacketSize.ToString()));
                //发送[包的总数量]到客户端   
                TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(PacketCount.ToString()));
                //发送[最后一个包的大小]到客户端   
                TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(LastDataPacket.ToString()));
                //发送[最后一个包的大小]到客户端   
                TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(StreamLength.ToString()));

                //数据包
                while (this.DirCount > 0)
                {
                    byte[] data = new byte[PacketSize];
                    //开始循环发送数据包   
                    for (int i = 0; i < PacketCount; i++)
                    {
                        //从文件流读取数据并填充数据包   
                        EzoneStream.Read(data, 0, data.Length);
                        //发送数据包   
                        TransferFiles.SendVarData(client, data);
                        //显示发送数据包的个数   
                        //this.textBox10.Text = ((int)(i + 1)).ToString();
                        //进度条值的显示   
                        // this.progressBar1.PerformStep();
                    }
                    //如果还有多余的数据包,则应该发送完毕!   
                    if (LastDataPacket != 0)
                    {
                        data = new byte[LastDataPacket];
                        EzoneStream.Read(data, 0, data.Length);
                        TransferFiles.SendVarData(client, data);
                        //this.progressBar1.Value = this.progressBar1.Maximum;
                    }
                    EzoneStream.Close();
                    this.DirCount--;
                    Thread.Sleep(1000);
                    EzoneStream = EzoneFile.OpenRead();
                    TransferFiles.SendVarData(client, System.Text.Encoding.Unicode.GetBytes(""));
                }

                //关闭套接字   
                client.Close();
                //关闭文件流   
                EzoneStream.Close();
            }
            catch(Exception ex)
            {
                this.error = ex.Message;
             }
            IsFinished = true;
        }
        //public static bool WaiteResponse(Socket client, string str, int max)
        //{
        //    int count = 0;
        //    while (count < max)
        //    {
        //        string result = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));
        //        if (result != null && result.Equals(str))
        //        {
        //            count = 0;
        //            break;
        //        }
        //        count++;
        //    }
        //    if (count != 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
    }

    public class FileReceiveP2P
    {
        Socket ClientSocket;
        private int Port = 18889;
        private string[] DirPath;
        public static bool IsFinished = true;
        public bool IsStop = false;
        public string ClientIP="none";
        public string error;

        public FileReceiveP2P(int _socketPort, string[] _dirPath)
        {
            this.Port = _socketPort;
            this.DirPath = _dirPath;
            IsFinished = false;
        }

        #region 功能函数
        public void StartReceive()
        {
            //创建一个网络端点   
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, this.Port);

            //MessageBox.Show(IPAddress.Any);   

            //创建一个套接字   
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定套接字到端口   
            server.Bind(ipep);

            //开始侦听(并堵塞该线程)   
            server.Listen(10);

            while (!IsStop)
            {
                try
                {
                    Socket client = server.Accept();
                    ClientSocket = client;
                    this.ClientIP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
                    Thread TempThread = new Thread(new ThreadStart(this.Create));
                    TempThread.Start();
                }
                catch (Exception ex)
                {
                    int k = 0;
                }
            }
        }
        #endregion

        public void Create()
        {
            IsFinished = false;
            try
            {
                Socket client = ClientSocket;
                //确认连接   
                //Socket client = server.Accept();

                //获得客户端节点对象   
                IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;

                //获得[文件名]   
                string SendFileName = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));
                //MessageBox.Show("文件名" + SendFileName);   

                //获得[包的大小]   
                string bagSize = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));
                //MessageBox.Show("包大小" + bagSize);   

                //获得[包的总数量]   
                int bagCount = int.Parse(System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client)));
                //MessageBox.Show("包的总数量" + bagCount);   

                //获得[最后一个包的大小]   
                string bagLast = System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client));
                //MessageBox.Show("最后一个包的大小" + bagLast);
                long StreamLength = long.Parse(System.Text.Encoding.Unicode.GetString(TransferFiles.ReceiveVarData(client)));
                //创建一个新文件   
                FileStream MyFileStream;

                for (int j = 0; j < this.DirPath.Length; j++)
                {
                    //创建一个新文件   
                    MyFileStream = new FileStream(Path.Combine(this.DirPath[j], SendFileName), FileMode.Create, FileAccess.Write);

                    //已发送包的个数   
                    int SendedCount = 0;
                    while (true)
                    {
                        byte[] data = TransferFiles.ReceiveVarData(client);
                        if (data.Length == 0 || MyFileStream.Length >= StreamLength)
                        {
                            //关闭文件流   
                            MyFileStream.Close();
                            Thread.Sleep(100);
                            break;
                        }
                        else
                        {
                            SendedCount++;
                            //将接收到的数据包写入到文件流对象   
                            MyFileStream.Write(data, 0, data.Length);
                            //显示已发送包的个数     
                        }
                    }
                }
                //关闭套接字   
                client.Close();
            }
            catch (Exception ex)
            {
                this.error = ex.Message;
            }
            IsFinished = true;
        }
    }
    ////////////////////////////Begin-公共模块//////////////////////////////////////
    public class TransferFiles
    {
        public TransferFiles()
        {
        }
        public static int SendData(Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            while (total < size)
            {
                sent = s.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }

            return total;
        }

        public static byte[] ReceiveData(Socket s, int size)
        {
            int total = 0;
            int dataleft = size;
            byte[] data = new byte[size];
            int recv;
            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, SocketFlags.None);
                if (recv == 0)
                {
                    data = null;
                    break;
                }

                total += recv;
                dataleft -= recv;
            }
            return data;
        }

        public static int SendVarData(Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;
            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            sent = s.Send(datasize);
            Thread.Sleep(100);

            while (total < size)
            {
                sent = s.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }

            return total;
        }

        public static byte[] ReceiveVarData(Socket s)
        {
            int total = 0;
            int recv=0;
            byte[] datasize = new byte[4];
            recv = s.Receive(datasize, 0, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];
            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, SocketFlags.None);
                if (recv == 0)
                {
                    data = null;
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        } 
    }
}
