﻿using Bridge;
using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExpressCraft
{
    public class GridView : Control
    {
        public HTMLDivElement GridFindPanel;

        public HTMLDivElement GridHeader;
        public HTMLDivElement GridHeaderContainer;
        public HTMLDivElement GridBodyContainer;
        public HTMLDivElement GridBody;

        private HTMLDivElement BottonOfTable;
        private HTMLDivElement RightOfTable;
        private HTMLDivElement RightOfTableHeader;

        public TextInput SearchTextInput;
        public SimpleButton btnFind;
        public SimpleButton btnClear;
        public SimpleButton btnClose;

        private ContextItem _showFindPanelContextItem;

        private bool _findPanelVisible;

        public bool FindPanelVisible
        {
            get { return _findPanelVisible; }
            set {
                if(_findPanelVisible != value)
                {
                    if(value)
                        ShowFindPanel();
                    else
                        CloseFindPanel();
                                        
                }
                
            }
        }

        public bool ResolveSearchDataIndex()
        {
            return (VisibleRowHandles != null && VisibleRowHandles.Count > 0);
        }

        private bool _highlighSearchResults = true;

        public bool HighlighSearchResults
        {
            get { return _highlighSearchResults; }
            set {
                if(_highlighSearchResults != value)
                {
                    _highlighSearchResults = value;
                    RenderGrid();
                }                    
            }
        }

        public void ShowFindPanel()
        {
            if(!_findPanelVisible)
            {
                _showFindPanelContextItem.Caption = "Close Find Panel";
                _findPanelVisible = true;
                GridFindPanel.Style.Visibility = Visibility.Inherit;

                SetDefaultSizes();

                RenderGrid();
            }
                
        }

        public void CloseFindPanel()
        {
            if(_findPanelVisible)
            {
                _showFindPanelContextItem.Caption = "Show Find Panel";
                _findPanelVisible = false;
                GridFindPanel.Style.Visibility = Visibility.Hidden;

                SetDefaultSizes();

                RenderGrid();
            }            
        }


        private DataTable _dataSource = null;

        public Action<int, int> OnFocusedRowChanged = null;
        public Action<int> OnRowDoubleClick = null;
        public Action<HTMLElement, int> OnCustomRowStyle = null;

        protected Action<MouseEvent<HTMLDivElement>> OnRowClick;
        protected Action<MouseEvent<HTMLDivElement>> OnDoubleClick;
        protected Action<MouseEvent> OnCellRowMouseDown;

        public HardSoftList<bool> SelectedRows = new HardSoftList<bool>(false);
        public List<int> VisibleRowHandles = null;

        public void SetVisibleRowHandles<T>(List<T> Cells, bool asc)
        {
            if(DataSource._searchActive)
            {
                if(asc)
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .Where((p) => DataSource._searchResults.Contains(p.Key))
                        .OrderBy(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
                else
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .Where((p) => DataSource._searchResults.Contains(p.Key))
                        .OrderByDescending(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
            }
            else
            {
                if(asc)
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))                        
                        .OrderBy(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
                else
                {
                    var sorted = Cells
                        .Select((x, i) => new KeyValuePair<int, T>(i, x))
                        .OrderByDescending(x => x.Value)
                        .ToList();

                    VisibleRowHandles = sorted.Select(x => x.Key).ToList();
                }
            }
            
        }

        public bool _allowRowDrag = false;

        public bool AllowRowDrag
        {
            get { return _allowRowDrag; }
            set
            {
                if(_allowRowDrag != value)
                {
                    _allowRowDrag = value;
                    RenderGrid();
                }
            }
        }

        public bool AutoGenerateColumnsFromSource = true;
        public bool AllowMultiSelection = true;

        private bool showAutoFilterRow = false;

        public bool ShowAutoFilterRow
        {
            get { return showAutoFilterRow; }
            set
            {
                if(showAutoFilterRow != value)
                {
                    showAutoFilterRow = value;
                    if(!showAutoFilterRow)
                    {
                        // Remove Filter.
                        for(int i = 0; i < ColumnCount(); i++)
                        {
                            //FilterEdit = null;
                            Columns[i].FilterEdit = null;
                            Columns[i].FilterValue = null;
                        }
                        CalculateVisibleRows();
                    }
                    RenderGrid();
                }
            }
        }

        public void CalculateVisibleRows()
        {
            List<int> calcVisibleRows = new List<int>();

            for(int y = 0; y < RowCount(); y++)
            {
                bool AddIndex = true;

                for(int x = 0; x < ColumnCount(); x++)
                {
                    if(!Columns[x].ValueMatchFilter(y))
                    {
                        AddIndex = false;
                        break;
                    }
                }
                if(AddIndex)
                {
                    calcVisibleRows.Add(y);
                }
            }

            VisibleRowHandles = calcVisibleRows;
            RenderGrid();
        }

        public float UnitHeight = 28.0f;
        private bool _columnAutoWidth = false;

        private int _focusedcolumn = -1;

        public int FocusedColumn
        {
            get
            {
                return _focusedcolumn;
            }
            set
            {
                if(value != FocusedColumn)
                {
                    var prev = _focusedcolumn;
                    _focusedcolumn = value;
                    //RenderGrid();
                }
            }
        }

        private int _focusedDataHandle = -1;

        public int FocusedDataHandle
        {
            get
            {
                return _focusedDataHandle;
            }
            set
            {
                if(value != _focusedDataHandle)
                {
                    var prev = _focusedDataHandle;
                    
                    _focusedDataHandle = value;
                    RenderGrid();
                    if(OnFocusedRowChanged != null)
                        OnFocusedRowChanged(_focusedDataHandle, prev);
                }
            }
        }

        public void SetDefaultSizes()
        {
            if(_columnHeadersVisible)
            {
                GridHeader.Style.Visibility = Visibility.Inherit;
                GridHeaderContainer.Style.Visibility = Visibility.Inherit;

                if(FindPanelVisible)
                {
                    GridHeaderContainer.SetBounds(0, 47, "100%", UnitHeight + 1);
                    GridBodyContainer.SetBounds(0, UnitHeight + 3 + 47, "100%", "(100% - " + (UnitHeight + 3 + 47) + "px)");
                }
                else
                {
                    GridHeaderContainer.SetBounds(0, 0, "100%", UnitHeight + 1);
                    GridBodyContainer.SetBounds(0, UnitHeight + 3, "100%", "(100% - " + (UnitHeight + 3) + "px)");                    
                }                
            }
            else
            {
                GridHeader.Style.Visibility = Visibility.Hidden;
                GridHeaderContainer.Style.Visibility = Visibility.Hidden;

                if(FindPanelVisible)
                {
                    GridBodyContainer.SetBounds(0, 1 + 46, "100%", "(100% - " + (1 + 46)  + "px)");
                }
                else
                {
                    GridBodyContainer.SetBounds(0, 1, "100%", "(100% - 1px)");
                }                
            }
        }

        private bool _columnHeadersVisible = true;

        public bool ColumnHeadersVisible
        {
            get
            {
                return _columnHeadersVisible;
            }
            set
            {
                if(value != _columnHeadersVisible)
                {
                    _columnHeadersVisible = value;

                    SetDefaultSizes();

                    RenderGrid();
                }
            }
        }

        public bool ColumnAutoWidth
        {
            get
            {
                return _columnAutoWidth;
            }
            set
            {
                if(value)
                    GridBodyContainer.Style.OverflowX = Overflow.Hidden;
                else
                    GridBodyContainer.Style.OverflowX = Overflow.Auto;

                if(_columnAutoWidth != value)
                {
                    _columnAutoWidth = value;
                    RenderGrid();
                }
            }
        }

        private bool _useEditForm = true;

        public bool UseEditForm
        {
            get
            {
                return _useEditForm;
            }
            set
            {
                if(value != _useEditForm)
                {
                    _useEditForm = value;
                    RenderGrid();
                }
            }
        }

        private SortSetting SortSettings;

        public void SortColumn()
        {
            if(SortSettings != null)
            {
                SortColumn(SortSettings.Column, SortSettings.SortMode);
            }
        }

        public void ClearSortColumn()
        {
            if(SortSettings != null)
            {
                SortColumn(SortSettings.Column, GridViewSortMode.None);
            }
        }

        public void SortColumn(GridViewColumn column, GridViewSortMode sort = GridViewSortMode.Asc)
        {
            column.SortedMode = sort;

            if(SortSettings != null && SortSettings.Column != column)
            {
                SortSettings.Column.SortedMode = GridViewSortMode.None;
                VisibleRowHandles = null;
            }

            if(sort == GridViewSortMode.None)
            {
                VisibleRowHandles = null;
            }
            else
            {
                bool sort1 = sort == GridViewSortMode.Asc;

                switch(column.Column.DataType)
                {
                    default:
                    case DataType.Object:
                        SetVisibleRowHandles((column.Column as DataColumnObject).Cells, sort1);
                        break;

                    case DataType.Bool:
                        SetVisibleRowHandles((column.Column as DataColumnBool).Cells, sort1);
                        break;

                    case DataType.DateTime:
                        SetVisibleRowHandles((column.Column as DataColumnDateTime).Cells, sort1);
                        break;

                    case DataType.String:
                        SetVisibleRowHandles((column.Column as DataColumnString).Cells, sort1);
                        break;

                    case DataType.Byte:
                        SetVisibleRowHandles((column.Column as DataColumnByte).Cells, sort1);
                        break;

                    case DataType.Short:
                        SetVisibleRowHandles((column.Column as DataColumnShort).Cells, sort1);
                        break;

                    case DataType.Integer:
                        SetVisibleRowHandles((column.Column as DataColumnInteger).Cells, sort1);
                        break;

                    case DataType.Long:
                        SetVisibleRowHandles((column.Column as DataColumnLong).Cells, sort1);
                        break;

                    case DataType.Float:
                        SetVisibleRowHandles((column.Column as DataColumnFloat).Cells, sort1);
                        break;

                    case DataType.Double:
                        SetVisibleRowHandles((column.Column as DataColumnDouble).Cells, sort1);
                        break;

                    case DataType.Decimal:
                        SetVisibleRowHandles((column.Column as DataColumnDecimal).Cells, sort1);
                        break;
                }
            }

            RenderGrid();
            SortSettings = new SortSetting()
            {
                Column = column,
                SortMode = sort
            };
        }

        public int ColumnCount()
        {
            return Columns.Count;
        }

        public int RowCount()
        {
            if(_dataSource == null)
                return 0;
            return _dataSource.RowCount;
        }

        public void ScrollToBottom()
        {
            GridBodyContainer.ScrollTop = GridBody.ClientHeight - GridBodyContainer.ClientHeight;
        }

        public void ScrollToTop()
        {
            GridBodyContainer.ScrollTop = 0;
        }

        public DataTable DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                FocusedDataHandle = -1;
                SelectedRows = new HardSoftList<bool>(false);
                VisibleRowHandles = new List<int>();

                if(_dataSource != null)
                {
                    _dataSource.OnDataSourceChanged -= DataSource_OnDataSourceChanged;
                }

                _dataSource = value;

                if(_dataSource != null)
                {
                    _dataSource.OnDataSourceChanged += DataSource_OnDataSourceChanged;

                    if(Columns.Count == 0 && AutoGenerateColumnsFromSource)
                    {
                        var sw = Stopwatch.StartNew();

                        for(int i = 0; i < _dataSource.ColumnCount; i++)
                        {
                            var sw1 = Stopwatch.StartNew();

                            var gvc = new GridViewColumn(this);
                            gvc.Caption = _dataSource.Columns[i].FieldName;
                            gvc.Column = _dataSource.Columns[i];
                            gvc.Visible = true;

                            switch(_dataSource.Columns[i].DataType)
                            {
                                case DataType.Byte:
                                case DataType.Short:
                                case DataType.Integer:
                                case DataType.Long:
                                case DataType.Float:
                                case DataType.Double:
                                case DataType.Decimal:
                                    gvc.BodyApparence.Alignment = TextAlign.Right;
                                    break;

                                case DataType.DateTime:
                                    if(Settings.GridViewAutoColumnFormatDates)
                                    {
                                        if(Settings.GridViewAutoColumnGenerateFormatAsDate)
                                            gvc.FormatString = "{0:d}";
                                        else
                                            gvc.FormatString = "{0:yyyy-MM-dd}";
                                    }

                                    break;

                                case DataType.Bool:
                                    gvc.CellDisplay = new GridViewCellDisplayCheckBox();
                                    break;
                            }

                            Columns.Add(gvc);

                            sw.Stop();
                            Console.WriteLine("DataSource AddColumn Auto: " + sw1.ElapsedMilliseconds);
                        }

                        sw.Stop();
                        Console.WriteLine("DataSource AutoColumns: " + sw.ElapsedMilliseconds);
                    }
                    RenderGrid();
                }
            }
        }

        public List<GridViewColumn> Columns = new List<GridViewColumn>();

        public GridViewColumn GetColumn(int i)
        {
            return Columns[i];
        }

        public object GetFocusedRowCellValue(int columnIndex)
        {
            return GetFocusedRowCellValue(Columns[columnIndex]);
        }

        public GridViewColumn GetGridViewColumnByFieldName(string FieldName)
        {
            for(int i = 0; i < ColumnCount(); i++)
            {
                if(Columns[i].Column.FieldName == FieldName)
                {
                    return Columns[i];
                }
            }
            return null;
        }

        public object GetFocusedRowCellValue(string FieldName)
        {
            return GetFocusedRowCellValue(GetColumnByFieldName(FieldName));
        }

        public object GetFocusedRowCellValue(GridViewColumn column)
        {
            return GetRowCellValue(FocusedDataHandle, column);
        }

        public object GetFocusedRowCellValue(DataColumn column)
        {
            return GetRowCellValue(FocusedDataHandle, column);
        }

        public object GetRowCellValue(int Datahandle, GridViewColumn column)
        {
            return GetRowCellValue(Datahandle, column.Column);
        }

        public object GetRowCellValue(int Datahandle, DataColumn column)
        {
            if(Datahandle == -1)
                return null;
            return column.GetCellValue(Datahandle);
        }

        public object GetRowCellValue(int Datahandle, string FieldName)
        {
            return GetRowCellValue(Datahandle, GetColumnByFieldName(FieldName));
        }

        public object GetRowCellValue(int Datahandle, int columnIndex)
        {
            return GetRowCellValue(Datahandle, Columns[columnIndex]);
        }

        public GridViewColumn GetGridViewColumnByFieldName(string fieldName, bool IgnoreCase = false)
        {
            for(int i = 0; i < ColumnCount(); i++)
            {
                if(Columns[i] != null && Columns[i].Column != null &&
                    string.Compare(Columns[i].Column.FieldName, fieldName, IgnoreCase) == 0)
                    return Columns[i];
            }

            return null;
        }

        public DataColumn GetColumnByFieldName(string fieldName, bool IgnoreCase = false)
        {
            if(DataSource == null)
                return null;

            for(int i = 0; i < DataSource.ColumnCount; i++)
            {
                if(DataSource.Columns[i] != null &&
                    string.Compare(DataSource.Columns[i].FieldName, fieldName, IgnoreCase) == 0)
                    return DataSource.Columns[i];
            }

            return null;
        }

        public void AddColumn(string caption, string fieldname, int width = 100, string formatstring = "", TextAlign alignment = TextAlign.Left, string forecolor = null, bool isBold = false)
        {
            var col = GetColumnByFieldName(fieldname);
            if(col == null)
                return;
            AddColumn(caption, col, width, formatstring, alignment, forecolor, isBold);
        }

        public void AddColumn(string caption, DataColumn column, int width = 100, string formatstring = "", TextAlign alignment = TextAlign.Left, string forecolor = null, bool isBold = false)
        {
            AddColumn(new GridViewColumn(this, width) { Caption = caption, BodyApparence = new GridViewCellApparence(isBold, alignment, forecolor), FormatString = formatstring, Column = column });
        }

        public void AddColumn(GridViewColumn column)
        {
            if(column == null)
                return;

            Columns.Add(column);

            RenderGrid();
        }

        public void AddColumns(params GridViewColumn[] columns)
        {
            if(columns == null || columns.Length == 0)
                return;

            Columns.AddRange(columns);

            RenderGrid();
        }

        public void RemoveColumn(GridViewColumn column)
        {
            Columns.Remove(column);

            RenderGrid();
        }

        public int GetDataSourceRow(int i)
        {            
            if(VisibleRowHandles == null || VisibleRowHandles.Count == 0)
            {
                if(DataSource._searchActive)
                {
                    return  DataSource._searchResults[i];
                }
                return i;
            }
                
            return VisibleRowHandles[i];
        }

        public float GetColumnWidths()
        {
            if(_columnAutoWidth)
            {
                return GridBodyContainer.ClientWidth;
            }
            else
            {
                float width = 0.0f;
                for(int i = 0; i < Columns.Count; i++)
                {
                    if(Columns[i].Visible)
                        width += Columns[i].Width;
                }
                return width;
            }
        }

        public void ClearSelection()
        {
            SelectedRows = new HardSoftList<bool>(false);
            RenderGrid();
        }

        public void SelectAllRows()
        {
            int length = RowCount();
            if(length == 0)
            {
                SelectedRows.ClearAll();
            }
            else
            {
                int[] index = new int[length];
                for(int i = 0; i < length; i++)
                {
                    index[i] = GetDataSourceRow(i);
                }
                SelectedRows.ClearAllSetHardRange(true, index);
            }
            RenderGrid();
        }

        private int PrevRenderGridScrollId = -1;

        public void DelayedRenderGrid(bool renderNoLag = false)
        {
            if(renderNoLag)
            {
                RenderGrid(false);
            }
            else
            {
                if(Settings.GridViewScrollDelayed)
                {
                    if(PrevRenderGridScrollId != -1)
                    {
                        Global.ClearTimeout(PrevRenderGridScrollId);
                        PrevRenderGridScrollId = -1;
                    }
                    PrevRenderGridScrollId = Global.SetTimeout(() =>
                    {
                        RenderGrid();
                    }, Math.Max(1, Settings.GridViewScrollDelayMS));
                }
                else
                {
                    RenderGrid();
                }
            }
            
        }

        private Stopwatch clickTimeDiff = null;

        public DataRow GetFocusedRow()
        {
            if(FocusedDataHandle > -1)
            {
                return DataSource[GetDataSourceRow(FocusedDataHandle)];
            }
            else
            {
                return null;
            }
        }

        public int GetVisibleCount()
        {
            if(Columns == null || Columns.Count == 0)
                return 0;
            int length = Columns.Count;
            int length1 = Columns.Count;

            for(int i = 0; i < length; i++)
            {
                if(!Columns[i].Visible)
                    length1--;
            }
            return length1;
        }

        public int GetBestFitForColumn(GridViewColumn column)
        {
            if(!column.Visible)
                return 0;

            int maxLength = 0;
            int maxIndex = -1;
            string maxStr = "";
            
            for(int i = 0; i < RowCount(); i++)
            {
                string value = column.GetDisplayValueByDataRowHandle(i);
                if(!string.IsNullOrWhiteSpace(value))
                {
                    int v = value.Length;
                    if(v > maxLength)
                    {
                        maxLength = v;
                        maxIndex = i;
                        maxStr = value;
                    }
                }
            }

            if(maxIndex > -1)
            {
                return (int)GetTextWidth(maxStr, Settings.DefaultFont) + 20;
            }else
            {
                return 0;
            }
        }

        public void BestFitAllColumns()
        {
            _disableRender = true;
            for(int i = 0; i < Columns.Count; i++)
            {
                if(Columns[i].Visible)
                {
                    Columns[i].Width = GetBestFitForColumn(Columns[i]);
                }
            }
            _disableRender = false;
            RenderGrid();
        }

        private string headingClass;
        private string cellClass;

        private Dictionary<int, HTMLDivElement> CacheRow = new Dictionary<int, HTMLDivElement>();
        int CountOfDeletion = 0;

        private int _searchTimer = -1;
        private void _search()
        {
            if(this.DataSource == null || !FindPanelVisible)
                return;
            this.DataSource.Search(SearchTextInput.Text, this);
        }

        public void MakeRowVisible(int rowHandle)
        {
            if(rowHandle < 0)
                return;

            var getTopMostRowIndex = GetRawTopRowIndex();

            if(rowHandle < getTopMostRowIndex)
            {
                GridBodyContainer.ScrollTop -= (int)((getTopMostRowIndex - rowHandle) * PixelsPerRow(RowCount()));                
            }
            else
            {
                getTopMostRowIndex = GetRawVisibleRowCount() + getTopMostRowIndex;
                if(rowHandle >= getTopMostRowIndex)
                {
                    GridBodyContainer.ScrollTop += (int)(((rowHandle - getTopMostRowIndex) + 1) * PixelsPerRow(RowCount()));                    
                }
            }
        }

        public GridView(bool autoGenerateColumns = true, bool columnAutoWidth = false) : base("grid")
        {
            if(Helper.NotDesktop)
            {
                UnitHeight = 53;
                headingClass = "heading heading-responsive";

                cellClass = "cell cell-responsive";
            }
            else
            {
                UnitHeight = 20;
                headingClass = "heading";
                cellClass = "cell";
            }

            this.Content.Style.Overflow = Overflow.Hidden;
            // #FIND #RENDER#
            renderGridInternal = () =>
            {
                if(_disableRender)
                    return;

                int StartedWith = RenderTime;

                GridHeaderContainer.ScrollLeft = GridBodyContainer.ScrollLeft;
                if(Settings.GridViewBlurOnScroll)
                    ProcessBlur();

                ValidateGridSize();

                if(ColumnCount() == 0)
                {
                    ClearGrid();
                    return;
                }

                int RawLeftCellIndex = 0;
                float RawLeftCellScrollPadding = 0;

                int RawLeftCellCount = Columns.Count;

                float LeftLocation = 0;
                bool foundLeftLocation = false;
                bool foundRightLocation = false;

                int ClientWidth = GridBodyContainer.ClientWidth;

                #region "Columns"

                float ViewWidth = GridBodyContainer.ScrollLeft + ClientWidth;
                float _columnAutoWidthSingle = 0.0f;

                if(_columnAutoWidth)
                {
                    _columnAutoWidthSingle = ClientWidth == 0 ? 0.0f : ClientWidth / GetVisibleCount();
                }

                float MaxWidth;
                float LastWidth;

                for(int x = 0; x < Columns.Count; x++)
                {
                    if(!Columns[x].Visible)
                        continue;

                    Columns[x].CachedX = LeftLocation;
                    LastWidth = _columnAutoWidth ? _columnAutoWidthSingle : Columns[x].Width;
                    LeftLocation += LastWidth;
                    if(!foundLeftLocation && LeftLocation >= GridBodyContainer.ScrollLeft)
                    {
                        foundLeftLocation = true;
                        RawLeftCellIndex = x;
                        RawLeftCellScrollPadding = LeftLocation - GridBodyContainer.ScrollLeft;
                    }
                    if(foundLeftLocation && !foundRightLocation && LeftLocation >= ViewWidth)
                    {
                        foundRightLocation = true;
                        RawLeftCellCount = x + 1;
                        break;
                    }
                    if(StartedWith != RenderTime)
                    {
                        return;
                    }
                }

                MaxWidth = LeftLocation;

                var colFragment = Document.CreateDocumentFragment();

                int uboundRowCount = RawLeftCellCount - 1;

                if(_columnHeadersVisible)
                {
                    for(int x = RawLeftCellIndex; x < RawLeftCellCount; x++)
                    {
                        if(x >= Columns.Count)
                            break;
                        if(!Columns[x].Visible)
                            continue;

                        var gcol = Columns[x];
                        var colIndex = x;
                        var apparence = gcol.HeadingApparence;

                        var col = Label(gcol.Caption,
                            (_columnAutoWidth ? gcol.CachedX : gcol.CachedX), 0, (_columnAutoWidth ? _columnAutoWidthSingle : gcol.Width) - (x == uboundRowCount ? 0 : 1),
                            apparence.IsBold, false, headingClass, apparence.Alignment, apparence.Forecolor);
                        if(gcol.SortedMode != GridViewSortMode.None)
                        {
                            var sortImage = Div(gcol.SortedMode == GridViewSortMode.Asc ? "grid-sort-up" : "grid-sort-down");
                            sortImage.SetBounds("(100% - 13px)", 11, 9, 5);
                            col.AppendChild(sortImage);
                        }

                        SetupColumn(col, x, gcol);

                        colFragment.AppendChild(col);

                        if(StartedWith != RenderTime)
                        {
                            return;
                        }
                    }
                }

                #endregion "Columns"

                if(_dataSource == null || _dataSource.RowCount == 0 || _dataSource.ColumnCount == 0)
                {
                    ClearGrid();
                    GridHeader.AppendChild(colFragment);
                    return;
                }

                var ppr = PixelsPerRow(_dataSource.RowCount);

                float RawTopRowIndex = GetRawTopRowIndex();
                float RawTopRowScrollPadding = RawTopRowIndex % 1.0f;
                float RawVisibleRowCount = GetRawVisibleRowCount();

                int Length = (int)(RawVisibleRowCount + RawTopRowIndex) + 1;
                int start = (int)RawTopRowIndex;

                #region "Selection"

                for(int x = SelectedRows.SL.Count - 1; x >= 0; x--)
                {
                    bool Found = false;
                    for(int i = start; i < Length; i++)
                    {
                        if(i < DataSource.RowCount)
                        {
                            var DataRowhandle = GetDataSourceRow(i);
                            if(SelectedRows.GetIndexValueByHardListIndex(SelectedRows.SL[x]).Index == DataRowhandle)
                            {
                                Found = true;
                                break;
                            }
                        }
                        if(StartedWith != RenderTime)
                        {
                            return;
                        }
                    }
                    if(StartedWith != RenderTime)
                    {
                        return;
                    }
                    if(!Found)
                    {
                        SelectedRows.SL.RemoveAt(x);
                    }
                }

                #endregion "Selection"

                var rowFragment = Document.CreateDocumentFragment();

                if(Settings.GridViewRowScrollPadding > 0)
                {
                    start -= Settings.GridViewRowScrollPadding;
                    Length += Settings.GridViewRowScrollPadding;
                }

                float Y = (start * ppr);// + RawTopRowScrollPadding;

                if(ShowAutoFilterRow)
                {
                    Length -= 1;
                    Y += UnitHeight;
                }

                // #TODO - CLEAN...
                if(start < 0)
                    start = 0;
                if(Length > DataSource.RowCount)
                    Length = DataSource.RowCount;

                if(CacheRow.Count > 10)
                {                   
                    if(CountOfDeletion > 8)
                    {
                        CacheRow = new Dictionary<int, HTMLDivElement>();
                        CountOfDeletion = 0;
                    }
                    else
                    {
                        int MaxDelete = CacheRow.Count - 10;
                        int __length = CacheRow.Count;
                        List<int> KeysToDelete = new List<int>();
                        for(int i = 0; i < __length; i++)
                        {
                            int fieldIndex = CacheRow.ElementAt(i).Key;
                            if(fieldIndex < start || fieldIndex >= Length)
                            {
                                KeysToDelete.Add(fieldIndex);
                                if(KeysToDelete.Count > MaxDelete)
                                {                                    
                                    break;
                                }
                            }
                        }
                        __length = KeysToDelete.Count;
                        if(__length > 0)
                            CountOfDeletion++;
                        for(int i = 0; i < __length; i++)
                        {
                            if(CacheRow.ContainsKey(KeysToDelete[i]))
                            {
                                var x = CacheRow[KeysToDelete[i]];
                                x.OnClick = null;
                                x.OnDblClick = null;
                                x.Empty();
                                x.OnDragStart = null;
                                x.Delete();

                                CacheRow.Remove(KeysToDelete[i]);
                            }                            
                        }
                    }
                    
                }

                int prevRowCache = CacheRow.Count;
                
                for(int i = start; i < Length; i++)
                {
                    if(!CacheRow.ContainsKey(i))
                    {
                        var DataRowhandle = GetDataSourceRow(i);
                        var dr = Div((i % 2 == 0 ? "cellrow even" : "cellrow") + (SelectedRows.GetValue(DataRowhandle, true) ? " cellrow-selected" : "") + (DataRowhandle == FocusedDataHandle ? " focusedrow" : ""));

                        dr.SetBounds(0, Y, _columnAutoWidth ? ClientWidth : MaxWidth, UnitHeight);
                        dr.SetAttribute("i", Convert.ToString(DataRowhandle));

                        dr.OnClick = OnRowClick;
                        if(Settings.IsChrome)
                        {
                            dr.OnDblClick = OnDoubleClick;
                        }

                        for(int x = RawLeftCellIndex; x < RawLeftCellCount; x++)
                        {
                            var col = Columns[x];
                            if(!col.Visible)
                                continue;

                            var apparence = col.BodyApparence;
                            bool useDefault = false;
                            HTMLElement cell;
                            if(col.CellDisplay == null || (useDefault = col.CellDisplay.UseDefaultElement))
                            {
                                var displayValue = col.GetDisplayValueByDataRowHandle(DataRowhandle);
                                cell = Label(displayValue,
                                col.CachedX, 0, _columnAutoWidth ? _columnAutoWidthSingle : col.Width, apparence.IsBold, false, cellClass, apparence.Alignment, apparence.Forecolor);
                                
                                var newCell = useDefault ?
                                    col.CellDisplay.OnCreateDefault(cell, this, DataRowhandle, x) :
                                    cell;
                                
                                if(_highlighSearchResults && DataSource._searchActive && !useDefault 
                                    && !string.IsNullOrWhiteSpace(displayValue) 
                                    && displayValue.ToLower().StartsWith(DataSource.SearchString))
                                {
                                    newCell.Empty();
                                    var markelement = Document.CreateElement("mark");
                                    int Slength = DataSource.SearchString.Length;
                                    markelement.TextContent = displayValue.Substring(0, Slength);
                                    newCell.AppendChildren(markelement, Document.CreateTextNode(displayValue.Substring(Slength)));
                                }

                                dr.AppendChild(newCell);
                            }
                            else
                            {
                                cell = col.CellDisplay.OnCreate(this, DataRowhandle, x);
                                cell.SetLocation(col.CachedX, 0);
                                cell.Style.Width = (_columnAutoWidth ? _columnAutoWidthSingle : col.Width).ToPx();

                                dr.AppendChild(cell);
                            }
                            cell.SetAttribute("i", x.ToString());
                            cell.OnMouseDown = OnCellRowMouseDown;
                        }

                        if(AllowRowDrag)
                        {
                            dr.SetAttribute("draggable", "true");

                            dr.OnDragStart = OnRowDragStart;
                        }

                        rowFragment.AppendChild(dr);

                        CacheRow[i] = dr;   
                    }

                    if(StartedWith != RenderTime)
                    {
                        if(prevRowCache == 0)
                            ClearGrid();

                        GridBody.AppendChild(rowFragment);

                        return;
                    }

                    Y += UnitHeight;
                }
                if(prevRowCache == 0)
                    ClearGrid();

                if(OnCustomRowStyle != null && rowFragment.ChildNodes != null)
                {
                    var count = rowFragment.ChildNodes.Length;
                    for(int i = 0; i < count; i++)
                    {
                        if(StartedWith != RenderTime)
                        {
                            GridBody.AppendChild(rowFragment);

                            return;
                        }

                        try
                        {
                            var child = (HTMLElement)rowFragment.ChildNodes[i];
                            OnCustomRowStyle(child, Global.ParseInt(child.GetAttribute("i")));
                        }
                        catch(Exception ex)
                        {
                            if(Application.AplicationDefition == ApplicationDefitnion.ExpressCraftConsole)
                                ConsoleForm.Log(ex.ToString(), ConsoleLogType.Error);
                        }
                    }
                }

                GridHeader.AppendChild(colFragment);
                GridBody.AppendChild(rowFragment);

                if(StartedWith != RenderTime)
                {
                    return;
                }

                RenderTime = -1;
            };

            GridHeaderContainer = Div("heading-container");

            GridHeader = Div();
            GridHeader.SetBounds(0, 0, 0, "29px");
            GridBodyContainer = Div();

            GridBodyContainer.Style.OverflowX = Overflow.Auto;
            GridBodyContainer.Style.OverflowY = Overflow.Auto;

            GridHeaderContainer.Style.Overflow = Overflow.Hidden;

            GridBody = Div();
            GridBody.SetBounds(0, 0, 0, 0);

            GridBodyContainer.AppendChild(GridBody);
            GridHeaderContainer.AppendChild(GridHeader);

            GridFindPanel = Div("heading-container");
            GridFindPanel.Style.Visibility = Visibility.Hidden;
            GridFindPanel.SetBounds(0, 0, "100%", 46);

            SearchTextInput = new TextInput() { OnTextChanged = (sender) => {
                if(_searchTimer > -1)
                {
                    Global.ClearTimeout(_searchTimer);
                }
                if(string.IsNullOrWhiteSpace(SearchTextInput.Text))
                    _search();
                else
                    _searchTimer = Global.SetTimeout(_search, 500);
            }, OnKeyDown = (sender, ev) => {
                if(ev.KeyCode == KeyCodes.Enter)
                {
                    btnFind.Content.Click();
                }
            } };
            SearchTextInput.Bounds = new Vector4(30, 13, 350, 22);
            SearchTextInput.SetAttribute("placeholder", "Enter text to search...");

            btnFind = new SimpleButton() { Text = "Find", ItemClick = (sender) => {
                if(_searchTimer > -1)
                {
                    Global.ClearTimeout(_searchTimer);
                }
                _search();
            }, Bounds = new Vector4(385, 13, 60, 22) };
            btnClear = new SimpleButton() { Text = "Clear", ItemClick = (sender) => {
                if(_searchTimer > -1)
                {
                    Global.ClearTimeout(_searchTimer);
                }
                SearchTextInput.Text = string.Empty;
            }, Bounds = new Vector4(449, 13, 60, 22) };

            btnClose = new SimpleButton() { Bounds = new Vector4(7, 15, 18, 18), ItemClick = (sender) => {
                btnClear.Content.Click();
                CloseFindPanel();
            } };
            btnClose.Content.InnerHTML = "&times;";


            GridFindPanel.AppendChildren(btnClose, SearchTextInput, btnFind, btnClear);

            SetDefaultSizes();

            Content.OnMouseUp = (ev) =>
            {
                if(ResizeIndex == -1)
                    return;
                int x = Script.Write<int>("ev.pageX");
                x = Columns[ResizeIndex].Width + (x - ResizePageX);
                if(x < 24)
                    x = 24;
                Columns[ResizeIndex].Width = x;

                Form.SetCursor(Cursor.Default);

                ev.PreventDefault();
                ev.StopImmediatePropagation();
                ev.StopPropagation();

                ResizeIndex = -1;
                ResizeSpan = null;
            };

            OnResize = (ev) =>
            {
                CacheRow = new Dictionary<int, HTMLDivElement>();
                DelayedRenderGrid();
            };
            int prevleft = 0;
            GridBodyContainer.OnScroll = (ev) =>
            {
                if(prevleft != GridBodyContainer.ScrollLeft)
                {
                    CacheRow = new Dictionary<int, HTMLDivElement>();
                    prevleft = GridBodyContainer.ScrollLeft;
                    DelayedRenderGrid();
                }else
                {
                    DelayedRenderGrid(true);
                }
                
                
            };
            OnLoaded = (ev) =>
            {
                RenderGrid();
            };
            OnCellRowMouseDown = (ev) =>
            {
                FocusedColumn = Global.ParseInt(ev.CurrentTarget.As<HTMLElement>().GetAttribute("i"));
            };
            OnRowClick = (ev) =>
            {
                if(!Settings.IsChrome)
                {
                    if(clickTimeDiff == null)
                    {
                        clickTimeDiff = Stopwatch.StartNew();
                    }
                    else
                    {
                        clickTimeDiff.Stop();
                        var ems = clickTimeDiff.ElapsedMilliseconds;
                        clickTimeDiff = null;

                        if(ems < 200)
                        {
                            OnDoubleClick(ev);
                        }
                    }
                }

                var DataRowHandle = Global.ParseInt(ev.CurrentTarget.GetAttribute("i"));

                var mev = ev.As<MouseEvent>();
                if(AllowMultiSelection)
                {
                    if(mev.CtrlKey)
                    {
                        SelectedRows.AddOrSet(true, DataRowHandle, true);
                        RenderGrid();
                        return;
                    }
                    else if(mev.ShiftKey && FocusedDataHandle > -1)
                    {                        
                        _disableRender = true;
                        SelectedRows.ClearAll();
                        if(DataRowHandle < FocusedDataHandle)
                        {
                            for(int i = DataRowHandle; i < FocusedDataHandle + 1; i++)
                            {
                                SelectedRows.AddOrSet(true, i, true);
                            }
                        }else
                        {
                            for(int i = FocusedDataHandle; i < DataRowHandle + 1; i++)
                            {
                                SelectedRows.AddOrSet(true, i, true);
                            }
                        }
                        _disableRender = false;
                        RenderGrid();
                        return;
                    }
                }
                SelectedRows.ClearAndAddOrSet(true, DataRowHandle, true);
                if(DataRowHandle != _focusedDataHandle)
                {
                    FocusedDataHandle = DataRowHandle;
                }
                else
                {
                    RenderGrid();
                }
            };
            Content.TabIndex = 0;
            OnDoubleClick = (ev) =>
            {
                int drh = Global.ParseInt(ev.CurrentTarget.GetAttribute("i"));
                if(OnRowDoubleClick != null)
                    OnRowDoubleClick(drh);

                if(_useEditForm)
                {
                    var idr = DataSource[drh];

                    var fdre = new DataRowEditForm(idr, this, true);
                    fdre.ShowDialog();
                }
            };

            Content.OnKeyDown = (ev) =>
            {
                var kev = ev.As<KeyboardEvent>();
                //Global.Alert("CONTROL + A");
                if(AllowMultiSelection && kev.CtrlKey && (kev.KeyCode == 65 || kev.KeyCode == 97))
                {
                    // keyCode == 65 || keyCode == 97
                    //Global.Alert("AllowMultiSelection = TRUE");
                    SelectAllRows();
                }
                else
                {
                    if(kev.KeyCode == KeyCodes.Up || kev.KeyCode == KeyCodes.Down)
                    {
                        _disableRender = true;
                        var prevFocused = FocusedDataHandle;
                        if(kev.KeyCode == KeyCodes.Up)
                        {
                            if(!(FocusedDataHandle - 1 < 0))
                                FocusedDataHandle--;
                        }
                        else if(kev.KeyCode == KeyCodes.Down)
                        {
                            if(!(FocusedDataHandle > RowCount()))
                                FocusedDataHandle++;
                        }
                        if(prevFocused != FocusedDataHandle)
                        {
                            if(kev.ShiftKey)
                            {
                                SelectedRows.AddOrSet(true, FocusedDataHandle, true);
                            }
                            else
                            {
                                SelectedRows.ClearAndAddOrSet(true, FocusedDataHandle, true);
                            }
                            
                            MakeRowVisible(FocusedDataHandle);

                            _disableRender = false;

                            RenderGrid();
                        }else
                        {
                            _disableRender = false;
                        }                        
                    }
                                 
                    //Global.Alert("AllowMultiSelection = FALSE");
                }
            };

            ContextMenu = new ContextMenu();

            ContextMenu.ContextItems.AddRange(new ContextItem[] {
                new ContextItem("Sort Ascending", (cm) => {
                    if(FocusedColumn > -1)
                    {
                        SortColumn(Columns[FocusedColumn], GridViewSortMode.Asc);
                    }
                }),
                new ContextItem("Sort Descending", (cm) => {
                    if(FocusedColumn > -1)
                    {
                        SortColumn(Columns[FocusedColumn], GridViewSortMode.Desc);
                    }
                }),
                new ContextItem("Clear All Sorting", (cm) => {
                    ClearSortColumn();
                },  true),
                //new ContextItem("Group By This Column"),
                //new ContextItem("Hide Group By Box", true),
                new ContextItem("Hide This Column", (ci) => {
                    if(FocusedColumn > -1)
                    {
                        Columns[FocusedColumn].Visible = false;
                        RenderGrid();
                    }
                }),
                new ContextItem("View Columns"),
                new ContextItem("Save Column Layout"),
                new ContextItem("Best Fit", (ci) => {
                    if(FocusedColumn > -1)
                    {
                        Columns[FocusedColumn].Width = GetBestFitForColumn(Columns[FocusedColumn]);                        
                    }
                }) ,
                new ContextItem("Best Fit (all columns)", (ci) => {
                    BestFitAllColumns();
                }, true),
                new ContextItem("Filter Editor...", true),
                _showFindPanelContextItem = new ContextItem("Show Find Panel") {
                    OnItemClick = (sender) => {
                        if(FindPanelVisible)
                        {                            
                            FindPanelVisible = false;
                        }else
                        {                            
                            FindPanelVisible = true;
                        }
                    }
                },                
                new ContextItem("Select All", (cm) => { SelectAllRows(); }),
                new ContextItem("Unselect All", (cm) => { ClearSelection(); })
            });

            Content.OnContextMenu = (ev) =>
            {
                if(Helper.NotDesktop)
                {
                    ev.PreventDefault();
                    ev.StopPropagation();

                    OnDoubleClick(ev.As<MouseEvent<HTMLDivElement>>());
                }
                else
                {
                    if(ContextMenu != null)
                    {
                        ContextMenu.Show(Helper.GetClientMouseLocation(ev));
                        ev.PreventDefault();
                        ev.StopPropagation();
                    }
                }
            };

            OnColumnOnClick = (ev) =>
            {
                if(ResizeIndex >= 0)
                    return;

                var gcol = Columns[Global.ParseInt(ev.CurrentTarget.GetAttribute("i"))];

                for(int i = 0; i < ColumnCount(); i++)
                {
                    if(Columns[i] != gcol)
                    {
                        Columns[i].SortedMode = GridViewSortMode.None;
                    }
                }
                switch(gcol.SortedMode)
                {
                    default:
                    case GridViewSortMode.None:
                        SortColumn(gcol, GridViewSortMode.Asc);
                        break;

                    case GridViewSortMode.Asc:
                        SortColumn(gcol, GridViewSortMode.Desc);
                        break;

                    case GridViewSortMode.Desc:
                        SortColumn(gcol, GridViewSortMode.None);
                        break;
                }
            };
            OnColumnDragStart = (ev) =>
            {
                Script.Call("ev.dataTransfer.setData", "gridviewColumnDrag", ev.CurrentTarget.GetAttribute("i"));
            };
            OnColumnDragOver = (ev) =>
            {
                ev.PreventDefault();
            };
            OnColumnDrop = (ev) =>
            {
                if(ev.Target == null || !(ev.Target is HTMLSpanElement))
                    return;

                var target = ev.Target.As<HTMLSpanElement>();

                if(target.ParentElement != GridHeader)
                    return;

                var HoverIndex = Global.ParseInt(target.GetAttribute("i"));
                var SelectedIndex = Script.Write<int>("parseInt(ev.dataTransfer.getData(\"gridviewColumnDrag\"));");
                if(SelectedIndex == HoverIndex)
                    return;

                if(HoverIndex < 0)
                    return;

                int x = Script.Write<int>("ev.layerX");
                x -= target.ClientLeft;
                int w = target.ClientWidth / 2;

                if(HoverIndex == SelectedIndex - 1 && x > w)
                    return;
                if(HoverIndex == SelectedIndex + 1 && x < w)
                    return;

                if(x < w)
                {
                    DragIndex = HoverIndex;
                }
                else
                {
                    DragIndex = HoverIndex + 1;
                }

                if(DragIndex < 0 || SelectedIndex < 0)
                    return;
                var col = Columns[SelectedIndex];
                if(DragIndex == Columns.Count)
                {
                    Columns.Remove(col);
                    Columns.Add(col);
                }
                else
                {
                    var col1 = Columns[DragIndex];
                    Columns.Remove(col);
                    Columns.Insert(Columns.IndexOf(col1), col);
                }

                RenderGrid();
            };
            OnColumnMouseDown = (ev) =>
            {
                int x = Script.Write<int>("ev.layerX");
                var target = ev.Target.As<HTMLSpanElement>();
                x -= target.ClientLeft;
                ResizePageX = Script.Write<int>("ev.pageX");

                FocusedColumn = Global.ParseInt(ev.CurrentTarget.GetAttribute("i"));

                if(x >= target.ClientWidth - 2)
                {
                    ResizeIndex = Global.ParseInt(target.GetAttribute("i"));
                    ResizeSpan = target;
                    Form.SetCursor(Cursor.EastWestResize);

                    ev.PreventDefault();
                }
                else
                {
                    ResizeSpan = null;
                    ResizeIndex = -1;
                }
            };
            OnColumnMouseMove = (ev) =>
            {
                if(ResizeIndex == -1)
                {
                    int x = Script.Write<int>("ev.layerX");
                    var target = ev.Target.As<HTMLSpanElement>();
                    x -= target.ClientLeft;

                    if(x >= target.ClientWidth - 2)
                    {
                        Form.SetCursor(Cursor.EastWestResize);
                        return;
                    }
                    Form.SetCursor(Cursor.Default);
                }
            };

            OnColumnMouseLeave = (ev) =>
            {
                if(ResizeIndex == -1)
                {
                    Form.SetCursor(Cursor.Default);
                }
            };

            OnRowDragStart = (ev) =>
            {
                Script.Call("ev.dataTransfer.setData", "gridviewRowDrag", JSON.Stringify(DataSource[Global.ParseInt(ev.CurrentTarget.GetAttribute("i"))].GetOfflineDataRow()));
            };

            Content.AppendChildren(GridFindPanel, GridHeaderContainer, GridBodyContainer);

            FilterRowOnChange = (te) =>
            {
                Columns[Global.ParseInt(te.Content.GetAttribute("i"))].FilterValue = te.Text;
            };

            AutoGenerateColumnsFromSource = autoGenerateColumns;
            ColumnAutoWidth = columnAutoWidth;
        }

        private void DataSource_OnDataSourceChanged(object sender, EventArgs e)
        {
            SortColumn();
            RenderGrid();
        }

        public override void Render()
        {
            base.Render();
            HasRendered = true;
            RenderGrid();

            if(Content.ParentElement != null)
            {
            }
        }

        public float GetRawVisibleRowCount()
        {
            return GridBodyContainer.ClientHeight == 0 ? 0.0f : GridBodyContainer.ClientHeight / UnitHeight;
        }

        public float GetRawTopRowIndex()
        {
            return GridBodyContainer.ScrollTop == 0 ? 0.0f : GridBodyContainer.ScrollTop / PixelsPerRow(RowCount());
        }

        public void ValidateGridWidth()
        {
            var width = GetColumnWidths();
            GridBody.Style.Width = (width).ToPx();
            GridHeader.Style.Width = ((width) + 24).ToPx(); // (width).ToPx();
            if(RightOfTable == null)
            {
                RightOfTable = Div();
                GridBody.AppendChild(RightOfTable);
            }
            if(RightOfTableHeader == null)
            {
                RightOfTableHeader = Div();
                GridHeader.AppendChild(RightOfTableHeader);
            }
            RightOfTable.SetBounds(width - 1, 0, 1, 1);
            RightOfTableHeader.SetBounds(width - 1, 0, 1, 1);
        }

        public float PixelsPerRow(int rowCount)
        {
            if(rowCount > Settings.MaximumPixelScrollingRows)
            {
                return 3.0f;
            }
            else
            {
                return UnitHeight;
            }
        }

        public void ValidateGridHeight()
        {
            var i = RowCount();
            var ppr = PixelsPerRow(i);
            var height = ppr * i;

            if(i > Settings.MaximumPixelScrollingRows && GridBodyContainer.ClientHeight > 0)
            {
                height += ((GridBodyContainer.ClientHeight / UnitHeight) * ppr);
            }

            GridBody.Style.Height = height.ToPx();
            if(BottonOfTable == null)
            {
                BottonOfTable = Div();
                GridBody.AppendChild(BottonOfTable);
            }
            BottonOfTable.SetBounds(0, height, 1, 1);
        }

        public void ValidateGridSize()
        {
            ValidateGridHeight();
            ValidateGridWidth();
        }

        public void ClearHeader()
        {
            GridHeader.Empty();
            GridHeader.AppendChild(RightOfTableHeader);
        }

        public void ClearColumns()
        {
            Columns = new List<GridViewColumn>();
        }

        public void ClearView()
        {
            _disableRender = true;
            Columns = new List<GridViewColumn>();
            VisibleRowHandles = new List<int>();
            SelectedRows = new HardSoftList<bool>(false);
            _dataSource = null;
            _disableRender = false;
            RenderGrid();
        }

        public void ClearBody()
        {
            GridBody.Empty();
            GridBody.AppendChildren(RightOfTable, BottonOfTable);
        }

        public void ClearGrid()
        {
            ClearHeader();
            ClearBody();
        }

        public int[] GetSelectedRowHandles()
        {
            List<int> listOfInt = new List<int>();
            int rowCount = RowCount();        
            for(int i = 0; i < rowCount; i++)
            {                
                int index = GetDataSourceRow(i);
                if(SelectedRows.GetValue(index, false))
                {
                    listOfInt.Add(i);
                }
            }
            return listOfInt.ToArray();
        }

        public int[] GetSelectedDataRowHandles()
        {
            List<int> listOfInt = new List<int>();
            int rowCount = RowCount();
            for(int i = 0; i < rowCount; i++)
            {
                int index = GetDataSourceRow(i);
                if(SelectedRows.GetValue(index, false))
                {
                    listOfInt.Add(index);
                }
            }
            return listOfInt.ToArray();
        }

        private int DragIndex = -1;
        private int ResizeIndex = -1;
        private int ResizePageX = 0;
        private HTMLSpanElement ResizeSpan = null;

        private Action<MouseEvent<HTMLSpanElement>> OnColumnOnClick;
        private Action<Event<HTMLSpanElement>> OnColumnDragStart;
        private Action<Event<HTMLSpanElement>> OnColumnDragOver;
        private Action<Event<HTMLSpanElement>> OnColumnDrop;
        private Action<MouseEvent<HTMLSpanElement>> OnColumnMouseDown;
        private Action<MouseEvent<HTMLSpanElement>> OnColumnMouseMove;
        private Action<MouseEvent<HTMLSpanElement>> OnColumnMouseLeave;

        private Action<Event<HTMLDivElement>> OnRowDragStart;

        private void SetupColumn(HTMLSpanElement se, int index, GridViewColumn gcol)
        {
            se.SetAttribute("i", Convert.ToString(index));
            se.SetAttribute("draggable", "true");
            se.OnClick = OnColumnOnClick;
            se.OnDragStart = OnColumnDragStart;
            se.OnDragOver = OnColumnDragOver;
            se.OnDrop = OnColumnDrop;
            se.OnMouseDown = OnColumnMouseDown;
            se.OnMouseMove = OnColumnMouseMove;
            se.OnMouseLeave = OnColumnMouseLeave;
        }

        private int lastId = -1;

        private int PrevScroll = -1;

        private void ProcessBlur()
        {
            if(PrevScroll != GridBodyContainer.ScrollTop)
            {
                GridBody.ClassList.Add("blur");
                if(lastId != -1)
                {
                    Global.ClearTimeout(lastId);
                    lastId = -1;
                }

                lastId = Global.SetTimeout(() =>
                {
                    GridBody.ClassList.Remove("blur");
                }, 100);
            }
            PrevScroll = GridBodyContainer.ScrollTop;
        }

        private Action<TextInput> FilterRowOnChange;

        private int RenderTime = -1;
        private Action renderGridInternal;
        private bool _disableRender = false;
        public void RenderGrid(bool clear = true)
        {
            if(_disableRender)
                return;

            if(clear)
                CacheRow = new Dictionary<int, HTMLDivElement>();
            
            if(RenderTime > -1)
            {
                Global.ClearTimeout(RenderTime);
                RenderTime = Global.SetTimeout(renderGridInternal, 1);
            }
            else
            {
                renderGridInternal();
            }
        }
    }

    public class SortSetting
    {
        public GridViewColumn Column;
        public GridViewSortMode SortMode;
    }
}