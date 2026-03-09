using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class MainPresenter // : IMainPresenter
    {
        private IMainView _view;
        public IMainView View => _view;

        private readonly IServiceProvider _services;
        public MainPresenter(IMainView view, IServiceProvider services)
        {
            _view = view;
            _services = services;
            _view.WorkOrderRequested += OnWorkOrderRequested;
            _view.VisionSettingsRequested += OnOVisionSettingsRequested;
            _view.PrinterManagementRequested += OnPrinterManagementRequested;
            _view.BarcodeDeviceSetupRequested += OnBarcodeDeviceSetupRequested;

        }


        private void OnBarcodeDeviceSetupRequested(object? sender, EventArgs e)
        {
            var barcodePresenter = _services.GetRequiredService<BarcodeScannerPresenter>();
            _view?.LoadView(barcodePresenter.View);
        }

        private void OnPrinterManagementRequested(object? sender, EventArgs e)
        {
            var printerPresenter = _services.GetRequiredService<PrinterManagementPresenter>();
            _view?.LoadView(printerPresenter.View);
        }

        private void OnWorkOrderRequested(object? sender, EventArgs e)
        {
            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _view?.LoadView(workOrdersPresenter.View);
        }

        private void OnOVisionSettingsRequested(object? sender, EventArgs e)
        {
            var visionSettingsPresenter = _services.GetRequiredService<VisionSettingsPresenter>();          
            _view?.LoadView(visionSettingsPresenter.View);
        }
    }
}
