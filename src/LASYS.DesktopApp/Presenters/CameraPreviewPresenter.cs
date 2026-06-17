using LASYS.Application.Interfaces.Services.Camera;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public sealed class CameraPreviewPresenter : IDisposable
    {
        private readonly ICameraPreviewView _view;
        private readonly IFrameHub _frameHub;
        private Guid _subscriptionId;
        public UserControl View  => _view.View;
        public CameraPreviewPresenter(ICameraPreviewView view, IFrameHub frameHub)
        {
            _view = view;
            _frameHub = frameHub;

            _subscriptionId = _frameHub.Subscribe(OnFrameReceived);
        }
        private void OnFrameReceived(Bitmap frame)
        {
            _view.InvokeOnUI(() =>
            {
                _view.DisplayFrame(frame);
            });

            frame.Dispose();
        }

        public void Dispose()
        {
            _frameHub.Unsubscribe(_subscriptionId);
        }
    }
}
