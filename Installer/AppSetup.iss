; Spolocny obsah instalatorov desktopovych programov INISSTools (GVDEditor, RawBankEditor, ElenFontEditor,
; ILTISSimulator). Kompiluje sa v Inno Setup 7 (staci 6.6). Subor je v UTF-8 bez BOM.
;
; Skript programu pred #include definuje to, co pozaduje Common.iss (MyAppName, MyAppExeName, MyAppDir,
; MyOutputBaseFilename) - MyAppName je zaroven nazov priecinka v %LocalAppData% a kluca HKCU\Software, kam si program
; uklada nastavenia (ToolsCore AppPaths a AppRegistry podla nazvu zostavy) - a volitelne:
;   MyAppExcludes  - dalsie vynechane subory v tvare parametra Excludes, kazdy s ciarkou na zaciatku
;   NeedAspNetCore - program potrebuje aj ASP.NET Core Runtime (x64)
;   NeedRuntimeX86 - program spusta aj 32-bitovy proces, potrebuje .NET Runtime (x86)

#ifndef MyAppExcludes
  #define MyAppExcludes ""
#endif

#include "Common.iss"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Nastavenia a logy (config, logs) si program zaklada az za behu - ak vznikli pri skusobnom spusteni zostavy,
; do instalatora nepatria. Bez createallsubdirs: prazdne priecinky po starsich zostavach (runtimes\linux-x64,
; satelity jazykov...) by sa inak vytvorili aj u pouzivatela.
Source: "{#MyAppDir}\*"; DestDir: "{app}"; Excludes: "\config,\logs{#MyAppExcludes}"; Flags: ignoreversion recursesubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
; Nedavno otvorene projekty si program zapisuje do HKCU\Software\<nazov> - pri odinstalovani zmazat cely kluc.
Root: HKCU; Subkey: "Software\{#MyAppName}"; Flags: uninsdeletekey

[UninstallDelete]
; Nastavenia a logy si program zaklada az za behu, instalator o nich nevie a sam by ich nezmazal.
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; spustenie programu ponukne posledna stranka sprievodcu (zaskrtavacie pole); pri tichej instalacii sa nespusti
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
; Aktualizacia z programu (ToolsCore AppUpdates) spusti tichu instalaciu s /RELAUNCH=1 - po nej sa program spusti znova,
; ako povodny pouzivatel aj vtedy, ked instalacia potrebovala prava spravcu.
Filename: "{app}\{#MyAppExeName}"; Flags: nowait runasoriginaluser; Check: RelaunchAfterUpdate

; programy WinForms - Desktop Runtime (s nim aj zakladny .NET Runtime)
#define NeedDesktopRuntime
#include "DotNet.iss"

[Code]
// /RELAUNCH=1 - instalaciu spustila aktualizacia z programu
function RelaunchAfterUpdate: Boolean;
begin
  Result := ExpandConstant('{param:RELAUNCH|0}') = '1';
end;
