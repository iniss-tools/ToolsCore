namespace ToolsCore.Forms
{
    partial class FError
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FError));
            this.panelHeader = new System.Windows.Forms.Panel();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lTitle = new System.Windows.Forms.Label();
            this.lMessage = new System.Windows.Forms.Label();
            this.llDetails = new System.Windows.Forms.LinkLabel();
            this.panelDetails = new System.Windows.Forms.Panel();
            this.rtbDetails = new ExControls.ExRichTextBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.bOK = new ExControls.ExButton();
            this.bCopy = new ExControls.ExButton();
            this.bLogs = new ExControls.ExButton();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.panelDetails.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // panelHeader
            //
            this.panelHeader.Controls.Add(this.llDetails);
            this.panelHeader.Controls.Add(this.lMessage);
            this.panelHeader.Controls.Add(this.lTitle);
            this.panelHeader.Controls.Add(this.pbIcon);
            resources.ApplyResources(this.panelHeader, "panelHeader");
            this.panelHeader.Name = "panelHeader";
            //
            // pbIcon
            //
            resources.ApplyResources(this.pbIcon, "pbIcon");
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbIcon.TabStop = false;
            //
            // lTitle
            //
            resources.ApplyResources(this.lTitle, "lTitle");
            this.lTitle.Name = "lTitle";
            //
            // lMessage
            //
            resources.ApplyResources(this.lMessage, "lMessage");
            this.lMessage.Name = "lMessage";
            //
            // llDetails
            //
            resources.ApplyResources(this.llDetails, "llDetails");
            this.llDetails.Name = "llDetails";
            this.llDetails.TabStop = true;
            this.llDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llDetails_LinkClicked);
            //
            // panelDetails
            //
            this.panelDetails.Controls.Add(this.rtbDetails);
            resources.ApplyResources(this.panelDetails, "panelDetails");
            this.panelDetails.Name = "panelDetails";
            //
            // rtbDetails
            //
            resources.ApplyResources(this.rtbDetails, "rtbDetails");
            this.rtbDetails.BorderColor = System.Drawing.Color.DimGray;
            this.rtbDetails.DefaultStyle = false;
            this.rtbDetails.DisabledBackColor = System.Drawing.SystemColors.Control;
            this.rtbDetails.DisabledBorderColor = System.Drawing.SystemColors.InactiveBorder;
            this.rtbDetails.DisabledForeColor = System.Drawing.SystemColors.GrayText;
            this.rtbDetails.HighlightColor = System.Drawing.SystemColors.Highlight;
            this.rtbDetails.Name = "rtbDetails";
            this.rtbDetails.ReadOnly = true;
            this.rtbDetails.WordWrap = false;
            //
            // panelButtons
            //
            this.panelButtons.Controls.Add(this.bOK);
            this.panelButtons.Controls.Add(this.bCopy);
            this.panelButtons.Controls.Add(this.bLogs);
            resources.ApplyResources(this.panelButtons, "panelButtons");
            this.panelButtons.Name = "panelButtons";
            //
            // bOK
            //
            resources.ApplyResources(this.bOK, "bOK");
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Name = "bOK";
            this.bOK.UseVisualStyleBackColor = true;
            //
            // bCopy
            //
            resources.ApplyResources(this.bCopy, "bCopy");
            this.bCopy.Name = "bCopy";
            this.bCopy.UseVisualStyleBackColor = true;
            this.bCopy.Click += new System.EventHandler(this.bCopy_Click);
            //
            // bLogs
            //
            resources.ApplyResources(this.bLogs, "bLogs");
            this.bLogs.Name = "bLogs";
            this.bLogs.UseVisualStyleBackColor = true;
            this.bLogs.Click += new System.EventHandler(this.bLogs_Click);
            //
            // FError
            //
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bOK;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panelDetails);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelButtons);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FError";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.panelDetails.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panelHeader;
        private PictureBox pbIcon;
        private Label lTitle;
        private Label lMessage;
        private LinkLabel llDetails;
        private Panel panelDetails;
        private ExControls.ExRichTextBox rtbDetails;
        private Panel panelButtons;
        private ExControls.ExButton bOK;
        private ExControls.ExButton bCopy;
        private ExControls.ExButton bLogs;
    }
}
