using System.Drawing;

namespace LASYS.OCR.Models
{
    public class CalibrationResult
    {
        public Rectangle ImageRegion { get; set; }  // in image coordinates
        public Rectangle ViewerRegion { get; set; } // original drawn region
    }
}
