using System.Globalization;
using ToolsCore.Iniss.Calendar;
using ToolsCore.XML;
using ToolsCore.Tools;

namespace ToolsCore.Forms.Calendar;

/// <summary>
/// Dialog na upravu datumoveho obmedzenia v kalendari obdobia.
/// </summary>
public partial class FDateLimitEdit : Form
{
    private DateLimit _dateLimit = null!;
    private string? _title;
    private CalendarCell? _lastSelectedCell;
    private int _previousSelectedRow;
    private int _previousSelectedCol;
    private bool _needUpdateText;
    private bool _needCheckText;
    private bool _textChanging;

    private readonly Style _style;

    private FDateLimitEdit(Style style)
    {
        _style = style;
        InitializeComponent();
        this.ApplyThemeAndFonts();
        InitColumns();
    }

    /// <summary>
    /// Otvori editor datumoveho obmedzenia. Jazyk textu a krajinu sviatkov urcuje <see cref="DateLimit.Loc" />.
    /// </summary>
    /// <param name="owner">okno, nad ktorym sa editor otvori</param>
    /// <param name="style">styl programu</param>
    /// <param name="dateFrom">prvy den obdobia</param>
    /// <param name="dateTo">posledny den obdobia</param>
    /// <param name="title">co sa upravuje (napr. vlak) - pripoji sa k titulku okna; <see langword="null" /> = nic</param>
    /// <param name="textNot">upravuje sa doplnok obmedzenia <paramref name="defaultValue" /></param>
    /// <param name="defaultValue">povodne obmedzenie</param>
    /// <param name="proposal">navrhovane obmedzenie; povodne ostava v poli s povodnym obmedzenim na porovnanie</param>
    /// <returns>upravene obmedzenie; <see langword="null" />, ak ho pouzivatel zrusil</returns>
    public static string? SetDateLimit(Form owner, Style style, DateTime dateFrom, DateTime dateTo, string? title = null, bool textNot = false,
        string defaultValue = "", string? proposal = null)
    {
        using var form = new FDateLimitEdit(style);
        form.Owner = owner;
        form._title = title;
        form._dateLimit = new DateLimit(dateFrom, dateTo, insertMarks: false);
        form._textChanging = true;
        form.tbDateLimit.Text = defaultValue;
        
        form.tbOldDateLimit.Text = defaultValue;
        if (textNot)
            form.tbDateLimit.Text = form._dateLimit.TextNot(defaultValue);
        // navrhovane obmedzenie - povodne ostava v poli s povodnym obmedzenim na porovnanie
        if (proposal != null)
            form.tbDateLimit.Text = proposal;
        form.InitCalendar(dateFrom, dateTo);
        form.TextToGrid();
        form.tbDateLimit.Select();
        form._textChanging = false;
        return form.ShowDialog() is DialogResult.OK ? form.tbDateLimit.Text : null;
    }

    private void FDateLimit_Load(object sender, EventArgs e)
    {
        CalculateFormSize();
        if (!string.IsNullOrEmpty(_title)) 
            Text += " " + _title;
        timer.Start();
    }

    private void CalculateFormSize()
    {
        var width = 0;
        width += Width - dgvCalendar.Width;
        for (var i = 0; i < dgvCalendar.ColumnCount; i++)
        {
            if (dgvCalendar.Columns[i].Visible) 
                width += dgvCalendar.Columns[i].Width;
        }

        var height = 5;
        height += Height - dgvCalendar.Height;
        height += dgvCalendar.ColumnHeadersHeight;
        for (var i = 0; i < dgvCalendar.RowCount; i++)
        {
            height += dgvCalendar.Rows[i].Height;
        }

        
        Width = width;
        if (height < 550) 
            Height = height;

        var minimumSize = MinimumSize;
        minimumSize.Width = width;
        MinimumSize = minimumSize;
    }

