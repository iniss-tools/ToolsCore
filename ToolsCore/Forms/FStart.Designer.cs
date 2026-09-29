namespace ToolsCore.Forms
{
    partial class FStart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FStart));
            ExControls.OptionsNode optionsNode1 = new ExControls.OptionsNode();
            ExControls.OptionsNode optionsNode2 = new ExControls.OptionsNode();
            this.exOptionsView1 = new ExControls.ExOptionsView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pProjects = new ExControls.ExOptionsPanel(this.exOptionsView1);
            this.pSettings = new ExControls.ExOptionsPanel(this.exOptionsView1);
            ((System.ComponentModel.ISupportInitialize)(this.exOptionsView1)).BeginInit();
            this.exOptionsView1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // exOptionsView1
            // 
            this.exOptionsView1.Controls.Add(this.statusStrip1);
            resources.ApplyResources(this.exOptionsView1, "exOptionsView1");
            this.exOptionsView1.HeaderNodeNameBackColor = System.Drawing.SystemColors.Control;
            this.exOptionsView1.HeaderNodeNameFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.exOptionsView1.HeaderNodeNameForeColor = System.Drawing.SystemColors.ControlText;
            this.exOptionsView1.HeaderNodeNameVisible = false;
            this.exOptionsView1.LinkToChildrenForeColor = System.Drawing.Color.Empty;
            this.exOptionsView1.Name = "exOptionsView1";
            this.exOptionsView1.Panels.Add(this.pProjects);
            this.exOptionsView1.Panels.Add(this.pSettings);
            this.exOptionsView1.SearchBoxVisible = false;
            // 
            // exOptionsView1.ToolStripMenu
            // 
            this.exOptionsView1.ToolStripMenu.BackColor = System.Drawing.SystemColors.Control;
            this.exOptionsView1.ToolStripMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exOptionsView1.ToolStripMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.exOptionsView1.ToolStripMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.exOptionsView1.ToolStripMenu.Location = new System.Drawing.Point(307, 0);
            this.exOptionsView1.ToolStripMenu.Name = "ToolStripMenu";
            this.exOptionsView1.ToolStripMenu.Size = new System.Drawing.Size(102, 25);
            this.exOptionsView1.ToolStripMenu.TabIndex = 1;
            this.exOptionsView1.ToolStripMenu.Text = "toolStrip1";
            this.exOptionsView1.ToolStripMenu.Visible = false;
            // 
            // 
            // 
            this.exOptionsView1.TreeView.BackColor = System.Drawing.SystemColors.Control;
            this.exOptionsView1.TreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.exOptionsView1.TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exOptionsView1.TreeView.FullRowSelect = true;
            this.exOptionsView1.TreeView.HideSelection = false;
            this.exOptionsView1.TreeView.HotTracking = true;
            this.exOptionsView1.TreeView.ItemHeight = 25;
            this.exOptionsView1.TreeView.Name = "treeView";
            this.exOptionsView1.TreeView.PathSeparator = " / ";
            this.exOptionsView1.TreeView.ShowLines = false;
            this.exOptionsView1.TreeView.ShowNodeToolTips = true;
            this.exOptionsView1.TreeView.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.SizingGrip = false;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // pProjects
            // 
            this.pProjects.Name = "pProjects";
            optionsNode1.Name = "";
            optionsNode1.Text = resources.GetString("optionsNode1.Text");
            this.pProjects.Node = optionsNode1;
            this.pProjects.NodeText = resources.GetString("pProjects.NodeText");
            this.pProjects.ParentNode = null;
            // 
            // pSettings
            // 
            this.pSettings.Name = "pSettings";
            optionsNode2.Name = "";
            optionsNode2.Text = resources.GetString("optionsNode2.Text");
            this.pSettings.Node = optionsNode2;
            this.pSettings.NodeText = resources.GetString("pSettings.NodeText");
            this.pSettings.ParentNode = null;
            // 
            // FStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.exOptionsView1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(514, 349);
            this.Name = "FStart";
            ((System.ComponentModel.ISupportInitialize)(this.exOptionsView1)).EndInit();
            this.exOptionsView1.ResumeLayout(false);
            this.exOptionsView1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ExControls.ExOptionsView exOptionsView1;
        private ExControls.ExOptionsPanel pProjects;
        private ExControls.ExOptionsPanel pSettings;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
    }
}