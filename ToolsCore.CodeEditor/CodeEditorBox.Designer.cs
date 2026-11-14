
using System.ComponentModel;

namespace ToolsCore.CodeEditor
{
    partial class CodeEditorBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scintillaEditor = new ScintillaNET.Scintilla();
            this.pVertical = new System.Windows.Forms.Panel();
            this.VScrollBarControl = new System.Windows.Forms.VScrollBar();
            this.pHorizontal = new System.Windows.Forms.Panel();
            this.HScrollBarControl = new System.Windows.Forms.HScrollBar();
            this.pVertical.SuspendLayout();
            this.pHorizontal.SuspendLayout();
            this.SuspendLayout();
            // 
            // Scintilla
            // 
            this.scintillaEditor.BorderStyle = ScintillaNET.BorderStyle.None;
            this.scintillaEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scintillaEditor.HScrollBar = false;
            this.scintillaEditor.Location = new System.Drawing.Point(0, 0);
            this.scintillaEditor.Name = "scintillaEditor";
            this.scintillaEditor.Size = new System.Drawing.Size(449, 329);
            this.scintillaEditor.TabIndex = 0;
            this.scintillaEditor.VScrollBar = false;
            this.scintillaEditor.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.Scintilla_UpdateUI);
            // 
            // pVertical
            // 
            this.pVertical.AutoSize = true;
            this.pVertical.Controls.Add(this.VScrollBarControl);
            this.pVertical.Dock = System.Windows.Forms.DockStyle.Right;
            this.pVertical.Location = new System.Drawing.Point(449, 0);
            this.pVertical.MinimumSize = new System.Drawing.Size(21, 0);
            this.pVertical.Name = "pVertical";
            this.pVertical.Size = new System.Drawing.Size(21, 329);
            this.pVertical.TabIndex = 1;
            this.pVertical.Visible = false;
            // 
            // VScrollBarControl
            // 
            this.VScrollBarControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VScrollBarControl.Location = new System.Drawing.Point(0, 0);
            this.VScrollBarControl.Name = "VScrollBarControl";
            this.VScrollBarControl.Size = new System.Drawing.Size(21, 329);
            this.VScrollBarControl.TabIndex = 0;
            // 
            // pHorizontal
            // 
            this.pHorizontal.AutoSize = true;
            this.pHorizontal.Controls.Add(this.HScrollBarControl);
            this.pHorizontal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pHorizontal.Location = new System.Drawing.Point(0, 329);
            this.pHorizontal.MinimumSize = new System.Drawing.Size(0, 21);
            this.pHorizontal.Name = "pHorizontal";
            this.pHorizontal.Padding = new System.Windows.Forms.Padding(0, 0, 21, 0);
            this.pHorizontal.Size = new System.Drawing.Size(470, 21);
            this.pHorizontal.TabIndex = 2;
            this.pHorizontal.Visible = false;
            // 
            // HScrollBarControl
            // 
            this.HScrollBarControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HScrollBarControl.Location = new System.Drawing.Point(0, 0);
            this.HScrollBarControl.Name = "HScrollBarControl";
            this.HScrollBarControl.Size = new System.Drawing.Size(449, 21);
            this.HScrollBarControl.TabIndex = 0;
            // 
            // CodeEditorBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.scintillaEditor);
            this.Controls.Add(this.pVertical);
            this.Controls.Add(this.pHorizontal);
            this.Name = "CodeEditorBox";
            this.Size = new System.Drawing.Size(470, 350);
            this.pVertical.ResumeLayout(false);
            this.pHorizontal.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pVertical;
        public System.Windows.Forms.VScrollBar VScrollBarControl;
        private System.Windows.Forms.Panel pHorizontal;
        public System.Windows.Forms.HScrollBar HScrollBarControl;
        public ScintillaNET.Scintilla scintillaEditor;
    }
}
