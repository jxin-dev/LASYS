using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
using LASYS.DesktopApp.State.Printing;
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
        private readonly IPrintingState _printingState;
        public MainPresenter(IMainView view, IServiceProvider serviceProvider, ICurrentUser currentUser, ILogService logService, ISessionTracker sessionTracker, IPrintingState printingState)
        {
            _view = view;
            View = (MainForm)view;
            _serviceProvider = serviceProvider;
            _currentUser = currentUser;
            _logService = logService;
            _sessionTracker = sessionTracker;
            _printingState = printingState;

            _view.WorkOrderRequested += OnWorkOrderRequested;
            _view.VisionSettingsRequested += OnOVisionSettingsRequested;
            _view.PrinterManagementRequested += OnPrinterManagementRequested;
            _view.BarcodeDeviceSetupRequested += OnBarcodeDeviceSetupRequested;

            _view.FormClosingRequested += OnFormClosingRequested;
            _view.LogoutRequested += OnLogoutRequested;

            _view.ShowUserInfo(_currentUser.FullName, _currentUser.SectionName ?? "Unknown", _currentUser.ImagePath);
        }

        private bool CanNavigate()
        {
            // Prevent navigation if printing is active

            var isBlocked = _printingState.Status is PrintingStatus.Printing or PrintingStatus.Paused;

            if (isBlocked)
            {
                _view.ShowNavigationBlocked("Cannot navigate while printing is in progress.");
                _view.RestorePreviousNavigation();
                return false;
            }
            // Navigation is allowed
            return true;
        }
        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            if (!CanNavigate()) return;

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


        private void OnBarcodeDeviceSetupRequested(object? sender, EventArgs e)
        {
            if (!CanNavigate()) return;

            var barcodePresenter = _serviceProvider.GetRequiredService<BarcodeScannerPresenter>();
            _view?.LoadView(barcodePresenter.View);
            _view?.SetActiveNavigation(_view.BarcodeNavItem);

        }

        private void OnPrinterManagementRequested(object? sender, EventArgs e)
        {
            if (!CanNavigate()) return;

            var printerPresenter = _serviceProvider.GetRequiredService<PrinterManagementPresenter>();
            _view?.LoadView(printerPresenter.View);
            _view?.SetActiveNavigation(_view.PrinterManagementNavItem);
        }

        private void OnWorkOrderRequested(object? sender, EventArgs e)
        {
            if (!CanNavigate()) return;

            _logService.Log("WorkOrderRequested received in MainPresenter.", MessageType.Info);
            var workOrdersPresenter = _serviceProvider.GetRequiredService<WorkOrdersPresenter>();
            _logService.Log($"Resolved WorkOrdersPresenter (HashCode={workOrdersPresenter.GetHashCode()}).", MessageType.Info);
            _view?.LoadView(workOrdersPresenter.View, false); //always new
            _view?.SetActiveNavigation(_view.WorkOrdersNavItem);

            _logService.Log("Loaded WorkOrders view into MainForm.", MessageType.Info);
        }

        private void OnOVisionSettingsRequested(object? sender, EventArgs e)
        {
            if (!CanNavigate()) return;

            var visionSettingsPresenter = _serviceProvider.GetRequiredService<VisionSettingsPresenter>();
            _view?.LoadView(visionSettingsPresenter.View);
            _view?.SetActiveNavigation(_view.VisionSettingsNavItem);
        }
    }
}
