using System.Drawing;

namespace LASYS.Application.Interfaces.Services.Camera
{
    public interface IFrameHub
    {
        void Publish(Bitmap frame);
        Guid Subscribe(Action<Bitmap> handler);
        void Unsubscribe(Guid id);
    }
}
