using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class OcrItemLookupForm : Form, IOcrItemLookupView
    {
        private GridViewWithPagination _gridWithPagination;
        public OcrItemLookupForm()
        {
            InitializeComponent();

            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 5,
                Dock = DockStyle.Fill,
            };

            _gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Revision", ColSpan = 1 },
                    new HeaderColumn { Text = "Box Type", ColSpan = 1 },
                ],
                ["Item Code", "Revision", "Box Type"]
            );

            _gridWithPagination.SetColumnWidths(200, 80, 80);

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {

            };

            pnlContainer.Controls.Add(_gridWithPagination);


            btnClose.Click += (s, e) => this.Close();
        }
    }
}
