using System;
using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Enums;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class LabelPrintingControl : UserControl, ILabelPrintingView
    {
        private readonly CustomProgressBar _progressBar;

        public event EventHandler? BackToWorkOrdersRequested;
        public event EventHandler? PrintRequested;
        public event EventHandler? PausePrintingRequested;
        public event EventHandler? ResumePrintingRequested;
        public event EventHandler? StopPrintingRequested;

        //Error Events

        public event EventHandler? RetryRequested;
        public event EventHandler? SkipRequested;
        public event EventHandler? StopBatchRequested;
        public event EventHandler<LabelOperationFailedEventArgs>? LabelOperationFailed;

        private readonly Random _random = new Random();
        public LabelPrintingControl()
        {
            InitializeComponent();

            _progressBar = new CustomProgressBar
            {
                Dock = DockStyle.Bottom,
                ProgressColor = Color.FromArgb(255, 220, 20, 60),
                ProgressBackgroundColor = Color.LightGray,
                ShowPercentage = false
            };

            pnlLoadingContainer.Controls.Add(_progressBar);
            UpdateProgress(50, "Printing Progress");


            btnBack.Click += (_, _) => BackToWorkOrdersRequested?.Invoke(this, EventArgs.Empty);
            //btnPrint.Click += (_, _) => LabelOperationFailed?.Invoke(this, new LabelOperationFailedEventArgs(LabelOperationType.Printing,sequenceNo: 1));

            //Testing random error generation for demonstration purposes
            btnPrint.Click += (_, _) =>
            {
                var values = Enum.GetValues(typeof(LabelOperationType));
                var randomOperation = (LabelOperationType)values.GetValue(_random.Next(values.Length))!;

                LabelOperationFailed?.Invoke(
                    this,
                    new LabelOperationFailedEventArgs(
                        randomOperation,
                        sequenceNo: 1,
                        barcodeResult: "ABC123",
                        ocrResult: "00002"
                    )
                );
            };

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
            errorForm.ShowDialog(modalBackground); // Make it modal over the overlay

            modalBackground.Dispose();


            //// Show the error form
            //using (var errorForm = new ErrorForm())
            //{
            //    errorForm.StartPosition = FormStartPosition.CenterParent;
            //    errorForm.Text = "Error";
            //    //errorForm.Owner = modalBackground; // set owner to the main form
            //    errorForm.TopMost = true;
            //    errorForm.ShowInTaskbar = false;
            //    errorForm.FormBorderStyle = FormBorderStyle.None;

            //    errorForm.ShowDialog(modalBackground);

            //}
            //modalBackground.Dispose();
            //pnlContent.Enabled = true;
            //var modalBackground = new Form
            //{
            //    StartPosition = FormStartPosition.Manual,
            //    FormBorderStyle = FormBorderStyle.None,
            //    Opacity = 50d,
            //    BackColor = Color.Black,
            //    Size = pnlContent.Size,
            //    Location = pnlContent.Location,
            //    ShowInTaskbar = false
            //};
            //Controls.Add(modalBackground);
            //modalBackground.Show();
            //var errorForm = new ErrorForm();
            //errorForm.Owner = modalBackground;

            //errorForm.ShowDialog();
            //modalBackground.Dispose();
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

        public void UpdateProgress(int percent, string message)
        {
            _progressBar.Value = percent;
            _progressBar.UpdateStatus(message);
        }

    }
}
