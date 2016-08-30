using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Text;
using System.ComponentModel;
using System.Net.Mail;
using System.Collections;
using System.Net.Sockets;

namespace ECSTOOL
{
    /// <summary>
    /// PandaHall网站发送Mail功能类
    /// </summary>
    public class MailHelper
    {
        public LogManager lm;
        private string tonames;
        private string server=ECSTOOL.Properties.Settings.Default.SMTPServer;
        private string user = ECSTOOL.Properties.Settings.Default.SMTPServerUser; 
        private string password = ECSTOOL.Properties.Settings.Default.SMTPServerPassword; 

        public MailHelper()
        {
            tonames = ECSTOOL.Properties.Settings.Default.ToNames;
            lm = new LogManager(Path.Combine(Program.startPath, "Log\\" + LogFile.Mail + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log"));
        }
        /// <summary>
        /// 发送Email功能函数
        /// </summary>
        /// <param name="tos">接收人列表</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="attachPath">附件路径</param>
        /// <returns>是否成功</returns>
        public bool SendEmail(string tos, string subject, string body, string attachPath)
        {
            bool flag = true;
            MailSender ms = new MailSender();
            ms.From = this.user;
            //ms.To = "jovenc@citiz.net";
            ms.Subject = subject;
            ms.Body = body;
            ms.UserName = this.user; 
            ms.Password = this.password; 
            ms.Server = this.server;
            ms.Attachments.Add(new MailSender.AttachmentInfo(attachPath));
            string[] Tos = (tonames+";"+tos).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            lm.WriteLog("#####################mail sending...########################");

            foreach (string _name in Tos)
            {
                ms.To = _name;
                try
                {
                    ms.SendMail();
                    lm.WriteLog("sent to [" + ms.To + "] from " + ms.From + " successfully");
                }
                catch(Exception ex)
                {
                    lm.WriteLog("sent to [" + ms.To + "] from " + ms.From + " fail");
                    lm.WriteLog("Exception: " + ex.StackTrace);
                    flag = false;
                }
            }
                return flag;
        }
        /// <summary>
        /// 发送Email功能函数
        /// </summary>
        /// <param name="to">接收人</param>
        /// <param name="cc">抄送</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="isBodyHtml">是否是HTML格式Mail</param>
        /// <returns>是否成功</returns>
        public bool SendEmail(string to, string cc, string subject, string body, bool isBodyHtml)
        {
            try
            {
                //Debug.WriteLine(DateTime.Now);
                //设置smtp
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(this.server);
                //设置帐户
                smtp.Credentials = new NetworkCredential(this.user, this.password);
                //开一个Message
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                //主题
                mail.Subject = subject;
                mail.SubjectEncoding = System.Text.Encoding.GetEncoding("GB2312");
                mail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
                //发送
                mail.From = new System.Net.Mail.MailAddress(this.user);
                //邮件等级
                mail.Priority = System.Net.Mail.MailPriority.High;
                //设置邮件格式
                mail.IsBodyHtml = isBodyHtml;
                //主题内容
                mail.Body = body;
                string[] tos = to.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string _str in tos)
                {
                    //设置发送对象
                    mail.To.Add(_str);
                }

                //如果有抄送人就抄送,没有就不送
                if (cc != null && cc != string.Empty)
                {
                    tos = cc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string _str in tos)
                    {
                        //抄送
                        mail.CC.Add(_str);
                    }
                }

                //发送Email
                smtp.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);

                object userState = mail;

                smtp.SendAsync(mail, userState);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bool mailSent = true;

            System.Net.Mail.MailMessage mail = (System.Net.Mail.MailMessage)e.UserState;
            string subject = mail.Subject;
            string content = mail.Body;

            if (e.Cancelled)
            {
                mailSent = false;
            }
            if (e.Error != null)
            {
                mailSent = false;
            }
            if (!mailSent)
            {
                string file = "Send Mail/Send Mail test asnc Error " + System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                StringBuilder sb = new StringBuilder();
                sb.Append("Send Failed!  ");
                sb.Append("Subject :" + subject + " ");
                sb.Append("Content :" + content);
                sb.Append("Exception :" + e.Error.ToString());
                lm.WriteLog(sb.ToString());
            }
            else
            {
                string file = "Send Mail/Send Mail test asnc Error " + System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                StringBuilder sb = new StringBuilder();
                sb.Append("Send Sucess!  ");
                sb.Append("Subject :" + subject + " ");
                sb.Append("Content :" + content);
                lm.WriteLog(sb.ToString());
            }

        }

    }
    /// <summary> 
    /// Mail 发送器 
    /// </summary> 
    public class MailSender
    {

