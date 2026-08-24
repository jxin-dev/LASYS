namespace LASYS.DesktopApp.Events
{
    public sealed class CameraSavedEventArgs : EventArgs
    {
        public int CameraIndex { get; }
        public string CameraName { get; }
        public string Resolution { get; }
        public int Focus { get; }
        public double Zoom { get; }
        public CameraSavedEventArgs(int cameraIndex, string cameraName, string resolution, int focus, double zoom)
        {
            CameraIndex = cameraIndex;
            CameraName = cameraName;
            Resolution = resolution;
            Focus = focus;
            Zoom = zoom;
        }
    }

}
