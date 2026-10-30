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
            OptionsNode optionsNode5 = new OptionsNode();
            OptionsNode optionsNode7 = new OptionsNode();
            OptionsNode optionsNode8 = new OptionsNode();
            OptionsNode optionsNode9 = new OptionsNode();
            panelBottom = new Panel();
            bSave = new ExButton();
            bStorno = new ExButton();
            optionsView = new ExOptionsView();
            pGeneral = new ExOptionsPanel(optionsView);
            pConcreteGeneral = new Panel();
            gboxGeneralProgram = new ExGroupBox();
            configBindingSource = new BindingSource(components);
            cboxReopenLast = new ExCheckBox();
            cboxMultipleInstances = new ExCheckBox();
            cboxCheckUpdates = new ExCheckBox();
            pDesktopColumns = new ExOptionsPanel(optionsView);
            dgvColumns = new DataGridView();
            cColName = new DataGridViewTextBoxColumn();
            cColVisible = new DataGridViewExCheckBoxColumn();
            cColMinWidth = new DataGridViewTextBoxColumn();
            desktopColumnBindingSource = new BindingSource(components);
            panel2 = new Panel();
            bResetColumns = new ExButton();
            bColDown = new ExButton();
            bColUp = new ExButton();
            cbAppLanguage = new ExComboBox();
            label2 = new Label();
            pShortcuts = new ExOptionsPanel(optionsView);
            keymapPage = new KeymapPage();
            fontsPage = new FontsPage();
            pStyles = new ExOptionsPanel(optionsView);
            ppStyles = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            tscbStyles = new ExToolStripComboBox();
            tsbApplyStyle = new IconToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tsbAddStyle = new IconToolStripButton();
            tsbRenameStyle = new IconToolStripButton();
            tsbDeleteStyle = new IconToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsbResetStyle = new IconToolStripButton();
            exGroupBox1 = new ExGroupBox();
            lFontStyleExample = new Label();
            tvStyles = new ExTreeView();
            gbColorItemSettings = new ExGroupBox();
            bResetColorSetting = new ExButton();
            cboxBold = new ExCheckBox();
            csForeColor = new ExColorSelector();
            lStyleBackColor = new ExLabel();
            lStyleForeColor = new ExLabel();
            csBackColor = new ExColorSelector();
            exGroupBox8 = new ExGroupBox();
            cboxDarkScrollbars = new ExCheckBox();
            cboxDarkTitlebar = new ExCheckBox();
            cboxDefaultVisual = new ExCheckBox();
            pFonts = new ExOptionsPanel(optionsView);
            pAppearance = new ExOptionsPanel(optionsView);
            gbAppearance = new ExGroupBox();
            lStyle = new Label();
            cbStyle = new ExComboBox();
            cboxShowToolbar = new ExCheckBox();
            appLanguageBindingSource = new BindingSource(components);
            styleBindingSource = new BindingSource(components);
            panelBottom.SuspendLayout();
            ((ISupportInitialize)optionsView).BeginInit();
            pGeneral.SuspendLayout();
            gboxGeneralProgram.SuspendLayout();
            ((ISupportInitialize)configBindingSource).BeginInit();
            pDesktopColumns.SuspendLayout();
            ((ISupportInitialize)dgvColumns).BeginInit();
            ((ISupportInitialize)desktopColumnBindingSource).BeginInit();
            panel2.SuspendLayout();
            pShortcuts.SuspendLayout();
            pAppearance.SuspendLayout();
            gbAppearance.SuspendLayout();
            pStyles.SuspendLayout();
            ppStyles.SuspendLayout();
            toolStrip1.SuspendLayout();
            exGroupBox1.SuspendLayout();
            gbColorItemSettings.SuspendLayout();
            exGroupBox8.SuspendLayout();
            pFonts.SuspendLayout();
            ((ISupportInitialize)appLanguageBindingSource).BeginInit();
            ((ISupportInitialize)styleBindingSource).BeginInit();
            SuspendLayout();
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(bSave);
            panelBottom.Controls.Add(bStorno);
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
            optionsView.Panels.Add(pDesktopColumns);
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
            optionsView.ToolStripMenu.Text = "toolStrip1";
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
            // pDesktopColumns
            // 
            pDesktopColumns.Controls.Add(dgvColumns);
            pDesktopColumns.Controls.Add(panel2);
            pDesktopColumns.Margin = new Padding(4, 3, 4, 3);
            pDesktopColumns.Name = "pDesktopColumns";
            optionsNode5.Name = "";
            optionsNode5.Text = resources.GetString("optionsNode5.Text");
            pDesktopColumns.Node = optionsNode5;
            pDesktopColumns.NodeText = resources.GetString("pDesktopColumns.NodeText");
            pDesktopColumns.ParentNode = null;
            // 
            // dgvColumns
            // 
            dgvColumns.AllowUserToAddRows = false;
            dgvColumns.AllowUserToDeleteRows = false;
            dgvColumns.AllowUserToResizeRows = false;
            dgvColumns.AutoGenerateColumns = false;
            dgvColumns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvColumns.Columns.AddRange(new DataGridViewColumn[] { cColName, cColVisible, cColMinWidth });
            dgvColumns.DataSource = desktopColumnBindingSource;
            resources.ApplyResources(dgvColumns, "dgvColumns");
            dgvColumns.Margin = new Padding(4, 3, 4, 3);
            dgvColumns.Name = "dgvColumns";
            dgvColumns.RowHeadersVisible = false;
            dgvColumns.RowHeadersWidth = 51;
            dgvColumns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 
            // cColName
            // 
            cColName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            cColName.DataPropertyName = "Name";
            resources.ApplyResources(cColName, "cColName");
            cColName.MinimumWidth = 6;
            cColName.Name = "cColName";
            cColName.ReadOnly = true;
            // 
            // cColVisible
            // 
            cColVisible.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            cColVisible.DataPropertyName = "Visible";
            resources.ApplyResources(cColVisible, "cColVisible");
            cColVisible.HighlightColor = Color.FromArgb(0, 120, 215);
            cColVisible.MinimumWidth = 6;
            cColVisible.Name = "cColVisible";
            cColVisible.Resizable = DataGridViewTriState.True;
            cColVisible.SquareBackColor = Color.White;
            // 
            // cColMinWidth
            // 
            cColMinWidth.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            cColMinWidth.DataPropertyName = "MinWidth";
            resources.ApplyResources(cColMinWidth, "cColMinWidth");
            cColMinWidth.MinimumWidth = 6;
            cColMinWidth.Name = "cColMinWidth";
            // 
            // desktopColumnBindingSource
            // 
            desktopColumnBindingSource.DataSource = typeof(DesktopColumn);
            // 
            // panel2
            // 
            panel2.Controls.Add(bResetColumns);
            panel2.Controls.Add(bColDown);
            panel2.Controls.Add(bColUp);
            resources.ApplyResources(panel2, "panel2");
            panel2.Margin = new Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            // 
            // bResetColumns
            // 
            resources.ApplyResources(bResetColumns, "bResetColumns");
            bResetColumns.Margin = new Padding(4, 3, 4, 3);
            bResetColumns.Name = "bResetColumns";
            bResetColumns.UseVisualStyleBackColor = true;
            bResetColumns.Click += BResetColumns_Click;
            // 
            // bColDown
            // 
            resources.ApplyResources(bColDown, "bColDown");
            bColDown.Margin = new Padding(4, 3, 4, 3);
            bColDown.Name = "bColDown";
            bColDown.UseVisualStyleBackColor = true;
            bColDown.Click += BColDown_Click;
            // 
            // bColUp
            // 
            resources.ApplyResources(bColUp, "bColUp");
            bColUp.Margin = new Padding(4, 3, 4, 3);
            bColUp.Name = "bColUp";
            bColUp.UseVisualStyleBackColor = true;
            bColUp.Click += BColUp_Click;
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
            pStyles.Controls.Add(ppStyles);
            pStyles.Margin = new Padding(4, 3, 4, 3);
            pStyles.Name = "pStyles";
            optionsNode8.Name = "";
            optionsNode8.Text = resources.GetString("optionsNode8.Text");
            pStyles.Node = optionsNode8;
            pStyles.NodeText = resources.GetString("pStyles.NodeText");
            pStyles.ParentNode = optionsNode3;
            // 
            // ppStyles
            // 
            ppStyles.Controls.Add(toolStrip1);
            ppStyles.Controls.Add(exGroupBox1);
            ppStyles.Controls.Add(tvStyles);
            ppStyles.Controls.Add(gbColorItemSettings);
            ppStyles.Controls.Add(exGroupBox8);
            resources.ApplyResources(ppStyles, "ppStyles");
            ppStyles.Margin = new Padding(4, 3, 4, 3);
            ppStyles.Name = "ppStyles";
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.Control;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, tscbStyles, tsbApplyStyle, toolStripSeparator1, tsbAddStyle, tsbRenameStyle, tsbDeleteStyle, toolStripSeparator2, tsbResetStyle });
            resources.ApplyResources(toolStrip1, "toolStrip1");
            toolStrip1.Name = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            resources.ApplyResources(toolStripLabel1, "toolStripLabel1");
            // 
            // tscbStyles
            // 
            tscbStyles.DropDownStyle = ComboBoxStyle.DropDownList;
            tscbStyles.DropDownWidth = 121;
            tscbStyles.FlatStyle = FlatStyle.Standard;
            tscbStyles.Name = "tscbStyles";
            resources.ApplyResources(tscbStyles, "tscbStyles");
            tscbStyles.SelectedIndexChanged += TscbStyles_SelectedIndexChanged;
            // 
            // tsbApplyStyle
            // 
            tsbApplyStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbApplyStyle.Name = "tsbApplyStyle";
            tsbApplyStyle.IconName = "checkmark";
            resources.ApplyResources(tsbApplyStyle, "tsbApplyStyle");
            tsbApplyStyle.Click += TsbApplyStyle_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsbAddStyle
            // 
            tsbAddStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbAddStyle.Name = "tsbAddStyle";
            tsbAddStyle.IconName = "add";
            resources.ApplyResources(tsbAddStyle, "tsbAddStyle");
            tsbAddStyle.Click += TsbAddStyle_Click;
            // 
            // tsbRenameStyle
            // 
            tsbRenameStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbRenameStyle.Name = "tsbRenameStyle";
            tsbRenameStyle.IconName = "rename";
            resources.ApplyResources(tsbRenameStyle, "tsbRenameStyle");
            tsbRenameStyle.Click += TsbRenameStyle_Click;
            // 
            // tsbDeleteStyle
            // 
            tsbDeleteStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbDeleteStyle.Name = "tsbDeleteStyle";
            tsbDeleteStyle.IconName = "delete";
            resources.ApplyResources(tsbDeleteStyle, "tsbDeleteStyle");
            tsbDeleteStyle.Click += TsbDeleteStyle_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            // 
            // tsbResetStyle
            // 
            tsbResetStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbResetStyle.Name = "tsbResetStyle";
            tsbResetStyle.IconName = "arrow-reset";
            resources.ApplyResources(tsbResetStyle, "tsbResetStyle");
            tsbResetStyle.Click += TsbResetStyle_Click;
            // 
            // exGroupBox1
            // 
            resources.ApplyResources(exGroupBox1, "exGroupBox1");
            exGroupBox1.Controls.Add(lFontStyleExample);
            exGroupBox1.DisabledForeColor = SystemColors.GrayText;
            exGroupBox1.Margin = new Padding(2);
            exGroupBox1.Name = "exGroupBox1";
            exGroupBox1.Padding = new Padding(2);
            exGroupBox1.TabStop = false;
            // 
            // lFontStyleExample
            // 
            resources.ApplyResources(lFontStyleExample, "lFontStyleExample");
            lFontStyleExample.BorderStyle = BorderStyle.FixedSingle;
            lFontStyleExample.Margin = new Padding(2, 0, 2, 0);
            lFontStyleExample.Name = "lFontStyleExample";
            lFontStyleExample.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tvStyles
            // 
            resources.ApplyResources(tvStyles, "tvStyles");
            tvStyles.FullRowSelect = true;
            tvStyles.HideSelection = false;
            tvStyles.Margin = new Padding(2);
            tvStyles.Name = "tvStyles";
            tvStyles.ShowLines = false;
            tvStyles.Style = ExTreeViewStyle.Light;
            tvStyles.AfterSelect += TvStyles_AfterSelect;
            // 
            // gbColorItemSettings
            // 
            resources.ApplyResources(gbColorItemSettings, "gbColorItemSettings");
            gbColorItemSettings.Controls.Add(bResetColorSetting);
            gbColorItemSettings.Controls.Add(cboxBold);
            gbColorItemSettings.Controls.Add(csForeColor);
            gbColorItemSettings.Controls.Add(lStyleBackColor);
            gbColorItemSettings.Controls.Add(lStyleForeColor);
            gbColorItemSettings.Controls.Add(csBackColor);
            gbColorItemSettings.DisabledForeColor = SystemColors.GrayText;
            gbColorItemSettings.Margin = new Padding(2);
            gbColorItemSettings.Name = "gbColorItemSettings";
            gbColorItemSettings.Padding = new Padding(2);
            gbColorItemSettings.TabStop = false;
            // 
            // bResetColorSetting
            // 
            resources.ApplyResources(bResetColorSetting, "bResetColorSetting");
            bResetColorSetting.Margin = new Padding(4, 3, 4, 3);
            bResetColorSetting.Name = "bResetColorSetting";
            bResetColorSetting.UseVisualStyleBackColor = true;
            bResetColorSetting.Click += BResetColorSetting_Click;
            // 
            // cboxBold
            // 
            resources.ApplyResources(cboxBold, "cboxBold");
            cboxBold.BoxBackColor = Color.White;
            cboxBold.HighlightColor = SystemColors.Highlight;
            cboxBold.Margin = new Padding(2);
            cboxBold.Name = "cboxBold";
            cboxBold.CheckedChanged += CboxBold_CheckedChanged;
            // 
            // csForeColor
            // 
            resources.ApplyResources(csForeColor, "csForeColor");
            csForeColor.BorderColor = SystemColors.ButtonShadow;
            csForeColor.Margin = new Padding(5);
            csForeColor.Name = "csForeColor";
            csForeColor.SelectedColorChanged += CsForeColor_SelectedColorChanged;
            // 
            // lStyleBackColor
            // 
            resources.ApplyResources(lStyleBackColor, "lStyleBackColor");
            lStyleBackColor.Margin = new Padding(2, 0, 2, 0);
            lStyleBackColor.Name = "lStyleBackColor";
            // 
            // lStyleForeColor
            // 
            resources.ApplyResources(lStyleForeColor, "lStyleForeColor");
            lStyleForeColor.Margin = new Padding(2, 0, 2, 0);
            lStyleForeColor.Name = "lStyleForeColor";
            // 
            // csBackColor
            // 
            resources.ApplyResources(csBackColor, "csBackColor");
            csBackColor.BorderColor = SystemColors.ButtonShadow;
            csBackColor.Margin = new Padding(5);
            csBackColor.Name = "csBackColor";
            csBackColor.SelectedColorChanged += CsBackColor_SelectedColorChanged;
            // 
            // exGroupBox8
            // 
            resources.ApplyResources(exGroupBox8, "exGroupBox8");
            exGroupBox8.Controls.Add(cboxDarkScrollbars);
            exGroupBox8.Controls.Add(cboxDarkTitlebar);
            exGroupBox8.Controls.Add(cboxDefaultVisual);
            exGroupBox8.DisabledForeColor = SystemColors.GrayText;
            exGroupBox8.Margin = new Padding(2);
            exGroupBox8.Name = "exGroupBox8";
            exGroupBox8.Padding = new Padding(2);
            exGroupBox8.TabStop = false;
            // 
            // cboxDarkScrollbars
            // 
            resources.ApplyResources(cboxDarkScrollbars, "cboxDarkScrollbars");
            cboxDarkScrollbars.BoxBackColor = Color.White;
            cboxDarkScrollbars.HighlightColor = SystemColors.Highlight;
            cboxDarkScrollbars.Margin = new Padding(2);
            cboxDarkScrollbars.Name = "cboxDarkScrollbars";
            cboxDarkScrollbars.CheckedChanged += CboxDarkScrollbars_CheckedChanged;
            // 
            // cboxDarkTitlebar
            // 
            resources.ApplyResources(cboxDarkTitlebar, "cboxDarkTitlebar");
            cboxDarkTitlebar.BoxBackColor = Color.White;
            cboxDarkTitlebar.HighlightColor = SystemColors.Highlight;
            cboxDarkTitlebar.Margin = new Padding(2);
            cboxDarkTitlebar.Name = "cboxDarkTitlebar";
            cboxDarkTitlebar.CheckedChanged += CboxDarkTitlebar_CheckedChanged;
            // 
            // cboxDefaultVisual
            // 
            resources.ApplyResources(cboxDefaultVisual, "cboxDefaultVisual");
            cboxDefaultVisual.BoxBackColor = Color.White;
            cboxDefaultVisual.HighlightColor = SystemColors.Highlight;
            cboxDefaultVisual.Margin = new Padding(2);
            cboxDefaultVisual.Name = "cboxDefaultVisual";
            cboxDefaultVisual.CheckedChanged += CboxDefaultVisual_CheckedChanged;
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
            // styleBindingSource
            // 
            styleBindingSource.DataSource = typeof(Style);
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
            pDesktopColumns.ResumeLayout(false);
            ((ISupportInitialize)dgvColumns).EndInit();
            ((ISupportInitialize)desktopColumnBindingSource).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            pShortcuts.ResumeLayout(false);
            pAppearance.ResumeLayout(false);
            pAppearance.PerformLayout();
            gbAppearance.ResumeLayout(false);
            gbAppearance.PerformLayout();
            pStyles.ResumeLayout(false);
            ppStyles.ResumeLayout(false);
            ppStyles.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            exGroupBox1.ResumeLayout(false);
            gbColorItemSettings.ResumeLayout(false);
            gbColorItemSettings.PerformLayout();
            exGroupBox8.ResumeLayout(false);
            exGroupBox8.PerformLayout();
            pFonts.ResumeLayout(false);
            ((ISupportInitialize)appLanguageBindingSource).EndInit();
            ((ISupportInitialize)styleBindingSource).EndInit();
            ResumeLayout(false);

        }

        #endregion
        private ExControls.ExButton bSave;
        private ExControls.ExButton bStorno;
        private Panel panelBottom;
        protected ExControls.ExOptionsPanel pGeneral;
        protected DataGridView dgvColumns;
        private Label label2;
        private ExControls.ExGroupBox gboxGeneralProgram;
        private ExControls.ExCheckBox cboxMultipleInstances;
        private ExControls.ExCheckBox cboxCheckUpdates;
        private BindingSource appLanguageBindingSource;
        private BindingSource desktopColumnBindingSource;
        private Panel panel2;
        private ExButton bColDown;
        private ExButton bColUp;
        private ExComboBox cbAppLanguage;
        protected Panel pConcreteGeneral;
        protected ExOptionsView optionsView;
        private Panel ppStyles;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabel1;
        private ExToolStripComboBox tscbStyles;
        private IconToolStripButton tsbApplyStyle;
        private ToolStripSeparator toolStripSeparator1;
        private IconToolStripButton tsbAddStyle;
        private IconToolStripButton tsbRenameStyle;
        private IconToolStripButton tsbDeleteStyle;
        private ExGroupBox exGroupBox1;
        private Label lFontStyleExample;
        private ExTreeView tvStyles;
        private ExGroupBox gbColorItemSettings;
        private ExCheckBox cboxBold;
        private ExColorSelector csForeColor;
        private ExLabel lStyleBackColor;
        private ExLabel lStyleForeColor;
        private ExColorSelector csBackColor;
        private ExGroupBox exGroupBox8;
        private ExCheckBox cboxDarkScrollbars;
        private ExCheckBox cboxDarkTitlebar;
        private ExCheckBox cboxDefaultVisual;
        private BindingSource styleBindingSource;
        private ExButton bResetColumns;
        protected ExOptionsPanel pShortcuts;
        private KeymapPage keymapPage;
        private FontsPage fontsPage;
        protected ExOptionsPanel pStyles;
        protected ExOptionsPanel pFonts;
        protected ExOptionsPanel pDesktopColumns;
        private ExOptionsPanel pAppearance;
        private ExGroupBox gbAppearance;
        private Label lStyle;
        private ExComboBox cbStyle;
        private ExCheckBox cboxShowToolbar;
        private ExCheckBox cboxReopenLast;
        private BindingSource configBindingSource;
        private DataGridViewTextBoxColumn cColName;
        private DataGridViewExCheckBoxColumn cColVisible;
        private DataGridViewTextBoxColumn cColMinWidth;
        private ExButton bResetColorSetting;
        private ToolStripSeparator toolStripSeparator2;
        private IconToolStripButton tsbResetStyle;
    }
}