# Inštalátory a vydania

Inštalátory programov INISSTools (GVDEditor, RawBankEditor, ElenFontEditor, ILTISSimulator, TableSimulator) sú
skripty Inno Setup 7 v repozitári programu. Spoločný základ je tu:

| Súbor                 | Obsah                                                                                            |
|-----------------------|--------------------------------------------------------------------------------------------------|
| `Common.iss`          | `[Setup]`, jazyky sprievodcu, verzia zo zostaveného `.exe`                                       |
| `AppSetup.iss`        | desktopové programy – súbory, odkazy, spustenie po inštalácii aj po aktualizácii (`/RELAUNCH=1`) |
| `DotNet.iss`          | chýbajúci .NET 10 ponúkne stiahnuť                                                               |
| `Build-Installer.ps1` | zostaví program vo verzii vydania a skompiluje inštalátor `<názov>-setup-<verzia>.exe`           |
| `Publish-Release.ps1` | vydanie v repozitári programu s inštalátorom a `update.json` pre kontrolu aktualizácií           |

Workflow vydania je `ToolsCore/.github/workflows/release-app.yml`. Repozitár programu ho volá krátkym
`.github/workflows/release.yml`.

## Vydanie novej verzie

1. Commitni a pushni všetko, čo do vydania patrí – aj v ToolsCore, ExControls a ďalších repozitároch, ktoré program
   odkazuje. Závislosti sa berú z hlavnej vetvy v čase zostavenia, ich commity sú v texte vydania.
2. Označ commit programu značkou `vX.Y` (alebo `vX.Y.Z`). Text anotovanej značky je text vydania:

   ```bash
   git tag -a v1.10 -m "Novinky vo verzii 1.10 ..."
   git push origin v1.10
   ```

   Rovnako sa dá na GitHube založiť nové vydanie s novou značkou – vtedy platí text vydania z GitHubu. Jednoduchá
   značka bez textu dostane ako text zoznam commitov od predchádzajúcej značky.
3. Workflow na runneri Windows zostaví program s verziou zo značky (`-p:Version=1.10`), skompiluje inštalátor,
   overí, že má verziu vydania, a zverejní vydanie `v1.10` v repozitári programu s dvoma súbormi: inštalátorom
   a `update.json`. Vydanie vznikne najprv ako koncept a zverejní sa až s oboma súbormi. Za posledné ho GitHub
   označí, len ak nie je staršie než doterajšie posledné – oprava staršej verzie (`v1.9.1` po `v1.10`) programom
   aktualizáciu neponúkne.

Zlyhaný beh sa dá zopakovať v záložke **Actions** (Run workflow so značkou). Už nahrané súbory nahradí.

Verzia v projekte programu (`<Version>`) platí len pre lokálne zostavy – vydanie ju prepíše verziou zo značky.

## Kontrola aktualizácií v programoch

Program číta `update.json` posledného vydania svojho repozitára
(`https://github.com/iniss-tools/<program>/releases/latest/download/update.json`): verziu, stránku vydania, text
vydania a inštalátor s adresou, SHA-256 a veľkosťou. Adresu dostane len zostava z vydania (`-p:UpdateIndex`,
`ToolsCore/Build/AppRelease.props` → `AssemblyMetadata("UpdateIndex")`). Lokálna zostava aktualizácie nekontroluje
a položka **Skontrolovať aktualizácie** je v nej nedostupná.

* Desktopové programy (`ToolsCore/Updates/AppUpdates`) – pri spustení najviac raz za 20 hodín (voľba
  **Kontrolovať aktualizácie pri spustení programu**) a ručne z ponuky Pomocník. Inštalátor stiahnu, overia veľkosť
  a SHA-256 z `update.json`, program zavrú a spustia `/SILENT /RELAUNCH=1`. Program nainštalovaný inak než
  inštalátorom (podľa `InstallLocation` v kľúči odinštalovania `<AppName>_is1`) len otvorí stránku vydania.
* TableSimulator (`Infrastructure/Hosting/ServerUpdates`) – dvakrát denne, ak to dovolí nastavenie servera; web ukáže
  pruh používateľom s oprávnením na nastavenie alebo správu servera. Inštaluje správca ručne.

## Jednorazové nastavenie

Žiadne tokeny ani secrety netreba – workflow pracuje len s dočasným tokenom behu (`GITHUB_TOKEN`), ktorému
`release.yml` povolí zápis do repozitára programu (`permissions: contents: write`). Podmienky:

* Repozitár programu aj všetky jeho závislosti (ToolsCore, ExControls, pri GVDEditore a ILTISSimulatore aj
  TableSimulator a GVDEditor) sú verejné – klonujú sa bez prihlásenia. Súkromná závislosť zastaví zostavenie
  chybou *Repozitár … sa nepodarilo stiahnuť*.
* Actions sú v repozitári programu povolené (predvolene sú). Spoločný workflow je vo verejnom ToolsCore, každý
  repozitár ho smie volať.

## Lokálna skúška

Inštalátor vo verzii vydania sa dá zostaviť aj lokálne (do `artifacts\`):

```powershell
.\ToolsCore\Installer\Build-Installer.ps1 -Project GVDEditor\GVDEditor\GVDEditor.csproj -Installer GVDEditor\GVDEditorInstaller.iss -Version 1.10
```

Text vydania a `update.json` bez zápisu na GitHub ukáže `Publish-Release.ps1 ... -DryRun`.
