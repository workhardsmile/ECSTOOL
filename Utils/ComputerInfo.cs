using System;
using System.Management;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Net.Sockets;
//using System.Diagnostics;

namespace ECSTOOL
{
    class ComputerInfo
    {
        //public string CpuID;
        //public string MacAddress;
        //public string DiskID;
        //public string IpAddress;
        //public string LoginUserName;
        //public string ComputerName;
        //public string SystemType;
        //public string TotalPhysicalMemory; //单位：M
        private static ComputerInfo _instance;
        public static ComputerInfo Instance()
        {
            if (_instance == null)
                _instance = new ComputerInfo();
            return _instance;
        }
        public ComputerInfo()
        {
            //CpuID = GetCpuID();
            //MacAddress = GetMacAddress();
            //DiskID = GetDiskID();
            //IpAddress = GetIPAddress();
            //LoginUserName = GetUserName();
            //SystemType = GetSystemType();
            //TotalPhysicalMemory = GetTotalPhysicalMemory();
            //ComputerName = GetComputerName();
        }
        public string GetCpuID()
        {
            try
            {
                //获取CPU序列号代码
                string cpuInfo = "";//cpu序列号
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }
        public static bool IsServer2003()
        {
            //获取系统信息
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            //获取操作系统ID
            System.PlatformID platformID = osInfo.Platform;
            //获取主版本号
            int versionMajor = osInfo.Version.Major;
            //获取副版本号
            int versionMinor = osInfo.Version.Minor;

            if (platformID == PlatformID.Win32NT && versionMajor == 5 && versionMinor == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsBusyPort(int port)
        {
            //Process p = new Process();
            //p.StartInfo = new ProcessStartInfo("netstat", "-a");
            //p.StartInfo.CreateNoWindow = true;
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //p.StartInfo.RedirectStandardOutput = true;
            //p.Start();
            //string result = p.StandardOutput.ReadToEnd();
            //if (result.IndexOf(Environment.MachineName.ToLower() + ":" + port) >= 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //} 
            bool tcpListen = false;
            bool udpListen = false;//设定端口状态标识位
            System.Net.IPAddress myIpAddress = IPAddress.Parse("127.0.0.1");
            System.Net.IPEndPoint myIpEndPoint = new IPEndPoint(myIpAddress, port);
            try
            {
                System.Net.Sockets.TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(myIpEndPoint);//对远程计算机的指定端口提出TCP连接请求
                tcpListen = true;
                tcpClient.Close();
            }
            catch { }
            try
            {
                System.Net.Sockets.UdpClient udpClient = new UdpClient();
                udpClient.Connect(myIpEndPoint);//对远程计算机的指定端口提出UDP连接请求
                udpListen = true;
                udpClient.Close();
            }
            catch { }
            if (tcpListen == false && udpListen == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static IList<string> GetMacList()
        {
            IList<string> mac = new List<string>();
            try
            {
                //获取网卡Mac地址
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac.Add(mo["MacAddress"].ToString());
                        break;
                    }
                }
                moc = null;
            }
            catch
            {
            }
            return mac;
        }
        public string GetMacAddress()
        {
            try
            {
                //获取网卡Mac地址
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        public string GetIPAddress()
        {
            try
            {
                //获取IP地址
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo["IpAddress"].ToString();
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        public string NetIP()
        {
            string strUrl = "http://city.ip138.com/city0.asp"; //获得IP的网址了  
            Uri uri = new Uri(strUrl);
            WebRequest wr = WebRequest.Create(uri);
            Stream s = wr.GetResponse().GetResponseStream();
            StreamReader sr = new StreamReader(s, System.Text.Encoding.Default);
            string all = sr.ReadToEnd(); //读取网站的数据  
            int i = all.IndexOf("[") + 1;
            int length = all.IndexOf("]") - i;
            string tempip = all.Substring(i, length);
            string ip = tempip.Trim();
            return ip;
        }
        public string GetDiskID()
        {
            try
            {
                //获取硬盘ID
                String HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                moc = null;
                mc = null;
                return HDid;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        /// <summary>
        /// 操作系统的登录用户名
        /// </summary>
        /// <returns></returns>
        public string GetUserName()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["UserName"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        /// <summary>
        /// PC类型
        /// </summary>
        /// <returns></returns>
        public string GetSystemType()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["SystemType"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        /// <summary>
        /// 物理内存
        /// </summary>
        /// <returns></returns>
        public string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["TotalPhysicalMemory"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
        /// <summary>
        /// 计算机名称
        /// </summary>
        /// <returns></returns>
        public string GetComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }
    }
}
