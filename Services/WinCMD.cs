using System;
using System.Diagnostics;
using System.Threading;

namespace ECSTOOL
{
    /// <summary> 
    /// Command 的摘要说明。 
    /// </summary> 
    public class WinCMD
    {
        public string ExePath;
        public string Command;
        public string Argument = "";
        public string Return;
        public int Second;
        /// <summary>
        /// 线程启动软件
        /// </summary>
        public void StartProgram()
        {
            StartExe(ExePath, Argument);
        }
        /// <summary>
        /// 线程执行DOS命令
        /// </summary>
        public void Execute()
        {
            Return = "";
            Return = Execute(Command, Second);
            //RunCmd(Command);
        }
        /// <summary>
        /// dosCommand Dos命令语句
        /// </summary>
        /// <param name="dosCommand"></param>
        /// <returns></returns>   
        public string Execute(string dosCommand)
        {
            return Execute(dosCommand, 300);
        }
        /// <summary>   
        /// 执行DOS命令，返回DOS命令的输出   
        /// </summary>   
        /// <param name="dosCommand">dos命令</param>   
        /// <param name="milliseconds">等待命令执行的时间（单位：秒），   
        /// 如果设定为0，则无限等待</param>   
        /// <returns>返回DOS命令的输出</returns>   
        public string Execute(string command, int seconds)
        {
            string output = "timeout!"; //输出字符串   
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象   
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令   
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出   
                //startInfo.Arguments = command;
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动   
                startInfo.RedirectStandardInput = false;//不重定向输入   
                startInfo.RedirectStandardOutput = true; //重定向输出   
                startInfo.CreateNoWindow = true;//不创建窗口   
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程   
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束   
                        }
                        else
                        {
                            process.WaitForExit(seconds * 1000); //等待进程结束，等待时间为指定的毫秒   
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        public string cmd_str = "";
        public string cmd_outstr = "";
        /// <summary> 
        /// 执行CMD语句 
        /// </summary> 
        /// <param name="cmd">要执行的CMD命令</param> 
        public void RunCmd()
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            //if (!cmd_str.Contains("exit"))
            //{
            //    cmd_str = cmd_str + " && exit";
            //}
            p.StandardInput.WriteLine(cmd_str);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
            cmd_outstr = p.StandardOutput.ReadToEnd();
            p.Close();
        }
        /// <summary> 
        /// 执行CMD语句 
        /// </summary> 
        /// <param name="cmd">要执行的CMD命令</param> 
        public string RunCmd(string cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            //if (!cmd.Contains("exit"))
            //{
            //    cmd = cmd + " && exit";
            //}
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
            string temp = p.StandardOutput.ReadToEnd();
            p.Close();
            return temp;
        }
        /// <summary> 
        /// 打开软件 
        /// </summary> 
        /// <param name="exePath">软件路径加名称（.exe文件）</param> 
        public void RunProgram(string exePath)
        {
            RunProgram(exePath, "");
        }
        /// <summary> 
        /// 打开软件并执行命令（输入) 
        /// </summary> 
        /// <param name="exePath">软件路径加名称（.exe文件）</param> 
        /// <param name="cmd">要执行的命令</param> 
        public void RunProgram(string exePath, string cmd)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = exePath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            if (cmd.Length != 0)
            {
                proc.StandardInput.WriteLine(cmd);
            }
            proc.Close();
        }
        /// <summary>
        /// 打开软件
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public string StartExe(string exePath)
        {
            string output = "";
            try
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = exePath;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.Start();
                output = cmd.StandardOutput.ReadToEnd();
                //Console.WriteLine(output);
                cmd.WaitForExit();
                cmd.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return output;
        }
        /// <summary>
        /// 打开软件（内置参数）
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public string StartExe(string exePath, string argument)
        {
            string output = "";
            try
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = exePath;
                cmd.StartInfo.Arguments = argument;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.Start();
                output = cmd.StandardOutput.ReadToEnd();
                //Console.WriteLine(output);
                cmd.WaitForExit();
                cmd.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return output;
        }
    }
}
