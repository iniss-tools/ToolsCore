namespace ToolsCore.Forms.Calendar
{
    partial class FDateLimitEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FDateLimitEdit));
            this.components = new System.ComponentModel.Container();
            this.exLabel1 = new ExControls.ExLabel();
            this.tbDateLimit = new ExControls.ExTextBox();
            this.dgvCalendar = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bCheck = new ExControls.ExButton();
            this.bReset = new ExControls.ExButton();
            this.exLabel2 = new ExControls.ExLabel();
            this.tbOldDateLimit = new ExControls.ExTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bStorno = new ExControls.ExButton();
            this.bOK = new ExControls.ExButton();
            this.bNever = new ExControls.ExButton();
            this.bDaily = new ExControls.ExButton();
            this.timer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCalendar)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // exLabel1
            // 
            resources.ApplyResources(this.exLabel1, "exLabel1");
            this.exLabel1.Name = "exLabel1";
            // 
            // tbDateLimit
            // 
            resources.ApplyResources(this.tbDateLimit, "tbDateLimit");
            this.tbDateLimit.BorderColor = System.Drawing.Color.DimGray;
            this.tbDateLimit.DisabledBackColor = System.Drawing.SystemColors.Control;
            this.tbDateLimit.DisabledBorderColor = System.Drawing.SystemColors.InactiveBorder;
            this.tbDateLimit.DisabledForeColor = System.Drawing.SystemColors.GrayText;
            this.tbDateLimit.HighlightColor = System.Drawing.SystemColors.Highlight;
            this.tbDateLimit.HintForeColor = System.Drawing.SystemColors.GrayText;
            this.tbDateLimit.HintText = null;
            this.tbDateLimit.Name = "tbDateLimit";
            this.tbDateLimit.TextChanged += new System.EventHandler(this.TbDateLimit_TextChanged);
            // 
            // dgvCalendar
            // 
            this.dgvCalendar.AllowUserToAddRows = false;
            this.dgvCalendar.AllowUserToDeleteRows = false;
            this.dgvCalendar.AllowUserToResizeColumns = false;
            this.dgvCalendar.AllowUserToResizeRows = false;
            this.dgvCalendar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCalendar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dgvCalendar, "dgvCalendar");
            this.dgvCalendar.Name = "dgvCalendar";
            this.dgvCalendar.ReadOnly = true;
            this.dgvCalendar.RowHeadersVisible = false;
            this.dgvCalendar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvCalendar.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvCalendar_CellMouseDown);
            this.dgvCalendar.CellMouseMove += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvCalendar_CellMouseMove);
            this.dgvCalendar.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvCalendar_CellMouseUp);
            this.dgvCalendar.CellStateChanged += new System.Windows.Forms.DataGridViewCellStateChangedEventHandler(this.DgvCalendar_CellStateChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bCheck);
            this.panel1.Controls.Add(this.bReset);
            this.panel1.Controls.Add(this.exLabel2);
            this.panel1.Controls.Add(this.tbOldDateLimit);
            this.panel1.Controls.Add(this.exLabel1);
            this.panel1.Controls.Add(this.tbDateLimit);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // bCheck
            // 
            resources.ApplyResources(this.bCheck, "bCheck");
            this.bCheck.Name = "bCheck";
            this.bCheck.UseVisualStyleBackColor = true;
            this.bCheck.Click += new System.EventHandler(this.BCheck_Click);
            // 
            // bReset
            // 
            resources.ApplyResources(this.bReset, "bReset");
            this.bReset.Name = "bReset";
            this.bReset.UseVisualStyleBackColor = true;
            this.bReset.Click += new System.EventHandler(this.BReset_Click);
            // 
            // exLabel2
            // 
            resources.ApplyResources(this.exLabel2, "exLabel2");
            this.exLabel2.Name = "exLabel2";
            // 
            // tbOldDateLimit
            // 
            resources.ApplyResources(this.tbOldDateLimit, "tbOldDateLimit");
            this.tbOldDateLimit.BorderColor = System.Drawing.Color.DimGray;
            this.tbOldDateLimit.DisabledBackColor = System.Drawing.SystemColors.Control;
            this.tbOldDateLimit.DisabledBorderColor = System.Drawing.SystemColors.InactiveBorder;
            this.tbOldDateLimit.DisabledForeColor = System.Drawing.SystemColors.GrayText;
            this.tbOldDateLimit.HighlightColor = System.Drawing.SystemColors.Highlight;
            this.tbOldDateLimit.HintForeColor = System.Drawing.SystemColors.GrayText;
            this.tbOldDateLimit.HintText = null;
            this.tbOldDateLimit.Name = "tbOldDateLimit";
            this.tbOldDateLimit.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.bStorno);
            this.panel2.Controls.Add(this.bOK);
            this.panel2.Controls.Add(this.bNever);
            this.panel2.Controls.Add(this.bDaily);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3);
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
            // bNever
            // 
            resources.ApplyResources(this.bNever, "bNever");
            this.bNever.Name = "bNever";
            this.bNever.UseVisualStyleBackColor = true;
            this.bNever.Click += new System.EventHandler(this.BNever_Click);
            // 
            // bDaily
            // 
            resources.ApplyResources(this.bDaily, "bDaily");
            this.bDaily.Name = "bDaily";
            this.bDaily.UseVisualStyleBackColor = true;
            this.bDaily.Click += new System.EventHandler(this.BDaily_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // FDateLimitEdit
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bStorno;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.dgvCalendar);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FDateLimitEdit";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FDateLimit_FormClosed);
            this.Load += new System.EventHandler(this.FDateLimit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCalendar)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ExControls.ExLabel exLabel1;
        private ExControls.ExTextBox tbDateLimit;
        private DataGridView dgvCalendar;
        private Panel panel1;
        private Panel panel2;
        private ExControls.ExButton bStorno;
        private ExControls.ExButton bOK;
        private ExControls.ExButton bNever;
        private ExControls.ExButton bDaily;
        private ExControls.ExButton bReset;
        private ExControls.ExLabel exLabel2;
        private ExControls.ExTextBox tbOldDateLimit;
        private ExControls.ExButton bCheck;
        private System.Windows.Forms.Timer timer;
    }
}