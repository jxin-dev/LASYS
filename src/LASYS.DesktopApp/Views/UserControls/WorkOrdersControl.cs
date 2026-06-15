using LASYS.Application.Common.Enums;
using LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WorkOrdersControl : UserControl, IWorkOrdersView
    {
        public event EventHandler<LabelPrintingRequestedEventArgs>? LabelPrintingRequested;
        private readonly GridViewWithPagination _gridWithPagination;
        private readonly Panel _loadingCard;
        private readonly Panel _loadingAccent;
        private readonly Label _loadingLabel;
        private readonly ProgressBar _loadingProgress;
        public WorkOrdersControl()
        {
            InitializeComponent();
            _gridWithPagination = new GridViewWithPagination
            {
                PageSize = 10,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _gridWithPagination.SetMergedHeaders(
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
                [
                    "Item Code", "Lot No", "Exp. Date", "Print Type", "Verdict", "Date Approved", "Prod Qty",
                    "Master Label Revision No.", "Label Ins. Revision No.",
                    "Qty", "Status",
                    "Qty", "Status",
                    "Qty", "Status"
                ]
            );

            _gridWithPagination.SetColumnWidths(
                100, 100, 100, 100, 100, 100, 100, 100, 100, 50, 100, 50, 100, 50, 100
            );

            _gridWithPagination.RowDoubleClicked += (sender, e) =>
            {
                if (e is WorkOrderItem labelInstruction)
                {
                    LabelPrintingRequested?.Invoke(this, new LabelPrintingRequestedEventArgs(labelInstruction));
                }
            };

            _gridWithPagination.Resize += (_, _) => LayoutLoadingCard();

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

      

        public void SetWorkOrders(List<WorkOrderItem> workOrders, int totalPages)
        {
            _gridWithPagination.PageSize = 10;
            _gridWithPagination.SetExternalDataMode(false);
            _gridWithPagination.SetTotalPages(totalPages);
            _gridWithPagination.SetRows(workOrders,
                 x =>
                 {
                     var ub = x.Details?.GetValueOrDefault(BoxType.UnitBox);
                     var aub = x.Details?.GetValueOrDefault(BoxType.AdditionalUnitBox);
                     var oub = x.Details?.GetValueOrDefault(BoxType.OuterUnitBox);
                     return 
                     [
                        x.ItemCode,
                        x.LotNo,
                        x.ExpirationDate?.ToString("yyyy-MM-dd")  ?? string.Empty,
                        ub?.PrintType ?? string.Empty,
                        ub?.Verdict ?? string.Empty,
                        ub?.DateApproved?.ToString("yyyy-MM-dd") ?? string.Empty,
                        x.TargetProductionQuantity.ToString(),
                        x.MasterLabelRevNumber.ToString(),
                        x.LabelInsRevNumber.ToString(),
                        ub?.TargetPrintQuantity?.ToString() ?? string.Empty,
                        ub?.InstructionStatus?.ToString() ?? string.Empty,
                        aub?.TargetPrintQuantity?.ToString() ?? string.Empty,
                        aub?.InstructionStatus?.ToString() ?? string.Empty,
                        oub?.TargetPrintQuantity?.ToString() ?? string.Empty,
                        oub?.InstructionStatus?.ToString() ?? string.Empty
                     ];
                 });

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

