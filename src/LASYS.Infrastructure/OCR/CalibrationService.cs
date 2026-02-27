using System.Drawing;
using LASYS.Application.Contracts;
using LASYS.Application.Interfaces;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.OCR
{
    public class CalibrationService : ICalibrationService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ocr.config.json");
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
        public async Task<OCRConfig> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new OCRConfig();

                var json = await File.ReadAllTextAsync(_filePath);

                var config = JsonConvert.DeserializeObject<OCRConfig>(json)
                       ?? new OCRConfig();

                //config.Products = config.Products.OrderByDescending(p => p.RegisteredAt).ToList();
                config.Products = [.. config.Products.OrderByDescending(p => p.RegisteredAt)];

                return config;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load config: {ex.Message}");
                return new OCRConfig();
            }
        }
        public async Task AddOrUpdateAsync(Rectangle imageRegion, Size imageSize, string itemCode)
        {
            var coordinates = new Coordinates
            {
                X = imageRegion.X,
                Y = imageRegion.Y,
                Width = imageRegion.Width,
                Height = imageRegion.Height,
                ImageWidth = imageSize.Width,
                ImageHeight = imageSize.Height
            };

            var ocrConfig = await LoadAsync(); // Load existing config
            ocrConfig.Products ??= new List<Product>();  // Ensure list exists

            var existingProduct = ocrConfig.Products.FirstOrDefault(p => p.ItemCode == itemCode);
            if (existingProduct != null)
            {
                //Update
                existingProduct.Coordinates = coordinates;
                existingProduct.RegisteredAt = DateTime.UtcNow;
            }
            else
            {
                //Add
                ocrConfig.Products.Add(new Product
                {
                    ItemCode = itemCode,
                    Coordinates = coordinates,
                    RegisteredAt = DateTime.UtcNow
                });
            }

            try
            {
                var json = JsonConvert.SerializeObject(ocrConfig, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save config: {ex.Message}");
                throw;
            }
        }
    }
}
