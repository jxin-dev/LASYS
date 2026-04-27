using LASYS.UIControls.Models;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IMainView
    {
        event EventHandler WorkOrderRequested;
        event EventHandler VisionSettingsRequested;
        event EventHandler PrinterManagementRequested;
        event EventHandler BarcodeDeviceSetupRequested;
        void LoadView(UserControl control, bool cache = true);
        void CloseView();
        event EventHandler FormClosingRequested;
        event EventHandler LogoutRequested;

        void ShowUserInfo(string fullName, string sectionName, string? imagePath);

        void ShowNavigationBlocked(string message);

        void SetActiveNavigation(NavItem? item);
        void RestorePreviousNavigation();


        NavItem WorkOrdersNavItem { get; }
        NavItem VisionSettingsNavItem { get; }
        NavItem PrinterManagementNavItem { get; }
        NavItem BarcodeNavItem { get; }

    }
}
