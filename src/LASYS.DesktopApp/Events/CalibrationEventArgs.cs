using DrawingSize = System.Drawing.Size;
namespace LASYS.DesktopApp.Events
{
    public class CalibrationEventArgs : EventArgs
    {
        public Rectangle Roi { get; }
        public DrawingSize PreviewSize { get; }
        public DrawingSize ImageSize { get; }
        public string ItemCode { get; }
        public int Revision { get; }
        public string BoxType { get; }
        public CalibrationEventArgs(Rectangle roi, DrawingSize previewSize, DrawingSize imageSize, string itemCode, int revision, string boxType)
        {
            Roi = roi;
            PreviewSize = previewSize;
            ImageSize = imageSize;
            ItemCode = itemCode;
            Revision = revision;
            BoxType = boxType;
        }
    }
}
