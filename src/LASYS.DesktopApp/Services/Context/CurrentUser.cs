using LASYS.Application.Interfaces.Context;

namespace LASYS.DesktopApp.Services.Context
{
    public class CurrentUser : ICurrentUser
    {
        public DateTime? LoginTime { get; private set; }
        public string? UserCode { get; private set; }
        public string? Username { get; private set; }
        public string? SectionId { get; private set; }
        public string? RoleCode { get; private set; }
        public string? PlantCode { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? MiddleName { get; private set; }
        public string FullName => $"{FirstName} {LastName}";
        public string ShortName => string.IsNullOrEmpty(LastName)
            ? FirstName ?? ""
            : $"{FirstName} {LastName[0]}.";

        public string LogIdentity => 
            string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(UserCode)
            ? "SYSTEM"
            : $"{Username ?? "Unknown"} ({UserCode ?? "N/A"})";


        public void SetUser(string? userCode,
                           string? username,
                           string? sectionId,
                           string? roleCode,
                           string? plantCode,
                           string? firstName,
                           string? lastName,
                           string? middleName)
        {
            LoginTime = DateTime.Now;
            UserCode = userCode;
            Username = username;
            SectionId = sectionId;
            RoleCode = roleCode;
            PlantCode = plantCode;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }

        public void Clear()
        {
            UserCode = null;
            Username = null;
            SectionId = null;
            RoleCode = null;
            PlantCode = null;
            FirstName = null;
            LastName = null;
            MiddleName = null;
        }

    }
}
