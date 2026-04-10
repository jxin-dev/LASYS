namespace LASYS.Application.Interfaces.Context
{
    public interface ICurrentUser
    {
        public DateTime? LoginTime { get; }
        string? UserCode { get; }
        string? Username { get; }
        string? SectionId { get; }
        string? RoleCode { get; }
        string? PlantCode { get; }
        string? FirstName { get; }
        string? LastName { get; }
        string? MiddleName { get; }
        string? NickName { get; }
        string? Position { get; }
        string? DepartmentCode { get; }
        string? SectionName { get; }
        string? ImagePath { get; }
        string FullName { get; }
        string ShortName { get; }
        string LogIdentity { get; }
        void SetUser(
            string userCode,
            string username,
            string? sectionId,
            string? roleCode,
            string? plantCode,
            string? firstName,
            string? lastName,
            string? middleName,
            string? nickName,
            string? position,
            string? departmentCode,
            string? sectionName,
            string? imagePath);
        void Clear();
    }
}
