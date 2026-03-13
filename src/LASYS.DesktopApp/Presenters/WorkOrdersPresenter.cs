using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class WorkOrdersPresenter
    {
        private readonly IWorkOrdersView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _serviceProvider;
        public UserControl View { get; }

        public WorkOrdersPresenter(IWorkOrdersView view, IMainView mainView, IServiceProvider serviceProvider)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _serviceProvider = serviceProvider;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;
        }


        private async void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
            //labelPrintingPresenter.SetWorkOrderId(e.WorkOrderId);
            _mainView?.LoadView(labelPrintingPresenter.View, false); //always new
            await labelPrintingPresenter.InitializeTemplateAsync(e.WorkOrderId);


        }
    }
}
