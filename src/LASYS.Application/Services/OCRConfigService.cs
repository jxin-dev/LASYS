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
    }
}
