namespace LASYS.Infrastructure.Services.Session
{
    public class SessionState
    {
        public string Status { get; set; } = "Closed";
        public string? UserCode { get; set; }
        public string? Username { get; set; }
        public string? MachineName { get; set; }
        public string? Environment { get; set; }
        public DateTime? LoginTime { get; set; }
    }
}
