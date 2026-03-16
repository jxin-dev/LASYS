using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Logging
{
    public class FileLogService : ILogService
    {
        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        public void Log(string message, MessageType type)
        {
            var path = GetLogFilePath();
            File.AppendAllText(path, $"{DateTime.Now} [{type}] {message}\n");
        }
        private string GetLogFilePath()
        {
            Directory.CreateDirectory(_logDirectory);

            string fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            return Path.Combine(_logDirectory, fileName);
        }
    }
}
