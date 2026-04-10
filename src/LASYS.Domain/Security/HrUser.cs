namespace LASYS.Domain.Security
{
    public class HrUser
    {
        public string? USER_CODE { get; set; }
        public string? USERNAME { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? MIDDLE_NAME { get; set; }
        public string? NICKNAME { get; set; }
        public string? POSITION { get; set; }
        public string? DEPARTMENT_CODE { get; set; }
        public string? SECTION_NAME { get; set; }
        public string? SECTION_ID { get; set; }
        public string? TEAM { get; set; }
        public string? PICTURE { get; set; } // from DB
    }
}
