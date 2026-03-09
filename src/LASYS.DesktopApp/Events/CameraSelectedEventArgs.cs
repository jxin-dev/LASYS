namespace LASYS.DesktopApp.Events
{
    public class CameraSelectedEventArgs : EventArgs
    {
        public string CameraName { get; }

        public CameraSelectedEventArgs(string cameraName)
        {
            CameraName = cameraName;
        }
    }

}
