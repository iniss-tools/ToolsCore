using System.ComponentModel;
using ExControls;

namespace ToolsCore.Forms
{
    partial class FontsPage
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FontsPage));
            gbInterface = new ExGroupBox();
            lInterfaceHint = new Label();
            cboxInterfaceCustom = new ExCheckBox();
            cbInterfaceFamily = new ExComboBox();
            lInterfaceSize = new Label();
            nudInterfaceSize = new ExNumericUpDown();
            lInterfacePreview = new Label();
            gbTables = new ExGroupBox();
            lTablesHint = new Label();
            cboxTablesCustom = new ExCheckBox();
            cbTablesFamily = new ExComboBox();
            lTablesSize = new Label();
            nudTablesSize = new ExNumericUpDown();
            lTablesPreview = new Label();
            gbEditor = new ExGroupBox();
            lEditorHint = new Label();
            cboxEditorCustom = new ExCheckBox();
            cbEditorFamily = new ExComboBox();
            lEditorSize = new Label();
            nudEditorSize = new ExNumericUpDown();
            cboxMonospaced = new ExCheckBox();
            lEditorPreview = new Label();
            gbInterface.SuspendLayout();
            ((ISupportInitialize)nudInterfaceSize).BeginInit();
            gbTables.SuspendLayout();
            ((ISupportInitialize)nudTablesSize).BeginInit();
            gbEditor.SuspendLayout();
            ((ISupportInitialize)nudEditorSize).BeginInit();
            SuspendLayout();
            //
            // gbInterface
            //
            gbInterface.Controls.Add(lInterfaceHint);
            gbInterface.Controls.Add(cboxInterfaceCustom);
            gbInterface.Controls.Add(cbInterfaceFamily);
            gbInterface.Controls.Add(lInterfaceSize);
            gbInterface.Controls.Add(nudInterfaceSize);
            gbInterface.Controls.Add(lInterfacePreview);
            gbInterface.DisabledForeColor = SystemColors.GrayText;
            resources.ApplyResources(gbInterface, "gbInterface");
            gbInterface.Name = "gbInterface";
            gbInterface.TabStop = false;
            //
            // lInterfaceHint
            //
            resources.ApplyResources(lInterfaceHint, "lInterfaceHint");
            lInterfaceHint.Name = "lInterfaceHint";
            //
            // cboxInterfaceCustom
            //
            resources.ApplyResources(cboxInterfaceCustom, "cboxInterfaceCustom");
            cboxInterfaceCustom.BoxBackColor = Color.White;
            cboxInterfaceCustom.HighlightColor = SystemColors.Highlight;
            cboxInterfaceCustom.Name = "cboxInterfaceCustom";
            cboxInterfaceCustom.CheckedChanged += CboxCustom_CheckedChanged;
            //
            // cbInterfaceFamily
            //
            cbInterfaceFamily.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbInterfaceFamily.DropDownStyle = ComboBoxStyle.DropDownList;
            cbInterfaceFamily.FormattingEnabled = true;
            resources.ApplyResources(cbInterfaceFamily, "cbInterfaceFamily");
            cbInterfaceFamily.Name = "cbInterfaceFamily";
            cbInterfaceFamily.UseDarkScrollBar = false;
            cbInterfaceFamily.SelectedIndexChanged += FontControl_Changed;
            //
            // lInterfaceSize
            //
            resources.ApplyResources(lInterfaceSize, "lInterfaceSize");
            lInterfaceSize.Name = "lInterfaceSize";
            //
            // nudInterfaceSize
            //
            nudInterfaceSize.DecimalPlaces = 0;
            resources.ApplyResources(nudInterfaceSize, "nudInterfaceSize");
            nudInterfaceSize.HighlightColor = SystemColors.Highlight;
            nudInterfaceSize.Maximum = new decimal(new int[] { 48, 0, 0, 0 });
            nudInterfaceSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            nudInterfaceSize.Name = "nudInterfaceSize";
            nudInterfaceSize.SelectedButtonColor = SystemColors.Highlight;
            nudInterfaceSize.Value = new decimal(new int[] { 9, 0, 0, 0 });
            nudInterfaceSize.ValueChanged += FontControl_Changed;
            //
            // lInterfacePreview
            //
            resources.ApplyResources(lInterfacePreview, "lInterfacePreview");
            lInterfacePreview.AutoEllipsis = true;
            lInterfacePreview.Name = "lInterfacePreview";
            //
            // gbTables
            //
            gbTables.Controls.Add(lTablesHint);
            gbTables.Controls.Add(cboxTablesCustom);
            gbTables.Controls.Add(cbTablesFamily);
            gbTables.Controls.Add(lTablesSize);
            gbTables.Controls.Add(nudTablesSize);
            gbTables.Controls.Add(lTablesPreview);
            gbTables.DisabledForeColor = SystemColors.GrayText;
            resources.ApplyResources(gbTables, "gbTables");
            gbTables.Name = "gbTables";
            gbTables.TabStop = false;
            //
            // lTablesHint
            //
            resources.ApplyResources(lTablesHint, "lTablesHint");
            lTablesHint.Name = "lTablesHint";
            //
            // cboxTablesCustom
            //
            resources.ApplyResources(cboxTablesCustom, "cboxTablesCustom");
            cboxTablesCustom.BoxBackColor = Color.White;
            cboxTablesCustom.HighlightColor = SystemColors.Highlight;
            cboxTablesCustom.Name = "cboxTablesCustom";
            cboxTablesCustom.CheckedChanged += CboxCustom_CheckedChanged;
            //
            // cbTablesFamily
            //
            cbTablesFamily.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbTablesFamily.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTablesFamily.FormattingEnabled = true;
            resources.ApplyResources(cbTablesFamily, "cbTablesFamily");
            cbTablesFamily.Name = "cbTablesFamily";
            cbTablesFamily.UseDarkScrollBar = false;
            cbTablesFamily.SelectedIndexChanged += FontControl_Changed;
            //
            // lTablesSize
            //
            resources.ApplyResources(lTablesSize, "lTablesSize");
            lTablesSize.Name = "lTablesSize";
            //
            // nudTablesSize
            //
            nudTablesSize.DecimalPlaces = 0;
            resources.ApplyResources(nudTablesSize, "nudTablesSize");
            nudTablesSize.HighlightColor = SystemColors.Highlight;
            nudTablesSize.Maximum = new decimal(new int[] { 48, 0, 0, 0 });
            nudTablesSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            nudTablesSize.Name = "nudTablesSize";
            nudTablesSize.SelectedButtonColor = SystemColors.Highlight;
            nudTablesSize.Value = new decimal(new int[] { 9, 0, 0, 0 });
            nudTablesSize.ValueChanged += FontControl_Changed;
            //
            // lTablesPreview
            //
            resources.ApplyResources(lTablesPreview, "lTablesPreview");
            lTablesPreview.AutoEllipsis = true;
            lTablesPreview.Name = "lTablesPreview";
            //
            // gbEditor
            //
            gbEditor.Controls.Add(lEditorHint);
            gbEditor.Controls.Add(cboxEditorCustom);
            gbEditor.Controls.Add(cbEditorFamily);
            gbEditor.Controls.Add(lEditorSize);
            gbEditor.Controls.Add(nudEditorSize);
            gbEditor.Controls.Add(cboxMonospaced);
            gbEditor.Controls.Add(lEditorPreview);
            gbEditor.DisabledForeColor = SystemColors.GrayText;
            resources.ApplyResources(gbEditor, "gbEditor");
            gbEditor.Name = "gbEditor";
            gbEditor.TabStop = false;
            //
            // lEditorHint
            //
            resources.ApplyResources(lEditorHint, "lEditorHint");
            lEditorHint.Name = "lEditorHint";
            //
            // cboxEditorCustom
            //
            resources.ApplyResources(cboxEditorCustom, "cboxEditorCustom");
            cboxEditorCustom.BoxBackColor = Color.White;
            cboxEditorCustom.HighlightColor = SystemColors.Highlight;
            cboxEditorCustom.Name = "cboxEditorCustom";
            cboxEditorCustom.CheckedChanged += CboxCustom_CheckedChanged;
            //
            // cbEditorFamily
            //
            cbEditorFamily.DropDownSelectedRowBackColor = SystemColors.Highlight;
            cbEditorFamily.DropDownStyle = ComboBoxStyle.DropDownList;
            cbEditorFamily.FormattingEnabled = true;
            resources.ApplyResources(cbEditorFamily, "cbEditorFamily");
            cbEditorFamily.Name = "cbEditorFamily";
            cbEditorFamily.UseDarkScrollBar = false;
            cbEditorFamily.SelectedIndexChanged += FontControl_Changed;
            //
            // lEditorSize
            //
            resources.ApplyResources(lEditorSize, "lEditorSize");
            lEditorSize.Name = "lEditorSize";
            //
            // nudEditorSize
            //
            nudEditorSize.DecimalPlaces = 0;
            resources.ApplyResources(nudEditorSize, "nudEditorSize");
            nudEditorSize.HighlightColor = SystemColors.Highlight;
            nudEditorSize.Maximum = new decimal(new int[] { 48, 0, 0, 0 });
            nudEditorSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            nudEditorSize.Name = "nudEditorSize";
            nudEditorSize.SelectedButtonColor = SystemColors.Highlight;
            nudEditorSize.Value = new decimal(new int[] { 9, 0, 0, 0 });
            nudEditorSize.ValueChanged += FontControl_Changed;
            //
            // cboxMonospaced
            //
            resources.ApplyResources(cboxMonospaced, "cboxMonospaced");
            cboxMonospaced.BoxBackColor = Color.White;
            cboxMonospaced.HighlightColor = SystemColors.Highlight;
            cboxMonospaced.Name = "cboxMonospaced";
            cboxMonospaced.CheckedChanged += CboxMonospaced_CheckedChanged;
            //
            // lEditorPreview
            //
            resources.ApplyResources(lEditorPreview, "lEditorPreview");
            lEditorPreview.AutoEllipsis = true;
            lEditorPreview.Name = "lEditorPreview";
            //
            // FontsPage
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(gbEditor);
            Controls.Add(gbTables);
            Controls.Add(gbInterface);
            Name = "FontsPage";
            resources.ApplyResources(this, "$this");
            gbInterface.ResumeLayout(false);
            gbInterface.PerformLayout();
            ((ISupportInitialize)nudInterfaceSize).EndInit();
            gbTables.ResumeLayout(false);
            gbTables.PerformLayout();
            ((ISupportInitialize)nudTablesSize).EndInit();
            gbEditor.ResumeLayout(false);
            gbEditor.PerformLayout();
            ((ISupportInitialize)nudEditorSize).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ExGroupBox gbInterface;
        private Label lInterfaceHint;
        private ExCheckBox cboxInterfaceCustom;
        private ExComboBox cbInterfaceFamily;
        private Label lInterfaceSize;
        private ExNumericUpDown nudInterfaceSize;
        private Label lInterfacePreview;
        private ExGroupBox gbTables;
        private Label lTablesHint;
        private ExCheckBox cboxTablesCustom;
        private ExComboBox cbTablesFamily;
        private Label lTablesSize;
        private ExNumericUpDown nudTablesSize;
        private Label lTablesPreview;
        private ExGroupBox gbEditor;
        private Label lEditorHint;
        private ExCheckBox cboxEditorCustom;
        private ExComboBox cbEditorFamily;
        private Label lEditorSize;
        private ExNumericUpDown nudEditorSize;
        private ExCheckBox cboxMonospaced;
        private Label lEditorPreview;
    }
}
