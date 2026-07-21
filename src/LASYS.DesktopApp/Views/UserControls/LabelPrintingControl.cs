using System.Windows.Forms;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.Devices.Enums;
using LASYS.Application.Features.Devices.Models;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class LabelPrintingControl : UserControl, ILabelPrintingView
    {
        private readonly Image _cameraOn = Properties.Resources.camera_on_24;
        private readonly Image _cameraOff = Properties.Resources.camera_off_24;

        private readonly Image _labelPreviewOn = Properties.Resources.label_on_24;
        private readonly Image _labelPreviewOff = Properties.Resources.label_off_24;

        private readonly Image _pauseIcon = Properties.Resources.pause24;
        private readonly Image _resumeIcon = Properties.Resources.resume24;
        private readonly Image _stopIcon = Properties.Resources.stopbatch24;
        private readonly Image _startIcon = Properties.Resources.play24;

        public event EventHandler? BackToWorkOrdersRequested;
        public event EventHandler<PrintRequestedEventArgs>? PrintRequested;

        public event EventHandler? PausePrintingRequested;
        public event EventHandler? ResumePrintingRequested;
        public event EventHandler? StopPrintingRequested;
        public event EventHandler? CameraPreviewRequested;
        public event EventHandler? LabelTemplatePreviewRequested;
        public event EventHandler? QuantityChanged;
        public event EventHandler? EndOfBatchChanged;

        private PrintJobStatus _currentJobStatus = PrintJobStatus.Initializing;

        private readonly DraggableResizerPanel _resizablePanel;

        private ListView? _logListView;

        private Label? _cameraStatus;
        private Label? _cameraDetails;

        private Label? _printerStatus;
        private Label? _printerDetails;

        private Label? _barcodeStatus;
        private Label? _barcodeDetails;

        public UserControl UserControl => this;

        public bool IsEndOfBatchChecked => chkEndOfBatch.Checked;
        public int Quantity => (int)nudQuantity.Value;

        private Guid? _printJobId;

        private readonly Panel _loadingCard;
        private readonly Panel _loadingAccent;
        private readonly Label _loadingLabel;
        private readonly ProgressBar _loadingProgress;

        private ModalOverlay _modalOverlay;
        public LabelPrintingControl()
        {
            InitializeComponent();

            _modalOverlay = new ModalOverlay(pnlContent);

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


            ActivityLogs();
            HardwareStatus();

            RegisterHideEvent(pnlContent);

            btnBack.Click += (_, _) => BackToWorkOrdersRequested?.Invoke(this, EventArgs.Empty);
            btnPrint.Click += (_, _) =>
            {
                if (_printJobId is null)
                    return;

                if (_currentJobStatus is PrintJobStatus.Ready or PrintJobStatus.Completed)
                {
                    PrintRequested?.Invoke(this, new PrintRequestedEventArgs(_printJobId.Value, (int)nudQuantity.Value, chkEndOfBatch.Checked));
                }

                if (_currentJobStatus is PrintJobStatus.InProgress or PrintJobStatus.Paused)
                {
                    StopPrintingRequested?.Invoke(this, EventArgs.Empty);
                }

            };


            btnPauseResume.Click += (_, _) =>
            {
                if (_currentJobStatus is PrintJobStatus.InProgress)
                {
                    PausePrintingRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (_currentJobStatus == PrintJobStatus.Paused)
                {
                    ResumePrintingRequested?.Invoke(this, EventArgs.Empty);
                }
            };


            btnCameraPreview.Image = _cameraOn;
            btnCameraPreview.Click += (_, _) => CameraPreviewRequested?.Invoke(this, EventArgs.Empty);

            btnLabelTemplatePreview.Image = _labelPreviewOn;
            btnLabelTemplatePreview.Click += (_, _) => LabelTemplatePreviewRequested?.Invoke(this, EventArgs.Empty);

            nudQuantity.ValueChanged += (_, _) => QuantityChanged?.Invoke(this, EventArgs.Empty);
            chkEndOfBatch.CheckedChanged += (_, _) => EndOfBatchChanged?.Invoke(this, EventArgs.Empty);

            //Loading card setup
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
                Text = "Initializing..."
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


        private void RegisterHideEvent(Control parent)
        {
            parent.MouseDown += HidePanelHandler;

            foreach (Control child in parent.Controls)
            {
                RegisterHideEvent(child);
            }

            parent.ControlAdded += (s, e) =>
            {
                RegisterHideEvent(e.Control!);
            };
        }
        private void HidePanelHandler(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (sender is Control ctrl)
            {
                Point p = pnlContent.PointToClient(ctrl.PointToScreen(e.Location));

                if (!_resizablePanel.Bounds.Contains(p))
                {
                    _resizablePanel.HidePanel();
                }
            }
        }

        private void ActivityLogs()
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8), // margin around content
                BackColor = Color.White
            };

            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Title
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Description
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Logs

            var title = new Label
            {
                Text = "Activity Logs",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 0, 5, 5),
                AutoSize = true
            };

            var description = new Label
            {
                Text = "Provides a detailed record of system activities and messages related to the label printing process, \nhelping users monitor the printing operation.",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                Padding = new Padding(5, 0, 5, 5),
                AutoSize = true
            };

            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 1, 1, 5) // margin around the ListView
            };

            _logListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                HideSelection = true,
            };

            _logListView.ItemSelectionChanged += (s, e) =>
            {
                e.Item!.Selected = false;
            };

            _logListView.Resize += (s, e) =>
            {
                int otherColumnsWidth =
                    _logListView.Columns[0].Width +
                    _logListView.Columns[1].Width;

                _logListView.Columns[2].Width =
                    _logListView.ClientSize.Width - otherColumnsWidth;
            };



            _logListView.Columns.Add("Date", 120);
            _logListView.Columns.Add("Time", 100);
            _logListView.Columns.Add("Message", 500);

            logPanel.Controls.Add(_logListView);

            container.Controls.Add(title, 0, 0);
            container.Controls.Add(description, 0, 1);
            container.Controls.Add(logPanel, 0, 2);

            _resizablePanel.AddTab("Activity Logs", container);
            _resizablePanel.ShowTab("Activity Logs", false);
        }

        private Color GetStatusColor(DeviceStatusCode status)
        {
            return status switch
            {
                DeviceStatusCode.Connected => Color.Green,
                DeviceStatusCode.Disconnected => Color.Red,
                DeviceStatusCode.Error => Color.Red,
                _ => Color.Black
            };
        }
        private void HardwareStatus()
        {
            Label CreateHeader(string text)
            {
                return new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true
                };
            }

            Label CreateCell(string text)
            {
                return new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5),
                    Font = new Font("Segoe UI", 9),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true
                };
            }

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(8),
                BackColor = Color.White
            };

            var title = new Label
            {
                Text = "Hardware Status",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Padding = new Padding(5, 0, 5, 10),
                AutoSize = true
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = 4,
                AutoSize = true,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };


            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));



            // Header
            table.Controls.Add(CreateHeader("Device"), 0, 0);
            table.Controls.Add(CreateHeader(""), 1, 0);
            table.Controls.Add(CreateHeader("Status"), 2, 0);
            table.Controls.Add(CreateHeader("Details"), 3, 0);

            //Camera
            _cameraStatus = CreateCell("Loading...");
            _cameraDetails = CreateCell("Loading...");
            table.Controls.Add(CreateCell("Camera"), 0, 1);
            table.Controls.Add(CreateCell(":"), 1, 1);
            table.Controls.Add(_cameraStatus, 2, 1);
            table.Controls.Add(_cameraDetails, 3, 1);

            //Printer
            _printerStatus = CreateCell("Loading...");
            _printerDetails = CreateCell("Loading...");
            table.Controls.Add(CreateCell("Printer"), 0, 2);
            table.Controls.Add(CreateCell(":"), 1, 2);
            table.Controls.Add(_printerStatus, 2, 2);
            table.Controls.Add(_printerDetails, 3, 2);

            //Barcode
            _barcodeStatus = CreateCell("Loading...");
            _barcodeDetails = CreateCell("Loading...");
            table.Controls.Add(CreateCell("Barcode Scanner"), 0, 3);
            table.Controls.Add(CreateCell(":"), 1, 3);
            table.Controls.Add(_barcodeStatus, 2, 3);
            table.Controls.Add(_barcodeDetails, 3, 3);

            //_cameraStatus.ForeColor = GetStatusColor(_cameraStatus.Text);

            //_printerStatus.ForeColor = GetStatusColor(_printerStatus.Text);

            //_barcodeStatus.ForeColor = GetStatusColor(_barcodeStatus.Text);

            container.Controls.Add(table);
            container.Controls.Add(title);
            _resizablePanel.AddTab("Hardware Status", container);
        }

        public void UpdateDeviceStatus(DeviceStatus status)
        {
            var text = status.Status.ToString();

            switch (status.Device)
            {
                case DeviceType.Camera:
                    _cameraStatus!.Text = text;
                    _cameraStatus.ForeColor = GetStatusColor(status.Status);
                    _cameraDetails!.Text = status.Description;
                    break;

                case DeviceType.Printer:
                    _printerStatus!.Text = text;
                    _printerStatus.ForeColor = GetStatusColor(status.Status);
                    _printerDetails!.Text = status.Description;
                    break;

                case DeviceType.BarcodeScanner:
                    _barcodeStatus!.Text = text;
                    _barcodeStatus.ForeColor = GetStatusColor(status.Status);
                    _barcodeDetails!.Text = status.Description;
                    break;
            }
        }

        public void HideError()
        {
            throw new NotImplementedException();
        }

        public void ShowError(ErrorForm errorForm)
        {
            errorForm.ControlBox = false;
            errorForm.Text = "Validation Error";
            _modalOverlay.Show(errorForm);

            //_modalOverlay.Show();

            //try
            //{
            //    errorForm.ControlBox = false;
            //    errorForm.Text = "Validation Error";
            //    errorForm.ShowDialog(_modalOverlay.Overlay);
            //}
            //finally
            //{
            //    _modalOverlay.Hide();
            //}

            //errorForm.ControlBox = false;
            //errorForm.Text = "Validation Error";

            //_modalOverlay.Show(errorForm);

            //// Force layout updates first
            //pnlContent.Update();

            //var panelBounds = new Rectangle(
            //    pnlContent.PointToScreen(Point.Empty),
            //    pnlContent.ClientSize);

            //var modalBackground = new Form
            //{
            //    StartPosition = FormStartPosition.Manual,
            //    FormBorderStyle = FormBorderStyle.None,
            //    Bounds = panelBounds,
            //    Opacity = 0.5,
            //    BackColor = Color.Black,
            //    ShowInTaskbar = false,
            //    Owner = FindForm()
            //};

            //try
            //{
            //    modalBackground.Show();
            //    modalBackground.Update();

            //    errorForm.Text = "Validation Error";
            //    errorForm.ControlBox = false;

            //    // Let WinForms center it relative to the overlay
            //    errorForm.StartPosition = FormStartPosition.CenterParent;

            //    errorForm.Invalidate(true);
            //    errorForm.Update();
            //    errorForm.ShowDialog(modalBackground);
            //}
            //finally
            //{
            //    modalBackground.Dispose();
            //}
        }

        public void InitializePrintingContext(PrintJobState printJob)
        {
            _printJobId = printJob.JobId;

            var context = printJob.Context;
            var labelInstruction = context.LabelInstructionDetails!;
            var product = context.ProductDetails!;
            var masterLabel = context.MasterLabelDetails!;
            var printDetails = context.PrintDetails!;

            lblInstructionCode.Text = labelInstruction.InstructionCode;
            lblItemCode.Text = labelInstruction.ItemCode;
            lblExpiryDate.Text = labelInstruction.ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A";
            lblLotNo.Text = labelInstruction.LotNo;

            lblLabelFile.Text = masterLabel.FilePath;

            lblCurrentSequence.Text = printDetails.NextSequence.ToString();
            lblBatchNumber.Text = printDetails.BatchNumber.ToString();
            lblSetNumber.Text = printDetails.SetNumber.ToString();


            lblTargetQuantity.Text = printJob.TargetQuantity.ToString();
            lblRemaining.Text = printJob.RemainingQuantity.ToString();
            lblLabelSample.Text = printDetails.TotalSample.ToString();
            lblTotalPrinted.Text = printDetails.TotalPrinted.ToString();
            lblTotalPassed.Text = printDetails.TotalPassed.ToString();
            lblTotalFailed.Text = printDetails.TotalFailed.ToString();


            UpdateQuantityControl(printJob);

        }

        public void UpdateQuantityControl(PrintJobState printJob)
        {
            var product = printJob.Context.ProductDetails!;
            var printDetails = printJob.Context.PrintDetails!;

            var batchSize = product.BatchSize;
            var currentBatchCount = printDetails.TotalPassed % batchSize;
            var remaining = printJob.RemainingQuantity;

            //var remainingInBatch = currentBatchCount == 0
            //    ? batchSize
            //    : batchSize - currentBatchCount;

            //var maxQty = Math.Min((long)remaining, remainingInBatch);

            //nudQuantity.Minimum = remaining == 0 ? 0 : 1;
            //nudQuantity.Maximum = (decimal)maxQty;
            //nudQuantity.Value = (decimal)maxQty;

            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = remaining;
            nudQuantity.Value = remaining < 50 ? remaining : 50;
        }

        private Color GetColor(MessageType type)
        {
            return type switch
            {
                MessageType.Info => Color.Black,
                MessageType.Warning => Color.DarkOrange,
                MessageType.Error => Color.Red,
                _ => Color.Black
            };
        }

        private void ClearLogs()
        {
            _logListView!.BeginUpdate();
            _logListView!.Items.Clear();
            _logListView!.EndUpdate();
        }
        public void AddLog(MessageType type, DateTime timeStamp, string message)
        {
            if (_logListView is null || _logListView.IsDisposed)
                return;

            const int MaxLogs = 1000;
            if (_logListView!.Items.Count >= MaxLogs)
            {
                _logListView.Items.RemoveAt(0);
            }

            var item = new ListViewItem(timeStamp.ToString("yyyy-MM-dd"));
            item.SubItems.Add(timeStamp.ToString("HH:mm:ss"));
            item.SubItems.Add(message);

            item.ForeColor = GetColor(type);

            _logListView.Items.Add(item);
            _logListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
            _logListView.Refresh();

            if (type == MessageType.Error || type == MessageType.Warning)
            {
                _resizablePanel.ShowTab("Activity Logs", false);
            }

            if (_logListView.Items.Count > 0)
                _logListView.TopItem = _logListView.Items[^1];
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


        public void SetPrintingState(PrintJobStatus status)
        {
            _currentJobStatus = status;
            switch (status)
            {
                case PrintJobStatus.Initializing:
                case PrintJobStatus.Printed:
                    btnPauseResume.Visible = false;
                    btnPrint.Visible = false;

                    lblPrintingProgress.Visible = false;
                    pbPrintingProgress.Visible = false;
                    break;
                case PrintJobStatus.Ready:
                case PrintJobStatus.Completed:
                    lblPrintingProgress.Visible = false;
                    pbPrintingProgress.Visible = false;

                    btnPauseResume.Visible = false;
                    btnPrint.Visible = true;
                    btnPrint.Text = "Start Print";
                    btnPrint.Image = _startIcon;
                    btnPrint.ForeColor = Color.Black;
                    btnPrint.BackColor = Color.SeaGreen;
                    break;
                case PrintJobStatus.Pending:
                    btnPrint.Enabled = false;
                    ClearLogs();
                    break;
                case PrintJobStatus.InProgress:
                    lblPrintingProgress.Visible = true;
                    pbPrintingProgress.Visible = true;
                    btnPauseResume.Visible = true;
                    btnPauseResume.Text = "Pause";
                    btnPauseResume.Image = _pauseIcon;
                    btnPauseResume.BackColor = Color.DarkOrange;

                    btnPrint.Enabled = true;
                    btnPrint.Visible = true;
                    btnPrint.Text = "Stop Print";
                    btnPrint.Image = _stopIcon;
                    btnPrint.ForeColor = Color.White;
                    btnPrint.BackColor = Color.Crimson;
                    break;
                case PrintJobStatus.Paused:
                    btnPauseResume.Visible = true;
                    btnPauseResume.Text = "Resume";
                    btnPauseResume.Image = _resumeIcon;
                    btnPauseResume.BackColor = Color.SeaGreen;
                    break;
                default:
                    break;
            }

        }

        public void UpdateProgress(int printedCount, int totalQuantity)
        {
            lblPrintingProgress.Text = $"{printedCount} / {totalQuantity}";
            pbPrintingProgress.Minimum = 0;
            pbPrintingProgress.Maximum = totalQuantity;
            pbPrintingProgress.Value = Math.Min(printedCount, totalQuantity);
        }

        public void ToggleActivityLogs()
        {
            _resizablePanel.ShowTab("Activity Logs", true);
            _logListView?.Invalidate();
            _logListView?.Update();
            _logListView?.Refresh();
        }


        public void UpdatePrintingResults(uint targetQuantity, long setNumber, long batchNumber, long startSequence, long remaining, long totalPrinted, long totalPassed, long totalFailed, long labelSample)
        {
            //if(remaining == 0)
            //{
            //    nudQuantity.Minimum = 0;
            //    nudQuantity.Maximum = 0;
            //    nudQuantity.Value = 0;
            //}

            lblTargetQuantity.Text = targetQuantity.ToString();
            lblSetNumber.Text = setNumber.ToString();
            lblBatchNumber.Text = batchNumber.ToString();
            lblCurrentSequence.Text = startSequence.ToString();
            lblRemaining.Text = remaining.ToString();
            lblTotalPrinted.Text = totalPrinted.ToString();
            lblTotalPassed.Text = totalPassed.ToString();
            lblTotalFailed.Text = totalFailed.ToString();
            lblLabelSample.Text = labelSample.ToString();
        }

        public void SetPreview(UserControl control)
        {
            control.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            //control.Size = new Size(320, 240);

            control.Location = new Point(btnCameraPreview.Left - control.Width - 5, pnlContent.Top + 5);

            Controls.Add(control);

            control.BringToFront();

            control.Visible = false;
        }

        public void ToggleCameraPreview(bool visible)
        {
            var cameraPreview = Controls.OfType<CameraPreviewControl>().FirstOrDefault();
            var labelPreview = Controls.OfType<LabelTemplatePreviewControl>().FirstOrDefault();

            if (cameraPreview == null)
                return;

            bool showCamera = !cameraPreview.Visible;

            // Hide label preview whenever camera button is clicked
            if (labelPreview != null)
                labelPreview.Visible = false;

            cameraPreview.Visible = showCamera;

            btnCameraPreview.Image = showCamera
                ? _cameraOff
                : _cameraOn;

            btnLabelTemplatePreview.Image = _labelPreviewOn;
        }

        public void ToggleLabelTemplatePreview(bool visible)
        {
            var cameraPreview = Controls.OfType<CameraPreviewControl>().FirstOrDefault();
            var labelPreview = Controls.OfType<LabelTemplatePreviewControl>().FirstOrDefault();

            if (labelPreview == null)
                return;

            bool showLabel = !labelPreview.Visible;

            // Hide camera preview whenever label button is clicked
            if (cameraPreview != null)
                cameraPreview.Visible = false;

            labelPreview.Visible = showLabel;

            btnLabelTemplatePreview.Image = showLabel
                ? _labelPreviewOff
                : _labelPreviewOn;

            btnCameraPreview.Image = _cameraOn;
        }
        public void SetLoading(bool isLoading)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetLoading(isLoading)));
                return;
            }

            _loadingCard.Visible = isLoading;
            if (isLoading)
            {
                LayoutLoadingCard();
                _loadingCard.BringToFront();
            }
        }
        private void LayoutLoadingCard()
        {
            if (this.ClientSize.Width <= 0 || this.ClientSize.Height <= 0 || _loadingCard == null)
            {
                return;
            }

            var contentHeight = _loadingCard.Height;
            var top = Math.Max(0, (this.Top + (this.Height - contentHeight) / 2));
            var left = Math.Max(0, (this.Left + (this.Width - _loadingCard.Width) / 2));

            _loadingCard.Left = left;
            _loadingCard.Top = top;

            _loadingLabel.Left = Math.Max(0, (_loadingCard.Width - _loadingLabel.Width) / 2);
            _loadingLabel.Top = 28;

            _loadingProgress.Left = Math.Max(0, (_loadingCard.Width - _loadingProgress.Width) / 2);
            _loadingProgress.Top = _loadingLabel.Bottom + 16;
        }

        public void SetBackButtonEnabled(bool enabled)
        {
            btnBack.Enabled = enabled;
        }

        public void ResetView()
        {
            var loadingText = "Initializing...";
            lblInstructionCode.Text = loadingText;
            lblItemCode.Text = loadingText;
            lblExpiryDate.Text = loadingText;
            lblLotNo.Text = loadingText;

            lblLabelFile.Text = loadingText;

            lblCurrentSequence.Text = "...";
            lblBatchNumber.Text = "...";
            lblSetNumber.Text = "...";


            lblTargetQuantity.Text = "...";
            lblRemaining.Text = "...";
            lblLabelSample.Text = "...";
            lblTotalPrinted.Text = "...";
            lblTotalPassed.Text = "...";
            lblTotalFailed.Text = "...";

            nudQuantity.Minimum = 0;
            nudQuantity.Maximum = 0;
            nudQuantity.Value = 0;
            chkEndOfBatch.Checked = false;
            ClearLogs();
        }

        public void ShowApprovalAuthorization(ApprovalAuthenticationForm approvalForm)
        {
            approvalForm.ControlBox = false;
            _modalOverlay.Show(approvalForm);


            //pnlContent.Update();

            //var panelBounds = new Rectangle(
            //    pnlContent.PointToScreen(Point.Empty),
            //    pnlContent.ClientSize);

            //var modalBackground = new Form
            //{
            //    StartPosition = FormStartPosition.Manual,
            //    FormBorderStyle = FormBorderStyle.None,
            //    Bounds = panelBounds,
            //    Opacity = 0.5,
            //    BackColor = Color.Black,
            //    ShowInTaskbar = false,
            //    Owner = FindForm()
            //};

            //try
            //{
            //    modalBackground.Show();
            //    modalBackground.Update();
            //    approvalForm.ControlBox = false;

            //    // Let WinForms center it relative to the overlay
            //    approvalForm.StartPosition = FormStartPosition.CenterParent;

            //    //approvalForm.Invalidate(true);
            //    //approvalForm.Update();
            //    approvalForm.ShowDialog(modalBackground);

            //}
            //finally
            //{
            //    modalBackground.Dispose();
            //}
        }

        public void HideModal()
        {
            _modalOverlay.HideOverlay();
        }

        public void ShowNotification(string message, MessageBoxIcon icon)
        {
           MessageBox.Show(message, "Notification", MessageBoxButtons.OK, icon);
        }

        public void SetEndOfBatch(bool isChecked)
        {
            chkEndOfBatch.Checked = isChecked;
        }
    }

}
