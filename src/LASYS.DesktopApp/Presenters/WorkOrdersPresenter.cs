using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class WorkOrdersPresenter
    {
        private readonly IWorkOrdersView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _services;

        public UserControl View { get; }
        public WorkOrdersPresenter(IWorkOrdersView view, IMainView mainView, IServiceProvider services)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _services = services;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;
        }

       

        private void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelPrintingPresenter = _services.GetRequiredService<LabelPrintingPresenter>();
            labelPrintingPresenter.SetWorkOrderId(e.WorkOrderId);
            _mainView?.LoadView(labelPrintingPresenter.View);
        }
    }
}
