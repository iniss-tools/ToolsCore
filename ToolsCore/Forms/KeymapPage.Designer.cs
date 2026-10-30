using ExControls;

namespace ToolsCore.Forms
{
    partial class KeymapPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(KeymapPage));
            pTop = new Panel();
            tbFilter = new ShortcutBox();
            cboxByKeys = new ExCheckBox();
            cboxChangedOnly = new ExCheckBox();
            bResetAll = new ExButton();
            tvCommands = new ExTreeView();
            pDetail = new Panel();
            lCommand = new Label();
            lShortcut = new Label();
            sbShortcut = new ShortcutBox();
            bRemove = new ExButton();
            llDefault = new LinkLabel();
            lConflict = new Label();
            llRemoveConflict = new LinkLabel();
            pTop.SuspendLayout();
            pDetail.SuspendLayout();
            SuspendLayout();
            //
            // pTop
            //
            pTop.Controls.Add(tbFilter);
            pTop.Controls.Add(cboxByKeys);
            pTop.Controls.Add(cboxChangedOnly);
            pTop.Controls.Add(bResetAll);
            resources.ApplyResources(pTop, "pTop");
            pTop.Name = "pTop";
            //
            // tbFilter
            //
            resources.ApplyResources(tbFilter, "tbFilter");
            tbFilter.BorderColor = Color.DimGray;
            tbFilter.DisabledBackColor = SystemColors.Control;
            tbFilter.DisabledBorderColor = SystemColors.InactiveBorder;
            tbFilter.DisabledForeColor = SystemColors.GrayText;
            tbFilter.HighlightColor = SystemColors.Highlight;
            tbFilter.Name = "tbFilter";
            tbFilter.KeysCaptured += TbFilter_KeysCaptured;
            tbFilter.TextChanged += TbFilter_TextChanged;
            //
            // cboxByKeys
            //
            resources.ApplyResources(cboxByKeys, "cboxByKeys");
            cboxByKeys.BoxBackColor = Color.White;
            cboxByKeys.HighlightColor = SystemColors.Highlight;
            cboxByKeys.Name = "cboxByKeys";
            cboxByKeys.CheckedChanged += CboxByKeys_CheckedChanged;
            //
            // cboxChangedOnly
            //
            resources.ApplyResources(cboxChangedOnly, "cboxChangedOnly");
            cboxChangedOnly.BoxBackColor = Color.White;
            cboxChangedOnly.HighlightColor = SystemColors.Highlight;
            cboxChangedOnly.Name = "cboxChangedOnly";
            cboxChangedOnly.CheckedChanged += CboxChangedOnly_CheckedChanged;
            //
            // bResetAll
            //
            resources.ApplyResources(bResetAll, "bResetAll");
            bResetAll.Name = "bResetAll";
            bResetAll.UseVisualStyleBackColor = true;
            bResetAll.Click += BResetAll_Click;
            //
            // tvCommands
            //
            resources.ApplyResources(tvCommands, "tvCommands");
            tvCommands.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvCommands.FullRowSelect = false;
            tvCommands.HideSelection = false;
            tvCommands.ItemHeight = 22;
            tvCommands.Name = "tvCommands";
            tvCommands.ShowLines = false;
            tvCommands.Style = ExTreeViewStyle.Light;
            tvCommands.DrawNode += TvCommands_DrawNode;
            tvCommands.AfterSelect += TvCommands_AfterSelect;
            tvCommands.KeyDown += TvCommands_KeyDown;
            tvCommands.Resize += TvCommands_Resize;
            //
            // pDetail
            //
            pDetail.Controls.Add(lCommand);
            pDetail.Controls.Add(lShortcut);
            pDetail.Controls.Add(sbShortcut);
            pDetail.Controls.Add(bRemove);
            pDetail.Controls.Add(llDefault);
            pDetail.Controls.Add(lConflict);
            pDetail.Controls.Add(llRemoveConflict);
            resources.ApplyResources(pDetail, "pDetail");
            pDetail.Name = "pDetail";
            //
            // lCommand
            //
            resources.ApplyResources(lCommand, "lCommand");
            lCommand.Name = "lCommand";
            //
            // lShortcut
            //
            resources.ApplyResources(lShortcut, "lShortcut");
            lShortcut.Name = "lShortcut";
            //
            // sbShortcut
            //
            resources.ApplyResources(sbShortcut, "sbShortcut");
            sbShortcut.BorderColor = Color.DimGray;
            sbShortcut.DisabledBackColor = SystemColors.Control;
            sbShortcut.DisabledBorderColor = SystemColors.InactiveBorder;
            sbShortcut.DisabledForeColor = SystemColors.GrayText;
            sbShortcut.HighlightColor = SystemColors.Highlight;
            sbShortcut.Name = "sbShortcut";
            sbShortcut.KeysCaptured += SbShortcut_KeysCaptured;
            //
            // bRemove
            //
            resources.ApplyResources(bRemove, "bRemove");
            bRemove.Name = "bRemove";
            bRemove.UseVisualStyleBackColor = true;
            bRemove.Click += BRemove_Click;
            //
            // llDefault
            //
            resources.ApplyResources(llDefault, "llDefault");
            llDefault.Name = "llDefault";
            llDefault.TabStop = true;
            llDefault.LinkClicked += LlDefault_LinkClicked;
            //
            // lConflict
            //
            resources.ApplyResources(lConflict, "lConflict");
            lConflict.Name = "lConflict";
            //
            // llRemoveConflict
            //
            resources.ApplyResources(llRemoveConflict, "llRemoveConflict");
            llRemoveConflict.Name = "llRemoveConflict";
            llRemoveConflict.TabStop = true;
            llRemoveConflict.LinkClicked += LlRemoveConflict_LinkClicked;
            //
            // KeymapPage
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            resources.ApplyResources(this, "$this");
            Controls.Add(tvCommands);
            Controls.Add(pTop);
            Controls.Add(pDetail);
            Name = "KeymapPage";
            pTop.ResumeLayout(false);
            pTop.PerformLayout();
            pDetail.ResumeLayout(false);
            pDetail.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pTop;
        private ShortcutBox tbFilter;
        private ExCheckBox cboxByKeys;
        private ExCheckBox cboxChangedOnly;
        private ExButton bResetAll;
        private ExTreeView tvCommands;
        private Panel pDetail;
        private Label lCommand;
        private Label lShortcut;
        private ShortcutBox sbShortcut;
        private ExButton bRemove;
        private LinkLabel llDefault;
        private Label lConflict;
        private LinkLabel llRemoveConflict;
    }
}
