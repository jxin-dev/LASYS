using LASYS.Application.Events;
using OpenCvSharp;
using DrawingSize = System.Drawing.Size;

namespace LASYS.Application.Interfaces
{
    public interface IOCRService : IDisposable
    {
        event EventHandler<OCRCompletedEventArgs> OCRCompleted;
        event EventHandler<OCRRegionEventArgs> OCRRegionDetected;
        event EventHandler<OCRReadingEventArgs> OCRReading;
        Task<string> ReadTextAsync(Mat mat, DrawingSize viewerSize, int x,int y,int width,int height,int imageWidth,int imageHeight);

    }
}
