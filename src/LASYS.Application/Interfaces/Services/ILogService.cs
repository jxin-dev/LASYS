using LASYS.Application.Common.Messaging;

namespace LASYS.Application.Interfaces.Services
{
    public interface ILogService
    {
        void Log(string message, MessageType type);
    }
}
