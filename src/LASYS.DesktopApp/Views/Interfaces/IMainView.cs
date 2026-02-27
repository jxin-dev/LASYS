namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IMainView
    {
        event EventHandler WorkOrderRequested;
        event EventHandler WebCameraConfigurationRequested;
        event EventHandler OCRCalibrationRequested;
        event EventHandler PrinterManagementRequested;
        event EventHandler BarcodeDeviceSetupRequested;

        event EventHandler EndToEndTestRequested;


        void LoadView(UserControl control);
    }
}
