[Setup]
AppName=LASYS Desktop App
AppVersion={#MyAppVersion}
DefaultDirName={pf}\LASYS Desktop App
DefaultGroupName=LASYS Desktop App
OutputBaseFilename=LASYSInstaller_{#StringChange(GetEnv("GITHUB_REF_NAME"), "refs/tags/", "")}
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
