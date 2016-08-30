using WeifenLuo.WinFormsUI.Docking;
namespace ECSTOOL
{
    partial class ServerList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerList));
            this.listViewServer = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuServer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.executeCMDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageStatusList = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripRefresh = new System.Windows.Forms.ToolStripButton();
            this.connectStripButton = new System.Windows.Forms.ToolStripButton();
            this.disconnectStripButton = new System.Windows.Forms.ToolStripButton();
            this.panelServer = new System.Windows.Forms.Panel();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupOS = new System.Windows.Forms.GroupBox();
            this.textPath = new System.Windows.Forms.TextBox();
            this.labelPath = new System.Windows.Forms.Label();
            this.textPwd = new System.Windows.Forms.TextBox();
            this.labelPwd = new System.Windows.Forms.Label();
            this.labelUser = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.textPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboType = new System.Windows.Forms.ComboBox();
            this.textIP = new System.Windows.Forms.TextBox();
            this.textGroup = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.编辑配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuServer.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.panelServer.SuspendLayout();
            this.groupOS.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewServer
            // 
            this.listViewServer.BackColor = System.Drawing.SystemColors.Window;
            this.listViewServer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewServer.ContextMenuStrip = this.contextMenuServer;
            this.listViewServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewServer.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listViewServer.FullRowSelect = true;
            this.listViewServer.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewServer.LargeImageList = this.imageStatusList;
            this.listViewServer.Location = new System.Drawing.Point(0, 25);
            this.listViewServer.Name = "listViewServer";
            this.listViewServer.Scrollable = false;
            this.listViewServer.ShowItemToolTips = true;
            this.listViewServer.Size = new System.Drawing.Size(291, 599);
            this.listViewServer.SmallImageList = this.imageStatusList;
            this.listViewServer.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listViewServer.TabIndex = 1;
            this.listViewServer.TileSize = new System.Drawing.Size(500, 36);
            this.listViewServer.UseCompatibleStateImageBehavior = false;
            this.listViewServer.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "IP";
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Home";
            this.columnHeader3.Width = 200;
            // 
            // contextMenuServer
            // 
            this.contextMenuServer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disConnectToolStripMenuItem,
            this.uploadFileToolStripMenuItem,
            this.downloadFileToolStripMenuItem,
            this.executeCMDToolStripMenuItem,
            this.编辑配置ToolStripMenuItem,
            this.AddServerToolStripMenuItem,
            this.deleteServerToolStripMenuItem});
            this.contextMenuServer.Name = "contextMenuServer";
            this.contextMenuServer.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuServer.Size = new System.Drawing.Size(153, 202);
            this.contextMenuServer.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuServer_Opening);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.connectToolStripMenuItem.Text = "连接";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disConnectToolStripMenuItem
            // 
            this.disConnectToolStripMenuItem.Enabled = false;
            this.disConnectToolStripMenuItem.Name = "disConnectToolStripMenuItem";
            this.disConnectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.disConnectToolStripMenuItem.Text = "断开";
            this.disConnectToolStripMenuItem.Click += new System.EventHandler(this.disConnectToolStripMenuItem_Click);
            // 
            // uploadFileToolStripMenuItem
            // 
            this.uploadFileToolStripMenuItem.Name = "uploadFileToolStripMenuItem";
            this.uploadFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.uploadFileToolStripMenuItem.Text = "发送文件";
            this.uploadFileToolStripMenuItem.Click += new System.EventHandler(this.uploadFileToolStripMenuItem_Click);
            // 
            // downloadFileToolStripMenuItem
            // 
            this.downloadFileToolStripMenuItem.Name = "downloadFileToolStripMenuItem";
            this.downloadFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.downloadFileToolStripMenuItem.Text = "下载文件";
            this.downloadFileToolStripMenuItem.Click += new System.EventHandler(this.downloadFileToolStripMenuItem_Click);
            // 
            // executeCMDToolStripMenuItem
            // 
            this.executeCMDToolStripMenuItem.Name = "executeCMDToolStripMenuItem";
            this.executeCMDToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.executeCMDToolStripMenuItem.Text = "执行命令";
            this.executeCMDToolStripMenuItem.Click += new System.EventHandler(this.executeCMDToolStripMenuItem_Click);
            // 
            // AddServerToolStripMenuItem
            // 
            this.AddServerToolStripMenuItem.Name = "AddServerToolStripMenuItem";
            this.AddServerToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.AddServerToolStripMenuItem.Text = "新增服务器";
            this.AddServerToolStripMenuItem.Click += new System.EventHandler(this.AddServerToolStripMenuItem_Click);
            // 
            // deleteServerToolStripMenuItem
            // 
            this.deleteServerToolStripMenuItem.Name = "deleteServerToolStripMenuItem";
            this.deleteServerToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteServerToolStripMenuItem.Text = "删除服务器";
            this.deleteServerToolStripMenuItem.Click += new System.EventHandler(this.deleteServerToolStripMenuItem_Click);
            // 
            // imageStatusList
            // 
            this.imageStatusList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageStatusList.ImageStream")));
            this.imageStatusList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageStatusList.Images.SetKeyName(0, "OFFLINE.ICO");
            this.imageStatusList.Images.SetKeyName(1, "ONLINE.ICO");
            this.imageStatusList.Images.SetKeyName(2, "OnLineBusy.ico");
            // 
            // menuStrip
            // 
            this.menuStrip.BackgroundImage = global::ECSTOOL.Properties.Resources.menu;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripRefresh,
            this.connectStripButton,
            this.disconnectStripButton});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip.Size = new System.Drawing.Size(291, 25);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "toolStrip1";
            // 
            // toolStripRefresh
            // 
            this.toolStripRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripRefresh.Image")));
            this.toolStripRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRefresh.Name = "toolStripRefresh";
            this.toolStripRefresh.Size = new System.Drawing.Size(52, 22);
            this.toolStripRefresh.Text = "重载";
            this.toolStripRefresh.Click += new System.EventHandler(this.toolStripRefresh_Click);
            // 
            // connectStripButton
            // 
            this.connectStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.connectStripButton.Image = ((System.Drawing.Image)(resources.GetObject("connectStripButton.Image")));
            this.connectStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.connectStripButton.Name = "connectStripButton";
            this.connectStripButton.Size = new System.Drawing.Size(76, 22);
            this.connectStripButton.Text = "全部连接";
            this.connectStripButton.Click += new System.EventHandler(this.connectStripButton_Click);
            // 
            // disconnectStripButton
            // 
            this.disconnectStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.disconnectStripButton.Image = ((System.Drawing.Image)(resources.GetObject("disconnectStripButton.Image")));
            this.disconnectStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.disconnectStripButton.Name = "disconnectStripButton";
            this.disconnectStripButton.Size = new System.Drawing.Size(76, 22);
            this.disconnectStripButton.Text = "全部断开";
            this.disconnectStripButton.Click += new System.EventHandler(this.disconnectStripButton_Click);
            // 
            // panelServer
            // 
            this.panelServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelServer.BackColor = System.Drawing.SystemColors.Window;
            this.panelServer.Controls.Add(this.buttonReset);
            this.panelServer.Controls.Add(this.buttonOK);
            this.panelServer.Controls.Add(this.groupOS);
            this.panelServer.Controls.Add(this.groupBox1);
            this.panelServer.Location = new System.Drawing.Point(12, 305);
            this.panelServer.Name = "panelServer";
            this.panelServer.Size = new System.Drawing.Size(267, 307);
            this.panelServer.TabIndex = 3;
            this.panelServer.Visible = false;
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(208, 274);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(50, 23);
            this.buttonReset.TabIndex = 8;
            this.buttonReset.Text = "关闭";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(142, 274);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(50, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupOS
            // 
            this.groupOS.Controls.Add(this.textPath);
            this.groupOS.Controls.Add(this.labelPath);
            this.groupOS.Controls.Add(this.textPwd);
            this.groupOS.Controls.Add(this.labelPwd);
            this.groupOS.Controls.Add(this.labelUser);
            this.groupOS.Controls.Add(this.textUser);
            this.groupOS.Controls.Add(this.textPort);
            this.groupOS.Controls.Add(this.labelPort);
            this.groupOS.Location = new System.Drawing.Point(3, 120);
            this.groupOS.Name = "groupOS";
            this.groupOS.Size = new System.Drawing.Size(261, 148);
            this.groupOS.TabIndex = 5;
            this.groupOS.TabStop = false;
            this.groupOS.Text = "Linux服务器";
            this.groupOS.Visible = false;
            // 
            // textPath
            // 
            this.textPath.Location = new System.Drawing.Point(79, 113);
            this.textPath.Name = "textPath";
            this.textPath.Size = new System.Drawing.Size(176, 21);
            this.textPath.TabIndex = 11;
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(10, 116);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(53, 12);
            this.labelPath.TabIndex = 10;
            this.labelPath.Text = "开放目录";
            // 
            // textPwd
            // 
            this.textPwd.Location = new System.Drawing.Point(79, 83);
            this.textPwd.Name = "textPwd";
            this.textPwd.PasswordChar = '*';
            this.textPwd.Size = new System.Drawing.Size(176, 21);
            this.textPwd.TabIndex = 9;
            // 
            // labelPwd
            // 
            this.labelPwd.AutoSize = true;
            this.labelPwd.Location = new System.Drawing.Point(11, 86);
            this.labelPwd.Name = "labelPwd";
            this.labelPwd.Size = new System.Drawing.Size(47, 12);
            this.labelPwd.TabIndex = 8;
            this.labelPwd.Text = "密   码";
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(11, 55);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(41, 12);
            this.labelUser.TabIndex = 7;
            this.labelUser.Text = "用户名";
            // 
            // textUser
            // 
            this.textUser.Location = new System.Drawing.Point(79, 52);
            this.textUser.Name = "textUser";
            this.textUser.Size = new System.Drawing.Size(176, 21);
            this.textUser.TabIndex = 6;
            // 
            // textPort
            // 
            this.textPort.Location = new System.Drawing.Point(79, 20);
            this.textPort.Name = "textPort";
            this.textPort.Size = new System.Drawing.Size(176, 21);
            this.textPort.TabIndex = 5;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(11, 23);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(47, 12);
            this.labelPort.TabIndex = 4;
            this.labelPort.Text = "SSH端口";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboType);
            this.groupBox1.Controls.Add(this.textIP);
            this.groupBox1.Controls.Add(this.textGroup);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 111);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "添加服务器";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "系统类型";
            // 
            // comboType
            // 
            this.comboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboType.FormattingEnabled = true;
            this.comboType.Items.AddRange(new object[] {
            "Linux",
            "Windows"});
            this.comboType.Location = new System.Drawing.Point(79, 84);
            this.comboType.Name = "comboType";
            this.comboType.Size = new System.Drawing.Size(176, 20);
            this.comboType.TabIndex = 4;
            this.comboType.SelectedIndexChanged += new System.EventHandler(this.comboType_SelectedIndexChanged);
            // 
            // textIP
            // 
            this.textIP.Location = new System.Drawing.Point(79, 20);
            this.textIP.Name = "textIP";
            this.textIP.Size = new System.Drawing.Size(176, 21);
            this.textIP.TabIndex = 1;
            // 
            // textGroup
            // 
            this.textGroup.Location = new System.Drawing.Point(79, 52);
            this.textGroup.Name = "textGroup";
            this.textGroup.Size = new System.Drawing.Size(176, 21);
            this.textGroup.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP地址";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "项目分组";
            // 
            // 编辑配置ToolStripMenuItem
            // 
            this.编辑配置ToolStripMenuItem.Name = "编辑配置ToolStripMenuItem";
            this.编辑配置ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.编辑配置ToolStripMenuItem.Text = "编辑配置";
            this.编辑配置ToolStripMenuItem.Click += new System.EventHandler(this.编辑配置ToolStripMenuItem_Click);
            // 
            // ServerList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 624);
            this.CloseButton = false;
            this.CloseButtonVisible = false;
            this.Controls.Add(this.panelServer);
            this.Controls.Add(this.listViewServer);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ServerList";
            this.Text = "服务器集群";
            this.Shown += new System.EventHandler(this.ServerList_Shown);
            this.contextMenuServer.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panelServer.ResumeLayout(false);
            this.groupOS.ResumeLayout(false);
            this.groupOS.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewServer;
        private System.Windows.Forms.ToolStrip menuStrip;
        private System.Windows.Forms.ToolStripButton connectStripButton;
        private System.Windows.Forms.ToolStripButton disconnectStripButton;
        private System.Windows.Forms.ImageList imageStatusList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ContextMenuStrip contextMenuServer;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disConnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem executeCMDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripRefresh;
        private System.Windows.Forms.Panel panelServer;
        private System.Windows.Forms.TextBox textIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupOS;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboType;
        private System.Windows.Forms.TextBox textGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textPwd;
        private System.Windows.Forms.Label labelPwd;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.TextBox textUser;
        private System.Windows.Forms.TextBox textPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.ToolStripMenuItem 编辑配置ToolStripMenuItem;
    }
}