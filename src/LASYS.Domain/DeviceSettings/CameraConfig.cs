namespace LASYS.Domain.DeviceSettings
{
    public class CameraConfig
    {
        public bool Enabled { get; set; } = true;
        public int CameraId { get; set; } = 0;
        public int FrameWidth { get; set; } = 3840;
        public int FrameHeight { get; set; } = 2160;
        public int FrameRate { get; set; } = 30;
    }
}
