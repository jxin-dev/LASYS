using LASYS.Application.Services;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Services;
using LASYS.Domain.OCR;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class OCRCalibrationControl : UserControl
    {
        private readonly DraggableResizerPanel _resizablePanel;

        private readonly OCRConfigService _ocrService;
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

        public OCRCalibrationControl(OCRConfigService ocrService, DeviceConfigService deviceConfigService)
        {
            _ocrService = ocrService;
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

            _cameraService = new CameraService();
            Load += OCRCalibrationControl_Load;



            picCameraPreview.MouseDown += (sender, e) =>
            {
                if (!_canDraw) return;

                _drawing = true;
                _startPoint = e.Location;
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

                    btnSaveCalibration.Click += async (sender, e) =>
                    {

                        // Convert viewer (PictureBox) region to image region
                        var result = _ocrService.ComputeImageRegion(_roi, picCameraPreview.Size, picCameraPreview.Image.Size);
                        if (result != null)
                        {
                            var product = new Product
                            {
                                ItemCode = "SR+OX2051C2", 
                                Coordinates = new Coordinates
                                {
                                    X = result.ImageRegion.X,
                                    Y = result.ImageRegion.Y,
                                    Width = result.ImageRegion.Width,
                                    Height = result.ImageRegion.Height,
                                    ImageWidth = picCameraPreview.Image.Width,
                                    ImageHeight = picCameraPreview.Image.Height
                                },
                                RegisteredAt = DateTime.Now
                            };
                            _ocrService?.AddOrUpdateProductAsync(product);
                            await ReloadRegisteredItemsAsync();

                            // Remove example
                            //await _ocrService.RemoveProductAsync("SR+OX2051C1");
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

            _resizablePanel.AddTab("Registered Item", container);

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
            var config = await _ocrService.LoadAsync();

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

        //private async void LoadRegisteredItem()
        //{
        //    var container = new Panel
        //    {
        //        Dock = DockStyle.Fill,
        //        AutoSize = true
        //    };

        //    _resizablePanel.AddTab("Registered Item", container);

        //    var gridWithPagination = new GridViewWithPagination
        //    {
        //        PageSize = 5,
        //        Dock = DockStyle.Fill,
        //        BackColor = Color.White
        //    };

        //    gridWithPagination.SetMergedHeaders(
        //        [
        //            new HeaderColumn { Text = "Item Code", ColSpan = 1 },
        //            new HeaderColumn { Text = "Coordinates", ColSpan = 6 },
        //            new HeaderColumn { Text = "Date Registered", ColSpan = 1 }
        //        ],
        //        ["Item Code", "X", "Y", "Width", "Height", "Image Width", "Image Height", "Date Registered"]
        //    );

        //    gridWithPagination.SetColumnWidths(
        //        150, 80, 80, 80, 80, 100, 100, 150
        //    );

        //    // Load and show in grid
        //    var config = await _ocrService.LoadAsync();
        //    gridWithPagination.SetRows(config.Products, p =>
        //    [
        //        p.ItemCode,
        //        p.Coordinates.X.ToString(),
        //        p.Coordinates.Y.ToString(),
        //        p.Coordinates.Width.ToString(),
        //        p.Coordinates.Height.ToString(),
        //        p.Coordinates.ImageWidth.ToString(),
        //        p.Coordinates.ImageHeight.ToString(),
        //        p.RegisteredAt.ToString("yyyy-MM-dd HH:mm:ss")
        //    ]);


        //    var newProduct = new Product
        //    {
        //        ItemCode = "SR+OX2051C1",
        //        Coordinates = new Coordinates { X = 640, Y = 376, Width = 48, Height = 18, ImageWidth = 1097, ImageHeight = 611 },
        //        RegisteredAt = DateTime.Now
        //    };

        //    //await _ocrService.AddOrUpdateProductAsync(newProduct);

        //    // Remove example
        //    //await _ocrService.RemoveProductAsync("SR+OX2051C1");


        //    gridWithPagination.RowDoubleClicked += (sender, e) =>
        //    {
        //        // Handle row double-click event
        //        if (e is Product data)
        //        {
        //            MessageBox.Show($"Item Code: {data.ItemCode}");
        //        }
        //    };

        //    container.Controls.Add(gridWithPagination);

        //}
    }

}
