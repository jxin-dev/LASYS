using LASYS.Application.Interfaces.Services.NiceLabel;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public sealed class LabelTemplatePreviewPresenter : IDisposable
    {
        private readonly ILabelTemplatePreviewView _view;
        private readonly ILabelPreviewHub _hub;
        public UserControl View => _view.View;
        public LabelTemplatePreviewPresenter(ILabelTemplatePreviewView view, ILabelPreviewHub hub)
        {
            _view = view;
            _hub = hub;

            _hub.PreviewGenerated += OnPreviewGenerated;
        }

        private void OnPreviewGenerated(object? sender, string imagePath)
        {
            if (!File.Exists(imagePath))
                return;

            using var bitmap = new Bitmap(imagePath);

            _view.InvokeOnUI(() =>
            {
                _view.DisplayTemplate(bitmap);
            });
        }

        public void Dispose()
        {
            _hub.PreviewGenerated -= OnPreviewGenerated;
        }
    }
}
