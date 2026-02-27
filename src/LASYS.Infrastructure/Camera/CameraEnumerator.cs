using DirectShowLib;
using LASYS.Application.Contracts;
using LASYS.Application.Interfaces;
namespace LASYS.Infrastructure.Camera
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
