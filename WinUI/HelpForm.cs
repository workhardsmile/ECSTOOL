using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ECSTOOL
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }
        public HelpForm(string text)
        {
            InitializeComponent();
            this.Text = text;
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            string snPath = Path.Combine(Program.startPath, "SN.TXT");
            string text = "程序版本：\t" + Program.version + "免费版";
            //text += "\r\n授权序列：\t"+ File.ReadAllText(snPath);
            text += "\r\n作者：\t\t沃克哈德";
            text += "\r\n联系方式：\tworkhard_smile@163.com (email)";
            text += "\r\n\t\thttp://blog.csdn.net/w565911788 (csdn)";        
            text += "\r\nCopyright © 沃克哈德. All Rights Reserved.";
            text += "\r\n*特别提示： 可通过联系方式发送Bug和改善建议,获取后续升级版本。";
            this.richTextAbout.Text = text;
            //string result = "\r\nIronPythonTest.exe\t -log logPath ;执行命令后输出日志全路径,默认Log/";
            //result += "\r\n\t\t\t -py filePath ;执行python脚本全路径";
            //result += "\r\n\t\t\t -t second ;等待时间S(秒)";
            //result += "\r\n\t\t\t -fo ;打开主窗体";
            //result += "\r\n\t\t\t -s ;即时退出系统";
            //this.richTextBox1.Text += result;
        }
    }
}
