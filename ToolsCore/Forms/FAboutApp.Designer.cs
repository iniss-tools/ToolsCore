using ExControls;

namespace ToolsCore.Forms
{
    partial class FAboutApp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAboutApp));
            this.lText = new System.Windows.Forms.Label();
            this.lAppVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lAppName = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.linkWeb = new System.Windows.Forms.LinkLabel();
            this.linkEmail = new System.Windows.Forms.LinkLabel();
            this.groupBox2 = new ExControls.ExGroupBox();
            this.groupBox1 = new ExControls.ExGroupBox();
            this.bOK = new ExControls.ExButton();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lText
            // 
            resources.ApplyResources(this.lText, "lText");
            this.lText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lText.Name = "lText";
            this.lText.Padding = new System.Windows.Forms.Padding(5);
            // 
            // lAppVersion
            // 
            resources.ApplyResources(this.lAppVersion, "lAppVersion");
            this.lAppVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAppVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lAppVersion.Name = "lAppVersion";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            // 
            // lAppName
            // 
            resources.ApplyResources(this.lAppName, "lAppName");
            this.lAppName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAppName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lAppName.Name = "lAppName";
            // 
            // picIcon
            // 
            this.picIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            resources.ApplyResources(this.picIcon, "picIcon");
            this.picIcon.Margin = new System.Windows.Forms.Padding(2);
            this.picIcon.Name = "picIcon";
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIcon.TabStop = false;
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            // 
            // linkWeb
            // 
            resources.ApplyResources(this.linkWeb, "linkWeb");
            this.linkWeb.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkWeb.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkWeb.Name = "linkWeb";
            this.linkWeb.TabStop = true;
            this.linkWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnWebLinkClicked);
            // 
            // linkEmail
            // 
            resources.ApplyResources(this.linkEmail, "linkEmail");
            this.linkEmail.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkEmail.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkEmail.Name = "linkEmail";
            this.linkEmail.TabStop = true;
            this.linkEmail.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnEmailLinkClicked);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.lText);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.TabStop = false;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.linkWeb);
            this.groupBox1.Controls.Add(this.linkEmail);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.TabStop = false;
            // 
            // bOK
            // 
            resources.ApplyResources(this.bOK, "bOK");
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.bOK.Margin = new System.Windows.Forms.Padding(2);
            this.bOK.Name = "bOK";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.picIcon);
            this.panel1.Controls.Add(this.lAppVersion);
            this.panel1.Controls.Add(this.bOK);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.lAppName);
            this.panel1.Controls.Add(this.groupBox2);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // FAboutApp
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel1);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(637, 493);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(304, 344);
            this.Name = "FAboutApp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnHelpButtonClicked);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Label lText;
        private Label lAppVersion;
        private Label label2;
        private Label lAppName;
        private PictureBox picIcon;
        private Label label7;
        private Label label8;
        private LinkLabel linkWeb;
        private LinkLabel linkEmail;
        private ExGroupBox groupBox2;
        private ExGroupBox groupBox1;
        private ExButton bOK;
        private Panel panel1;
    }
}