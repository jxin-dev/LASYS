using System.Threading.Tasks;
using LASYS.Application.Interfaces;
using LASYS.Domain.DeviceSettings;

namespace LASYS.Application.Services
{
    public class DeviceConfigService
    {
        private readonly IDeviceConfigJsonRepository _deviceConfigJsonRepository;

        public DeviceConfigService(IDeviceConfigJsonRepository deviceConfigJsonRepository)
        {
            _deviceConfigJsonRepository = deviceConfigJsonRepository;
        }

        public async Task<DeviceConfiguration> LoadAsync()
        {
            return await _deviceConfigJsonRepository.LoadAsync();
        }

        public async Task UpdateCameraAsync(CameraConfig camera)
        {
            var config = await _deviceConfigJsonRepository.LoadAsync();
            config.Camera = camera;
            await _deviceConfigJsonRepository.SaveAsync(config);
        }

        public async Task UpdateBarcodeScannerAsync(BarcodeScannerConfig barcodeScanner)
        {
            var config = await _deviceConfigJsonRepository.LoadAsync();
            config.BarcodeScanner = barcodeScanner;
            await _deviceConfigJsonRepository.SaveAsync(config);
        }
    }
}
