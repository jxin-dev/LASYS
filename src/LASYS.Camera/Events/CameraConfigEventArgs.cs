namespace LASYS.Camera.Events
{
    public sealed class CameraConfigEventArgs : EventArgs
    {
        public string Message { get; }
        public CameraConfigEventArgs(string message)
        {
            Message = message;
        }
    }
}
