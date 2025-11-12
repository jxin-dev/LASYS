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

        public int RowHeight { get; set; } = 28;
        public int HeaderHeight { get; set; } = 32;
        public Color HeaderBackColor { get; set; } = Color.FromArgb(45, 45, 48);
        public Color HeaderForeColor { get; set; } = Color.White;
        public Color RowBackColor { get; set; } = Color.White;
        public Color AltRowBackColor { get; set; } = Color.FromArgb(245, 245, 245);
        public Color GridLineColor { get; set; } = Color.FromArgb(200, 200, 200);
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

            foreach (var item in data)
            {
                _rowDataObjects.Add(item!);
                _rows.Add(displaySelector(item));
            }

            UpdateLayout();
        }




        private void UpdateLayout()
        {
            using var g = CreateGraphics();
            CalculateColumnWidths(g);
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

            int totalContentWidth = _columnWidths.Sum();
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
            using var gridPen = new Pen(GridLineColor);
            using var headerBrush = new SolidBrush(HeaderBackColor);
            using var headerTextBrush = new SolidBrush(HeaderForeColor);

            int x = xOffset;

            // Top merged header row
            for (int topIndex = 0, subIndex = 0; topIndex < _topHeaders.Count; topIndex++)
            {
                var top = _topHeaders[topIndex];
                int width = _columnWidths.Skip(subIndex).Take(top.ColSpan).Sum();

                if (top.ColSpan == 1)
                {
                    // Single column → draw it spanning BOTH header rows
                    g.FillRectangle(headerBrush, x, 0, width, HeaderHeight * 2);
                    g.DrawRectangle(gridPen, x, 0, width, HeaderHeight * 2);
                    g.DrawString(top.Text, HeaderFont, headerTextBrush,
                        new RectangleF(x, 0, width, HeaderHeight * 2),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
                else
                {
                    // Multi-column merged header
                    g.FillRectangle(headerBrush, x, 0, width, HeaderHeight);
                    g.DrawRectangle(gridPen, x, 0, width, HeaderHeight);
                    g.DrawString(top.Text, HeaderFont, headerTextBrush,
                        new RectangleF(x, 0, width, HeaderHeight),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                    // Sub-headers row
                    int subX = x;
                    for (int i = 0; i < top.ColSpan; i++)
                    {
                        int colWidth = _columnWidths[subIndex + i];
                        g.FillRectangle(headerBrush, subX, HeaderHeight, colWidth, HeaderHeight);
                        g.DrawRectangle(gridPen, subX, HeaderHeight, colWidth, HeaderHeight);
                        g.DrawString(_subHeaders[subIndex + i], HeaderFont, headerTextBrush,
                            new RectangleF(subX, HeaderHeight, colWidth, HeaderHeight),
                            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                        subX += colWidth;
                    }
                }

                x += width;
                subIndex += top.ColSpan;
            }
        }

        private void DrawDataRow(Graphics g, string[] row, int rowIndex, int y, int xOffset, int viewportWidth)
        {
            int totalWidth = _columnWidths.Sum();
            var backColor = (rowIndex % 2 == 0) ? RowBackColor : AltRowBackColor;
            using var b = new SolidBrush(backColor);
            g.FillRectangle(b, -_hScroll.Value, y, totalWidth, RowHeight); // spans all columns

            // 2) Text and grid lines
            using var f = new SolidBrush(ForeColor);
            using var gridPen = new Pen(GridLineColor);

            int x = xOffset;
            for (int c = 0; c < row.Length && c < _columnWidths.Length; c++)
            {
                int colWidth = _columnWidths[c];
                g.DrawString(row[c], RowFont, f,
                    new RectangleF(x + 6, y + 4, colWidth - 12, RowHeight - 8));

                g.DrawRectangle(gridPen, x, y, colWidth, RowHeight);
                x += colWidth;
            }

            // 3) Highlight if selected
            if (rowIndex == _selectedRowIndex)
            {
                using var highlightPen = new Pen(Color.LimeGreen, 3);
                g.DrawRectangle(highlightPen, -_hScroll.Value, y, totalWidth - 1, RowHeight - 1);
            }
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

        public Color HoverBackColor { get; set; } = Color.FromArgb(50, 50, 50);
        public new Color DefaultBackColor { get; set; } = Color.FromArgb(45, 45, 48);

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
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };


            Controls.Add(_icon);
            Controls.Add(_textLabel);

            // Mouse events
            MouseEnter += (s, e) => BackColor = HoverBackColor;
            MouseLeave += (s, e) => BackColor = DefaultBackColor;
            MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);

            // Propagate events to children
            _icon.MouseEnter += (s, e) => BackColor = HoverBackColor;
            _icon.MouseLeave += (s, e) => BackColor = DefaultBackColor;
            _icon.MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);

            _textLabel.MouseEnter += (s, e) => BackColor = HoverBackColor;
            _textLabel.MouseLeave += (s, e) => BackColor = DefaultBackColor;
            _textLabel.MouseClick += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);
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


        private int _currentPage = 1;
        private int _pageSize = 10;
        private List<object> _allRows = new(); // all row objects
        private List<object> _filteredRows = new(); // filtered by search

        private Func<object, string[]> _displaySelector = null!;

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
                BackColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(5),
                Margin = new Padding(0)
            };
            EnableDoubleBuffer(_searchPanel);

            _txtSearch = new PlaceholderTextBox
            {
                PlaceholderText = "Search...",
                PlaceholderColor = Color.FromArgb(160, 160, 160),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 80, 80),
            };
            EnableDoubleBuffer(_txtSearch);

            _txtSearch.TextChanged += (sender, e) =>
            {
                string searchTerm = _txtSearch.Text.Trim().ToLower();

                _filteredRows = string.IsNullOrEmpty(searchTerm)
                    ? _allRows.ToList()
                    : _allRows.Where(o => _displaySelector(o).Any(s => s.ToLower().Contains(searchTerm)))
                               .ToList();

                _currentPage = 1; // Reset to first page
                UpdatePage();

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
                BackColor = Color.FromArgb(45, 45, 48)
            };
            EnableDoubleBuffer(_paginationPanel);


            // Container for buttons, centered horizontally
            var buttonContainer = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point((_paginationPanel.Width - 0) / 2, 5), // temporary X, will adjust later
            };
            EnableDoubleBuffer(buttonContainer);

            // Initialize page label
            _lblPage = new Label
            {
                Text = "Page 1 of 1",
                AutoSize = false,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 35,
            };
            EnableDoubleBuffer(_lblPage);

            // Initialize IconButtons
            _btnFirst = CreateButton("First", @"Resources/first.png");
            _btnPrev = CreateButton("Prev", @"Resources/previous.png");
            _btnNext = CreateButton("Next", @"Resources/next.png");
            _btnLast = CreateButton("Last", @"Resources/last.png");

            EnableDoubleBuffer(_btnFirst);
            EnableDoubleBuffer(_btnPrev);
            EnableDoubleBuffer(_btnNext);
            EnableDoubleBuffer(_btnLast);

            // Add buttons to container
            buttonContainer.Controls.AddRange([_btnFirst, _btnPrev, _lblPage, _btnNext, _btnLast]);

            // Add container to panel
            _paginationPanel.Controls.Add(buttonContainer);

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
            _btnFirst.Clicked += (s, e) => { _currentPage = 1; UpdatePage(); };
            _btnPrev.Clicked += (s, e) => { if (_currentPage > 1) _currentPage--; UpdatePage(); };
            _btnNext.Clicked += (s, e) => { if (_currentPage < TotalPages) _currentPage++; UpdatePage(); };
            _btnLast.Clicked += (s, e) => { _currentPage = TotalPages; UpdatePage(); };
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
                Width = 70,
                Height = 30,
                DefaultBackColor = _paginationPanel.BackColor,
                HoverBackColor = LightenColor(_paginationPanel.BackColor, 20),
                ForeColor = Color.White,
                Icon = Image.FromFile(imagePath) // Load icon from file
            };
        }

        public int PageSize
        {
            get => _pageSize;
            set { _pageSize = Math.Max(1, value); UpdatePage(); }
        }

        public int CurrentPage => _currentPage;

        public int TotalPages => (_filteredRows.Count + _pageSize - 1) / _pageSize;
        public void SetRows<T>(IEnumerable<T> rows, Func<T, string[]> displaySelector)
        {
            _allRows = rows.Cast<object>().ToList();
            _filteredRows = _allRows.ToList(); // reset filter
            _displaySelector = obj => displaySelector((T)obj);
            _currentPage = 1;
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

        public event EventHandler<object?>? RowDoubleClicked
        {
            add => _grid.RowDoubleClicked += value;
            remove => _grid.RowDoubleClicked -= value;
        }
        private void UpdatePage()
        {
            if (_filteredRows.Count == 0)
            {
                _grid.SetRows(Array.Empty<object>(), o => Array.Empty<string>());
                _lblPage.Text = "Page 0 of 0";
                return;
            }

            int totalPages = (_filteredRows.Count + _pageSize - 1) / _pageSize;

            var pageRows = _filteredRows
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            _grid.SetRows(pageRows, _displaySelector);
            _grid.ClearSelection();

            _lblPage.Text = $"Page {_currentPage} of {totalPages}";

            // Enable/disable buttons
            _btnFirst.Enabled = _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled = _btnLast.Enabled = _currentPage < totalPages;

        }
    }
}