        /// <summary> 

        /// SMTP服务器域名 

        /// </summary> 

        public string Server
        {

            get { return server; }

            set { if (value != server) server = value; }

        } private string server = "";


        /// <summary> 

        /// SMTP服务器端口 [默认为25] 

        /// </summary> 

        public int Port
        {

            get { return port; }

            set { if (value != port) port = value; }

        } private int port = 25;


        /// <summary> 

        /// 用户名 [如果需要身份验证的话] 

        /// </summary> 

        public string UserName
        {

            get { return userName; }

            set { if (value != userName) userName = value; }

        } private string userName = "";


        /// <summary> 

        /// 密码 [如果需要身份验证的话] 

        /// </summary> 

        public string Password
        {

            get { return password; }

            set { if (value != password) password = value; }

        } private string password = "";


        /// <summary> 

        /// 发件人地址 

        /// </summary> 

        public string From
        {

            get { return from; }

            set { if (value != from) from = value; }

        } private string from = "";


        /// <summary> 

        /// 收件人地址 

        /// </summary> 

        public string To
        {

            get { return to; }

            set { if (value != to) to = value; }

        } private string to = "";


        /// <summary> 

        /// 发件人姓名 

        /// </summary> 

        public string FromName
        {

            get { return fromName; }

            set { if (value != fromName) fromName = value; }

        } private string fromName = "";


        /// <summary> 

        /// 收件人姓名 

        /// </summary> 

        public string ToName
        {

            get { return toName; }

            set { if (value != toName) toName = value; }

        } private string toName = "";


        /// <summary> 

        /// 邮件的主题 

        /// </summary> 

        public string Subject
        {

            get { return subject; }

            set { if (value != subject) subject = value; }

        } private string subject = "";


        /// <summary> 

        /// 邮件正文 

        /// </summary> 

        public string Body
        {

            get { return body; }

            set { if (value != body) body = value; }

        } private string body = "";


        /// <summary> 

        /// 超文本格式的邮件正文 

        /// </summary> 

        public string HtmlBody
        {

            get { return htmlBody; }

            set { if (value != htmlBody) htmlBody = value; }

        } private string htmlBody = "";


        /// <summary> 

        /// 是否是html格式的邮件 

        /// </summary> 

        public bool IsHtml
        {

            get { return isHtml; }

            set { if (value != isHtml) isHtml = value; }

        } private bool isHtml = false;


        /// <summary> 

        /// 语言编码 [默认为GB2312] 

        /// </summary> 

        public string LanguageEncoding
        {

            get { return languageEncoding; }

            set { if (value != languageEncoding) languageEncoding = value; }

        } private string languageEncoding = "GB2312";


        /// <summary> 

        /// 邮件编码 [默认为8bit] 

        /// </summary> 

        public string MailEncoding
        {

            get { return encoding; }

            set { if (value != encoding) encoding = value; }

        } private string encoding = "8bit";


        /// <summary> 

        /// 邮件优先级 [默认为3] 

        /// </summary> 

        public int Priority
        {

            get { return priority; }

            set { if (value != priority) priority = value; }

        } private int priority = 3;


        /// <summary> 

