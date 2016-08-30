using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Reflection;
using PlugsRoot;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ECSTOOL
{
    public delegate void DelegateClosePlug(object sender, EventCloseForm e);
    public partial class PlugsForm : DockContent
    {
        private IApplication application = null;
        private string pluginpath = Path.Combine(Application.StartupPath, "Plugs");
        public event DelegateClosePlug DelagateClosePlugF; //声明事件

        public PlugsForm()
        {
            InitializeComponent();
        }
        public PlugsForm(IApplication appli)
        {
            InitializeComponent();
            application = appli;
        }
        private void PlugForm_Load(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(pluginpath);
            if (!di.Exists)
            {
                di.Create();
            }
            FileInfo[] plugins = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            //IEnumerable<FileInfo> plugins = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Distinct<FileInfo>();
            foreach (FileInfo plugin in plugins)
            {
                FindPluginAssembly(plugin);
            }
            plugins = di.GetFiles("*.exe", SearchOption.TopDirectoryOnly);
            //IEnumerable<FileInfo> plugins = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Distinct<FileInfo>();
            foreach (FileInfo plugin in plugins)
            {
                LoadExe(plugin);
            }
        }
        void LoadExe(FileInfo file)
        {
            int i = 0;
            IntPtr hIcon = FileIcon.ExtractAssociatedIcon(this.Handle, file.FullName, ref i);
            Image img = Icon.FromHandle(hIcon).ToBitmap();
            //ExtractIcon.GetIcon(file.FullName, false).ToBitmap();
            //FileIcon.GetFileIcon(file.FullName, true).ToBitmap();
            this.imageList.Images.Add(file.Name, img);

            string name = file.Name.Remove(file.Name.LastIndexOf("."));
            string group = "独立工具";
            ListViewItem item = new ListViewItem(name);

            if (this.listViewTool.Groups[group] != null)
            {
                item.Group = this.listViewTool.Groups[group];
            }
            else
            {
                this.listViewTool.Groups.Add(new ListViewGroup(group, group));
                item.Group = this.listViewTool.Groups[group];
            }
            item.ToolTipText = file.Name;
            item.ImageKey = file.Name;
            listViewTool.Items.Add(item);
        }
        void FindPluginAssembly(FileInfo file)
        {
            Assembly assembly = System.Reflection.Assembly.LoadFile(file.FullName);
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("IPlugin") != null)
                {
                    PluginInfoAttribute infop = (PluginInfoAttribute)(TypeDescriptor.GetAttributes(type)[typeof(PluginInfoAttribute)]);
                    if (infop != null)
                    {
                        int i = 0;
                        IntPtr hIcon = FileIcon.ExtractAssociatedIcon(this.Handle, file.FullName, ref i);
                        Image img = Icon.FromHandle(hIcon).ToBitmap();
                        //ExtractIcon.GetIcon(file.FullName, false).ToBitmap();
                        //FileIcon.GetFileIcon(file.FullName, true).ToBitmap();
                        this.imageList.Images.Add(file.Name, img);

                        ListViewItem item = new ListViewItem(infop.Name);
                        item.SubItems.Add(type.FullName);
                        item.SubItems.Add(infop.Version);
                        item.SubItems.Add(infop.Author);
                        if (this.listViewTool.Groups[infop.Group] != null)
                        {
                            item.Group = this.listViewTool.Groups[infop.Group];
                        }
                        else
                        {
                            this.listViewTool.Groups.Add(new ListViewGroup(infop.Group, infop.Group));
                            item.Group = this.listViewTool.Groups[infop.Group];
                        }
                        item.ToolTipText = "Author: " + infop.Author + "\nVersion: " + infop.Version;
                        item.ImageKey = file.Name;
                        listViewTool.Items.Add(item);
                        listViewTool.Refresh();
                        if (infop.LoadWhenStart)
                        {
                            listViewTool.Items[listViewTool.Items.Count - 1].Selected = true;
                            listViewTool_Click(null, null);
                        }
                    }
                }
            }
        }

        private void listViewTool_Click(object sender, EventArgs e)
        {
            if (this.listViewTool.SelectedItems.Count > 0)
            {
                ListViewItem item = this.listViewTool.SelectedItems[0];
                if (!item.Group.Name.Equals("独立工具"))
                {
                    Assembly assembly = Assembly.LoadFile(Path.Combine(pluginpath, item.ImageKey));
                    //Type type = assembly.GetType(item.SubItems[1].Text.Trim());
                    IPlugin instance = (IPlugin)assembly.CreateInstance(item.SubItems[1].Text.Trim());
                    Form idc = FindDocument(instance.GetText());
                    if (idc == null)
                    {
                        instance.Application = application;
                        instance.ShowOwn();
                    }
                    else
                    {
                        idc.Activate();
                        //idc.Show(application.MyDockPanel, DockState.Document);
                    }
                }
                else
                {
                    Process.Start(Path.Combine(pluginpath, item.ImageKey));
                }
            }
        }
        /// 在dockPanel中查找已经打开的窗口
        /// </summary>
        /// <param name="text">传入的窗口标题</param>
        /// <returns>返回的窗口</returns>
        private Form FindDocument(string text)
        {
            if (application.MyDockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in this.MdiParent.MdiChildren)
                    if (form.Text == text)
                        return form as DockContent;

                return null;
            }
            else
            {
                foreach (Form form in application.MyDockPanel.FloatWindows)
                    if (form.Text == text)
                        return form;
                foreach (DockContent content in application.MyDockPanel.Documents)
                    if (content.DockHandler.TabText == text)
                        return content;
                return null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            EventCloseForm E = new EventCloseForm("close");
            this.Invoke(DelagateClosePlugF,null, E);
            //DelagateClosePlugF(null, E);
        }
    }
    public class EventCloseForm : EventArgs
    {
        private string m_value;
        public string M_Value
        {
            get
            {
                return m_value;
            }
            set
            {
                this.m_value = value;
            }
        }
        public EventCloseForm(string arg)
        {
            this.m_value = arg;
        }
    }
    
}
