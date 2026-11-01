using System.ComponentModel;
using ExControls;
using ToolsCore.Icons;

namespace ToolsCore.Forms
{
    partial class ColorSchemePage
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ColorSchemePage));
            toolStrip = new ToolStrip();
            tslScheme = new ToolStripLabel();
            tscbScheme = new ExToolStripComboBox();
            tsddbScheme = new IconToolStripDropDownButton();
            tsmiDuplicate = new ToolStripMenuItem();
            tsmiRename = new ToolStripMenuItem();
            tsmiDelete = new ToolStripMenuItem();
            tssScheme = new ToolStripSeparator();
            tsmiResetScheme = new ToolStripMenuItem();
            tvKeys = new ExTreeView();
            pPreview = new Panel();
            pDetail = new Panel();
            lKey = new Label();
            cboxBold = new ExCheckBox();
            cboxFore = new ExCheckBox();
            csFore = new ExColorSelector();
            cboxBack = new ExCheckBox();
            csBack = new ExColorSelector();
            picContrast = new PictureBox();
            lContrast = new Label();
            llReset = new LinkLabel();
            lInherit = new Label();
            toolStrip.SuspendLayout();
            pDetail.SuspendLayout();
            ((ISupportInitialize)picContrast).BeginInit();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { tslScheme, tscbScheme, tsddbScheme });
            resources.ApplyResources(toolStrip, "toolStrip");
            toolStrip.Name = "toolStrip";
            //
            // tslScheme
            //
            tslScheme.Name = "tslScheme";
            resources.ApplyResources(tslScheme, "tslScheme");
            //
            // tscbScheme
            //
            tscbScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            tscbScheme.FlatStyle = FlatStyle.Standard;
            tscbScheme.Name = "tscbScheme";
            resources.ApplyResources(tscbScheme, "tscbScheme");
            tscbScheme.SelectedIndexChanged += TscbScheme_SelectedIndexChanged;
            //
            // tsddbScheme
            //
            tsddbScheme.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsddbScheme.DropDownItems.AddRange(new ToolStripItem[] { tsmiDuplicate, tsmiRename, tsmiDelete, tssScheme, tsmiResetScheme });
            tsddbScheme.IconName = "settings";
            tsddbScheme.Name = "tsddbScheme";
            resources.ApplyResources(tsddbScheme, "tsddbScheme");
            //
            // tsmiDuplicate
            //
            tsmiDuplicate.Name = "tsmiDuplicate";
            resources.ApplyResources(tsmiDuplicate, "tsmiDuplicate");
            tsmiDuplicate.Click += TsmiDuplicate_Click;
            //
            // tsmiRename
            //
            tsmiRename.Name = "tsmiRename";
            resources.ApplyResources(tsmiRename, "tsmiRename");
            tsmiRename.Click += TsmiRename_Click;
            //
            // tsmiDelete
            //
            tsmiDelete.Name = "tsmiDelete";
            resources.ApplyResources(tsmiDelete, "tsmiDelete");
            tsmiDelete.Click += TsmiDelete_Click;
            //
            // tssScheme
            //
            tssScheme.Name = "tssScheme";
            resources.ApplyResources(tssScheme, "tssScheme");
            //
            // tsmiResetScheme
            //
            tsmiResetScheme.Name = "tsmiResetScheme";
            resources.ApplyResources(tsmiResetScheme, "tsmiResetScheme");
            tsmiResetScheme.Click += TsmiResetScheme_Click;
            //
            // tvKeys
            //
            resources.ApplyResources(tvKeys, "tvKeys");
            tvKeys.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvKeys.FullRowSelect = false;
            tvKeys.HideSelection = false;
            tvKeys.ItemHeight = 22;
            tvKeys.Name = "tvKeys";
            tvKeys.ShowLines = false;
            tvKeys.Style = ExTreeViewStyle.Light;
            tvKeys.DrawNode += TvKeys_DrawNode;
            tvKeys.AfterSelect += TvKeys_AfterSelect;
            //
            // pPreview
            //
            resources.ApplyResources(pPreview, "pPreview");
            pPreview.Name = "pPreview";
            //
            // pDetail
            //
            pDetail.Controls.Add(lKey);
            pDetail.Controls.Add(cboxBold);
            pDetail.Controls.Add(cboxFore);
            pDetail.Controls.Add(csFore);
            pDetail.Controls.Add(cboxBack);
            pDetail.Controls.Add(csBack);
            pDetail.Controls.Add(picContrast);
            pDetail.Controls.Add(lContrast);
            pDetail.Controls.Add(llReset);
            pDetail.Controls.Add(lInherit);
            resources.ApplyResources(pDetail, "pDetail");
            pDetail.Name = "pDetail";
            //
            // lKey
            //
            resources.ApplyResources(lKey, "lKey");
            lKey.Name = "lKey";
            //
            // cboxBold
            //
            resources.ApplyResources(cboxBold, "cboxBold");
            cboxBold.BoxBackColor = Color.White;
            cboxBold.HighlightColor = SystemColors.Highlight;
            cboxBold.Name = "cboxBold";
            cboxBold.CheckedChanged += Detail_Changed;
            //
            // cboxFore
            //
            resources.ApplyResources(cboxFore, "cboxFore");
            cboxFore.BoxBackColor = Color.White;
            cboxFore.HighlightColor = SystemColors.Highlight;
            cboxFore.Name = "cboxFore";
            cboxFore.CheckedChanged += Detail_Changed;
            //
            // csFore
            //
            csFore.BorderColor = SystemColors.ButtonShadow;
            resources.ApplyResources(csFore, "csFore");
            csFore.Name = "csFore";
            csFore.SelectedColorChanged += Detail_Changed;
            //
            // cboxBack
            //
            resources.ApplyResources(cboxBack, "cboxBack");
            cboxBack.BoxBackColor = Color.White;
            cboxBack.HighlightColor = SystemColors.Highlight;
            cboxBack.Name = "cboxBack";
            cboxBack.CheckedChanged += Detail_Changed;
            //
            // csBack
            //
            csBack.BorderColor = SystemColors.ButtonShadow;
            resources.ApplyResources(csBack, "csBack");
            csBack.Name = "csBack";
            csBack.SelectedColorChanged += Detail_Changed;
            //
            // picContrast
            //
            resources.ApplyResources(picContrast, "picContrast");
            picContrast.Name = "picContrast";
            picContrast.SizeMode = PictureBoxSizeMode.CenterImage;
            picContrast.TabStop = false;
            //
            // lContrast
            //
            resources.ApplyResources(lContrast, "lContrast");
            lContrast.Name = "lContrast";
            //
            // llReset
            //
            resources.ApplyResources(llReset, "llReset");
            llReset.Name = "llReset";
            llReset.TabStop = true;
            llReset.LinkClicked += LlReset_LinkClicked;
            //
            // lInherit
            //
            resources.ApplyResources(lInherit, "lInherit");
            lInherit.Name = "lInherit";
            //
            // ColorSchemePage
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tvKeys);
            Controls.Add(pDetail);
            Controls.Add(pPreview);
            Controls.Add(toolStrip);
            Name = "ColorSchemePage";
            resources.ApplyResources(this, "$this");
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            pDetail.ResumeLayout(false);
            pDetail.PerformLayout();
            ((ISupportInitialize)picContrast).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripLabel tslScheme;
        private ExToolStripComboBox tscbScheme;
        private IconToolStripDropDownButton tsddbScheme;
        private ToolStripMenuItem tsmiDuplicate;
        private ToolStripMenuItem tsmiRename;
        private ToolStripMenuItem tsmiDelete;
        private ToolStripSeparator tssScheme;
        private ToolStripMenuItem tsmiResetScheme;
        private ExTreeView tvKeys;
        private Panel pPreview;
        private Panel pDetail;
        private Label lKey;
        private ExCheckBox cboxBold;
        private ExCheckBox cboxFore;
        private ExColorSelector csFore;
        private ExCheckBox cboxBack;
        private ExColorSelector csBack;
        private PictureBox picContrast;
        private Label lContrast;
        private LinkLabel llReset;
        private Label lInherit;
    }
}
