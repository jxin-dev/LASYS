using System.Text.Json;
using LASYS.Application.Common.Messaging;
using LASYS.Application.Interfaces.Context;
using LASYS.Application.Interfaces.Services;
using LASYS.Infrastructure.Persistence.Connection;

namespace LASYS.Infrastructure.Services.Session
{
    public class SessionTracker : ISessionTracker
    {
        private readonly string _sessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.state");

        private readonly ILogService _logService;
        private readonly ICurrentUser _currentUser;
        private readonly DatabaseSettings _settings;

        public SessionTracker(ILogService logService,
                              ICurrentUser currentUser,
                              DatabaseSettings settings)
        {
            _logService = logService;
            _currentUser = currentUser;
            _settings = settings;
        }

        public void EndSession()
        {
            var state = new SessionState
            {
                Status = "Closed"
            };

            SaveState(state);
        }
        private void SaveState(SessionState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_sessionFile, json);
        }
        public void StartSession()
        {
            if (File.Exists(_sessionFile))
            {
                var json = File.ReadAllText(_sessionFile);
                var previous = JsonSerializer.Deserialize<SessionState>(json);

                if (previous?.Status == "Running")
                {
                    _logService.Log(
                        $"Previous session ended unexpectedly. " +
                        $"User: {previous.Username} ({previous.UserCode}) | " +
                        $"Machine: {previous.MachineName} | " +
                        $"LoginTime: {previous.LoginTime?.ToString("yyyy-MM-dd HH:mm:ss")}",
                        MessageType.Warning);
                }
            }

            var state = new SessionState
            {
                Status = "Running",
                UserCode = _currentUser.UserCode,
                Username = _currentUser.Username,
                MachineName = Environment.MachineName,
                Environment = _settings.Environment,
                LoginTime = _currentUser.LoginTime
            };
            SaveState(state);
        }
    }
}
