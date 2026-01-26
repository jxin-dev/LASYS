using LASYS.Application.Services;
using LASYS.Domain.OCR;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class OCRCalibrationControl : UserControl
    {
        private readonly DraggableResizerPanel _resizablePanel;

        private readonly OCRConfigService _ocrService;

        public OCRCalibrationControl(OCRConfigService ocrService)
        {
            _ocrService = ocrService;
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            _resizablePanel = new DraggableResizerPanel
            {
                HeightPercentage = 0.7,
                MinimumPanelHeight = 350,
                DefaultPanelHeight = 400
            };

            pnlContent.Controls.Add(_resizablePanel);
            LoadRegisteredItem();



        }

        private async void LoadRegisteredItem()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _resizablePanel.AddTab("Registered Item", container);

            var gridWithPagination = new GridViewWithPagination
            {
                PageSize = 5,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Coordinates", ColSpan = 6 },
                    new HeaderColumn { Text = "Date Registered", ColSpan = 1 }
                ],
                ["Item Code", "X", "Y", "Width", "Height", "Image Width", "Image Height", "Date Registered"]
            );

            gridWithPagination.SetColumnWidths(
                150, 80, 80, 80, 80, 100, 100, 150
            );
           
            // Load and show in grid
            var config = await _ocrService.LoadAsync();
            gridWithPagination.SetRows(config.Products, p =>
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


            var newProduct = new Product
            {
                ItemCode = "SR+OX2051C1",
                Coordinates = new Coordinates { X = 640, Y = 376, Width = 48, Height = 18, ImageWidth = 1097, ImageHeight = 611 },
                RegisteredAt = DateTime.Now
            };

            //await _ocrService.AddOrUpdateProductAsync(newProduct);

            // Remove example
            //await _ocrService.RemoveProductAsync("SR+OX2051C1");


            gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                // Handle row double-click event
                if (e is Product data)
                {
                    MessageBox.Show($"Item Code: {data.ItemCode}");
                }
            };

            container.Controls.Add(gridWithPagination);

        }
    }

}
