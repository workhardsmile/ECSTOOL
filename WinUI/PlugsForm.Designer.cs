namespace ECSTOOL
{
    partial class PlugsForm
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
            this.listViewTool = new System.Windows.Forms.ListView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // listViewTool
            // 
            this.listViewTool.BackColor = System.Drawing.SystemColors.Control;
            this.listViewTool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTool.LargeImageList = this.imageList;
            this.listViewTool.Location = new System.Drawing.Point(0, 0);
            this.listViewTool.MultiSelect = false;
            this.listViewTool.Name = "listViewTool";
            this.listViewTool.Scrollable = false;
            this.listViewTool.ShowItemToolTips = true;
            this.listViewTool.Size = new System.Drawing.Size(284, 262);
            this.listViewTool.SmallImageList = this.imageList;
            this.listViewTool.TabIndex = 0;
            this.listViewTool.TileSize = new System.Drawing.Size(500, 36);
            this.listViewTool.UseCompatibleStateImageBehavior = false;
            this.listViewTool.View = System.Windows.Forms.View.Tile;
            this.listViewTool.DoubleClick += new System.EventHandler(this.listViewTool_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(32, 32);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // PlugsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.listViewTool);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "PlugsForm";
            this.Text = "插件工具集";
            this.Load += new System.EventHandler(this.PlugForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewTool;
        private System.Windows.Forms.ImageList imageList;
    }
}