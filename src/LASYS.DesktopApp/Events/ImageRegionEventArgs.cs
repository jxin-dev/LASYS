using DrawingSize = System.Drawing.Size;
namespace LASYS.DesktopApp.Events
{
    public class ImageRegionEventArgs : EventArgs
    {
        public Rectangle Roi { get; }
        public DrawingSize PreviewSize { get; }
        public DrawingSize ImageSize { get; }
        public ImageRegionEventArgs(Rectangle roi, DrawingSize previewSize, DrawingSize imageSize)
        {
            Roi = roi;
            PreviewSize = previewSize;
            ImageSize = imageSize;
        }
    }
}
