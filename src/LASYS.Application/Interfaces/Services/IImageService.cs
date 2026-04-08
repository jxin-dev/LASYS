namespace LASYS.Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> GetExternalImageUrlAsync(string userCode);
        Task<string> GetUserImageUrlAsync(string userCode);
    }
}
