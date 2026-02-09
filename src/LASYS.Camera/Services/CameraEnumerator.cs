using DirectShowLib;
using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;

namespace LASYS.Camera.Services
{
    public sealed class CameraEnumerator : ICameraEnumerator
    {
        public IReadOnlyList<CameraInfo> GetCameras()
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            var cameras = new List<CameraInfo>();

            for (int i = 0; i < devices.Length; i++)
            {
                cameras.Add(new CameraInfo
                {
                    Index = i,
                    Name = devices[i].Name
                });
            }

            return cameras;
        }
    }
}
