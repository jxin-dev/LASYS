namespace LASYS.UIControls.Controls
{
    public class CustomList<T> : Control
    {
        private readonly DoubleBufferedFlowLayoutPanel _panel;
        private readonly TableLayoutPanel _headerRow;
        private readonly TableLayoutPanel _container;

        public IList<ColumnDefinition<T>> Columns { get; } = new List<ColumnDefinition<T>>();

        public CustomList()
        {
            Dock = DockStyle.Fill;

            _container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            _container.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            _container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _headerRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                AutoSize = false
            };

            _panel = new DoubleBufferedFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Red
            };
            _container.Controls.Add(_panel);
            _container.Controls.Add(_headerRow);
            _container.Controls.SetChildIndex(_headerRow, 0);
            _container.Controls.SetChildIndex(_panel, 1);

            Controls.Add(_container);
           

        
            _panel.SizeChanged += (_, _) =>
            {
                foreach (Control c in _panel.Controls)
                {
                    c.Width = _panel.ClientSize.Width;
                }
            };
        }

        public void CreateHeader()
        {
            if (Columns.Count == 0)
                throw new InvalidOperationException("Columns must be added before creating the header.");

            _headerRow.SuspendLayout();

            _headerRow.Controls.Clear();
            _headerRow.ColumnStyles.Clear();
            _headerRow.RowStyles.Clear();

            _headerRow.ColumnCount = Columns.Count;
            _headerRow.RowCount = 1;
            _headerRow.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            _headerRow.BackColor = Color.DimGray;
            _headerRow.ForeColor = Color.White;

            _headerRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];

                _headerRow.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Absolute, col.Width));

                Control headerControl;

                if (col.Type == ColumnType.CheckBox)
                {
                    var cb = new CheckBox
                    {
                        Dock = DockStyle.Fill,
                        CheckAlign = ContentAlignment.MiddleCenter
                    };

                    cb.CheckedChanged += (_, _) =>
                    {
                        foreach (var row in _panel.Controls
                                 .OfType<CustomRowControl<T>>())
                        {
                            if (row.TryGetCellControl(col.Key, out var cell)
                                && cell is CheckBox rowCb)
                            {
                                rowCb.Checked = cb.Checked;
                            }
                        }
                    };

                    headerControl = cb;
                }
                else
                {
                    headerControl = new Label
                    {
                        Text = col.Header ?? "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font(Font, FontStyle.Bold)
                    };
                }

                _headerRow.Controls.Add(headerControl, i, 0);
            }

            _headerRow.ResumeLayout();
        }

        public void SetItems(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (Columns.Count == 0)
                throw new InvalidOperationException("Columns must be defined before calling SetItems().");

            _panel.SuspendLayout();
            _panel.Controls.Clear();

            var columnsSnapshot = Columns.ToList();

            foreach (var item in items)
            {
                var row = new CustomRowControl<T>(columnsSnapshot, item)
                {
                    Dock = DockStyle.Top,
                    Width = _panel.ClientSize.Width,
                    //Margin = new Padding(0)
                    Margin = new Padding(0,1,0,1)
                };

                _panel.Controls.Add(row);
                //_panel.Controls.SetChildIndex(row, 0);
            }

            _panel.ResumeLayout(true);
        }
    }
    public class DoubleBufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public DoubleBufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
    public class ColumnAction<T>
    {
        public string Text { get; set; }
        public Action<T> Action { get; set; }
    }
    public enum ColumnType
    {
        CheckBox,
        Icon,
        Text,
        Link,
        Badge,
        Button
    }
    public class ColumnDefinition<T>
    {
        public string Key { get; set; }
        public string Header { get; set; }
        public int Width { get; set; } = 100;
        public ColumnType Type { get; set; }

        public Func<T, object> ValueGetter { get; set; }
        public Action<T, object> ValueSetter { get; set; }

        public List<ColumnAction<T>> Actions { get; set; } = new();
    }

    public class CustomRowControl<T> : Control
    {
        private readonly T _item;
        private readonly Dictionary<string, Control> _cells = new();

        public CustomRowControl(IEnumerable<ColumnDefinition<T>> columns, T item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            Dock = DockStyle.Top;
            Height = 36;

            var columnList = columns.ToList();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columnList.Count,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                AutoSize = false
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            foreach (var col in columnList)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, col.Width));

            BuildCells(layout, columnList);

            Controls.Add(layout);
        }

        public bool TryGetCellControl(string key, out Control control)
        {
            return _cells.TryGetValue(key, out control);
        }

        private void BuildCells(TableLayoutPanel layout, IReadOnlyList<ColumnDefinition<T>> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var cell = CreateCell(col);

                if (!string.IsNullOrWhiteSpace(col.Key))
                    _cells[col.Key] = cell;

                layout.Controls.Add(cell, i, 0);
            }
        }

        private Control CreateCell(ColumnDefinition<T> col)
        {
            switch (col.Type)
            {
                case ColumnType.CheckBox:
                    var cb = new CheckBox
                    {
                        Dock = DockStyle.Fill,
                        Checked = Convert.ToBoolean(col.ValueGetter?.Invoke(_item)),
                        CheckAlign = ContentAlignment.MiddleCenter
                    };

                    if (col.ValueSetter != null)
                        cb.CheckedChanged += (_, _) =>
                            col.ValueSetter(_item, cb.Checked);

                    return cb;

                case ColumnType.Text:
                    return new Label
                    {
                        Text = col.ValueGetter?.Invoke(_item)?.ToString(),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                case ColumnType.Link:
                    return new LinkLabel
                    {
                        Text = col.ValueGetter?.Invoke(_item)?.ToString(),
                        Dock = DockStyle.Fill
                    };

                case ColumnType.Badge:
                    return new Label
                    {
                        Text = col.ValueGetter?.Invoke(_item)?.ToString(),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.LightGray
                    };

                case ColumnType.Button:
                    var panel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false
                    };

                    foreach (var action in col.Actions)
                    {
                        var btn = new Button
                        {
                            Text = action.Text,
                            AutoSize = true
                        };

                        btn.Click += (_, _) => action.Action(_item);
                        panel.Controls.Add(btn);
                    }

                    return panel;

                case ColumnType.Icon:
                    return new PictureBox
                    {
                        Image = col.ValueGetter?.Invoke(_item) as Image,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.CenterImage
                    };

                default:
                    throw new NotSupportedException();
            }
        }
    }
}

// Note: Sample usage of CustomList<T> would be in a form where you define columns and set items like this:
/*

 */