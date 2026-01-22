; Get the Git tag (e.g., "v1.2.3")
#define GitTag GetEnv("GITHUB_REF_NAME")

; Strip the leading "v" to get "1.2.3"
#define MyAppVersion Copy(GitTag, 2, 9999)

[Setup]
AppName=LASYS Desktop App
AppVersion={#MyAppVersion}
DefaultDirName={pf}\LASYS Desktop App
DefaultGroupName=LASYS Desktop App
OutputBaseFilename=LASYSInstaller_{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\LASYS Desktop App"; Filename: "{app}\LASYS.DesktopApp.exe"
Name: "{commondesktop}\LASYS Desktop App"; Filename: "{app}\LASYS.DesktopApp.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
Filename: "{app}\LASYS.DesktopApp.exe"; Description: "Launch LASYS Desktop App"; Flags: nowait postinstall skipifsilent
