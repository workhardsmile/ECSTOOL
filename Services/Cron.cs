using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace ECSTOOL
{
    public class Cron
    {
        //规则文件
        public string crontabPath; //定时任务
        public static Queue<MessagePackage> MessageQueue;
        static LogManager lm;

        public Cron()
        {
            MessageQueue = new Queue<MessagePackage>();
            crontabPath = Path.Combine(Program.startPath,"crontab.lst"); //定时任务
            lm = new LogManager(Path.Combine(Program.startPath, "Log\\"+ LogFile.Cron + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
        }
        public Cron(string project)
        {
             MessageQueue = new Queue<MessagePackage>();
             crontabPath = Path.Combine(Program.startPath, project + "\\crontab.lst"); //定时任务
             lm = new LogManager(Path.Combine(Program.startPath, "Log\\" + project + "." + LogFile.Cron + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
        }
        private List<string> Rules
        {
            get
            {
                List<string> rules = new List<string>();
                StreamReader sr = new StreamReader(crontabPath);
                string rule;
                do
                {
                    rule = sr.ReadLine();
                    if (!string.IsNullOrEmpty(rule))
                    {
                        rules.Add(rule);
                    }
                } while (!string.IsNullOrEmpty(rule));
                sr.Close();
                return rules;
            }
        }
        /// <summary>
        /// 服务启动
        /// </summary>
        /// <param name="args"></param>
        public void OnStart()
        {
            lm.WriteLog(string.Format(@"Cron for Windows by http://wangheng.org : Service Started at [{0}]", DateTime.Now.ToString()));
            Timer timer;
            TimeSpan interval = TimeSpan.FromMinutes(1);
            timer = new Timer(new TimerCallback(obj => RefreshTask()), null, interval, interval);
            RefreshTask();
        }
        /// <summary>
        /// 服务停止
        /// </summary>
        public void OnStop()
        {
            lm.WriteLog(string.Format(@"Cron for Windows by http://wangheng.org : Service Stoped at [{0}]", DateTime.Now.ToString()));
        }
        /// <summary>
        /// 刷新并判断是否有要运行的规则
        /// </summary>
        public void RefreshTask()
        {
            try
            {
                foreach (var rule in Rules)
                {
                    if (BingoToRun(rule))
                    {
                        //System.Diagnostics.Process.Start(@"c:\ftp\ftp_backup.bat");
                        //=======================================================================
                        //System.Diagnostics.Process.Start(rule.Split(' ')[5].ToString());//定时任务
                        //=======================================================================
                        string _cmd=rule.Split(new char[]{'|'},StringSplitOptions.RemoveEmptyEntries)[5].ToString();
                        MessageType.AddMessageQueue(ref MessageQueue, "127.0.0.1:" + _cmd, MessageType.COMMAND_ENGION, "127.0.0.1");
                        lm.WriteLog("=============================================\n");
                        lm.WriteLog(string.Format("Program or Command \"{0}\" Started at [{1}].", _cmd, DateTime.Now.ToString()));
                        lm.WriteLog("=============================================\n");
                    }
                    else
                    {
                        lm.WriteLog(string.Format("The time is {0}. Nothing to do...", DateTime.Now));
                    }
                }
            }
            catch (Exception e)
            {
                lm.WriteLog("================ Error Message===================\n");
                lm.WriteLog(e.Message);
                lm.WriteLog("=============================================\n");
            }
        }
        /// <summary>
        /// 判断是否满足运行条件，JianQu是DateTime的扩展方法
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private bool BingoToRun(string rule)
        {
            //string[] args = Regex.Split(rule, "");
            string[] args = rule.Split(new char[]{'|'},StringSplitOptions.RemoveEmptyEntries);
            if (DateTime.Now.JianQu(args[0], args[1], args[2], args[3], args[4]))
                return true;
            else
                return false;
        }
    }
}
