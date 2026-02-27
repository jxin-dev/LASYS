using System.Drawing;
using LASYS.Application.Events;
using LASYS.Application.Interfaces;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using CvRect = OpenCvSharp.Rect;
using DrawingSize = System.Drawing.Size;

namespace LASYS.Infrastructure.OCR
{
    public class OCRService : IOCRService
    {
        private readonly string _tessDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        private readonly string _ocrLogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "ocr");

        private readonly string _language = "eng";
        private TesseractEngine? _engine;
        private readonly SemaphoreSlim _ocrSemaphore = new(1, 1);

        private CvRect? region;

        public event EventHandler<OCRCompletedEventArgs>? OCRCompleted;
        public event EventHandler<OCRRegionEventArgs>? OCRRegionDetected;
        public event EventHandler<OCRReadingEventArgs>? OCRReading;

        public OCRService()
        {
            InitializeEngine();
            //Create if not exists
            if (!Directory.Exists(_ocrLogsDirectory))
                Directory.CreateDirectory(_ocrLogsDirectory);
        }

        private void InitializeEngine()
        {
            _engine = new TesseractEngine(_tessDataDirectory, _language, EngineMode.Default);
            _engine.SetVariable("tessedit_char_whitelist", "0123456789");
        }

        private Rectangle ComputeViewerRegion(CvRect rect, DrawingSize viewerSize, DrawingSize imageSize)
        {
            float ratio = Math.Min((float)viewerSize.Width / imageSize.Width,
                                   (float)viewerSize.Height / imageSize.Height);

            SizeF displaySize = new SizeF(imageSize.Width * ratio, imageSize.Height * ratio);
            PointF offset = new PointF(
                (viewerSize.Width - displaySize.Width) / 2f,
                (viewerSize.Height - displaySize.Height) / 2f
            );

            RectangleF viewerRect = new RectangleF(
                rect.X * ratio + offset.X,
                rect.Y * ratio + offset.Y,
                rect.Width * ratio,
                rect.Height * ratio
            );

            return Rectangle.Round(viewerRect);
        }
        public async Task<string> ReadTextAsync(Mat mat, DrawingSize viewerSize, int x, int y, int width, int height, int imageWidth, int imageHeight)
        {

            if (mat == null || mat.Empty())
            {
                OCRCompleted?.Invoke(this, new OCRCompletedEventArgs(string.Empty, false));
                return string.Empty;
            }


            // SNAPSHOT FIRST (thread-safe)
            Mat snapshot = mat.Clone();
           
            var scaleX = snapshot.Width / (double)imageWidth;
            var scaleY = snapshot.Height / (double)imageHeight;

            int newX = (int)(x * scaleX);
            int newY = (int)(y * scaleY);
            int newWidth = (int)(width * scaleX);
            int newHeight = (int)(height * scaleY);

            // Clamp to mat bounds to avoid out-of-range errors
            newX = Math.Clamp(newX, 0, snapshot.Width - 1);
            newY = Math.Clamp(newY, 0, snapshot.Height - 1);
            newWidth = Math.Clamp(newWidth, 1, snapshot.Width - newX);
            newHeight = Math.Clamp(newHeight, 1, snapshot.Height - newY);

            region = new CvRect(newX, newY, newWidth, newHeight);

            var viewerRegion = ComputeViewerRegion(region.Value, viewerSize, new DrawingSize(snapshot.Width, snapshot.Height));
            OCRRegionDetected?.Invoke(this, new OCRRegionEventArgs(viewerRegion));

            await _ocrSemaphore.WaitAsync();
            OCRReading?.Invoke(this, new OCRReadingEventArgs(true));

            bool success = false;
            string result = string.Empty;
            try
            {
                result = await Task.Run(() =>
               {
                   using (snapshot)
                   {
                       using Mat roiMat = new Mat(snapshot, region.Value).Clone();

                       // UPSCALE HERE (for small text)
                       using Mat resized = new Mat();
                       Cv2.Resize(
                           roiMat,
                           resized,
                           new OpenCvSharp.Size(),   // auto size
                           2.0,                      // scale X
                           2.0,                      // scale Y
                           InterpolationFlags.Cubic  // best for text
                       );

                       // Preprocessing improves small text detection
                       using Mat gray = new Mat();
                       Cv2.CvtColor(resized, gray, ColorConversionCodes.BGR2GRAY);

                       // Improve contrast
                       using Mat claheResult = new Mat();
                       using (var clahe = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8)))
                       {
                           clahe.Apply(gray, claheResult);
                       }
                       // Threshold
                       using Mat thresholded = new Mat();
                       Cv2.Threshold(claheResult, thresholded, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                       using Bitmap bitmap = BitmapConverter.ToBitmap(thresholded);
                       //using Bitmap bitmap = BitmapConverter.ToBitmap(roiMat); //no improvement

                       // NO MEMORYSTREAM
                       using var pix = PixConverter.ToPix(bitmap);
                       using var page = _engine!.Process(pix);

                       var text = page.GetText()?.Trim() ?? string.Empty;

                       // Save ROI image only
                       if (!string.IsNullOrWhiteSpace(text))
                       {
                           string safeResult = string.Concat(text.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
                           string savePath = Path.Combine(_ocrLogsDirectory, $"{safeResult}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                           Cv2.ImWrite(savePath, roiMat, new ImageEncodingParam(ImwriteFlags.JpegQuality, 90));
                       }

                       return text;
                   }
                   

               });

                success = !string.IsNullOrWhiteSpace(result);

                return result;
            }
            catch (Exception)
            {
                OCRCompleted?.Invoke(this, new OCRCompletedEventArgs(string.Empty, false));
                return string.Empty;
            }
            finally
            {
                OCRReading?.Invoke(this, new OCRReadingEventArgs(false));
                OCRCompleted?.Invoke(this, new OCRCompletedEventArgs(result, success));
                _ocrSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _ocrSemaphore.Dispose();
        }
    }
}
