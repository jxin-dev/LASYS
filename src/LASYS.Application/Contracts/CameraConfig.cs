namespace LASYS.Application.Contracts
{
    public sealed class CameraConfig
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public int Focus { get; set; } = 0;
        //public int FrameWidth { get; set; } = 3840;
        //public int FrameHeight { get; set; } = 2160;
        //public int FrameRate { get; set; } = 10;
    }
}
