using LASYS.Application.Events;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;

namespace LASYS.Application.Interfaces.Services
{
    public interface IOCRService : IDisposable
    {
        event EventHandler<OCRCompletedEventArgs> OCRCompleted;
        event EventHandler<OCRRegionEventArgs> OCRRegionDetected;
        event EventHandler<OCRReadingEventArgs> OCRReading;
        event EventHandler<OCRRegionEventArgs> OCRRegionPreview;
        Task<string> ReadTextAsync(Mat mat, DrawingSize viewerSize, int x,int y,int width,int height,int imageWidth,int imageHeight);

        void PreviewRegion(DrawingSize viewerSize, int x, int y, int width, int height, int imageWidth, int imageHeight);

    }
}
