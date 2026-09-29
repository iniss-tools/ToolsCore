using ToolsCore.XML;
using ExControls;
using ToolsCore.Properties;

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
            Label desktopMenuModeLabel;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FAppSettingsBase));
            OptionsNode optionsNode1 = new OptionsNode();
            OptionsNode optionsNode2 = new OptionsNode();
            OptionsNode optionsNode3 = new OptionsNode();
            OptionsNode optionsNode4 = new OptionsNode();
            OptionsNode optionsNode5 = new OptionsNode();
            OptionsNode optionsNode6 = new OptionsNode();
            OptionsNode optionsNode7 = new OptionsNode();
            OptionsNode optionsNode8 = new OptionsNode();
            OptionsNode optionsNode9 = new OptionsNode();
            OptionsNode optionsNode10 = new OptionsNode();
            panelBottom = new Panel();
            bSave = new ExButton();
            bStorno = new ExButton();
            listIcons = new ImageList(components);
            optionsView = new ExOptionsView();
            pGeneral = new ExOptionsPanel(optionsView);
            pConcreteGeneral = new Panel();
            gboxGeneralProgram = new ExGroupBox();
            cbStartup = new ExComboBox();
            configBindingSource = new BindingSource(components);
            label3 = new Label();
            cboxClassicGui = new ExCheckBox();
            cboxMultipleInstances = new ExCheckBox();
            pDesktop = new ExOptionsPanel(optionsView);
            pDesktopComponents = new ExOptionsPanel(optionsView);
            pConcreteDesktopComponents = new Panel();
            ppDesktopComponents = new Panel();
            rbMenuToolStrip = new ExRadioButton();
            cboxShowRowHeaders = new ExCheckBox();
            rbMenuOnly = new ExRadioButton();
            rbToolOnly = new ExRadioButton();
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
            cboxFitLastCol = new ExCheckBox();
            pLocalization = new ExOptionsPanel(optionsView);
            pConcreteLocalization = new Panel();
            panel3 = new Panel();
            cbAppLanguage = new ExComboBox();
            label2 = new Label();
            pShortcuts = new ExOptionsPanel(optionsView);
            dgvShortcuts = new DataGridView();
            cShortcutName = new DataGridViewTextBoxColumn();
            cShortcut = new DataGridViewTextBoxColumn();
            cShortcutAttach = new DataGridViewButtonColumn();
            cShortcutDeattach = new DataGridViewButtonColumn();
            cShortcutReset = new DataGridViewButtonColumn();
            commandShortcutBindingSource = new BindingSource(components);
            panel1 = new Panel();
            bShortcutsReset = new ExButton();
            lShortcutMsg = new Label();
            pStyles = new ExOptionsPanel(optionsView);
            ppStyles = new Panel();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            tscbStyles = new ExToolStripComboBox();
            tsbApplyStyle = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tsbAddStyle = new ToolStripButton();
            tsbRenameStyle = new ToolStripButton();
            tsbDeleteStyle = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsbResetStyle = new ToolStripButton();
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
            gbCategorySettings = new ExGroupBox();
            label8 = new ExLabel();
            cbFont = new ExComboBox();
            nudFontSize = new ExNumericUpDown();
            cboxOnlyNonPropFont = new ExCheckBox();
            exGroupBox8 = new ExGroupBox();
            cboxHighlightStatusBar = new ExCheckBox();
            cboxDarkScrollbars = new ExCheckBox();
            cboxDarkTitlebar = new ExCheckBox();
            cboxDefaultVisual = new ExCheckBox();
            pFonts = new ExOptionsPanel(optionsView);
            pgFonts = new ExPropertyGrid();
            exGroupBox3 = new ExGroupBox();
            lFontExample = new Label();
            pEnvironment = new ExOptionsPanel(optionsView);
            pLogging = new ExOptionsPanel(optionsView);
            ppLogging = new Panel();
            cbShowErrors = new ExComboBox();
            gbLogging = new ExGroupBox();
            cboxLoggingInfo = new ExCheckBox();
            cboxLoggingError = new ExCheckBox();
            label1 = new Label();
            appLanguageBindingSource = new BindingSource(components);
            debugModeBindingSource = new BindingSource(components);
            styleBindingSource = new BindingSource(components);
            desktopMenuModeLabel = new Label();
            panelBottom.SuspendLayout();
            ((ISupportInitialize)optionsView).BeginInit();
            pGeneral.SuspendLayout();
            gboxGeneralProgram.SuspendLayout();
            ((ISupportInitialize)configBindingSource).BeginInit();
            pDesktopComponents.SuspendLayout();
            ppDesktopComponents.SuspendLayout();
            pDesktopColumns.SuspendLayout();
            ((ISupportInitialize)dgvColumns).BeginInit();
            ((ISupportInitialize)desktopColumnBindingSource).BeginInit();
            panel2.SuspendLayout();
            pLocalization.SuspendLayout();
            panel3.SuspendLayout();
            pShortcuts.SuspendLayout();
            ((ISupportInitialize)dgvShortcuts).BeginInit();
            ((ISupportInitialize)commandShortcutBindingSource).BeginInit();
            panel1.SuspendLayout();
            pStyles.SuspendLayout();
            ppStyles.SuspendLayout();
            toolStrip1.SuspendLayout();
            exGroupBox1.SuspendLayout();
            gbColorItemSettings.SuspendLayout();
            gbCategorySettings.SuspendLayout();
            ((ISupportInitialize)nudFontSize).BeginInit();
            exGroupBox8.SuspendLayout();
            pFonts.SuspendLayout();
            pgFonts.SuspendLayout();
            exGroupBox3.SuspendLayout();
            pLogging.SuspendLayout();
            ppLogging.SuspendLayout();
            gbLogging.SuspendLayout();
            ((ISupportInitialize)appLanguageBindingSource).BeginInit();
            ((ISupportInitialize)debugModeBindingSource).BeginInit();
            ((ISupportInitialize)styleBindingSource).BeginInit();
            SuspendLayout();
            // 
            // desktopMenuModeLabel
            // 
            desktopMenuModeLabel.AutoSize = true;
            desktopMenuModeLabel.Location = new Point(15, 16);
            desktopMenuModeLabel.Margin = new Padding(4, 0, 4, 0);
            desktopMenuModeLabel.Name = "desktopMenuModeLabel";
            desktopMenuModeLabel.Size = new Size(70, 15);
            desktopMenuModeLabel.TabIndex = 1;
            desktopMenuModeLabel.Text = resources.GetString("desktopMenuModeLabel.Text");
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
            // listIcons
            // 
            listIcons.ColorDepth = ColorDepth.Depth32Bit;
            listIcons.ImageStream = (ImageListStreamer)resources.GetObject("listIcons.ImageStream");
            listIcons.TransparentColor = Color.Transparent;
            listIcons.Images.SetKeyName(0, "general.png");
            listIcons.Images.SetKeyName(1, "environment.png");
            listIcons.Images.SetKeyName(2, "desktop.png");
            listIcons.Images.SetKeyName(3, "columns.png");
            listIcons.Images.SetKeyName(4, "localization.png");
            listIcons.Images.SetKeyName(5, "shortcut.png");
            listIcons.Images.SetKeyName(6, "colors.png");
            listIcons.Images.SetKeyName(7, "font.png");
            listIcons.Images.SetKeyName(8, "debugging.png");
            listIcons.Images.SetKeyName(9, "start.png");
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
            optionsView.Panels.Add(pGeneral);
            optionsView.Panels.Add(pDesktop);
            optionsView.Panels.Add(pDesktopComponents);
            optionsView.Panels.Add(pDesktopColumns);
            optionsView.Panels.Add(pLocalization);
            optionsView.Panels.Add(pShortcuts);
            optionsView.Panels.Add(pStyles);
            optionsView.Panels.Add(pFonts);
            optionsView.Panels.Add(pEnvironment);
            optionsView.Panels.Add(pLogging);
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
            optionsView.TreeView.ImageIndex = 0;
            optionsView.TreeView.ImageList = listIcons;
            optionsView.TreeView.ItemHeight = 22;
            optionsView.TreeView.Name = "treeView";
            optionsView.TreeView.PathSeparator = " / ";
            optionsView.TreeView.SelectedImageIndex = 0;
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
            optionsNode1.ImageKey = "general.png";
            optionsNode1.Name = "";
            optionsNode1.SelectedImageKey = "general.png";
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
            gboxGeneralProgram.Controls.Add(cbStartup);
            gboxGeneralProgram.Controls.Add(label3);
            gboxGeneralProgram.Controls.Add(cboxClassicGui);
            gboxGeneralProgram.Controls.Add(cboxMultipleInstances);
            gboxGeneralProgram.DisabledForeColor = SystemColors.GrayText;
            gboxGeneralProgram.Margin = new Padding(4, 3, 4, 3);
            gboxGeneralProgram.Name = "gboxGeneralProgram";
            gboxGeneralProgram.Padding = new Padding(4, 3, 4, 3);
            gboxGeneralProgram.TabStop = false;
            // 
            // cbStartup
            // 
            cbStartup.DataBindings.Add(new Binding("SelectedValue", configBindingSource, "Startup", true));
            cbStartup.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbStartup.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStartup.FormattingEnabled = true;
            resources.ApplyResources(cbStartup, "cbStartup");
            cbStartup.Margin = new Padding(4, 3, 4, 3);
            cbStartup.Name = "cbStartup";
            cbStartup.StyleDisabled.ArrowColor = null;
            cbStartup.StyleDisabled.BackColor = null;
            cbStartup.StyleDisabled.BorderColor = null;
            cbStartup.StyleDisabled.ButtonBackColor = null;
            cbStartup.StyleDisabled.ButtonBorderColor = null;
            cbStartup.StyleDisabled.ButtonRenderFirst = null;
            cbStartup.StyleDisabled.ForeColor = null;
            cbStartup.StyleHighlight.ArrowColor = null;
            cbStartup.StyleHighlight.BackColor = null;
            cbStartup.StyleHighlight.BorderColor = null;
            cbStartup.StyleHighlight.ButtonBackColor = null;
            cbStartup.StyleHighlight.ButtonBorderColor = null;
            cbStartup.StyleHighlight.ButtonRenderFirst = null;
            cbStartup.StyleHighlight.ForeColor = null;
            cbStartup.StyleNormal.ArrowColor = null;
            cbStartup.StyleNormal.BackColor = null;
            cbStartup.StyleNormal.BorderColor = null;
            cbStartup.StyleNormal.ButtonBackColor = null;
            cbStartup.StyleNormal.ButtonBorderColor = null;
            cbStartup.StyleNormal.ButtonRenderFirst = null;
            cbStartup.StyleNormal.ForeColor = null;
            cbStartup.StyleSelected.ArrowColor = null;
            cbStartup.StyleSelected.BackColor = null;
            cbStartup.StyleSelected.BorderColor = null;
            cbStartup.StyleSelected.ButtonBackColor = null;
            cbStartup.StyleSelected.ButtonBorderColor = null;
            cbStartup.StyleSelected.ButtonRenderFirst = null;
            cbStartup.StyleSelected.ForeColor = null;
            cbStartup.UseDarkScrollBar = false;
            // 
            // configBindingSource
            // 
            configBindingSource.DataSource = typeof(ConfigBase);
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            // 
            // cboxClassicGui
            // 
            resources.ApplyResources(cboxClassicGui, "cboxClassicGui");
            cboxClassicGui.BoxBackColor = Color.White;
            cboxClassicGui.DataBindings.Add(new Binding("Checked", configBindingSource, "ClassicGUI", true));
            cboxClassicGui.HighlightColor = SystemColors.Highlight;
            cboxClassicGui.Margin = new Padding(4, 3, 4, 3);
            cboxClassicGui.Name = "cboxClassicGui";
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
            // pDesktop
            // 
            pDesktop.GenerateLinksToChildren = true;
            pDesktop.Margin = new Padding(4, 3, 4, 3);
            pDesktop.Name = "pDesktop";
            optionsNode2.ImageKey = "desktop.png";
            optionsNode2.Name = "";
            optionsNode2.SelectedImageKey = "desktop.png";
            optionsNode2.Text = resources.GetString("optionsNode2.Text");
            pDesktop.Node = optionsNode2;
            pDesktop.NodeText = resources.GetString("pDesktop.NodeText");
            optionsNode3.ImageKey = "environment.png";
            optionsNode3.Name = "";
            optionsNode3.SelectedImageKey = "environment.png";
            optionsNode3.Text = resources.GetString("optionsNode3.Text");
            pDesktop.ParentNode = optionsNode3;
            // 
            // pDesktopComponents
            // 
            pDesktopComponents.Controls.Add(pConcreteDesktopComponents);
            pDesktopComponents.Controls.Add(ppDesktopComponents);
            pDesktopComponents.Margin = new Padding(4, 3, 4, 3);
            pDesktopComponents.Name = "pDesktopComponents";
            optionsNode4.ImageKey = "desktop.png";
            optionsNode4.Name = "";
            optionsNode4.SelectedImageKey = "desktop.png";
            optionsNode4.Text = resources.GetString("optionsNode4.Text");
            pDesktopComponents.Node = optionsNode4;
            pDesktopComponents.NodeText = resources.GetString("pDesktopComponents.NodeText");
            pDesktopComponents.ParentNode = optionsNode2;
            // 
            // pConcreteDesktopComponents
            // 
            resources.ApplyResources(pConcreteDesktopComponents, "pConcreteDesktopComponents");
            pConcreteDesktopComponents.Margin = new Padding(4, 3, 4, 3);
            pConcreteDesktopComponents.Name = "pConcreteDesktopComponents";
            // 
            // ppDesktopComponents
            // 
            resources.ApplyResources(ppDesktopComponents, "ppDesktopComponents");
            ppDesktopComponents.Controls.Add(desktopMenuModeLabel);
            ppDesktopComponents.Controls.Add(rbMenuToolStrip);
            ppDesktopComponents.Controls.Add(cboxShowRowHeaders);
            ppDesktopComponents.Controls.Add(rbMenuOnly);
            ppDesktopComponents.Controls.Add(rbToolOnly);
            ppDesktopComponents.Margin = new Padding(4, 3, 4, 3);
            ppDesktopComponents.Name = "ppDesktopComponents";
            // 
            // rbMenuToolStrip
            // 
            resources.ApplyResources(rbMenuToolStrip, "rbMenuToolStrip");
            rbMenuToolStrip.HighlightColor = SystemColors.Highlight;
            rbMenuToolStrip.Margin = new Padding(4, 3, 4, 3);
            rbMenuToolStrip.Name = "rbMenuToolStrip";
            rbMenuToolStrip.TabStop = true;
            rbMenuToolStrip.UseVisualStyleBackColor = true;
            // 
            // cboxShowRowHeaders
            // 
            resources.ApplyResources(cboxShowRowHeaders, "cboxShowRowHeaders");
            cboxShowRowHeaders.BoxBackColor = Color.White;
            cboxShowRowHeaders.DataBindings.Add(new Binding("Checked", configBindingSource, "ShowRowsHeader", true));
            cboxShowRowHeaders.HighlightColor = SystemColors.Highlight;
            cboxShowRowHeaders.Margin = new Padding(4, 3, 4, 3);
            cboxShowRowHeaders.Name = "cboxShowRowHeaders";
            // 
            // rbMenuOnly
            // 
            resources.ApplyResources(rbMenuOnly, "rbMenuOnly");
            rbMenuOnly.HighlightColor = SystemColors.Highlight;
            rbMenuOnly.Margin = new Padding(4, 3, 4, 3);
            rbMenuOnly.Name = "rbMenuOnly";
            rbMenuOnly.TabStop = true;
            rbMenuOnly.UseVisualStyleBackColor = true;
            // 
            // rbToolOnly
            // 
            resources.ApplyResources(rbToolOnly, "rbToolOnly");
            rbToolOnly.HighlightColor = SystemColors.Highlight;
            rbToolOnly.Margin = new Padding(4, 3, 4, 3);
            rbToolOnly.Name = "rbToolOnly";
            rbToolOnly.TabStop = true;
            rbToolOnly.UseVisualStyleBackColor = true;
            // 
            // pDesktopColumns
            // 
            pDesktopColumns.Controls.Add(dgvColumns);
            pDesktopColumns.Controls.Add(panel2);
            pDesktopColumns.Margin = new Padding(4, 3, 4, 3);
            pDesktopColumns.Name = "pDesktopColumns";
            optionsNode5.ImageKey = "columns.png";
            optionsNode5.Name = "";
            optionsNode5.SelectedImageKey = "columns.png";
            optionsNode5.Text = resources.GetString("optionsNode5.Text");
            pDesktopColumns.Node = optionsNode5;
            pDesktopColumns.NodeText = resources.GetString("pDesktopColumns.NodeText");
            pDesktopColumns.ParentNode = optionsNode2;
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
            panel2.Controls.Add(cboxFitLastCol);
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
            // cboxFitLastCol
            // 
            resources.ApplyResources(cboxFitLastCol, "cboxFitLastCol");
            cboxFitLastCol.BoxBackColor = Color.White;
            cboxFitLastCol.DataBindings.Add(new Binding("Checked", configBindingSource, "FitLastColumn", true));
            cboxFitLastCol.HighlightColor = SystemColors.Highlight;
            cboxFitLastCol.Margin = new Padding(4, 3, 4, 3);
            cboxFitLastCol.Name = "cboxFitLastCol";
            // 
            // pLocalization
            // 
            pLocalization.Controls.Add(pConcreteLocalization);
            pLocalization.Controls.Add(panel3);
            pLocalization.Margin = new Padding(4, 3, 4, 3);
            pLocalization.Name = "pLocalization";
            optionsNode6.ImageKey = "localization.png";
            optionsNode6.Name = "";
            optionsNode6.SelectedImageKey = "localization.png";
            optionsNode6.Text = resources.GetString("optionsNode6.Text");
            pLocalization.Node = optionsNode6;
            pLocalization.NodeText = resources.GetString("pLocalization.NodeText");
            pLocalization.ParentNode = optionsNode3;
            // 
            // pConcreteLocalization
            // 
            resources.ApplyResources(pConcreteLocalization, "pConcreteLocalization");
            pConcreteLocalization.Margin = new Padding(4, 3, 4, 3);
            pConcreteLocalization.Name = "pConcreteLocalization";
            // 
            // panel3
            // 
            resources.ApplyResources(panel3, "panel3");
            panel3.Controls.Add(cbAppLanguage);
            panel3.Controls.Add(label2);
            panel3.Margin = new Padding(4, 3, 4, 3);
            panel3.Name = "panel3";
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
            pShortcuts.Controls.Add(dgvShortcuts);
            pShortcuts.Controls.Add(panel1);
            pShortcuts.Margin = new Padding(4, 3, 4, 3);
            pShortcuts.Name = "pShortcuts";
            optionsNode7.ImageKey = "shortcut.png";
            optionsNode7.Name = "";
            optionsNode7.SelectedImageKey = "shortcut.png";
            optionsNode7.Text = resources.GetString("optionsNode7.Text");
            pShortcuts.Node = optionsNode7;
            pShortcuts.NodeText = resources.GetString("pShortcuts.NodeText");
            pShortcuts.ParentNode = optionsNode3;
            // 
            // dgvShortcuts
            // 
            dgvShortcuts.AllowUserToAddRows = false;
            dgvShortcuts.AllowUserToDeleteRows = false;
            dgvShortcuts.AllowUserToResizeRows = false;
            dgvShortcuts.AutoGenerateColumns = false;
            dgvShortcuts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvShortcuts.Columns.AddRange(new DataGridViewColumn[] { cShortcutName, cShortcut, cShortcutAttach, cShortcutDeattach, cShortcutReset });
            dgvShortcuts.DataSource = commandShortcutBindingSource;
            resources.ApplyResources(dgvShortcuts, "dgvShortcuts");
            dgvShortcuts.Margin = new Padding(4, 3, 4, 3);
            dgvShortcuts.Name = "dgvShortcuts";
            dgvShortcuts.ReadOnly = true;
            dgvShortcuts.RowHeadersVisible = false;
            dgvShortcuts.RowHeadersWidth = 51;
            dgvShortcuts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvShortcuts.CellContentClick += DgvShortcuts_CellContentClick;
            // 
            // cShortcutName
            // 
            cShortcutName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            cShortcutName.DataPropertyName = "Name";
            cShortcutName.Frozen = true;
            resources.ApplyResources(cShortcutName, "cShortcutName");
            cShortcutName.MinimumWidth = 6;
            cShortcutName.Name = "cShortcutName";
            cShortcutName.ReadOnly = true;
            // 
            // cShortcut
            // 
            cShortcut.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            cShortcut.DataPropertyName = "Shortcut";
            cShortcut.Frozen = true;
            resources.ApplyResources(cShortcut, "cShortcut");
            cShortcut.MinimumWidth = 6;
            cShortcut.Name = "cShortcut";
            cShortcut.ReadOnly = true;
            // 
            // cShortcutAttach
            // 
            resources.ApplyResources(cShortcutAttach, "cShortcutAttach");
            cShortcutAttach.MinimumWidth = 6;
            cShortcutAttach.Name = "cShortcutAttach";
            cShortcutAttach.ReadOnly = true;
            cShortcutAttach.UseColumnTextForButtonValue = true;
            // 
            // cShortcutDeattach
            // 
            resources.ApplyResources(cShortcutDeattach, "cShortcutDeattach");
            cShortcutDeattach.MinimumWidth = 6;
            cShortcutDeattach.Name = "cShortcutDeattach";
            cShortcutDeattach.ReadOnly = true;
            cShortcutDeattach.UseColumnTextForButtonValue = true;
            // 
            // cShortcutReset
            // 
            cShortcutReset.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(cShortcutReset, "cShortcutReset");
            cShortcutReset.MinimumWidth = 6;
            cShortcutReset.Name = "cShortcutReset";
            cShortcutReset.ReadOnly = true;
            cShortcutReset.UseColumnTextForButtonValue = true;
            // 
            // commandShortcutBindingSource
            // 
            commandShortcutBindingSource.DataSource = typeof(CmdShortcut);
            // 
            // panel1
            // 
            panel1.Controls.Add(bShortcutsReset);
            panel1.Controls.Add(lShortcutMsg);
            resources.ApplyResources(panel1, "panel1");
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            // 
            // bShortcutsReset
            // 
            resources.ApplyResources(bShortcutsReset, "bShortcutsReset");
            bShortcutsReset.Margin = new Padding(4, 3, 4, 3);
            bShortcutsReset.Name = "bShortcutsReset";
            bShortcutsReset.UseVisualStyleBackColor = true;
            bShortcutsReset.Click += BShortcutsReset_Click;
            // 
            // lShortcutMsg
            // 
            resources.ApplyResources(lShortcutMsg, "lShortcutMsg");
            lShortcutMsg.Margin = new Padding(4, 0, 4, 0);
            lShortcutMsg.Name = "lShortcutMsg";
            // 
            // pStyles
            // 
            pStyles.Controls.Add(ppStyles);
            pStyles.Margin = new Padding(4, 3, 4, 3);
            pStyles.Name = "pStyles";
            optionsNode8.ImageKey = "colors.png";
            optionsNode8.Name = "";
            optionsNode8.SelectedImageKey = "colors.png";
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
            ppStyles.Controls.Add(gbCategorySettings);
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
            tsbApplyStyle.Image = GlobalResources.correct;
            tsbApplyStyle.ImageTransparentColor = Color.Magenta;
            tsbApplyStyle.Name = "tsbApplyStyle";
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
            tsbAddStyle.Image = GlobalResources.add;
            tsbAddStyle.ImageTransparentColor = Color.Magenta;
            tsbAddStyle.Name = "tsbAddStyle";
            resources.ApplyResources(tsbAddStyle, "tsbAddStyle");
            tsbAddStyle.Click += TsbAddStyle_Click;
            // 
            // tsbRenameStyle
            // 
            tsbRenameStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbRenameStyle.Image = GlobalResources.rename;
            tsbRenameStyle.ImageTransparentColor = Color.Magenta;
            tsbRenameStyle.Name = "tsbRenameStyle";
            resources.ApplyResources(tsbRenameStyle, "tsbRenameStyle");
            tsbRenameStyle.Click += TsbRenameStyle_Click;
            // 
            // tsbDeleteStyle
            // 
            tsbDeleteStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbDeleteStyle.Image = GlobalResources.delete;
            tsbDeleteStyle.ImageTransparentColor = Color.Magenta;
            tsbDeleteStyle.Name = "tsbDeleteStyle";
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
            tsbResetStyle.Image = GlobalResources.restart;
            tsbResetStyle.ImageTransparentColor = Color.Magenta;
            tsbResetStyle.Name = "tsbResetStyle";
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
            // gbCategorySettings
            // 
            resources.ApplyResources(gbCategorySettings, "gbCategorySettings");
            gbCategorySettings.Controls.Add(label8);
            gbCategorySettings.Controls.Add(cbFont);
            gbCategorySettings.Controls.Add(nudFontSize);
            gbCategorySettings.Controls.Add(cboxOnlyNonPropFont);
            gbCategorySettings.DisabledForeColor = SystemColors.GrayText;
            gbCategorySettings.Margin = new Padding(2);
            gbCategorySettings.Name = "gbCategorySettings";
            gbCategorySettings.Padding = new Padding(2);
            gbCategorySettings.TabStop = false;
            // 
            // label8
            // 
            resources.ApplyResources(label8, "label8");
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            // 
            // cbFont
            // 
            resources.ApplyResources(cbFont, "cbFont");
            cbFont.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbFont.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFont.FormattingEnabled = true;
            cbFont.Margin = new Padding(2);
            cbFont.Name = "cbFont";
            cbFont.StyleDisabled.ArrowColor = null;
            cbFont.StyleDisabled.BackColor = null;
            cbFont.StyleDisabled.BorderColor = null;
            cbFont.StyleDisabled.ButtonBackColor = null;
            cbFont.StyleDisabled.ButtonBorderColor = null;
            cbFont.StyleDisabled.ButtonRenderFirst = null;
            cbFont.StyleDisabled.ForeColor = null;
            cbFont.StyleHighlight.ArrowColor = null;
            cbFont.StyleHighlight.BackColor = null;
            cbFont.StyleHighlight.BorderColor = null;
            cbFont.StyleHighlight.ButtonBackColor = null;
            cbFont.StyleHighlight.ButtonBorderColor = null;
            cbFont.StyleHighlight.ButtonRenderFirst = null;
            cbFont.StyleHighlight.ForeColor = null;
            cbFont.StyleNormal.ArrowColor = null;
            cbFont.StyleNormal.BackColor = null;
            cbFont.StyleNormal.BorderColor = null;
            cbFont.StyleNormal.ButtonBackColor = null;
            cbFont.StyleNormal.ButtonBorderColor = null;
            cbFont.StyleNormal.ButtonRenderFirst = null;
            cbFont.StyleNormal.ForeColor = null;
            cbFont.StyleSelected.ArrowColor = null;
            cbFont.StyleSelected.BackColor = null;
            cbFont.StyleSelected.BorderColor = null;
            cbFont.StyleSelected.ButtonBackColor = null;
            cbFont.StyleSelected.ButtonBorderColor = null;
            cbFont.StyleSelected.ButtonRenderFirst = null;
            cbFont.StyleSelected.ForeColor = null;
            cbFont.UseDarkScrollBar = false;
            cbFont.SelectedIndexChanged += CbFont_SelectedIndexChanged;
            // 
            // nudFontSize
            // 
            resources.ApplyResources(nudFontSize, "nudFontSize");
            nudFontSize.HighlightColor = SystemColors.Highlight;
            nudFontSize.Margin = new Padding(2);
            nudFontSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudFontSize.Name = "nudFontSize";
            nudFontSize.SelectedButtonColor = SystemColors.Highlight;
            nudFontSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudFontSize.ValueChanged += NudFontSize_ValueChanged;
            // 
            // cboxOnlyNonPropFont
            // 
            resources.ApplyResources(cboxOnlyNonPropFont, "cboxOnlyNonPropFont");
            cboxOnlyNonPropFont.BoxBackColor = Color.White;
            cboxOnlyNonPropFont.HighlightColor = SystemColors.Highlight;
            cboxOnlyNonPropFont.Margin = new Padding(2);
            cboxOnlyNonPropFont.Name = "cboxOnlyNonPropFont";
            cboxOnlyNonPropFont.CheckedChanged += CboxOnlyNonPropFont_CheckedChanged;
            // 
            // exGroupBox8
            // 
            resources.ApplyResources(exGroupBox8, "exGroupBox8");
            exGroupBox8.Controls.Add(cboxHighlightStatusBar);
            exGroupBox8.Controls.Add(cboxDarkScrollbars);
            exGroupBox8.Controls.Add(cboxDarkTitlebar);
            exGroupBox8.Controls.Add(cboxDefaultVisual);
            exGroupBox8.DisabledForeColor = SystemColors.GrayText;
            exGroupBox8.Margin = new Padding(2);
            exGroupBox8.Name = "exGroupBox8";
            exGroupBox8.Padding = new Padding(2);
            exGroupBox8.TabStop = false;
            // 
            // cboxHighlightStatusBar
            // 
            resources.ApplyResources(cboxHighlightStatusBar, "cboxHighlightStatusBar");
            cboxHighlightStatusBar.BoxBackColor = Color.White;
            cboxHighlightStatusBar.HighlightColor = SystemColors.Highlight;
            cboxHighlightStatusBar.Margin = new Padding(4, 3, 4, 3);
            cboxHighlightStatusBar.Name = "cboxHighlightStatusBar";
            cboxHighlightStatusBar.CheckedChanged += CboxHighlightStatusBar_CheckedChanged;
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
            pFonts.Controls.Add(pgFonts);
            pFonts.Controls.Add(exGroupBox3);
            pFonts.Margin = new Padding(4, 3, 4, 3);
            pFonts.Name = "pFonts";
            optionsNode9.ImageKey = "font.png";
            optionsNode9.Name = "";
            optionsNode9.SelectedImageKey = "font.png";
            optionsNode9.Text = resources.GetString("optionsNode9.Text");
            pFonts.Node = optionsNode9;
            pFonts.NodeText = resources.GetString("pFonts.NodeText");
            pFonts.ParentNode = optionsNode3;
            // 
            // pgFonts
            // 
            pgFonts.BackColor = SystemColors.Control;
            pgFonts.BrowsableProperties = null;
            // 
            // 
            // 
            pgFonts.ButtonAlphabetical.ImageIndex = 0;
            pgFonts.ButtonAlphabetical.Name = "";
            pgFonts.ButtonAlphabetical.Size = new Size(23, 22);
            // 
            // 
            // 
            pgFonts.ButtonCategorized.ImageIndex = 1;
            pgFonts.ButtonCategorized.Name = "";
            pgFonts.ButtonCategorized.Size = new Size(23, 22);
            // 
            // 
            // 
            pgFonts.ButtonPropertyPages.Enabled = false;
            pgFonts.ButtonPropertyPages.ImageIndex = 2;
            pgFonts.ButtonPropertyPages.Name = "";
            pgFonts.ButtonPropertyPages.Size = new Size(23, 22);
            pgFonts.ButtonPropertyPages.Visible = false;
            resources.ApplyResources(pgFonts, "pgFonts");
            pgFonts.FirstHideAllProperties = false;
            pgFonts.HelpVisible = false;
            pgFonts.HiddenAttributes = null;
            pgFonts.HiddenProperties = null;
            // 
            // pgFonts.InnerToolStrip
            // 
            pgFonts.InnerToolStrip.AccessibleName = "Property Grid";
            pgFonts.InnerToolStrip.AccessibleRole = AccessibleRole.ToolBar;
            pgFonts.InnerToolStrip.AllowMerge = false;
            pgFonts.InnerToolStrip.AutoSize = false;
            pgFonts.InnerToolStrip.BackColor = SystemColors.Control;
            pgFonts.InnerToolStrip.CanOverflow = false;
            pgFonts.InnerToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            pgFonts.InnerToolStrip.Items.AddRange(new ToolStripItem[] { pgFonts.ButtonCategorized, pgFonts.ButtonAlphabetical, pgFonts.Separator, pgFonts.ButtonPropertyPages });
            pgFonts.InnerToolStrip.Location = new Point(0, 0);
            pgFonts.InnerToolStrip.Name = "InnerToolStrip";
            pgFonts.InnerToolStrip.Padding = new Padding(2, 0, 1, 0);
            pgFonts.InnerToolStrip.Size = new Size(630, 25);
            pgFonts.InnerToolStrip.TabIndex = 1;
            pgFonts.InnerToolStrip.TabStop = true;
            pgFonts.InnerToolStrip.Text = "PropertyGridToolBar";
            pgFonts.Location = new Point(0, 0);
            pgFonts.Margin = new Padding(4, 3, 4, 3);
            pgFonts.Name = "pgFonts";
            pgFonts.PropertySort = PropertySort.NoSort;
            pgFonts.Size = new Size(630, 397);
            pgFonts.TabIndex = 0;
            pgFonts.SelectedGridItemChanged += PgFonts_SelectedGridItemChanged;
            // 
            // exGroupBox3
            // 
            exGroupBox3.Controls.Add(lFontExample);
            exGroupBox3.DisabledForeColor = SystemColors.GrayText;
            resources.ApplyResources(exGroupBox3, "exGroupBox3");
            exGroupBox3.Margin = new Padding(4, 3, 4, 3);
            exGroupBox3.Name = "exGroupBox3";
            exGroupBox3.Padding = new Padding(4, 3, 4, 3);
            exGroupBox3.TabStop = false;
            // 
            // lFontExample
            // 
            resources.ApplyResources(lFontExample, "lFontExample");
            lFontExample.Margin = new Padding(4, 0, 4, 0);
            lFontExample.Name = "lFontExample";
            lFontExample.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pEnvironment
            // 
            pEnvironment.GenerateLinksToChildren = true;
            pEnvironment.Margin = new Padding(4, 3, 4, 3);
            pEnvironment.Name = "pEnvironment";
            pEnvironment.Node = optionsNode3;
            pEnvironment.NodeText = resources.GetString("pEnvironment.NodeText");
            pEnvironment.ParentNode = null;
            // 
            // pLogging
            // 
            pLogging.Controls.Add(ppLogging);
            pLogging.Margin = new Padding(4, 3, 4, 3);
            pLogging.Name = "pLogging";
            optionsNode10.ImageKey = "debugging.png";
            optionsNode10.Name = "";
            optionsNode10.SelectedImageKey = "debugging.png";
            optionsNode10.Text = resources.GetString("optionsNode10.Text");
            pLogging.Node = optionsNode10;
            pLogging.NodeText = resources.GetString("pLogging.NodeText");
            pLogging.ParentNode = null;
            // 
            // ppLogging
            // 
            ppLogging.Controls.Add(cbShowErrors);
            ppLogging.Controls.Add(gbLogging);
            ppLogging.Controls.Add(label1);
            resources.ApplyResources(ppLogging, "ppLogging");
            ppLogging.Margin = new Padding(4, 3, 4, 3);
            ppLogging.Name = "ppLogging";
            // 
            // cbShowErrors
            // 
            cbShowErrors.DataBindings.Add(new Binding("SelectedValue", configBindingSource, "DebugModeGUI", true));
            cbShowErrors.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbShowErrors.DropDownStyle = ComboBoxStyle.DropDownList;
            cbShowErrors.FormattingEnabled = true;
            resources.ApplyResources(cbShowErrors, "cbShowErrors");
            cbShowErrors.Margin = new Padding(4, 3, 4, 3);
            cbShowErrors.Name = "cbShowErrors";
            cbShowErrors.StyleDisabled.ArrowColor = null;
            cbShowErrors.StyleDisabled.BackColor = null;
            cbShowErrors.StyleDisabled.BorderColor = null;
            cbShowErrors.StyleDisabled.ButtonBackColor = null;
            cbShowErrors.StyleDisabled.ButtonBorderColor = null;
            cbShowErrors.StyleDisabled.ButtonRenderFirst = null;
            cbShowErrors.StyleDisabled.ForeColor = null;
            cbShowErrors.StyleHighlight.ArrowColor = null;
            cbShowErrors.StyleHighlight.BackColor = null;
            cbShowErrors.StyleHighlight.BorderColor = null;
            cbShowErrors.StyleHighlight.ButtonBackColor = null;
            cbShowErrors.StyleHighlight.ButtonBorderColor = null;
            cbShowErrors.StyleHighlight.ButtonRenderFirst = null;
            cbShowErrors.StyleHighlight.ForeColor = null;
            cbShowErrors.StyleNormal.ArrowColor = null;
            cbShowErrors.StyleNormal.BackColor = null;
            cbShowErrors.StyleNormal.BorderColor = null;
            cbShowErrors.StyleNormal.ButtonBackColor = null;
            cbShowErrors.StyleNormal.ButtonBorderColor = null;
            cbShowErrors.StyleNormal.ButtonRenderFirst = null;
            cbShowErrors.StyleNormal.ForeColor = null;
            cbShowErrors.StyleSelected.ArrowColor = null;
            cbShowErrors.StyleSelected.BackColor = null;
            cbShowErrors.StyleSelected.BorderColor = null;
            cbShowErrors.StyleSelected.ButtonBackColor = null;
            cbShowErrors.StyleSelected.ButtonBorderColor = null;
            cbShowErrors.StyleSelected.ButtonRenderFirst = null;
            cbShowErrors.StyleSelected.ForeColor = null;
            cbShowErrors.UseDarkScrollBar = false;
            // 
            // gbLogging
            // 
            resources.ApplyResources(gbLogging, "gbLogging");
            gbLogging.Controls.Add(cboxLoggingInfo);
            gbLogging.Controls.Add(cboxLoggingError);
            gbLogging.DisabledForeColor = SystemColors.GrayText;
            gbLogging.Margin = new Padding(4, 3, 4, 3);
            gbLogging.Name = "gbLogging";
            gbLogging.Padding = new Padding(4, 3, 4, 3);
            gbLogging.TabStop = false;
            // 
            // cboxLoggingInfo
            // 
            resources.ApplyResources(cboxLoggingInfo, "cboxLoggingInfo");
            cboxLoggingInfo.BoxBackColor = Color.White;
            cboxLoggingInfo.DataBindings.Add(new Binding("CheckState", configBindingSource, "LoggingInfo", true));
            cboxLoggingInfo.HighlightColor = SystemColors.Highlight;
            cboxLoggingInfo.Margin = new Padding(4, 3, 4, 3);
            cboxLoggingInfo.Name = "cboxLoggingInfo";
            // 
            // cboxLoggingError
            // 
            resources.ApplyResources(cboxLoggingError, "cboxLoggingError");
            cboxLoggingError.BoxBackColor = Color.White;
            cboxLoggingError.DataBindings.Add(new Binding("CheckState", configBindingSource, "LoggingError", true));
            cboxLoggingError.HighlightColor = SystemColors.Highlight;
            cboxLoggingError.Margin = new Padding(4, 3, 4, 3);
            cboxLoggingError.Name = "cboxLoggingError";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            // 
            // appLanguageBindingSource
            // 
            appLanguageBindingSource.DataSource = typeof(AppLanguage);
            // 
            // debugModeBindingSource
            // 
            debugModeBindingSource.DataSource = typeof(DebugMode);
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
            KeyDown += FAppSettingsBase_KeyDown;
            panelBottom.ResumeLayout(false);
            ((ISupportInitialize)optionsView).EndInit();
            pGeneral.ResumeLayout(false);
            pGeneral.PerformLayout();
            gboxGeneralProgram.ResumeLayout(false);
            gboxGeneralProgram.PerformLayout();
            ((ISupportInitialize)configBindingSource).EndInit();
            pDesktopComponents.ResumeLayout(false);
            pDesktopComponents.PerformLayout();
            ppDesktopComponents.ResumeLayout(false);
            ppDesktopComponents.PerformLayout();
            pDesktopColumns.ResumeLayout(false);
            ((ISupportInitialize)dgvColumns).EndInit();
            ((ISupportInitialize)desktopColumnBindingSource).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            pLocalization.ResumeLayout(false);
            pLocalization.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            pShortcuts.ResumeLayout(false);
            ((ISupportInitialize)dgvShortcuts).EndInit();
            ((ISupportInitialize)commandShortcutBindingSource).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            pStyles.ResumeLayout(false);
            ppStyles.ResumeLayout(false);
            ppStyles.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            exGroupBox1.ResumeLayout(false);
            gbColorItemSettings.ResumeLayout(false);
            gbColorItemSettings.PerformLayout();
            gbCategorySettings.ResumeLayout(false);
            gbCategorySettings.PerformLayout();
            ((ISupportInitialize)nudFontSize).EndInit();
            exGroupBox8.ResumeLayout(false);
            exGroupBox8.PerformLayout();
            pFonts.ResumeLayout(false);
            pgFonts.ResumeLayout(false);
            exGroupBox3.ResumeLayout(false);
            pLogging.ResumeLayout(false);
            ppLogging.ResumeLayout(false);
            ppLogging.PerformLayout();
            gbLogging.ResumeLayout(false);
            gbLogging.PerformLayout();
            ((ISupportInitialize)appLanguageBindingSource).EndInit();
            ((ISupportInitialize)debugModeBindingSource).EndInit();
            ((ISupportInitialize)styleBindingSource).EndInit();
            ResumeLayout(false);

        }

        #endregion
        private ExControls.ExButton bSave;
        private ExControls.ExButton bStorno;
        private Panel panelBottom;
        private ImageList listIcons;
        protected ExControls.ExOptionsPanel pGeneral;
        private Panel panel1;
        private ExControls.ExButton bShortcutsReset;
        private Label lShortcutMsg;
        private Panel ppLogging;
        private ExControls.ExComboBox cbShowErrors;
        private ExControls.ExGroupBox gbLogging;
        private ExControls.ExCheckBox cboxLoggingInfo;
        private ExControls.ExCheckBox cboxLoggingError;
        private Label label1;
        protected DataGridView dgvColumns;
        private Label label2;
        private ExControls.ExGroupBox gboxGeneralProgram;
        private ExControls.ExCheckBox cboxClassicGui;
        private ExControls.ExCheckBox cboxMultipleInstances;
        private BindingSource appLanguageBindingSource;
        private BindingSource debugModeBindingSource;
        private BindingSource commandShortcutBindingSource;
        private BindingSource desktopColumnBindingSource;
        private ExControls.ExGroupBox exGroupBox3;
        private ExControls.ExPropertyGrid pgFonts;
        private Label lFontExample;
        private ExCheckBox cboxShowRowHeaders;
        private ExRadioButton rbToolOnly;
        private ExRadioButton rbMenuOnly;
        private ExRadioButton rbMenuToolStrip;
        private Panel panel2;
        private ExButton bColDown;
        private ExButton bColUp;
        private ExCheckBox cboxFitLastCol;
        private ExComboBox cbAppLanguage;
        protected DataGridView dgvShortcuts;
        protected Panel pConcreteGeneral;
        protected ExOptionsView optionsView;
        private Panel ppStyles;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabel1;
        private ExToolStripComboBox tscbStyles;
        private ToolStripButton tsbApplyStyle;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton tsbAddStyle;
        private ToolStripButton tsbRenameStyle;
        private ToolStripButton tsbDeleteStyle;
        private ExGroupBox exGroupBox1;
        private Label lFontStyleExample;
        private ExTreeView tvStyles;
        private ExGroupBox gbColorItemSettings;
        private ExCheckBox cboxBold;
        private ExColorSelector csForeColor;
        private ExLabel lStyleBackColor;
        private ExLabel lStyleForeColor;
        private ExColorSelector csBackColor;
        private ExGroupBox gbCategorySettings;
        private ExLabel label8;
        private ExComboBox cbFont;
        private ExNumericUpDown nudFontSize;
        private ExCheckBox cboxOnlyNonPropFont;
        private ExGroupBox exGroupBox8;
        private ExCheckBox cboxDarkScrollbars;
        private ExCheckBox cboxDarkTitlebar;
        private ExCheckBox cboxDefaultVisual;
        private BindingSource styleBindingSource;
        private ExButton bResetColumns;
        private ExComboBox cbStartup;
        private Label label3;
        protected ExOptionsPanel pShortcuts;
        protected ExOptionsPanel pStyles;
        protected ExOptionsPanel pFonts;
        protected ExOptionsPanel pLogging;
        protected ExOptionsPanel pDesktopComponents;
        protected ExOptionsPanel pDesktopColumns;
        protected ExOptionsPanel pLocalization;
        private ExOptionsPanel pEnvironment;
        private ExOptionsPanel pDesktop;
        protected Panel panel3;
        protected Panel pConcreteLocalization;
        private Panel ppDesktopComponents;
        protected Panel pConcreteDesktopComponents;
        private BindingSource configBindingSource;
        private DataGridViewTextBoxColumn cColName;
        private DataGridViewExCheckBoxColumn cColVisible;
        private DataGridViewTextBoxColumn cColMinWidth;
        private DataGridViewTextBoxColumn cShortcutName;
        private DataGridViewTextBoxColumn cShortcut;
        private DataGridViewButtonColumn cShortcutAttach;
        private DataGridViewButtonColumn cShortcutDeattach;
        private DataGridViewButtonColumn cShortcutReset;
        private ExButton bResetColorSetting;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton tsbResetStyle;
        private ExCheckBox cboxHighlightStatusBar;
    }
}