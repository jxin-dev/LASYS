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

        public DeviceConfiguration Load()
        {
            return _deviceConfigJsonRepository.Load();
        }

        public void UpdateCamera(Camera camera)
        {
            var config = _deviceConfigJsonRepository.Load();
            config.Camera = camera;
            _deviceConfigJsonRepository.Save(config);
        }

        public void UpdateBarcodeScanner(BarcodeScanner barcodeScanner)
        {
            var config = _deviceConfigJsonRepository.Load();
            config.BarcodeScanner = barcodeScanner;
            _deviceConfigJsonRepository.Save(config);
        }
    }
}