    private void InitColumns()
    {
        dgvCalendar.Columns.Clear();
        var monthCol = new CalendarCellColumn();
        dgvCalendar.Columns.Add(monthCol);

        monthCol.Name = "c0";
        monthCol.HeaderText = GlobalResources.Calendar_Month;
        monthCol.Width = (int)Math.Round(TextRenderer.MeasureText("99.9999", dgvCalendar.Font).Width * 1.05 + 0.5);
        monthCol.SortMode = DataGridViewColumnSortMode.NotSortable;
        monthCol.DefaultCellStyle.BackColor = _style.ControlsColorScheme.Box.BackColor;
        monthCol.DefaultCellStyle.ForeColor = _style.ControlsColorScheme.Box.ForeColor;
        monthCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dgvCalendar.RowTemplate.Height = TextRenderer.MeasureText("Čý", dgvCalendar.Font).Height + 3;
        dgvCalendar.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        
        for (var i = 1; i <= 38; i++)
        {
            var dayofweek = (DayOfWeek)(i % 7);
            var dayCol = new CalendarCellColumn();
            dgvCalendar.Columns.Add(dayCol);
            dayCol.Name = "c" + i;
            dayCol.HeaderText = DateTimeFormatInfo.CurrentInfo.GetDayName(dayofweek).Substring(0, 2);
            dayCol.HeaderCell.ToolTipText = DateTimeFormatInfo.CurrentInfo.GetDayName(dayofweek);
            dayCol.Width = TextRenderer.MeasureText("99", dgvCalendar.Font).Width + 3;
            dayCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            dayCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            dayCol.DefaultCellStyle.BackColor = _style.ControlsColorScheme.Box.BackColor;
            dayCol.DefaultCellStyle.ForeColor = _style.ControlsColorScheme.Box.ForeColor;
        }
    }

    private void InitCalendar(DateTime dateFrom, DateTime dateTo)
    {
        var colIndex = -1; //index stlpca
        var rowIndex = -1; //index riadku
        var currentDate = dateFrom; //prave spravovavany datum v iteracii cyklu
        var currentDay = -1; //prave spracovavany mesiac
        var currentMonth = -1; //prave spracovavany mesiac
        var absoluteIndex = 0; //kazde policko ma unikatne ID

        //hlavna slucka
        while (currentDate <= dateTo)
        {
            //ked prechadzame do noveho mesiaca
            if (currentDate.Month != currentMonth)
            {
                currentDay = currentDate.Day;
                currentMonth = currentDate.Month;
                
                rowIndex = dgvCalendar.Rows.Add();
                dgvCalendar[0, rowIndex].Value = currentDate.ToString("M.yyyy", CultureInfo.CurrentCulture);

                //kvoli zarovnaniu na dni v tyzdni
                colIndex = (int)currentDate.AddDays(-currentDay).DayOfWeek;
                if (colIndex == 0) 
                    colIndex = 7;

                colIndex += currentDay;
            }

            var cell = (CalendarCell)dgvCalendar[colIndex, rowIndex];
            cell.Style.BackColor = _style.ControlsColorScheme.Box.BackColor;
            cell.Value = currentDay;
            cell.ToolTipText = currentDate.ToString("dddd d. MMMM yyyy", CultureInfo.CurrentCulture);

            if (currentDate.DayOfWeek == DayOfWeek.Sunday || DateLimit.IsHoliday(currentDate))
            {
                cell.Style.BackColor = Color.MediumVioletRed; //policko znazornuje nedelu alebo sviatok
                cell.Style.ForeColor = Color.White;
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Saturday)
            {
                cell.Style.BackColor = Color.SteelBlue; //policko znazornuje sobotu
                cell.Style.ForeColor = Color.White;
            }

            cell.AbsoluteIndex = absoluteIndex; //pridelime ID policku

            //priprava na dalsiu iteraciu (inkrementacia hodnot)
            currentDate = currentDate.AddDays(1);
            currentDay++;
            absoluteIndex++;
            colIndex++;
        }
    }

