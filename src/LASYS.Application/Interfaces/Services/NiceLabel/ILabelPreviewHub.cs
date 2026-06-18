namespace LASYS.Application.Interfaces.Services.NiceLabel
{
    public interface ILabelPreviewHub
    {
        event EventHandler<string>? PreviewGenerated;
        void Publish(string imagePath);
    }
}
