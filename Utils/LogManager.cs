using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ECSTOOL
{
    public class LogManager
    {
        //<summary>
        //保存日志的文件夹
        //<summary>
        private string logPath = string.Empty;
        public string LogPath
        {
            get
            {
                if (logPath == string.Empty)
                {
                    if (AppDomain.CurrentDomain.BaseDirectory != null)
                    {
                        // winfrom 应用
                        logPath = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    else
                    {
                        // web应用
                        logPath = AppDomain.CurrentDomain.BaseDirectory + @"bin\";
                    }
                }
                return logPath;
            }
            set 
            { 
                logPath = value;
            }
        }
        public LogManager(string logPath)
        {
            this.logPath = logPath;
            //if (!File.Exists(logPath))
            //{
            //    File.CreateText(logPath);
            //} 
        }
        //<summary>
        //写日志
        //<summary>
        public void WriteLog(string msg)
        {
            try
            {                               
                StreamWriter sw = File.AppendText(logPath);
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        //<summary>
        //写日志
        //<summary>
        public void WriteLog(string logPath,string msg)
        {
            try
            {
                StreamWriter sw = File.AppendText(logPath);
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    //<summary>
    //日志类型
    //<summary>
    public enum LogFile
    {
        Connect,
        Socket,
        Server,
        File,
        CmdOutput,
        Lua,
        Trace,
        Warning,
        Error,
        Mail,
        Cron
    }
}
