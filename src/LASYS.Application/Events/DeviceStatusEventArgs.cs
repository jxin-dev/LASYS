using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class DeviceStatusEventArgs : EventArgs
    {
        public DeviceType Device { get; }
        public string Message { get; }
        public string Description { get; }
        public DeviceStatusEventArgs(DeviceType device, string message, string description)
        {
            Device = device;
            Message = message;
            Description = description;
        }
    }
}
