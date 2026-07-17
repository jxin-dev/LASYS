namespace LASYS.Application.Features.BatchPrinting.Events
{
    public class ApprovalCredentialsEventArgs : EventArgs
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public ApprovalCredentialsEventArgs(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
