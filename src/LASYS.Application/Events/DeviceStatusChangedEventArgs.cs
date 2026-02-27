namespace LASYS.Application.Events
{
    public class DeviceStatusChangedEventArgs : EventArgs
    {
        public string Status { get; }
        public DeviceStatusChangedEventArgs(string status)
        {
            Status = status;
        }
    }
}
