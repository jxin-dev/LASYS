using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
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
            _view.WebCameraConfigurationRequested += OnWebCameraConfigurationRequested;
            _view.OCRCalibrationRequested += OnOCRCalibrationRequested;
        }


        private void OnWebCameraConfigurationRequested(object? sender, EventArgs e)
        {
            var cameraPresenter = _services.GetRequiredService<WebCameraPresenter>();
            cameraPresenter.ConfigurationSaved += () =>
            {
                var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
                _view?.LoadView(workOrdersPresenter.View);
            };

            _view?.LoadView(cameraPresenter.View);
            cameraPresenter.LoadCameras();

        }

        private void OnWorkOrderRequested(object? sender, EventArgs e)
        {
            var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            _view?.LoadView(workOrdersPresenter.View);
        }

        private void OnOCRCalibrationRequested(object? sender, EventArgs e)
        {
            var ocrPresenter = _services.GetRequiredService<OCRCalibrationPresenter>();
            //ocrControl.CameraNotAvailable += () =>
            //{
            //    var cameraPresenter = _services.GetRequiredService<WebCameraPresenter>();
            //    cameraPresenter.LoadCameras();

            //    cameraPresenter.ConfigurationSaved += () =>
            //    {
            //        // Go back to WorkOrdersControl after saving
            //        var workOrdersPresenter = _services.GetRequiredService<WorkOrdersPresenter>();
            //        _view?.LoadView(workOrdersPresenter.View);
            //    };

            //    _view?.LoadView(cameraPresenter.View);
            //};

            _view?.LoadView(ocrPresenter.View);
        }
    }
}
