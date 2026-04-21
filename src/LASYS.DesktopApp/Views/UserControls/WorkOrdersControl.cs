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
        public event EventHandler<string>? SearchTermChanged;
        private readonly GridViewWithPagination _gridWithPagination;
        private readonly Panel _loadingCard;
        private readonly Panel _loadingAccent;
        private readonly Label _loadingLabel;
        private readonly ProgressBar _loadingProgress;
        private static readonly Func<GetWorkOrdersResult, string[]> _workOrderRowSelector = ProjectWorkOrderRow;

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

            _gridWithPagination.SearchTermChanged += (sender, e) =>
            {
                SearchTermChanged?.Invoke(this, e);
            };

            _gridWithPagination.Resize += (_, _) => LayoutLoadingCard();

            // Enable external data mode for pagination (presenter handles paging)
            _gridWithPagination.SetExternalDataMode(true);

            pnlContent.Controls.Add(_gridWithPagination);

            _loadingCard = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Width = 320,
                Height = 110,
                Visible = false
            };

            _loadingAccent = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = Color.FromArgb(0, 110, 100)
            };

            _loadingLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 110, 100),
                Text = "Loading work orders..."
            };

            _loadingProgress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Width = 240,
                Height = 18,
                ForeColor = Color.FromArgb(0, 140, 125)
            };

            _loadingCard.Controls.Add(_loadingAccent);
            _loadingCard.Controls.Add(_loadingLabel);
            _loadingCard.Controls.Add(_loadingProgress);

            pnlContent.Controls.Add(_loadingCard);
            pnlContent.Controls.SetChildIndex(_loadingCard, 0);
            pnlContent.Resize += (_, _) => LayoutLoadingCard();
            LayoutLoadingCard();
        }

        public void ShowNotification(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        public void SetLoading(bool isLoading)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetLoading(isLoading)));
                return;
            }

            _gridWithPagination.Enabled = !isLoading;
            _loadingCard.Visible = isLoading;
            if (isLoading)
            {
                LayoutLoadingCard();
                _loadingCard.BringToFront();
            }
        }

        public void SetWorkOrders(List<GetWorkOrdersResult> workOrders, int totalPages)
        {
            _gridWithPagination.SetTotalPages(totalPages);
            _gridWithPagination.SetRows(workOrders, _workOrderRowSelector);
        }

        private static string[] ProjectWorkOrderRow(GetWorkOrdersResult item)
        {
            var itemCode = item.ItemCode ?? string.Empty;
            var lotNo = item.LotNo ?? string.Empty;
            var expDate = item.ExpDate ?? string.Empty;
            var printType = item.PrintType ?? string.Empty;
            var verdict = item.Verdict ?? string.Empty;
            var dateApproved = item.DateApproved ?? string.Empty;
            var ubStatus = item.UB_LI_Status ?? string.Empty;
            var aubStatus = item.AUB_LI_Status ?? string.Empty;
            var oubStatus = item.OUB_LI_Status ?? string.Empty;

            var row = new string[15];
            row[0] = itemCode;
            row[1] = lotNo;
            row[2] = expDate;
            row[3] = printType;
            row[4] = verdict;
            row[5] = dateApproved;
            row[6] = item.ProdQty.ToString();
            row[7] = item.MasterLabelRevisionNo.ToString();
            row[8] = item.LabelInsRevisionNo.ToString();
            row[9] = item.UB_Qty.ToString();
            row[10] = ubStatus;
            row[11] = item.AUB_Qty.ToString();
            row[12] = aubStatus;
            row[13] = item.OUB_Qty.ToString();
            row[14] = oubStatus;
            return row;
        }

        private void LayoutLoadingCard()
        {
            if (pnlContent.ClientSize.Width <= 0 || pnlContent.ClientSize.Height <= 0 || _loadingCard == null)
            {
                return;
            }

            var contentHeight = _loadingCard.Height;
            var top = Math.Max(0, (_gridWithPagination.Top + (_gridWithPagination.Height - contentHeight) / 2));
            var left = Math.Max(0, (_gridWithPagination.Left + (_gridWithPagination.Width - _loadingCard.Width) / 2));

            _loadingCard.Left = left;
            _loadingCard.Top = top;

            _loadingLabel.Left = Math.Max(0, (_loadingCard.Width - _loadingLabel.Width) / 2);
            _loadingLabel.Top = 28;

            _loadingProgress.Left = Math.Max(0, (_loadingCard.Width - _loadingProgress.Width) / 2);
            _loadingProgress.Top = _loadingLabel.Bottom + 16;
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

