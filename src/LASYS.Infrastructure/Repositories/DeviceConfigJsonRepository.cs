using LASYS.Application.Interfaces;
using LASYS.Domain.DeviceSettings;
using Newtonsoft.Json;

namespace LASYS.Infrastructure.Repositories
{
    public class DeviceConfigJsonRepository : IDeviceConfigJsonRepository
    {
        private readonly string _filePath = "device_config.json";
        public DeviceConfiguration Load()
        {
            if (!File.Exists(_filePath))
            {
                return new DeviceConfiguration();
            }
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<DeviceConfiguration>(json) ?? new DeviceConfiguration();
        }

        public void Save(DeviceConfiguration config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
