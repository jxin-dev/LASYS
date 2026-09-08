using LASYS.Application.Common.Enums;

namespace LASYS.Application.Events
{
    public class DeviceStatusEventArgs : EventArgs
    {
        public DeviceType_RemoveThis Device { get; }
        public string Message { get; }
        public string Description { get; }
        public DeviceStatusEventArgs(DeviceType_RemoveThis device, string message, string description)
        {
            Device = device;
            Message = message;
            Description = description;
        }
    }
}
