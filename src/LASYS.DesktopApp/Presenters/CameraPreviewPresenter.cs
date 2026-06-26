using LASYS.Application.Events;
using LASYS.Application.Interfaces.Services;
using LASYS.Application.Interfaces.Services.Camera;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.Infrastructure.OCR;

namespace LASYS.DesktopApp.Presenters
{
    public sealed class CameraPreviewPresenter : IDisposable
    {
        private readonly ICameraPreviewView _view;
        private readonly IFrameHub _frameHub;
        private readonly IOCRService _ocrService;

        private Guid _subscriptionId;
        public UserControl View => _view.View;

        private Rectangle? _ocrRegion;
        private bool _isReading;
        private Size _currentFrameSize;
        public CameraPreviewPresenter(ICameraPreviewView view, IFrameHub frameHub, IOCRService ocrService)
        {
            _view = view;
            _frameHub = frameHub;
            _ocrService = ocrService;

            _subscriptionId = _frameHub.Subscribe(OnFrameReceived);
            _ocrService.OCRRegionDetected += OnOcrRegionDetected;
            _ocrService.OCRReading += OnOcrReading;
        }

        private async void OnOcrReading(object? sender, OCRReadingEventArgs e)
        {
            _isReading = e.IsReading;
            if (_isReading)
            {
                _view.InvokeOnUI(() =>
                {
                    if (_ocrRegion.HasValue)
                        _view.ShowOcrRegion(_ocrRegion.Value);
                });
            }
            else
            {
                await Task.Delay(100); // Keep rectangle visible for 100ms

                _view.InvokeOnUI(() =>
                {
                    _view.HideOcrRegion();
                });
            }
            //_view.InvokeOnUI(() =>
            //{
            //    if (_isReading && _ocrRegion.HasValue)
            //        _view.ShowOcrRegion(_ocrRegion.Value);
            //    else
            //        _view.HideOcrRegion();
            //});
        }

        private static Rectangle ConvertToViewer(
            Rectangle imageRegion,
            Size viewerSize,
            Size imageSize)
        {
            float ratio = Math.Min(
                (float)viewerSize.Width / imageSize.Width,
                (float)viewerSize.Height / imageSize.Height);

            SizeF displaySize = new(
                imageSize.Width * ratio,
                imageSize.Height * ratio);

            PointF offset = new(
                (viewerSize.Width - displaySize.Width) / 2f,
                (viewerSize.Height - displaySize.Height) / 2f);

            return Rectangle.Round(new RectangleF(
                imageRegion.X * ratio + offset.X,
                imageRegion.Y * ratio + offset.Y,
                imageRegion.Width * ratio,
                imageRegion.Height * ratio));
        }

        private void OnOcrRegionDetected(object? sender, OCRRegionEventArgs e)
        {
            _ocrRegion = ConvertToViewer(e.Region, _view.PictureBoxSize, _currentFrameSize);

            _view.InvokeOnUI(() =>
            {
                if (_isReading && _ocrRegion.HasValue)
                {
                    _view.ShowOcrRegion(_ocrRegion.Value);
                }
            });
        }

        private void OnFrameReceived(Bitmap frame)
        {
            _currentFrameSize = frame.Size;
            _view.InvokeOnUI(() =>
            {
                _view.DisplayFrame(frame);
            });

            frame.Dispose();
        }

        public void Dispose()
        {
            _frameHub.Unsubscribe(_subscriptionId);

            _ocrService.OCRRegionDetected -= OnOcrRegionDetected;
            _ocrService.OCRReading -= OnOcrReading;
        }
    }
}