    private bool TextToGrid()
    {
        try
        {
            var bitArray = _dateLimit.TextToBitArray(tbDateLimit.Text);
            for (var i = 0; i < dgvCalendar.Rows.Count; i++)
            {
                for (var j = 0; j < dgvCalendar.Columns.Count; j++)
                {
                    var cell = (CalendarCell) dgvCalendar[j, i];
                    if (cell.AbsoluteIndex >= 0 && cell.SelectedDay != bitArray[cell.AbsoluteIndex])
                    {
                        cell.SelectedDay = bitArray[cell.AbsoluteIndex];
                        dgvCalendar.InvalidateCell(cell);
                    }
                }
            }

            _textChanging = true;
            tbDateLimit.Text = _dateLimit.BitArrayToText(bitArray);
            _textChanging = false;
        }
        catch (Exception e)
        {
            Utils.ShowError(e.Message);
            tbDateLimit.Select();
            return false;
        }

        return true;
    }

    private void GridToText()
    {
        var bitArray = _dateLimit.CreateBitArray();
        for (var i = 0; i < dgvCalendar.Rows.Count; i++)
        {
            for (var j = 1; j < dgvCalendar.Columns.Count; j++)
            {
                var cell = (CalendarCell) dgvCalendar[j, i];
                if (cell.AbsoluteIndex >= 0 && cell.SelectedDay)
                {
                    bitArray[cell.AbsoluteIndex] = true;
                }
            }
        }

        _textChanging = true;
        tbDateLimit.Text = _dateLimit.BitArrayToText(bitArray);
        _textChanging = false;
    }

    private void BReset_Click(object sender, EventArgs e)
    {
        tbDateLimit.Text = tbOldDateLimit.Text;
        TextToGrid();
    }

    private void BCheck_Click(object sender, EventArgs e) => TextToGrid();

