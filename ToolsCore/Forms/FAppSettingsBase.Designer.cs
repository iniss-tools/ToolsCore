using ToolsCore.XML;
using ExControls;
using ToolsCore.Properties;
using ToolsCore.Icons;

namespace ToolsCore.Forms
{
    partial class FAppSettingsBase
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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FAppSettingsBase));
            OptionsNode optionsNode1 = new OptionsNode();
            OptionsNode optionsNode3 = new OptionsNode();
            OptionsNode optionsNode7 = new OptionsNode();
            OptionsNode optionsNode8 = new OptionsNode();
            OptionsNode optionsNode9 = new OptionsNode();
            panelBottom = new Panel();
            bSave = new ExButton();
            bStorno = new ExButton();
            bApply = new ExButton();
            bManage = new ExDropDownButton();
            cmsManage = new ContextMenuStrip(components);
            tsmiExport = new ToolStripMenuItem();
            tsmiImport = new ToolStripMenuItem();
            tssManage = new ToolStripSeparator();
            tsmiResetAll = new ToolStripMenuItem();
            optionsView = new ExOptionsView();
            pGeneral = new ExOptionsPanel(optionsView);
            pConcreteGeneral = new Panel();
            gboxGeneralProgram = new ExGroupBox();
            configBindingSource = new BindingSource(components);
            cboxReopenLast = new ExCheckBox();
            cboxMultipleInstances = new ExCheckBox();
            cboxCheckUpdates = new ExCheckBox();
            bCheckNow = new ExButton();
            lLastCheck = new Label();
            cbAppLanguage = new ExComboBox();
            label2 = new Label();
            pShortcuts = new ExOptionsPanel(optionsView);
            keymapPage = new KeymapPage();
            fontsPage = new FontsPage();
            colorSchemePage = new ColorSchemePage();
            cboxFollowSystem = new ExCheckBox();
            lLightScheme = new Label();
            cbLightScheme = new ExComboBox();
            lDarkScheme = new Label();
            cbDarkScheme = new ExComboBox();
            pStyles = new ExOptionsPanel(optionsView);
            pFonts = new ExOptionsPanel(optionsView);
            pAppearance = new ExOptionsPanel(optionsView);
            gbAppearance = new ExGroupBox();
            lStyle = new Label();
            cbStyle = new ExComboBox();
            cboxShowToolbar = new ExCheckBox();
            appLanguageBindingSource = new BindingSource(components);
            panelBottom.SuspendLayout();
            ((ISupportInitialize)optionsView).BeginInit();
            pGeneral.SuspendLayout();
            gboxGeneralProgram.SuspendLayout();
            ((ISupportInitialize)configBindingSource).BeginInit();
            pShortcuts.SuspendLayout();
            pAppearance.SuspendLayout();
            gbAppearance.SuspendLayout();
            pStyles.SuspendLayout();
            pFonts.SuspendLayout();
            ((ISupportInitialize)appLanguageBindingSource).BeginInit();
            SuspendLayout();
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(bSave);
            panelBottom.Controls.Add(bStorno);
            panelBottom.Controls.Add(bApply);
            panelBottom.Controls.Add(bManage);
            resources.ApplyResources(panelBottom, "panelBottom");
            panelBottom.Margin = new Padding(4, 3, 4, 3);
            panelBottom.Name = "panelBottom";
            // 
            // bSave
            // 
            resources.ApplyResources(bSave, "bSave");
            bSave.Margin = new Padding(4, 3, 4, 3);
            bSave.Name = "bSave";
            bSave.UseVisualStyleBackColor = true;
            bSave.Click += BSave_Click;
            // 
            // bStorno
            // 
            resources.ApplyResources(bStorno, "bStorno");
            bStorno.DialogResult = DialogResult.Cancel;
            bStorno.Margin = new Padding(4, 3, 4, 3);
            bStorno.Name = "bStorno";
            bStorno.UseVisualStyleBackColor = true;
            bStorno.Click += BStorno_Click;
            //
            // bApply
            //
            resources.ApplyResources(bApply, "bApply");
            bApply.Margin = new Padding(4, 3, 4, 3);
            bApply.Name = "bApply";
            bApply.UseVisualStyleBackColor = true;
            bApply.Click += BApply_Click;
            //
            // bManage
            //
            resources.ApplyResources(bManage, "bManage");
            bManage.DropDownMenu = cmsManage;
            bManage.Margin = new Padding(4, 3, 4, 3);
            bManage.Name = "bManage";
            bManage.UseVisualStyleBackColor = true;
            //
            // cmsManage
            //
            cmsManage.Items.AddRange(new ToolStripItem[] { tsmiExport, tsmiImport, tssManage, tsmiResetAll });
            cmsManage.Name = "cmsManage";
            resources.ApplyResources(cmsManage, "cmsManage");
            //
            // tsmiExport
            //
            tsmiExport.Name = "tsmiExport";
            resources.ApplyResources(tsmiExport, "tsmiExport");
            tsmiExport.Click += TsmiExport_Click;
            //
            // tsmiImport
            //
            tsmiImport.Name = "tsmiImport";
            resources.ApplyResources(tsmiImport, "tsmiImport");
            tsmiImport.Click += TsmiImport_Click;
            //
            // tssManage
            //
            tssManage.Name = "tssManage";
            resources.ApplyResources(tssManage, "tssManage");
            //
            // tsmiResetAll
            //
            tsmiResetAll.Name = "tsmiResetAll";
            resources.ApplyResources(tsmiResetAll, "tsmiResetAll");
            tsmiResetAll.Click += TsmiResetAll_Click;
            // 
            // optionsView
            // 
            resources.ApplyResources(optionsView, "optionsView");
            optionsView.HeaderNodeNameBackColor = SystemColors.Control;
            optionsView.HeaderNodeNameFont = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            optionsView.HeaderNodeNameForeColor = SystemColors.ControlText;
            optionsView.LinkToChildrenForeColor = Color.Empty;
            optionsView.Margin = new Padding(4, 3, 4, 3);
            optionsView.Name = "optionsView";
            optionsView.Padding = new Padding(6);
            optionsView.TreeWidth = 230;
            optionsView.Panels.Add(pGeneral);
            optionsView.Panels.Add(pAppearance);
            optionsView.Panels.Add(pStyles);
            optionsView.Panels.Add(pFonts);
            optionsView.Panels.Add(pShortcuts);
            // 
            // optionsView.ToolStripMenu
            // 
            optionsView.ToolStripMenu.BackColor = SystemColors.Control;
            optionsView.ToolStripMenu.Dock = DockStyle.Fill;
            optionsView.ToolStripMenu.GripStyle = ToolStripGripStyle.Hidden;
            optionsView.ToolStripMenu.ImageScalingSize = new Size(20, 20);
            optionsView.ToolStripMenu.Location = new Point(534, 0);
            optionsView.ToolStripMenu.Name = "ToolStripMenu";
            optionsView.ToolStripMenu.Size = new Size(102, 25);
            optionsView.ToolStripMenu.TabIndex = 1;
            // 
            // 
            // 
            optionsView.TreeView.Dock = DockStyle.Fill;
            optionsView.TreeView.FullRowSelect = true;
            optionsView.TreeView.HideSelection = false;
            optionsView.TreeView.ItemHeight = 22;
            optionsView.TreeView.Name = "treeView";
            optionsView.TreeView.PathSeparator = " / ";
            optionsView.TreeView.ShowLines = false;
            optionsView.TreeView.ShowNodeToolTips = true;
            optionsView.TreeView.Style = ExTreeViewStyle.Light;
            optionsView.TreeView.TabIndex = 0;
            // 
            // pGeneral
            // 
            pGeneral.Controls.Add(pConcreteGeneral);
            pGeneral.Controls.Add(gboxGeneralProgram);
            pGeneral.Margin = new Padding(4, 3, 4, 3);
            pGeneral.Name = "pGeneral";
            optionsNode1.Name = "";
            optionsNode1.Text = resources.GetString("optionsNode1.Text");
            pGeneral.Node = optionsNode1;
            pGeneral.NodeText = resources.GetString("pGeneral.NodeText");
            pGeneral.ParentNode = null;
            // 
            // pConcreteGeneral
            // 
            resources.ApplyResources(pConcreteGeneral, "pConcreteGeneral");
            pConcreteGeneral.Margin = new Padding(4, 3, 4, 3);
            pConcreteGeneral.Name = "pConcreteGeneral";
            // 
            // gboxGeneralProgram
            // 
            resources.ApplyResources(gboxGeneralProgram, "gboxGeneralProgram");
            gboxGeneralProgram.Controls.Add(cbAppLanguage);
            gboxGeneralProgram.Controls.Add(label2);
            gboxGeneralProgram.Controls.Add(cboxReopenLast);
            gboxGeneralProgram.Controls.Add(cboxMultipleInstances);
            gboxGeneralProgram.Controls.Add(cboxCheckUpdates);
            gboxGeneralProgram.Controls.Add(bCheckNow);
            gboxGeneralProgram.Controls.Add(lLastCheck);
            gboxGeneralProgram.DisabledForeColor = SystemColors.GrayText;
            gboxGeneralProgram.Margin = new Padding(4, 3, 4, 3);
            gboxGeneralProgram.Name = "gboxGeneralProgram";
            gboxGeneralProgram.Padding = new Padding(4, 3, 4, 3);
            gboxGeneralProgram.TabStop = false;
            // 
            // configBindingSource
            // 
            configBindingSource.DataSource = typeof(ConfigBase);
            //
            // cboxReopenLast
            //
            resources.ApplyResources(cboxReopenLast, "cboxReopenLast");
            cboxReopenLast.BoxBackColor = Color.White;
            cboxReopenLast.HighlightColor = SystemColors.Highlight;
            cboxReopenLast.Margin = new Padding(4, 3, 4, 3);
            cboxReopenLast.Name = "cboxReopenLast";
            // 
            // cboxMultipleInstances
            // 
            resources.ApplyResources(cboxMultipleInstances, "cboxMultipleInstances");
            cboxMultipleInstances.BoxBackColor = Color.White;
            cboxMultipleInstances.DataBindings.Add(new Binding("Checked", configBindingSource, "MoreInstance", true));
            cboxMultipleInstances.HighlightColor = SystemColors.Highlight;
            cboxMultipleInstances.Margin = new Padding(4, 3, 4, 3);
            cboxMultipleInstances.Name = "cboxMultipleInstances";
            //
            // cboxCheckUpdates
            //
            resources.ApplyResources(cboxCheckUpdates, "cboxCheckUpdates");
            cboxCheckUpdates.BoxBackColor = Color.White;
            cboxCheckUpdates.DataBindings.Add(new Binding("Checked", configBindingSource, "CheckUpdates", true));
            cboxCheckUpdates.HighlightColor = SystemColors.Highlight;
            cboxCheckUpdates.Margin = new Padding(4, 3, 4, 3);
            cboxCheckUpdates.Name = "cboxCheckUpdates";
            //
            // bCheckNow
            //
            resources.ApplyResources(bCheckNow, "bCheckNow");
            bCheckNow.Margin = new Padding(4, 3, 4, 3);
            bCheckNow.Name = "bCheckNow";
            bCheckNow.UseVisualStyleBackColor = true;
            bCheckNow.Click += BCheckNow_Click;
            //
            // lLastCheck
            //
            resources.ApplyResources(lLastCheck, "lLastCheck");
            lLastCheck.Name = "lLastCheck";
            // 
            // cbAppLanguage
            // 
            cbAppLanguage.DataBindings.Add(new Binding("SelectedValue", configBindingSource, "Language", true));
            cbAppLanguage.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbAppLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAppLanguage.FormattingEnabled = true;
            resources.ApplyResources(cbAppLanguage, "cbAppLanguage");
            cbAppLanguage.Margin = new Padding(4, 3, 4, 3);
            cbAppLanguage.Name = "cbAppLanguage";
            cbAppLanguage.StyleDisabled.ArrowColor = null;
            cbAppLanguage.StyleDisabled.BackColor = null;
            cbAppLanguage.StyleDisabled.BorderColor = null;
            cbAppLanguage.StyleDisabled.ButtonBackColor = null;
            cbAppLanguage.StyleDisabled.ButtonBorderColor = null;
            cbAppLanguage.StyleDisabled.ButtonRenderFirst = null;
            cbAppLanguage.StyleDisabled.ForeColor = null;
            cbAppLanguage.StyleHighlight.ArrowColor = null;
            cbAppLanguage.StyleHighlight.BackColor = null;
            cbAppLanguage.StyleHighlight.BorderColor = null;
            cbAppLanguage.StyleHighlight.ButtonBackColor = null;
            cbAppLanguage.StyleHighlight.ButtonBorderColor = null;
            cbAppLanguage.StyleHighlight.ButtonRenderFirst = null;
            cbAppLanguage.StyleHighlight.ForeColor = null;
            cbAppLanguage.StyleNormal.ArrowColor = null;
            cbAppLanguage.StyleNormal.BackColor = null;
            cbAppLanguage.StyleNormal.BorderColor = null;
            cbAppLanguage.StyleNormal.ButtonBackColor = null;
            cbAppLanguage.StyleNormal.ButtonBorderColor = null;
            cbAppLanguage.StyleNormal.ButtonRenderFirst = null;
            cbAppLanguage.StyleNormal.ForeColor = null;
            cbAppLanguage.StyleSelected.ArrowColor = null;
            cbAppLanguage.StyleSelected.BackColor = null;
            cbAppLanguage.StyleSelected.BorderColor = null;
            cbAppLanguage.StyleSelected.ButtonBackColor = null;
            cbAppLanguage.StyleSelected.ButtonBorderColor = null;
            cbAppLanguage.StyleSelected.ButtonRenderFirst = null;
            cbAppLanguage.StyleSelected.ForeColor = null;
            cbAppLanguage.UseDarkScrollBar = false;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            // 
            // pShortcuts
            // 
            pShortcuts.Controls.Add(keymapPage);
            pShortcuts.Margin = new Padding(4, 3, 4, 3);
            pShortcuts.Name = "pShortcuts";
            optionsNode7.Name = "";
            optionsNode7.Text = resources.GetString("optionsNode7.Text");
            pShortcuts.Node = optionsNode7;
            pShortcuts.NodeText = resources.GetString("pShortcuts.NodeText");
            pShortcuts.ParentNode = null;
            // 
            // keymapPage
            // 
            resources.ApplyResources(keymapPage, "keymapPage");
            keymapPage.Name = "keymapPage";
            // 
            // pStyles
            // 
            pStyles.Controls.Add(colorSchemePage);
            pStyles.Margin = new Padding(4, 3, 4, 3);
            pStyles.Name = "pStyles";
            optionsNode8.Name = "";
            optionsNode8.Text = resources.GetString("optionsNode8.Text");
            pStyles.Node = optionsNode8;
            pStyles.NodeText = resources.GetString("pStyles.NodeText");
            pStyles.ParentNode = optionsNode3;
            //
            // colorSchemePage
            //
            resources.ApplyResources(colorSchemePage, "colorSchemePage");
            colorSchemePage.Name = "colorSchemePage";
            colorSchemePage.SchemesChanged += ColorSchemePage_SchemesChanged;
            // 
            // pFonts
            // 
            pFonts.Controls.Add(fontsPage);
            pFonts.Margin = new Padding(4, 3, 4, 3);
            pFonts.Name = "pFonts";
            optionsNode9.Name = "";
            optionsNode9.Text = resources.GetString("optionsNode9.Text");
            pFonts.Node = optionsNode9;
            pFonts.NodeText = resources.GetString("pFonts.NodeText");
            pFonts.ParentNode = optionsNode3;
            //
            // fontsPage
            //
            resources.ApplyResources(fontsPage, "fontsPage");
            fontsPage.Name = "fontsPage";
            //
            // pAppearance
            //
            pAppearance.Controls.Add(gbAppearance);
            pAppearance.Margin = new Padding(4, 3, 4, 3);
            pAppearance.Name = "pAppearance";
            optionsNode3.Name = "";
            optionsNode3.Text = resources.GetString("optionsNode3.Text");
            pAppearance.Node = optionsNode3;
            pAppearance.NodeText = resources.GetString("pAppearance.NodeText");
            pAppearance.ParentNode = null;
            //
            // gbAppearance
            //
            resources.ApplyResources(gbAppearance, "gbAppearance");
            gbAppearance.Controls.Add(cboxShowToolbar);
            gbAppearance.Controls.Add(cbDarkScheme);
            gbAppearance.Controls.Add(lDarkScheme);
            gbAppearance.Controls.Add(cbLightScheme);
            gbAppearance.Controls.Add(lLightScheme);
            gbAppearance.Controls.Add(cboxFollowSystem);
            gbAppearance.Controls.Add(cbStyle);
            gbAppearance.Controls.Add(lStyle);
            gbAppearance.DisabledForeColor = SystemColors.GrayText;
            gbAppearance.Margin = new Padding(4, 3, 4, 3);
            gbAppearance.Name = "gbAppearance";
            gbAppearance.Padding = new Padding(4, 3, 4, 3);
            gbAppearance.TabStop = false;
            //
            // lStyle
            //
            resources.ApplyResources(lStyle, "lStyle");
            lStyle.Margin = new Padding(4, 0, 4, 0);
            lStyle.Name = "lStyle";
            //
            // cbStyle
            //
            cbStyle.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStyle.FormattingEnabled = true;
            resources.ApplyResources(cbStyle, "cbStyle");
            cbStyle.Margin = new Padding(4, 3, 4, 3);
            cbStyle.Name = "cbStyle";
            cbStyle.UseDarkScrollBar = false;
            cbStyle.SelectionChangeCommitted += CbStyle_SelectionChangeCommitted;
            //
            // cboxFollowSystem
            //
            resources.ApplyResources(cboxFollowSystem, "cboxFollowSystem");
            cboxFollowSystem.BoxBackColor = Color.White;
            cboxFollowSystem.HighlightColor = SystemColors.Highlight;
            cboxFollowSystem.Margin = new Padding(4, 3, 4, 3);
            cboxFollowSystem.Name = "cboxFollowSystem";
            cboxFollowSystem.CheckedChanged += CboxFollowSystem_CheckedChanged;
            //
            // lLightScheme
            //
            resources.ApplyResources(lLightScheme, "lLightScheme");
            lLightScheme.Margin = new Padding(4, 0, 4, 0);
            lLightScheme.Name = "lLightScheme";
            //
            // cbLightScheme
            //
            cbLightScheme.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbLightScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cbLightScheme.FormattingEnabled = true;
            resources.ApplyResources(cbLightScheme, "cbLightScheme");
            cbLightScheme.Margin = new Padding(4, 3, 4, 3);
            cbLightScheme.Name = "cbLightScheme";
            cbLightScheme.UseDarkScrollBar = false;
            cbLightScheme.SelectionChangeCommitted += CbSystemScheme_SelectionChangeCommitted;
            //
            // lDarkScheme
            //
            resources.ApplyResources(lDarkScheme, "lDarkScheme");
            lDarkScheme.Margin = new Padding(4, 0, 4, 0);
            lDarkScheme.Name = "lDarkScheme";
            //
            // cbDarkScheme
            //
            cbDarkScheme.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbDarkScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDarkScheme.FormattingEnabled = true;
            resources.ApplyResources(cbDarkScheme, "cbDarkScheme");
            cbDarkScheme.Margin = new Padding(4, 3, 4, 3);
            cbDarkScheme.Name = "cbDarkScheme";
            cbDarkScheme.UseDarkScrollBar = false;
            cbDarkScheme.SelectionChangeCommitted += CbSystemScheme_SelectionChangeCommitted;
            //
            // cboxShowToolbar
            //
            resources.ApplyResources(cboxShowToolbar, "cboxShowToolbar");
            cboxShowToolbar.BoxBackColor = Color.White;
            cboxShowToolbar.DataBindings.Add(new Binding("Checked", configBindingSource, "ShowToolbar", true));
            cboxShowToolbar.HighlightColor = SystemColors.Highlight;
            cboxShowToolbar.Margin = new Padding(4, 3, 4, 3);
            cboxShowToolbar.Name = "cboxShowToolbar";
            // 
            // appLanguageBindingSource
            // 
            appLanguageBindingSource.DataSource = typeof(AppLanguage);
            // 
            // FAppSettingsBase
            // 
            AcceptButton = bSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = bStorno;
            resources.ApplyResources(this, "$this");
            Controls.Add(optionsView);
            Controls.Add(panelBottom);
            HelpButton = true;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(903, 587);
            Name = "FAppSettingsBase";
            ShowIcon = false;
            HelpButtonClicked += FAppSettingsBase_HelpButtonClicked;
            Load += FAppSettingsBase_Load;
            panelBottom.ResumeLayout(false);
            ((ISupportInitialize)optionsView).EndInit();
            pGeneral.ResumeLayout(false);
            pGeneral.PerformLayout();
            gboxGeneralProgram.ResumeLayout(false);
            gboxGeneralProgram.PerformLayout();
            ((ISupportInitialize)configBindingSource).EndInit();
            pShortcuts.ResumeLayout(false);
            pAppearance.ResumeLayout(false);
            pAppearance.PerformLayout();
            gbAppearance.ResumeLayout(false);
            gbAppearance.PerformLayout();
            pStyles.ResumeLayout(false);
            pFonts.ResumeLayout(false);
            ((ISupportInitialize)appLanguageBindingSource).EndInit();
            ResumeLayout(false);

        }

        #endregion
        private ExControls.ExButton bSave;
        private ExControls.ExButton bStorno;
        private ExControls.ExButton bApply;
        private ExControls.ExDropDownButton bManage;
        private ContextMenuStrip cmsManage;
        private ToolStripMenuItem tsmiExport;
        private ToolStripMenuItem tsmiImport;
        private ToolStripSeparator tssManage;
        private ToolStripMenuItem tsmiResetAll;
        private Panel panelBottom;
        protected ExControls.ExOptionsPanel pGeneral;
        private Label label2;
        private ExControls.ExGroupBox gboxGeneralProgram;
        private ExControls.ExCheckBox cboxMultipleInstances;
        private ExControls.ExCheckBox cboxCheckUpdates;
        private ExButton bCheckNow;
        private Label lLastCheck;
        private BindingSource appLanguageBindingSource;
        private ExComboBox cbAppLanguage;
        protected Panel pConcreteGeneral;
        protected ExOptionsView optionsView;
        protected ExOptionsPanel pShortcuts;
        private KeymapPage keymapPage;
        private FontsPage fontsPage;
        private ColorSchemePage colorSchemePage;
        private ExCheckBox cboxFollowSystem;
        private Label lLightScheme;
        private ExComboBox cbLightScheme;
        private Label lDarkScheme;
        private ExComboBox cbDarkScheme;
        protected ExOptionsPanel pStyles;
        protected ExOptionsPanel pFonts;
        private ExOptionsPanel pAppearance;
        private ExGroupBox gbAppearance;
        private Label lStyle;
        private ExComboBox cbStyle;
        private ExCheckBox cboxShowToolbar;
        private ExCheckBox cboxReopenLast;
        private BindingSource configBindingSource;
    }
}