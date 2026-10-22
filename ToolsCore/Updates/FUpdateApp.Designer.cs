namespace ToolsCore.Updates
{
    partial class FUpdateApp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FUpdateApp));
            pbIcon = new System.Windows.Forms.PictureBox();
            lTitle = new System.Windows.Forms.Label();
            lVersions = new System.Windows.Forms.Label();
            lNotes = new System.Windows.Forms.Label();
            rtbNotes = new ExControls.ExRichTextBox();
            linkPage = new System.Windows.Forms.LinkLabel();
            pFooter = new System.Windows.Forms.Panel();
            progress = new System.Windows.Forms.ProgressBar();
            bUpdate = new ExControls.ExButton();
            bSkip = new ExControls.ExButton();
            bLater = new ExControls.ExButton();
            ((System.ComponentModel.ISupportInitialize)pbIcon).BeginInit();
            pFooter.SuspendLayout();
            SuspendLayout();
            //
            // pbIcon
            //
            resources.ApplyResources(pbIcon, "pbIcon");
            pbIcon.Name = "pbIcon";
            pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pbIcon.TabStop = false;
            //
            // lTitle
            //
            resources.ApplyResources(lTitle, "lTitle");
            lTitle.Name = "lTitle";
            //
            // lVersions
            //
            resources.ApplyResources(lVersions, "lVersions");
            lVersions.Name = "lVersions";
            //
            // lNotes
            //
            resources.ApplyResources(lNotes, "lNotes");
            lNotes.Name = "lNotes";
            //
            // rtbNotes
            //
            resources.ApplyResources(rtbNotes, "rtbNotes");
            rtbNotes.BorderColor = System.Drawing.Color.DimGray;
            rtbNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbNotes.DefaultStyle = false;
            rtbNotes.DisabledBackColor = System.Drawing.SystemColors.Control;
            rtbNotes.DisabledBorderColor = System.Drawing.SystemColors.InactiveBorder;
            rtbNotes.DisabledForeColor = System.Drawing.SystemColors.GrayText;
            rtbNotes.HighlightColor = System.Drawing.SystemColors.Highlight;
            rtbNotes.Name = "rtbNotes";
            rtbNotes.ReadOnly = true;
            rtbNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            //
            // linkPage
            //
            resources.ApplyResources(linkPage, "linkPage");
            linkPage.Name = "linkPage";
            linkPage.TabStop = true;
            linkPage.LinkClicked += OnPageLinkClicked;
            //
            // pFooter
            //
            pFooter.Controls.Add(progress);
            pFooter.Controls.Add(bUpdate);
            pFooter.Controls.Add(bSkip);
            pFooter.Controls.Add(bLater);
            resources.ApplyResources(pFooter, "pFooter");
            pFooter.Name = "pFooter";
            //
            // progress
            //
            resources.ApplyResources(progress, "progress");
            progress.Name = "progress";
            //
            // bUpdate
            //
            resources.ApplyResources(bUpdate, "bUpdate");
            bUpdate.Name = "bUpdate";
            bUpdate.UseVisualStyleBackColor = true;
            bUpdate.Click += OnUpdateClick;
            //
            // bSkip
            //
            resources.ApplyResources(bSkip, "bSkip");
            bSkip.Name = "bSkip";
            bSkip.UseVisualStyleBackColor = true;
            bSkip.Click += OnSkipClick;
            //
            // bLater
            //
            resources.ApplyResources(bLater, "bLater");
            bLater.Name = "bLater";
            bLater.UseVisualStyleBackColor = true;
            bLater.Click += OnLaterClick;
            //
            // FUpdateApp
            //
            AcceptButton = bUpdate;
            resources.ApplyResources(this, "$this");
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = bLater;
            Controls.Add(rtbNotes);
            Controls.Add(linkPage);
            Controls.Add(lNotes);
            Controls.Add(lVersions);
            Controls.Add(lTitle);
            Controls.Add(pbIcon);
            Controls.Add(pFooter);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FUpdateApp";
            ShowIcon = false;
            ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)pbIcon).EndInit();
            pFooter.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lTitle;
        private System.Windows.Forms.Label lVersions;
        private System.Windows.Forms.Label lNotes;
        private ExControls.ExRichTextBox rtbNotes;
        private System.Windows.Forms.LinkLabel linkPage;
        private System.Windows.Forms.Panel pFooter;
        private System.Windows.Forms.ProgressBar progress;
        private ExControls.ExButton bUpdate;
        private ExControls.ExButton bSkip;
        private ExControls.ExButton bLater;
    }
}
