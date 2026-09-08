using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class VisualInspectionForm : Form, IVisualInspectionView
    {
        private bool allowClose = false;

        private InspectionStep currentStep = InspectionStep.VisualInspection;

        // Save inspection results
        private bool? visualInspectionResult = null;
        private bool? labelVerificationResult = null;

        private readonly VisualInspectionSampleType _sampleType;

        public event EventHandler<VisualInspectionApprovalEventArgs>? ApprovalRequested;

        public VisualInspectionForm()
        {
            InitializeComponent();

            btnProceed.Click += btnProceed_Click;
            btnBack.Click += btnBack_Click;

            //StartInspection();
        }

        //public VisualInspectionForm(VisualInspectionSampleType sampleType, string sequenceNo)
        //{
        //    InitializeComponent();

        //    _sampleType = sampleType;

        //    lblSequenceNo.Text = sequenceNo;

        //    btnProceed.Click += btnProceed_Click;
        //    btnBack.Click += btnBack_Click;

        //    StartInspection();
        //}

        public void Configure(VisualInspectionSampleType sampleType, string sequenceNo)
        {
            lblSequenceNo.Text = sequenceNo;
            StartInspection();
        }

        // ==========================================
        // START INSPECTION
        // ==========================================

        private void StartInspection()
        {
            currentStep =
                InspectionStep.VisualInspection;

            visualInspectionResult = null;
            labelVerificationResult = null;

            // Hide result panels
            pnlLabelVisualInspection.Visible = false;
            pnlLabelVerification.Visible = false;

            // Show inspection selection
            pnlInspection.Visible = true;

            // Reset radio buttons
            rdbPassed.Checked = false;
            rdbFailed.Checked = false;

            lblInspectionTitle.Text =
                "Please perform Visual Inspection";

            btnProceed.Text = "Next";
            btnProceed.Image = Properties.Resources.arrow_right_alt_24;

            UpdateNavigationButtons();
        }


        // ==========================================
        // SHOW CURRENT STEP
        // ==========================================

        private void ShowCurrentStep()
        {
            pnlInspection.Visible = true;

            btnProceed.Image = Properties.Resources.arrow_right_alt_24;
            btnProceed.Text = "Next";

            // Reset first
            rdbPassed.Checked = false;
            rdbFailed.Checked = false;


            switch (currentStep)
            {
                case InspectionStep.VisualInspection:

                    lblInspectionTitle.Text =
                        "Please perform Visual Inspection";

                    // Restore previous selection
                    if (visualInspectionResult.HasValue)
                    {
                        rdbPassed.Checked =
                            visualInspectionResult.Value;

                        rdbFailed.Checked =
                            !visualInspectionResult.Value;
                    }

                    break;


                case InspectionStep.LabelVerification:

                    lblInspectionTitle.Text = "Please perform Label Verification";

                    // Restore previous selection
                    if (labelVerificationResult.HasValue)
                    {
                        rdbPassed.Checked =
                            labelVerificationResult.Value;

                        rdbFailed.Checked =
                            !labelVerificationResult.Value;
                    }

                    break;
            }


            UpdateNavigationButtons();
        }


        // ==========================================
        // UPDATE NAVIGATION BUTTON
        // ==========================================

        private void UpdateNavigationButtons()
        {
            if (currentStep == InspectionStep.VisualInspection)
            {
                btnBack.Visible = false;
            }
            else
            {
                btnBack.Visible = true;
                btnBack.Text = "Back";
                btnBack.Image = Properties.Resources.arrow_left_alt_24;
            }

        }


        // ==========================================
        // CANCEL / BACK
        // ==========================================

        private void btnBack_Click(object? sender, EventArgs e)
        {
            btnProceed.AutoSize = false;

            switch (currentStep)
            {
                // ----------------------------------
                // FIRST STEP = CANCEL
                // ----------------------------------

                case InspectionStep.VisualInspection:
                    break;

                // ----------------------------------
                // BACK TO VISUAL INSPECTION
                // ----------------------------------

                case InspectionStep.LabelVerification:

                    currentStep = InspectionStep.VisualInspection;

                    // Hide result panel while editing
                    pnlLabelVisualInspection.Visible =
                        false;

                    ShowCurrentStep();

                    break;


                // ----------------------------------
                // BACK FROM FINAL SCREEN
                // ----------------------------------

                case InspectionStep.Completed:

                    // If Label Verification was completed,
                    // go back to Label Verification
                    if (labelVerificationResult.HasValue)
                    {
                        currentStep =
                            InspectionStep.LabelVerification;

                        pnlLabelVerification.Visible =
                            false;
                    }
                    else
                    {
                        // Failed during Visual Inspection
                        currentStep =
                            InspectionStep.VisualInspection;

                        pnlLabelVisualInspection.Visible =
                            false;
                    }

                    ShowCurrentStep();

                    break;
            }
        }


        // ==========================================
        // NEXT / APPROVE / CONFIRM
        // ==========================================

        private void btnProceed_Click(object? sender, EventArgs e)
        {
            // --------------------------------------
            // FINAL ACTION
            // --------------------------------------

            if (currentStep == InspectionStep.Completed)
            {
                HandleFinalAction();

                return;
            }


            // --------------------------------------
            // VALIDATE SELECTION
            // --------------------------------------

            if (!rdbPassed.Checked &&
                !rdbFailed.Checked)
            {
                MessageBox.Show(
                    "Please select Passed or Failed.",
                    "Visual Inspection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }


            switch (currentStep)
            {
                // ==================================
                // STEP 1
                // VISUAL INSPECTION
                // ==================================

                case InspectionStep.VisualInspection:

                    // Save result
                    visualInspectionResult =
                        rdbPassed.Checked;


                    // ----------------------------------
                    // FAILED
                    // ----------------------------------

                    if (visualInspectionResult == false)
                    {
                        lblVisual.Text =
                            "FAILED";

                        lblVisual.ForeColor =
                            Color.Firebrick;


                        // Show failed result
                        pnlLabelVisualInspection.Visible =
                            true;


                        // Do not proceed to next inspection
                        pnlLabelVerification.Visible =
                            false;


                        // Hide radio buttons
                        pnlInspection.Visible =
                            false;


                        // Go to final action
                        currentStep =
                            InspectionStep.Completed;


                        // Button is Confirm
                        btnProceed.Image = Properties.Resources.done_outline_24;
                        btnProceed.Text = "Confirm";


                        UpdateNavigationButtons();

                        return;
                    }


                    // ----------------------------------
                    // PASSED
                    // CONTINUE TO LABEL VERIFICATION
                    // ----------------------------------

                    lblVisual.Text =
                        "PASSED";

                    lblVisual.ForeColor =
                        Color.DarkGreen;


                    // Show result
                    pnlLabelVisualInspection.Visible =
                        true;


                    // Move to next step
                    currentStep =
                        InspectionStep.LabelVerification;


                    ShowCurrentStep();

                    break;


                // ==================================
                // STEP 2
                // LABEL VERIFICATION
                // ==================================

                case InspectionStep.LabelVerification:

                    // Save result
                    labelVerificationResult =
                        rdbPassed.Checked;


                    // Update result display
                    if (labelVerificationResult == true)
                    {
                        lblVerification.Text =
                            "PASSED";

                        lblVerification.ForeColor =
                            Color.DarkGreen;
                    }
                    else
                    {
                        lblVerification.Text =
                            "FAILED";

                        lblVerification.ForeColor =
                            Color.Firebrick;
                    }


                    // Show result
                    pnlLabelVerification.Visible =
                        true;


                    // Hide radio buttons
                    pnlInspection.Visible =
                        false;


                    // Move to final action
                    currentStep =
                        InspectionStep.Completed;


                    // ----------------------------------
                    // BUTTON TEXT
                    // ----------------------------------

                    if (HasFailedInspection())
                    {
                        btnProceed.Image = Properties.Resources.done_outline_24;
                        btnProceed.Text = "Confirm";
                    }
                    else
                    {
                        btnProceed.Image = Properties.Resources.done_outline_24;
                        btnProceed.AutoSize = true;
                        btnProceed.Text = "Approve Printing";
                    }


                    UpdateNavigationButtons();

                    break;
            }
        }


        // ==========================================
        // FINAL ACTION
        // ==========================================

        private void HandleFinalAction()
        {
            var result = MessageBox.Show(this, "Do you want to proceed?", "Visual Inspection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            bool isRejection = HasFailedInspection();

            // Last sample + passed = no approval required
            if (!isRejection && _sampleType == VisualInspectionSampleType.LastSample)
            {
                CompleteApproved();
                return;
            }

            // Ask Presenter to handle approval
            ApprovalRequested?.Invoke(this, new VisualInspectionApprovalEventArgs(isRejection));
        }

        // ==========================================
        // CHECK FAILED INSPECTION
        // ==========================================

        private bool HasFailedInspection()
        {
            return
                visualInspectionResult == false ||
                labelVerificationResult == false;
        }


        public void HideInspection()
        {
            Hide();
        }

        public void ShowInspection()
        {
            Show();
            Activate();
        }

        public void CompleteApproved()
        {
            allowClose = true;
            Close();
        }

        public void CompleteRejected()
        {
            allowClose = true;
            Close();
        }

        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Block Alt+F4, X button, etc.
            }
        }

        // ==========================================
        // ENUMS
        // ==========================================

        private enum InspectionStep
        {
            VisualInspection,
            LabelVerification,
            Completed
        }
    }
}