namespace LASYS.DesktopApp.Events
{
    public class CameraSelectedEventArgs : EventArgs
    {
        public int CameraIndex { get; }

        public CameraSelectedEventArgs(int cameraIndex)
        {
            CameraIndex = cameraIndex;
        }
    }

}
