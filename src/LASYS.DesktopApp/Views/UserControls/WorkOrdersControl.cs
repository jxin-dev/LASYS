using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WorkOrdersControl : UserControl, IWorkOrdersView
    {
        public event EventHandler<LabelPrintingRequestedEventArgs>? LabelPrintingRequested;
        public event EventHandler<int>? PageNoChanged;
        private readonly GridViewWithPagination _gridWithPagination;

        public WorkOrdersControl()
        {
            InitializeComponent();
            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 50,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _gridWithPagination.SetMergedHeaders(
                new HeaderColumn[]
                {
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
                },
                new string[]
                {
                    "Item Code", "Lot No", "Exp. Date", "Print Type", "Verdict", "Date Approved", "Prod Qty",
                    "Master Label Revision No.", "Label Ins. Revision No.",
                    "Qty", "Status",
                    "Qty", "Status",
                    "Qty", "Status"
                }
            );

            _gridWithPagination.SetColumnWidths(
                100, 100, 100, 100, 100, 100, 100, 100, 100, 50, 100, 50, 100, 50, 100
            );

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                // e is the row object passed by the grid
                if (e is GetWorkOrdersResult data)
                {
                    // Show box type selection dialog on double-click
                    //using var dlg = new BoxTypeSelectionDialog();
                    //var owner = FindForm();
                    //var dr = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();

                    //if (dr == DialogResult.OK)
                    //{
                        // Pass item code and lot number so label printing view can be populated
                        LabelPrintingRequested?.Invoke(this, new LabelPrintingRequestedEventArgs(
                            data.ItemCode ?? string.Empty,
                            data.LotNo ?? string.Empty,
                            string.Empty,
                            LASYS.Application.Common.Enums.BoxType.NotSet,
                            data.PrintType,
                            data.UB_LI_Code,
                            data.AUB_LI_Code,
                            data.OUB_LI_Code,
                            data.CB_LI_Code,
                            data.ACB_LI_Code,
                            data.OCB_LI_Code));
                    //}
                }
            };

            _gridWithPagination.PageNoChanged += (sender, e) =>
            {
                PageNoChanged?.Invoke(this, e);
            };

            // Enable external data mode for pagination (presenter handles paging)
            _gridWithPagination.SetExternalDataMode(true);

            pnlContent.Controls.Add(_gridWithPagination);
        }

        public void ShowNotification(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        public void SetWorkOrders(List<GetWorkOrdersResult> workOrders, int totalPages)
        {
            _gridWithPagination.SetTotalPages(totalPages);
            _gridWithPagination.SetRows(workOrders, item => new string[]
            {
                item.ItemCode!,
                item.LotNo!,
                item.ExpDate!,
                item.PrintType!,
                item.Verdict!,
                item.DateApproved!,
                item.ProdQty.ToString(),
                item.MasterLabelRevisionNo.ToString(),
                item.LabelInsRevisionNo.ToString(),
                item.UB_Qty.ToString(), item.UB_LI_Status!,
                item.AUB_Qty.ToString(), item.AUB_LI_Status!,
                item.OUB_Qty.ToString(), item.OUB_LI_Status!
            });
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

