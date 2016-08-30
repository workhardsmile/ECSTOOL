using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Threading;
using System.Collections;
using System.IO;

namespace ECSTOOL
{
    public delegate void DelegateCloseFile(object sender, EventCloseForm e);
    public delegate void DelegateLoadComboList(string[] list, bool isFile);
    public partial class ServerOperator : DockContent
    {
        public event DelegateCloseFile DelagateCloseFileF; //声明事件
        //public DelegateLoadComboList DelegateLoadComboListF;

        public MainFrame parent;
        public string[] files;
        public string[] drectories;
        private Hashtable serverList;

        public ServerOperator()
        {
            InitializeComponent();
            this.comboSourceIP.Text = "localhost";
            this.comboTargetIP.Text = "localhost";
            this.comboSourceIP.Enabled = false;
            this.comboTargetIP.Enabled = false;
            this.comboTargetDir.BackColor = SystemColors.Control;
            this.comboSourceFile.BackColor = SystemColors.Control;
        }
        public ServerOperator(bool isSend, string paramIP, Hashtable ListIP)
        {
            InitializeComponent();
            this.serverList = ListIP;
            if (isSend)
            {
                this.comboTargetIP.Text = paramIP;
                this.comboSourceIP.Text = "localhost";
                this.comboTargetIP.Items.Add("localhost");
                if (ListIP != null)
                {
                    foreach (string _IP in ListIP.Keys)
                    {
                        this.comboTargetIP.Items.Add(_IP);
                    }
                }
                this.comboSourceIP.Enabled = false;
                this.comboSourceFile.BackColor = SystemColors.Control;
                //this.comboSourceFile.Enabled = false;
                //this.comboTargetIP.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else
            {
                this.comboSourceIP.Text = paramIP;
                this.comboTargetIP.Text = "localhost";
                this.comboSourceIP.Items.Add("localhost");
                if (ListIP != null)
                {
                    foreach (string _IP in ListIP.Keys)
                    {
                        this.comboSourceIP.Items.Add(_IP);
                    }
                }
                this.comboTargetIP.Enabled = false;
                this.comboTargetDir.BackColor = SystemColors.Control;
                //this.comboTargetDir.Enabled = false;
                //this.comboSourceIP.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }
        private void LoadComboList(string[] list, bool isFile)
        {
            if (isFile)
            {
                this.comboSourceFile.Items.Clear();
                foreach (string s in list)
                {
                    this.comboSourceFile.Items.Add(s);
                }
            }
            else
            {
                this.comboTargetDir.Items.Clear();
                foreach (string s in list)
                {
                    this.comboTargetDir.Items.Add(s);
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            EventCloseForm E = new EventCloseForm("close");
            this.Invoke(DelagateCloseFileF, null, E);
        }
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (this.comboSourceIP.Text.Trim().Equals("") || this.comboSourceFile.Text.Trim().Equals(""))
            {
                MessageType.MessageBoxShow("发送源IP和文件路径不能为空！", "warnig");
                return;
            }
            if (!this.comboTargetIP.Text.Trim().Equals("") && !this.comboTargetDir.Text.Trim().Equals(""))
            {
                string temp = "";
                if (this.comboTargetIP.Text.Trim().ToLower().Equals("localhost") || this.comboTargetIP.Text.Trim().ToLower().Equals("127.0.0.1"))
                {
                    temp = Program.IP;
                }
                else
                {
                    temp = this.comboTargetIP.Text.Trim();
                }
                this.listBoxValue.Items.Add(temp);

                temp = this.comboTargetDir.Text + "?";// +(this.checkEnforce.Checked ? "true;" : "false;");
                string param = "";
                if (!this.comboParam1.Text.Trim().Equals("") && this.comboParam1.Text.IndexOf("=") > 0)
                {
                    param = this.comboParam1.Text.Trim();
                    if (!this.comboParam1.Items.Contains(param))
                    {
                        this.comboParam1.Items.Add(param);
                    }
                    temp += param + "?";
                }
                if (!this.comboParam2.Text.Trim().Equals("") && this.comboParam2.Text.IndexOf("=") > 0)
                {
                    param = this.comboParam2.Text.Trim();
                    if (!this.comboParam2.Items.Contains(param))
                    {
                        this.comboParam2.Items.Add(param);
                    }
                    temp += param + "?";
                }
                if (!this.comboParam3.Text.Trim().Equals("") && this.comboParam3.Text.IndexOf("=") > 0)
                {
                    param = this.comboParam3.Text.Trim();
                    if (!this.comboParam3.Items.Contains(param))
                    {
                        this.comboParam3.Items.Add(param);
                    }
                    temp += param + "?";
                }
                if (this.listTask.Items.Contains(temp) && this.listBoxValue.Items[this.listTask.Items.IndexOf(temp)].Equals(this.comboTargetIP.Text.Trim()))
                {
                    MessageType.MessageBoxShow("接收源已添加！", "warnig");
                    this.listBoxValue.Items.RemoveAt(this.listTask.Items.Count);
                    return;
                }
                this.listTask.Items.Add(temp);
            }
            else
            {
                MessageType.MessageBoxShow("接收IP和目录不能为空！", "warnig");
                return;
            }
            this.comboTargetDir.Text = "";
            this.comboParam1.Text = "";
            this.comboParam2.Text = "";
            this.comboParam3.Text = "";
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (this.listTask.SelectedItem != null)
            {
                this.listBoxValue.Items.RemoveAt(this.listTask.SelectedIndex);
                this.listTask.Items.RemoveAt(this.listTask.SelectedIndex);
                this.buttonDel.Enabled = false;
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = Program.startPath + "\\Scripts";
            ofd.Filter = "*.dat|*.DAT|所有文件(*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string file = ofd.FileName;
                this.buttonClear_Click(null, null);
                if (ofd.CheckFileExists)
                {
                    string text = File.ReadAllText(file, TxtFileEncoding.GetEncoding(file));
                    string[] context = text.Split(char.Parse("\n"));
                    foreach (string str in context)
                    {
                        if (!str.Equals(""))
                        {
                            string[] _test = str.Split('|');
                            if (_test.Length >= 2 && !this.listTask.Items.Contains(_test[1]))
                            {
                                this.listBoxValue.Items.Add(_test[0]);
                                this.listTask.Items.Add(_test[1]);
                            }
                        }
                    }
                }
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (this.listTask.Items.Count > 0)
            {
                string temp = "";
                for (int i = 0; i < this.listTask.Items.Count; i++)
                {
                    temp += this.listBoxValue.Items[i] + "|" + this.listTask.Items[i] + "\n";
                }
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Program.startPath + "\\Scripts";
                sfd.Filter = "*.dat|*.DAT|所有文件(*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, temp, Encoding.UTF8);
                    //MessageBox.Show("保存成功！", "信息提示", MessageBoxButtons.OK);
                    MessageType.MessageBoxShow("导出成功！", "info");
                }
            }
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("检查无误，确定要执行列表全部传输任务吗？", "信息提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (this.listTask.Items.Count > 0)
                {
                    //发送
                    if (this.comboSourceIP.Text.Trim().ToLower().Equals("localhost") && this.comboSourceIP.Enabled == false)
                    {
                        string file = this.comboSourceFile.Text;
                        if (!file.Trim().Equals("") && File.Exists(file))
                        {
                            ArrayList StrIP = new ArrayList();
                            for (int i = 0; i < this.listTask.Items.Count; i++)
                            {
                                //string[] _str = this.listTask.Items[i].ToString().Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                string[] temps = this.listTask.Items[i].ToString().Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                if (temps.Length >= 1)
                                {
                                    //替换参数
                                    for (int j = 1; j < temps.Length; j++)
                                    {
                                        string[] param = temps[j].Split('=');
                                        if (param.Length >= 2)
                                        {
                                            temps[0] = temps[0].Replace(param[0], param[1]);
                                        }
                                    }
                                    //添加任务
                                    bool flag = true;
                                    for (int k = 0; k < StrIP.Count; k++)
                                    {
                                        string t = (string)StrIP[k];
                                        if (t.Contains(this.listBoxValue.Items[i].ToString()))
                                        {
                                            StrIP[k] = t + "?" + temps[0];
                                            flag = false;
                                            break;
                                        }
                                    }
                                    if (flag)
                                    {
                                        string ip = this.listBoxValue.Items[i].ToString();
                                        ServerInfo si = (ServerInfo)this.serverList[ip];
                                        if (si != null)
                                        {
                                            if (si.OsType == 1 && (Program.ListNetIP == null || Program.ListNetIP[ip] == null))
                                            {
                                                StrIP.Add("W" + ip + "?" + temps[0]);
                                            }
                                            else if (si.OsType == 1 && Program.ListNetIP != null && Program.ListNetIP[ip] != null)
                                            {
                                                StrIP.Add("N" + ip + "?" + temps[0]);
                                            }
                                            else if (si.OsType == 0)
                                            {
                                                StrIP.Add("L" + ip +":"+si.SshPort+":"+si.User+":"+si.Password+ ";" + temps[0]);
                                            }
                                        }
                                        else
                                        {
                                            StrIP.Add("W" + ip + "?" + temps[0]);
                                        }
                                    }
                                }
                            }
                            string winFile = file + ">";
                            string netFile = file + "|";
                            string linuxFile = file + ">";
                            
                            for (int k = 0; k < StrIP.Count; k++)
                            {
                                string m=((string)StrIP[k]);
                                if (m.Substring(0, 1).Equals("W"))
                                {
                                    winFile += m.Remove(0,1) + "|";
                                }
                                else if (m.Substring(0, 1).Equals("N"))
                                {
                                    netFile += m.Remove(0, 1) + "|";
                                }
                                else if (m.Substring(0, 1).Equals("L"))
                                {
                                    linuxFile += m.Remove(0, 1) + "|";
                                }
                            }
                            if (!winFile.Equals(file + ">"))
                            {
                                MessageType.AddMessageQueue(ref this.parent.MessageQueue, winFile , MessageType.SEND_FILE, Program.IP);
                            }
                            if (!netFile.Equals(file + "|"))
                            {
                                string[] task = netFile.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                if (task.Length > 1)
                                {
                                    for (int i = 1; i < task.Length; i++)
                                    {
                                        string[] objs = task[i].Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (objs.Length > 1)
                                        {
                                            MessageType.AddMessageQueue(ref this.parent.MessageQueue, task[0] + ">" + task[i].Replace(objs[0] + "?", ""), MessageType.N_SEND_FILE, objs[0], Program.NetIP);
                                            //MessageType.AddMessageQueue(ref this.parent.MessageQueue, task[0] + ">" + task[i].Replace(objs[0] + "?", ""), MessageType.N_SEND_FILE, Program.IP, Program.IP);
                                        }
                                    }
                                }
                                //MessageType.AddMessageQueue(ref this.parent.MessageQueue, netFile, MessageType.C_SEND_FILE, Program.IP);
                            }
                            if (!linuxFile.Equals(file + ">"))
                            {
                                MessageType.AddMessageQueue(ref this.parent.MessageQueue, linuxFile, MessageType.L_SEND_FILE, Program.IP);
                            }
                            this.parent.FileType = 1;
                        }
                        else
                        {
                            MessageType.MessageBoxShow("请检查发送源IP与文件是否正确？", "question");
                        }
                    }
                    else
                    {
                        string downFile = this.comboSourceFile.Text;
                        string downIP = this.comboSourceIP.Text;
                        ServerInfo si = (ServerInfo)this.serverList[downIP];
                        if (!downFile.Trim().Equals("") && !downIP.Trim().Equals(""))
                        {
                            if (si.OsType == 1 && Program.ListNetIP != null && Program.ListNetIP[downIP] != null)
                            {
                                downFile += ">" + Program.NetIP + "?";
                            }
                            else if (si.OsType == 0)
                            {
                                downFile += ">";
                            }
                            else
                            {
                                downFile += ">" + Program.IP + "?";
                            }
                            for (int i = 0; i < this.listTask.Items.Count; i++)
                            {
                                string[] temps = this.listTask.Items[i].ToString().Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                                if (temps.Length >= 1)
                                {
                                    for (int j = 1; j < temps.Length; j++)
                                    {
                                        string[] param = temps[j].Split('=');
                                        if (param.Length >= 2)
                                        {
                                            temps[0] = temps[0].Replace(param[0], param[1]);
                                        }
                                    }
                                }
                                downFile += temps[0] + "?";
                            }
                            if ( si.OsType ==1 && Program.ListNetIP != null && Program.ListNetIP[downIP] != null)
                            {
                                //发送两次任务，保证成功率
                                MessageType.AddMessageQueue(ref this.parent.MessageQueue, downFile, MessageType.SEND_FILE, downIP, Program.NetIP);
                                //Thread.Sleep(10);
                                //MessageType.AddMessageQueue(ref this.parent.MessageQueue, downFile, MessageType.SEND_FILE, downIP, Program.NetIP);
                            }
                            else if (si.OsType == 0)
                            {
                                MessageType.AddMessageQueue(ref this.parent.MessageQueue, downIP+":"+si.SshPort+":"+si.User+":"+si.Password+">"+downFile, MessageType.L_DOWNLOAD_FILE, Program.IP);
                            }
                            else
                            {
                                MessageType.AddMessageQueue(ref this.parent.MessageQueue, downFile, MessageType.SEND_FILE, downIP, Program.IP);
                            }
                            this.parent.FileType = 2;
                        }
                        else
                        {
                            MessageType.MessageBoxShow("请检查发送源IP与文件是否正确？", "question");
                        }
                    }
                    this.listTask.Items.Clear();
                    this.listBoxValue.Items.Clear();
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.listTask.Items.Clear();
            this.listBoxValue.Items.Clear();
        }

        private void listTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listTask.SelectedItem != null)
            {
                this.listBoxValue.SelectedIndex = this.listTask.SelectedIndex;
                this.buttonDel.Enabled = true;
            }
        }

        private void listBoxValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBoxValue.SelectedItem != null)
            {
                this.listTask.SelectedIndex = this.listBoxValue.SelectedIndex;
                this.buttonDel.Enabled = true;
            }
        }

        private void comboSourceFile_DropDown(object sender, EventArgs e)
        {
            string temp = this.comboSourceIP.Text.Trim();
            //if (!temp.Equals("") && !temp.Equals("localhost") && !temp.Equals("127.0.0.1"))
            if (!temp.Equals("") && !temp.Equals("localhost"))
            {
                //get_files(3 depth)
                ServerInfo si = (ServerInfo)serverList[temp];
                if (Program.ListNetIP != null && Program.ListNetIP[si.IP] != null)
                {
                    MessageType.AddMessageQueue(ref this.parent.MessageQueue, "2|" + si.HomePath, MessageType.C_GET_FILES, si.IP, Program.NetIP);
                }
                else
                {
                    MessageType.AddMessageQueue(ref this.parent.MessageQueue, "2|" + si.HomePath, MessageType.C_GET_FILES, si.IP, Program.IP);
                }
                this.comboSourceFile.BackColor = SystemColors.Window;
            }
            else
            {
                this.comboSourceFile.Items.Clear();
                this.comboSourceFile.BackColor = SystemColors.Control;
            }
        }

        private void comboTargetDir_DropDown(object sender, EventArgs e)
        {
            string temp = this.comboTargetIP.Text.Trim();
            //if (!temp.Equals("") && !temp.Equals("localhost") && !temp.Equals("127.0.0.1"))
            if (!temp.Equals("") && !temp.Equals("localhost"))
            {
                //get_files(3 depth)
                ServerInfo si = (ServerInfo)serverList[temp];
                if (Program.ListNetIP != null && Program.ListNetIP[si.IP] != null)
                {
                    MessageType.AddMessageQueue(ref this.parent.MessageQueue, "2|" + si.HomePath, MessageType.C_GET_DIRS, si.IP, Program.NetIP);
                }
                else
                {
                    MessageType.AddMessageQueue(ref this.parent.MessageQueue, "2|" + si.HomePath, MessageType.C_GET_DIRS, si.IP, Program.IP);
                }
                this.comboTargetDir.BackColor = SystemColors.Window;
            }
            else
            {
                this.comboTargetDir.Items.Clear();
                this.comboTargetDir.BackColor = SystemColors.Control;
            }
        }

        private void ServerOperator_Shown(object sender, EventArgs e)
        {
            if (this.comboSourceIP.Text.Trim().Equals("localhost"))
            {
                this.comboTargetDir_DropDown(null, null);
            }
            else
            {
                this.comboSourceFile_DropDown(null, null);
            }
            this.parent.DelegateLoadComboListF += new DelegateLoadComboList(LoadComboList);
        }

        public void comboSourceFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void comboSourceFile_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (this.comboSourceIP.Text.Trim().Equals("localhost"))
                {
                    string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                    if (File.Exists(path))
                    {
                        this.comboSourceFile.Text = path;
                        this.buttonClear_Click(null, null);
                    }
                    else
                    {
                        MessageType.MessageBoxShow("文件不存在！请重新选择。", "question");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageType.MessageBoxShow(ex.Message, "error");
            }
        }

        private void comboTargetDir_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (this.comboTargetIP.Text.Trim().Equals("localhost"))
                {
                    string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                    if (Directory.Exists(path))
                    {
                        this.comboTargetDir.Text = path;
                    }
                    else
                    {
                        MessageType.MessageBoxShow("目录不存在！请重新选择。", "question");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageType.MessageBoxShow(ex.Message, "error");
            }
        }

        private void comboSourceFile_TextChanged(object sender, EventArgs e)
        {
            this.buttonClear_Click(null, null);
        }
    }
}
