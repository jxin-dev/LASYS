using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WorkOrdersControl : UserControl, IWorkOrdersView
    {
        public event EventHandler<LabelPrintingRequestedEventArgs>? LabelPrintingRequested;
        public WorkOrdersControl()
        {
            InitializeComponent();
            var gridWithPagination = new GridViewWithPagination
            {
                PageSize = 5,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            gridWithPagination.SetMergedHeaders(
                [
                    new HeaderColumn { Text = "Item Code", ColSpan = 1 },
                    new HeaderColumn { Text = "Lot No", ColSpan = 1 },
                    new HeaderColumn { Text = "Exp. Date", ColSpan = 1 },
                    new HeaderColumn { Text = "Print Type", ColSpan = 1 },
                    new HeaderColumn { Text = "Verdict", ColSpan = 1 },
                    new HeaderColumn { Text = "Date Approved", ColSpan = 1 },
                    new HeaderColumn { Text = "Prod Qty", ColSpan = 1 },
                    new HeaderColumn { Text = "Master Label Revision No.", ColSpan = 1 },
                    new HeaderColumn { Text = "Label Ins. Revision No.", ColSpan = 1 },
                    new HeaderColumn { Text = "UNIT BOX", ColSpan = 2 },
                    new HeaderColumn { Text = "ADDTNL UNIT BOX", ColSpan = 2 },
                    new HeaderColumn { Text = "OUTR UNIT BOX", ColSpan = 2 },

                ],
                ["Item Code", "Lot No", "Exp. Date", "Print Type", "Verdict", "Date Approved", "Prod Qty", "Master Label Revision No.", "Label Ins. Revision No.",
                "Qty", "Status",
                "Qty", "Status",
                "Qty", "Status"]
            );

            gridWithPagination.SetColumnWidths(
                100, 100, 100, 100, 100, 100, 100, 100, 100, 50, 100, 50, 100, 50, 100
            );

            // Suppose this is from your DB
            List<SampleData> results = LoadFromDatabase();

            gridWithPagination.SetRows(results, item =>
            [
                item.ItemCode,
                item.LotNo,
                item.ExpDate,
                item.PrintType,
                item.Verdict,
                item.DateApproved,
                item.ProductQty.ToString(),
                item.MasterLabelRevNo.ToString(),
                item.LabelInsRevNo.ToString(),
                item.UnitBoxQty.ToString(), item.UnitBoxStatus,
                item.AddtnlUnitBoxQty.ToString(), item.AddtnlUnitBoxStatus,
                item.OuterUnitBoxQty.ToString(), item.OuterUnitBoxStatus
            ]);



            gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                // Handle row double-click event
                if (e is SampleData data)
                {
                    LabelPrintingRequested?.Invoke(this, new LabelPrintingRequestedEventArgs(data.Id));

                }

            };

            pnlContent.Controls.Add(gridWithPagination);


            List<SampleData> LoadFromDatabase()
            {
                // Simulate loading data from a database
                return new List<SampleData>
                {
                    new SampleData(1, "SR+OX2051C1", "250529SG", "2030-04-30", "Original", "Approved", "05/29/2025",180000,6,1,"3600","For Printing","","","",""),
                    new SampleData(2, "SR+OX2051C2", "250529SG", "2030-04-30", "Original", "Approved", "05/29/2025",180000,6,1,"3600","For Printing","","","",""),
                    new SampleData(3, "SR+OX2051C3", "250529SG", "2030-04-30", "Original", "Approved", "05/29/2025",180000,6,1,"3600","For Printing","","","",""),
                };
            }
        }

    }

    public record SampleData(
        int Id,
        string ItemCode,
        string LotNo,
        string ExpDate,
        string PrintType,
        string Verdict,
        string DateApproved,
        int ProductQty,
        int MasterLabelRevNo,
        int LabelInsRevNo,
        string UnitBoxQty, string UnitBoxStatus,
        string AddtnlUnitBoxQty, string AddtnlUnitBoxStatus,
        string OuterUnitBoxQty, string OuterUnitBoxStatus
    );

}

