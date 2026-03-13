using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Messaging;
using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class LabelPrintingControl : UserControl, ILabelPrintingView
    {
        //private readonly CustomProgressBar _progressBar;
        private readonly Image _pauseIcon = Properties.Resources.pause24;
        private readonly Image _resumeIcon = Properties.Resources.resume24;
        private readonly Image _stopIcon = Properties.Resources.stopbatch24;
        private readonly Image _startIcon = Properties.Resources.play24;

        public event EventHandler? BackToWorkOrdersRequested;
        public event EventHandler? PrintRequested;
        public event EventHandler? PausePrintingRequested;
        public event EventHandler? ResumePrintingRequested;
        public event EventHandler? StopPrintingRequested;

        private PrintingState _currentState = PrintingState.Initializing;

        private readonly DraggableResizerPanel _resizablePanel;

        private ListView? _logListView;

        private Label? _cameraStatus;
        private Label? _cameraDetails;

        private Label? _printerStatus;
        private Label? _printerDetails;

        private Label? _barcodeStatus;
        private Label? _barcodeDetails;

        public UserControl UserControl => this;

        public LabelPrintingControl()
        {
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


            ActivityLogs();
            HardwareStatus();
            //_progressBar = new CustomProgressBar
            //{
            //    Dock = DockStyle.Bottom,
            //    ProgressColor = Color.FromArgb(255, 220, 20, 60),
            //    ProgressBackgroundColor = Color.LightGray,
            //    ShowPercentage = false
            //};

            //pnlLoadingContainer.Controls.Add(_progressBar);
            //UpdateProgress(50, "Printing Progress");


            RegisterHideEvent(pnlContent);

            btnBack.Click += (_, _) => BackToWorkOrdersRequested?.Invoke(this, EventArgs.Empty);

            btnPrint.Click += (_, _) =>
            {
                if (_currentState == PrintingState.Idle)
                {
                    PrintRequested?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    StopPrintingRequested?.Invoke(this, EventArgs.Empty);
                }
            };


            btnPauseResume.Click += (_, _) =>
            {
                if (_currentState == PrintingState.Printing)
                {
                    PausePrintingRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (_currentState == PrintingState.Paused)
                {
                    ResumePrintingRequested?.Invoke(this, EventArgs.Empty);
                }
            };

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

            _logListView.OwnerDraw = true;

            _logListView.DrawColumnHeader += (s, e) =>
            {
                using var font = new Font("Segoe UI", 9, FontStyle.Bold);
                e.Graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);
                TextRenderer.DrawText(
                    e.Graphics,
                    e.Header!.Text,
                    font,
                    e.Bounds,
                    Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };

            _logListView.DrawItem += (s, e) => e.DrawDefault = true;
            _logListView.DrawSubItem += (s, e) => e.DrawDefault = true;

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
        }

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Configuring..." => Color.Green,
                "Reconnecting..." => Color.Green,
                "Printing..." => Color.Green,
                "Resuming..." => Color.Green,
                "Connecting..." => Color.Green,
                "Scanning..." => Color.Green,
                "Communicating..." => Color.Green,

                "Connected" => Color.Green,
                "Ready" => Color.Green,
                "Print Completed" => Color.Green,

                "Disconnected" => Color.Red,
                "Timeout" => Color.Red,
                "Not Detected" => Color.Red,
                "Error" => Color.Red,

                "Not Configured" => Color.Red,
                "Offline" => Color.Red,
                "Print Failed" => Color.Red,

                "Paused" => Color.Black,
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

            _cameraStatus.ForeColor = GetStatusColor(_cameraStatus.Text);

            _printerStatus.ForeColor = GetStatusColor(_printerStatus.Text);

            _barcodeStatus.ForeColor = GetStatusColor(_barcodeStatus.Text);

            container.Controls.Add(table);
            container.Controls.Add(title);
            _resizablePanel.AddTab("Hardware Status", container);
        }

        public void UpdateCameraStatus(string status, string details)
        {
            _cameraStatus!.Text = status;
            _cameraStatus.ForeColor = GetStatusColor(status);
            _cameraDetails!.Text = details;
        }

        public void UpdatePrinterStatus(string status, string details)
        {
            _printerStatus!.Text = status;
            _printerStatus.ForeColor = GetStatusColor(status);
            _printerDetails!.Text = details;
        }
        public void UpdateBarcodeStatus(string status, string details)
        {
            _barcodeStatus!.Text = status;
            _barcodeStatus.ForeColor = GetStatusColor(status);
            _barcodeDetails!.Text = details;
        }

        public void HideError()
        {
            throw new NotImplementedException();
        }

        public void ShowError(ErrorForm errorForm)
        {

            // Create a semi-transparent overlay
            var modalBackground = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.5, // 50% transparent
                BackColor = Color.Black,
                Size = pnlContent.Size,
                Location = pnlContent.PointToScreen(Point.Empty),
                ShowInTaskbar = false,
                Owner = this.FindForm()
            };

            modalBackground.Show();

            errorForm.Text = "Validation Error";
            var containerScreenLocation = pnlContent.PointToScreen(Point.Empty);
            int x = containerScreenLocation.X + (pnlContent.Width - errorForm.Width) / 2;
            int y = containerScreenLocation.Y + (pnlContent.Height - errorForm.Height) / 2;
            errorForm.StartPosition = FormStartPosition.Manual;
            errorForm.Location = new Point(x, y);
            errorForm.ControlBox = false;// Removes the close button (Alt+F4 still works unless handled)
            //errorForm.TopMost = true;
            //errorForm.BringToFront();
            try
            {
                errorForm.ShowDialog(modalBackground);// Make it modal over the overlay
            }
            finally
            {
                modalBackground.Dispose();
            }
        }

        public void UpdateWorkOrderData(WorkOrderDto workOrderDto)
        {
            if (workOrderDto == null)
            {
                throw new ArgumentNullException(nameof(workOrderDto));
            }
            var labelInstruction = workOrderDto.LabelInstruction;
            var barcodeLabel = workOrderDto.BarcodeLabel;
            var batchInformation = workOrderDto.BatchInformation;
            var printingResult = workOrderDto.PrintingResultInformation;

            // Update Label Instruction Section
            lblInstructionCode.Text = labelInstruction.InstructionCode;
            lblItemCode.Text = labelInstruction.ItemCode;
            lblExpiryDate.Text = labelInstruction.ExpiryDate?.ToString("yyyy-MM-dd") ?? "Not Applicable";
            lblLotNo.Text = labelInstruction.LotNo;
            lblLabelFile.Text = labelInstruction.LabelFile;
            // Update Barcode Label Section
            nudQuantity.Value = barcodeLabel.Quantity;
            lblStartSequence.Text = barcodeLabel.StartSequence.ToString();
            // Update Batch Information Section
            cbEndOfBatch.Checked = batchInformation.IsEndOfBatch;
            lblBatchNumber.Text = batchInformation.BatchNumber.ToString();
            lblSetNumber.Text = batchInformation.SetNumber.ToString();
            // Update Printing Result Information Section
            lblTotalQuantity.Text = printingResult.TargetQuantity.ToString();
            lblRemaining.Text = printingResult.Remaining.ToString();
            lblLabelSample.Text = printingResult.LabelSample.ToString();
            lblTotalPrinted.Text = printingResult.TotalPrinted.ToString();
            lblTotalPassed.Text = printingResult.TotalPassed.ToString();
            lblTotalFailed.Text = printingResult.TotalFailed.ToString();
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

        public void SetPrintingState(PrintingState state)
        {
            _currentState = state;
            switch (state)
            {
                case PrintingState.Initializing:
                    btnPrint.Visible = false;
                    btnPauseResume.Visible = false;
                    ClearLogs();

                    break;
                case PrintingState.Idle:
                case PrintingState.Stopped:
                case PrintingState.Completed:
                    btnPauseResume.Visible = false;
                    btnPrint.Visible = true;
                    btnPrint.Text = "Start Print";
                    btnPrint.Image = _startIcon;
                    btnPrint.ForeColor = Color.Black;
                    btnPrint.BackColor = Color.SeaGreen;
                    break;

                case PrintingState.Printing:
                    btnPauseResume.Visible = true;
                    btnPauseResume.Text = "Pause";
                    btnPauseResume.Image = _pauseIcon;
                    btnPauseResume.BackColor = Color.DarkOrange;

                    btnPrint.Visible = true;
                    btnPrint.Text = "Stop Print";
                    btnPrint.Image = _stopIcon;
                    btnPrint.ForeColor = Color.White;
                    btnPrint.BackColor = Color.Crimson;
                    break;

                case PrintingState.Paused:
                    btnPauseResume.Visible = true;
                    btnPauseResume.Text = "Resume";
                    btnPauseResume.Image = _resumeIcon;
                    btnPauseResume.BackColor = Color.SeaGreen;
                    break;
                case PrintingState.Resumed:
                    btnPauseResume.Visible = true;
                    btnPauseResume.Text = "Pause";
                    btnPauseResume.Image = _pauseIcon;
                    btnPauseResume.BackColor = Color.DarkOrange;
                    break;
                case PrintingState.Disabled:
                    btnPrint.Visible = false;
                    btnPauseResume.Visible = false;
                    break;
            }
        }
        private void ClearLogs()
        {
            _logListView!.BeginUpdate();
            _logListView!.Items.Clear();
            _logListView!.EndUpdate();
        }
        public void AddLog(MessageType type, DateTime timeStamp, string message)
        {
            var item = new ListViewItem(timeStamp.ToString("yyyy-MM-dd"));
            item.SubItems.Add(timeStamp.ToString("HH:mm:ss"));
            item.SubItems.Add(message);

            item.ForeColor = GetColor(type);
            _logListView!.Items.Add(item);

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


        //public void UpdateProgress(int percent, string message)
        //{
        //    _progressBar.Value = percent;
        //    _progressBar.UpdateStatus(message);
        //}

    }
}
