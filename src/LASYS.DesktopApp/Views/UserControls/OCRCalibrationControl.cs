using System.Reflection;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.OCR.Models;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class OCRCalibrationControl : UserControl, IOCRCalibrationView
    {
        private readonly DraggableResizerPanel _resizablePanel;

        private GridViewWithPagination _gridWithPagination;

        private TextBox? _txtItemCode;
        private TextBox? _txtX;
        private TextBox? _txtY;
        private TextBox? _txtWidth;
        private TextBox? _txtHeight;
        private TextBox? _txtImgWidth;
        private TextBox? _txtImgHeight;

        private Label? _lblCameraStatus;
        private Button? _btnReconnectCamera;

        private Label? _lblScannerStatus;
        private Label? _lblPrinterStatus;
        private Label? _lblOcrStatus;

        private Bitmap? _currentFrame;
        public Size PictureBoxSize => picCameraPreview.Size;

        //Calibration region
        public event EventHandler<ImageRegionEventArgs>? ComputeImageRegionRequested;
        public event EventHandler<CalibrationEventArgs>? SaveCalibrationClicked;


        // Events
        public event EventHandler? InitializeRequested;
        public event EventHandler? StreamingRequested;
        public event EventHandler? LoadRegisteredOcrItemsRequested;
        public event EventHandler<OCRCoordinatesEventArgs>? OCRTriggered;

        private Rectangle _roi;
        private Point _startPoint;
        private bool _drawing = false;
        private bool _canDraw = true;
        private Rectangle? _ocrViewerRegion;

        private NormalizedRect? _normalizedRegion;
        private Button btnSaveCalibration = new();
        private Button btnCancelCalibration = new();
        public OCRCalibrationControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            _gridWithPagination = new GridViewWithPagination();

            _resizablePanel = new DraggableResizerPanel
            {
                HeightPercentage = 0.7,
                MinimumPanelHeight = 350,
                DefaultPanelHeight = 400
            };

            pnlContent.Controls.Add(_resizablePanel);

            LoadRegisteredItem();

            CalibrationSetup();

            SystemDevices();

            Load += delegate
            {
                BeginInvoke(() =>
                {
                    _resizablePanel.ShowTab("System Devices");
                    StreamingRequested?.Invoke(this, EventArgs.Empty); // start streaming immediately on load
                });
            };


            picCameraPreview.MouseDown += (sender, e) =>
            {
                if (!_canDraw) return;

                _drawing = true;
                _startPoint = e.Location;
                _resizablePanel.HidePanel();
                ClearCoordinateFields();
            };
            picCameraPreview.MouseMove += (sender, e) =>
            {
                if (_drawing && _canDraw)
                {
                    int x = Math.Min(e.X, _startPoint.X);
                    int y = Math.Min(e.Y, _startPoint.Y);
                    int width = Math.Abs(e.X - _startPoint.X);
                    int height = Math.Abs(e.Y - _startPoint.Y);
                    _roi = new Rectangle(x, y, width, height);
                    _normalizedRegion = NormalizedRect.FromAbsolute(_roi, picCameraPreview.Size);
                    picCameraPreview.Invalidate(); // Triggers Paint event to redraw
                }
            };
            picCameraPreview.MouseUp += (sender, e) =>
            {
                if (!_canDraw) return;
                _drawing = false;

                if (_roi.Width > 5 && _roi.Height > 5)
                {
                    _canDraw = false; //Block future draws until buttons clicked

                    btnSaveCalibration = new Button
                    {
                        Text = "✔",
                        Width = 20,
                        Height = 20,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 7, FontStyle.Regular),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.ForestGreen,
                        ForeColor = Color.WhiteSmoke
                    };
                    btnSaveCalibration.FlatAppearance.BorderSize = 0;

                    btnSaveCalibration.Click += delegate
                    {
                        ComputeImageRegionRequested?.Invoke(this, new ImageRegionEventArgs(_roi, picCameraPreview.Size, picCameraPreview.Image?.Size ?? Size.Empty));
                    };

                    btnCancelCalibration = new Button
                    {
                        Text = "✘",
                        Width = 20,
                        Height = 20,
                        Font = new Font("Segoe UI", 7, FontStyle.Regular),
                        TextAlign = ContentAlignment.MiddleCenter,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.DarkRed,
                        ForeColor = Color.WhiteSmoke
                    };
                    btnCancelCalibration.FlatAppearance.BorderSize = 0;

                    btnCancelCalibration.Click += (sender, e) =>
                    {
                        _roi = Rectangle.Empty;
                        picCameraPreview.Invalidate();
                        picCameraPreview.Controls.Remove(btnSaveCalibration);
                        picCameraPreview.Controls.Remove(btnCancelCalibration);
                        btnSaveCalibration.Dispose();
                        btnCancelCalibration.Dispose();
                        _canDraw = true; //Allow drawing again
                        _normalizedRegion = null;
                    };



                    Point regionScreen = picCameraPreview.PointToScreen(new Point(_roi.Left, _roi.Top));
                    Point regionPictureBox = picCameraPreview.PointToClient(regionScreen);
                    btnSaveCalibration.Location = new Point(regionPictureBox.X, regionPictureBox.Y - 21);
                    btnCancelCalibration.Location = new Point(regionPictureBox.X + 21, regionPictureBox.Y - 21);


                    picCameraPreview.Controls.Add(btnSaveCalibration);
                    picCameraPreview.Controls.Add(btnCancelCalibration);

                    btnSaveCalibration.BringToFront();
                    btnCancelCalibration.BringToFront();
                }
            };

            picCameraPreview.Paint += (sender, e) =>
            {
                if (_ocrViewerRegion.HasValue)
                {
                    using var pen = new Pen(Color.LimeGreen, 2);
                    e.Graphics.DrawRectangle(pen, _ocrViewerRegion.Value);
                }

                if (_normalizedRegion != null)
                {
                    Rectangle rect = _normalizedRegion.ToAbsolute(picCameraPreview.Size);
                    e.Graphics.DrawRectangle(Pens.Red, rect);
                }

            };

            picCameraPreview.Resize += (sender, e) =>
            {
                if (btnSaveCalibration != null && btnCancelCalibration != null && _normalizedRegion != null)
                {
                    Rectangle rect = _normalizedRegion.ToAbsolute(picCameraPreview.Size);
                    Point regionScreen = picCameraPreview.PointToScreen(new Point(rect.Left, rect.Top));
                    Point regionPictureBox = picCameraPreview.PointToClient(regionScreen);

                    btnSaveCalibration.Location = new Point(regionPictureBox.X, regionPictureBox.Y - 21);
                    btnCancelCalibration.Location = new Point(regionPictureBox.X + 21, regionPictureBox.Y - 21);
                }
                picCameraPreview.Invalidate();
            };

        }



        private void LoadRegisteredItem()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _resizablePanel.AddTab("OCR Registered", container);

            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 5,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Coordinates", ColSpan = 6 },
                    new HeaderColumn { Text = "Date Registered", ColSpan = 1 }
                ],
                ["Item Code", "X", "Y", "Width", "Height", "Image Width", "Image Height", "Date Registered"]
            );

            _gridWithPagination.SetColumnWidths(
                150, 80, 80, 80, 80, 100, 100, 150
            );

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                if (e is Product data)
                {
                    //MessageBox.Show($"Item Code: {data.ItemCode}");

                    OCRTriggered?.Invoke(this, new OCRCoordinatesEventArgs(data.Coordinates.X,
                                                                      data.Coordinates.Y,
                                                                      data.Coordinates.Width,
                                                                      data.Coordinates.Height,
                                                                      data.Coordinates.ImageWidth,
                                                                      data.Coordinates.ImageHeight));
                }
            };

            container.Controls.Add(_gridWithPagination);

            // Initial load
            //await ReloadRegisteredItemsAsync();

        }

        public void RaiseLoadRegisteredOcrItemsRequested()
        {
            LoadRegisteredOcrItemsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SystemDevices()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(15)
            };
            _resizablePanel.AddTab("System Devices", container);

            var titleLabel = new Label
            {
                Text = "System Devices",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };


            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 4,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // --- Camera ---
            var lblCameraTitle = new Label
            {
                Text = "Camera:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(0, 12, 6, 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblCameraStatus = new Label
            {
                Text = "Not connected",
                AutoSize = true,
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 12, 0, 12)
            };

            _btnReconnectCamera = new Button
            {
                Text = "Reconnect",
                AutoSize = true,
                Padding = new Padding(6, 3, 6, 3),
                Visible = false
            };

            _btnReconnectCamera.Click += delegate
            {
                SetReconnectCameraButtonVisibility(false);
                InitializeRequested?.Invoke(this, EventArgs.Empty);
            };


            // --- Scanner ---
            var lblScannerTitle = new Label
            {
                Text = "Scanner:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(0, 12, 6, 12),
            };

            _lblScannerStatus = new Label
            {
                Text = "Not detected",
                AutoSize = true,
                ForeColor = Color.Gray,
                Padding = new Padding(0, 12, 0, 12)
            };

            // --- Printer ---
            var lblPrinterTitle = new Label
            {
                Text = "Printer:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(0, 12, 6, 12),
            };

            _lblPrinterStatus = new Label
            {
                Text = "Not detected",
                AutoSize = true,
                ForeColor = Color.Gray,
                Padding = new Padding(0, 12, 0, 12)
            };

            // --- OCR ---
            var lblOcrTitle = new Label
            {
                Text = "OCR:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(0, 12, 6, 12),
            };

            _lblOcrStatus = new Label
            {
                Text = "Not ready",
                AutoSize = true,
                ForeColor = Color.Gray,
                Padding = new Padding(0, 12, 0, 12),
            };

            // Add controls to table
            table.Controls.Add(lblCameraTitle, 0, 0);
            table.Controls.Add(_lblCameraStatus, 1, 0);
            table.Controls.Add(_btnReconnectCamera, 2, 0);


            table.Controls.Add(lblScannerTitle, 0, 1);
            table.Controls.Add(_lblScannerStatus, 1, 1);

            table.Controls.Add(lblPrinterTitle, 0, 2);
            table.Controls.Add(_lblPrinterStatus, 1, 2);

            table.Controls.Add(lblOcrTitle, 0, 3);
            table.Controls.Add(_lblOcrStatus, 1, 3);

            container.Controls.Add(table);
            container.Controls.Add(titleLabel);
        }
        private void CalibrationSetup()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(15)
            };

            _resizablePanel.AddTab("Calibration Setup", container);

            // Title at the top
            var titleLabel = new Label
            {
                Text = "OCR Calibration Setup",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };


            // Main layout under title
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(0, 5, 0, 0)
            };
            container.Controls.Add(layout);

            container.Controls.Add(titleLabel);
            // Coordinates grid (includes Item Code at top)
            var coordGrid = new TableLayoutPanel
            {
                ColumnCount = 4,
                AutoSize = true,
                Dock = DockStyle.Top
            };

            for (int i = 0; i < 4; i++)
                coordGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            layout.Controls.Add(coordGrid);


            // Coordinates title
            var coordLabel = new Label
            {
                Text = "Coordinates",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };
            coordGrid.Controls.Add(coordLabel, 0, 1);
            coordGrid.SetColumnSpan(coordLabel, 4);

            // Helper to create coordinate TextBoxes
            TextBox CreateCoordField(string label, int col, int row)
            {
                coordGrid.Controls.Add(new Label
                {
                    Text = label,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 5, 5, 5)
                }, col, row);

                var tb = new TextBox
                {
                    Width = 80,
                    Margin = new Padding(0, 5, 15, 5),
                    ReadOnly = true,
                    BackColor = SystemColors.ControlLight,
                    TabStop = false
                };
                coordGrid.Controls.Add(tb, col + 1, row);
                return tb;
            }

            // Add coordinate fields
            _txtX = CreateCoordField("X", 0, 2);
            _txtY = CreateCoordField("Y", 2, 2);
            _txtWidth = CreateCoordField("Width", 0, 3);
            _txtHeight = CreateCoordField("Height", 2, 3);
            _txtImgWidth = CreateCoordField("Image Width", 0, 4);
            _txtImgHeight = CreateCoordField("Image Height", 2, 4);


            // Item Code Label
            var itemCodeLabel = new Label
            {
                Text = "Item Code:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };
            coordGrid.Controls.Add(itemCodeLabel, 0, 0);

            // Item Code TextBox spans columns 1 → 2 (same width as X + Y fields)
            _txtItemCode = new TextBox
            {
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 15, 5),
                Width = _txtX.Width + _txtY.Width + 25
            };
            coordGrid.Controls.Add(_txtItemCode, 1, 0);
            coordGrid.SetColumnSpan(_txtItemCode, 3); // span columns 1,2,3

            var buttonPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Left,
                Margin = new Padding(0, 20, 0, 0)
            };

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Add panel to grid
            coordGrid.Controls.Add(buttonPanel, 0, 5);
            coordGrid.SetColumnSpan(buttonPanel, 4);

            // Save button
            var btnSave = new Button
            {
                Text = "Save Calibration",
                Width = 160,
                Height = 35,
                Margin = new Padding(0, 0, 10, 0)
            };
            buttonPanel.Controls.Add(btnSave, 0, 0);

            // OCR Read button
            var btnTestOcr = new Button
            {
                Text = "OCR Read",
                Width = 120,
                Height = 35,
                Margin = new Padding(0, 0, 10, 0)
            };
            buttonPanel.Controls.Add(btnTestOcr, 1, 0);


            btnSave.Click += delegate
            {
                SaveCalibrationClicked?.Invoke(this, new CalibrationEventArgs(_roi, picCameraPreview.Size, picCameraPreview.Image?.Size ?? Size.Empty, _txtItemCode.Text.Trim()));
            };

            btnTestOcr.Click += delegate
            {

                // Try to parse the coordinate values from the textboxes
                bool parsedX = int.TryParse(_txtX?.Text, out int x);
                bool parsedY = int.TryParse(_txtY?.Text, out int y);
                bool parsedWidth = int.TryParse(_txtWidth?.Text, out int width);
                bool parsedHeight = int.TryParse(_txtHeight?.Text, out int height);
                bool parsedImgWidth = int.TryParse(_txtImgWidth?.Text, out int imgWidth);
                bool parsedImgHeight = int.TryParse(_txtImgHeight?.Text, out int imgHeight);

                OCRTriggered?.Invoke(this, new OCRCoordinatesEventArgs(x, y, width, height, imgWidth, imgHeight));
            };
        }

        public void UpdateCoordinateFields(Rectangle imageRegion, Size imageSize)
        {
            _normalizedRegion = null;
            picCameraPreview.Controls.Remove(btnSaveCalibration);
            picCameraPreview.Controls.Remove(btnCancelCalibration);

            btnSaveCalibration.Dispose();
            btnCancelCalibration.Dispose();

            _canDraw = true;

            _txtX!.Text = imageRegion.X.ToString();
            _txtY!.Text = imageRegion.Y.ToString();
            _txtWidth!.Text = imageRegion.Width.ToString();
            _txtHeight!.Text = imageRegion.Height.ToString();
            _txtImgWidth!.Text = imageSize.Width.ToString();
            _txtImgHeight!.Text = imageSize.Height.ToString();
            _resizablePanel.ShowTab("Calibration Setup");

            _txtItemCode?.Focus();
        }

        private void ClearCoordinateFields()
        {
            _txtX!.Text =
            _txtY!.Text =
            _txtWidth!.Text =
            _txtHeight!.Text =
            _txtImgWidth!.Text =
            _txtImgHeight!.Text = string.Empty;
        }



        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }
        public void ShowCameraStatus(string message, bool isError = false)
        {
            if (isError)
                _lblCameraStatus!.ForeColor = Color.Crimson;
            else
                _lblCameraStatus!.ForeColor = Color.ForestGreen;

            _lblCameraStatus!.Text = message;
        }

        public void ShowOCRRegion(Rectangle viewerRegion)
        {
            _ocrViewerRegion = viewerRegion;
        }

        public void DisplayFrame(Bitmap bitmap)
        {
            if (bitmap == null) return;

            // Dispose previous frame safely
            if (_currentFrame != null)
            {
                var old = picCameraPreview.Image;
                picCameraPreview.Image = null; // detach from PictureBox
                old?.Dispose();
                _currentFrame.Dispose();
            }

            // Clone the incoming bitmap (so PictureBox owns its own copy)
            _currentFrame = (Bitmap)bitmap.Clone();
            picCameraPreview.Image = _currentFrame;
        }

        public void SetReconnectCameraButtonVisibility(bool isVisible)
        {
            _btnReconnectCamera!.Visible = isVisible;
        }

        public void FinishCalibration(string message, bool isError = false)
        {
            _normalizedRegion = null;

            _roi = Rectangle.Empty;

            picCameraPreview.Invalidate();

            picCameraPreview.Controls.Remove(btnSaveCalibration);
            picCameraPreview.Controls.Remove(btnCancelCalibration);

            btnSaveCalibration.Dispose();
            btnCancelCalibration.Dispose();

            _canDraw = true;

            ClearCoordinateFields();

            var title = isError ? "Error Saving" : "Save Coordinates";
            var icon = isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information;

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);

            _resizablePanel.ShowTab("OCR Registered");

        }

        public void DisplayRegisteredOcrItems(OCRConfig config)
        {
            _gridWithPagination.SetRows(config.Products, p =>
            [
             p.ItemCode,
                p.Coordinates.X.ToString(),
                p.Coordinates.Y.ToString(),
                p.Coordinates.Width.ToString(),
                p.Coordinates.Height.ToString(),
                p.Coordinates.ImageWidth.ToString(),
                p.Coordinates.ImageHeight.ToString(),
                p.RegisteredAt.ToString("yyyy-MM-dd HH:mm:ss")
            ]);
        }

        public void ShowOCRResult(string result, string msg, bool isSuccess = true)
        {
            if (isSuccess)
                _lblOcrStatus!.ForeColor = Color.ForestGreen;
            else
                _lblOcrStatus!.ForeColor = Color.Crimson;

            _lblOcrStatus!.Text = msg;

            if (string.IsNullOrEmpty(result))
                MessageBox.Show("No text detected", "OCR Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show($"Detected: {result}", "OCR Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _ocrViewerRegion = null;
            picCameraPreview.Invalidate();
        }

        public void ShowPrinterStatus(string message)
        {
            _lblPrinterStatus!.ForeColor = Color.ForestGreen;
            _lblPrinterStatus!.Text = message;
        }

        public void ShowBarcodeStatus(string message, bool isError = false)
        {
            _lblScannerStatus!.ForeColor = isError ? Color.Crimson : Color.ForestGreen;
            _lblScannerStatus!.Text = message;
        }
    }
}
public static class ControlExtensions
{
    public static void DoubleBuffered(this Control control, bool enable)
    {
        typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(control, enable, null);
    }
}
