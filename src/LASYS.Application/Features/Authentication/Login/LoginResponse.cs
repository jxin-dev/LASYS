namespace LASYS.Application.Features.Authentication.Login
{
    public record LoginResponse(string UserCode,
                                string UserName,
                                string? SectionId,
                                string? RoleCode,
                                string? PlantCode,
                                string? FirstName,
                                string? LastName,
                                string? MiddleName,
                                string? Nickname,
                                string? Position,
                                string? DepartmentCode,
                                string? SectionName,
                                string? ImagePath);
}
