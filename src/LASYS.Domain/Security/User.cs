namespace LASYS.Domain.Security;

public class User
{
    public required string USER_CODE { get; set; }        // char(20) NOT NULL
    public required string USER_NAME { get; set; }          // varchar(50) NOT NULL
    public required string USER_PASSWORD { get; set; }      // char(50) NOT NULL
    public string? SECTION_ID { get; set; }         // char(128) NOT NULL
    public string? ROLE_CODE { get; set; }          // varchar(128) NULL
    public string? PLANT_CODE { get; set; }         // varchar(128) NULL
    public string? COMMENT { get; set; }            // mediumtext NULL
    public string? FIRST_NAME { get; set; }         // varchar(150) NULL
    public string? LAST_NAME { get; set; }          // varchar(150) NULL
    public string? MIDDLE_NAME { get; set; }        // varchar(150) NULL

    // Flags (char(0) is unusual, but mapping to string keeps compatibility)
    public string? ACCESS_FLAG { get; set; }        // char(0) NULL
    public string? ACTIVE_FLAG { get; set; }        // char(0) NULL

    public string? CREATED_USER_CODE { get; set; }  // varchar(50) NOT NULL
    public string? CREATED_SECTION_ID { get; set; } // char(128) NOT NULL
    public uint CREATED_IP_ADDRESS { get; set; }   // int(10) unsigned zerofill NOT NULL
    public ulong CREATED_DATETIME { get; set; }    // bigint unsigned NOT NULL

    public string? LASTUPDATE_USER_CODE { get; set; }   // varchar(50) NOT NULL
    public string? LASTUPDATE_SECTION_ID { get; set; }  // char(128) NOT NULL
    public uint LASTUPDATE_IP_ADDRESS { get; set; }    // int(10) unsigned zerofill NOT NULL
    public ulong LASTUPDATE_DATETIME { get; set; }     // bigint unsigned NOT NULL

    public ulong LASTPASSRENEW_DATETIME { get; set; }  // bigint unsigned NOT NULL
}
