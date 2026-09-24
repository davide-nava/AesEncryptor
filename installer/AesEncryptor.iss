#define MyAppVersion GetEnv("VERSION")

[Setup]
AppName=AesEncryptor
AppVersion={#MyAppVersion}
AppPublisher=Davide
DefaultDirName={autopf}\AesEncryptor
DefaultGroupName=AesEncryptor
OutputDir=.
OutputBaseFilename=AesEncryptor-Setup

Compression=lzma
SolidCompression=yes

[Files]
Source: "..\src\AesEncryptor.Wpf\bin\Release\net11.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\AesEncryptor"; Filename: "{app}\AesEncryptor.Wpf.exe"
Name: "{autodesktop}\AesEncryptor"; Filename: "{app}\AesEncryptor.Wpf.exe"

[Run]
Filename: "{app}\AesEncryptor.Wpf.exe"; Description: "Run AesEncryptor"; Flags: nowait postinstall skipifsilent
