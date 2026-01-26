using LASYS.Domain.OCR;

namespace LASYS.Application.Interfaces
{
    public interface IOCRConfigJsonRepository
    {
        Task<OCRConfiguration> LoadAsync();
        Task SaveAsync(OCRConfiguration config);
    }
}