        /// 附件 [AttachmentInfo] 

        /// </summary> 

        public IList Attachments
        {

            get { return attachments; }
            // set { if (value != attachments) attachments = value; } 

        } private ArrayList attachments = new ArrayList();

        /// <summary> 

        /// 发送邮件 

        /// </summary> 

        public void SendMail()
        {

            // 创建TcpClient对象， 并建立连接 

            TcpClient tcp = null;

            try
            {

                tcp = new TcpClient(server, port);

            }

            catch (Exception)
            {

                throw new Exception("无法连接服务器");

            }


            ReadString(tcp.GetStream());//获取连接信息 


            // 开始进行服务器认证 

            // 如果状态码是250则表示操作成功 

            if (!Command(tcp.GetStream(), "EHLO Localhost", "250"))

                throw new Exception("登陆阶段失败");


            if (userName != "")
            {

                // 需要身份验证 

                if (!Command(tcp.GetStream(), "AUTH LOGIN", "334"))

                    throw new Exception("身份验证阶段失败");

                string nameB64 = ToBase64(userName); // 此处将username转换为Base64码 

                if (!Command(tcp.GetStream(), nameB64, "334"))

                    throw new Exception("身份验证阶段失败");

                string passB64 = ToBase64(password); // 此处将password转换为Base64码 

                if (!Command(tcp.GetStream(), passB64, "235"))

                    throw new Exception("身份验证阶段失败");

            }

            // 准备发送 

            WriteString(tcp.GetStream(), "mail From: " + from);

            WriteString(tcp.GetStream(), "rcpt to: " + to);

            WriteString(tcp.GetStream(), "data");


            // 发送邮件头 

            WriteString(tcp.GetStream(), "Date: " + DateTime.Now); // 时间 

            WriteString(tcp.GetStream(), "From: " + fromName + "<" + from + ">"); // 发件人 

            WriteString(tcp.GetStream(), "Subject: " + subject); // 主题 

            WriteString(tcp.GetStream(), "To:" + toName + "<" + to + ">"); // 收件人 


            //邮件格式 

            WriteString(tcp.GetStream(), "Content-Type: multipart/mixed; boundary=\"unique-boundary-1\"");

            WriteString(tcp.GetStream(), "Reply-To:" + from); // 回复地址 

            WriteString(tcp.GetStream(), "X-Priority:" + priority); // 优先级 

            WriteString(tcp.GetStream(), "MIME-Version:1.0"); // MIME版本 


            // 数据ID,随意 
            // WriteString (tcp.GetStream(), "Message-Id: " + DateTime.Now.ToFileTime() + "@security.com"); 

            WriteString(tcp.GetStream(), "Content-Transfer-Encoding:" + encoding); // 内容编码 

            WriteString(tcp.GetStream(), "X-Mailer:JcPersonal.Utility.MailSender"); // 邮件发送者 

            WriteString(tcp.GetStream(), "");


            WriteString(tcp.GetStream(), ToBase64("This is a multi-part message in MIME format."));

            WriteString(tcp.GetStream(), "");


            // 从此处开始进行分隔输入 

            WriteString(tcp.GetStream(), "--unique-boundary-1");


            // 在此处定义第二个分隔符 

            WriteString(tcp.GetStream(), "Content-Type: multipart/alternative;Boundary=\"unique-boundary-2\"");

            WriteString(tcp.GetStream(), "");


            if (!isHtml)
            {

                // 文本信息 

                WriteString(tcp.GetStream(), "--unique-boundary-2");

                WriteString(tcp.GetStream(), "Content-Type: text/plain;charset=" + languageEncoding);

                WriteString(tcp.GetStream(), "Content-Transfer-Encoding:" + encoding);

                WriteString(tcp.GetStream(), "");

                WriteString(tcp.GetStream(), body);

                WriteString(tcp.GetStream(), "");//一个部分写完之后就写如空信息，分段 

                WriteString(tcp.GetStream(), "--unique-boundary-2--");//分隔符的结束符号，尾巴后面多了-- 

                WriteString(tcp.GetStream(), "");

            }

            else
            {

                //html信息 

                WriteString(tcp.GetStream(), "--unique-boundary-2");

                WriteString(tcp.GetStream(), "Content-Type: text/html;charset=" + languageEncoding);

                WriteString(tcp.GetStream(), "Content-Transfer-Encoding:" + encoding);

                WriteString(tcp.GetStream(), "");

                WriteString(tcp.GetStream(), htmlBody);

                WriteString(tcp.GetStream(), "");

                WriteString(tcp.GetStream(), "--unique-boundary-2--");//分隔符的结束符号，尾巴后面多了-- 

                WriteString(tcp.GetStream(), "");

            }


            // 发送附件 

            // 对文件列表做循环 

            for (int i = 0; i < attachments.Count; i++)
            {

                WriteString(tcp.GetStream(), "--unique-boundary-1"); // 邮件内容分隔符 

                WriteString(tcp.GetStream(), "Content-Type: application/octet-stream;name=\"" + ((AttachmentInfo)attachments[i]).FileName + "\""); // 文件格式 

                WriteString(tcp.GetStream(), "Content-Transfer-Encoding: base64"); // 内容的编码 

                WriteString(tcp.GetStream(), "Content-Disposition:attachment;filename=\"" + ((AttachmentInfo)attachments[i]).FileName + "\""); // 文件名 

                WriteString(tcp.GetStream(), "");

                WriteString(tcp.GetStream(), ((AttachmentInfo)attachments[i]).Bytes); // 写入文件的内容 

                WriteString(tcp.GetStream(), "");

            }


            Command(tcp.GetStream(), ".", "250"); // 最后写完了，输入"." 


            // 关闭连接 

            tcp.Close();

        }


