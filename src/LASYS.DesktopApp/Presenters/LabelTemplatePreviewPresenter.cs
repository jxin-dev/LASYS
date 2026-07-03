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

        private void OnPreviewGenerated(object? sender, string? e)
        {
            if (string.IsNullOrEmpty(e) || !File.Exists(e))
            {
                _view.InvokeOnUI(() => _view.ResetTemplate());
                return;
            }

            using var bitmap = new Bitmap(e);

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
