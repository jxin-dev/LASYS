using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public sealed class VisualInspectionPresenter
    {
        public VisualInspectionForm View { get; }
        private readonly IVisualInspectionView _view;
        private readonly ILabelPrintingView _labelPrintingView;
        private readonly IBatchPrintProcessService _batchPrintService;
        public VisualInspectionPresenter(IVisualInspectionView view, IBatchPrintProcessService batchPrintService, ILabelPrintingView labelPrintingView)
        {
            _view = view;
            View = (VisualInspectionForm)view;
            _batchPrintService = batchPrintService;
            _labelPrintingView = labelPrintingView;

            _view.ApprovalRequested += OnApprovalRequested;
        }


        private async void OnApprovalRequested(object? sender, VisualInspectionApprovalEventArgs e)
        {
            // Hide Visual Inspection
            _view.InvokeOnUI(() => _view.HideInspection());

            // Show Approval Modal
            var approval =
                await _batchPrintService
                    .RequestApprovalAsync(default);


            // Approval cancelled / failed
            if (!approval.IsApproved)
            {
                // Show Visual Inspection again
                _view.InvokeOnUI(() => _view.ShowInspection());

                return;
            }


            // ==================================
            // APPROVED
            // ==================================

            if (e.IsRejection)
            {
                _batchPrintService.CompleteVisualInspection(VisualInspectionResult.Rejected, approval.UserCode!, approval.SectionId!);

                _view.InvokeOnUI(() => _view.CompleteRejected());
                _view.InvokeOnUI(() => _labelPrintingView.HideModal());
            }
            else
            {
                _batchPrintService.CompleteVisualInspection(VisualInspectionResult.Approved, approval.UserCode!, approval.SectionId!);

                _view.InvokeOnUI(() => _view.CompleteApproved());
                _view.InvokeOnUI(() => _labelPrintingView.HideModal());

            }
        }
    }
}

