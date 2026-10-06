using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Services;
using LASYS.Application.Interfaces.Context;
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
        private readonly ICurrentUser _currentUser;
        public VisualInspectionPresenter(IVisualInspectionView view, IBatchPrintProcessService batchPrintService, ILabelPrintingView labelPrintingView, ICurrentUser currentUser)
        {
            _view = view;
            View = (VisualInspectionForm)view;
            _batchPrintService = batchPrintService;
            _labelPrintingView = labelPrintingView;
            _currentUser = currentUser;

            _view.ApprovalRequested += OnApprovalRequested;
        }


        private async void OnApprovalRequested(object? sender, VisualInspectionApprovalEventArgs e)
        {
            // Hide Visual Inspection
            _view.InvokeOnUI(() => _view.HideInspection());

            if (!e.RequiresApproval)
            {
                _batchPrintService.CompleteVisualInspection(e.IsRejection
                    ? VisualInspectionResult.Rejected
                    : VisualInspectionResult.Approved, _currentUser.UserCode!, _currentUser.SectionId!);

                _view.InvokeOnUI(() => _view.CloseInspection());
                _view.InvokeOnUI(() => _labelPrintingView.HideModal());

                return;
            }

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

                _view.InvokeOnUI(() => _view.CloseInspection());
                _view.InvokeOnUI(() => _labelPrintingView.HideModal());
            }
            else
            {
                _batchPrintService.CompleteVisualInspection(VisualInspectionResult.Approved, approval.UserCode!, approval.SectionId!);

                _view.InvokeOnUI(() => _view.CloseInspection());
                _view.InvokeOnUI(() => _labelPrintingView.HideModal());

            }
        }
    }
}

