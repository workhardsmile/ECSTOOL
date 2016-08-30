using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace ECSTOOL
{
    public static class Program
    {
        public static string[] Args;
        public static string startPath = Application.StartupPath;
        public static string version = "0.0.0.0";
        public static string OwnSN = "unkown";
        public static bool IsActived = false;
        public static int ServerPort = 18888;
        public static int SenderPort = 18889;
        public static string Token = "hello world";
        public static bool IsServer;
        public static bool IsEncry;
        public static string IP = "127.0.0.1";   //内网IP
        public static string NetIP = "127.0.0.1";   //公网IP
        public static Hashtable ListNetIP;
        public static string EtcFile = Path.Combine(Program.startPath, "ServerETC.lst");

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #region
            version = GetVersion();
            ComputerInfo cinfo = new ComputerInfo();
            #endregion
            OwnSN = cinfo.GetCpuID();
            IP = cinfo.GetIPAddress();
            try
            {
                NetIP = cinfo.NetIP();
            }
            catch
            {
                NetIP = IP;
            }
            #region
            CheckActive();
            //IsActived = true;
            #endregion
            Thread.CurrentThread.IsBackground = true;
            #region
            if (IsActived == true)
            {
                try
                {
                    ServerPort = ECSTOOL.Properties.Settings.Default.ServerPort;
                    SenderPort = ECSTOOL.Properties.Settings.Default.SenderPort;
                    Token = ECSTOOL.Properties.Settings.Default.Token;
                    IsServer = ECSTOOL.Properties.Settings.Default.IsServer;
                    IsEncry = ECSTOOL.Properties.Settings.Default.IsEncry;
                }
                catch
                {
                }

                Args = args;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrame());
            }
            else
            {
                HelpForm hf = new HelpForm("你的试用期已到！");
                hf.ShowDialog();
            }
            #endregion
        }

        private static void CheckActive()
        {
            string snPath = Path.Combine(Program.startPath, "SN.TXT");
            string sysFile = Path.Combine(System.Environment.GetEnvironmentVariable("appdata"), Program.OwnSN);
            string tempFile = Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), "SN.tmp");
            string key = "iron" + Program.version.Replace(".", "");
            int count = 0;
            string[] active;
            if (File.Exists(snPath))
            {
                active = GetParams(snPath, key);
            }
            else
            {
                return;
            }
            if (active != null && active.Length >= 5 && active[0].Equals("$") && active[4].Equals("$"))
            {
                try
                {
                    EncryDecryUtil edu = new EncryDecryUtil();
                    if (active[3].Equals("false"))
                    {
                        if (File.Exists(sysFile))
                        {
                            edu.DecryptFile(sysFile, tempFile, key);
                            string[] temp = GetParams(tempFile, key);
                            if (temp != null && temp.Length >= 5 && temp[0].Equals("$") && temp[4].Equals("$"))
                            {
                                count = IsActive(active[2], temp[2], temp[1]);
                                string text = active[0] + "`" + active[1] + "`" + count + "`false`" + active[4];
                                text = edu.EncryptString(text, key);
                                File.WriteAllText(snPath, text);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (active[3].Equals("true"))
                    {
                        if (File.Exists(sysFile))
                        {
                            edu.DecryptFile(sysFile, tempFile, key);
                            string[] temp = GetParams(tempFile, key);
                            if (temp != null && temp.Length >= 5 && temp[0].Equals("$") && temp[4].Equals("$"))
                            {
                                //count = IsActive(active[2], temp[2], temp[1]);
                                count = IsActive("0", temp[2], temp[1]);
                            }
                        }
                        else
                        {
                            Program.IsActived = true;
                            edu.EncryptFile(snPath, sysFile, key);
                            string text = active[0] + "`" + active[1] + "`0`false`" + active[4];
                            text = edu.EncryptString(text, key);
                            File.WriteAllText(snPath, text);
                        }
                    }
                    else if (active[3].Equals("update"))
                    {
                        if (File.Exists(sysFile))
                        {
                            Program.IsActived = true;
                            edu.EncryptFile(snPath, sysFile, key);
                            string text = active[0] + "`" + active[1] + "`0`false`" + active[4];
                            text = edu.EncryptString(text, key);
                            File.WriteAllText(snPath, text);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("你的版本和授权序列号不一致！", "信息提示");
                }
            }
        }
        public static string[] GetParams(string tempFile, string key)
        {
            string[] temp;
            EncryDecryUtil edu = new EncryDecryUtil();
            string text = File.ReadAllText(tempFile, TxtFileEncoding.GetEncoding(tempFile));
            text = edu.DecryptString(text, key);
            temp = text.Split('`');
            if (temp.Length >= 3)
            {
                if (temp[0].ToUpper().Equals("E2"))
                {
                    text = edu.DecryptString(temp[1], temp[2]);
                    temp = text.Split('`');
                }
            }
            else
            {
                temp = null;
            }
            return temp;
        }
        public static int IsActive(string countSn, string countSys, string dateSys)
        {

            int count1 = 0, count2 = 0;
            DateTime dt = DateTime.Now.Date;
            try
            {
                count1 = Convert.ToInt32(countSn);
                count2 = Convert.ToInt32(countSys) - count1;
                DateTime date = Convert.ToDateTime(dateSys).Date;
                if (count2 > 0 && dt < date)
                {
                    Program.IsActived = true;
                    if (dt >= date.AddDays(-3))
                    {
                        MessageBox.Show("你的试用天数已不足3天！","信息提示");                        
                    }    
                    else if (count2 < 5)
                    {
                        MessageBox.Show("你的试用次数已不足5次！", "信息提示");  
                    }                                   
                }
            }
            catch
            {
            }
            count1 +=1;
            return count1;
        }
        private static string GetVersion()
        {
            //string version = "0.0.0.1";
            //Process current = Process.GetCurrentProcess();
            //FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(current.MainModule.FileName);
            //version = myFileVersionInfo.FileVersion;
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
