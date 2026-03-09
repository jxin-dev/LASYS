namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IMainView
    {
        event EventHandler WorkOrderRequested;
        event EventHandler VisionSettingsRequested;
        event EventHandler PrinterManagementRequested;
        event EventHandler BarcodeDeviceSetupRequested;
        void LoadView(UserControl control);
    }
}
