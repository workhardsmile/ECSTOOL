using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;
using System.Threading;
using LuaInterface;
using System.Diagnostics;

namespace ECSTOOL
{
    public delegate void DelegateSetEnable(bool flag);
    public delegate void DelegateOutputMessage(object sender, EventArgs e);

    public partial class CommandForm : DockContent
    {
        public MainFrame parent;
        public string name = "Python";
        //public IronPythonController IPC = new IronPythonController();
        public DelegateSetEnable DelegateSetEnableF;
        //public event DelegateOutputMessage EventOutputMessage;
        private int MaxLines = 100;

        //public Thread LuaExecThread;

        public CommandForm()
        {
            InitializeComponent();
            //IPC.SetVariable("Ipy_this", this);
        }
        public CommandForm(string ListIP)
        {
            InitializeComponent();
            //IPC.SetVariable("Ipy_this", this);
            this.textExecObj.Text = ListIP;
        }

        public void OutputMessage(object sender, EventArgs e)
        {
            string message = (string)sender;
            if (this.txtPrint.Lines.Length >= this.MaxLines)
            {
                string[] temp = new string[this.txtPrint.Lines.Length - this.MaxLines];
                ExitSave(this.MaxLines);
                for (int i = this.MaxLines; i < this.txtPrint.Lines.Length; i++)
                {
                    temp[i - this.MaxLines] = this.txtPrint.Lines[i];
                }
                this.txtPrint.Lines = temp;
            }
            this.txtPrint.Text += "\n" + message;
            try
            {
                this.txtPrint.SelectionStart = this.txtPrint.Text.Length;
            }
            catch { }
            this.txtPrint.Focus();
        }
        private void toolStripOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Program.startPath + "\\Scripts";
            ofd.Filter = "*.lua|*.LUA|所有文件(*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string file = ofd.FileName;
                if (ofd.CheckFileExists)
                {
                    WinCMD wc = new WinCMD();
                    wc.ExePath = Program.startPath + @"\Plugs\应用脚本编辑器.exe";
                    wc.Argument = file;
                    string s = file.Replace(" ", "");
                    if (file.Length == s.Length)
                    {
                        Thread temp = new Thread(wc.StartProgram);
                        temp.Start();
                    }

                    string text = File.ReadAllText(file, TxtFileEncoding.GetEncoding(file));
                    this.txtScript.Text = text;
                    this.toolStripPath.Text = "Path: " + file;
                }
            }
        }
        private void toolStripSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Program.startPath + "\\Scripts";
            sfd.Filter = "*.lua|*.LUA|所有文件(*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, this.txtScript.Text, Encoding.UTF8);
                //MessageBox.Show("保存成功！", "信息提示", MessageBoxButtons.OK);
                MessageType.MessageBoxShow("保存成功！", "info");
                this.toolStripPath.Text = "Path: " + sfd.FileName;
            }
        }

        private void txtScript_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0 && e.KeyCode == Keys.O)
            {
                toolStripOpen_Click(null, null);
            }
            else if ((Control.ModifierKeys & Keys.Control) != 0 && e.KeyCode == Keys.S)
            {
                toolStripSave_Click(null, null);
            }
            else if ((Control.ModifierKeys & Keys.Control) != 0 && e.KeyCode == Keys.F5)
            {
                toolStripExec_Click(null, null);
            }
            else if ((Control.ModifierKeys & Keys.Shift) != 0 && e.KeyCode == Keys.F5)
            {
                toolStripStop_Click(null, null);
            } 
            else
            {
                return;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            ExitSave(this.txtPrint.Lines.Length);
            this.parent.ironPythonToolStripMenuItem1.DisplayStyle = ToolStripItemDisplayStyle.Text;
        }

        private void CommandForm_Shown(object sender, EventArgs e)
        {
            this.DelegateSetEnableF += new DelegateSetEnable(SetEnable);
            this.parent.EventOutputMessage += new DelegateOutputMessage(OutputMessage);
            this.txtPrint.Text = "";
            this.textCommand.Focus();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitSave(this.txtPrint.Lines.Length);
            this.txtPrint.Text = "";
        }
        private void ExitSave(int length)
        {
            string log = "";
            string logName = log + LogFile.CmdOutput;
            for (int i = 0; i < length; i++)
            {
                log += this.txtPrint.Lines[i] + "\r\n";
            }
            if (logName != null && !logName.Equals(""))
            {
                LogManager lm = new LogManager(Path.Combine(Program.startPath, "Log\\" + logName + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
                lm.WriteLog(log);
            }
            Thread.Sleep(100);
            //尽量减少队列处理
            //MessageType.AddMessageQueue(ref this.parent.MessageQueue, log, MessageType.OUTPUT_LOG, Program.IP);
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.MaxLines = 50;
            this.toolStripMenuItem1.Checked = true;
            this.toolStripMenuItem2.Checked = false;
            this.toolStripMenuItem3.Checked = false;
            this.toolStripMenuItem4.Checked = false;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.MaxLines = 100;
            this.toolStripMenuItem2.Checked = true;
            this.toolStripMenuItem1.Checked = false;
            this.toolStripMenuItem3.Checked = false;
            this.toolStripMenuItem4.Checked = false;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.MaxLines = 200;
            this.toolStripMenuItem3.Checked = true;
            this.toolStripMenuItem2.Checked = false;
            this.toolStripMenuItem1.Checked = false;
            this.toolStripMenuItem4.Checked = false;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.MaxLines = 500;
            this.toolStripMenuItem4.Checked = true;
            this.toolStripMenuItem2.Checked = false;
            this.toolStripMenuItem3.Checked = false;
            this.toolStripMenuItem1.Checked = false;
        }
        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnEnter_Click(null, null);
            }
        }
        private void btnEnter_Click(object sender, EventArgs e)
        {
            string commands = this.textCommand.Text.Trim();
            if (!commands.Equals(""))
            {
                commands = this.textExecObj.Text.Trim().ToLower() + ":" + commands;
                this.parent.ExecCommand(commands);
            }
        }
        //执行完毕
        public void SetEnable(bool flag)
        {
            if (flag)
            {
                this.toolStripExec.Enabled = false;
                this.toolStripStop.Enabled = true;
            }
            else
            {
                toolStripStop_Click(null, null);
            }
        }
        private void toolStripStop_Click(object sender, EventArgs e)
        {
            if (this.parent.LuaPlus != null)
            {
                this.parent.LuaPlus.pLuaVM.Close();
                this.parent.LuaPlus.pLuaVM = null;
                this.parent.LuaPlus = null;
            }
            if (this.parent.LuaExecThread != null)
            {
                this.parent.LuaExecThread.Abort();
                this.parent.LuaExecThread = null;
            }
            this.toolStripExec.Enabled = true;
            this.toolStripStop.Enabled = false;
        }
        private void toolStripExec_Click(object sender, EventArgs e)
        {
            if (ComputerInfo.IsServer2003())
            {
                MessageType.AddMessageQueue(ref this.parent.MessageQueue, "LUA虚拟机目前不支持WindowsServer2003！请尝试XP、Win7等版本。", MessageType.OUTPUT_COMMAND, Program.IP);
            }
            else
            {
                this.parent.scriptText = this.txtScript.Text;
                if (this.parent.LuaExecThread == null || !this.parent.LuaExecThread.IsAlive)
                {
                    this.parent.LuaExecThread = new Thread(this.parent.LuaExecText);
                    this.parent.LuaExecThread.Start();
                    SetEnable(true);
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Program.startPath, @"Docs\使用帮助.CHM");
            if (File.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}
