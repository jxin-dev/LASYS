using LASYS.Application.Interfaces;
using LASYS.DesktopApp.DTOs;
using LASYS.DesktopApp.Errors;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class LabelPrintingPresenter
    {
        public UserControl View { get; }
        private readonly ILabelPrintingView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _services;
        private readonly IPrintingService _printingService;

        public LabelPrintingPresenter(ILabelPrintingView view,
                                      IMainView mainView,
                                      IServiceProvider services,
                                      IPrintingService printingService)
        {

            _view = view;
            _mainView = mainView;
            _services = services;
            _printingService = printingService;

            View = (UserControl)view;

            _view.BackToWorkOrdersRequested += OnBackToWorkOrdersRequested;
            _view.PrintRequested += OnPrintRequested;
            _view.PausePrintingRequested += OnPausePrintingRequested;
            _view.ResumePrintingRequested += OnResumePrintingRequested;
            _view.StopPrintingRequested += OnStopPrintingRequested;

            _view.LabelOperationFailed += OnLabelOperationFailed;
        }

        private void OnLabelOperationFailed(object? sender, LabelOperationFailedEventArgs e)
        {
            var errorForm = _services.GetRequiredService<ErrorForm>();
            errorForm.MessageText = LabelErrorMessages.GetMessage(e);
            _view.ShowError(errorForm);
        }


        private void OnBackToWorkOrdersRequested(object? sender, EventArgs e)
        {
            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _mainView?.LoadView(workOrdersPresenter.View);
        }

        private void OnStopPrintingRequested(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnResumePrintingRequested(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnPausePrintingRequested(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnPrintRequested(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetWorkOrderId(int workOrderId)
        {
            //Use repository to fetch work order data if needed using workOrderId and update the view.
            _view.UpdateWorkOrderData(new WorkOrderDto());
        }
    }
}
