using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class OcrItemLookupForm : Form, IOcrItemLookupView
    {
        private GridViewWithPagination _gridWithPagination;
        public event Func<Task>? ViewLoaded;
        public event Action<OcrSupportedItemDto>? ItemSelected;

        public OcrItemLookupForm()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;

            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 2,
                Dock = DockStyle.Fill,
            };

            _gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Revision", ColSpan = 1 },
                    new HeaderColumn { Text = "Box Type", ColSpan = 1 },
                    new HeaderColumn { Text = "File Path", ColSpan = 1 },

                ],
                ["Item Code", "Revision", "Box Type", "File Path"]
            );

            _gridWithPagination.SetColumnWidths(100, 80, 80, 400);

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                if (e is OcrSupportedItemDto item)
                {
                    ItemSelected?.Invoke(item);
                }
            };

            pnlContainer.Controls.Add(_gridWithPagination);


            btnClose.Click += (s, e) => this.Close();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (ViewLoaded != null)
            {
                await ViewLoaded.Invoke();
            }
        }

        public void DisplayItems(List<OcrSupportedItemDto> items)
        {
            _gridWithPagination.SetRows(items, item => new[]
            {
                item.ItemCode,
                item.RevisionNumber.ToString(),
                item.BoxType,
                item.FilePath
            });
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
