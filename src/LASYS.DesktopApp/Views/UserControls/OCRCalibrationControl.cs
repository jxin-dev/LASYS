using System.Drawing.Drawing2D;
using System.Reflection;
using LASYS.Application.Contracts;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;
using OpenCvSharp.Internal.Vectors;

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

        private RichTextBox? _richTextOCRResult;

        private Label? _lblCameraStatus;
        private Button? _btnReconnectCamera;

        private Label? _lblScannerStatus;
        private Label? _lblPrinterStatus;
        private Label? _lblOcrStatus;

        private CheckBox? _focusCheckBox;
        private Label? _focusLevelLabel;

        private Panel? _focusOverlay;
        private TrackBar? _focusSlider;
        private Label? _focusValueLabel;


        private Bitmap? _currentFrame;
        public Size PictureBoxSize => picCameraPreview.Size;

        //Calibration region
        public event EventHandler<ImageRegionEventArgs>? ComputeImageRegionRequested;
        public event EventHandler<CalibrationEventArgs>? SaveCalibrationClicked;


        // Events
        public event EventHandler? ReconnectCameraRequested;
        public event EventHandler? LoadRegisteredOcrItemsRequested;
        public event EventHandler? InitializeRequested;
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

            CameraSettings();

            LoadRegisteredItem();

            CalibrationSetup();

            SystemDevices();

            Load += delegate
            {
                BeginInvoke(() =>
                {
                    _resizablePanel.ShowTab("System Devices");
                    InitializeRequested?.Invoke(this, EventArgs.Empty); // start streaming immediately on load


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



        private void CameraSettings()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(15)
            };

            _resizablePanel.AddTab("Camera Settings", container);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0, 5, 0, 0),
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            container.Controls.Add(layout);

            var titleLabel = new Label
            {
                Text = "Camera Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Height = 40,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(titleLabel, 0, 0);

            var instructionLabel = new Label
            {
                Text = "Select your preferred camera from the list below, choose the desired resolution,\r\nand click 'Save' to store your configuration.\r\n",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(5, 0, 0, 10)
            };
            layout.Controls.Add(instructionLabel, 0, 1);

            var cameraSettingPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true,
                Dock = DockStyle.Top,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            cameraSettingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            cameraSettingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            layout.Controls.Add(cameraSettingPanel, 0, 2);

            var cameraSelectionLabel = new Label
            {
                Text = "Camera Name:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };
            cameraSettingPanel.Controls.Add(cameraSelectionLabel, 0, 0);

            var cameraSelectionComboBox = new ComboBox
            {
                AutoSize = true,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cameraSettingPanel.Controls.Add(cameraSelectionComboBox, 1, 0);


            var cameraResolutionLabel = new Label
            {
                Text = "Resolution:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };
            cameraSettingPanel.Controls.Add(cameraResolutionLabel, 0, 1);

            var cameraResolutionComboBox = new ComboBox
            {
                AutoSize = true,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cameraSettingPanel.Controls.Add(cameraResolutionComboBox, 1, 1);

            _focusCheckBox = new CheckBox
            {
                Text = "Manual Focus:",
                AutoSize = true,
                Checked = true,
                CheckAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 5, 5, 5)
            };
            cameraSettingPanel.Controls.Add(_focusCheckBox, 0, 2);

            _focusLevelLabel = new Label
            {
                Text = "0",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Margin = new Padding(5, 0, 0, 0)
            };
            cameraSettingPanel.Controls.Add(_focusLevelLabel, 1, 2);

            var saveButton = new Button
            {
                Text = "Save Configuration",
                Font = new Font("Segoe UI Semibold,", 9),
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Height = 36,
                ForeColor = Color.White,
                BackColor = SystemColors.HotTrack,
                Margin = new Padding(0, 5, 0, 0)
            };

            cameraSettingPanel.Controls.Add(saveButton, 0, 3);

            InitializeFocusOverlay();


            _focusCheckBox.CheckedChanged += (s, e) =>
            {
                _focusOverlay!.Visible = _focusCheckBox.Checked;
                if (!_focusCheckBox.Checked)
                    _focusLevelLabel.Text = "0";
                else
                    _focusLevelLabel.Text = _focusSlider?.Value.ToString();

            };
        }
        private void InitializeFocusOverlay()
        {
            // Overlay panel
            _focusOverlay = new Panel
            {
                Width = 280,
                Height = 44,
                BackColor = Color.FromArgb(160, 25, 25, 25)
            };

            _focusOverlay.DoubleBuffered(true);



            // Slider container
            Panel sliderContainer = new Panel
            {
                Width = 190,
                Height = 24,
                Left = 10,
                Top = 10,
                BackColor = Color.FromArgb(45, 45, 45),
            };

            // Trackbar
            _focusSlider = new TrackBar
            {
                Minimum = 0,
                Maximum = 255,
                TickStyle = TickStyle.None,
                Dock = DockStyle.Fill,
                Value = 120
            };

          
            // Value badge
            _focusValueLabel = new Label
            {
                Text = _focusSlider.Value.ToString(),
                Width = 44,
                Height = 24,
                Left = sliderContainer.Right + 10,
                Top = sliderContainer.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _focusSlider.Scroll += (s, e) =>
            {
                _focusValueLabel.Text = _focusSlider.Value.ToString();
                _focusLevelLabel!.Text = _focusSlider.Value.ToString();

            };
            sliderContainer.Controls.Add(_focusSlider);


            _focusValueLabel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 120, 215));
                e.Graphics.DrawRectangle(pen, 0, 0,
                    _focusValueLabel.Width - 1,
                    _focusValueLabel.Height - 1);
            };

            // Add controls
            _focusOverlay.Controls.Add(sliderContainer);
            _focusOverlay.Controls.Add(_focusValueLabel);

            picCameraPreview.Controls.Add(_focusOverlay);

            CenterOverlay();

            picCameraPreview.Resize += (s, e) => CenterOverlay();

            
        }

        private void CenterOverlay()
        {
            if (_focusOverlay == null) return;

            _focusOverlay.Left =
                (picCameraPreview.Width - _focusOverlay.Width) / 2;

            _focusOverlay.Top = 10;
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

            var titleLabel = new Label
            {
                Text = "OCR Calibration Setup",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                //BorderStyle = BorderStyle.FixedSingle
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 5, 0, 0),
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            container.Controls.Add(layout);

            container.Controls.Add(titleLabel);


            _richTextOCRResult = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = container.BackColor,
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(_richTextOCRResult, 1, 0);
            layout.SetRowSpan(_richTextOCRResult, 3);

            var labelInfoLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                Dock = DockStyle.Top,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            layout.Controls.Add(labelInfoLayout, 0, 0);

            var itemCodeLabel = new Label
            {
                Text = "Item Code:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };

            labelInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            labelInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            labelInfoLayout.Controls.Add(itemCodeLabel, 0, 0);

            _txtItemCode = new TextBox
            {
                Width = 250,
                Margin = new Padding(0, 5, 15, 5),
            };
            labelInfoLayout.Controls.Add(_txtItemCode, 1, 0);

            var coordGrid = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 4,
                AutoSize = true,
                Dock = DockStyle.Top,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int i = 0; i < 4; i++)
                coordGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            layout.Controls.Add(coordGrid, 0, 1);

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


            var buttonPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 5, 0, 0),
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            layout.Controls.Add(buttonPanel, 0, 2);

            // Save button
            var btnSave = new Button
            {
                Text = "Save Calibration",
                Width = 160,
                Height = 35,
            };
            buttonPanel.Controls.Add(btnSave, 0, 0);

            // OCR Read button
            var btnTestOcr = new Button
            {
                Text = "OCR Read",
                Width = 120,
                Height = 35,
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
                ReconnectCameraRequested?.Invoke(this, EventArgs.Empty);
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
            if (bitmap == null)
            {
                _canDraw = false;
                return;
            }
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

            //if (string.IsNullOrEmpty(result))
            //    MessageBox.Show("No text detected", "OCR Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //else
            //    MessageBox.Show($"Detected: {result}", "OCR Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        public void DisplayOCRResult(string result)
        {
            _richTextOCRResult!.AppendText(result + Environment.NewLine);
            _richTextOCRResult.ScrollToCaret();
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