        /// <summary> 

        /// 向流中写入字符 

        /// </summary> 

        /// <param name="netStream">来自TcpClient的流</param> 

        /// <param name="str">写入的字符</param> 

        protected void WriteString(NetworkStream netStream, string str)
        {

            str = str + "\r\n"; // 加入换行符 


            // 将命令行转化为byte[] 

            byte[] bWrite = Encoding.GetEncoding(languageEncoding).GetBytes(str.ToCharArray());


            // 由于每次写入的数据大小是有限制的，那么我们将每次写入的数据长度定在７５个字节，一旦命令长度超过了７５，就分步写入。 

            int start = 0;

            int length = bWrite.Length;

            int page = 0;

            int size = 75;

            int count = size;

            try
            {

                if (length > 75)
                {

                    // 数据分页 

                    if ((length / size) * size < length)

                        page = length / size + 1;

                    else

                        page = length / size;

                    for (int i = 0; i < page; i++)
                    {

                        start = i * size;

                        if (i == page - 1)

                            count = length - (i * size);

                        netStream.Write(bWrite, start, count);// 将数据写入到服务器上 

                    }

                }

                else

                    netStream.Write(bWrite, 0, bWrite.Length);

            }

            catch (Exception)
            {

                // 忽略错误 

            }

        }


        /// <summary> 

        /// 从流中读取字符 

        /// </summary> 

        /// <param name="netStream">来自TcpClient的流</param> 

        /// <returns>读取的字符</returns> 

        protected string ReadString(NetworkStream netStream)
        {

            string sp = null;

            byte[] by = new byte[1024];

            int size = netStream.Read(by, 0, by.Length);// 读取数据流 

            if (size > 0)
            {

                sp = Encoding.Default.GetString(by);// 转化为String 

            }

            return sp;

        }


        /// <summary> 

        /// 发出命令并判断返回信息是否正确 

        /// </summary> 

        /// <param name="netStream">来自TcpClient的流</param> 

        /// <param name="command">命令</param> 