    private void DgvCalendar_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e)
    {
        //zabudovny mechanizmus selektovania sa tu nepouziva
        e.Cell.Selected = false;
    }

    private void DgvCalendar_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button is not (MouseButtons.Left or MouseButtons.Right)) 
            return;

        if (!ModifierKeys.HasFlag(Keys.Shift))
        {
            var doNotResetLastSelected = false;
            for (var i = 0; i < dgvCalendar.Rows.Count; i++)
            {
                for (var j = 0; j < dgvCalendar.Columns.Count; j++)
                {
                    var cell = (CalendarCell) dgvCalendar[j, i];
                    if (cell.AbsoluteIndex >= 0 && j == e.ColumnIndex && i == e.RowIndex)
                    {
                        doNotResetLastSelected = true;
                        cell.Highlighted = true;
                        cell.HighlightColor = _style.ControlsColorScheme.Box.ForeColor;
                        cell.SelectedDay = !cell.SelectedDay;
                        _lastSelectedCell = cell;
                        dgvCalendar.InvalidateCell(cell);
                        _needUpdateText = true;
                    }
                    else if (cell.Highlighted)
                    {
                        cell.Highlighted = false;
                        dgvCalendar.InvalidateCell(cell);
                    }
                }
            }
            if (!doNotResetLastSelected)
            {
                _lastSelectedCell = null;
            }
        }
        else
        {
            CalendarCell? oToCell = null;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) 
                oToCell = (CalendarCell)dgvCalendar[e.ColumnIndex, e.RowIndex];

            PaintSelection(_lastSelectedCell, oToCell);
        }
    }

    private void DgvCalendar_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button is not (MouseButtons.Left or MouseButtons.Right) || (e.RowIndex == _previousSelectedRow && e.ColumnIndex == _previousSelectedCol)) 
            return;

        _previousSelectedRow = e.RowIndex;
        _previousSelectedCol = e.ColumnIndex;
        CalendarCell? toCell = null;
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            toCell = (CalendarCell)dgvCalendar[e.ColumnIndex, e.RowIndex];
        }

        PaintSelection(_lastSelectedCell, toCell);
    }

    private void DgvCalendar_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
    {
        DoSelectionFromPainted();
    }

    private void DoSelectionFromPainted()
    {
        var needTextUpdate = false;
        checked
        {
            for (var i = 0; i < dgvCalendar.Rows.Count; i++)
            {
                for (var j = 0; j < dgvCalendar.Columns.Count; j++)
                {
                    var cell = (CalendarCell)dgvCalendar[j, i];

                    //policko nie je selektovatelne (je prazdne)
                    if (cell.AbsoluteIndex < 0) 
                        continue;

                    switch (cell.Action)
                    {
                        case CalendarCellAction.Selection:
                        {
                            if (!cell.SelectedDay)
                            {
                                cell.SelectedDay = true;
                                dgvCalendar.InvalidateCell(cell);
                                needTextUpdate = true;
                            }

                            cell.Action = CalendarCellAction.None;
                            break;
                        }
                        case CalendarCellAction.Deselection:
                        {
                            if (cell.SelectedDay)
                            {
                                cell.SelectedDay = false;
                                dgvCalendar.InvalidateCell(cell);
                                needTextUpdate = true;
                            }

                            cell.Action = CalendarCellAction.None;
                            break;
                        }
                    }
                }
            }
            if (needTextUpdate)
            {
                _needUpdateText = true;
            }
        }
    }

    private void PaintSelection(CalendarCell? fromCell, CalendarCell? toCell)
    {
        int fromCellAbsIndex;
        bool doSelection;
        int toCellAbsIndex;

        if (fromCell != null && toCell is { AbsoluteIndex: >= 0 })
        {
            doSelection = fromCell.SelectedDay;
            fromCellAbsIndex = fromCell.AbsoluteIndex;
            
            toCellAbsIndex = toCell.AbsoluteIndex;
        }
        else
        {
            doSelection = false;
            fromCellAbsIndex = int.MinValue;
            toCellAbsIndex = int.MinValue;
        }

        var min = Math.Min(fromCellAbsIndex, toCellAbsIndex);
        var max = Math.Max(fromCellAbsIndex, toCellAbsIndex);

        checked
        {
            for (var i = 0; i < dgvCalendar.Rows.Count; i++)
            {
                for (var j = 0; j < dgvCalendar.Columns.Count; j++)
                {
                    var cell = (CalendarCell) dgvCalendar[j, i];

                    CalendarCellAction action;
                    if (cell.AbsoluteIndex >= min && cell.AbsoluteIndex <= max && cell.AbsoluteIndex != fromCellAbsIndex)
                    {
                        action = doSelection ? CalendarCellAction.Selection : CalendarCellAction.Deselection;
                    }
                    else
                    {
                        action = CalendarCellAction.None;
                    }

                    if (cell.Action != action)
                    {
                        cell.Action = action;
                        dgvCalendar.InvalidateCell(cell);
                    }
                }
            }
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        timer.Stop();
        if (_needUpdateText)
        {
            _needUpdateText = false;
            GridToText();
        }
        timer.Start();
    }

    private void FDateLimit_FormClosed(object sender, FormClosedEventArgs e)
    {
        timer.Stop();
    }

    private void TbDateLimit_TextChanged(object sender, EventArgs e)
    {
        if (_textChanging)
            return;

        _needCheckText = true;
    }

    private void BOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;

        if (_needCheckText) 
            DialogResult = TextToGrid() ? DialogResult.OK : DialogResult.None;
    }

    private void BDaily_Click(object sender, EventArgs e) => SelectDates(true);

    private void BNever_Click(object sender, EventArgs e) => SelectDates(false);

    private void SelectDates(bool daily) //daily = false, means never
    {
        var needUpdateText = false;
        for (var i = 0; i < dgvCalendar.Rows.Count; i++)
        {
            for (var j = 0; j < dgvCalendar.Columns.Count; j++)
            {
                var framedCell = (CalendarCell) dgvCalendar[j, i];
                if (framedCell.AbsoluteIndex >= 0 && framedCell.SelectedDay != daily)
                {
                    framedCell.SelectedDay = daily;
                    needUpdateText = true;
                    dgvCalendar.InvalidateCell(framedCell);
                }
            }
        }

        if (needUpdateText) 
            _needUpdateText = true;
    }
}
