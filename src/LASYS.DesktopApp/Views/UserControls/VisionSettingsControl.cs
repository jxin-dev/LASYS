using System.Reflection;
using LASYS.Application.Contracts;
using LASYS.Application.Events;
using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class VisionSettingsControl : UserControl, IVisionSettingsView
    {
        private readonly DraggableResizerPanel _resizablePanel;

        private GridViewWithPagination _gridWithPagination;

        private TextBox? _txtItemCode;
        private TextBox? _txtRevisionNumber;
        private TextBox? _txtX;
        private TextBox? _txtY;
        private TextBox? _txtWidth;
        private TextBox? _txtHeight;
        private TextBox? _txtImgWidth;
        private TextBox? _txtImgHeight;
        private TextBox? _txtBoxType;

        private string _selectedFilePath = string.Empty;

        private Button? _printSampleLabelButton;
        private Button? _btnTestOcr;

        private RichTextBox? _richTextOCRResult;

        private CheckBox? _focusCheckBox;
        private Label? _focusLevelLabel;

        private Panel? _focusOverlay;
        private TrackBar? _focusSlider;
        private Label? _focusValueLabel;


        private ComboBox? _cameraSelectionComboBox;
        private ComboBox? _cameraResolutionComboBox;


        private Bitmap? _currentFrame;
        public Size PictureBoxSize => picCameraPreview.Size;

        public CameraInfo? SelectedCamera => _cameraSelectionComboBox?.SelectedItem as CameraInfo;

        //Calibration region
        public event EventHandler<ImageRegionEventArgs>? ComputeImageRegionRequested;
        public event EventHandler<CalibrationEventArgs>? SaveCalibrationClicked;


        // Events
        public event EventHandler? LoadRegisteredOcrItemsRequested;
        public event EventHandler? InitializeRequested;
        public event EventHandler<OCRCoordinatesEventArgs>? OCRTriggered;
        public event EventHandler<int>? FocusValueChanged;

        public event EventHandler<CameraSelectedEventArgs>? CameraPreviewStateChanged;
        public event EventHandler<CameraSavedEventArgs>? CameraConfigurationSaved;
        public event EventHandler? LoadCamerasRequested;
        public event EventHandler<string>? CameraResolutionSelected;
        public event EventHandler<OCRCoordinatesEventArgs>? OCRCalibrationPreview;
        public event EventHandler? SelectOcrItemRequested;
        public event Action<Product>? OcrItemChosen;
        public event EventHandler<PrintLabelEventArgs>? PrintLabelRequested;

        private Rectangle _roi;
        private Point _startPoint;
        private bool _drawing = false;
        private bool _canDraw = true;
        private Rectangle? _ocrViewerRegion;
        private Rectangle? _ocrPreviewRegion;

        private NormalizedRect? _normalizedRegion;
        private Button btnSaveCalibration = new();
        private Button btnCancelCalibration = new();
        public VisionSettingsControl()
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

            OCRRegionCalibration();

            //SystemDevices();

            Load += delegate
            {
                BeginInvoke(() =>
                {
                    _resizablePanel.ShowTab("Camera Settings");
                    InitializeRequested?.Invoke(this, EventArgs.Empty); // start streaming immediately on load
                });
            };


            picCameraPreview.MouseDown += (sender, e) =>
            {
                if (!_canDraw) return;

                _ocrPreviewRegion = null;
                picCameraPreview.Invalidate();

                _drawing = true;
                _startPoint = e.Location;
                _resizablePanel.HidePanel();
                //ClearCoordinateFields();
            };
            picCameraPreview.MouseMove += (sender, e) =>
            {
                if (_drawing && _canDraw)
                {
                    int maxX = picCameraPreview.Width - 1;
                    int maxY = picCameraPreview.Height - 1;

                    int mouseX = Math.Max(0, Math.Min(e.X, maxX));
                    int mouseY = Math.Max(0, Math.Min(e.Y, maxY));

                    int x = Math.Min(mouseX, _startPoint.X);
                    int y = Math.Min(mouseY, _startPoint.Y);
                    int width = Math.Abs(mouseX - _startPoint.X);
                    int height = Math.Abs(mouseY - _startPoint.Y);

                    _roi = new Rectangle(x, y, width, height);

                    _normalizedRegion = NormalizedRect.FromAbsolute(_roi, picCameraPreview.Size);

                    picCameraPreview.Invalidate();
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
                        ForeColor = Color.WhiteSmoke,
                        Cursor = Cursors.Hand
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
                        ForeColor = Color.WhiteSmoke,
                        Cursor = Cursors.Hand
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

                    // ---- POSITION BUTTONS ----
                    int offset = 21;

                    int buttonY = (_roi.Top - offset < 0)
                        ? _roi.Bottom + 2     // place below ROI if near top
                        : _roi.Top - offset;  // place above ROI

                    btnSaveCalibration.Location = new Point(_roi.Left, buttonY);
                    btnCancelCalibration.Location = new Point(_roi.Left + 21, buttonY);

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

                if (_ocrPreviewRegion.HasValue)
                {
                    using var pen = new Pen(Color.FromArgb(0, 150, 136), 2);
                    e.Graphics.DrawRectangle(pen, _ocrPreviewRegion.Value);
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
                //BackColor = Color.White
            };

            _gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Revision", ColSpan = 1 },
                    new HeaderColumn { Text = "Box Type", ColSpan = 1 },
                    new HeaderColumn { Text = "Coordinates", ColSpan = 6 },
                    new HeaderColumn { Text = "Date Registered", ColSpan = 1 }
                ],
                ["Item Code", "Revision", "Box Type", "X", "Y", "Width", "Height", "Image Width", "Image Height", "Date Registered"]
            );

            _gridWithPagination.SetColumnWidths(
                150, 80, 80, 80, 80, 80, 80, 100, 100, 150
            );

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                if (e is Product data)
                {
                    _resizablePanel.HidePanel();
                    OCRCalibrationPreview?.Invoke(this, new OCRCoordinatesEventArgs(data.Coordinates.X,
                                                                      data.Coordinates.Y,
                                                                      data.Coordinates.Width,
                                                                      data.Coordinates.Height,
                                                                      data.Coordinates.ImageWidth,
                                                                      data.Coordinates.ImageHeight));


                    OcrItemChosen?.Invoke(data);
                }
            };

            container.Controls.Add(_gridWithPagination);

        }

        public void RaiseLoadRegisteredOcrItemsRequested()
        {
            LoadRegisteredOcrItemsRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadCamerasRequested?.Invoke(this, EventArgs.Empty);
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
                Text = "Select your camera from the list, choose the desired resolution, adjust the focus if needed, \nthen click Save Configuration to apply the settings.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(5, 0, 0, 10)
            };
            layout.Controls.Add(instructionLabel, 0, 1);

            var cameraSettingPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 4,
                AutoSize = true,
                Dock = DockStyle.Top,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            cameraSettingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            cameraSettingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 255));
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

            _cameraSelectionComboBox = new ComboBox
            {
                AutoSize = true,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cameraSelectionComboBox.SelectionChangeCommitted += (sender, e) =>
            {
                if (_cameraSelectionComboBox.SelectedItem is string cameraName)
                {
                    CameraPreviewStateChanged?.Invoke(this, new CameraSelectedEventArgs(cameraName));
                }
            };

            cameraSettingPanel.Controls.Add(_cameraSelectionComboBox, 1, 0);

            var refreshButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Width = 25,
                Height = 24,
                BackColor = Color.DarkSlateGray,
                FlatAppearance = { BorderSize = 0 },
                ImageAlign = ContentAlignment.MiddleCenter,
                Image = Properties.Resources.refresh_24
            };
            refreshButton.Click += (sender, e) =>
            {
                LoadCamerasRequested?.Invoke(this, EventArgs.Empty);
            };

            cameraSettingPanel.Controls.Add(refreshButton, 2, 0);

            var cameraResolutionLabel = new Label
            {
                Text = "Resolution:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };
            cameraSettingPanel.Controls.Add(cameraResolutionLabel, 0, 1);

            _cameraResolutionComboBox = new ComboBox
            {
                AutoSize = true,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cameraResolutionComboBox.SelectedIndexChanged += (sender, e) =>
            {
                CameraResolutionSelected?.Invoke(this, _cameraResolutionComboBox.Text);
            };

            cameraSettingPanel.Controls.Add(_cameraResolutionComboBox, 1, 1);

            _focusCheckBox = new CheckBox
            {
                Text = "Focus:",
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
                Margin = new Padding(0, 0, 0, 0)
            };
            cameraSettingPanel.Controls.Add(_focusLevelLabel, 1, 2);

            var saveButton = new Button
            {
                Text = "Save Configuration",
                Height = 36,
                Width = 160,
                Margin = new Padding(0, 5, 0, 0),
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),
                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(0, 150, 136),
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },
                Cursor = Cursors.Hand
            };

            saveButton.Click += (sender, e) =>
            {
                if (_cameraResolutionComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "Please select a camera.",
                        "Camera Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                if (_cameraResolutionComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a resolution.", "Camera Configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (_cameraSelectionComboBox.SelectedItem is string cameraName)
                {
                    CameraConfigurationSaved?.Invoke(this, new CameraSavedEventArgs(_cameraResolutionComboBox.SelectedIndex, cameraName, _cameraResolutionComboBox.Text, _focusCheckBox.Checked ? _focusSlider!.Value : 0));
                }
            };

            cameraSettingPanel.Controls.Add(saveButton, 0, 3);
            cameraSettingPanel.SetColumnSpan(saveButton, 2);


            InitializeFocusOverlay();


            _focusCheckBox.CheckedChanged += (s, e) =>
            {
                bool manual = _focusCheckBox.Checked;

                _focusSlider!.Enabled = manual;
                _focusOverlay!.Visible = manual;

                if (manual)
                {
                    UpdateFocusUI(_focusSlider.Value == 0 ? 1 : _focusSlider.Value);
                }
                else
                {
                    _focusLevelLabel!.Text = "Auto";
                    FocusValueChanged?.Invoke(this, 0); // 0 = auto focus
                }
            };

        }
        private void InitializeFocusOverlay()
        {
            // Overlay panel
            _focusOverlay = new Panel
            {
                Width = 280,
                Height = 44,
                BackColor = Color.FromArgb(160, 25, 25, 25),
                Visible = false
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
                Maximum = 1000,
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
                UpdateFocusUI(_focusSlider.Value);

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

        public void SetTestButtonText(string text)
        {
            _btnTestOcr!.Text = text;
        }
        private void OCRRegionCalibration()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(15)
            };

            _resizablePanel.AddTab("OCR Region Calibration", container);

            var titleLabel = new Label
            {
                Text = "OCR Region Calibration",
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
                ColumnCount = 5,
                RowCount = 2,
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
            labelInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            labelInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            labelInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));


            labelInfoLayout.Controls.Add(itemCodeLabel, 0, 0);

            _txtItemCode = new TextBox
            {
                Width = 250,
                Margin = new Padding(0, 5, 15, 5),
                ReadOnly = true
            };
            labelInfoLayout.Controls.Add(_txtItemCode, 1, 0);
            labelInfoLayout.SetColumnSpan(_txtItemCode, 3);



            var searchButton = new Button
            {
                Text = "Search",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),

                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text

                // remove borders
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(0, 150, 136),
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },

                Cursor = Cursors.Hand
            };
            labelInfoLayout.Controls.Add(searchButton, 4, 0);

            searchButton.Click += (sender, e) =>
            {
                SelectOcrItemRequested?.Invoke(this, EventArgs.Empty);
            };

            var revisionNoLabel = new Label
            {
                Text = "Revision No:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };

            labelInfoLayout.Controls.Add(revisionNoLabel, 0, 1);

            _txtRevisionNumber = new TextBox
            {
                Width = 50,
                Margin = new Padding(0, 5, 15, 5),
                ReadOnly = true,
                Dock = DockStyle.Left
            };
            labelInfoLayout.Controls.Add(_txtRevisionNumber, 1, 1);


            var boxTypeLabel = new Label
            {
                Text = "Box Type:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 5, 5)
            };
            labelInfoLayout.Controls.Add(boxTypeLabel, 2, 1);


            _txtBoxType = new TextBox
            {
                Width = 60,
                Margin = new Padding(0, 5, 15, 5),
                ReadOnly = true
            };
            labelInfoLayout.Controls.Add(_txtBoxType, 3, 1);


            _printSampleLabelButton = new Button
            {
                Text = "Print",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),

                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text

                // remove borders
                FlatAppearance =
                {              
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(0, 150, 136),
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },

                Cursor = Cursors.Hand
            };

            labelInfoLayout.Controls.Add(_printSampleLabelButton, 4, 1);

            _printSampleLabelButton.Click += (sender, e) => 
            {
                PrintLabelRequested?.Invoke(this, new PrintLabelEventArgs(_txtItemCode!.Text.Trim(), int.Parse(_txtRevisionNumber!.Text.Trim()), _txtBoxType!.Text.Trim(), _selectedFilePath));
            };

            labelInfoLayout.Paint += (s, e) =>
            {
                if (s is not TableLayoutPanel panel)
                    return;

                //using var pen = new Pen(Color.Gray, 1);
                using var pen = new Pen(Color.FromArgb(0, 166, 147), 2);
                int y = panel.ClientSize.Height - 1;

                e.Graphics.DrawLine(pen, 0, y, panel.ClientSize.Width, y);

            };


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
            var btnSaveRegion = new Button
            {
                Text = "Save Region",
                Width = 120,
                Height = 35,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),
                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(0, 150, 136),
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },
                Cursor = Cursors.Hand
            };
            buttonPanel.Controls.Add(btnSaveRegion, 0, 0);

            // OCR Read button
            _btnTestOcr = new Button
            {
                Text = "Run OCR",
                Width = 120,
                Height = 35,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),
                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(0, 150, 136),
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },
                Cursor = Cursors.Hand
            };
            buttonPanel.Controls.Add(_btnTestOcr, 1, 0);

            //sample data
            //_txtItemCode.Text = "SR*FF2032";
            //_txtRevisionNumber.Text = "1";
            //_txtBoxType.Text = "UB";
            //

            btnSaveRegion.Click += delegate
            {
                int revision = int.TryParse(_txtRevisionNumber.Text, out var r) ? r : 0;
                string boxType = _txtBoxType.Text.Trim();
                if (_roi.IsEmpty)
                {
                    if (int.TryParse(_txtX.Text, out var x) &&
                        int.TryParse(_txtY.Text, out var y) &&
                        int.TryParse(_txtWidth.Text, out var width) &&
                        int.TryParse(_txtHeight.Text, out var height))
                    {
                        _roi = new Rectangle(x, y, width, height);
                    }
                }

                SaveCalibrationClicked?.Invoke(this, new CalibrationEventArgs(_roi, picCameraPreview.Size, picCameraPreview.Image?.Size ?? Size.Empty, _txtItemCode.Text.Trim(), revision, boxType));
            };

            _btnTestOcr.Click += delegate
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
            _resizablePanel.ShowTab("OCR Region Calibration");

            _txtItemCode?.Focus();
        }

        private void ClearCoordinateFields()
        {
            _txtItemCode!.Text =
            _txtRevisionNumber!.Text =  
            _txtBoxType!.Text =
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
        public void ShowOCRRegion(Rectangle viewerRegion)
        {
            _ocrViewerRegion = viewerRegion;
        }

        public void PreviewOCRRegion(Rectangle viewerRegion)
        {
            _ocrPreviewRegion = viewerRegion;
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

        //public void SetReconnectCameraButtonVisibility(bool isVisible)
        //{
        //    _btnReconnectCamera!.Visible = isVisible;
        //}

        public void FinishCalibration(string message, bool isError = false)
        {
            _normalizedRegion = null;

            _roi = Rectangle.Empty;

            _ocrPreviewRegion = null;
            _ocrViewerRegion = null;

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
                p.RevisionNo.ToString(),
                p.BoxType,
                p.Coordinates.X.ToString(),
                p.Coordinates.Y.ToString(),
                p.Coordinates.Width.ToString(),
                p.Coordinates.Height.ToString(),
                p.Coordinates.ImageWidth.ToString(),
                p.Coordinates.ImageHeight.ToString(),
                p.RegisteredAt.ToString("yyyy-MM-dd HH:mm:ss")
            ]);
        }

        public void DisplayOCRResult(string result)
        {
            _richTextOCRResult!.AppendText(result + Environment.NewLine);
            _richTextOCRResult.ScrollToCaret();
            _ocrPreviewRegion = null;
            picCameraPreview.Invalidate();
        }

        public bool AskRestartConfirmation(string message, string title = "Restart Required")
        {
            throw new NotImplementedException();
        }

        public void SetCameraList(IEnumerable<string>? cameras)
        {
            _cameraSelectionComboBox!.Items.Clear();

            if (cameras == null || !cameras.Any())
                return;
            _cameraSelectionComboBox?.Items.AddRange(cameras.ToArray());
        }

        public void SetCameraResolutions(IEnumerable<string>? resolution)
        {
            _cameraResolutionComboBox?.Items.Clear();

            if (resolution == null || !resolution.Any()) return;
            _cameraResolutionComboBox?.Items.AddRange(resolution.ToArray());
        }

        public void SelectCamera(string cameraName, string resolution, int focus)
        {
            // Camera selection
            var index = _cameraSelectionComboBox!.FindStringExact(cameraName);
            if (index >= 0)
                _cameraSelectionComboBox.SelectedIndex = index;


            // Resolution selection (this one is OK because it uses Items)
            var resIndex = _cameraResolutionComboBox!.FindStringExact(resolution);
            if (resIndex >= 0)
                _cameraResolutionComboBox.SelectedIndex = resIndex;

            UpdateFocusUI(focus);

        }

        private void UpdateFocusUI(int value)
        {
            _focusCheckBox!.Checked = value != 0;

            value = Math.Max(_focusSlider!.Minimum,
                    Math.Min(_focusSlider.Maximum, value));

            _focusSlider.Value = value;

            _focusValueLabel!.Text = value.ToString();

            if (value == 0)
            {
                _focusLevelLabel!.Text = "Auto";
            }
            else
            {
                _focusLevelLabel!.Text = value.ToString();
                _focusOverlay!.Visible = true;
            }

            FocusValueChanged?.Invoke(this, value);
        }

        public void ShowCameraNotification(string message, string caption, bool isError = false)
        {
            if (isError)
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void TestOCRTCompleted()
        {
            picCameraPreview.Invalidate();
        }

        public void SetSelectedOcrItem(OcrSupportedItemDto selected, Coordinates? coordinates = null)
        {
            _txtItemCode!.Text = selected.ItemCode.Trim();
            _txtRevisionNumber!.Text = selected.RevisionNumber.ToString();
            _txtBoxType!.Text = selected.BoxType;
            _selectedFilePath = selected.FilePath;
            
            if (coordinates != null)
            {
                _txtX!.Text = coordinates.X.ToString();
                _txtY!.Text = coordinates.Y.ToString();
                _txtWidth!.Text = coordinates.Width.ToString();
                _txtHeight!.Text = coordinates.Height.ToString();
                _txtImgWidth!.Text = coordinates.ImageWidth.ToString();
                _txtImgHeight!.Text = coordinates.ImageHeight.ToString();
            }
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