        /// <param name="state">正确的状态码</param> 

        /// <returns>是否正确</returns> 

        protected bool Command(NetworkStream netStream, string command, string state)
        {

            string sp = null;

            bool success = false;

            try
            {

                WriteString(netStream, command);// 写入命令 

                sp = ReadString(netStream);// 接受返回信息 

                if (sp.IndexOf(state) != -1)// 判断状态码是否正确 

                    success = true;

            }

            catch (Exception)
            {

                // 忽略错误 

            }

            return success;

        }


        /// <summary> 

        /// 字符串编码为Base64 

        /// </summary> 

        /// <param name="str">字符串</param> 

        /// <returns>Base64编码的字符串</returns> 

        protected string ToBase64(string str)
        {

            try
            {

                byte[] by = Encoding.Default.GetBytes(str.ToCharArray());

                str = Convert.ToBase64String(by);

            }

            catch (Exception)
            {

                // 忽略错误 

            }

            return str;

        }


        /// <summary> 

        /// 附件信息 

        /// </summary> 

        public struct AttachmentInfo
        {

            /// <summary>
            /// 附件的文件名 [如果输入路径，则自动转换为文件名]
            /// </summary> 

            public string FileName
            {

                get { return fileName; }

                set { fileName = Path.GetFileName(value); }

            } private string fileName;


            /// <summary> 

            /// 附件的内容 [由经Base64编码的字节组成] 

            /// </summary> 

            public string Bytes
            {

                get { return bytes; }

                set { if (value != bytes) bytes = value; }

            } private string bytes;


            /// <summary> 

            /// 从流中读取附件内容并构造 

            /// </summary> 

            /// <param name="ifileName">附件的文件名</param> 

            /// <param name="stream">流</param> 

            public AttachmentInfo(string ifileName, Stream stream)
            {

                fileName = Path.GetFileName(ifileName);

                byte[] by = new byte[stream.Length];

                stream.Read(by, 0, (int)stream.Length); // 读取文件内容 

                //格式转换 

                bytes = Convert.ToBase64String(by); // 转化为base64编码 

            }


            /// <summary> 

            /// 按照给定的字节构造附件 

            /// </summary> 

            /// <param name="ifileName">附件的文件名</param> 

            /// <param name="ibytes">附件的内容 [字节]</param> 

            public AttachmentInfo(string ifileName, byte[] ibytes)
            {

                fileName = Path.GetFileName(ifileName);

                bytes = Convert.ToBase64String(ibytes); // 转化为base64编码 

            }


            /// <summary> 

            /// 从文件载入并构造 

            /// </summary> 

            /// <param name="path"></param> 

            public AttachmentInfo(string path)
            {

                fileName = Path.GetFileName(path);

                FileStream file = new FileStream(path, FileMode.Open);

                byte[] by = new byte[file.Length];

                file.Read(by, 0, (int)file.Length); // 读取文件内容 

                //格式转换 

                bytes = Convert.ToBase64String(by); // 转化为base64编码 

                file.Close();

            }

        }
    }


    //// 使用: 


    //  MailSender ms = new MailSender (); 

    //  ms.From = "jovenc@tom.com"; 

    //  ms.To = "jovenc@citiz.net"; 

    //  ms.Subject = "Subject"; 

    //  ms.Body = "body text"; 

    //  ms.UserName = "########"; // 怎么能告诉你呢 

    //  ms.Password = "********"; // 怎么能告诉你呢 

    //  ms.Server = "smtp.tom.com"; 


    //  ms.Attachments.Add (new MailSender.AttachmentInfo (@"D:\test.txt")); 


    //  Console.WriteLine ("mail sending..."); 

    //  try 

    //  { 

    //      ms.SendMail (); 

    //      Console.WriteLine ("mail sended."); 

    //  } 

    //  catch (Exception e) 

    //  { 

    //      Console.WriteLine (e); 

    //  } 
}
