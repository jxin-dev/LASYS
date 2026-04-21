using System.Drawing;
using LASYS.Application.Contracts;

namespace LASYS.Application.Interfaces.Services
{
    public interface ICalibrationService
    {
        CalibrationResult? ComputeImageRegion(Rectangle viewerRegion, Size pictureBoxSize, Size imageSize);
        Task AddOrUpdateAsync(Rectangle imageRegion, Size imageSize, string itemCode, int revision, string boxType);
        Task<OCRConfig> LoadAsync();
    }
}
