using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class MainPresenter 
    {
        private IMainView _view;
        public MainForm View { get; }

        private readonly IServiceProvider _serviceProvider;
        public MainPresenter(IMainView view, IServiceProvider serviceProvider)
        {
            _view = view;
            View = (MainForm)view;

            _serviceProvider = serviceProvider;
          
            _view.WorkOrderRequested += OnWorkOrderRequested;
            _view.VisionSettingsRequested += OnOVisionSettingsRequested;
            _view.PrinterManagementRequested += OnPrinterManagementRequested;
            _view.BarcodeDeviceSetupRequested += OnBarcodeDeviceSetupRequested;
        }


        //private readonly IServiceProvider _services;
        //public MainPresenter(IMainView view, IServiceProvider services)
        //{
        //    _view = view;
        //    View = (MainForm)view;

        //    _services = services;
        //    _view.WorkOrderRequested += OnWorkOrderRequested;
        //    _view.VisionSettingsRequested += OnOVisionSettingsRequested;
        //    _view.PrinterManagementRequested += OnPrinterManagementRequested;
        //    _view.BarcodeDeviceSetupRequested += OnBarcodeDeviceSetupRequested;

        //}


        private void OnBarcodeDeviceSetupRequested(object? sender, EventArgs e)
        {
            var barcodePresenter = _serviceProvider.GetRequiredService<BarcodeScannerPresenter>();
            _view?.LoadView(barcodePresenter.View);

        }

        private void OnPrinterManagementRequested(object? sender, EventArgs e)
        {
            var printerPresenter = _serviceProvider.GetRequiredService<PrinterManagementPresenter>();
            _view?.LoadView(printerPresenter.View);
        }

        private void OnWorkOrderRequested(object? sender, EventArgs e)
        {
            var workOrdersPresenter = _serviceProvider.GetRequiredService<WorkOrdersPresenter>();
            _view?.LoadView(workOrdersPresenter.View, false); //always new
        }

        private void OnOVisionSettingsRequested(object? sender, EventArgs e)
        {
            var visionSettingsPresenter = _serviceProvider.GetRequiredService<VisionSettingsPresenter>();
            _view?.LoadView(visionSettingsPresenter.View);
        }
    }
}
