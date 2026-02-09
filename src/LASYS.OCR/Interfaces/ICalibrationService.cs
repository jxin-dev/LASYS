using System.Drawing;
using LASYS.OCR.Models;

namespace LASYS.OCR.Interfaces
{
    public interface ICalibrationService
    {
        CalibrationResult? ComputeImageRegion(Rectangle viewerRegion, Size pictureBoxSize, Size imageSize);
        Task AddOrUpdateAsync(Rectangle imageRegion, Size imageSize, string itemCode);
        Task<OCRConfig> LoadAsync();
    }
}
