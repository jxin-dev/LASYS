using System.Drawing;

namespace LASYS.Application.Events
{
    public sealed class OCRRegionEventArgs : EventArgs
    {
        public Rectangle Region { get; }

        public OCRRegionEventArgs(Rectangle region)
        {
            Region = region;
        }
    }
}
