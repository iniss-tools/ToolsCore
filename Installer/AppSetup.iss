; Spolocny obsah instalatorov programov INISSTools (GVDEditor, RawBankEditor, ElenFontEditor, ILTISSimulator).
; Kompiluje sa v Inno Setup 7 (staci 6.6, kvoli WizardStyle dynamic). Subor je v UTF-8 bez BOM.
;
; Skript programu pred #include definuje:
;   MyAppName            - nazov programu; je to aj AppId a nazov priecinka v %LocalAppData% a kluca HKCU\Software,
;                          kam si program uklada nastavenia (ToolsCore AppPaths a AppRegistry podla nazvu zostavy)
;   MyAppExeName         - spustitelny subor programu
;   MyAppDir             - priecinok so zostavenym Release
;   MyOutputBaseFilename - nazov instalatora bez pripony
; a volitelne:
;   MyAppExcludes        - dalsie vynechane subory v tvare parametra Excludes, kazdy s ciarkou na zaciatku
;   NeedAspNetCore       - program potrebuje aj ASP.NET Core Runtime (x64)
;   NeedRuntimeX86       - program spusta aj 32-bitovy proces, potrebuje .NET Runtime (x86)

#define MyAppPublisher "iniss.6f.sk"
#define MyAppURL "https://iniss.6f.sk/"
; hlavna verzia .NET, pre ktoru su programy zostavene (net10.0-windows) - menit spolu s TargetFramework
#define DotNetVersion "10"

#ifndef MyAppExcludes
  #define MyAppExcludes ""
#endif

; Verzia sa cita zo zostaveneho .exe, aby bola na jednom mieste - v Properties\AssemblyInfo.cs.
; Preto musi byt Release build hotovy skor, nez sa instalator kompiluje.
#if !FileExists(MyAppDir + "\" + MyAppExeName)
  #expr Error("Chyba zostava Release " + MyAppDir + "\" + MyAppExeName + " - najprv dotnet build -c Release")
