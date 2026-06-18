using System.Drawing;
using LASYS.Application.Interfaces.Services.NiceLabel;

namespace LASYS.Infrastructure.Hardware.Printers.Sato
{
    public sealed class LabelPreviewHub : ILabelPreviewHub
    {
        public event EventHandler<string>? PreviewGenerated;
        public void Publish(string imagePath)
        {
            PreviewGenerated?.Invoke(this, imagePath);
        }
    }
}
