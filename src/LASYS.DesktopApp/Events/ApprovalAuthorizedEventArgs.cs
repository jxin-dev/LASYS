namespace LASYS.DesktopApp.Events
{
    public class ApprovalAuthorizedEventArgs : EventArgs
    {
        public string UserCode { get; }
        public string SectionId { get; }
        public ApprovalAuthorizedEventArgs(string userCode, string sectionId)
        {
            UserCode = userCode;
            SectionId = sectionId;
        }
    }
}
