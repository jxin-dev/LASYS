using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
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
        private readonly ICurrentUser _currentUser;
        private readonly ILogService _logService;
        private readonly ISessionTracker _sessionTracker;

        public MainPresenter(IMainView view, IServiceProvider serviceProvider, ICurrentUser currentUser, ILogService logService, ISessionTracker sessionTracker)
        {
            _view = view;
            View = (MainForm)view;

            _serviceProvider = serviceProvider;
            _currentUser = currentUser;
            _logService = logService;
            _sessionTracker = sessionTracker;

            _view.WorkOrderRequested += OnWorkOrderRequested;
            _view.VisionSettingsRequested += OnOVisionSettingsRequested;
            _view.PrinterManagementRequested += OnPrinterManagementRequested;
            _view.BarcodeDeviceSetupRequested += OnBarcodeDeviceSetupRequested;

            _view.FormClosingRequested += OnFormClosingRequested;
            _view.LogoutRequested += OnLogoutRequested;

            _view.ShowUserInfo(_currentUser.FullName, _currentUser.SectionName ?? "Unknown", _currentUser.ImagePath);
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            _logService.Log("User logged out", MessageType.Info);
            _currentUser.Clear();
            _view.CloseView();
        }

        private void OnFormClosingRequested(object? sender, EventArgs e)
        {
            _sessionTracker.EndSession();

            if (_currentUser.LoginTime != null)
            {
                var loginTime = _currentUser.LoginTime.Value;
                var duration = DateTime.Now - loginTime;

                _logService.Log(
                    $"Application closed | LoginTime: {loginTime:HH:mm:ss} | Duration: {duration:hh\\:mm\\:ss}",
                    MessageType.Info);
            }
            else
            {
                _logService.Log("Application closed", MessageType.Info);
            }
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
