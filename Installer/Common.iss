; Spolocny zaklad instalatorov INISSTools: verzia zo zostaveneho programu, [Setup] a jazyky sprievodcu.
; Kompiluje sa v Inno Setup 7 (staci 6.6, kvoli WizardStyle dynamic). Subory su v UTF-8 bez BOM.
;
; Skript programu pred #include definuje:
;   MyAppName            - nazov programu (aj AppId)
;   MyAppExeName         - spustitelny subor programu
;   MyAppDir             - priecinok so zostavenym programom (Release)
;   MyOutputBaseFilename - nazov instalatora bez pripony
; a volitelne:
;   MyPrivilegesRequired - predvoleny rozsah instalacie: lowest (len pouzivatel, predvolene) alebo admin (vsetci)

#define MyAppPublisher "iniss.6f.sk"
#define MyAppURL "https://iniss.6f.sk/"

#ifndef MyPrivilegesRequired
  #define MyPrivilegesRequired "lowest"
#endif

; Verzia sa cita zo zostaveneho .exe, aby bola na jednom mieste v projekte programu.
; Preto musi byt program zostaveny skor, nez sa instalator kompiluje.
#if !FileExists(MyAppDir + "\" + MyAppExeName)
  #expr Error("Chyba zostava " + MyAppDir + "\" + MyAppExeName + " - najprv zostav Release (prikaz je v skripte instalatora)")
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
; lowest: predvolene len pre aktualneho pouzivatela - do podpriecinka Programs v %LocalAppData%;
; druhu moznost (vsetci pouzivatelia / len ja) ponukne dialog alebo parameter /ALLUSERS, /CURRENTUSER
PrivilegesRequired={#MyPrivilegesRequired}
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename={#MyOutputBaseFilename}
Compression=lzma
SolidCompression=yes
; svetly alebo tmavy vzhlad podla nastavenia Windows, rovnako ako programy
WizardStyle=modern dynamic
; programy su 64-bitove (win-x64): instalator sa spusti len na x64 alebo Windows 11 na Arm a pri instalacii
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
