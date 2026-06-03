using System.Collections.Concurrent;
using System.Drawing;
using LASYS.Application.Interfaces.Services.Camera;

namespace LASYS.Infrastructure.Hardware.Camera
{
    public sealed class FrameHub : IFrameHub
    {
        private readonly ConcurrentDictionary<Guid, Action<Bitmap>> _subscribers = new();
        public void Publish(Bitmap frame)
        {
            foreach (var sub in _subscribers.Values)
            {
                try
                {
                    // IMPORTANT: clone per subscriber-safe usage
                    sub((Bitmap)frame.Clone());
                }
                catch
                {
                    // avoid crashing pipeline
                }
            }
        }

        public Guid Subscribe(Action<Bitmap> handler)
        {
            var id = Guid.NewGuid();
            _subscribers[id] = handler;
            return id;
        }

        public void Unsubscribe(Guid id)
        {
            _subscribers.TryRemove(id, out _);
        }
    }
}
