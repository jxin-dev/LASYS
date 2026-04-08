namespace LASYS.Application.Features.Authentication.Login
{
    public record LoginResponse(string USER_CODE, string USER_NAME, string? SECTION_ID, string? ROLE_CODE, string? PLANT_CODE, string? FIRST_NAME ,string? LAST_NAME, string? MIDDLE_NAME, string? IMAGE_PATH);
}

