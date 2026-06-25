using LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class OcrItemLookupForm : Form, IOcrItemLookupView
    {
        private GridViewWithPagination _gridWithPagination;
        public event Func<Task>? ViewLoaded;
        public event Action<OcrSupportedItemDto>? ItemSelected;
        public event EventHandler<SampleLabelPrintingRequestedEventArgs>? LabelPrintingRequested;

        public OcrItemLookupForm()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;

            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 10,
                Dock = DockStyle.Fill,
            };

            _gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Instruction Label Revision", ColSpan = 1 },
                    new HeaderColumn { Text = "Master Label Revision", ColSpan = 1 }
                ],
                ["Item Code", "Instruction Label Revision", "Master Label Revision"]
            );

            _gridWithPagination.SetColumnWidths(200, 150, 150);

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                if (e is OcrSupportedItemDto labelInstruction)
                {
                    LabelPrintingRequested?.Invoke(this, new SampleLabelPrintingRequestedEventArgs(labelInstruction));
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

        //public void DisplayItems(List<OcrSupportedItemDto> items)
        //{
        //    _gridWithPagination.SetRows(items, item => new[]
        //    {
        //        item.ItemCode,
        //        item.RevisionNumber.ToString(),
        //        item.BoxType,
        //        item.FilePath
        //    });
        //}

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void DisplayItems(List<OcrSupportedItemDto> items, int totalPages)
        {
            _gridWithPagination.PageSize = 10;
            _gridWithPagination.SetExternalDataMode(false);
            _gridWithPagination.SetTotalPages(totalPages);
            _gridWithPagination.SetRows(items,
                 x =>
                 {
                     return
                     [
                        x.ItemCode,
                        x.LabelInstructionRevNumber.ToString(),
                        x.MasterLabelRevNumber.ToString()
                     ];
                 });
        }


        public void InvokeOnUI(Action action)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
    }
}
