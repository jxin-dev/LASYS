using LASYS.DesktopApp.Presenters.Interfaces;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace LASYS.DesktopApp.Presenters
{
    public class MainPresenter : IMainPresenter
    {
        private IMainView? _view;
        private readonly IServiceProvider _services;
        public MainPresenter(IServiceProvider services)
        {
            _services = services;
        }

        public void AttachView(IMainView view)
        {
            _view = view;
            _view.WorkOrderRequested += OnWorkOrderRequested;
            _view.WebCameraConfigurationRequested += OnWebCameraConfigurationRequested;
            _view.OCRCalibrationRequested += OnOCRCalibrationRequested;
        }

        private void OnWebCameraConfigurationRequested(object? sender, EventArgs e)
        {
            var cameraControl = _services.GetRequiredService<WebCameraControl>();

            cameraControl.ConfigurationSaved += () =>
            {
                var workOrdersControl = _services.GetRequiredService<WorkOrdersControl>();
                _view?.LoadView(workOrdersControl);
            };

            _view?.LoadView(cameraControl);
        }

        private void OnWorkOrderRequested(object? sender, EventArgs e)
        {
            var workOrdersControl = _services.GetRequiredService<WorkOrdersControl>();
            _view?.LoadView(workOrdersControl);
        }

        private void OnOCRCalibrationRequested(object? sender, EventArgs e)
        {
            var ocrControl = _services.GetRequiredService<OCRCalibrationControl>();

            ocrControl.CameraNotAvailable += () =>
            {
                var cameraControl = _services.GetRequiredService<WebCameraControl>();
                cameraControl.ConfigurationSaved += () =>
                {
                    // Go back to WorkOrdersControl after saving
                    var workOrdersControl = _services.GetRequiredService<WorkOrdersControl>();
                    _view?.LoadView(workOrdersControl);
                };

                _view?.LoadView(cameraControl);
            };

            _view?.LoadView(ocrControl);
        }
    }
}
