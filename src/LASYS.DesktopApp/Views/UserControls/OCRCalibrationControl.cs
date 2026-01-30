using LASYS.Application.Services;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Services;
using LASYS.Domain.OCR;
using LASYS.OCR.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class OCRCalibrationControl : UserControl
    {
        private readonly DraggableResizerPanel _resizablePanel;

        private readonly OCRConfigService _ocrConfigService;
        private readonly ICameraService _cameraService;

        private readonly DeviceConfigService _deviceConfigService;

        public event Action? CameraNotAvailable;

        private Point _startPoint;
        private Rectangle _roi;
        private bool _drawing = false;
        private bool _canDraw = true;
        private NormalizedRect? _normalizedRegion;
        private Button btnSaveCalibration = new();
        private Button btnCancelCalibration = new();
        private Rectangle? _ocrViewerRegion;


        private GridViewWithPagination _gridWithPagination;

        private TextBox? _txtItemCode;
        private TextBox? _txtX;
        private TextBox? _txtY;
        private TextBox? _txtWidth;
        private TextBox? _txtHeight;
        private TextBox? _txtImgWidth;
        private TextBox? _txtImgHeight;

        public OCRCalibrationControl(OCRConfigService ocrConfigService, DeviceConfigService deviceConfigService)
        {
            _ocrConfigService = ocrConfigService;
            _deviceConfigService = deviceConfigService;

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

            _cameraService = new CameraService();
            Load += OCRCalibrationControl_Load;



            picCameraPreview.MouseDown += (sender, e) =>
            {
                if (!_canDraw || picCameraPreview.Image == null) return;

                _drawing = true;
                _startPoint = e.Location;
                _resizablePanel.HidePanel();
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

                    btnSaveCalibration.Click += (sender, e) =>
                    {

                        // Convert viewer (PictureBox) region to image region
                        var result = _ocrConfigService.ComputeImageRegion(_roi, picCameraPreview.Size, picCameraPreview.Image.Size);
                        if (result != null)
                        {
                            UpdateCoordinateFields(result.ImageRegion,picCameraPreview.Image.Size);
                            _resizablePanel.ShowTab("Calibration Setup");
                        }

                        _normalizedRegion = NormalizedRect.FromAbsolute(_roi, picCameraPreview.Size);
                        _roi = Rectangle.Empty;
                        picCameraPreview.Invalidate();
                        picCameraPreview.Controls.Remove(btnSaveCalibration);
                        picCameraPreview.Controls.Remove(btnCancelCalibration);
                        btnSaveCalibration.Dispose();
                        btnCancelCalibration.Dispose();
                        _canDraw = true; //Allow drawing again

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
                        ClearCoordinateFields();
                        _roi = Rectangle.Empty;
                        picCameraPreview.Invalidate();

                        picCameraPreview.Controls.Remove(btnSaveCalibration);
                        picCameraPreview.Controls.Remove(btnCancelCalibration);
                        btnSaveCalibration.Dispose();
                        btnCancelCalibration.Dispose();
                        _canDraw = true; //Allow drawing again
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
                    using var pen = new Pen(Color.LimeGreen, 1);
                    e.Graphics.DrawRectangle(pen, _ocrViewerRegion.Value);
                }

                if (_roi != Rectangle.Empty)
                {

                    if (_normalizedRegion != null)
                    {
                        Rectangle rect = _normalizedRegion.ToAbsolute(picCameraPreview.Size);
                        e.Graphics.DrawRectangle(Pens.Red, rect);
                    }
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


        private async void LoadRegisteredItem()
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
                    MessageBox.Show($"Item Code: {data.ItemCode}");
                }
            };

            container.Controls.Add(_gridWithPagination);

            // Initial load
            await ReloadRegisteredItemsAsync();
        }
        private async Task ReloadRegisteredItemsAsync()
        {
            var config = await _ocrConfigService.LoadAsync();

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

            btnSave.Click += async(sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtItemCode.Text))
                {
                    MessageBox.Show("Please enter an Item Code.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Try to parse the coordinate values from the textboxes
                bool parsedX = int.TryParse(_txtX.Text, out int x);
                bool parsedY = int.TryParse(_txtY.Text, out int y);
                bool parsedWidth = int.TryParse(_txtWidth.Text, out int width);
                bool parsedHeight = int.TryParse(_txtHeight.Text, out int height);
                bool parsedImgWidth = int.TryParse(_txtImgWidth.Text, out int imgWidth);
                bool parsedImgHeight = int.TryParse(_txtImgHeight.Text, out int imgHeight);

                if (!parsedX || !parsedY || !parsedWidth || !parsedHeight || !parsedImgWidth || !parsedImgHeight)
                {
                    MessageBox.Show("Invalid coordinate values. Please check and try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate rectangle dimensions
                if (width <= 5 || height <= 5)
                {
                    MessageBox.Show("Please specify a valid region with width and height greater than 5.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Rectangle imageRegion = new Rectangle(x, y, width, height);

                var product = new Product
                {
                    ItemCode = _txtItemCode.Text.Trim(),
                    Coordinates = new Coordinates
                    {
                        X = imageRegion.X,
                        Y = imageRegion.Y,
                        Width = imageRegion.Width,
                        Height = imageRegion.Height,
                        ImageWidth = imgWidth,
                        ImageHeight = imgHeight
                    },
                    RegisteredAt = DateTime.Now
                };

                await _ocrConfigService.AddOrUpdateProductAsync(product);
                await ReloadRegisteredItemsAsync();

                MessageBox.Show("Saved successfully", "OCR Registration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //_resizablePanel.ShowTab("OCR Registered");
            };

            btnTestOcr.Click += (sender, e) =>
            {
                // Validate coordinates
                if (!int.TryParse(_txtX!.Text, out int x) ||
                    !int.TryParse(_txtY!.Text, out int y) ||
                    !int.TryParse(_txtWidth!.Text, out int width) ||
                    !int.TryParse(_txtHeight!.Text, out int height))
                {
                    MessageBox.Show("Invalid OCR coordinates.", "OCR Read",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                _resizablePanel.HidePanel();
                // Read OCR 
                // Show messagebox with ocr result
                string result = "Test123";
                MessageBox.Show($"OCR Read Completed Successfully.\n\nDetected Text:\n{result}","OCR Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            };
        }


        private void UpdateCoordinateFields(Rectangle imageRegion, Size imageSize)
        {
            _txtX!.Text = imageRegion.X.ToString();
            _txtY!.Text = imageRegion.Y.ToString();
            _txtWidth!.Text = imageRegion.Width.ToString();
            _txtHeight!.Text = imageRegion.Height.ToString();
            _txtImgWidth!.Text = imageSize.Width.ToString();
            _txtImgHeight!.Text = imageSize.Height.ToString();
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

        private void OCRCalibrationControl_Load(object? sender, EventArgs e)
        {

            this.BeginInvoke(async () =>
            {

                var cameras = _cameraService.GetAvailableCameras();

                if (cameras.Count == 0)
                {
                    await Task.Delay(50);
                    MessageBox.Show(
                        "No camera devices found. Please connect a camera and try again.",
                        "Camera Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                var config = await _deviceConfigService.LoadAsync();
                var selectedCamera = cameras.FirstOrDefault(c => c.Index == config.Camera.CameraIndex && c.Name == config.Camera.CameraName);

                if (selectedCamera == null || config.Camera == null || !config.Camera.Enabled || config.Camera.CameraIndex >= cameras.Count)
                {
                    await Task.Delay(50);
                    MessageBox.Show(
                        "The configured camera is not available or disabled. Please select another camera.",
                        "Camera Not Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    CameraNotAvailable?.Invoke(); // redirect to camera selection
                    return;
                }

                _ = _cameraService.StartPreviewAsync(selectedCamera, picCameraPreview);

                // wait ONLY for first frame
                await _cameraService.PreviewStartedAsync;
            });
        }

    }

}
