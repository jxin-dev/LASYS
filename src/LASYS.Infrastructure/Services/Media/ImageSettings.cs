namespace LASYS.Infrastructure.Services.Media
{
    public class ImageSettings
    {
        public string ExternalTemplate { get; set; } = default!;
        public string BasePath { get; set; } = default!;
        public string DefaultImage { get; set; } = "default.jpg";
    }
}