#endif
#define MyAppVersion GetVersionNumbersString(MyAppDir + "\" + MyAppExeName)

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
; verzia aj vo vlastnostiach suboru instalatora (inak 0.0.0.0)
VersionInfoVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Predvolene sa instaluje iba pre aktualneho pouzivatela - do podpriecinka Programs v %LocalAppData%.
; Pre vsetkych pouzivatelov sa da zvolit v dialogu alebo parametrom /ALLUSERS.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename={#MyOutputBaseFilename}
Compression=lzma
SolidCompression=yes
; svetly alebo tmavy vzhlad podla nastavenia Windows, rovnako ako programy
WizardStyle=modern dynamic
; program je 64-bitovy (win-x64): instalator sa spusti len na x64 alebo Windows 11 na Arm a pri instalacii
; pre vsetkych pouzivatelov pouzije 64-bitovy Program Files (nie Program Files (x86))
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CreateUninstallRegKey=yes
; ikona programu v zozname aplikacii Windows
UninstallDisplayIcon={app}\{#MyAppExeName}
UpdateUninstallLogAppName=no
UsePreviousLanguage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"

[CustomMessages]
english.DotNetGroup=.NET components:
english.DotNetTask=Download and install missing .NET {#DotNetVersion} components (requires administrator rights)
english.DotNetMemo=.NET components to download and install:
english.DotNetDownloadFailed=The .NET components could not be downloaded:%n%1%n%nContinue the installation without them? The program will then offer to download them when it starts.
english.DotNetInstalling=Installing %1...
english.DotNetInstallFailed=Component %1 was not installed:%n%2%n%nThe program will not start without it.
czech.DotNetGroup=Součásti .NET:
czech.DotNetTask=Stáhnout a nainstalovat chybějící součásti .NET {#DotNetVersion} (vyžaduje oprávnění správce)
czech.DotNetMemo=Součásti .NET, které se stáhnou a nainstalují:
czech.DotNetDownloadFailed=Součásti .NET se nepodařilo stáhnout:%n%1%n%nPokračovat v instalaci bez nich? Program je pak nabídne stáhnout při spuštění.
czech.DotNetInstalling=Instaluje se %1...
czech.DotNetInstallFailed=Součást %1 se nenainstalovala:%n%2%n%nBez ní se program nespustí.
slovak.DotNetGroup=Súčasti .NET:
slovak.DotNetTask=Stiahnuť a nainštalovať chýbajúce súčasti .NET {#DotNetVersion} (vyžaduje práva správcu)
slovak.DotNetMemo=Súčasti .NET, ktoré sa stiahnu a nainštalujú:
slovak.DotNetDownloadFailed=Súčasti .NET sa nepodarilo stiahnuť:%n%1%n%nPokračovať v inštalácii bez nich? Program ich potom ponúkne stiahnuť pri spustení.
slovak.DotNetInstalling=Inštaluje sa %1...
slovak.DotNetInstallFailed=Súčasť %1 sa nenainštalovala:%n%2%n%nBez nej sa program nespustí.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
; ponuka sa len vtedy, ked niektory potrebny framework .NET chyba
Name: "dotnet"; Description: "{cm:DotNetTask}"; GroupDescription: "{cm:DotNetGroup}"; Check: DotNetMissing

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

[Code]
// Programy su zavisle od frameworku (SelfContained=false). Instalator zisti, ktore zdielane frameworky .NET
// v pocitaci chybaju; ak uloha "dotnet" ostane zaskrtnuta, stiahne ich instalatory od Microsoftu (adresa aka.ms
// vedie vzdy na najnovsiu verziu) a po skopirovani suborov ich spusti.

var
  DotNetTitles: array of String;
  DotNetFiles: array of String;
  DotNetDownloadPage: TDownloadWizardPage;
  DotNetDownloaded: Boolean;
  DotNetRestart: Boolean;

// Priecinok .NET pre danu architekturu tam, kde ho hlada aj spustac programu: zapisuje ho instalator .NET
// do 32-bitoveho pohladu registra, inak je to predvoleny priecinok v Program Files.
function DotNetRoot(const Arch: String): String;
begin
  if not RegQueryStringValue(HKLM32, 'SOFTWARE\dotnet\Setup\InstalledVersions\' + Arch, 'InstallLocation', Result)
     or (Result = '') then
  begin
    if Arch = 'x86' then
      Result := ExpandConstant('{commonpf32}\dotnet')
    else
      Result := ExpandConstant('{commonpf64}\dotnet');
  end;
end;

// Program zostaveny pre .NET {#DotNetVersion}.0 pobezi s hocijakou verziou {#DotNetVersion}.x (roll forward v ramci hlavnej verzie).
// Rozhoduju priecinky v shared\ - zoznam sharedfx v registri nie je uplny (nove verzie v nom chybaju).
function FrameworkInstalled(const Arch, Framework: String): Boolean;
var
  FindRec: TFindRec;
begin
  Result := FindFirst(AddBackslash(DotNetRoot(Arch)) + 'shared\' + Framework + '\{#DotNetVersion}.*', FindRec);
  if Result then
    FindClose(FindRec);
end;

procedure AddDotNetPart(const Title, Arch, Framework, Product: String);
var
  I: Integer;
begin
  if FrameworkInstalled(Arch, Framework) then
    Exit;
  Log('Chyba ' + Framework + ' {#DotNetVersion} (' + Arch + ') v ' + DotNetRoot(Arch));
  I := GetArrayLength(DotNetTitles);
  SetArrayLength(DotNetTitles, I + 1);
  SetArrayLength(DotNetFiles, I + 1);
  DotNetTitles[I] := Title;
  DotNetFiles[I] := Product + '-win-' + Arch + '.exe';
end;

function InitializeSetup: Boolean;
begin
  // Desktop Runtime obsahuje aj zakladny .NET Runtime
  AddDotNetPart('.NET Desktop Runtime {#DotNetVersion} (x64)', 'x64', 'Microsoft.WindowsDesktop.App', 'windowsdesktop-runtime');
#ifdef NeedAspNetCore
  AddDotNetPart('ASP.NET Core Runtime {#DotNetVersion} (x64)', 'x64', 'Microsoft.AspNetCore.App', 'aspnetcore-runtime');
#endif
#ifdef NeedRuntimeX86
  AddDotNetPart('.NET Runtime {#DotNetVersion} (x86)', 'x86', 'Microsoft.NETCore.App', 'dotnet-runtime');
#endif
  Result := True;
end;

function DotNetMissing: Boolean;
begin
  Result := GetArrayLength(DotNetFiles) > 0;
end;

procedure InitializeWizard;
begin
  DotNetDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  DotNetDownloadPage.ShowBaseNameInsteadOfUrl := True;
end;

procedure AppendMemo(var Memo: String; const Info, NewLine: String);
begin
  if Info = '' then
    Exit;
  if Memo <> '' then
    Memo := Memo + NewLine + NewLine;
  Memo := Memo + Info;
end;

// Stranka Pripravene na instalaciu vypise aj to, ktore frameworky .NET sa stiahnu.
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo,
  MemoGroupInfo, MemoTasksInfo: String): String;
var
  Info: String;
  I: Integer;
begin
  Result := '';
  AppendMemo(Result, MemoUserInfoInfo, NewLine);
  AppendMemo(Result, MemoDirInfo, NewLine);
  AppendMemo(Result, MemoTypeInfo, NewLine);
  AppendMemo(Result, MemoComponentsInfo, NewLine);
  AppendMemo(Result, MemoGroupInfo, NewLine);
  AppendMemo(Result, MemoTasksInfo, NewLine);
  if WizardIsTaskSelected('dotnet') then
  begin
    Info := CustomMessage('DotNetMemo');
    for I := 0 to GetArrayLength(DotNetTitles) - 1 do
      Info := Info + NewLine + Space + DotNetTitles[I];
    AppendMemo(Result, Info, NewLine);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  I: Integer;
begin
  Result := True;
  if (CurPageID <> wpReady) or not WizardIsTaskSelected('dotnet') then
    Exit;

  DotNetDownloaded := False;
  DotNetDownloadPage.Clear;
  for I := 0 to GetArrayLength(DotNetFiles) - 1 do
    DotNetDownloadPage.Add('https://aka.ms/dotnet/{#DotNetVersion}.0/' + DotNetFiles[I], DotNetFiles[I], '');
  DotNetDownloadPage.Show;
  try
    try
      DotNetDownloadPage.Download;
      DotNetDownloaded := True;
    except
      if DotNetDownloadPage.AbortedByUser then
        Result := False
      else
        // bez .NET sa da instalovat dalej - spustac programu ho pri spusteni ponukne stiahnut sam
        Result := SuppressibleMsgBox(FmtMessage(CustomMessage('DotNetDownloadFailed'), [GetExceptionMessage]),
          mbError, MB_YESNO, IDYES) = IDYES;
    end;
  finally
    DotNetDownloadPage.Hide;
  end;
end;

procedure InstallDotNet;
var
  I, ResultCode: Integer;
  Error: String;
begin
  for I := 0 to GetArrayLength(DotNetFiles) - 1 do
  begin
    WizardForm.StatusLabel.Caption := FmtMessage(CustomMessage('DotNetInstalling'), [DotNetTitles[I]]);
    // .NET sa instaluje pre cely pocitac - pri instalacii len pre pouzivatela si runas vyziada prava spravcu (UAC)
    if not ShellExec('runas', ExpandConstant('{tmp}\') + DotNetFiles[I], '/install /passive /norestart', '',
      SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      Error := SysErrorMessage(ResultCode)
    // 3010 = hotovo, ale treba restart; 1638 = uz je nainstalovana novsia verzia
    else if (ResultCode <> 0) and (ResultCode <> 3010) and (ResultCode <> 1638) then
      Error := SysErrorMessage(ResultCode)
    else
      Error := '';

    Log(DotNetFiles[I] + ': ' + IntToStr(ResultCode));
    if ResultCode = 3010 then
      DotNetRestart := True;
    if Error <> '' then
      SuppressibleMsgBox(FmtMessage(CustomMessage('DotNetInstallFailed'), [DotNetTitles[I], Error]), mbError, MB_OK, IDOK);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and DotNetDownloaded then
    InstallDotNet;
end;

function NeedRestart: Boolean;
begin
  Result := DotNetRestart;
end;
