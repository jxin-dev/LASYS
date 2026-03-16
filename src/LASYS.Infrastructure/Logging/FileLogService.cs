using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Logging
{
    public class FileLogService : ILogService
    {
        private readonly ICurrentUser _currentUser;
        private static readonly object _lock = new();
        public FileLogService(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }
        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        public void Log(string message, MessageType type)
        {
            try
            {
                var path = GetLogFilePath();
                var user = _currentUser.LogIdentity;

                var logMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [{type}] {user} - {message}";
               
                lock (_lock)
                {
                    File.AppendAllText(path, $"{logMessage}{Environment.NewLine}");
                }
            }
            catch
            {
                // Avoid crashing the application because of logging failure
            }
        }
        private string GetLogFilePath()
        {
            Directory.CreateDirectory(_logDirectory);

            string fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            return Path.Combine(_logDirectory, fileName);
        }
    }
}
