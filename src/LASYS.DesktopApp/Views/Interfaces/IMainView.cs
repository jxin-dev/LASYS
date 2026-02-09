using LASYS.DesktopApp.Core.Interfaces;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IMainView
    {
        event EventHandler WorkOrderRequested;
        event EventHandler WebCameraConfigurationRequested;
        event EventHandler OCRCalibrationRequested;
        void LoadView(UserControl control);
    }
}
