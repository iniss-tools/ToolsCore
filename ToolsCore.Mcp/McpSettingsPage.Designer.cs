namespace ToolsCore.Mcp
{
    partial class McpSettingsPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(McpSettingsPage));
            this.lHint = new System.Windows.Forms.Label();
            this.gbServer = new ExControls.ExGroupBox();
            this.cboxEnabled = new ExControls.ExCheckBox();
            this.lPort = new System.Windows.Forms.Label();
            this.nudPort = new ExControls.ExNumericUpDown();
            this.lStatus = new System.Windows.Forms.Label();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.bClaude = new ExControls.ExButton();
            this.bVsCode = new ExControls.ExButton();
            this.bAddress = new ExControls.ExButton();
            this.bNewKey = new ExControls.ExButton();
            this.lClients = new System.Windows.Forms.Label();
            this.gbWrites = new ExControls.ExGroupBox();
            this.cboxWrites = new ExControls.ExCheckBox();
            this.lWritesHint = new System.Windows.Forms.Label();
            this.gbServer.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.gbWrites.SuspendLayout();
            this.SuspendLayout();
            // 
            // lHint
            // 
            resources.ApplyResources(this.lHint, "lHint");
            this.lHint.Name = "lHint";
            // 
            // gbServer
            // 
            this.gbServer.Controls.Add(this.cboxEnabled);
            this.gbServer.Controls.Add(this.lPort);
            this.gbServer.Controls.Add(this.nudPort);
            this.gbServer.Controls.Add(this.lStatus);
            this.gbServer.Controls.Add(this.flpButtons);
            this.gbServer.Controls.Add(this.lClients);
            resources.ApplyResources(this.gbServer, "gbServer");
            this.gbServer.TabStop = false;
            this.gbServer.Name = "gbServer";
            // 
            // cboxEnabled
            // 
            resources.ApplyResources(this.cboxEnabled, "cboxEnabled");
            this.cboxEnabled.Name = "cboxEnabled";
            this.cboxEnabled.CheckedChanged += new System.EventHandler(this.Setting_Changed);
            // 
            // lPort
            // 
            resources.ApplyResources(this.lPort, "lPort");
            this.lPort.Name = "lPort";
            // 
            // nudPort
            // 
            resources.ApplyResources(this.nudPort, "nudPort");
            this.nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.nudPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
            this.nudPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
            this.nudPort.Name = "nudPort";
            this.nudPort.ValueChanged += new System.EventHandler(this.Setting_Changed);
            // 
            // lStatus
            // 
            resources.ApplyResources(this.lStatus, "lStatus");
            this.lStatus.Name = "lStatus";
            // 
            // flpButtons
            // 
            this.flpButtons.Controls.Add(this.bClaude);
            this.flpButtons.Controls.Add(this.bVsCode);
            this.flpButtons.Controls.Add(this.bAddress);
            this.flpButtons.Controls.Add(this.bNewKey);
            resources.ApplyResources(this.flpButtons, "flpButtons");
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpButtons.WrapContents = false;
            this.flpButtons.Name = "flpButtons";
            // 
            // bClaude
            // 
            resources.ApplyResources(this.bClaude, "bClaude");
            this.bClaude.UseVisualStyleBackColor = true;
            this.bClaude.Name = "bClaude";
            this.bClaude.Click += new System.EventHandler(this.bClaude_Click);
            // 
            // bVsCode
            // 
            resources.ApplyResources(this.bVsCode, "bVsCode");
            this.bVsCode.UseVisualStyleBackColor = true;
            this.bVsCode.Name = "bVsCode";
            this.bVsCode.Click += new System.EventHandler(this.bVsCode_Click);
            // 
            // bAddress
            // 
            resources.ApplyResources(this.bAddress, "bAddress");
            this.bAddress.UseVisualStyleBackColor = true;
            this.bAddress.Name = "bAddress";
            this.bAddress.Click += new System.EventHandler(this.bAddress_Click);
            // 
            // bNewKey
            // 
            resources.ApplyResources(this.bNewKey, "bNewKey");
            this.bNewKey.UseVisualStyleBackColor = true;
            this.bNewKey.Name = "bNewKey";
            this.bNewKey.Click += new System.EventHandler(this.bNewKey_Click);
            // 
            // lClients
            // 
            resources.ApplyResources(this.lClients, "lClients");
            this.lClients.Name = "lClients";
            // 
            // gbWrites
            // 
            this.gbWrites.Controls.Add(this.cboxWrites);
            this.gbWrites.Controls.Add(this.lWritesHint);
            resources.ApplyResources(this.gbWrites, "gbWrites");
            this.gbWrites.TabStop = false;
            this.gbWrites.Name = "gbWrites";
            // 
            // cboxWrites
            // 
            resources.ApplyResources(this.cboxWrites, "cboxWrites");
            this.cboxWrites.Name = "cboxWrites";
            this.cboxWrites.CheckedChanged += new System.EventHandler(this.Setting_Changed);
            // 
            // lWritesHint
            // 
            resources.ApplyResources(this.lWritesHint, "lWritesHint");
            this.lWritesHint.Name = "lWritesHint";
            // 
            // McpSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lHint);
            this.Controls.Add(this.gbServer);
            this.Controls.Add(this.gbWrites);
            this.Name = "McpSettingsPage";
            resources.ApplyResources(this, "$this");
            this.gbWrites.ResumeLayout(false);
            this.gbWrites.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flpButtons.PerformLayout();
            this.gbServer.ResumeLayout(false);
            this.gbServer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lHint;
        private ExControls.ExGroupBox gbServer;
        private ExControls.ExCheckBox cboxEnabled;
        private System.Windows.Forms.Label lPort;
        private ExControls.ExNumericUpDown nudPort;
        private System.Windows.Forms.Label lStatus;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private ExControls.ExButton bClaude;
        private ExControls.ExButton bVsCode;
        private ExControls.ExButton bAddress;
        private ExControls.ExButton bNewKey;
        private System.Windows.Forms.Label lClients;
        private ExControls.ExGroupBox gbWrites;
        private ExControls.ExCheckBox cboxWrites;
        private System.Windows.Forms.Label lWritesHint;
    }
}
