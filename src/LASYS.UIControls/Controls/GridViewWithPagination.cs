namespace LASYS.UIControls.Controls
{
    public class HeaderColumn : Control
    {
        public int ColSpan { get; set; } = 1;
    }
    internal class MultiHeaderGridView : Control
    {
        private readonly VScrollBar _vScroll;
        private readonly HScrollBar _hScroll;

        private List<HeaderColumn> _topHeaders = new();
        private List<string> _subHeaders = new();
        private List<string[]> _rows = new();
        private int[] _columnWidths = Array.Empty<int>();
        private int[] _headerSegmentWidths = Array.Empty<int>();
        private int _totalContentWidth;
        private Font? _subHeaderFont;
        private Pen? _gridLinePen;
        private Pen? _headerDividerPen;
        private Pen? _accentPen;
        private SolidBrush? _headerBackBrush;
        private SolidBrush? _headerTextBrush;
        private SolidBrush? _rowBackBrush;
        private SolidBrush? _altRowBackBrush;
        private SolidBrush? _rowTextBrush;
        private SolidBrush? _accentBrush;

        public int RowHeight { get; set; } = 28;
        public int HeaderHeight { get; set; } = 32;

        public Color HeaderBackColor = Color.FromArgb(210, 230, 225);
        public Color HeaderForeColor = Color.FromArgb(0, 100, 90);
        public Color RowBackColor { get; set; } = Color.White;

        public Color AltRowBackColor = Color.FromArgb(245, 250, 248); // subtle green tint
        public Color GridLineColor = Color.FromArgb(210, 220, 218);

        public Font HeaderFont { get; set; } = new Font("Segoe UI", 9F, FontStyle.Bold);
        public Font RowFont { get; set; } = new Font("Segoe UI", 9F, FontStyle.Regular);


        private bool _isResizing = false;
        private int _resizeColumnIndex = -1;
        private int _resizeStartX;
        private int _resizeStartWidth;
        private const int ResizeThreshold = 4; // px from border to detect resize


        private int _selectedRowIndex = -1;
        private readonly List<object> _rowDataObjects = new(); // stores the "real" row objects

        public MultiHeaderGridView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            _vScroll = new VScrollBar { Dock = DockStyle.Right };
            _vScroll.Scroll += (s, e) => Invalidate();
            Controls.Add(_vScroll);

            _hScroll = new HScrollBar { Dock = DockStyle.Bottom };
            _hScroll.Scroll += (s, e) => Invalidate();
            Controls.Add(_hScroll);
        }

        public void SetMergedHeaders(HeaderColumn[] topHeaders, string[] subHeaders)
        {
            _topHeaders = [.. topHeaders];
            _subHeaders = [.. subHeaders];
            UpdateLayout();
        }

        private int[]? _customColumnWidths = null;

        public void SetColumnWidths(params int[] widths)
        {
            if (widths == null || widths.Length != _subHeaders.Count)
                throw new ArgumentException("Width array must match number of sub-columns.", nameof(widths));

            _customColumnWidths = (int[])widths.Clone();
            UpdateLayout();
        }


        public void SetRows<T>(IEnumerable<T> data, Func<T, string[]> displaySelector)
        {
            _rows.Clear();
            _rowDataObjects.Clear();

            if (data is ICollection<T> collection)
            {
                if (_rows.Capacity < collection.Count)
                    _rows.Capacity = collection.Count;

                if (_rowDataObjects.Capacity < collection.Count)
                    _rowDataObjects.Capacity = collection.Count;
            }

            if (data is IList<T> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    _rowDataObjects.Add(item!);
                    _rows.Add(displaySelector(item));
                }

                UpdateLayout();
                return;
            }

            foreach (var item in data)
            {
                _rowDataObjects.Add(item!);
                _rows.Add(displaySelector(item));
            }

            UpdateLayout();
        }




        private void UpdateLayout()
        {
            if (_customColumnWidths == null)
            {
                using var g = CreateGraphics();
                CalculateColumnWidths(g);
            }
            else
            {
                _columnWidths = (int[])_customColumnWidths.Clone();
            }

            CacheLayoutMetrics();
            UpdateScrollBars();
            Invalidate();
        }

        private void CalculateColumnWidths(Graphics g)
        {
            if (_customColumnWidths != null)
            {
                _columnWidths = (int[])_customColumnWidths.Clone();
                return; // use fixed widths directly
            }

            _columnWidths = new int[_subHeaders.Count];
            for (int col = 0; col < _subHeaders.Count; col++)
            {
                int maxWidth = (int)g.MeasureString(_subHeaders[col], HeaderFont).Width + 20;

                foreach (var row in _rows)
                {
                    if (col < row.Length)
                    {
                        int w = (int)g.MeasureString(row[col], RowFont).Width + 20;
                        if (w > maxWidth) maxWidth = w;
                    }
                }
                _columnWidths[col] = maxWidth;
            }
        }

        private void CacheLayoutMetrics()
        {
            _totalContentWidth = 0;
            for (int i = 0; i < _columnWidths.Length; i++)
            {
                _totalContentWidth += _columnWidths[i];
            }

            if (_topHeaders.Count == 0 || _columnWidths.Length == 0)
            {
                _headerSegmentWidths = Array.Empty<int>();
                return;
            }

            _headerSegmentWidths = new int[_topHeaders.Count];
            for (int topIndex = 0, subIndex = 0; topIndex < _topHeaders.Count; topIndex++)
            {
                var top = _topHeaders[topIndex];
                int width = 0;

                for (int i = 0; i < top.ColSpan && subIndex + i < _columnWidths.Length; i++)
                {
                    width += _columnWidths[subIndex + i];
                }

                _headerSegmentWidths[topIndex] = width;
                subIndex += top.ColSpan;
            }
        }

        private void UpdateScrollBars()
        {
            int headerTotal = HeaderHeight * 2;
            int rowsHeight = _rows.Count * RowHeight;

            int viewportRowsHeight = Math.Max(0, Height - _hScroll.Height - headerTotal);
            if (rowsHeight > viewportRowsHeight)
            {
                _vScroll.Enabled = true;
                _vScroll.Minimum = 0;
                _vScroll.LargeChange = Math.Max(1, viewportRowsHeight);
                _vScroll.SmallChange = Math.Max(1, RowHeight);
                _vScroll.Maximum = Math.Max(0, rowsHeight - 1); // effective end = Max - LargeChange + 1
                ClampV();
            }
            else
            {
                _vScroll.Enabled = false;
                _vScroll.Value = 0;
            }

            int totalContentWidth = _totalContentWidth;
            int viewportWidth = Math.Max(0, Width - _vScroll.Width);
            if (totalContentWidth > viewportWidth)
            {
                _hScroll.Enabled = true;
                _hScroll.Minimum = 0;
                _hScroll.LargeChange = Math.Max(1, viewportWidth);
                _hScroll.SmallChange = 20;
                _hScroll.Maximum = Math.Max(0, totalContentWidth - 1);
                ClampH();
            }
            else
            {
                _hScroll.Enabled = false;
                _hScroll.Value = 0;
            }
        }

        private void ClampV()
        {
            int end = Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1);
            if (_vScroll.Value > end) _vScroll.Value = end;
            if (_vScroll.Value < 0) _vScroll.Value = 0;
        }

        private void ClampH()
        {
            int end = Math.Max(0, _hScroll.Maximum - _hScroll.LargeChange + 1);
            if (_hScroll.Value > end) _hScroll.Value = end;
            if (_hScroll.Value < 0) _hScroll.Value = 0;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!_vScroll.Enabled) return;
            int deltaRows = -Math.Sign(e.Delta); // one row per wheel notch
            int target = _vScroll.Value + deltaRows * RowHeight;
            int end = Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1);
            _vScroll.Value = Math.Max(0, Math.Min(end, target));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);

            int headerTotal = HeaderHeight * 2;
            int xOffset = -_hScroll.Value;

            // 1) Draw headers (never scrolled vertically)
            DrawMergedHeaders(g, xOffset);

            // 2) Clip the data region so rows can NEVER paint over headers
            var dataViewport = new Rectangle(
                0,
                headerTotal,
                Math.Max(0, Width - _vScroll.Width),
                Math.Max(0, Height - _hScroll.Height - headerTotal)
            );
            if (dataViewport.Width <= 0 || dataViewport.Height <= 0) return;

            var oldClip = g.Clip; // save
            g.SetClip(dataViewport);

            // 3) Compute visible rows & starting Y
            int firstRowIndex = _vScroll.Enabled ? _vScroll.Value / RowHeight : 0;
            int rowOffset = _vScroll.Enabled ? _vScroll.Value % RowHeight : 0;
            int startY = headerTotal - rowOffset; // may be < headerTotal, but it's CLIPPED

            int rowsToDraw = dataViewport.Height / RowHeight + 2; // +2 for partials

            int y = startY;
            for (int i = 0; i < rowsToDraw; i++)
            {
                int rowIndex = firstRowIndex + i;
                if (rowIndex >= _rows.Count) break;

                DrawDataRow(g, _rows[rowIndex], rowIndex, y, xOffset, dataViewport.Width);
                y += RowHeight;
            }

            // Restore clip
            g.SetClip(oldClip, System.Drawing.Drawing2D.CombineMode.Replace);

            // 4) Right divider line
            using var gridPen = new Pen(GridLineColor);
            g.DrawLine(gridPen, Width - _vScroll.Width - 1, 0, Width - _vScroll.Width - 1, Height);
        }

        private void DrawMergedHeaders(Graphics g, int xOffset)
        {
            var gridPen = GetGridLinePen();
            var headerBrush = GetHeaderBackBrush();
            var headerTextBrush = GetHeaderTextBrush();
            var subHeaderFont = GetSubHeaderFont();
            var dividerPen = GetHeaderDividerPen();
            var accentPen = GetAccentPen();

            int x = xOffset;

            // Top merged header row
            for (int topIndex = 0, subIndex = 0; topIndex < _topHeaders.Count; topIndex++)
            {
                var top = _topHeaders[topIndex];
                int width = topIndex < _headerSegmentWidths.Length
                    ? _headerSegmentWidths[topIndex]
                    : GetSegmentWidth(subIndex, top.ColSpan);

                if (top.ColSpan == 1)
                {
                    // Single column → draw it spanning BOTH header rows
                    g.FillRectangle(headerBrush, x, 0, width, HeaderHeight * 2);
                    g.DrawRectangle(gridPen, x, 0, width, HeaderHeight * 2);
                    g.DrawString(top.Text, HeaderFont, headerTextBrush,
                        new RectangleF(x, 0, width, HeaderHeight * 2),
                        StringFormat.GenericTypographic);
                }
                else
                {
                    // Multi-column merged header
                    g.FillRectangle(headerBrush, x, 0, width, HeaderHeight);
                    g.DrawRectangle(gridPen, x, 0, width, HeaderHeight);
                    g.DrawString(top.Text, HeaderFont, headerTextBrush,
                        new RectangleF(x, 0, width, HeaderHeight),
                        StringFormat.GenericTypographic);

                    // Sub-headers row
                    int subX = x;
                    for (int i = 0; i < top.ColSpan; i++)
                    {
                        int colWidth = _columnWidths[subIndex + i];
                        if (subX + colWidth < 0)
                        {
                            subX += colWidth;
                            continue;
                        }

                        if (subX > Width - _vScroll.Width)
                            break;

                        g.FillRectangle(headerBrush, subX, HeaderHeight, colWidth, HeaderHeight);
                        g.DrawRectangle(gridPen, subX, HeaderHeight, colWidth, HeaderHeight);
                        g.DrawString(_subHeaders[subIndex + i], subHeaderFont, headerTextBrush,
                            new RectangleF(subX, HeaderHeight, colWidth, HeaderHeight),
                            StringFormat.GenericTypographic);

                        subX += colWidth;
                    }
                }

                x += width;
                g.DrawLine(dividerPen, x - 1, 0, x - 1, HeaderHeight * 2);
                subIndex += top.ColSpan;
            }
            g.DrawLine(accentPen, 0, HeaderHeight * 2 - 1, Width, HeaderHeight * 2 - 1);
        }

        private void DrawDataRow(Graphics g, string[] row, int rowIndex, int y, int xOffset, int viewportWidth)
        {
            int totalWidth = _totalContentWidth;
            var backColor = (rowIndex % 2 == 0) ? RowBackColor : AltRowBackColor;

            bool isSelected = rowIndex == _selectedRowIndex;
            if (isSelected)
            {
                backColor = Color.FromArgb(220, 245, 240); // soft green highlight
            }

            var backBrush = GetRowBackBrush(backColor);
            g.FillRectangle(backBrush, -_hScroll.Value, y, totalWidth, RowHeight);

            var textColor = isSelected
                ? Color.FromArgb(0, 80, 70) // darker green text when selected
                : ForeColor;

            // 2) Text and grid lines
            var f = GetRowTextBrush();
            var gridPen = GetGridLinePen();

            int x = xOffset;
            for (int c = 0; c < row.Length && c < _columnWidths.Length; c++)
            {
                int colWidth = _columnWidths[c];
                TextRenderer.DrawText(g, row[c], RowFont,
                    new Rectangle(x + 6, y + 4, colWidth - 12, RowHeight - 8),
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

                g.DrawRectangle(gridPen, x, y, colWidth, RowHeight);
                x += colWidth;
            }

            if (isSelected)
            {
                g.FillRectangle(GetAccentBrush(), -_hScroll.Value, y, 4, RowHeight);
            }

            

        }

        private int GetSegmentWidth(int startIndex, int colSpan)
        {
            int width = 0;
            for (int i = 0; i < colSpan && startIndex + i < _columnWidths.Length; i++)
            {
                width += _columnWidths[startIndex + i];
            }

            return width;
        }

        private Font GetSubHeaderFont()
        {
            _subHeaderFont ??= new Font("Segoe UI", 8F, FontStyle.Bold);
            return _subHeaderFont;
        }

        private Pen GetGridLinePen()
        {
            if (_gridLinePen == null || _gridLinePen.Color != GridLineColor)
            {
                _gridLinePen?.Dispose();
                _gridLinePen = new Pen(GridLineColor);
            }

            return _gridLinePen;
        }

        private Pen GetHeaderDividerPen()
        {
            var color = Color.FromArgb(180, 200, 195);
            if (_headerDividerPen == null || _headerDividerPen.Color != color)
            {
                _headerDividerPen?.Dispose();
                _headerDividerPen = new Pen(color, 2);
            }

            return _headerDividerPen;
        }

        private Pen GetAccentPen()
        {
            var color = Color.FromArgb(0, 166, 147);
            if (_accentPen == null || _accentPen.Color != color)
            {
                _accentPen?.Dispose();
                _accentPen = new Pen(color, 2);
            }

            return _accentPen;
        }

        private SolidBrush GetHeaderBackBrush()
        {
            if (_headerBackBrush == null || _headerBackBrush.Color != HeaderBackColor)
            {
                _headerBackBrush?.Dispose();
                _headerBackBrush = new SolidBrush(HeaderBackColor);
            }

            return _headerBackBrush;
        }

        private SolidBrush GetHeaderTextBrush()
        {
            if (_headerTextBrush == null || _headerTextBrush.Color != HeaderForeColor)
            {
                _headerTextBrush?.Dispose();
                _headerTextBrush = new SolidBrush(HeaderForeColor);
            }

            return _headerTextBrush;
        }

        private SolidBrush GetRowBackBrush(Color color)
        {
            if (color == RowBackColor)
            {
                if (_rowBackBrush == null || _rowBackBrush.Color != color)
                {
                    _rowBackBrush?.Dispose();
                    _rowBackBrush = new SolidBrush(color);
                }

                return _rowBackBrush;
            }

            if (_altRowBackBrush == null || _altRowBackBrush.Color != color)
            {
                _altRowBackBrush?.Dispose();
                _altRowBackBrush = new SolidBrush(color);
            }

            return _altRowBackBrush;
        }

        private SolidBrush GetRowTextBrush()
        {
            if (_rowTextBrush == null || _rowTextBrush.Color != ForeColor)
            {
                _rowTextBrush?.Dispose();
                _rowTextBrush = new SolidBrush(ForeColor);
            }

            return _rowTextBrush;
        }

        private SolidBrush GetAccentBrush()
        {
            var color = Color.FromArgb(0, 166, 147);
            if (_accentBrush == null || _accentBrush.Color != color)
            {
                _accentBrush?.Dispose();
                _accentBrush = new SolidBrush(color);
            }

            return _accentBrush;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subHeaderFont?.Dispose();
                _gridLinePen?.Dispose();
                _headerDividerPen?.Dispose();
                _accentPen?.Dispose();
                _headerBackBrush?.Dispose();
                _headerTextBrush?.Dispose();
                _rowBackBrush?.Dispose();
                _altRowBackBrush?.Dispose();
                _rowTextBrush?.Dispose();
                _accentBrush?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isResizing)
            {
                int delta = e.X - _resizeStartX;
                int newWidth = Math.Max(20, _resizeStartWidth + delta);
                _columnWidths[_resizeColumnIndex] = newWidth;
                _customColumnWidths = (int[])_columnWidths.Clone(); // store custom widths
                CacheLayoutMetrics();
                UpdateScrollBars();
                Invalidate();
                return;
            }

            // Detect if we're near a column edge in the subheader row
            int xOffset = -_hScroll.Value;
            int x = xOffset;
            for (int i = 0; i < _columnWidths.Length; i++)
            {
                x += _columnWidths[i];
                if (Math.Abs(e.X - x) <= ResizeThreshold && e.Y >= HeaderHeight && e.Y <= HeaderHeight * 2)
                {
                    Cursor = Cursors.VSplit;
                    return;
                }
            }
            Cursor = Cursors.Default;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && Cursor == Cursors.VSplit)
            {
                int xOffset = -_hScroll.Value;
                int x = xOffset;
                for (int i = 0; i < _columnWidths.Length; i++)
                {
                    x += _columnWidths[i];
                    if (Math.Abs(e.X - x) <= ResizeThreshold && e.Y >= HeaderHeight && e.Y <= HeaderHeight * 2)
                    {
                        _isResizing = true;
                        _resizeColumnIndex = i;
                        _resizeStartX = e.X;
                        _resizeStartWidth = _columnWidths[i];
                        Capture = true;
                        return;
                    }
                }
            }

            int headerTotalHeight = HeaderHeight * 2;
            if (e.Y >= headerTotalHeight)
            {
                int firstRowIndex = _vScroll.Enabled ? _vScroll.Value / RowHeight : 0;
                int rowOffset = _vScroll.Enabled ? _vScroll.Value % RowHeight : 0;
                int relativeY = e.Y - headerTotalHeight + rowOffset;
                int rowIndex = firstRowIndex + (relativeY / RowHeight);

                if (rowIndex >= 0 && rowIndex < _rows.Count)
                {
                    _selectedRowIndex = rowIndex;
                    Invalidate();
                }
                else
                {
                    ClearSelection();

                }
            }
        }

        public void ClearSelection()
        {
            _selectedRowIndex = -1;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isResizing)
            {
                _isResizing = false;
                _resizeColumnIndex = -1;
                Capture = false;
            }
        }

        public object? GetSelectedRowData()
        {
            if (_selectedRowIndex >= 0 && _selectedRowIndex < _rowDataObjects.Count)
                return _rowDataObjects[_selectedRowIndex];
            return null;
        }

        public event EventHandler<object?>? RowDoubleClicked;        

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            int headerTotalHeight = HeaderHeight * 2;
            int firstRowIndex = _vScroll.Enabled ? _vScroll.Value / RowHeight : 0;
            int rowOffset = _vScroll.Enabled ? _vScroll.Value % RowHeight : 0;
            int relativeY = e.Y - headerTotalHeight + rowOffset;
            int rowIndex = firstRowIndex + (relativeY / RowHeight);

            // Only trigger if double-click is inside a real row
            if (rowIndex >= 0 && rowIndex < _rows.Count)
            {
                var obj = GetSelectedRowData();
                if (obj != null)
                {
                    RowDoubleClicked?.Invoke(this, obj);
                }
            }
        }
    }
    internal class IconButton : Control
    {
        private readonly PictureBox _icon;
        private readonly Label _textLabel;

        public event EventHandler? Clicked;

        public Image? Icon
        {
            get => _icon.Image;
            set => _icon.Image = value;
        }

        public string ButtonText
        {
            get => _textLabel.Text;
            set => _textLabel.Text = value;
        }

        public Color HoverBackColor { get; set; } = Color.FromArgb(200, 230, 225); //Color.FromArgb(50, 50, 50);
        public new Color DefaultBackColor { get; set; } = Color.White; //Color.FromArgb(45, 45, 48);
        public Color TextColor { get; set; } = Color.FromArgb(0, 110, 100);


        public IconButton()
        {
            Height = 30;
            Width = 80;
            Cursor = Cursors.Hand;
            BackColor = DefaultBackColor;

            _icon = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(20, 20),
                Location = new Point(5, 5),
                BackColor = Color.Transparent
            };

            _textLabel = new Label
            {
                AutoSize = false,
                Location = new Point(30, 0),
                Width = Width - 35,
                Height = Height,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = TextColor,
                BackColor = Color.Transparent
            };


            Controls.Add(_icon);
            Controls.Add(_textLabel);

            // Mouse events
            //MouseEnter += (s, e) => BackColor = HoverBackColor;
            //MouseLeave += (s, e) => BackColor = DefaultBackColor;
            MouseEnter += (s, e) =>
            {
                if (Enabled)
                    BackColor = HoverBackColor;
            };

            MouseLeave += (s, e) =>
            {
                if (Enabled)
                    BackColor = DefaultBackColor;
            };

            MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);

            // Propagate events to children
            _icon.MouseEnter += (s, e) => BackColor = HoverBackColor;
            _icon.MouseLeave += (s, e) => BackColor = DefaultBackColor;
            _icon.MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);

            _textLabel.MouseEnter += (s, e) => BackColor = HoverBackColor;
            _textLabel.MouseLeave += (s, e) => BackColor = DefaultBackColor;
            _textLabel.MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);

            this.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(180, 200, 195));
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };
        }
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            BackColor = Enabled ? DefaultBackColor : Color.FromArgb(235, 235, 235);
            ForeColor = Enabled ? Color.FromArgb(0, 110, 100) : Color.Gray;

            Invalidate();
        }
    }
    internal class PlaceholderTextBox : TextBox
    {
        public new string PlaceholderText { get; set; } = "Search…";
        public Color PlaceholderColor { get; set; } = Color.Gray;
        public int PlaceholderPaddingLeft { get; set; } = 0;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_PAINT = 0x000F;
            if (m.Msg == WM_PAINT && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(PlaceholderText))
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    var rect = ClientRectangle;

                    // push right a bit so it doesn’t hug the border
                    rect.Offset(PlaceholderPaddingLeft, 0);
                    rect.Inflate(-PlaceholderPaddingLeft, 0);

                    var flags = TextFormatFlags.Left |
                                TextFormatFlags.EndEllipsis |
                                TextFormatFlags.VerticalCenter;

                    TextRenderer.DrawText(g, PlaceholderText, Font, rect, PlaceholderColor, flags);
                }
            }
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Invalidate(); }

    }
    public class GridViewWithPagination : Control
    {
        private readonly MultiHeaderGridView _grid;
        private readonly Panel _paginationPanel;
        private readonly IconButton _btnFirst, _btnPrev, _btnNext, _btnLast;
        private readonly Label _lblPage;
        private readonly Panel _searchPanel;
        private readonly PlaceholderTextBox _txtSearch;

        private bool _isExternalData = false;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private List<object> _allRows = new(); // all row objects
        private List<object> _filteredRows = new(); // filtered by search
        private int _rowsVersion;
        private int _lastRenderedRowsVersion = -1;
        private int _lastRenderedPage = -1;
        private int _lastRenderedPageSize = -1;
        private int _lastRenderedTotalPages = -1;
        private bool _lastRenderedExternalData;

        private Func<object, string[]> _displaySelector = null!;
        private readonly global::System.Windows.Forms.Timer _searchDebounceTimer;
        private string _pendingSearchTerm = string.Empty;

        private void EnableDoubleBuffer(Control c)
        {
            c.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(c, true, null);
        }

        public GridViewWithPagination()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            _searchDebounceTimer = new global::System.Windows.Forms.Timer { Interval = 1000 };
            _searchDebounceTimer.Tick += (_, _) =>
            {
                _searchDebounceTimer.Stop();
                ApplySearchTerm(_pendingSearchTerm);
            };


            SuspendLayout();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            EnableDoubleBuffer(layout);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // search
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // grid
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // pagination

            _searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(230, 240, 238), // Color.FromArgb(60, 60, 60),
                Padding = new Padding(5),
                Margin = new Padding(0)
            };
            EnableDoubleBuffer(_searchPanel);

            _txtSearch = new PlaceholderTextBox
            {
                PlaceholderText = "Search...",
                PlaceholderColor = Color.Gray, //Color.FromArgb(160, 160, 160),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Black, //Color.White,
                BackColor = Color.White //Color.FromArgb(80, 80, 80),
            };
            EnableDoubleBuffer(_txtSearch);

            _txtSearch.TextChanged += (sender, e) =>
            {
                _pendingSearchTerm = _txtSearch.Text.Trim();
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            _searchPanel.Controls.Add(_txtSearch);


            // Initialize grid
            _grid = new MultiHeaderGridView
            {
                Dock = DockStyle.Fill
            };
            EnableDoubleBuffer(_grid);

            // Pagination panel
            _paginationPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                BackColor = Color.FromArgb(230, 240, 238) //Color.FromArgb(45, 45, 48)
            };
            EnableDoubleBuffer(_paginationPanel);

            // Container for buttons (TableLayoutPanel)
            var buttonContainer = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 5,
                RowCount = 1,
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Top,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            EnableDoubleBuffer(buttonContainer);

            // Define column styles (auto-size each)
            buttonContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // First
            buttonContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Prev
            buttonContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Label
            buttonContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Next
            buttonContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Last

            // Initialize page label
            _lblPage = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Page 1 of 1",
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 140, 125), //Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                Padding = new Padding(10, 8, 10, 8),
            };
            EnableDoubleBuffer(_lblPage);

            // Initialize buttons
            _btnFirst = CreateButton("First", @"Resources/first.png");
            _btnPrev = CreateButton("Prev", @"Resources/previous.png");
            _btnNext = CreateButton("Next", @"Resources/next.png");
            _btnLast = CreateButton("Last", @"Resources/last.png");

            EnableDoubleBuffer(_btnFirst);
            EnableDoubleBuffer(_btnPrev);
            EnableDoubleBuffer(_btnNext);
            EnableDoubleBuffer(_btnLast);

            // Center content inside cells
            _btnFirst.Anchor = AnchorStyles.None;
            _btnPrev.Anchor = AnchorStyles.None;
            _btnNext.Anchor = AnchorStyles.None;
            _btnLast.Anchor = AnchorStyles.None;

            // Add controls to table (column, row)
            buttonContainer.Controls.Add(_btnFirst, 0, 0);
            buttonContainer.Controls.Add(_btnPrev, 1, 0);
            buttonContainer.Controls.Add(_lblPage, 2, 0);
            buttonContainer.Controls.Add(_btnNext, 3, 0);
            buttonContainer.Controls.Add(_btnLast, 4, 0);

            // Center the whole container inside pagination panel
            buttonContainer.Location = new Point(
                (_paginationPanel.Width - buttonContainer.PreferredSize.Width) / 2,
                5
            );

            // Add container to panel
            _paginationPanel.Controls.Add(buttonContainer);

            // Layout
            layout.Controls.Add(_searchPanel, 0, 0);
            layout.Controls.Add(_grid, 0, 1);
            layout.Controls.Add(_paginationPanel, 0, 2);

            Controls.Add(layout);

            ResumeLayout(true);

            _paginationPanel.Resize += (s, e) =>
            {
                buttonContainer.Location = new Point(
                    (_paginationPanel.Width - buttonContainer.Width) / 2,
                    (_paginationPanel.Height - buttonContainer.Height) / 2
                );

                _lblPage.Location = new Point(
                      (_paginationPanel.Width - _lblPage.Width) / 2,
                      (_paginationPanel.Height - _lblPage.Height) / 2
                  );
            };

            // Wire button events
            _btnFirst.Clicked += (s, e) =>
            {
                _currentPage = 1;
                PageNoChanged?.Invoke(this, 1);
                if (!_isExternalData) UpdatePage();
            };

            _btnPrev.Clicked += (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    PageNoChanged?.Invoke(this, _currentPage);
                }

                if (!_isExternalData) UpdatePage();
            };

            _btnNext.Clicked += (s, e) =>
            {
                if (_currentPage < TotalPages)
                {
                    _currentPage++;
                    PageNoChanged?.Invoke(this, _currentPage);
                }

                if (!_isExternalData) UpdatePage();
            };

            _btnLast.Clicked += (s, e) =>
            {
                _currentPage = TotalPages;
                PageNoChanged?.Invoke(this, TotalPages);
                if (!_isExternalData) UpdatePage();
            };
        }


        private IconButton CreateButton(string text, string imagePath)
        {
            Color LightenColor(Color color, int amount = 20)
            {
                int r = Math.Min(255, color.R + amount);
                int g = Math.Min(255, color.G + amount);
                int b = Math.Min(255, color.B + amount);
                return Color.FromArgb(r, g, b);
            }

            return new IconButton
            {
                ButtonText = text,
                Width = 80,
                Height = 30,
                DefaultBackColor = Color.White, //_paginationPanel.BackColor,
                HoverBackColor = Color.FromArgb(200, 230, 225), //LightenColor(_paginationPanel.BackColor, 20),
                TextColor = Color.FromArgb(0, 110, 100),
                Icon = Image.FromFile(imagePath) // Load icon from file
            };
        }

        public int PageSize
        {
            get => _pageSize;
            set { _pageSize = Math.Max(1, value); UpdatePage(); }
        }

        public int CurrentPage => _currentPage;

        public int TotalPages;
        public void SetRows<T>(IEnumerable<T> rows, Func<T, string[]> displaySelector)
        {
            _rowsVersion++;
            if (rows is ICollection<T> collection)
            {
                _allRows = new List<object>(collection.Count);
                _filteredRows = new List<object>(collection.Count);
            }
            else
            {
                _allRows = new List<object>();
                _filteredRows = new List<object>();
            }

            if (rows is IList<T> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    _allRows.Add(item!);
                    _filteredRows.Add(item!);
                }
            }
            else
            {
                foreach (var item in rows)
                {
                    _allRows.Add(item!);
                    _filteredRows.Add(item!);
                }
            }

            _displaySelector = obj => displaySelector((T)obj);

            if (_isExternalData)
            {
                _currentPage = _currentPage < 1 ? 1 : _currentPage;
            }
            else
            {
                TotalPages = (_filteredRows.Count + _pageSize - 1) / _pageSize;
                _currentPage = 1;
            }
            UpdatePage();
        }
        // Expose SetMergedHeaders
        public void SetMergedHeaders(HeaderColumn[] topHeaders, string[] subHeaders)
        {
            _grid.SetMergedHeaders(topHeaders, subHeaders);
        }

        // Expose SetColumnWidths
        public void SetColumnWidths(params int[] widths)
        {
            _grid.SetColumnWidths(widths);
        }

        public void SetTotalPages(int totalPages)
        {
            TotalPages = totalPages;
            _lblPage.Text = $"Page {_currentPage} of {totalPages}";
            _btnNext.Enabled = _btnLast.Enabled = _currentPage < totalPages;
        }

        public event EventHandler<object?>? RowDoubleClicked
        {
            add => _grid.RowDoubleClicked += value;
            remove => _grid.RowDoubleClicked -= value;
        }

        public event EventHandler<int>? PageNoChanged;
        public event EventHandler<string>? SearchTermChanged;

        private void UpdatePage()
        {
            int currentRowsVersion = _rowsVersion;
            int currentPage = _currentPage;
            int pageSize = _pageSize;
            int filteredCount = _filteredRows.Count;
            int totalPages = filteredCount == 0 ? 0 : (filteredCount + pageSize - 1) / pageSize;

            if (_lastRenderedRowsVersion == currentRowsVersion &&
                _lastRenderedPage == currentPage &&
                _lastRenderedPageSize == pageSize &&
                _lastRenderedTotalPages == totalPages &&
                _lastRenderedExternalData == _isExternalData)
            {
                return;
            }

            if (_isExternalData)
            {
                _grid.SetRows(_filteredRows, _displaySelector);
                _grid.ClearSelection();
                _btnFirst.Enabled = _btnPrev.Enabled = _currentPage > 1;
                _lastRenderedRowsVersion = currentRowsVersion;
                _lastRenderedPage = currentPage;
                _lastRenderedPageSize = pageSize;
                _lastRenderedTotalPages = TotalPages;
                _lastRenderedExternalData = true;
                return;
            }

            if (filteredCount == 0)
            {
                _grid.SetRows(Array.Empty<object>(), o => Array.Empty<string>());
                _lblPage.Text = "Page 0 of 0";
                _lastRenderedRowsVersion = currentRowsVersion;
                _lastRenderedPage = currentPage;
                _lastRenderedPageSize = pageSize;
                _lastRenderedTotalPages = 0;
                _lastRenderedExternalData = false;
                return;
            }

            var pageRows = new List<object>(Math.Min(pageSize, filteredCount));
            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, filteredCount);
            for (int i = startIndex; i < endIndex; i++)
            {
                pageRows.Add(_filteredRows[i]);
            }

            _grid.SetRows(pageRows, _displaySelector);
            _grid.ClearSelection();

            _lblPage.Text = $"Page {currentPage} of {totalPages}";

            // Enable/disable buttons
            _btnFirst.Enabled = _btnPrev.Enabled = currentPage > 1;
            _btnNext.Enabled = _btnLast.Enabled = currentPage < totalPages;

            _lastRenderedRowsVersion = currentRowsVersion;
            _lastRenderedPage = currentPage;
            _lastRenderedPageSize = pageSize;
            _lastRenderedTotalPages = totalPages;
            _lastRenderedExternalData = false;

        }

        public void SetExternalDataMode(bool isExternal)
        {
            _isExternalData = isExternal;
        }

        private void ApplySearchTerm(string searchTerm)
        {
            // skip filtering if data is external (handled by caller)
            if (_isExternalData)
            {
                SearchTermChanged?.Invoke(this, searchTerm);
                return;
            }

            string normalizedSearchTerm = searchTerm.ToLowerInvariant();

            _filteredRows = string.IsNullOrEmpty(normalizedSearchTerm)
                ? _allRows.ToList()
                : _allRows.Where(o => _displaySelector(o).Any(s => s.ToLower().Contains(normalizedSearchTerm)))
                           .ToList();

            _rowsVersion++;

            _currentPage = 1; // Reset to first page
            UpdatePage();
        }
    }
}
