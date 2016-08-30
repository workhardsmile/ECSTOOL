using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace ECSTOOL
{
    public delegate void DelegateCloseServer(object sender, EventCloseForm e);
    public delegate void DelegateCheckListview(object sender, EventArgs e);
    public delegate void DelegateFileTrans(bool isSend, string paramIP, Hashtable ListIP);
    public delegate void DelegateCommand(string ListIP);
    public partial class ServerList : DockContent
    {
        public event DelegateCloseServer DelagateCloseServerF; //声明事件
        public event DelegateCheckListview DelegateCheckListviewF; //声明事件
        public DelegateFileTrans DelegateFileTransF;
        public DelegateCommand DelegateCommandF;

        public MainFrame parent;

        public ServerList()
        {
            InitializeComponent();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            EventCloseForm E = new EventCloseForm("close");
            this.Invoke(DelagateCloseServerF, null, E);
        }

        #region 自定义命令方法
        public byte GetServerOstype(string CommandContent)
        {
            byte flag = 0;
            flag = ((ServerInfo)this.listViewServer.Items[CommandContent].Tag).OsType;
            return flag;
        }
        public string[] GetAllServers(string CommandContent)
        {
            string[] flag = new string[this.listViewServer.Items.Count];
            for (int i = 0; i < this.listViewServer.Items.Count; i++)
            {
                flag[i] = this.listViewServer.Items[i].Name;
            }
            return flag;
        }
        public string[] GetErrorServers(string CommandContent)
        {
            string[] flag = null;
            ArrayList flags = new ArrayList();
            for (int i = 0; i < this.listViewServer.Items.Count; i++)
            {
                if (((ServerInfo)this.listViewServer.Items[CommandContent].Tag).HomePath.Equals("配置错误"))
                {
                    flags.Add(this.listViewServer.Items[i].Name);
                }
            }
            flag = (string[])flags.ToArray(typeof(string));
            return flag;
        }
        public bool DeleteOneServer(string CommandContent)
        {
            bool flag = true;
            try
            {
                this.parent.WinClient.ClientSessionTable.Remove(CommandContent);
                this.parent.WinClient._hostTable.Remove(CommandContent);
                this.parent.WinClient._hostToken.Remove(CommandContent);
                this.listViewServer.Items.RemoveByKey(CommandContent);
                this.listViewServer.Refresh();
                MessageType.MessageBoxShow("临时删除成功！彻底删除方式：\r\n使用命令删除或删除 [" + CommandContent + "] 信息行于 " + Program.EtcFile,"info");                
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        #endregion

        public void CheckListview(object sender, EventArgs e)
        {
            string[] obj = ((string)sender).Split(':');
            if (obj.Length > 1)
            {
                ListViewItem lvi = this.listViewServer.Items[obj[0]];
                if (lvi != null)
                {
                    if (obj[1].Trim().ToLower().Equals("closed") && lvi.ImageIndex == 1)
                    {
                        lvi.ImageIndex = 0;
                        this.listViewServer.Refresh();
                    }
                    else if (obj[1].Trim().ToLower().Equals("normal") && lvi.ImageIndex == 0)
                    {
                        lvi.ImageIndex = 1;
                        this.listViewServer.Refresh();
                    }
                }
            }
        }
        public void CheckSession(object sender, EventArgs e)
        {
            if (sender != null)
            {
                this.Invoke(DelegateCheckListviewF, sender, e);
            }
        }
        private void ServerList_Shown(object sender, EventArgs e)
        {
            //FileInfo fi = new FileInfo(Path.Combine(Program.startPath, "ServerETC.lst"));
            string readPath = Program.EtcFile;
            this.panelServer.Visible = false;
            this.listViewServer.Items.Clear();
            if (File.Exists(readPath))
            {
                StreamReader sr = new StreamReader(readPath);
                string temp;

                if (this.parent.WinClient._hostTable == null)
                {
                    this.parent.WinClient._hostTable = new Hashtable();
                }              
                if (this.parent.WinClient._hostToken == null)
                {
                    this.parent.WinClient._hostToken = new Hashtable();
                }
               
                if (this.parent.LinuxServer.ServerTable == null)
                {
                    this.parent.LinuxServer.ServerTable = new Hashtable();
                }

                while ((temp = sr.ReadLine()) != null)
                {
                    if (temp== "" || temp.Trim().IndexOf("#") == 0 || System.Text.RegularExpressions.Regex.Match(temp, @"\r*\n[\s| ]*\r*").Success)
                    {
                        continue;
                    }
                    else
                    {
                        ServerInfo _si = new ServerInfo(temp);
                        ListViewItem item = new ListViewItem(_si.IP);
                        item.Name = _si.IP;
                        if (_si.OsType == 0)
                        {
                            item.SubItems.Add("linux");
                        }
                        else
                        {
                            item.SubItems.Add("win");
                            item.BackColor = System.Drawing.Color.AliceBlue;
                        }
                        item.SubItems.Add(_si.HomePath);
                        item.Tag = _si;
                        //item.ToolTipText = _si.HomePath;
                        if (this.listViewServer.Groups[_si.Group] == null)
                        {
                            this.listViewServer.Groups.Add(new ListViewGroup(_si.Group, _si.Group));
                        }
                        item.Group = this.listViewServer.Groups[_si.Group];
                        if (_si.HomePath.Equals("配置错误"))
                        {
                            item.ImageIndex = 2;
                            //item.ForeColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            item.ImageIndex = 0;
                            if (_si.OsType == 1)
                            {                               
                                    if (this.parent.WinClient._hostTable[_si.IP] == null)
                                    {
                                         this.parent.WinClient._hostTable.Add(_si.IP, _si.ServerPort);
                                    }
                                    if (this.parent.WinClient._hostToken[_si.IP] == null)
                                    {
                                         this.parent.WinClient._hostToken.Add(_si.IP, _si.Token);
                                    }
                            }
                            else if (_si.OsType == 0)
                            {
                                if (this.parent.LinuxServer.ServerTable[_si.IP] == null)
                                {
                                      this.parent.LinuxServer.ServerTable.Add(_si.IP, _si);
                                }
                            }
                        }
                        this.listViewServer.Items.Add(item);
                        this.listViewServer.Refresh();
                    }
                }
                sr.Close();
                this.parent.WinClient.DelegateCheckSessionF += new DelegateCheckSession(CheckSession);
                this.parent.LinuxServer.DelegateCheckSessionF += new DelegateCheckSession(CheckSession);
                this.DelegateCheckListviewF += new DelegateCheckListview(CheckListview);
            }
        }
        private void contextMenuServer_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.listViewServer.SelectedItems.Count == 1)
            {
                if (this.listViewServer.SelectedItems[0].ImageIndex == 0)
                {
                    this.connectToolStripMenuItem.Enabled = true;
                    this.disConnectToolStripMenuItem.Enabled = false;
                    this.uploadFileToolStripMenuItem.Enabled = false;
                    this.downloadFileToolStripMenuItem.Enabled = false;
                    this.executeCMDToolStripMenuItem.Enabled = false;
                }
                else if (this.listViewServer.SelectedItems[0].ImageIndex == 1)
                {
                    this.connectToolStripMenuItem.Enabled = false;
                    this.disConnectToolStripMenuItem.Enabled = true;
                    this.uploadFileToolStripMenuItem.Enabled = true;
                    this.downloadFileToolStripMenuItem.Enabled = true;
                    this.executeCMDToolStripMenuItem.Enabled = true;
                }
                else
                {
                    this.connectToolStripMenuItem.Enabled = false;
                    this.disConnectToolStripMenuItem.Enabled = false;
                    this.uploadFileToolStripMenuItem.Enabled = false;
                    this.downloadFileToolStripMenuItem.Enabled = false;
                    this.executeCMDToolStripMenuItem.Enabled = false;
                }
                this.deleteServerToolStripMenuItem.Enabled = true;
            }
            else if (this.listViewServer.SelectedItems.Count == 0)
            {
                this.connectToolStripMenuItem.Enabled = false;
                this.disConnectToolStripMenuItem.Enabled = false;
                this.uploadFileToolStripMenuItem.Enabled = false;
                this.downloadFileToolStripMenuItem.Enabled = false;
                this.executeCMDToolStripMenuItem.Enabled = false;
                this.deleteServerToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.connectToolStripMenuItem.Enabled = true;
                this.disConnectToolStripMenuItem.Enabled = true;
                this.uploadFileToolStripMenuItem.Enabled = true;
                this.downloadFileToolStripMenuItem.Enabled = true;
                this.executeCMDToolStripMenuItem.Enabled = true;
                this.deleteServerToolStripMenuItem.Enabled = true;
            }
        }
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                ListViewItem lvi = this.listViewServer.SelectedItems[i];
                if (lvi.SubItems[1].Text.Equals("win") && (!lvi.SubItems[2].Text.Equals("配置错误")))
                {
                    ThreadPool.QueueUserWorkItem(this.parent.StartOneSocket, lvi.Text);
                }
                else if (lvi.SubItems[1].Text.Equals("linux"))
                {
                    //……
                    ThreadPool.QueueUserWorkItem(this.parent.StartOneSsh, lvi.Text);
                }
                this.listViewServer.Refresh();
            }
        }
        private void disConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                ListViewItem lvi = this.listViewServer.SelectedItems[i];
                if (lvi.SubItems[1].Text.Equals("win"))
                {
                    ThreadPool.QueueUserWorkItem(this.parent.CloseOneSocket, lvi.Text);
                }
                else if (lvi.SubItems[1].Text.Equals("linux"))
                {
                    //……
                    ThreadPool.QueueUserWorkItem(this.parent.CloseOneSsh, lvi.Text);
                }
                this.listViewServer.Refresh();
            }
        }
        private void deleteServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                //this.listViewServer.Items.Remove(this.listViewServer.SelectedItems[i]);
                //this.listViewServer.Refresh();
                DeleteOneServer(this.listViewServer.SelectedItems[i].Name);
            }
        }

        private void connectStripButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in this.listViewServer.Items)
            {
                if (lvi.ImageIndex == 0)
                {
                    if (lvi.SubItems[1].Text.Equals("win") && (!lvi.SubItems[2].Text.Equals("配置错误")))
                    {
                        ThreadPool.QueueUserWorkItem(this.parent.StartOneSocket, lvi.Text);
                    }
                    else if (lvi.SubItems[1].Text.Equals("linux"))
                    {
                        //……
                        ThreadPool.QueueUserWorkItem(this.parent.StartOneSsh, lvi.Text);
                    }
                    this.listViewServer.Refresh();
                }
            }
        }

        private void disconnectStripButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in this.listViewServer.Items)
            {
                if (lvi.ImageIndex == 1)
                {
                    if (lvi.SubItems[1].Text.Equals("win"))
                    {
                        ThreadPool.QueueUserWorkItem(this.parent.CloseOneSocket, lvi.Text);
                    }
                    else if (lvi.SubItems[1].Text.Equals("linux"))
                    {
                        //……
                        ThreadPool.QueueUserWorkItem(this.parent.CloseOneSsh, lvi.Text);
                    }
                    this.listViewServer.Refresh();
                }
            }
        }

        private void uploadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hashtable ListIP = new Hashtable();
            ServerInfo si=null;
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                if (this.listViewServer.SelectedItems[i].ImageIndex == 1)// && this.listViewServer.SelectedItems[i].Name != "127.0.0.1")
                {
                    si = (ServerInfo)this.listViewServer.SelectedItems[i].Tag;
                    ListIP.Add(this.listViewServer.SelectedItems[i].Name,si);
                }
            }
            //string[] ListIP = (string[])arrIP.ToArray(typeof(string));
            if (ListIP.Count > 0)
            {
                this.Invoke(this.DelegateFileTransF, true, si.IP , ListIP);
            }
            else
            {
                MessageType.MessageBoxShow("尚未与选中的服务器建立连接！请先建立连接后再发送文件。", "question");
                return;
            }
            //this.Invoke(this.DelegateFileTransF,false,
            //string CommandContent = "F:\\[大家网]Python核心编程(第二版).pdf>10.34.130.62?C:\\firefox?C:\\firefox\\firefox fire|10.34.135.154?C:\\firefox|";
            //if (!ThreadPool.QueueUserWorkItem(this.SendFile, CommandContent))
            //{
            //    return;
            //}
        }

        private void downloadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hashtable ListIP = new Hashtable();
            ServerInfo si = null;
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                if (this.listViewServer.SelectedItems[i].ImageIndex == 1)// && this.listViewServer.SelectedItems[i].Name != "127.0.0.1")
                {
                    si = (ServerInfo)this.listViewServer.SelectedItems[i].Tag;
                    ListIP.Add(this.listViewServer.SelectedItems[i].Name, si);                    
                }
            }
            //string[] ListIP = (string[])arrIP.ToArray(typeof(string));
            if (ListIP.Count > 0)
            {
                this.Invoke(this.DelegateFileTransF, false, si.IP, ListIP);
            }
            else
            {
                MessageType.MessageBoxShow("尚未与选中的服务器建立连接！请先建立连接后再发送文件。", "question");
                return;
            }
            //string CommandContent = "10.34.135.154|18889|1|C:\\firefox?C:\\firefox\\firefox fire";
            //if (!ThreadPool.QueueUserWorkItem(this.DownloadFile, CommandContent))
            //{
            //    return;
            //}
        }

        private void AddServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.textIP.Text = "";
            this.textGroup.Text = "";
            this.panelServer.Visible = true;
            this.groupOS.Visible = false;
            this.comboType.SelectedIndex = 1;
            comboType_SelectedIndexChanged(null,null);
        }

        private void toolStripRefresh_Click(object sender, EventArgs e)
        {
            ServerList_Shown(null,null);
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.groupOS.Visible = true;
            if (this.comboType.SelectedIndex == 0)
            {
                this.groupOS.Text = "Linux服务器";
                this.labelPort.Text = "SSH端口";
                this.labelUser.Text = "用户名";
                this.labelPwd.Text = "密  码";
                this.textPort.Text = "22";
                this.textUser.Text = "";
                this.textPwd.Text = "";
                this.textPath.Text = "";
            }
            else
            {
                this.groupOS.Text = "Windows服务器";
                this.labelPort.Text = "Serv端口";
                this.labelUser.Text = "File端口";
                this.labelPwd.Text = "Token值";
                this.textPort.Text = "18888";
                this.textUser.Text = "18889";
                this.textPwd.Text = "";
                this.textPath.Text = "";
            }
        }
        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.panelServer.Visible = false;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string strIP = this.textIP.Text.Trim().ToLower();
            string strGroup = this.textGroup.Text.Trim().ToUpper();
            int type = this.comboType.SelectedIndex;
            string p1 = this.textPort.Text.Trim();
            string p2 = this.textUser.Text.Trim();
            string p3 = this.textPwd.Text.Trim();
            string homePath = this.textPath.Text;
            LogManager LM = new LogManager(Program.EtcFile);
            EncryDecryUtil edu = new EncryDecryUtil();
            //string temp = edu.EncryptString("chengdu & nhn123!@#", ServerInfo.KEY);
            if (this.groupOS.Text.Equals("Linux服务器"))
            {
                int port = 22;
                try
                {
                    port = Convert.ToInt32(p1);
                }
                catch
                {
                    MessageType.MessageBoxShow("请确认[SSH端口]是否正整数？", "question");
                    return;
                }
                if (strIP != "" && port > 0 && strGroup != "" && p2 != "" && p3 != "" && homePath.Contains("/"))
                {
                    string temp = edu.EncryptString(p2 + "&" + p3, MessageType.KEY);
                    temp = strIP + ";" + strGroup + ";" + type + ";" + port + ";" + temp + ";" + homePath;
                    LM.WriteLog(temp);
                }
                else
                {
                    MessageType.MessageBoxShow("请确认各项配置是否正确？", "question");
                    return;
                }
            }
            else
            {
                int sport=18888, fport=18889;
                try
                {
                    sport = Convert.ToInt32(p1);
                }
                catch
                {
                    MessageType.MessageBoxShow("请确认[Serv端口]是否正整数？", "question");
                    return;
                }
                try
                {
                    fport = Convert.ToInt32(p2);
                }
                catch
                {
                    MessageType.MessageBoxShow("请确认[File端口]是否正整数？", "question");
                    return;
                }
                if (strIP != "" && sport > 0 && fport > 0 && strGroup != "" && p3 != "" && homePath.Contains(":"))
                {
                    string temp = edu.EncryptString(p3, MessageType.KEY);
                    temp = strIP + ";" + strGroup + ";" + type + ";" + sport + "&" + fport + ";" + temp + ";" + homePath;
                    LM.WriteLog(temp);
                }
                else
                {
                    MessageType.MessageBoxShow("请确认各项配置是否正确？", "question");
                    return;
                }
            }
            MessageType.MessageBoxShow("添加服务器<"+strIP+">配置成功！", "info");
            this.panelServer.Visible = false;
            this.ServerList_Shown(null, null);
        }

        private void executeCMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ListIP = "";
            for (int i = 0; i < this.listViewServer.SelectedItems.Count; i++)
            {
                if (this.listViewServer.SelectedItems[i].ImageIndex == 1)// && this.listViewServer.SelectedItems[i].Name != "127.0.0.1")
                {
                    ListIP += this.listViewServer.SelectedItems[i].Name + ",";
                }
            }
            //string[] ListIP = (string[])arrIP.ToArray(typeof(string));
            if (!ListIP.Equals(""))
            {
                ListIP = ListIP.Remove(ListIP.LastIndexOf(','));
                this.Invoke(this.DelegateCommandF, ListIP);
            }
            else
            {
                MessageType.MessageBoxShow("尚未与选中的服务器建立连接！请先建立连接后再执行命令。", "question");
                return;
            }
        }

        private void 编辑配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = Program.EtcFile;
            WinCMD wc = new WinCMD();
            string s = Program.startPath;
            wc.ExePath = Path.Combine(s, "Plugs\\应用脚本编辑器.exe");
            wc.Argument = file;
            s = file.Replace(" ", "");
            if (file.Length == s.Length)
            {
                Thread temp = new Thread(wc.StartProgram);
                temp.Start();
            }
            else
            {
                Process.Start(Program.startPath);
            }
        }

    }
    public class ServerInfo
    {
        private string ip;
        private string group;
        private byte osType = 0;
        private int sshPort;
        private int serverPort;
        private int senderPort;
        private string user;
        private string password;
        private string token;
        private string homePath;

        public bool IsConnected = false;

        public string IP
        {
            get { return ip; }
            //set { ip = value; }
        }

        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        public byte OsType
        {
            get { return osType; }
            //set { osType = value; }
        }

        public int SshPort
        {
            get { return sshPort; }
            //set { sshPort = value; }
        }

        public int ServerPort
        {
            get { return serverPort; }
            //set { serverPort = value; }
        }

        public int SenderPort
        {
            get { return senderPort; }
            //set { senderPort = value; }
        }

        public string User
        {
            get { return user; }
            //set { user = value; }
        }

        public string Password
        {
            get { return password; }
            //set { password = value; }
        }

        public string Token
        {
            get { return token; }
            //set { token = value; }
        }

        public string HomePath
        {
            get { return homePath; }
            //set { homePath = value; }
        }
        /// <summary>
        /// 原始配置行
        /// </summary>
        /// <param name="serverInfo"></param>
        public ServerInfo(string serverInfo)
        {
            string[] server = serverInfo.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (server.Length >= 6)
            {
                try
                {
                    EncryDecryUtil edu = new EncryDecryUtil();
                    this.ip = server[0].Trim();
                    this.group = server[1].Trim();
                    this.osType = Convert.ToByte(server[2].Trim());
                    if (this.osType == 0)
                    {
                        this.sshPort = Convert.ToInt32(server[3].Trim());
                        this.serverPort = 0;
                        this.senderPort = 0;


                        string[] temp = edu.DecryptString(server[4].Trim(), MessageType.KEY).Split('&');
                        this.user = temp[0].Trim();
                        this.password = temp[1].Trim();
                    }
                    else
                    {
                        this.sshPort = 0;
                        string[] temp = server[3].Trim().Split('&');
                        this.serverPort = Convert.ToInt32(temp[0]);
                        this.senderPort = Convert.ToInt32(temp[1]);

                        this.user = null;
                        this.password = null;
                        this.token = edu.DecryptString(server[4].Trim(), MessageType.KEY);
                    }
                    this.homePath = server[5];
                }
                catch
                {
                    this.homePath = "配置错误";
                }
            }
        }
        /// <summary>
        /// Windows ServerInfo
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="group"></param>
        /// <param name="serverPort"></param>
        /// <param name="senderPort"></param>
        /// <param name="token"></param>
        /// <param name="homePath"></param>
        public ServerInfo(string _ip, string _group, int _serverPort, int _senderPort, string _token, string _homePath)
        {
            this.ip = _ip;
            this.group = _group;
            this.osType = 1;
            this.sshPort = 0;
            this.serverPort = _serverPort;
            this.senderPort = _senderPort;
            this.user = null;
            this.password = null;
            this.token = _token;
            this.homePath = _homePath;
        }
        /// <summary>
        /// Linux ServerInfo
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="group"></param>
        /// <param name="sshPort"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="homePath"></param>
        public ServerInfo(string _ip, string _group, int _sshPort, string _user, string _password, string _homePath)
        {
            this.ip = _ip;
            this.group = _group;
            this.osType = 0;
            this.sshPort = _sshPort;
            this.serverPort = 0;
            this.senderPort = 0;
            this.user = _user;
            this.password = _password;
            this.homePath = _homePath;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool SaveServerInfo(string filePath)
        {
            bool flag = false;

            EncryDecryUtil edu = new EncryDecryUtil();
            string temp = this.ip + ";" + this.group + ";" + this.osType + ";" + this.sshPort + ";";
            if (this.osType == 0)
            {
                temp += edu.EncryptString(this.user + "&" + this.password, MessageType.KEY) + ";";
            }
            else
            {
                temp += edu.EncryptString(this.token, MessageType.KEY);
            }
            temp += this.homePath;

            string savePath = Program.EtcFile;
            if (File.Exists(savePath))
            {
                LogManager lm = new LogManager(savePath);
                lm.WriteLog(temp);
                flag = true;
            }
            else
            {
                flag = false;
            }
            return flag;
        }
    }
}
