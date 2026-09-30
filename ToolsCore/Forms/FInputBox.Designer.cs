namespace ToolsCore.Forms
{
    partial class FInputBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FInputBox));
            this.label1 = new System.Windows.Forms.Label();
            this.tbValue = new ExControls.ExTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bStorno = new ExControls.ExButton();
            this.bOK = new ExControls.ExButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tbValue
            // 
            resources.ApplyResources(this.tbValue, "tbValue");
            this.tbValue.BorderColor = System.Drawing.Color.DimGray;
            this.tbValue.DisabledBackColor = System.Drawing.SystemColors.Control;
            this.tbValue.DisabledBorderColor = System.Drawing.SystemColors.InactiveBorder;
            this.tbValue.DisabledForeColor = System.Drawing.SystemColors.GrayText;
            this.tbValue.HighlightColor = System.Drawing.SystemColors.Highlight;
            this.tbValue.HintForeColor = System.Drawing.SystemColors.GrayText;
            this.tbValue.HintText = null;
            this.tbValue.Name = "tbValue";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bStorno);
            this.panel1.Controls.Add(this.bOK);
            this.panel1.Controls.Add(this.tbValue);
            this.panel1.Controls.Add(this.label1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            // 
            // bStorno
            // 
            resources.ApplyResources(this.bStorno, "bStorno");
            this.bStorno.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bStorno.Name = "bStorno";
            this.bStorno.UseVisualStyleBackColor = true;
            // 
            // bOK
            // 
            resources.ApplyResources(this.bOK, "bOK");
            this.bOK.Name = "bOK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.BOK_Click);
            // 
            // FInputBox
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bStorno;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(650, 110);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(352, 110);
            this.Name = "FInputBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private ExControls.ExTextBox tbValue;
        private Panel panel1;
        private ExControls.ExButton bStorno;
        private ExControls.ExButton bOK;
    }
}