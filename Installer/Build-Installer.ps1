<#
.SYNOPSIS
Zostavi program INISSTools vo verzii vydania a skompiluje jeho instalator.

.DESCRIPTION
Pouziva ho workflow vydania (ToolsCore/.github/workflows/release-app.yml), na skusku sa da spustit aj lokalne
(Windows PowerShell 5.1 aj PowerShell 7). Projekt zostavi v Release s -p:Version=<verzia>, instalator skompiluje
Inno Setup do docasneho priecinka a skopiruje ho do OutDir ako <nazov>-<verzia>.exe. Vysledok (subor, SHA-256,
velkost) zapise do OutDir\installer.json a v GitHub Actions aj do vystupov kroku.

.EXAMPLE
.\ToolsCore\Installer\Build-Installer.ps1 -Project GVDEditor\GVDEditor\GVDEditor.csproj -Installer GVDEditor\GVDEditorInstaller.iss -Version 1.10
#>
param(
    # projekt programu (.csproj)
    [Parameter(Mandatory = $true)] [string] $Project,
    # skript instalatora (.iss)
    [Parameter(Mandatory = $true)] [string] $Installer,
    # verzia vydania X.Y alebo X.Y.Z
    [Parameter(Mandatory = $true)] [string] $Version,
    # dotnet publish namiesto dotnet build (TableSimulator)
    [switch] $Publish,
    # adresa update.json posledneho vydania (kontrola aktualizacii); prazdna = zostava bez kontroly aktualizacii
    [string] $UpdateIndex = '',
    [string] $Iscc = 'C:\Program Files\Inno Setup 7\ISCC.exe',
    [string] $OutDir = 'artifacts'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($Version -notmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)(\.(0|[1-9]\d*))?$') {
    throw "Neplatna verzia '$Version' - ocakava sa X.Y alebo X.Y.Z"
}
if (-not (Test-Path $Iscc)) { throw "Chyba kompilator Inno Setup: $Iscc" }

$verb = if ($Publish) { 'publish' } else { 'build' }
$properties = @("-p:Version=$Version", '-p:ContinuousIntegrationBuild=true')
if ($UpdateIndex) { $properties += "-p:UpdateIndex=$UpdateIndex" }
Write-Host "dotnet $verb $Project $properties"
& dotnet $verb $Project -c Release -nologo @properties
if ($LASTEXITCODE -ne 0) { throw "dotnet $verb skoncil s kodom $LASTEXITCODE" }

# vlastny vystupny priecinok (/O): Output vedla skriptu moze obsahovat starsie instalatory
New-Item -ItemType Directory -Force $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$isccOut = Join-Path $OutDir 'iscc'
if (Test-Path $isccOut) { Remove-Item $isccOut -Recurse -Force }
& $Iscc /Q "/O$isccOut" $Installer
if ($LASTEXITCODE -ne 0) { throw "ISCC skoncil s kodom $LASTEXITCODE" }
$setups = @(Get-ChildItem $isccOut -Filter *.exe)
if ($setups.Count -ne 1) { throw "Po kompilacii sa ocakava jeden instalator, je ich $($setups.Count)" }
$setup = $setups[0]

# Instalator berie verziu zo zostaveneho programu - kontrola, ze zostava naozaj dostala verziu vydania.
$parts = @($Version.Split('.')) + @('0', '0')
$expected = [version]($parts[0..3] -join '.')
$actual = [version]$setup.VersionInfo.FileVersion
if ($actual -ne $expected) { throw "Instalator ma verziu $actual, vydanie $expected - zostavil sa iny program?" }

$name = '{0}-{1}.exe' -f $setup.BaseName, $Version
$target = Join-Path $OutDir $name
Move-Item $setup.FullName $target -Force
Remove-Item $isccOut -Recurse -Force

$file = Get-Item $target
$hash = (Get-FileHash $target -Algorithm SHA256).Hash.ToLowerInvariant()
$result = [ordered]@{ path = $file.FullName; name = $name; sha256 = $hash; size = $file.Length }
$result | ConvertTo-Json | Set-Content (Join-Path $OutDir 'installer.json') -Encoding UTF8
if ($env:GITHUB_OUTPUT) {
    "path=$($file.FullName)", "name=$name", "sha256=$hash", "size=$($file.Length)" |
        Add-Content $env:GITHUB_OUTPUT -Encoding UTF8
}
Write-Host "Instalator: $target ($($file.Length) B, SHA-256 $hash)"
