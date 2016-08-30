namespace ECSTOOL
{
    partial class ServerOperator
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
            this.comboSourceIP = new System.Windows.Forms.ComboBox();
            this.comboTargetIP = new System.Windows.Forms.ComboBox();
            this.comboTargetDir = new System.Windows.Forms.ComboBox();
            this.listTask = new System.Windows.Forms.ListBox();
            this.comboParam1 = new System.Windows.Forms.ComboBox();
            this.comboSourceFile = new System.Windows.Forms.ComboBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkEnforce = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboParam3 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboParam2 = new System.Windows.Forms.ComboBox();
            this.listBoxValue = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonDel = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonSubmit = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboSourceIP
            // 
            this.comboSourceIP.FormattingEnabled = true;
            this.comboSourceIP.Location = new System.Drawing.Point(71, 19);
            this.comboSourceIP.Name = "comboSourceIP";
            this.comboSourceIP.Size = new System.Drawing.Size(179, 20);
            this.comboSourceIP.TabIndex = 0;
            this.comboSourceIP.SelectedValueChanged += new System.EventHandler(this.comboSourceFile_DropDown);
            // 
            // comboTargetIP
            // 
            this.comboTargetIP.FormattingEnabled = true;
            this.comboTargetIP.Location = new System.Drawing.Point(71, 17);
            this.comboTargetIP.Name = "comboTargetIP";
            this.comboTargetIP.Size = new System.Drawing.Size(179, 20);
            this.comboTargetIP.TabIndex = 1;
            this.comboTargetIP.SelectedIndexChanged += new System.EventHandler(this.comboTargetDir_DropDown);
            // 
            // comboTargetDir
            // 
            this.comboTargetDir.AllowDrop = true;
            this.comboTargetDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboTargetDir.FormattingEnabled = true;
            this.comboTargetDir.Location = new System.Drawing.Point(366, 17);
            this.comboTargetDir.Name = "comboTargetDir";
            this.comboTargetDir.Size = new System.Drawing.Size(414, 20);
            this.comboTargetDir.TabIndex = 3;
            this.comboTargetDir.DragDrop += new System.Windows.Forms.DragEventHandler(this.comboTargetDir_DragDrop);
            this.comboTargetDir.DragEnter += new System.Windows.Forms.DragEventHandler(this.comboSourceFile_DragEnter);
            // 
            // listTask
            // 
            this.listTask.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listTask.FormattingEnabled = true;
            this.listTask.ItemHeight = 12;
            this.listTask.Location = new System.Drawing.Point(168, 22);
            this.listTask.Name = "listTask";
            this.listTask.Size = new System.Drawing.Size(722, 292);
            this.listTask.TabIndex = 4;
            this.listTask.SelectedIndexChanged += new System.EventHandler(this.listTask_SelectedIndexChanged);
            // 
            // comboParam1
            // 
            this.comboParam1.FormattingEnabled = true;
            this.comboParam1.Location = new System.Drawing.Point(65, 20);
            this.comboParam1.Name = "comboParam1";
            this.comboParam1.Size = new System.Drawing.Size(179, 20);
            this.comboParam1.TabIndex = 6;
            // 
            // comboSourceFile
            // 
            this.comboSourceFile.AllowDrop = true;
            this.comboSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSourceFile.FormattingEnabled = true;
            this.comboSourceFile.Location = new System.Drawing.Point(366, 19);
            this.comboSourceFile.Name = "comboSourceFile";
            this.comboSourceFile.Size = new System.Drawing.Size(524, 20);
            this.comboSourceFile.TabIndex = 2;
            this.comboSourceFile.TextChanged += new System.EventHandler(this.comboSourceFile_TextChanged);
            this.comboSourceFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.comboSourceFile_DragDrop);
            this.comboSourceFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.comboSourceFile_DragEnter);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(10, 166);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 7;
            this.buttonAdd.Text = "添加";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "来源IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(296, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "来自文件";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "目的IP";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(253, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 12);
            this.label4.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(259, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "目录（可含变参）";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "变参1=值";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboSourceIP);
            this.groupBox1.Controls.Add(this.comboSourceFile);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(897, 48);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "发送文件源";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(698, 57);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(203, 12);
            this.label10.TabIndex = 10;
            this.label10.Text = "（*localhost 本地拖动文件或目录）";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkEnforce);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.comboTargetDir);
            this.groupBox2.Controls.Add(this.comboTargetIP);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(4, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(897, 97);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "接收文件源";
            // 
            // checkEnforce
            // 
            this.checkEnforce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkEnforce.AutoSize = true;
            this.checkEnforce.Checked = true;
            this.checkEnforce.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEnforce.Enabled = false;
            this.checkEnforce.Location = new System.Drawing.Point(818, 19);
            this.checkEnforce.Name = "checkEnforce";
            this.checkEnforce.Size = new System.Drawing.Size(72, 16);
            this.checkEnforce.TabIndex = 19;
            this.checkEnforce.Text = "强制覆盖";
            this.checkEnforce.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.comboParam3);
            this.groupBox4.Controls.Add(this.comboParam1);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.comboParam2);
            this.groupBox4.Location = new System.Drawing.Point(6, 44);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(885, 47);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "高级设置（可空）";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.Location = new System.Drawing.Point(775, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 12);
            this.label9.TabIndex = 18;
            this.label9.Text = "（如：$p1$=0.1.0）";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(524, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 17;
            this.label8.Text = "变参3=值";
            // 
            // comboParam3
            // 
            this.comboParam3.FormattingEnabled = true;
            this.comboParam3.Location = new System.Drawing.Point(583, 20);
            this.comboParam3.Name = "comboParam3";
            this.comboParam3.Size = new System.Drawing.Size(191, 20);
            this.comboParam3.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(253, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "变参2=值";
            // 
            // comboParam2
            // 
            this.comboParam2.FormattingEnabled = true;
            this.comboParam2.Location = new System.Drawing.Point(317, 20);
            this.comboParam2.Name = "comboParam2";
            this.comboParam2.Size = new System.Drawing.Size(191, 20);
            this.comboParam2.TabIndex = 14;
            // 
            // listBoxValue
            // 
            this.listBoxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxValue.FormattingEnabled = true;
            this.listBoxValue.ItemHeight = 12;
            this.listBoxValue.Location = new System.Drawing.Point(8, 22);
            this.listBoxValue.Name = "listBoxValue";
            this.listBoxValue.Size = new System.Drawing.Size(154, 292);
            this.listBoxValue.TabIndex = 16;
            this.listBoxValue.SelectedIndexChanged += new System.EventHandler(this.listBoxValue_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.listTask);
            this.groupBox3.Controls.Add(this.listBoxValue);
            this.groupBox3.Location = new System.Drawing.Point(4, 195);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(897, 320);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "接收路径列表";
            // 
            // buttonDel
            // 
            this.buttonDel.Enabled = false;
            this.buttonDel.Location = new System.Drawing.Point(91, 166);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(75, 23);
            this.buttonDel.TabIndex = 18;
            this.buttonDel.Text = "删除";
            this.buttonDel.UseVisualStyleBackColor = true;
            this.buttonDel.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(253, 166);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 19;
            this.buttonImport.Text = "导入";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(334, 166);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 20;
            this.buttonExport.Text = "导出";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonSubmit
            // 
            this.buttonSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSubmit.Location = new System.Drawing.Point(823, 166);
            this.buttonSubmit.Name = "buttonSubmit";
            this.buttonSubmit.Size = new System.Drawing.Size(75, 23);
            this.buttonSubmit.TabIndex = 21;
            this.buttonSubmit.Text = "提交执行";
            this.buttonSubmit.UseVisualStyleBackColor = true;
            this.buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(172, 166);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 23;
            this.buttonClear.Text = "清空";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // ServerOperator
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 519);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonSubmit);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonDel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ServerOperator";
            this.Text = "文件传输服务";
            this.Shown += new System.EventHandler(this.ServerOperator_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboSourceIP;
        private System.Windows.Forms.ComboBox comboTargetIP;
        private System.Windows.Forms.ComboBox comboTargetDir;
        private System.Windows.Forms.ListBox listTask;
        private System.Windows.Forms.ComboBox comboParam1;
        private System.Windows.Forms.ComboBox comboSourceFile;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboParam3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboParam2;
        private System.Windows.Forms.ListBox listBoxValue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonDel;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonSubmit;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkEnforce;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonClear;
    }
}