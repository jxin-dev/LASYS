using System.Drawing;
using LASYS.Application.Interfaces;
using LASYS.Domain.OCR;

namespace LASYS.Application.Services
{
    public class OCRConfigService
    {
        private readonly IOCRConfigJsonRepository _ocrConfigJsonRepository;

        public OCRConfigService(IOCRConfigJsonRepository ocrConfigJsonRepository)
        {
            _ocrConfigJsonRepository = ocrConfigJsonRepository;
        }

        // Load full configuration
        public async Task<OCRConfiguration> LoadAsync()
        {
            return await _ocrConfigJsonRepository.LoadAsync();
        }

        // Save full configuration
        public async Task SaveAsync(OCRConfiguration config)
        {
            await _ocrConfigJsonRepository.SaveAsync(config);
        }

        // Add or update a single product
        public async Task AddOrUpdateProductAsync(Product product)
        {
            var config = await _ocrConfigJsonRepository.LoadAsync();

            // Check if product exists
            var existing = config.Products.FirstOrDefault(p => p.ItemCode == product.ItemCode);
            if (existing != null)
            {
                existing.Coordinates = product.Coordinates;
                existing.RegisteredAt = product.RegisteredAt;
            }
            else
            {
                config.Products.Add(product);
            }

            await _ocrConfigJsonRepository.SaveAsync(config);
        }

        // Remove a product by item code
        public async Task RemoveProductAsync(string itemCode)
        {
            var config = await _ocrConfigJsonRepository.LoadAsync();
            var product = config.Products.FirstOrDefault(p => p.ItemCode == itemCode);
            if (product != null)
            {
                config.Products.Remove(product);
                await _ocrConfigJsonRepository.SaveAsync(config);
            }
        }

        public CalibrationResult? ComputeImageRegion(Rectangle viewerRegion, Size pictureBoxSize, Size imageSize)
        {
            if (viewerRegion.Width <= 0 || viewerRegion.Height <= 0)
                return null;

            float ratio = Math.Min((float)pictureBoxSize.Width / imageSize.Width,
                                   (float)pictureBoxSize.Height / imageSize.Height);

            SizeF displaySize = new SizeF(imageSize.Width * ratio, imageSize.Height * ratio);
            PointF offset = new PointF(
                (pictureBoxSize.Width - displaySize.Width) / 2f,
                (pictureBoxSize.Height - displaySize.Height) / 2f
            );

            RectangleF imgRect = new RectangleF(
                (viewerRegion.X - offset.X) / ratio,
                (viewerRegion.Y - offset.Y) / ratio,
                viewerRegion.Width / ratio,
                viewerRegion.Height / ratio
            );

            imgRect = RectangleF.Intersect(imgRect, new RectangleF(PointF.Empty, imageSize));

            return new CalibrationResult
            {
                ImageRegion = Rectangle.Round(imgRect),
                ViewerRegion = viewerRegion
            };
        }
    }

    public class CalibrationResult
    {
        public Rectangle ImageRegion { get; set; }  // in image coordinates
        public Rectangle ViewerRegion { get; set; } // original drawn region
    }

    public class NormalizedRect
    {
        public float X; // 0 to 1
        public float Y;
        public float Width;
        public float Height;

        public Rectangle ToAbsolute(Size size)
        {
            return new Rectangle(
                (int)(X * size.Width),
                (int)(Y * size.Height),
                (int)(Width * size.Width),
                (int)(Height * size.Height)
            );
        }

        public static NormalizedRect FromAbsolute(Rectangle rect, Size size)
        {
            return new NormalizedRect
            {
                X = (float)rect.X / size.Width,
                Y = (float)rect.Y / size.Height,
                Width = (float)rect.Width / size.Width,
                Height = (float)rect.Height / size.Height
            };
        }
    }
}
